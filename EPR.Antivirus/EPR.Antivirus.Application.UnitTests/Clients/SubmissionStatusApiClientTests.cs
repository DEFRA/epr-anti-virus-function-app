namespace EPR.Antivirus.Application.UnitTests.Clients;

using System.Net;
using System.Text.Json;
using Application.Clients;
using Data.DTOs.SubmissionStatusApi;
using Data.Enums;
using Data.Options;
using Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

[TestClass]
public class SubmissionStatusApiClientTests
{
    private const string PomContainerName = "pom-blob-container-name";
    private const string RegistrationContainerName = "registration-blob-container-name";
    private const string SubsidiaryContainerName = "subsidiary-blob-container-name";
    private const string CompaniesHouseContainerName = "companieshouse-blob-container-name";
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };
    private readonly Mock<IOptions<BlobStorageOptions>> _blobStorageOptionsMock = new();

    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private HttpClient _httpClient;

    private SubmissionStatusApiClient _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://baseaddress.com/"),
        };
        _blobStorageOptionsMock.Setup(x => x.Value).Returns(new BlobStorageOptions
        {
            PomContainerName = PomContainerName,
            RegistrationContainerName = RegistrationContainerName,
            SubsidiaryContainerName = SubsidiaryContainerName,
            SubsidiaryCompaniesHouseContainerName = CompaniesHouseContainerName
        });

        _systemUnderTest = new SubmissionStatusApiClient(
            _httpClient,
            _blobStorageOptionsMock.Object,
            Mock.Of<ILogger<SubmissionStatusApiClient>>());
    }

    [TestMethod]
    [DataRow(FileType.Pom, ScanResult.Success)]
    [DataRow(FileType.Brands, ScanResult.Success)]
    [DataRow(FileType.CompanyDetails, ScanResult.FailedToVirusScan)]
    [DataRow(FileType.Partnerships, ScanResult.Success)]
    [DataRow(FileType.Subsidiaries, ScanResult.Success)]
    [DataRow(FileType.CompaniesHouse, ScanResult.Success)]
    public async Task PostEventAsync_WhenValidRequest_NoErrorThrown(FileType fileType, ScanResult scanResult)
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var submissionId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var blobName = Guid.NewGuid().ToString();
        var errors = new List<string> { "99" };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Created,
            });

        // Act & Assert
        await _systemUnderTest
            .Invoking(x => x.PostEventAsync(
                new SubmissionClientPostEventRequest(orgId, userId, submissionId, fileType, blobName, fileId, scanResult, errors, false)))
            .Should()
            .NotThrowAsync();

        _httpMessageHandlerMock
            .Protected()
            .Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(
                req => IsValidReport(req.Content, fileType, scanResult)),
            ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    public async Task PostEventAsync_WhenHttpClientThrowsError_ThrowsSubmissionStatusApiClientException()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var submissionId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var blobName = Guid.NewGuid().ToString();
        const FileType fileType = FileType.Pom;
        const ScanResult scanResult = ScanResult.Success;
        var errors = new List<string> { "99" };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException());

        // Act & Assert
        await _systemUnderTest
            .Invoking(x => x.PostEventAsync(
                new SubmissionClientPostEventRequest(orgId, userId, submissionId, fileType, blobName, fileId, scanResult, errors, false)))
            .Should()
            .ThrowAsync<SubmissionStatusApiClientException>()
            .WithMessage("A success status code was not received when sending the error report");
    }

    [TestMethod]
    public async Task GetSubmissionFileAsync_WhenValidRequest_ReturnsSubmissionFileResult()
    {
        // Arrange
        var fileId = Guid.NewGuid();

        var submissionFileResult = new SubmissionFileResult(
            Guid.NewGuid(),
            SubmissionType.Producer,
            Guid.NewGuid(),
            "SomeFileName",
            FileType.Pom,
            Guid.NewGuid(),
            Guid.NewGuid(),
            It.IsAny<string>(),
            It.IsAny<Guid>());
        var submissionFileResultJson = JsonSerializer.Serialize(submissionFileResult);

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(submissionFileResultJson),
            });

        // Act
        var act = await _systemUnderTest.GetSubmissionFileAsync(fileId);

        // Assert
        act.Should().Be(submissionFileResult);
    }

    [TestMethod]
    public async Task GetSubmissionFileAsync_WhenInvalidRequest_ThrowsSubmissionStatusApiClientException()
    {
        // Arrange
        var fileId = Guid.NewGuid();

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException());

        // Act & Assert
        await _systemUnderTest
            .Invoking(x => x.GetSubmissionFileAsync(fileId))
            .Should()
            .ThrowAsync<SubmissionStatusApiClientException>()
            .WithMessage("A success status code was not received when requesting file metadata");
    }

    private static bool IsValidReport(HttpContent? content, FileType fileType, ScanResult scanResult)
    {
        if (content is null)
        {
            return false;
        }

        var stringContent = content.ReadAsStringAsync().Result;
        var report = JsonSerializer.Deserialize<SubmissionEventRequest>(
            stringContent, Options);

        if (report is null)
        {
            return false;
        }

        var expectedBlobContainer = RegistrationContainerName;

        if (fileType == FileType.Pom)
        {
            expectedBlobContainer = PomContainerName;
        }
        else if (fileType == FileType.Subsidiaries)
        {
            expectedBlobContainer = SubsidiaryContainerName;
        }
        else if (fileType == FileType.CompaniesHouse)
        {
            expectedBlobContainer = CompaniesHouseContainerName;
        }

        return report.AntivirusScanResult == scanResult && report.BlobContainerName == expectedBlobContainer;
    }
}
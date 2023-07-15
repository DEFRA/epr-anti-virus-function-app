namespace EPR.Antivirus.Application.Tests.Clients;

using System.Net;
using System.Text.Json;
using Application.Clients;
using Application.Clients.Interfaces;
using Data.DTOs.SubmissionStatusApi;
using Data.Enums;
using Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

[TestClass]
public class SubmissionStatusApiClientTests
{
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private HttpClient _httpClient;

    private ISubmissionStatusApiClient _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://baseaddress.com/"),
        };

        _systemUnderTest = new SubmissionStatusApiClient(_httpClient, Mock.Of<ILogger<SubmissionStatusApiClient>>());
    }

    [TestMethod]
    public async Task PostEventAsync_WhenValidRequest_NoErrorThrown()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var submissionId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var blobName = Guid.NewGuid().ToString();
        const ScanResult scanResult = ScanResult.Success;
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

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x.PostEventAsync(orgId, userId, submissionId, blobName, fileId, scanResult, errors))
            .Should()
            .NotThrowAsync();
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
        const ScanResult scanResult = ScanResult.Success;
        var errors = new List<string> { "99" };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException());

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x.PostEventAsync(orgId, userId, submissionId, blobName, fileId, scanResult, errors))
            .Should()
            .ThrowAsync<SubmissionStatusApiClientException>()
            .WithMessage("A success status code was not received when sending the error report");
    }

    [TestMethod]
    public async Task GetSubmissionFileAsync_WhenValidRequest_ReturnsSubmissionFileResult()
    {
        // Arrange
        var fileId = Guid.NewGuid();

        var submissionFileResult = new SubmissionFileResult(Guid.NewGuid(), SubmissionType.Producer, Guid.NewGuid(), FileType.Pom, Guid.NewGuid(), Guid.NewGuid());
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

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x.GetSubmissionFileAsync(fileId))
            .Should()
            .ThrowAsync<SubmissionStatusApiClientException>()
            .WithMessage("A success status code was not received when requesting file metadata");
    }
}
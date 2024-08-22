namespace EPR.Antivirus.Application.UnitTests.Services;

using Application.Services;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Data.Enums;
using Data.Options;
using Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public class BlobStorageServiceTests
{
    private Mock<BlobClient> _blobClientMock;
    private Mock<BlobContainerClient> _blobContainerClientMock;
    private Mock<BlobServiceClient> _blobServiceClientMock;
    private Mock<IOptions<BlobStorageOptions>> _optionsMock;

    private BlobStorageService _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _blobClientMock = new Mock<BlobClient>();
        var blobContentInfo = BlobsModelFactory.BlobContentInfo(
            new ETag("a"), DateTimeOffset.Now, null, string.Empty, 0);
        _blobClientMock.Setup(x => x.UploadAsync(It.IsAny<string>()))
            .ReturnsAsync(Response.FromValue(blobContentInfo, new Mock<Response>().Object));
        var blobInfo = BlobsModelFactory.BlobInfo(new ETag("a"), DateTimeOffset.Now);
        _blobClientMock
            .Setup(x => x.SetMetadataAsync(It.IsAny<IDictionary<string, string>>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Response.FromValue(blobInfo, new Mock<Response>().Object));
        _blobContainerClientMock = new Mock<BlobContainerClient>();
        _blobContainerClientMock.Setup(x => x.GetBlobClient(It.IsAny<string>()))
            .Returns(_blobClientMock.Object);
        _blobServiceClientMock = new Mock<BlobServiceClient>();
        _blobServiceClientMock.Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(_blobContainerClientMock.Object);

        _optionsMock = new Mock<IOptions<BlobStorageOptions>>();
        _optionsMock.Setup(x => x.Value).Returns(
            new BlobStorageOptions
            {
                ConnectionString = "ConnectionString",
                PomContainerName = "PomContainerName",
                RegistrationContainerName = "RegistrationContainerName",
                SubsidiaryContainerName = "SubsidiaryContainerName",
                SubsidiaryCompaniesHouseContainerName = "SubsidiaryCompaniesHouseContainerName"
            });

        _systemUnderTest = new BlobStorageService(
            _blobServiceClientMock.Object, _optionsMock.Object, Mock.Of<ILogger<BlobStorageService>>());
    }

    [TestMethod]
    public void CreateMetadata_WhenCalled_ReturnsDataInMetadataFormat()
    {
        // Arrange
        var submissionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        const FileType fileType = FileType.Brands;
        const string csvFileMimeType = "text/csv";
        var submissionPeriod = It.IsAny<string>();
        string fileName = "testfile.csv";
        var organisationId = Guid.NewGuid();

        // Act
        var result = BlobStorageService.CreateMetadata(submissionId, userId, fileType, submissionPeriod, fileName, organisationId);

        // Assert
        result.Should().Contain("fileTypeEPR", fileType.ToString());
        result.Should().Contain("submissionId", submissionId.ToString());
        result.Should().Contain("userId", userId.ToString());
        result.Should().Contain("fileType", csvFileMimeType);
    }

    [TestMethod]
    [DataRow(SubmissionType.Producer)]
    [DataRow(SubmissionType.Registration)]
    [DataRow(SubmissionType.Subsidiary)]
    public async Task UploadFileStreamWithMetadataAsync_WhenBlobIsCreated_NameIsReturned(SubmissionType submissionType)
    {
        // Arrange
        var stream = new MemoryStream();
        var metadata = new Dictionary<string, string>();

        var blobName = Guid.NewGuid().ToString();
        _blobClientMock.Setup(x => x.Name).Returns(blobName);

        // Act
        var result = await _systemUnderTest.UploadFileStreamWithMetadataAsync(stream, submissionType, metadata);

        // Assert
        result.Should().Be(blobName);
    }

    [TestMethod]
    [DataRow(SubmissionType.Producer)]
    [DataRow(SubmissionType.Registration)]
    [DataRow(SubmissionType.Subsidiary)]
    public async Task UploadFileStreamWithMetadataAsync_WhenBlobCreationFails_ThrowsBlobStorageServiceException(SubmissionType submissionType)
    {
        // Arrange
        var stream = new MemoryStream();
        var metadata = new Dictionary<string, string>();

        _blobClientMock.Setup(x => x.Name).Throws<Exception>();

        // Act & Assert
        await _systemUnderTest
            .Invoking(x => x.UploadFileStreamWithMetadataAsync(stream, submissionType, metadata))
            .Should()
            .ThrowAsync<BlobStorageServiceException>()
            .WithMessage("Error occurred during uploading to blob storage");
    }
}
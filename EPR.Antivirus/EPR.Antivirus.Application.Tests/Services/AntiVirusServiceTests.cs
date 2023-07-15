namespace EPR.Antivirus.Application.Tests.Services;

using Application.Clients.Interfaces;
using Application.Services;
using Application.Services.Interfaces;
using Data.Constants;
using Data.DTOs.SubmissionStatusApi;
using Data.DTOs.TradeAntivirusQueue;
using Data.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public class AntivirusServiceTests
{
    private Mock<ISubmissionStatusApiClient> _submissionStatusApiClientMock;
    private Mock<IServiceBusQueueClient> _serviceBusQueueClientMock;
    private Mock<IBlobStorageService> _blobStorageServiceMock;
    private Mock<ITradeAntivirusApiClient> _tradeAntivirusFileResultMock;
    private Mock<ILogger<AntivirusService>> _loggerMock;

    private IAntivirusService _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _submissionStatusApiClientMock = new Mock<ISubmissionStatusApiClient>();
        _serviceBusQueueClientMock = new Mock<IServiceBusQueueClient>();
        _blobStorageServiceMock = new Mock<IBlobStorageService>();
        _tradeAntivirusFileResultMock = new Mock<ITradeAntivirusApiClient>();
        _loggerMock = new Mock<ILogger<AntivirusService>>();

        _systemUnderTest = new AntivirusService(
            _submissionStatusApiClientMock.Object,
            _serviceBusQueueClientMock.Object,
            _blobStorageServiceMock.Object,
            _tradeAntivirusFileResultMock.Object,
            _loggerMock.Object);
    }

    [TestMethod]
    public async Task HandleAsync_NoWarningLogged_WhenVirusCheckSuccess()
    {
        // Arrange
        var blobId = Guid.NewGuid().ToString();
        var message = new TradeAntivirusQueueResult
        {
            Key = Guid.NewGuid(),
            Collection = "pom",
            Status = ScanResult.Success
        };
        var submissionFileResult = new SubmissionFileResult(
            Guid.NewGuid(),
            SubmissionType.Producer,
            Guid.NewGuid(),
            FileType.Pom,
            Guid.NewGuid(),
            Guid.NewGuid());

        _blobStorageServiceMock
            .Setup(x => x.UploadFileStreamWithMetadataAsync(It.IsAny<Stream>(), It.IsAny<SubmissionType>(), It.IsAny<IDictionary<string, string>>()))
            .ReturnsAsync(blobId);
        _submissionStatusApiClientMock
            .Setup(x => x.GetSubmissionFileAsync(It.IsAny<Guid>()))
            .ReturnsAsync(submissionFileResult);

        // Act
        await _systemUnderTest.HandleAsync(message);

        // Assert
        _loggerMock.VerifyLog(x => x.LogInformation("Antivirus check was successful"), Times.Once);
        _submissionStatusApiClientMock.Verify(
            x =>
            x.PostEventAsync(
                submissionFileResult.OrganisationId,
                submissionFileResult.UserId,
                submissionFileResult.SubmissionId,
                blobId,
                submissionFileResult.FileId,
                message.Status,
                new List<string>()),
            Times.Once);
    }

    [TestMethod]
    [DataRow(ScanResult.AwaitingProcessing)]
    [DataRow(ScanResult.Quarantined)]
    [DataRow(ScanResult.FileInaccessible)]
    [DataRow(ScanResult.FailedToVirusScan)]
    public async Task HandleAsync_LogsWarning_WhenVirusCheckNotSuccess(ScanResult scanResult)
    {
        // Arrange
        var message = new TradeAntivirusQueueResult
        {
            Key = Guid.NewGuid(),
            Collection = "pom",
            Status = scanResult
        };

        var submissionFileResult = new SubmissionFileResult(
            Guid.NewGuid(),
            SubmissionType.Producer,
            Guid.NewGuid(),
            FileType.Pom,
            Guid.NewGuid(),
            Guid.NewGuid());

        _submissionStatusApiClientMock
            .Setup(x => x.GetSubmissionFileAsync(It.IsAny<Guid>()))
            .ReturnsAsync(submissionFileResult);

        // Act
        await _systemUnderTest.HandleAsync(message);

        // Assert
        _loggerMock.VerifyLog(x => x.LogWarning("Antivirus check did not indicate success. Received {}", scanResult.ToString()));
        _submissionStatusApiClientMock.Verify(
            x =>
                x.PostEventAsync(
                    submissionFileResult.OrganisationId,
                    submissionFileResult.UserId,
                    submissionFileResult.SubmissionId,
                    string.Empty,
                    submissionFileResult.FileId,
                    message.Status,
                    new List<string> { ErrorCodes.UploadedFileContainsThreat }),
            Times.Once);
    }
}
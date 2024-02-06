namespace EPR.Antivirus.Application.UnitTests.Services;

using Application.Clients.Interfaces;
using Application.Services;
using Application.Services.Interfaces;
using Common.Logging.Constants;
using Common.Logging.Models;
using Common.Logging.Services;
using Data.Constants;
using Data.DTOs.SubmissionStatusApi;
using Data.DTOs.TradeAntivirusQueue;
using Data.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
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
    private Mock<ILoggingService> _loggingServiceMock;
    private Mock<IFeatureManager> _featureManagerMock;

    private IAntivirusService _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _submissionStatusApiClientMock = new Mock<ISubmissionStatusApiClient>();
        _serviceBusQueueClientMock = new Mock<IServiceBusQueueClient>();
        _blobStorageServiceMock = new Mock<IBlobStorageService>();
        _tradeAntivirusFileResultMock = new Mock<ITradeAntivirusApiClient>();
        _loggerMock = new Mock<ILogger<AntivirusService>>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _featureManagerMock = new Mock<IFeatureManager>();

        _systemUnderTest = new AntivirusService(
            _submissionStatusApiClientMock.Object,
            _serviceBusQueueClientMock.Object,
            _blobStorageServiceMock.Object,
            _loggingServiceMock.Object,
            _tradeAntivirusFileResultMock.Object,
            _loggerMock.Object,
            _featureManagerMock.Object);
    }

    [TestMethod]
    public async Task HandleAsync_CallsLoggingServiceWithRightParameters_WhenVirusCheckSuccess()
    {
        // Arrange

        var blobName = Guid.NewGuid().ToString();

        var message = new TradeAntivirusQueueResult
        {
            Key = Guid.NewGuid(),
            Collection = "pom",
            Status = ScanResult.Success
        };

        var submissionFile = new SubmissionFileResult(
            Guid.NewGuid(),
            SubmissionType.Producer,
            Guid.NewGuid(),
            "SomeFileName",
            FileType.Pom,
            Guid.NewGuid(),
            Guid.NewGuid(),
            It.IsAny<string>(),
            It.IsAny<Guid>());

        _blobStorageServiceMock
            .Setup(x => x.UploadFileStreamWithMetadataAsync(
                It.IsAny<Stream>(), It.IsAny<SubmissionType>(), It.IsAny<IDictionary<string, string>>()))
            .ReturnsAsync(blobName);

        _submissionStatusApiClientMock.Setup(x => x.GetSubmissionFileAsync(It.IsAny<Guid>()))
            .ReturnsAsync(submissionFile);

        var monitoringEvent = new ProtectiveMonitoringEvent(
            submissionFile.SubmissionId,
            "epr-anti-virus-function-app",
            PmcCodes.Code0203,
            Priorities.NormalEvent,
            TransactionCodes.AntivirusCleanUpload,
            $"Antivirus check was successful for file '{submissionFile.FileName}'",
            $"FileId: '{submissionFile.FileId}'");

        // Act
        await _systemUnderTest.HandleAsync(message);

        // Assert
        _loggingServiceMock.Verify(x => x.SendEventAsync(submissionFile.UserId, monitoringEvent), Times.Once);
        _submissionStatusApiClientMock.Verify(
            x => x.PostEventAsync(
                It.Is<SubmissionClientPostEventRequest>(t =>
                    t.OrganisationId == submissionFile.OrganisationId &&
                    t.UserId == submissionFile.UserId &&
                    t.SubmissionId == submissionFile.SubmissionId &&
                    t.FileType == submissionFile.FileType &&
                    t.BlobName == blobName &&
                    t.FileId == submissionFile.FileId &&
                    t.ScanResult == message.Status &&
                    t.Errors.Count == 0)),
            Times.Once());
    }

    [TestMethod]
    [DataRow(ScanResult.AwaitingProcessing)]
    [DataRow(ScanResult.Quarantined)]
    [DataRow(ScanResult.FileInaccessible)]
    [DataRow(ScanResult.FailedToVirusScan)]
    public async Task HandleAsync_CallsLoggingServiceWithRightParameters_WhenVirusCheckNotSuccess(ScanResult scanResult)
    {
        // Arrange

        var message = new TradeAntivirusQueueResult
        {
            Key = Guid.NewGuid(),
            Collection = "pom",
            Status = scanResult
        };

        var submissionFile = new SubmissionFileResult(
            Guid.NewGuid(),
            SubmissionType.Producer,
            Guid.NewGuid(),
            "SomeFileName",
            FileType.Pom,
            Guid.NewGuid(),
            Guid.NewGuid(),
            It.IsAny<string>(),
            It.IsAny<Guid>());

        _submissionStatusApiClientMock.Setup(x => x.GetSubmissionFileAsync(It.IsAny<Guid>()))
            .ReturnsAsync(submissionFile);

        var monitoringEvent = new ProtectiveMonitoringEvent(
            submissionFile.SubmissionId,
            "epr-anti-virus-function-app",
            PmcCodes.Code0203,
            Priorities.ExceptionEvent,
            TransactionCodes.AntivirusThreatDetected,
            $"Antivirus check did not indicate success for file '{submissionFile.FileName}'",
            $"FileId: '{submissionFile.FileId}'");

        // Act
        await _systemUnderTest.HandleAsync(message);

        // Assert
        _loggingServiceMock.Verify(x => x.SendEventAsync(submissionFile.UserId, monitoringEvent), Times.Once);
        _submissionStatusApiClientMock.Verify(
            x => x.PostEventAsync(
                It.Is<SubmissionClientPostEventRequest>(t =>
                    t.OrganisationId == submissionFile.OrganisationId &&
                    t.UserId == submissionFile.UserId &&
                    t.SubmissionId == submissionFile.SubmissionId &&
                    t.FileType == submissionFile.FileType &&
                    t.BlobName == string.Empty &&
                    t.FileId == submissionFile.FileId &&
                    t.ScanResult == message.Status &&
                    t.Errors.Count == 1 && t.Errors.Contains(ErrorCodes.UploadedFileContainsThreat))),
            Times.Once());
    }

    [TestMethod]
    public async Task HandleAsync_LogsErrorAndDoesNotThrowException_WhenLoggingServiceCallFailed()
    {
        // Arrange

        var message = new TradeAntivirusQueueResult
        {
            Key = Guid.NewGuid(),
            Collection = "pom",
            Status = ScanResult.Success
        };

        _submissionStatusApiClientMock.Setup(x => x.GetSubmissionFileAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new SubmissionFileResult(
                Guid.NewGuid(),
                SubmissionType.Producer,
                Guid.NewGuid(),
                "SomeFileName",
                FileType.Pom,
                Guid.NewGuid(),
                Guid.NewGuid(),
                It.IsAny<string>(),
                It.IsAny<Guid>()));

        _loggingServiceMock.Setup(x => x.SendEventAsync(
                It.IsAny<Guid>(), It.IsAny<ProtectiveMonitoringEvent>()))
            .Throws(new Exception());

        var act = async () => await _systemUnderTest.HandleAsync(message);

        // Act

        // Assert
        await act.Should().NotThrowAsync();
        _loggerMock.VerifyLog(x => x.LogError("An error occurred creating the protective monitoring event"), Times.Once);
    }

    [TestMethod]
    [DataRow(SubmissionType.Producer, FileType.Pom, true, null)]
    [DataRow(SubmissionType.Producer, FileType.Pom, false, null)]
    [DataRow(SubmissionType.Registration, FileType.CompanyDetails, true, true)]
    [DataRow(SubmissionType.Registration, FileType.CompanyDetails, false, false)]
    [DataRow(SubmissionType.Registration, FileType.Brands, true, true)]
    [DataRow(SubmissionType.Registration, FileType.Brands, false, false)]
    [DataRow(SubmissionType.Registration, FileType.Partnerships, true, true)]
    [DataRow(SubmissionType.Registration, FileType.Partnerships, false, false)]
    public async Task HandleAsync_CorrectlySetsRequiresRowValidation_ForValidFileTypeAndFeatureFlag(SubmissionType submissionType, FileType fileType, bool featureFlag, bool? requiresRowValidation)
    {
        // Arrange
        var blobName = Guid.NewGuid().ToString();

        var message = new TradeAntivirusQueueResult
        {
            Key = Guid.NewGuid(),
            Collection = "CompanyDetails",
            Status = ScanResult.Success
        };

        var submissionFile = new SubmissionFileResult(
            Guid.NewGuid(),
            submissionType,
            Guid.NewGuid(),
            "SomeRegistrationFileName",
            fileType,
            Guid.NewGuid(),
            Guid.NewGuid(),
            It.IsAny<string>(),
            It.IsAny<Guid>());

        _featureManagerMock.Setup(x => x.IsEnabledAsync(nameof(FeatureFlags.EnableRegistrationRowValidation)))
            .ReturnsAsync(featureFlag);

        _blobStorageServiceMock
            .Setup(x => x.UploadFileStreamWithMetadataAsync(
                It.IsAny<Stream>(), It.IsAny<SubmissionType>(), It.IsAny<IDictionary<string, string>>()))
            .ReturnsAsync(blobName);

        _submissionStatusApiClientMock.Setup(x => x.GetSubmissionFileAsync(It.IsAny<Guid>()))
            .ReturnsAsync(submissionFile);

        var monitoringEvent = new ProtectiveMonitoringEvent(
            submissionFile.SubmissionId,
            "epr-anti-virus-function-app",
            PmcCodes.Code0203,
            Priorities.NormalEvent,
            TransactionCodes.AntivirusCleanUpload,
            $"Antivirus check was successful for file '{submissionFile.FileName}'",
            $"FileId: '{submissionFile.FileId}'");

        // Act
        await _systemUnderTest.HandleAsync(message);

        // Assert
        _loggingServiceMock.Verify(x => x.SendEventAsync(submissionFile.UserId, monitoringEvent), Times.Once);
        _submissionStatusApiClientMock.Verify(
            x => x.PostEventAsync(
                It.Is<SubmissionClientPostEventRequest>(t =>
                    t.OrganisationId == submissionFile.OrganisationId &&
                    t.UserId == submissionFile.UserId &&
                    t.SubmissionId == submissionFile.SubmissionId &&
                    t.FileType == submissionFile.FileType &&
                    t.BlobName == blobName &&
                    t.FileId == submissionFile.FileId &&
                    t.ScanResult == message.Status &&
                    t.Errors.Count == 0 &&
                    t.RequiresRowValidation == requiresRowValidation)),
            Times.Once());
    }
}
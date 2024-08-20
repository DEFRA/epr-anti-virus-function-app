namespace EPR.Antivirus.Application.UnitTests.Clients;

using Application.Clients;
using Azure.Messaging.ServiceBus;
using Data.DTOs.ServiceBusQueue;
using Data.Enums;
using Data.Options;
using Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public class ServiceBusQueueClientTests
{
    private Mock<ServiceBusSender> _serviceBusSenderMock;
    private Mock<ServiceBusClient> _serviceBusClientMock;
    private Mock<IOptions<ServiceBusOptions>> _optionsMock;

    private ServiceBusQueueClient _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _serviceBusSenderMock = new Mock<ServiceBusSender>();
        _serviceBusClientMock = new Mock<ServiceBusClient>();
        _serviceBusClientMock.Setup(x => x.CreateSender(It.IsAny<string>()))
            .Returns(_serviceBusSenderMock.Object);
        _optionsMock = new Mock<IOptions<ServiceBusOptions>>();
        _optionsMock.Setup(x => x.Value).Returns(
            new ServiceBusOptions
            {
                ConnectionString = "ConnectionString",
                PomUploadQueueName = "PomUploadQueueName",
                RegistrationDataQueueName = "RegistrationDataQueueName",
            });

        _systemUnderTest = new ServiceBusQueueClient(_serviceBusClientMock.Object, _optionsMock.Object, Mock.Of<ILogger<ServiceBusQueueClient>>());
    }

    [TestMethod]
    public async Task PostMessageAsync_WhenValid_CreateSenderIsCalled()
    {
        // Arrange
        var blobName = Guid.NewGuid().ToString();
        var message = new ServiceBusQueueMessage(
            blobName,
            Guid.NewGuid(),
            It.IsAny<SubmissionSubType>(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            It.IsAny<string>(),
            It.IsAny<Guid>(),
            false);

        // Act
        _systemUnderTest.SendMessageAsync(It.IsAny<SubmissionType>(), message);

        // Assert
        _serviceBusClientMock.Verify(x => x.CreateSender(It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task PostMessageAsync_WhenMessageFails_ThrowsServiceBusQueueClientException()
    {
        // Arrange
        var blobName = Guid.NewGuid().ToString();
        var message = new ServiceBusQueueMessage(
            blobName,
            Guid.NewGuid(),
            It.IsAny<SubmissionSubType>(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            It.IsAny<string>(),
            It.IsAny<Guid>(),
            false);

        _serviceBusSenderMock
            .Setup(x => x.SendMessageAsync(It.IsAny<ServiceBusMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        // Act & Assert
        await _systemUnderTest
            .Invoking(x => x.SendMessageAsync(It.IsAny<SubmissionType>(), message))
            .Should()
            .ThrowAsync<ServiceBusQueueClientException>()
            .WithMessage("Failed to post message to service bus");
    }
}
namespace EPR.Antivirus.Function.Tests;

using System;
using System.Text.Json;
using Application.Services.Interfaces;
using Data.DTOs.TradeAntivirusQueue;
using Data.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

[TestClass]
public class ServiceBusQueueTriggerTests
{
    private Mock<IAntivirusService> _antivirusServiceMock;

    private ServiceBusQueueTrigger _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _antivirusServiceMock = new Mock<IAntivirusService>();
        _systemUnderTest = new ServiceBusQueueTrigger(_antivirusServiceMock.Object, Mock.Of<ILogger<ServiceBusQueueTrigger>>());
    }

    [TestMethod]
    public async Task RunAsync_DoesNotThrowException_WhenMessagePayloadIsValid()
    {
        // Arrange
        var serviceBusReceivedMessage = JsonSerializer.Serialize(new TradeAntivirusQueueResult
        {
            Key = Guid.NewGuid(),
            Collection = "pom",
            Status = ScanResult.Success
        });

        // Act / Assert
        await _systemUnderTest.Invoking(x => x.RunAsync(serviceBusReceivedMessage))
            .Should()
            .NotThrowAsync<Exception>();
    }

    [TestMethod]
    public async Task RunAsync_ThrowsException_WhenMessagePayloadIsInvalid()
    {
        // Arrange
        const string serviceBusReceivedMessage = "{ \"Key\": \"invalid key\" }";

        // Act / Assert
        await _systemUnderTest.Invoking(x => x.RunAsync(serviceBusReceivedMessage))
            .Should()
            .ThrowAsync<Exception>();
    }
}
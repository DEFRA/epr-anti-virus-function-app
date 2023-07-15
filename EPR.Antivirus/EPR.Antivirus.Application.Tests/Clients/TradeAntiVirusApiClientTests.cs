namespace EPR.Antivirus.Application.Tests.Clients;

using System.Net;
using Application.Clients;
using Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

[TestClass]
public class TradeAntivirusApiClientTests
{
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private HttpClient _httpClient;

    private TradeAntivirusApiClient _systemUnderTest;

    [TestInitialize]
    public void TestInitialize()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://baseaddress.com/"),
        };

        _systemUnderTest =
            new TradeAntivirusApiClient(_httpClient, Mock.Of<ILogger<TradeAntivirusApiClient>>());
    }

    [TestMethod]
    public async Task GetSubmissionFileAsync_WhenValidRequest_ReturnsSubmissionFileResult()
    {
        // Arrange
        const string collection = "CollectionName";
        var key = Guid.NewGuid();

        const string fileContent = "fileContent";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(fileContent),
            })
            .Verifiable();

        // Act
        var result = await _systemUnderTest.GetFileAsync(collection, key);

        // Assert
        var resultReader = new StreamReader(result);
        var resultFileContent = await resultReader.ReadToEndAsync();
        resultFileContent.Should().Be(fileContent);
    }

    [TestMethod]
    public async Task GetSubmissionFileAsync_WhenInvalidRequest_ThrowsTradeAntivirusApiClientException()
    {
        // Arrange
        const string collection = "CollectionName";
        var key = Guid.NewGuid();

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException())
            .Verifiable();

        // Act / Assert
        await _systemUnderTest
            .Invoking(x => x.GetFileAsync(collection, key))
            .Should()
            .ThrowAsync<TradeAntivirusApiClientException>()
            .WithMessage("A success status code was not received when requesting antivirus file");
    }
}
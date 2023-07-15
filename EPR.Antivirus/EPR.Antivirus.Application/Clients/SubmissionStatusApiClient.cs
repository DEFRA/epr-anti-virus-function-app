namespace EPR.Antivirus.Application.Clients;

using System.Net.Http.Json;
using Data.DTOs.SubmissionStatusApi;
using Data.Enums;
using Exceptions;
using Interfaces;
using Microsoft.Extensions.Logging;

public class SubmissionStatusApiClient : ISubmissionStatusApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SubmissionStatusApiClient> _logger;

    public SubmissionStatusApiClient(
        HttpClient httpClient, ILogger<SubmissionStatusApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<SubmissionFileResult?> GetSubmissionFileAsync(Guid fileId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"submissions/files/{fileId}");
            response.EnsureSuccessStatusCode();

            _logger.LogInformation("Data received from Submission Status Api");

            return await response.Content.ReadFromJsonAsync<SubmissionFileResult>();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "A success status code was not received when requesting file metadata");
            throw new SubmissionStatusApiClientException(
                "A success status code was not received when requesting file metadata", exception);
        }
    }

    public async Task PostEventAsync(Guid organisationId, Guid userId, Guid submissionId, string blobName, Guid fileId, ScanResult scanResult, List<string> errors)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Add(nameof(organisationId), organisationId.ToString());
            _httpClient.DefaultRequestHeaders.Add(nameof(userId), userId.ToString());

            var response = await _httpClient.PostAsJsonAsync(
                $"submissions/{submissionId}/events",
                new SubmissionEventRequest(fileId, scanResult, blobName, Errors: errors));
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("Event posted to Submission Status Api");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "A success status code was not received when requesting file metadata");
            throw new SubmissionStatusApiClientException(
                "A success status code was not received when sending the error report", exception);
        }
    }
}
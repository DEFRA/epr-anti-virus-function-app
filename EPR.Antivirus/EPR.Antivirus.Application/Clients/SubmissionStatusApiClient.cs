namespace EPR.Antivirus.Application.Clients;

using System.Net.Http.Json;
using Data.DTOs.SubmissionStatusApi;
using Data.Enums;
using Data.Options;
using Exceptions;
using Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class SubmissionStatusApiClient : ISubmissionStatusApiClient
{
    private readonly HttpClient _httpClient;
    private readonly BlobStorageOptions _options;
    private readonly ILogger<SubmissionStatusApiClient> _logger;

    public SubmissionStatusApiClient(
        HttpClient httpClient,
        IOptions<BlobStorageOptions> options,
        ILogger<SubmissionStatusApiClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
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

    public async Task PostEventAsync(SubmissionClientPostEventRequest request)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Add("organisationId", request.OrganisationId.ToString());
            _httpClient.DefaultRequestHeaders.Add("userId", request.UserId.ToString());

            var response = await _httpClient.PostAsJsonAsync(
                $"submissions/{request.SubmissionId}/events",
                new SubmissionEventRequest(
                    request.FileId,
                    request.FileType == FileType.Pom ? _options.PomContainerName : _options.RegistrationContainerName,
                    request.ScanResult,
                    request.BlobName,
                    Errors: request.Errors));
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
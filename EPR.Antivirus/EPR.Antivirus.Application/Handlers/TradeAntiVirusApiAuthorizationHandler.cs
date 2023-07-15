﻿namespace EPR.Antivirus.Application.Handlers;

using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using Azure.Core;
using Azure.Identity;
using Data.Options;
using Microsoft.Extensions.Options;

[ExcludeFromCodeCoverage]
public class TradeAntivirusApiAuthorizationHandler : DelegatingHandler
{
    private const string Bearer = "Bearer";

    private readonly TokenRequestContext _tokenRequestContext;
    private readonly ClientSecretCredential _credentials;

    public TradeAntivirusApiAuthorizationHandler(IOptions<TradeAntivirusApiOptions> options)
    {
        var antivirusApiOptions = options.Value;
        _tokenRequestContext = new TokenRequestContext(new[] { antivirusApiOptions.Scope });
        _credentials = new ClientSecretCredential(antivirusApiOptions.TenantId, antivirusApiOptions.ClientId, antivirusApiOptions.ClientSecret);
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var tokenResult = await _credentials.GetTokenAsync(_tokenRequestContext, cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue(Bearer, tokenResult.Token);

        return await base.SendAsync(request, cancellationToken);
    }
}
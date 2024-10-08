# Use the SDK image to build your application
# FROM defradigital/dotnetcore-development:dotnet8.0 AS installer-env
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS installer-env

# Copy everything
COPY EPR.Antivirus.Function/. ./EPR.Antivirus.Function/.
COPY EPR.Antivirus.Application/. ./EPR.Antivirus.Application/.
COPY EPR.Antivirus.Data/. ./EPR.Antivirus.Data/.
COPY Directory.Build.props ./
COPY NuGet.Config ./
COPY stylecop.ruleset ./

# Use Build Argument to insert PAT into Nuget file
ARG PAT=localhost
RUN sed -i "s|</configuration>|<packageSourceCredentials><epr-packaging-common><add key=\"Username\" value=\"PAT\" /><add key=\"ClearTextPassword\" value=\"${PAT}\" /></epr-packaging-common></packageSourceCredentials></configuration>|" NuGet.Config

# Install Internal CA certificate
COPY EPR.Antivirus.Function/Certificates/internal-ca.crt /usr/local/share/ca-certificates/internal-ca.crt
RUN chmod 644 /usr/local/share/ca-certificates/internal-ca.crt && update-ca-certificates

# Build the application
RUN dotnet publish EPR.Antivirus.Function/*.csproj --output /home/site/wwwroot 

# Use the Azure Functions image as the final image
# FROM defradigital/dotnetcore:dotnet8.0
FROM mcr.microsoft.com/azure-functions/dotnet:4-dotnet8

# Create a non-root user and set permissions
RUN groupadd -r dotnet && \
    useradd -r -g dotnet dotnet && \
    chown -R dotnet /home

# Switch to the non-root user for the final image
USER dotnet

# Set environment variables
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

# Copy the published application from the installer-env image
COPY --from=installer-env ["/home/site/wwwroot", "/home/site/wwwroot"]

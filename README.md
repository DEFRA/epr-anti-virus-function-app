# EPR Antivirus Function


## Overview

This function listens to an Azure ServiceBus Topic on DEFRA Trade Anti-Virus Solution. If the uploaded file is reported as clean, the file is retrieved and normal processing will continue, otherwise for an infected file, a protective monitoring event with transaction code of `EPR_ANTIVIRUS_THREAT_DETECTED` will be generated.

## Service Bus Payload

```json
{
    "Key":"{{Submission FileId GUID}}",
    "Collection":"pom",
    "Status":"Success"
}
```

## How To Run 
 
### Prerequisites 
In order to run the service you will need the following dependencies 
 
- .NET 8
- Azure CLI

#### epr-packaging-common
##### Developers working for a DEFRA supplier
In order to restore and build the source code for this project, access to the `epr-packaging-common` package store will need to have been setup.
 - Login to Azure DevOps
 - Navigate to [Personal Access Tokens](https://dev.azure.com/defragovuk/_usersSettings/tokens)
 - Create a new token
   - Enable the `Packaging (Read)` scope
Add the following to your `src/Nuget.Config`
```xml
<packageSourceCredentials>
  <epr-packaging-common>
    <add key="Username" value="<email address>" />
    <add key="ClearTextPassword" value="<personal access token>" />
  </epr-packaging-common>
</packageSourceCredentials>
```
##### Members of the public
Clone the [epr_common](https://dev.azure.com/defragovuk/RWD-CPR-EPR4P-ADO/_git/epr_common) repository and add it as a project to the solution you wish to use it in. By default the repository will reference the files as if they are coming from the NuGet package. You simply need to update the references to make them point to the newly added project.
 
### Run 
Go to `EPR.AntiVirus/EPR.Antivirus.Function` directory and execute:

```
func start
```

### Docker
Run in terminal at the solution source root:

```
docker build -t antivirus -f EPR.Antivirus.Function/Dockerfile --build-arg PAT={YOUR_PAT_HERE} .
```

Fill out the environment variables and run the following command:  

```
docker run -e AzureWebJobsStorage="X" -e BlobStorage:ConnectionString="X" -e BlobStorage:PomContainerName="X" -e BlobStorage:RegistrationContainerName="X" -e FUNCTIONS_EXTENSION_VERSION="X" -e FUNCTIONS_WORKER_RUNTIME="X" -e LoggingApi:BaseUrl="X" -e ServiceBus:ConnectionString="X" -e ServiceBus:PomUploadQueueName="X" -e ServiceBus:RegistrationDataQueueName="X" -e SubmissionApi:BaseUrl="X" -e TradeAntivirusApi:BaseUrl="X" -e TradeAntivirusApi:ClientId="X" -e TradeAntivirusApi:ClientSecret="X" -e TradeAntivirusApi:EnableDirectAccess="X" -e TradeAntivirusApi:Scope="X" -e TradeAntivirusApi:SubscriptionKey="X" -e TradeAntivirusApi:TenantId="X" -e TradeAntivirusApi:Timeout="X" -e TradeAntivirusServiceBus:AntivirusTopicName="X" -e TradeAntivirusServiceBus:ConnectionString="X" -e TradeAntivirusServiceBus:SubscriptionName="X" antivirus
```

## How To Test 
 
### Unit tests 

On root directory `EPR.AntiVirus`, execute:

```
dotnet test
```
 
### Pact tests 
 
N/A
 
### Integration tests

N/A
 
## How To Debug 
 
## Environment Variables - deployed environments 

The structure of the appsettings can be found in the repository. Example configurations for the different environments can be found in [epr-app-config-settings](https://dev.azure.com/defragovuk/RWD-CPR-EPR4P-ADO/_git/epr-app-config-settings).

| Variable Name                                | Description                                                                                                          |
| -------------------------------------------- | -------------------------------------------------------------------------------------------------------------------- |
| AzureWebJobsStorage                          | The connection string for the Azure Web Jobs Storage                                                                 |
| BlobStorage__ConnectionString                | The connection string for the blob storage, where clean files are stored                                             |
| BlobStorage__PomContainerName                | The name of the blob container on the storage account, where uploaded POM files will be stored                       |
| BlobStorage__RegistrationContainerName       | The name of the blob container on the storage account, where uploaded organisation registration files will be stored |
| FUNCTIONS_EXTENSION_VERSION                  | The extension version for Azure Function - i.e. ~4                                                                   |
| FUNCTIONS_WORKER_RUNTIME                     | The runtime name for the Azure Function - i.e. `dotnet`                                                              |
| LoggingApi__BaseUrl                          | The base URL for the Logging API WebApp                                                                              |
| ServiceBus__ConnectionString                 | The connection string for the service bus                                                                            |
| ServiceBus__PomUploadQueueName               | The name of the upload queue, for the POM files                                                                      |
| ServiceBus__RegistrationDataQueueName        | The name of the upload queue for the organisation registration files                                                 |
| SubmissionApi__BaseUrl                       | The base URL for the Submission Status API WebApp                                                                    |
| TradeAntivirusApi__BaseUrl                   | The base URL for the Trade Antivirus API WebApp                                                                      |
| TradeAntivirusApi__ClientId                  | The client ID for the Trade Antivirus API WebApp                                                                     |
| TradeAntivirusApi__ClientSecret              | The client secret for the Trade Antivirus API WebApp                                                                 |
| TradeAntivirusApi__EnableDirectAccess        | A flag to enable direct access to the Trade Antivirus API                                                            |
| TradeAntivirusApi__Scope                     | The scope for the Trade Antivirus API                                                                                |
| TradeAntivirusApi__SubscriptionKey           | The subscription key for the Trade Antivirus API                                                                     |
| TradeAntivirusApi__TenantId                  | The tenant ID for the Trade Antivirus API                                                                            |
| TradeAntivirusApi__Timeout                   | The timeout value for the Trade Antivirus API                                                                        |
| TradeAntivirusServiceBus__AntivirusTopicName | The topic name for the Trade Antivirus service bus                                                                   |
| TradeAntivirusServiceBus__ConnectionString   | The connection string for the Trade Antivirus service bus                                                            |
| TradeAntivirusServiceBus__SubscriptionName   | The subscription name for the Trade Antivirus service bus                                                            |

## Additional Information 

See [ADR-023: Anti-Virus Service](https://eaflood.atlassian.net/wiki/spaces/MWR/pages/4318167185/ADR-023+Anti-Virus+Service)

See [DEFRA Trade Anti Virus API - secret renewal](https://eaflood.atlassian.net/wiki/spaces/EDIA/pages/6447759417/DEFRA+Trade+Anti+Virus+API+-+secret+renewal)

 
### Monitoring and Health Check 

Enable Health Check in the Azure portal and set the URL path to `ServiceBusQueueTrigger`

## Directory Structure 

### Source files 

- `EPR.AntiVirus.Application` - Application .NET source files
- `EPR.AntiVirus.Application.UnitTests` - .NET unit test files
- `EPR.AntiVirus.Functions` - Function .NET source files
- `EPR.AntiVirus.Functions.UnitTests` - .NET unit test files
- `EPR.AntiVirus.Data` - Data .NET source files

## Contributing to this project



Please read the [contribution guidelines](CONTRIBUTING.md) before submitting a pull request.

## Licence

[Licence information](LICENCE.md).


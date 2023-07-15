# EPR Submission Event Dispatcher

## Overview

TODO

## How To Run 
 
### Prerequisites 
In order to run the service you will need the following dependencies 
 
- .Net 6 
 
### Dependencies 
 
 
 
### Run 
 On root directory, execute
```
make run
```
### Docker
Run in terminal at the solution source root -

```
docker build -t antivirus -f EPR.AntiVirus.Functions/Dockerfile .
```

Fill out the environment variables and run the following command -  #TODO: UPDATE COMMAND BELOW
```
docker run -e AzureWebJobsStorage:"X" -e FUNCTIONS_WORKER_RUNTIME:"X" -e FUNCTIONS_WORKER_RUNTIME_VERSION:"X" -e ServiceBus:ConnectionString="X" -e ServiceBus:UploadQueueName="X" -e ServiceBus:SplitQueueName="X" -e StorageAccount:ConnectionString="X" -e StorageAccount:BlobContainerName="file" -e SubmissionApi:BaseUrl="X" -e SubmissionApi:SubmissionEndpoint="X" -e SubmissionApi:Version="X" -e SubmissionApi:SubmissionEventEndpoint="X" -e SubmissionApi:ValidationEventType=0 submissionchecksplitter
```

## How To Test 
 
### Unit tests 

On root directory, execute
```
make unit-tests
```
 
 
### Pact tests 
 
N/A
 
### Integration tests

N/A
 
## How To Debug 
 
 
## Environment Variables - deployed environments 
A copy of the configuration file and a description of each variable can be found [here](https://eaflood.atlassian.net/wiki/spaces/MWR/pages/4368500352/Anti+Virus+Variables).

## Additional Information 
 
### Logging into Azure 
 
### Usage 
 
### Monitoring and Health Check 
 
## Directory Structure 

### Source files 
- `EPR.AntiVirus.Application` - Application .Net source files
- `EPR.AntiVirus.Application.Tests` - .Net unit test files
- `EPR.AntiVirus.Functions` - Function .Net source files
- `EPR.AntiVirus.Functions.Tests` - .Net unit test files
 
### Source packages 

## Contributing 
 

#!/usr/bin/env bash

dotnet test EPR.Antivirus/EPR.Antivirus.Function.Tests/EPR.Antivirus.Function.Tests.csproj --logger "trx;logfilename=testResults.Function.trx"

dotnet test EPR.Antivirus/EPR.Antivirus.Application.Tests/EPR.Antivirus.Application.Tests.csproj --logger "trx;logfilename=testResults.Application.trx"

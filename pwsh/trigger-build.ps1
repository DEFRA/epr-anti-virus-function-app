param(
  [Parameter(Mandatory)]
  [string]$OrganizationUrl,
  [Parameter(Mandatory)]
  [string]$ProjectName,
  [Parameter(Mandatory)]
  [int]$PipelineDefinitionId,
  [Parameter(Mandatory)]
  [string]$BranchName,
  [int]$SleepDuration = 10,
  [switch]$EnableDebug
)

Set-StrictMode -Version 3.0

[string]$functionName = $MyInvocation.MyCommand
[DateTime]$startTime = [DateTime]::UtcNow
[int]$exitCode = -1

Write-Host "${functionName} started at $($startTime.ToString('u'))"

Set-Variable -Name ErrorActionPreference -Value Continue -scope global -WhatIf:$false
Set-Variable -Name VerbosePreference -Value Continue -Scope global -WhatIf:$false

if ($EnableDebug) {
  Set-Variable -Name DebugPreference -Value Continue -Scope Global -WhatIf:$false
}

try {
  $modulePath = Join-Path -Path $PSScriptRoot -ChildPath "modules/TriggerAdoBuild/TriggerAdoBuild.psm1"
  Import-Module -Name $modulePath
  
  [int]$buildId = Invoke-AdoPipeline `
    -OrganizationUrl $OrganizationUrl `
    -ProjectName $ProjectName `
    -PipelineDefinitionId $PipelineDefinitionId `
    -BranchName $BranchName
   
  [bool]$isComplete = $false
  
  do {
    [string]$buildStatus = Get-AdoBuildStatus `
      -OrganizationUrl $OrganizationUrl `
      -ProjectName $ProjectName `
      -BuildId $buildId
  
    if ([string]::IsNullOrWhiteSpace($buildStatus)) {
      $isComplete = $false
      Write-Host "ADO build is in progress. Sleeping for $SleepDuration seconds."
      Start-Sleep -Seconds $SleepDuration
    }
    elseif ($buildStatus -eq "succeeded") {
      Write-Host "ADO build has succeeded. Exiting."
      $isComplete = $true
    }
    else {
      throw [System.Management.Automation.RuntimeException]::new("ADO build has been deleted, cancelled or has failed. Exiting.")
    }
      
  } while (-not $isComplete)
  
  $exitCode = 0
}
catch {
  $exitCode = -2
  Write-Error $_.Exception.ToString()
  throw $_.Exception
}
finally {
  [DateTime]$endTime = [DateTime]::UtcNow
  [Timespan]$duration = $endTime.Subtract($startTime)
  
  Write-Host "${functionName} finished at $($endTime.ToString('u')) (duration $($duration -f 'g')) with exit code $exitCode"
  
  exit $exitCode
}
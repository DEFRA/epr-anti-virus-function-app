Set-StrictMode -Version 3.0

function Invoke-AdoPipeline {
  param (
    [Parameter(Mandatory)]
    [string]$OrganizationUrl,
    [Parameter(Mandatory)]
    [string]$ProjectName,
    [Parameter(Mandatory, ValueFromPipeline)]
    [int]$PipelineDefinitionId,
    [Parameter(Mandatory)]
    [string]$BranchName
  )

  begin {
    [string]$functionName = $MyInvocation.MyCommand
    Write-Debug "${functionName}:begin:start"
    Write-Debug "${functionName}:begin:OrganizationUrl=$OrganizationUrl"
    Write-Debug "${functionName}:begin:ProjectName=$ProjectName"
    Write-Debug "${functionName}:begin:BranchName=$BranchName"
    Write-Debug "${functionName}:begin:PipelineDefinitionId=$PipelineDefinitionId"
    Write-Debug "${functionName}:begin:end"
  }

  process {
    Write-Debug "${functionName}:process:start"
    
    [int]$buildId = $(az pipelines build queue `
      --organization $OrganizationUrl `
      --project $ProjectName `
      --definition-id $PipelineDefinitionId `
      --branch $BranchName `
      --query id `
      --output tsv)
   
    if ($LASTEXITCODE -ne 0) {
      throw [System.Management.Automation.RuntimeException]::new("An error occurred invoking the Azure DevOps pipeline")
    }
    
    Write-Output $buildId
    Write-Verbose "ADO pipeline build: $OrganizationUrl/$ProjectName/_build/results?buildId=$buildId&view=results"
    Write-Debug "${functionName}:process:buildId=$buildId"
    Write-Debug "${functionName}:process:end"
  }

  end {
    Write-Debug "${functionName}:end:start"
    Write-Debug "${functionName}:end:end"
  }
}

function Get-AdoBuildStatus {
  param (
    [Parameter(Mandatory)]
    [string]$OrganizationUrl,
    [Parameter(Mandatory)]
    [string]$ProjectName,
    [Parameter(Mandatory)]
    [int]$BuildId
  )

  begin {
    [string]$functionName = $MyInvocation.MyCommand
    Write-Debug "${functionName}:begin:start"
    Write-Debug "${functionName}:begin:OrganizationUrl=$OrganizationUrl"
    Write-Debug "${functionName}:begin:ProjectName=$ProjectName"
    Write-Debug "${functionName}:begin:BuildId=$BuildId"
    Write-Debug "${functionName}:begin:end"
  }

  process {
    Write-Debug "${functionName}:process:start"
    
    [string]$status = $(az pipelines build show `
      --organization $OrganizationUrl `
      --project $ProjectName `
      --id $BuildId `
      --query result `
      --output tsv)

    if ($LASTEXITCODE -ne 0) {
      throw [System.Management.Automation.RuntimeException]::new("An error occurred getting the Azure DevOps build status")
    }
  
    Write-Output $status
    Write-Debug "${functionName}:process:end"
  }

  end {
    Write-Debug "${functionName}:end:start"
    Write-Debug "${functionName}:end:end"
  }
}
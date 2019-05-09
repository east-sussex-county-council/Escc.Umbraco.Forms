Param(
  [Parameter(HelpMessage="Where are the source files for the application? (required)")]
  [string]$sourceFolder,
	
  [Parameter(Mandatory=$True,HelpMessage="Where is the folder where applications are installed to? (required)")]
  [string]$destinationFolder,
  
  [Parameter(HelpMessage="Where is the folder where applications are backed up before being updated?")]
  [string]$backupFolder,
	
  [Parameter(Mandatory=$True,HelpMessage="Where are the XDT transforms for the *.example.config files? (required)")]
  [string]$transformsFolder,

  [Parameter(Mandatory=$True,HelpMessage="What is the name of the IIS site this application is being set up to support? (required)")]
  [string]$websiteName,

  [Parameter(Mandatory=$True,HelpMessage="Why is this change being made? (required)")]
  [string]$comment
)

########################################################################
### BOOTSTRAP. Copy this section into each application setup script  ###
###            to make sure required functions are available.        ###
########################################################################

$pathOfThisScript = Split-Path $MyInvocation.MyCommand.Path -Parent
$parentFolderOfThisScript = $pathOfThisScript | Split-Path -Parent
$scriptsProject = 'Escc.WebApplicationSetupScripts'
$functionsPath = "$pathOfThisScript\..\$scriptsProject\functions.ps1"
if (Test-Path $functionsPath) {
  Write-Host "Checking $scriptsProject is up-to-date"
  Push-Location "$pathOfThisScript\..\$scriptsProject"
  git pull origin master
  Pop-Location
  Write-Host
  .$functionsPath
} else {
  if ($env:GIT_ORIGIN_URL) {
    $repoUrl = $env:GIT_ORIGIN_URL -f $scriptsProject
    git clone $repoUrl "$pathOfThisScript\..\$scriptsProject"
  } 
  else 
  {
    Write-Warning '$scriptsProject project not found. Please set a GIT_ORIGIN_URL environment variable on your system so that it can be downloaded.
  
Example: C:\>set GIT_ORIGIN_URL=https://example-git-server.com/{0}"
  
{0} will be replaced with the name of the repository to download.'
    Exit
  }
}

########################################################################
### END BOOTSTRAP. #####################################################
########################################################################

$destinationFolder = NormaliseFolderPath $destinationFolder
$backupFolder = NormaliseFolderPath $backupFolder
if (!$backupFolder) { $backupFolder = "$destinationFolder\backups" }
$backupFolder = "$backupFolder\$websiteName"
$destinationFolder = "$destinationFolder\$websiteName"
$transformsFolder = NormaliseFolderPath $transformsFolder

# Install the application
$projectName = "Escc.Umbraco.Forms.Workflows.ApplyRetentionSchedule" 
$appSourceFolder = NormaliseFolderPath $sourceFolder "$PSScriptRoot\$projectName"

BackupApplication "$destinationFolder/$projectName" $backupFolder $comment

# Which is newer, debug or release?
$debugExists = Test-Path "$appSourceFolder/bin/Debug/$projectName.exe"
$releaseExists = Test-Path "$appSourceFolder/bin/Release/$projectName.exe"
if (!$debugExists -and !$releaseExists) {
	Write-Host "$projectName.exe file not found. Build the project and try again."
	Exit
} elseif ($debugExists -and !$releaseExists) {
	$buildFolder = "Debug"
} elseif (!$debugExists -and $releaseExists) {
	$buildFolder = "Release"
} else {
	if ((Get-Item "$appSourceFolder/bin/Debug/$projectName.exe").LastWriteTimeUtc -gt (Get-Item "$appSourceFolder/bin/Release/$projectName.exe").LastWriteTimeUtc) {
		$buildFolder = "Debug"
	} else {
		$buildFolder = "Release"
	}
}

# Copy files.
robocopy "$appSourceFolder/bin/$buildFolder" "$destinationFolder/$projectName" /MIR /IF *.dll *.pdb *.exe

TransformConfig "$appSourceFolder\app.example.config" "$destinationFolder\$projectName\$projectName.exe.config" "$transformsFolder\$projectName\app.config.xdt"
if (Test-Path "$transformsFolder\$projectName\app.config.$websiteName.xdt") {
	# Transform to temp file to avoid file locking problem
	TransformConfig "$destinationFolder\$projectName\$projectName.exe.config" "$destinationFolder\$projectName\$projectName.exe.temp.config" "$transformsFolder\$projectName\app.config.$websiteName.xdt"
	copy "$destinationFolder\$projectName\$projectName.exe.temp.config" "$destinationFolder\$projectName\$projectName.exe.config"
	del "$destinationFolder\$projectName\$projectName.exe.temp.config"
}

Write-Host
Write-Host "Done. The application needs to be configured as a scheduled task, if you have not already done so." -ForegroundColor "Green"
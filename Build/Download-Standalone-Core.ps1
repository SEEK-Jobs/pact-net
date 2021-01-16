param (
)

Add-Type -AssemblyName System.IO.Compression.FileSystem
function Unzip
{
    param([string]$zipfile, [string]$outpath)

    [System.IO.Compression.ZipFile]::ExtractToDirectory($zipfile, $outpath)
}

$PSScriptFilePath = (Get-Item $MyInvocation.MyCommand.Path).FullName
$BuildRoot = Split-Path -Path $PSScriptFilePath -Parent
$SolutionRoot = Split-Path -Path $BuildRoot -Parent
$OutputPath = Join-Path $BuildRoot -ChildPath 'tools'
$NuGetExe = Join-Path $BuildRoot -ChildPath '..\.nuget\nuget.exe'
$7ZipSnapIn = Join-Path $BuildRoot -ChildPath '..\packages\7Zip4PowerShell.1.8.0\tools\7Zip4PowerShell.psd1'

If(Test-Path $OutputPath)
{
	Remove-Item $OutputPath -Recurse -Force | Out-Null
}
New-Item -ItemType directory -Path $OutputPath | Out-Null

& $NuGetExe install "$SolutionRoot\.nuget\download-standalone-core\packages.config" -outputdirectory "$SolutionRoot\packages"

Import-Module -Name $7ZipSnapIn

$StandaloneCoreVersion = '1.88.14';
$StandaloneCoreDownloadBaseUri = "https://github.com/pact-foundation/pact-ruby-standalone/releases/download/v$StandaloneCoreVersion"

# Download and extract the Windows core
$WindowsStandaloneCoreFileName = "pact-$StandaloneCoreVersion-win32.zip"
$WindowsStandaloneCoreFilePath = "$OutputPath\$WindowsStandaloneCoreFileName"
$WindowsStandaloneCoreDownload = "$StandaloneCoreDownloadBaseUri/$WindowsStandaloneCoreFileName";

Write-Output "Downloading the Windows Standalone Core from '$WindowsStandaloneCoreDownload'..."
$Client = New-Object System.Net.WebClient
$Client.DownloadFile($WindowsStandaloneCoreDownload, $WindowsStandaloneCoreFilePath)
Write-Output "Done"

Write-Output "Extracting the Windows Standalone Core..."
Expand-7Zip -ArchiveFileName $WindowsStandaloneCoreFilePath -TargetPath $OutputPath
Remove-Item $WindowsStandaloneCoreFilePath
Rename-Item "$OutputPath\pact" "pact-win32"
Write-Output "Done"

# Download and extract the OSX core
$OsxCompressionExtension = '.gz'
$OsxStandaloneCoreFileName = "pact-$StandaloneCoreVersion-osx.tar"
$OsxStandaloneCoreFilePath = "$OutputPath\$OsxStandaloneCoreFileName"
$OsxStandaloneCoreDownload = "$StandaloneCoreDownloadBaseUri/$OsxStandaloneCoreFileName$OsxCompressionExtension";

Write-Output "Downloading the OSX Standalone Core from '$OsxStandaloneCoreDownload'..."
$Client = New-Object System.Net.WebClient
$Client.DownloadFile($OsxStandaloneCoreDownload, "$OsxStandaloneCoreFilePath$OsxCompressionExtension")
Write-Output "Done"

Write-Output "Extracting the OSX Standalone Core..."
Expand-7Zip -ArchiveFileName "$OsxStandaloneCoreFilePath$OsxCompressionExtension" -TargetPath $OutputPath
Remove-Item "$OsxStandaloneCoreFilePath$OsxCompressionExtension"
Expand-7Zip -ArchiveFileName $OsxStandaloneCoreFilePath -TargetPath $OutputPath
Remove-Item $OsxStandaloneCoreFilePath
Rename-Item "$OutputPath\pact" "pact-osx"
Write-Output "Done"

# Download and extract the 32-bit Linux core
$Linux32CompressionExtension = '.gz'
$Linux32StandaloneCoreFileName = "pact-$StandaloneCoreVersion-linux-x86.tar"
$Linux32StandaloneCoreFilePath = "$OutputPath\$Linux32StandaloneCoreFileName"
$Linux32StandaloneCoreDownload = "$StandaloneCoreDownloadBaseUri/$Linux32StandaloneCoreFileName$Linux32CompressionExtension";

Write-Output "Downloading the Linux 32-bit Standalone Core from '$Linux32StandaloneCoreDownload'..."
$Client = New-Object System.Net.WebClient
$Client.DownloadFile($Linux32StandaloneCoreDownload, "$Linux32StandaloneCoreFilePath$Linux32CompressionExtension")
Write-Output "Done"

Write-Output "Extracting the Linux 32-bit Standalone Core..."
Expand-7Zip -ArchiveFileName "$Linux32StandaloneCoreFilePath$Linux32CompressionExtension" -TargetPath $OutputPath
Remove-Item "$Linux32StandaloneCoreFilePath$Linux32CompressionExtension"
Expand-7Zip -ArchiveFileName $Linux32StandaloneCoreFilePath -TargetPath $OutputPath
Remove-Item $Linux32StandaloneCoreFilePath
Rename-Item "$OutputPath\pact" "pact-linux-x86"
Write-Output "Done"

# Download and extract the 64-bit Linux core
$Linux64CompressionExtension = '.gz'
$Linux64StandaloneCoreFileName = "pact-$StandaloneCoreVersion-linux-x86_64.tar"
$Linux64StandaloneCoreFilePath = "$OutputPath\$Linux64StandaloneCoreFileName"
$Linux64StandaloneCoreDownload = "$StandaloneCoreDownloadBaseUri/$Linux64StandaloneCoreFileName$Linux64CompressionExtension";

Write-Output "Downloading the Linux 64-bit Standalone Core from '$Linux64StandaloneCoreDownload'..."
$Client = New-Object System.Net.WebClient
$Client.DownloadFile($Linux64StandaloneCoreDownload, "$Linux64StandaloneCoreFilePath$Linux64CompressionExtension")
Write-Output "Done"

Write-Output "Extracting the Linux 64-bit Standalone Core..."
Expand-7Zip -ArchiveFileName "$Linux64StandaloneCoreFilePath$Linux64CompressionExtension" -TargetPath $OutputPath
Remove-Item "$Linux64StandaloneCoreFilePath$Linux64CompressionExtension"
Expand-7Zip -ArchiveFileName $Linux64StandaloneCoreFilePath -TargetPath $OutputPath
Remove-Item $Linux64StandaloneCoreFilePath
Rename-Item "$OutputPath\pact" "pact-linux-x86_64"
Write-Output "Done"

param (
	[switch]$GenerateSummaryReport
)

$PSScriptFilePath = (Get-Item $MyInvocation.MyCommand.Path).FullName
$BuildRoot = Split-Path -Path $PSScriptFilePath -Parent
$SolutionRoot = Split-Path -Path $BuildRoot -Parent

cd $SolutionRoot
& git submodule update --init
cd $BuildRoot

$NuGetExe = Join-Path $BuildRoot -ChildPath '..\.nuget\nuget.exe'
$OpenCoverExe = Join-Path $BuildRoot -ChildPath '..\packages\OpenCover.4.6.519\tools\OpenCover.Console.exe'
$XUnitExe = Join-Path $BuildRoot -ChildPath '..\packages\xunit.runner.console.2.2.0\tools\xunit.console.exe'
$ReportGenExe = Join-Path $BuildRoot -ChildPath '..\packages\ReportGenerator.1.9.1.0\ReportGenerator.exe'

& $NuGetExe install "$SolutionRoot\.nuget\coverage\packages.config" -outputdirectory "$SolutionRoot\packages"

New-Item -ItemType directory -Path "$BuildRoot\coverage" -ErrorAction:ignore

& $OpenCoverExe `
    -register:user `
    "-target:$XUnitExe" `
    '-targetargs:..\PactNet.Tests\bin\Release\net46\PactNet.Tests.dll -noshadow' `
    '-filter:+[PactNet]* -[*Tests]*' `
    '-output:.\coverage\results.xml'
    
if($GenerateSummaryReport)
{
	& $ReportGenExe '-reports:.\coverage\results.xml' '-reporttypes:HtmlSummary' "-targetdir:$BuildRoot\coverage"
}
else
{
	& $ReportGenExe '-reports:.\coverage\results.xml' "-targetdir:$BuildRoot\coverage"
}

cd $SolutionRoot

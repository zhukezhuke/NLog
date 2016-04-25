<#

.SYNOPSIS
This is a Powershell script to bootstrap a Cake build.

.DESCRIPTION
This Powershell script will download NuGet if missing, restore NuGet tools (including Cake)
and execute your Cake build script with the parameters you provide.

.PARAMETER Script
The build script to execute.
.PARAMETER Target
The build script target to run.
.PARAMETER Configuration
The build configuration to use.
.PARAMETER Verbosity
Specifies the amount of information to be displayed.
.PARAMETER Experimental
Tells Cake to use the latest Roslyn release.
.PARAMETER WhatIf
Performs a dry run of the build script.
No tasks will be executed.
.PARAMETER Mono
Tells Cake to use the Mono scripting engine.

.LINK
http://cakebuild.net
#>

Param(
    [string]$Script = "build.cake",
    [string]$Target = "Default",
    [string]$Configuration = "Release",
    [ValidateSet("Quiet", "Minimal", "Normal", "Verbose", "Diagnostic")]
    [string]$Verbosity = "Verbose",
    [switch]$Experimental,
    [Alias("DryRun","Noop")]
    [switch]$WhatIf,
    [switch]$Mono,
    [switch]$SkipToolPackageRestore,
	[string]$DnxVersion = "1.0.0-rc1-update1",
	[string]$DnxInstallOption="-NGen",
	[Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$ScriptArgs
)

Write-Host "Preparing to run build script..."

# Should we show verbose messages?
if($Verbose.IsPresent)
{
    $VerbosePreference = "continue"
}

$NUGET_VERSION = "latest"
$TOOLS_DIR = Join-Path $PSScriptRoot "tools"
$NUGET_EXE = Join-Path $TOOLS_DIR "nuget.exe"
$NUGET_SOURCE = "https://api.nuget.org/v3/index.json"
$PACKAGES_CONFIG = Join-Path $TOOLS_DIR "packages.config"

$CAKE_EXE = Join-Path $TOOLS_DIR "Cake/Cake.exe"


# Should we use mono?
$UseMono = "";
if($Mono.IsPresent) {
    Write-Verbose -Message "Using the Mono based scripting engine."
    $UseMono = "-mono"
}

# Should we use the new Roslyn?
$UseExperimental = "";
if($Experimental.IsPresent -and !($Mono.IsPresent)) {
    Write-Verbose -Message "Using experimental version of Roslyn."
    $UseExperimental = "-experimental"
}

# Is this a dry run?
$UseDryRun = "";
if($WhatIf.IsPresent) {
    $UseDryRun = "-dryrun"
}

# Make sure tools folder exists
if ((Test-Path $PSScriptRoot) -and !(Test-Path $TOOLS_DIR)) {
    Write-Verbose -Message "Creating tools directory..."
    New-Item -Path $TOOLS_DIR -Type directory | out-null
}

If( !(Test-Path $NUGET_EXE)) {
	Write-Verbose "Restoring nuget.exe from nuget.org"
	Invoke-WebRequest "https://dist.nuget.org/win-x86-commandline/$NUGET_VERSION/nuget.exe" -OutFile $NUGET_EXE
}

# Save nuget.exe path to environment to be available to child processed
$ENV:NUGET_EXE = $NUGET_EXE

# Make sure that Cake has been installed.
if (!(Test-Path $CAKE_EXE)) {
    Throw "Could not find Cake.exe at $CAKE_EXE"
}

&$NUGET_EXE install $PACKAGES_CONFIG -OutputDirectory $TOOLS_DIR -ExcludeVersion -Source $NUGET_SOURCE

# Download latest dnvm utility
&{$Branch='dev';iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.ps1'))}
&"dnvm" install "$DnxVersion" -runtime CLR -arch x86 -alias default $DnxInstallOption

# Start Cake
Write-Host "Running build script..."
Invoke-Expression "$CAKE_EXE `"$Script`" -target=`"$Target`" -configuration=`"$Configuration`" -verbosity=`"$Verbosity`" $UseMono $UseDryRun $UseExperimental -dnxVersion=`"$dnxVersion`" $ScriptArgs"
exit $LASTEXITCODE

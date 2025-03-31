# Get-VersionInfo.ps1

# Load Directory.Build.props as an XML
[xml]$xml = Get-Content "$PSScriptRoot/../src/Directory.Build.props"

# Extract VersionPrefix, VersionSuffix, and FileVersion
$versionPrefix = $xml.Project.PropertyGroup.VersionPrefix
$versionSuffix = $xml.Project.PropertyGroup.VersionSuffix
$fileVersion = $xml.Project.PropertyGroup.FileVersion

# Combine VersionPrefix and VersionSuffix only if VersionSuffix is not empty
if ([string]::IsNullOrWhiteSpace($versionSuffix)) {
    $fullVersion = $versionPrefix
} else {
    $fullVersion = "$versionPrefix-$versionSuffix"
}

# Output the results for GitHub Actions
echo "version=$fullVersion" >> $env:GITHUB_OUTPUT
echo "file-version=$fileVersion" >> $env:GITHUB_OUTPUT
echo "clean-version=$versionPrefix" >> $env:GITHUB_OUTPUT
# InjectGitVersion.ps1
#
# Set the version in the projects AssemblyInfo.cs file
#

# Get version info from Git. example 1.2.3-45-g6789abc
$gitVersion = git describe --tags --long --always;
$gitBranch = git rev-parse --abbrev-ref HEAD;

# Parse Git version info into semantic pieces
if ($gitVersion -match '(.*)-(\d+)-[g](\w+)$')
{
    $gitTag = $Matches[1];
    $gitCount = $Matches[2];
    $gitSHA1 = $Matches[3];

    $fileVersion = $gitTag + "." + $gitCount;
    $infoVersion = $gitTag + "." + $gitCount + "-" + $gitSHA1;
    if ($gitBranch -ne "master")
    {
        $infoVersion += "-" + $gitBranch;
    }
}
else
{
    Write-Warning "Can not detect version number!";
    $fileVersion = "0.0.0.0";
    $infoVersion = "0.0.0.0-SNAPSHOT";
}

echo "File version: $fileVersion"
echo "Info version: $infoVersion"

# Define file variables
$assemblyFile = $args[0] + "\Properties\AssemblyInfo.cs";
$templateFile = $args[0] + "\Properties\AssemblyInfo_template.cs";

# Read template file, overwrite place holders with git version info
$newAssemblyContent = Get-Content $templateFile |
%{$_ -replace '\$FILEVERSION\$', $fileVersion } |
%{$_ -replace '\$INFOVERSION\$', $infoVersion };

# Write AssemblyInfo.cs file only if there are changes
If (-not (Test-Path $assemblyFile) -or ((Compare-Object (Get-Content $assemblyFile) $newAssemblyContent))) {
    echo "Injecting Git Version Info to AssemblyInfo.cs";
    $newAssemblyContent > $assemblyFile;       
}

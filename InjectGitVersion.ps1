# InjectGitVersion.ps1
#
# Set the version in the projects AssemblyInfo.cs file
#

$env:Path += ';C:\msys64\usr\bin'

# Get version info from Git. example 1.2.3-45-g6789abc
$gitVersion = git describe --long --always;

# Parse Git version info into semantic pieces
$gitVersion -match '(.*)-(\d+)-[g](\w+)$';
$gitTag = $Matches[1];
$gitCount = $Matches[2];
$gitSHA1 = $Matches[3];

# Define file variables
$assemblyFile = $args[0] + "\Properties\AssemblyInfo.cs";
$templateFile = $args[0] + "\Properties\AssemblyInfo_template.cs";

# Read template file, overwrite place holders with git version info
$newAssemblyContent = Get-Content $templateFile |
    %{$_ -replace '\$FILEVERSION\$', ($gitTag + "." + $gitCount) } |
    %{$_ -replace '\$INFOVERSION\$', ($gitTag + "." + $gitCount + "-" + $gitSHA1) };

 Write AssemblyInfo.cs file only if there are changes
If (-not (Test-Path $assemblyFile) -or ((Compare-Object (Get-Content $assemblyFile) $newAssemblyContent))) {
    echo "Injecting Git Version Info to AssemblyInfo.cs"
    $newAssemblyContent > $assemblyFile;       
}

# Variables
$projectFile = "src\MsSqlToolBelt\MsSqlToolBelt.csproj"
$projectFile = Resolve-Path -Path $projectFile

$outputFile = "src\MsSqlToolBelt\Packages.csv"
$outputFile = Resolve-Path -Path $outputFile

Write-Host "Files:"
Write-Host " - Project file: $projectFile"
Write-Host " - Output......: $outputFile"

$doc = [XML](Get-Content -Path $projectFile)

$elements = $doc.SelectNodes("//PackageReference")

if ($elements -eq $null)
{
    Write-Host "Can't determine packages."
    return -1
}

$result = [System.Collections.ArrayList]::new()
foreach ($element in $elements)
{
    $package = $element.GetAttribute("Include")
    $version = $element.GetAttribute("Version")
    $result.Add("$package;$version") | Out-Null
}

$result | Out-File $outputFile

Write-Host "NuGet packages extracted and saved to $outputFile"

return 0;

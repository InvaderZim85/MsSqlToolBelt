Param
(
    [Parameter(Position = 0, Mandatory = $false, HelpMessage = "The path of the project file")]
    [string]$projectFile
)

# Checks if the specified filepath is valid or not
function IsFileValid($filePath)
{
    # Check if the value is empty or not
    if ([string]::IsNullOrEmpty($filePath))
    {
        return $false
    }

    # Get the complete path and check if the file exists
    $tmpPath = $filePath
    try
    {
        $tmpPath = Resolve-Path -Path $filePath
    }
    catch
    {
        return $false
    }
    
    if ([System.IO.File]::Exists($tmpPath))
    {
        return $true
    }

    return $false
}

# Generates a new version number 
function GenerateVersionNumber($oldVersion) 
{
    $tmpVersion = [Version]::new()

    if (-not [string]::IsNullOrEmpty($oldVersion)) 
    {
        try 
        {
            $tmpVersion = [Version]::new($oldVersion)
        }
        catch
        {
            Write-Host "Can't determine old version. Fallback to new version."
        }
    }

    $year = Get-Date -Format "yy"
    $dayOfYear = (Get-Date).DayOfYear
    $build = 0
    $minuteSinceMidnight = [System.Math]::Round((Get-Date).ToUniversalTime().TimeOfDay.TotalMinutes)

    Write-Host "Compare old and new version"
    if ($year -eq $tmpVersion.Major -and $dayOfYear -eq $tmpVersion.Minor)
    {
        $build = $tmpVersion.Build + 1 # Add on to the build number
    }

    $newVersion = [Version]::new($year, $dayOfYear, $build, $minuteSinceMidnight)

    return $newVersion
}

# Extracts the old version number
function GetVersionNumber($filePath)
{
    $fileValid = IsFileValid $filePath;
    if (-not $fileValid)
    {
        Write-Host "Can't determine current version number."
        return "0.0.0.0";
    }

    $doc = [XML](Get-Content -Path $filePath)

    # Get the specified element
    $element = $doc.SelectSingleNode("//AssemblyVersion")

    if ($element -eq $null)
    {
        Write-Host "Can't determine version number. Fallback"
        return "0.0.0.0"
    }

    # Return the version number
    return $element.InnerText
}

# Changes the version in the project file
function ChangeProjectConfig($filePath, $newVersion)
{
    # AssemblyVersion
    ChangeXmlFile $filePath "//AssemblyVersion" $newVersion

    # FileVersion
    ChangeXmlFile $filePath "//FileVersion" $newVersion
}

# Changes the value of a specified node
function ChangeXmlFile($filePath, $nodeName, $newValue)
{
    $fileValid = IsFileValid $filePath;

    if (-not $fileValid)
    {
        Write-Host "ERROR > Can't load file '$filePath'"
        return
    }

    $doc = [XML](Get-Content -Path $filePath)

    # Get the specified element
    $element = $doc.SelectSingleNode($nodeName)

    Write-Host "Node: $nodeName - Old value: $($element.InnerText); New value: $newValue"

    # Change the element
    $element.InnerText = $newValue

    # Save the changes
    $doc.Save($filePath)
}

if ([string]::IsNullOrEmpty($projectFile))
{
    $projectFile = ".\MsSqlToolBelt\MsSqlToolBelt.csproj"
}

Write-Host "Start version update. Files:"
Write-Host " - Project file: $projectFile"

try
{
    # Check if both files valid...
    $validProjectFile = IsFileValid $projectFile

    if (-not $validProjectFile)
    {
        Write-Host "The project file is not valid."
        return
    }

    # Set the complete path
    $projectFile = Resolve-Path -Path $projectFile

    # Get the old and new version number
    $oldVersion = GetVersionNumber $projectFile
    $newVersion = GenerateVersionNumber $oldVersion

    Write-Host "Change version from $oldVersion to $newVersion"

    # Update the files
    Write-Host "Update project file"
    ChangeProjectConfig $projectFile $newVersion

    return 0
}
catch
{
    Write-Host "An error has occurred during the process. Error: $_"
    return 1
}
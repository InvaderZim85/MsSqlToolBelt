# Checks if the specified file path is valid
function IsFileValid($filePath) {
    # Check if the value is empty or not
    if ([string]::IsNullOrEmpty($filePath)) {
        return $false;
    }

    # Get the complete path and check if the file exists
    $tmpPath = $filePath
    try {
        $tmpPath = Resolve-Path -Path $filePath
    }
    catch {
        return $false
    }

    if ([System.IO.File]::Exists($tmpPath)) {
        return $true
    }

    return $false
}

# Gets the current version number
function GetVersionNumber($filePath) {
    $fileValid = IsFileValid $filePath

    if (-not $fileValid) {
        Write-Host "Can't determine current version number."
        return "1.0.0.0"
    }

    $doc = [XML](Get-Content -Path $filePath)

    # Get the specified element
    $element = $doc.SelectSingleNode("//AssemblyVersion")

    if ($element -eq $null) {
        Write-Host "Can't determine current version number. Fallback to 1.0.0.0"
        return "1.0.0.0"
    }

    # Return the version number
    return $element.InnerText
}

# Generates a new version number
function GenerateVersionNumber($oldVersion) {
    $tmpVersion = [Version]::new()

    if (-not [string]::IsNullOrEmpty($oldVersion)) {
        try {
            $tmpVersion = [Version]::new($oldVersion)
        }
        catch {
            Write-Host "Can't determine old version. Fallback to new version"
        }
    }

    $year = Get-Date -Format "yy"
    $dayOfYear = (Get-Date).DayOfYear
    $build = 0
    $minutesSinceMidnight = [System.Math]::Round((Get-Date).ToUniversalTime().TimeOfDay.TotalMinutes)

    Write-Host "Compare old and nwe version"
    if ($year -eq $tmpVersion.Major -and $dayOfYear -eq $tmpVersion.Minor) {
        # Add on to the build number if the major and minor versions are equal
        $build = $tmpVersion.Build + 1
    }

    $newVersion = [Version]::new($year, $dayOfYear, $build, $minutesSinceMidnight)

    return $newVersion
}

# Changes the value of a specified XML node
function ChangeXmlNode($filePath, $nodeName, $newValue) {
    $fileValid = IsFileValid $filePath

    if (-not $fileValid) {
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

# Changes the version of the project file
function ChangeProjectFile($filePath, $newVersion) {
    # Assembly version
    ChangeXmlNode $filePath "//AssemblyVersion" $newVersion

    # File Version
    ChangeXmlNode $filePath "//FileVersion" $newVersion
}

# ===========================
# Main entry point
# ===========================
$projectFile = "src\MsSqlToolBelt\MsSqlToolBelt\MsSqlToolBelt.csproj"
Write-Host "Start version update. Files:"
Write-Host " - Project file: $projectFile"

try {
    # Check if the file is valid
    $validProjectFile = IsFileValid $projectFile

    if (-not $validProjectFile) {
        Write-Host "The project file is not valid."
        return 1
    }

    # ===========================
    # Change version number
    # Set the complete path
    $projectFile = Resolve-Path -Path $projectFile

    # Get the old and new version number
    $oldVersion = GetVersionNumber $projectFile
    $newVersion = GenerateVersionNumber $oldVersion

    Write-Host "Change version from $oldVersion to $newVersion"

    # Update the files
    Write-Host "Update project file"
    ChangeProjectFile $projectFile $newVersion

    # ===========================
    # Extract the packages to 
    # generate the package file
    $packageScript = "src/MsSqlToolBelt/ExtractPackages.ps1"

    # Run the file
    & $packageScript

    # ===========================
    # Create the release
    $dotnet = "C:\Program Files\dotnet\dotnet.exe"
    $output = Resolve-Path -Path "src\MsSqlToolBelt\MsSqlToolBelt\bin\"

    # Clear the previous builds
    Write-Host "Clear output directory $output"
    Remove-Item "$output\*" -Recurse -Confirm:$false

    # Create the release
    & $dotnet publish "src/MsSqlToolBelt/MsSqlToolBelt.sln" -p:PublishProfile=src/InputCounter/Properties/PublishProfiles/FolderProfile

    return 0
}
catch {
    Write-Host "An error has occurred during the process. Error: $_"
    return 1
}
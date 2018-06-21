$GameDirectory = "./game_files"
$PatcherBin = "../GOIModdingAPI/Patcher/bin/Release/netcoreapp2.0/Patcher.dll"
$DistDirectory  ="../dist"

[string[]] $DistFiles = @(
    "GettingOverIt_Data/Managed/Assembly-CSharp.dll",
    "GettingOverIt_Data/Managed/0Harmony.dll",
    "GettingOverIt_Data/Managed/ModAPI.dll"
    "GettingOverIt_Data/Managed/ModAPI.pdb"
)

# Patch game assembly
dotnet $PatcherBin $GameDirectory

if (Test-Path $DistDirectory) {
    Remove-Item -Recurse -Path $DistDirectory
}

# Create dist directory and copy files to it
New-Item -ItemType Directory -Force -Path $DistDirectory

foreach ($filePath in $DistFiles) {
    $distFilePathDirectory = [Io.Path]::Combine($DistDirectory, [Io.Path]::GetDirectoryName($filePath))
    $distFileName = [Io.Path]::GetFileName($filePath)
    
    # Create directory if it doesn't exist
    if ((Test-Path -Path $distFilePathDirectory) -eq $false) {
        Write-Host "Creating directory ${$distFilePathDirectory}"
        New-Item -ItemType Directory -Force -Path $distFilePathDirectory
    }

    Write-Host "Copying ${$filePath} to dist"
    Copy-Item $filePath -Destination [Io.Path]::Combine($distFilePathDirectory, $distFileName)
}

tree /F $DistDirectory # debug
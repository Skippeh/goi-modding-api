Param(
    [Parameter(Mandatory=$true)]
    [String]$steam_username,

    [Parameter(Mandatory=$true)]
    [String]$steam_password
)

$AppId = 240720
$DepotId = 240721
$DownloadDirectory = "./game_files"

Add-Type -AssemblyName System.IO.Compression.FileSystem

$DepotDownloaderUrl = "https://github.com/SteamRE/DepotDownloader/releases/download/DepotDownloader_2.3.0/depotdownloader-2.3.0-hotfix1.zip"
[Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
[Net.ServicePointManager]::SecurityProtocol = "tls12, tls11, tls"
$WebClient = New-Object System.Net.WebClient

Remove-Item -Recurse -Path "./depotdownloader"
New-Item -ItemType Directory -Force -Path "./depotdownloader"

Write-Host "Downloading and extracting DepotDownloader"

try {
    $CurrentDirectory = (Get-Item -Path ".").FullName
    $target = [Io.Path]::Combine($CurrentDirectory, "depotdownloader", "bin.zip");
    $WebClient.DownloadFile($DepotDownloaderUrl, $target)

    $source = $target
    $target = [Io.Path]::Combine($CurrentDirectory, "depotdownloader")
    [System.IO.Compression.ZipFile]::ExtractToDirectory($source, $target)
    Remove-Item -Path "./depotdownloader/bin.zip"
}
catch {
    Write-Host -ForegroundColor Red $_.Exception
    return 1
}

Write-Host "Downloading game files"

dotnet "./depotdownloader/depotdownloader.dll" -username $steam_username -password $steam_password -app $AppId -filelist filelist.txt -os windows -depot $DepotId -dir $DownloadDirectory
param()
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

$manifestPath = Join-Path $PSScriptRoot 'package\manifest.json'
$manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
$dll = Join-Path $PSScriptRoot 'bin\Release\net48\BetterMinimap.dll'
if (-not (Test-Path $dll)) { throw 'Build the Release DLL first.' }
$version = [System.Reflection.AssemblyName]::GetAssemblyName($dll).Version
if ($version.ToString(3) -ne $manifest.version_number) { throw 'DLL and manifest versions differ.' }

$output = Join-Path $PSScriptRoot 'artifacts'
New-Item -ItemType Directory -Force $output | Out-Null
$stage = Join-Path $output ('package-' + [guid]::NewGuid().ToString('N'))
$zip = Join-Path $output ("AndrewSha-BetterMinimap-$($manifest.version_number)-r2modman.zip")
try {
    New-Item -ItemType Directory -Force (Join-Path $stage 'plugins') | Out-Null
    Copy-Item $dll (Join-Path $stage 'plugins\BetterMinimap.dll')
    Copy-Item $manifestPath (Join-Path $stage 'manifest.json')
    Copy-Item (Join-Path $PSScriptRoot 'README.md') (Join-Path $stage 'README.md')
    Copy-Item (Join-Path $PSScriptRoot 'LICENSE') (Join-Path $stage 'LICENSE')
    Copy-Item (Join-Path $PSScriptRoot 'CHANGELOG.md') (Join-Path $stage 'CHANGELOG.md')

    # Original 256x256 compass icon, generated without third-party assets.
    $bitmap = New-Object System.Drawing.Bitmap 256,256
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $ring = New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(91,177,178)),8
    $north = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(244,188,84))
    $south = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(224,231,233))
    try {
        $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
        $graphics.Clear([System.Drawing.Color]::FromArgb(25,35,45))
        $graphics.DrawEllipse($ring,32,32,192,192)
        $graphics.DrawLine($ring,128,18,128,40)
        $graphics.DrawLine($ring,128,216,128,238)
        $graphics.DrawLine($ring,18,128,40,128)
        $graphics.DrawLine($ring,216,128,238,128)
        $graphics.FillPolygon($north, [System.Drawing.Point[]]@(
            [System.Drawing.Point]::new(174,64),
            [System.Drawing.Point]::new(146,146),
            [System.Drawing.Point]::new(110,110)))
        $graphics.FillPolygon($south, [System.Drawing.Point[]]@(
            [System.Drawing.Point]::new(82,192),
            [System.Drawing.Point]::new(146,146),
            [System.Drawing.Point]::new(110,110)))
        $bitmap.Save((Join-Path $stage 'icon.png'), [System.Drawing.Imaging.ImageFormat]::Png)
    } finally {
        $graphics.Dispose(); $bitmap.Dispose()
        $ring.Dispose(); $north.Dispose(); $south.Dispose()
    }

    if (Test-Path $zip) { Remove-Item $zip }
    $archive = [System.IO.Compression.ZipFile]::Open($zip, [System.IO.Compression.ZipArchiveMode]::Create)
    try {
        Get-ChildItem $stage -File -Recurse | ForEach-Object {
            $entry = $_.FullName.Substring($stage.Length + 1).Replace('\', '/')
            [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $_.FullName, $entry) | Out-Null
        }
    } finally { $archive.Dispose() }
    Write-Output $zip
} finally {
    if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
}

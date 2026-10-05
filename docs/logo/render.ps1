# Makes every icon from the two drawings beside this script, with Edge's headless renderer:
#   - the Android launcher icon: an adaptive icon's foreground (the sticker) and monochrome layer
#     (for themed icons), at each screen density, into MoneyBud.Phone.Android's Resources;
#   - moneybud.ico, the Windows icon of the desktop window and the phone-in-a-PC window.
# Run it again after changing a drawing, and commit what it makes:
#   powershell -ExecutionPolicy Bypass -File docs/logo/render.ps1

$ErrorActionPreference = 'Stop'
$here = $PSScriptRoot
$repo = Resolve-Path (Join-Path $here '..\..')
$edge = @(
    "${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe",
    "$env:ProgramFiles\Microsoft\Edge\Application\msedge.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $edge) { throw 'Microsoft Edge was not found; it is what draws the SVGs.' }

$work = Join-Path ([IO.Path]::GetTempPath()) ('moneybud-logo-' + [Guid]::NewGuid())
New-Item -ItemType Directory $work | Out-Null

# Draws one SVG as a square PNG of $size pixels, showing the part of the drawing in $viewBox.
function Render([string] $svg, [string] $viewBox, [int] $size, [string] $out) {
    $text = Get-Content (Join-Path $here $svg) -Raw
    $text = $text -replace 'viewBox="[^"]*"', "viewBox=`"$viewBox`"" `
                  -replace 'width="\d+" height="\d+"', "width=`"$size`" height=`"$size`""
    $page = Join-Path $work 'page.html'
    Set-Content $page "<!doctype html><html><body style=`"margin:0;background:transparent`">$text</body></html>" -Encoding utf8
    New-Item -ItemType Directory -Force (Split-Path $out) | Out-Null
    if (Test-Path $out) { Remove-Item $out }
    # Edge talks on stderr, which Windows PowerShell would take for a failure: whether the picture
    # was written is what counts.
    $ErrorActionPreference = 'Continue'
    & $edge --headless --disable-gpu --hide-scrollbars --user-data-dir="$work\profile" `
        --default-background-color=00000000 --window-size="$size,$size" `
        --screenshot="$out" "file:///$($page -replace '\\', '/')" 2>&1 | Out-Null
    if (-not (Test-Path $out)) { throw "Edge did not draw $out." }
}

# Android: an adaptive icon is 108 dp square, of which a launcher may show as little as the middle
# circle of 66 dp. The sticker (about 106 units from its middle, at 132,158) is fitted into that
# circle: 108 / 66 × 2 × 106 ≈ 347 units across. The background is transparent, so the launcher shows
# the sticker die-cut, as Niagara shows its own stickers.
$android = Join-Path $repo 'src\MoneyBud.Phone.Android\Resources'
$adaptive = '-41.5 -15.5 347 347'
$densities = [ordered]@{ mdpi = 108; hdpi = 162; xhdpi = 216; xxhdpi = 324; xxxhdpi = 432 }
foreach ($d in $densities.Keys) {
    Render 'moneybud.svg' $adaptive $densities[$d] "$android\mipmap-$d\ic_launcher_foreground.png"
    Render 'monochrome.svg' $adaptive $densities[$d] "$android\mipmap-$d\ic_launcher_monochrome.png"
}

# Windows: one .ico holding each size Windows asks for, every one a PNG, which Windows reads since Vista.
$sizes = 16, 24, 32, 48, 64, 128, 256
$images = foreach ($s in $sizes) {
    $png = Join-Path $work "ico-$s.png"
    Render 'moneybud.svg' '14 40 236 236' $s $png
    , [IO.File]::ReadAllBytes($png)
}
$ico = New-Object IO.MemoryStream
$w = New-Object IO.BinaryWriter $ico
$w.Write([UInt16]0); $w.Write([UInt16]1); $w.Write([UInt16]$sizes.Count)
$offset = 6 + 16 * $sizes.Count
for ($i = 0; $i -lt $sizes.Count; $i++) {
    $side = if ($sizes[$i] -ge 256) { 0 } else { $sizes[$i] }
    $w.Write([byte]$side); $w.Write([byte]$side); $w.Write([byte]0); $w.Write([byte]0)
    $w.Write([UInt16]1); $w.Write([UInt16]32)
    $w.Write([UInt32]$images[$i].Length); $w.Write([UInt32]$offset)
    $offset += $images[$i].Length
}
foreach ($bytes in $images) { $w.Write($bytes) }
$w.Flush()
[IO.File]::WriteAllBytes((Join-Path $here 'moneybud.ico'), $ico.ToArray())

Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue
Write-Host 'Icons made.'

<#
    Builds a release archive laid out the way Nexus and Vortex expect:

        BepInEx/plugins/Vampscape/Vampscape.dll

    Deliberately not the dev deploy path (plugins/MoonlightPeaksMods/Vampscape), which only
    exists to keep hand-built DLLs clear of Vortex during development.

    There is no test project to run. Every code path here reads Unity and game types - the
    decorate state machine, GameCamera's Cinemachine cameras, the decoratable area's confiner -
    so a console runner could not exercise anything meaningful. Verification is in TESTING.md
    instead.
#>

$ErrorActionPreference = 'Stop'

$modRoot  = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = $modRoot
$project  = Join-Path $modRoot 'src\Vampscape.csproj'

# Single source of truth for the version, so the archive can never disagree with the DLL.
$version = ([xml](Get-Content $project)).Project.PropertyGroup.Version | Where-Object { $_ }
if (-not $version) { throw "Could not read <Version> from $project" }

Write-Host "Packing Vampscape $version"

# SkipDeploy keeps a release build from overwriting the copy under test in the game folder.
dotnet build $project -c Release -p:SkipDeploy=true
if ($LASTEXITCODE -ne 0) { throw 'Build failed' }

$dll = Join-Path $modRoot 'src\bin\Release\netstandard2.1\Vampscape.dll'
if (-not (Test-Path $dll)) { throw "Built DLL not found at $dll" }

$staging = Join-Path $env:TEMP "Vampscape-pack-$([guid]::NewGuid().ToString('N'))"
$target  = Join-Path $staging 'BepInEx\plugins\Vampscape'
New-Item -ItemType Directory -Force -Path $target | Out-Null
Copy-Item $dll $target

$dist = Join-Path $repoRoot 'dist'
New-Item -ItemType Directory -Force -Path $dist | Out-Null

$archive = Join-Path $dist "Vampscape-$version.zip"
if (Test-Path $archive) { Remove-Item $archive }

Compress-Archive -Path (Join-Path $staging 'BepInEx') -DestinationPath $archive
Remove-Item $staging -Recurse -Force

Write-Host "Created $archive"

# Convenience for the author's machine only: the sibling mods all collect their archives in one
# shared dist/ two levels up. The guard is that the parent folder is literally named "mods" and a
# dist/ already exists beside it, neither of which is true for someone who clones this on its own.
$parent = Split-Path -Parent $modRoot
if ((Split-Path -Leaf $parent) -eq 'mods') {
    $sharedDist = Join-Path (Split-Path -Parent $parent) 'dist'

    if ((Test-Path $sharedDist) -and ((Resolve-Path $sharedDist).Path -ne (Resolve-Path $dist).Path)) {
        try {
            Copy-Item $archive $sharedDist -Force
            Write-Host "Also copied to $sharedDist"
        }
        catch {
            # A convenience copy failing must not fail the pack - the real archive already exists.
            Write-Warning "Could not copy to $sharedDist : $($_.Exception.Message)"
        }
    }
}

Write-Host 'Extract it over the game folder to install.'

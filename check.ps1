# Runs every gate in coding-guide/enforcement.md. -Fast skips formatting and tests.
param([switch]$Fast)
$ErrorActionPreference = 'Stop'
$solution = "$PSScriptRoot/Keymo.slnx"

function Step($name, [scriptblock]$run) {
    Write-Host "== $name"
    & $run
    if ($LASTEXITCODE) { throw "$name failed" }
}

Step 'limits' { & "$PSScriptRoot/coding-guide/scripts/limits.ps1" }
Step 'build' { dotnet build $solution -c Release --nologo -v q }
if ($Fast) { return }
Step 'format' { dotnet format $solution --verify-no-changes --no-restore }
Step 'test' { dotnet test $solution -c Release --no-build --nologo -v q }
Write-Host 'All gates passed.'

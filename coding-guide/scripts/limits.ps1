# Fails when a source file breaks the length limits in rules.md, unless listed in baseline/limits.txt.
# A listed file that no longer breaks them also fails, so the baseline can only shrink.
$maxFileLines = 300
$maxLineLength = 140
$root = Resolve-Path "$PSScriptRoot/../.."
$baseline = @(Get-Content "$PSScriptRoot/../baseline/limits.txt" -ErrorAction SilentlyContinue |
    Where-Object { $_ -and -not $_.StartsWith('#') })

$failures = @()
$files = Get-ChildItem -Path "$root/src", "$root/tests" -Recurse -Filter *.cs |
    Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' }
foreach ($file in $files) {
    $path = [IO.Path]::GetRelativePath($root, $file.FullName).Replace('\', '/')
    $lines = @(Get-Content $file.FullName)
    $longest = ($lines | Measure-Object -Property Length -Maximum).Maximum
    $violates = $lines.Count -gt $maxFileLines -or $longest -gt $maxLineLength
    if ($violates -and $baseline -notcontains $path) {
        $failures += "$path : $($lines.Count) lines, longest line $longest"
    }
    if (-not $violates -and $baseline -contains $path) {
        $failures += "$path : now within limits, remove it from baseline/limits.txt"
    }
}

if ($failures) {
    $failures | ForEach-Object { Write-Host "limits: $_" }
    exit 1
}
exit 0

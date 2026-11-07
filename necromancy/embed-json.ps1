param(
    [Parameter(Mandatory = $true)]
    [string] $Root
)

$embedPy = Join-Path $Root "python\embed_json.py"
if (-not (Test-Path $embedPy)) {
    Write-Error "embed_json.py not found: $embedPy"
    exit 1
}

$python = $null
foreach ($candidate in @("python", "python3")) {
    if (Get-Command $candidate -ErrorAction SilentlyContinue) {
        $python = $candidate
        break
    }
}
if (-not $python) {
    Write-Error "python / python3 not found."
    exit 1
}

& $python $embedPy $Root
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

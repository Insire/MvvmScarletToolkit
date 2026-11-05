# PostToolUse: formats the file Claude just edited.
#   *.xaml / *.axaml under src/ -> XamlStyler with Settings.XamlStyler (same scope as build.ps1)
#   *.cs                        -> dotnet format whitespace (.editorconfig)
# Never blocks: formatting problems are reported but exit code stays 0.
$ErrorActionPreference = 'Continue'
try
{
    $payload = [Console]::In.ReadToEnd() | ConvertFrom-Json
}
catch
{
    exit 0
}

$path = $payload.tool_input.file_path
if (-not $path -or -not (Test-Path -LiteralPath $path)) { exit 0 }

$root = $env:CLAUDE_PROJECT_DIR
if (-not $root) { $root = $payload.cwd }
$rootFull = [System.IO.Path]::GetFullPath($root).TrimEnd('\', '/')
$full = [System.IO.Path]::GetFullPath($path)
if (-not $full.StartsWith($rootFull, [System.StringComparison]::OrdinalIgnoreCase)) { exit 0 }

$relative = $full.Substring($rootFull.Length).TrimStart('\', '/') -replace '\\', '/'
if ($relative -match '(^|/)(bin|obj)/') { exit 0 }

Push-Location $rootFull
try
{
    if ($relative -match '^src/.*\.(xaml|axaml)$')
    {
        $output = & dotnet xstyler -f "$full" -c Settings.XamlStyler 2>&1
        if ($LASTEXITCODE -ne 0)
        {
            # tool not restored yet -> restore once and retry
            & dotnet tool restore *> $null
            $output = & dotnet xstyler -f "$full" -c Settings.XamlStyler 2>&1
        }
        if ($LASTEXITCODE -ne 0) { [Console]::Error.WriteLine("XamlStyler failed for ${relative}: $output") }
    }
    elseif ($relative -match '\.cs$')
    {
        $output = & dotnet format whitespace --folder --include "$relative" 2>&1
        if ($LASTEXITCODE -ne 0) { [Console]::Error.WriteLine("dotnet format failed for ${relative}: $output") }
    }
}
finally
{
    Pop-Location
}

exit 0

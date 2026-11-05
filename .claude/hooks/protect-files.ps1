# PreToolUse: blocks edits to build output, local IDE state and the version file.
# Exit code 2 blocks the tool call and shows stderr to Claude.
$ErrorActionPreference = 'Stop'
try
{
    $payload = [Console]::In.ReadToEnd() | ConvertFrom-Json
}
catch
{
    exit 0
}

$path = $payload.tool_input.file_path
if (-not $path) { $path = $payload.tool_input.notebook_path }
if (-not $path) { exit 0 }

$root = $env:CLAUDE_PROJECT_DIR
if (-not $root) { $root = $payload.cwd }

$full = [System.IO.Path]::GetFullPath($path)
$rootFull = [System.IO.Path]::GetFullPath($root).TrimEnd('\', '/')
if (-not $full.StartsWith($rootFull, [System.StringComparison]::OrdinalIgnoreCase)) { exit 0 }

$relative = $full.Substring($rootFull.Length).TrimStart('\', '/') -replace '\\', '/'

$rules = @(
    @{ Pattern = '(^|/)(bin|obj)/';           Reason = 'build output (bin/obj)' },
    @{ Pattern = '(^|/)TestResults/';         Reason = 'test results' },
    @{ Pattern = '^results/';                 Reason = 'build results' },
    @{ Pattern = '\.DotSettings\.user$';      Reason = 'local Rider/ReSharper settings' },
    @{ Pattern = '^version\.json$';           Reason = 'the Nerdbank.GitVersioning version file; bump versions manually' }
)

foreach ($rule in $rules)
{
    if ($relative -match $rule.Pattern)
    {
        [Console]::Error.WriteLine("Blocked: '$relative' is $($rule.Reason). Edit the source instead, or ask the user to change it.")
        exit 2
    }
}

exit 0

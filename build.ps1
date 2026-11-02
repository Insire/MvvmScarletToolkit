$env:VALIDATE_DETERMINISTIC_BUILDS = 'true'

dotnet tool restore
& dotnet xstyler -d .\src -r -c Settings.XamlStyler
& dotnet run --project cake/Build.csproj -- $args
exit $LASTEXITCODE;

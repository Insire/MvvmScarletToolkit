$env:VALIDATE_DETERMINISTIC_BUILDS = 'true'

dotnet tool restore
& dotnet xstyler -d .\src -r -c .XamlStyler
& dotnet run --project cake/Build.csproj -- $args
exit $LASTEXITCODE;

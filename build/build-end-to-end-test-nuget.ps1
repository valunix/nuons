$null = New-Item -ItemType Directory -Path "..\artifacts\local-nuget" -Force -ErrorAction SilentlyContinue

$version = "0.0.7"
$projectFile = "./../src/Nuons/Nuons.csproj"

dotnet clean $projectFile
dotnet build $projectFile -c Release "/p:Version=$version" "/p:AssemblyVersion=$version" "/p:FileVersion=$version"
dotnet pack $projectFile -c Release --no-build -o "..\artifacts\local-nuget" "/p:Version=$version" "/p:PackageVersion=$version" "/p:AssemblyVersion=$version" "/p:FileVersion=$version"

Write-Host "NuGet package created in ..\artifacts\local-nuget directory with version $version" -ForegroundColor Green
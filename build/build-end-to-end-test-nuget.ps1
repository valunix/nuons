$null = New-Item -ItemType Directory -Path "..\artifacts\local-nuget" -Force -ErrorAction SilentlyContinue

$version = "0.0.7"
$nuonProjectFile = "./../src/Nuons/Nuons.csproj"

dotnet clean $nuonProjectFile
dotnet build $nuonProjectFile -c Release "/p:Version=$version" "/p:AssemblyVersion=$version" "/p:FileVersion=$version"
dotnet pack $nuonProjectFile -c Release --no-build -o "..\artifacts\local-nuget" "/p:Version=$version" "/p:PackageVersion=$version" "/p:AssemblyVersion=$version" "/p:FileVersion=$version"

Write-Host "NuGet package for $nuonProjectFile created in ..\artifacts\local-nuget directory with version $version" -ForegroundColor Green


$nuonsLibraryProjectFile = "./../src/Nuons.Library/Nuons.Library.csproj"

dotnet clean $nuonsLibraryProjectFile
dotnet build $nuonsLibraryProjectFile -c Release "/p:Version=$version" "/p:AssemblyVersion=$version" "/p:FileVersion=$version"
dotnet pack $nuonsLibraryProjectFile -c Release --no-build -o "..\artifacts\local-nuget" "/p:Version=$version" "/p:PackageVersion=$version" "/p:AssemblyVersion=$version" "/p:FileVersion=$version"

Write-Host "NuGet package for $nuonsLibraryProjectFile created in ..\artifacts\local-nuget directory with version $version" -ForegroundColor Green

$nuonsStartupProjectFile = "./../src/Nuons.Startup/Nuons.Startup.csproj"

dotnet clean $nuonsStartupProjectFile
dotnet build $nuonsStartupProjectFile -c Release "/p:Version=$version" "/p:AssemblyVersion=$version" "/p:FileVersion=$version"
dotnet pack $nuonsStartupProjectFile -c Release --no-build -o "..\artifacts\local-nuget" "/p:Version=$version" "/p:PackageVersion=$version" "/p:AssemblyVersion=$version" "/p:FileVersion=$version"

Write-Host "NuGet package for $nuonsStartupProjectFile created in ..\artifacts\local-nuget directory with version $version" -ForegroundColor Green
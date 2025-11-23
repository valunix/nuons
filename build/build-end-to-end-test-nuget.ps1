$null = New-Item -ItemType Directory -Path "..\artifacts\local-nuget" -Force -ErrorAction SilentlyContinue

$version = "0.0.7"
$nuonProjectFile = "./../src/Nuons/Nuons.csproj"

dotnet clean $nuonProjectFile
dotnet build $nuonProjectFile -c Release "/p:Version=$version" "/p:AssemblyVersion=$version" "/p:FileVersion=$version"
dotnet pack $nuonProjectFile -c Release --no-build -o "..\artifacts\local-nuget" "/p:Version=$version" "/p:PackageVersion=$version" "/p:AssemblyVersion=$version" "/p:FileVersion=$version"

Write-Host "NuGet package for $nuonProjectFile created in ..\artifacts\local-nuget directory with version $version" -ForegroundColor Green


$nuonsAbstractionsProjectFile = "./../src/Nuons.Abstractions/Nuons.Abstractions.csproj"

dotnet clean $nuonsAbstractionsProjectFile
dotnet build $nuonsAbstractionsProjectFile -c Release "/p:Version=$version" "/p:AssemblyVersion=$version" "/p:FileVersion=$version"
dotnet pack $nuonsAbstractionsProjectFile -c Release --no-build -o "..\artifacts\local-nuget" "/p:Version=$version" "/p:PackageVersion=$version" "/p:AssemblyVersion=$version" "/p:FileVersion=$version"

Write-Host "NuGet package for $nuonsAbstractionsProjectFile created in ..\artifacts\local-nuget directory with version $version" -ForegroundColor Green

$nuonsTempProjectFile = "./../src/Nuons.Temp/Nuons.Temp.csproj"

dotnet clean $nuonsTempProjectFile
dotnet build $nuonsTempProjectFile -c Release "/p:Version=$version" "/p:AssemblyVersion=$version" "/p:FileVersion=$version"
dotnet pack $nuonsTempProjectFile -c Release --no-build -o "..\artifacts\local-nuget" "/p:Version=$version" "/p:PackageVersion=$version" "/p:AssemblyVersion=$version" "/p:FileVersion=$version"

Write-Host "NuGet package for $nuonsTempProjectFile created in ..\artifacts\local-nuget directory with version $version" -ForegroundColor Green
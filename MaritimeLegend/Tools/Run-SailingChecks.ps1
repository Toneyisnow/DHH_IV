$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$checkDir=Join-Path $project 'Temp/WorldSailingChecks'
New-Item -ItemType Directory -Force -Path $checkDir | Out-Null
$assets=[System.Security.SecurityElement]::Escape((Join-Path $project 'Assets'))
$adapter=[System.Security.SecurityElement]::Escape((Join-Path $PSScriptRoot 'ManualSailingChecks/Program.cs'))
$csproj=@"
<Project Sdk="Microsoft.NET.Sdk">
 <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><DefineConstants>MANUAL_SAILING_CHECKS</DefineConstants><EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup>
 <ItemGroup>
  <Compile Include="$assets/Core/**/*.cs" Exclude="$assets/Core/Simulation/WorldSession.cs" />
  <Compile Include="$assets/Sailing/CoastNavigation.cs" />
  <Compile Include="$assets/Sailing/StreamedSeaNavigation.cs" />
  <Compile Include="$assets/Sailing/SeaFleetFollowing.cs" />
  <Compile Include="$assets/Tests/EditMode/Editor/ManualSailingChecks.cs" />
  <Compile Include="$assets/Tests/EditMode/Editor/SailingArchitectureChecks.cs" />
  <Compile Include="$assets/Tests/EditMode/Editor/WorldSailingChecks.cs" />
  <Compile Include="$adapter" />
 </ItemGroup>
</Project>
"@
$file=Join-Path $checkDir 'Checks.csproj'
[IO.File]::WriteAllText($file,$csproj)
Push-Location $project
try { dotnet run --project $file; if($LASTEXITCODE -ne 0){throw 'Sailing checks failed'} } finally { Pop-Location }

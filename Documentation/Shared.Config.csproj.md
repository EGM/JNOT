# Shared.Config.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
	  <TargetFrameworks>net10.0;net48</TargetFrameworks>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AssemblyName>Jnot.$(MSBuildProjectName)</AssemblyName>
    <RootNamespace>Jnot.$(MSBuildProjectName.Replace(" ", "_"))</RootNamespace>
	<LangVersion>10.0</LangVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Tomlyn.Signed" Version="0.20.0" />
  </ItemGroup>

</Project>

```

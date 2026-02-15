# Shared.Info.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFrameworks>net10.0-windows;net48</TargetFrameworks>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AssemblyName>Jnot.$(MSBuildProjectName)</AssemblyName>
    <RootNamespace>Jnot.$(MSBuildProjectName.Replace(" ", "_"))</RootNamespace>
	<LangVersion>10.0</LangVersion>
	<OutputType>Library</OutputType>
	<UseWindowsForms>True</UseWindowsForms>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Shared.UI.Controls\Shared.UI.Controls.csproj" />
    <ProjectReference Include="..\Shared.UI\Shared.UI.csproj" />
  </ItemGroup>

</Project>

```

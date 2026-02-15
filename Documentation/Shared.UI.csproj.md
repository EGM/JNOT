# Shared.UI.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFrameworks>net10.0-windows;net48</TargetFrameworks>
    <Nullable>enable</Nullable>
    <UseWindowsForms>true</UseWindowsForms>
    <ImplicitUsings>enable</ImplicitUsings>
    <AssemblyName>Jnot.$(MSBuildProjectName)</AssemblyName>
    <RootNamespace>Jnot.$(MSBuildProjectName.Replace(" ", "_"))</RootNamespace>
	<LangVersion>10.0</LangVersion>
	<OutputType>Library</OutputType>
  </PropertyGroup>

</Project>

```

# CURRENT POSITION

**Project:** JNOT Infrastructure

**Last Worked On:** 2026‑02‑03

**State:** Mid‑implementation structure, folders, structure docs, tool support

**Next Action:** Create DENO/TS tools to help manage.

**Blocked By:** None

**Notes:** None

## ACTIVE THREADS

### 1. Infrastructure

- New file structure
- New class signatures

### 3. Documentation

- README done, but still needs another review

## SCRATCHPAD

- Need to expand testing.
- generateomd instructions:

```text
*********************** Object Model Generator ***********************

Usage:
 --source=[source folder] --compareSource=[oldSourceFolder] --preprocessors=[defines] --output=[out location] --format=[html,md] --filter=[regex] --showPrivate --showInternal

Required parameters (one or more):
  source               Specifies the folder of source files to include for the object model.
                       Separate with ; for multiple folders
  assemblies           Specifies a set of assemblies to include for the object model.
                       Separate with ; for multiple assemblies

Optional parameters:
  compareSource        Specifies a folder to compare source and generate a diff model
                       This can be useful for finding API changes or compare branches
  compareAssemblies    Specifies a set of assemblies to include to generate a adiff model.
                       Separate with ; for multiple assemblies
  output        Output location
  preprocessors        Define a set of preprocessors values. Use ; to separate multiple
  exclude              Defines one or more strings that can't be part of the path Ie '/Samples/;/UnitTests/'
                       (use forward slash for folder separators)
  regexfilter          Defines a regular expression for filtering on full file names in the source
  referenceAssemblies  Specifies a set of assemblies to include for references for better type resolution.
  showPrivate          Show private members (default is false)
  showInternal         Show internal members (default is false)
  filter               A set of namespaces or classes to ignore. For example: -filter=Microsoft.CSharp;Microsoft.VisualBasic
Using Nuget comparison:  nuget                nuget packages to generate OMD for (separate multiple with semicolon). Example: /nuget=Newtonsoft.Json:13.0.0
  compareNuget         nuget packages to compare versions with (separate multiple with semicolon). Example: /nuget=Newtonsoft.Json:12.0.0
  tfm                  Target Framework to use against NuGet package. Example: /tfm=net8.0-windows10.0.19041.0
  ```

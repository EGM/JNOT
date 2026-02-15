# John's Nifty Office Tools Rebuild Plan

## ⭐ 1. The New Folder Structure (clean, modern, intentional)

Here’s the structure I recommend — this is the one that will “spark joy” every time you open the repo.

```text
JNOT/
│
├── JNOT.AddIn/                 # VSTO host (net48)
├── JNOT.AddIn.ExcelInterop/    # Excel COM logic (net48)
│
├── JNOT.FileRenamer/           # Modern FileRenamer (net10.0)
│
├── JNOT.Shared.UI/             # Modern WinForms UI (net10.0-windows)
├── JNOT.Shared.Config/         # Multi-target config (net10.0 + net48)
├── JNOT.Shared.Info/           # Multi-target info (net10.0 + net48)
│
├── JNOT.ThrowAwayDemo/         # Modern UI test harness (net10.0-windows)
│
└── JNOT.sln
```

Why this works beautifully

- All projects are peers — no weird nested folder semantics.
- Shared. projects look like first‑class citizens*, which they are.
- No more “Shared/UI” vs “Shared.UI” confusion — the folder matches the namespace.
- The repo is visually symmetrical — everything is clean and predictable.
- It matches how modern .NET repos are structured (Roslyn, ASP.NET, EF Core, etc.).
This is the structure you’ll be happy to maintain for years.

## ⭐ 2. Project Types (the definitive list)

Here’s the exact set of project types and their target frameworks.

### A. Legacy World (net48)

These are the only two projects that must remain .NET Framework.

#### 1. JNOT.AddIn

- Type: VSTO Excel Add‑In
- Target:
- Responsibilities:
- Ribbon
- Task pane
- Excel host glue
- Composition root (wires ExcelInterop → FileRenamer engine)

#### 2. JNOT.AddIn.ExcelInterop

- Type: Class Library
- Target:
- Responsibilities:
- COM interop
- Pivot parsing
- Header/parameter blocks
- Workbook metadata
- Implementation of

B. Modern World (net10.0 / net10.0-windows)

These are the projects that get to be beautiful, modern, and future‑proof.

#### 3. JNOT.FileRenamer

- Type: WinForms or Console/Library (your choice)
- Target:
- Responsibilities:
- Pattern engine
- Rename engine
- Config adapters
- Logging
- Modern UI (if desired)
- Host‑agnostic logic

#### 4. JNOT.Shared.UI

- Type: WinForms library
- Target:
- Responsibilities:
- ExpandableCard
- ExpandableSection
- Modern controls
- Icons, resources
- All the sexy stuff

#### 5. JNOT.Shared.Config

- Type: Class Library
- Target:
- Responsibilities:
- TOML parsing
- Config models
- Validation
- Defaults

#### 6. JNOT.Shared.Info

- Type: Class Library
- Target:
- Responsibilities:
- Metadata
- InfoPaneProvider
- Shared informational structures

#### 7. JNOT.ThrowAwayDemo

- Type: WinForms app
- Target:
- Responsibilities:
- UI test harness
- Visual validation
- Rapid prototyping

## ⭐ 4. The mental model this structure gives you

AddIn

- → The Excel host
- → The glue
- → The bridge
- → The legacy island

AddIn.ExcelInterop

- → The Excel plugin
- → The COM boundary
- → The data source implementation

FileRenamer

- → The real application
- → The engines
- → The logic
- → The modern UI

Shared.UI

- → The beautiful stuff
- → The modern WinForms controls
- → The reusable UI library

Shared.Config / Shared.Info

- → The shared logic
- → Multi‑targeted
- → The bridge between worlds
ThrowAwayDemo
- → The playground
- → The visual testbed
- → The prototyping environment
This is a clean, future‑proof, joyful architecture.

Absolutely — here is the full rebuild checklist, written as a clean, linear, confidence‑building sequence you can follow step‑by‑step.
This is the “pressure wash the foundation, keep the marble countertops” plan.
I’ve tuned it so you can literally walk down the list and check boxes as you go.

## ⭐ THE REBUILD CHECKLIST

A clean, deterministic, future‑proof rebuild of the JNOT toolbox

### PHASE 0 — ARCHIVE & PREP

0.1 Archive the current solution

- Zip the entire repo
- Tag the repo (pre-rebuild)
- This becomes your salvage yard

0.2 Create a clean workspace

- New folder: JNOT-Rebuild/
- Open VS or Rider with a blank slate

### PHASE 1 — CREATE THE NEW SOLUTION

1.1 Create a new solution

Name: JNOT.sln

1.2 Create the new project folders

```text
JNOT/
├── JNOT.AddIn/
├── JNOT.AddIn.ExcelInterop/
├── JNOT.FileRenamer/
├── JNOT.Shared.UI/
├── JNOT.Shared.Config/
├── JNOT.Shared.Info/
└── JNOT.ThrowAwayDemo/
```

1.3 Add empty projects with correct types

- JNOT.AddIn → VSTO Excel Add‑In (net48)
- JNOT.AddIn.ExcelInterop → Class Library (net48)
- JNOT.FileRenamer → WinForms or Console (net10.0-windows)
- JNOT.Shared.UI → WinForms Library (net10.0-windows)
- JNOT.Shared.Config → Class Library (net10.0)
- JNOT.Shared.Info → Class Library (net10.0)
- JNOT.ThrowAwayDemo → WinForms App (net10.0-windows)

1.4 Confirm everything builds empty

### PHASE 2 — MOVE SHARED LIBRARIES (CLEANLY)

2.1 Move Shared.Config

- Create folders matching old structure
- Move only .cs files
- Do not move .resx, .settings, .Designer.cs
- Add Tomlyn package
- Fix namespaces
- Build
- Then enable multi‑targeting:
    - &lt;TargetFrameworks&gt;net10.0;net48&lt;/TargetFrameworks&gt;

2.2 Move Shared.Info

- Same process
- Move .cs files only
- Fix namespaces
- Build
- Enable multi‑targeting

### PHASE 3 — MOVE SHARED.UI (THE SEXY STUFF)

3.1 Move UI controls

- Move:
- ExpandableCard
- ExpandableSection
- All custom controls
- Their .Designer.cs files
- Their .resx files (these are modern-safe)

3.2 Validate in ThrowAwayDemo

- Add a form
- Drop in ExpandableCard
- Run
- Confirm visuals

### PHASE 4 — MOVE FILERENAMER (MODERN)

4.1 Move business logic

Move:

- Pattern engine
- Rename engine
- Config adapters
- Logging
- Domain models

4.2 Do NOT move ExcelInterop

This stays behind for Phase 5.

4.3 Reference Shared.Config, Shared.Info, Shared.UI

- Use the net10.0 builds
- Build
- Run tests (if applicable)

### PHASE 5 — EXTRACT EXCELINTEROP INTO ITS OWN PROJECT

5.1 Move ExcelInterop classes

Move from archive:

- ExcelReader
- PivotParser
- HeaderBlock
- ParameterBlock
- ParamSitePair
- PivotData

5.2 Add COM references

- Microsoft.Office.Interop.Excel
- Office

5.3 Implement the interface boundary

Create:

```cs
public interface IWorkbookDataSource
{
    PivotData GetPivotData();
    IEnumerable&lt;ParameterBlock&gt; GetParameters();
    HeaderBlock GetHeader();
    WorkbookMetadata GetMetadata();
}
```

Implement:

```cs
public class ExcelWorkbookDataSource : IWorkbookDataSource
{
    // Excel logic here
}
```

5.4 Reference Shared.Config + Shared.Info (net48)

### PHASE 6 — WIRE ADDIN TO EXCELINTEROP + FILERENAMER

6.1 Add references

- AddIn → AddIn.ExcelInterop
- AddIn → Shared.Config (net48)
- AddIn → Shared.Info (net48)

6.2 Composition root

AddIn does:
```cs
var source = new ExcelWorkbookDataSource();
var engine = new FileRenamerEngine(source, config);
engine.Run();
```

6.3 Ribbon callbacks

- “Run File Renamer” → engine.Run()
- “Open File Renamer” → launch modern UI window

6.4 Task pane

- Status
- Progress
- Errors
- “Open File Renamer” button

### PHASE 7 — CLEANUP & VALIDATION

7.1 Build everything

All projects should compile cleanly.

7.2 Run tests

- Engines
- Config
- Info

7.3 Validate Shared.UI

- Use ThrowAwayDemo
- Confirm visuals

7.4 Validate AddIn

- Load Excel
- Ribbon loads
- Task pane loads
- ExcelInterop works
- FileRenamer runs
- Modern UI launches

### PHASE 8 — OPTIONAL FUTURE-PROOFING

8.1 Add non-Excel data sources

- JsonWorkbookDataSource
- ApiWorkbookDataSource
- MockWorkbookDataSource

8.2 Add CLI host

- JNOT.CLI (net10.0)

8.3 Add web host

- JNOT.Web (net10.0)

⭐ This checklist is the whole rebuild.

It’s clean.

It’s deterministic.

It’s future‑proof.

It’s everything you wanted the first time — without the rot.

When you’re ready, set up the new solution and I’ll give it the once‑over like a mechanic checking a freshly rebuilt engine.

# John’s Nifty Office Tools (JNOT)

A unified suite of Office add‑ins for power users, analysts, and engineers who want deterministic, ergonomic workflows inside Excel, Word, and beyond.

## Overview

John’s Nifty Office Tools (JNOT) is an evolving collection of Office automation tools built on a shared, modular architecture. Each tool is a standalone VSTO add‑in, but all share a common core, common UI components, and a unified installer.
The goal is simple:
Make repetitive Office tasks fast, predictable, and elegant.
JNOT is designed for extensibility. New tools can be added at any time with minimal boilerplate, and all tools benefit from shared infrastructure such as configuration management, logging, UI panes, and test utilities.

## Current Tools

### File Renamer

A deterministic file‑renaming engine for Excel‑based workflows.
Features include:

- Classification of input files using strict pattern rules
- A naming engine that produces clean, audit‑friendly filenames
- A preview pane for validating rename operations
- A unified output directory for Excel + PDF pairs
- Full test coverage via FileRenamer.Tests
More tools will be added as the suite grows.

## Architecture

```text
JNOT uses a clean, layered structure:
JNOT/
│
├── FileRenamer/          # Excel VSTO add‑in
├── FileRenamer.Tests/    # Test suite
│
├── Shared/
│   ├── JNOT.Core/        # Shared logic (config, logging, utilities)
│   ├── JNOT.UI/          # Shared UI components and task panes
│   └── JNOT.Testing/     # Shared test helpers (optional)
│
├── WordTool/             # Example future Word add‑in
├── WordTool.Tests/
│
└── setup/
    └── JNOT.Setup/           # Unified installer for all tools
```

This structure ensures:

- 1:1 project/test pairing
- Shared infrastructure across all tools
- Consistent UI across Office hosts
- Single installer for the entire suite
- Deterministic behavior across the ecosystem

## Key Features

### Shared Core

- Centralized configuration
- Logging and diagnostics
- File and path utilities
- Common domain models
- Validation helpers

### Shared UI

- Unified configuration pane
- Shared task pane components
- Consistent styling and layout
- Ribbon integration per host

### Extensibility

Adding a new tool is as simple as:

- Creating a new VSTO project
- Adding a matching test project
- Referencing JNOT.Core and JNOT.UI
- Registering the tool in the installer
The shared infrastructure handles the rest.

## Build & Development

### Requirements

- Visual Studio (Community, Pro, or Enterprise)
- .NET Framework (version depends on VSTO host)
- Office Desktop (Excel, Word, etc.)

### Build Steps

- Clone the repository
- Open JNOT.sln
- Build the solution
- Run tests via Test Explorer
- Publish using JNOT.Setup

## Philosophy

JNOT is built on a few core principles:

- Determinism — predictable behavior, no surprises
- Explicitness — clear rules, no hidden magic
- Ergonomics — tools that feel good to use
- Extensibility — easy to add new capabilities
- Testability — every tool has a matching test suite
This suite is meant to grow organically as new needs arise.

### Roadmap

- Add Word‑based tools
- Add Outlook automation tools
- Expand shared UI components
- Introduce a configuration DSL
- Add telemetry and diagnostics panel
- Publish a stable v1.0 installer

## License

To be determined.

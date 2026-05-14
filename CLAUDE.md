# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

VPPropertyValidator is a **M-Files Vault Application Framework (VAF)** extension written in C# (.NET Framework 4.7.2). It intercepts document check-in events and validates property values against configurable regex patterns, blocking the check-in if validation fails.

## Build & Deploy

**Build (CLI):**
```powershell
dotnet build VPPropertyValidator.sln
# or
msbuild VPPropertyValidator.sln /p:Configuration=Release
```

Build configurations: `Debug|AnyCPU`, `Release|AnyCPU`, `DebugWithoutDeployment|AnyCPU`.

Post-build, MSBuild zips the output into a `.mfappx` package (the M-Files application format).

**Deploy to M-Files vault:**
```powershell
.\install-application.ps1
```

The script connects to the local M-Files server (localhost:2266, ncacn_ip_tcp) and installs the `.mfappx` into the configured vault. Override credentials/vault by creating `install-application.user.json`.

There is no automated test suite. Testing is done manually via the M-Files vault UI after deployment.

## Architecture

The application has two source files:

- **`VaultApplication.cs`** — Extends `ConfigurableVaultApplicationBase<Configuration>`. Registers a `MFEventHandlerBeforeCheckInChangesFinalize` event handler that runs `ValidateProperties()` on every document check-in.
- **`Configuration.cs`** — Defines the `Configuration` class (holds `List<ValidationRule>`) and `ValidationRule` class. Rules are edited through the M-Files Admin UI without code changes.

**Validation flow:**
1. Check-in event fires → `ValidateProperties()` is called.
2. For each `ValidationRule`: if `TargetClasses` is populated, retrieve the document's class ID once and skip non-matching classes.
3. Read the target property value and match against `RegexPattern` (always `RegexOptions.IgnoreCase`).
4. Empty patterns are treated as passing. Malformed regex patterns are caught, logged to the Windows Event Log via M-Files logging, and the rule is skipped (fail-open).
5. If any rules fail, a formatted exception is thrown with all `ValidationMessage` strings concatenated, which M-Files surfaces to the user and blocks the check-in.

## Key Dependencies

- **MFiles.VAF** v25.3.727.1 — Core Vault Application Framework
- **MFiles.VAF.Extensions** v24.12.75 — Extended base classes and utilities
- Requires M-Files server version ≥ 23.5.0.0
- Application GUID: `38295ee1-c4ea-4e4f-8fda-e0e464607208` (in `appdef.xml`)
- Marked `multi-server-compatible=true`

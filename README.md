# CRN Typer for Windows

A small Windows app that types CRNs into Banner using keyboard shortcuts.

**[Download CRN Typer for Windows](https://github.com/mechboi09/crn-typer-windows/raw/refs/heads/main/download/CRN-Typer-Windows.zip)**

## Version 2: saved options and a cleaner editor

- Four named tabs with the same F1–F4 shortcuts.
- Three CRN rows to start; **+ Add CRN** and **Remove** adjust the list.
- **Paste list** accepts spaces, commas, tabs or newlines and checks duplicates and invalid CRNs before replacing the option.
- CRNs, option names, visible rows and speed save locally after a short pause in editing and on quit. They restore when you reopen the app.
- Settings are written atomically; no account, server or GitHub synchronization is used.
- Normal now requests **200 ms**. Fast retains 1 ms requested pauses; actual timing depends on the computer and browser.
- Status reports keys sent; it cannot confirm course acceptance.

## Get started

1. Download and extract the ZIP.
2. Open **CRN-Typer-AUB.exe**.
3. Fill one option with up to six CRNs and select **Normal** or **Fast**.
4. Click the first empty CRN box, then press **F1–F4** for your chosen option.
5. Review the result in Summary.

Start with **one empty CRN field**. The app finishes at **Add to Summary**; it does not press the separate registration **Submit** button.

## Modes

| Mode | Behavior |
| --- | --- |
| **Normal** | Waits 200 ms for Banner to create and focus the next field. |
| **Fast** | Activates Add Another CRN itself, with 1 ms requested pauses. Actual timing depends on Windows and the browser. |

Fast mode may outrun a busy browser. Test on a practice form first. Selecting Fast reveals a small Easter egg.

## Stay in control

- **Escape** cancels an active run; switching windows also stops it.
- Clear partially entered text before trying again.
- CRNs stay in memory until the app closes. No network calls, credentials, or saved CRN lists.

This unofficial utility cannot inspect the active field or confirm whether a class was accepted. The EXE uses Windows' built-in .NET Framework.

## Source

The application source is in [src/CRNTyper.cs](src/CRNTyper.cs). Normal and Fast sequence checks passed; live registration behavior needs user testing.

**Using a Mac?** [Get CRN Typer for Mac](https://github.com/mechboi09/crn-typer-mac).

## Local storage

Settings: `%APPDATA%\CRNTyper-AUB\settings.xml` (with a `.bak` backup of the previous save). Clearing or moving this file resets your settings. Keep the file if you reinstall or replace the executable. Files are local and unencrypted.

## Separate LAU version

[Download LAU for Windows](https://github.com/mechboi09/crn-typer-windows/raw/refs/heads/main/lau/CRN-Typer-LAU-Windows.zip) · [LAU instructions](lau/README.md)

LAU is a separate EXE with its own settings folder, up to ten CRNs, Normal (200 ms), Fast and Instant. It sends Tab only between CRNs and ends with Enter, which may submit immediately. Close other typers to avoid F1–F4 conflicts. Instant cannot retract already queued events.

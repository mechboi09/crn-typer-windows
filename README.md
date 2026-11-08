# CRN Typer for Windows

A small Windows app that types CRNs into Banner using keyboard shortcuts.

**[Download CRN Typer for Windows](https://github.com/mechboi09/crn-typer-windows/raw/refs/heads/main/download/CRN-Typer-Windows.zip)**

## Get started

1. Download and extract the ZIP.
2. Open **CRN-Typer-Combined.exe**.
3. Fill one option with up to six CRNs and select **Normal** or **Fast**.
4. Click the first empty CRN box, then press **F1–F4** for your chosen option.
5. Review the result in Summary.

Start with **one empty CRN field**. The app finishes at **Add to Summary**; it does not press the separate registration **Submit** button.

## Modes

| Mode | Behavior |
| --- | --- |
| **Normal** | Waits 300 ms for Banner to create and focus the next field. |
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

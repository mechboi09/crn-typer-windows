# CRN Typer for LAU — Windows

[Download standalone LAU app](https://github.com/mechboi09/crn-typer-windows/raw/refs/heads/main/lau/CRN-Typer-LAU-Windows.zip)

## Version 2: saved options and a cleaner editor

- Four named tabs with the same F1–F4 shortcuts.
- Three CRN rows to start; **+ Add CRN** and **Remove** adjust the list.
- **Paste list** accepts spaces, commas, tabs or newlines and checks duplicates and invalid CRNs before replacing the option.
- CRNs, option names, visible rows and speed save locally after a short pause in editing and on quit. They restore when you reopen the app.
- Settings are written atomically; no account, server or GitHub synchronization is used.
- Normal now requests **200 ms**. Fast retains 1 ms requested pauses; actual timing depends on the computer and browser.
- Status reports keys sent; it cannot confirm course acceptance.

Ten CRNs per option. Normal 200 ms, Fast 1 ms requested, Instant single-batch mode. Sequence: CRN, Tab, CRN ... final Enter only. Enter may submit immediately. Review numbers before running. Escape cannot recall Instant events already queued.

Settings: `%APPDATA%\CRNTyper-LAU\settings.xml`. This is a separate executable and storage location from AUB. Close other typers before launching to free F1–F4. No live registration test has been performed.

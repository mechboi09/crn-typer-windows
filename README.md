# CRN typer for Windows

**[Download CRN typer.exe](https://github.com/mechboi09/crn-typer-windows/raw/refs/heads/main/download/CRN%20typer.exe)**

One standalone EXE. No ZIP, installer or bundled README. These are the instructions.

## Use

1. Close any older CRN typer apps to free the F1–F4 shortcuts.
2. Open **CRN typer.exe** and enter up to **seven CRNs per option**.
3. Click the first empty CRN box on the practice/registration form.
4. Press **F1**, **F2**, **F3** or **F4** for the matching column, then release the key.
5. Review the result in Summary. The app ends at **Add to Summary**; it does not intentionally activate the separate Submit button.

## Editing

- Four compact columns; digits only, with leading zeros retained.
- Paste a list directly into any box. Spaces, commas, tabs and newlines separate CRNs. Pasting fills that box and those below it, replacing the remaining entries below. Invalid or duplicate pasted values are rejected.
- **Ctrl+A once** selects the current CRN. **Twice within 0.7 seconds** selects the numbers in that column.
- Copy, Cut, Paste and Delete/Backspace apply to a selected column. Clearing it returns the cursor to the first box. The other columns remain unchanged.
- CRNs are kept in memory only. **No autosave or settings files.** Closing the app discards the entries.

## Speed

The small Normal/Fast switch starts in **Fast**. The inactive label is gray, with a black switch and outlined white knob. **Tips** opens the in-app notes.

| Mode | Sequence between CRNs |
| --- | --- |
| Normal | Tab, then a requested **200 ms** pause, relying on the page to create/focus the next box. |
| Fast | Tab + Enter to activate Add Another CRN, then a requested **1 ms** pause. |

Both finish with **Tab, Tab, Enter**. Actual timing depends on Windows and the browser.

Escape cancels. Switching windows or holding modifier keys stops a run. Changes of tab or focused field inside the same browser window are not detected. The app sends keys; it cannot verify the active field, page acceptance or registration success. Test on an offline practice form first. Live registration has not been verified.

## Source and checks

Source: [src/CRNTyper.cs](src/CRNTyper.cs). Windows' built-in .NET Framework is used; no additional packages are bundled. Tests cover typing sequences, speed switching, numeric input, paste parsing, column selection/deletion and first-box focus.

[Mac version](https://github.com/mechboi09/crn-typer-mac) · [Separate LAU version](lau/README.md)
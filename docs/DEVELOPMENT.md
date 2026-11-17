# Developer notes

The C# Windows Forms application uses Windows' built-in .NET Framework and user32 SendInput/RegisterHotKey. There are no third-party runtime packages or saved settings files.

Source: [CRNTyper.cs](../src/CRNTyper.cs).
Tests: [Tests.cs](../src/Tests.cs) and [SelectionTests.cs](../src/SelectionTests.cs).

Compile the app as a Windows executable with references to System.Windows.Forms, System.Drawing and System.Core. Compile each test separately with the source and its appropriate entry point: Tests or SelectionTests. The tests do not send keyboard input.

Checks cover sequence sizes and modes, 200 ms Normal timing, paste parsing, digit filtering, leading zeros, the speed switch, column selection/deletion and first-box focus.

Normal sends Tab between CRNs and relies on the page to create/focus the next field. Fast sends Tab + Enter between CRNs. Both end with two grouped Tabs and Enter. Timing values are programmed pauses; actual delivery depends on OS scheduling and the browser.

The app cannot inspect the browser's focused field or confirm acceptance of a class. Changes of browser tab or focused field are not detected by the active-window check.

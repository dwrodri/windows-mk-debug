# windows-mk-debug

A Windows EXE application for debugging mouse and keyboard automation.

## Description

This .NET 6.0 Windows Forms application provides real-time tracking of mouse movements and keyboard events, useful for debugging automation scripts and understanding user input patterns.

## Building and Releases

### Automatic Builds

This project uses GitHub Actions for continuous integration and automated releases:

- **CI Build**: Runs on every push to main/master/develop branches and pull requests
- **Release Build**: Automatically triggered when you create and push a version tag

### Creating a Release

To create a new release with an automatically built EXE:

1. Commit and push your changes to the main branch
2. Create and push a version tag:
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```
3. GitHub Actions will automatically:
   - Build the application as a self-contained Windows x64 executable
   - Create a ZIP archive containing the EXE and dependencies
   - Create a GitHub release with the archive attached
   - Generate release notes from your recent commits

### Manual Build

To build locally:
```bash
dotnet restore MouseKeyboardTracker.csproj
dotnet build MouseKeyboardTracker.csproj --configuration Release
dotnet publish MouseKeyboardTracker.csproj --configuration Release --runtime win-x64 --self-contained true
```

## Features

- **Real-time Mouse Position Tracking**: Displays current X,Y coordinates as you move the mouse anywhere on screen
- **Global Keyboard Event Logging**: Records all key press/release events with precise timestamps
- **System-wide Monitoring**: Captures input events even when the application doesn't have focus
- **Modifier Key Detection**: Shows key combinations like Ctrl+C, Alt+Tab, Shift+A, Win+R
- **Toggle Control**: Enable/disable input monitoring with a simple checkbox
- **Clear Log Function**: Reset the keystroke log at any time
- **Auto-scrolling Log**: Automatically scrolls to show the latest keyboard events

## Requirements

- .NET 6.0 Runtime (for framework-dependent builds)
- Windows OS (uses Windows-specific APIs for global hooks)
- Administrator privileges may be required for global hook functionality

## Usage

### Starting the Application

1. Download the latest release from the [Releases page](https://github.com/dwrodri/windows-mk-debug/releases)
2. Extract the ZIP file to a folder of your choice
3. Run `MouseKeyboardTracker.exe`
4. Grant any permissions if prompted by Windows security

The application window will open with tracking enabled by default.

### Interface Overview

**Mouse Position Display**
- Located at the top of the window in blue text
- Shows real-time coordinates: `Mouse Position: (1234, 567)`
- Updates continuously as you move the mouse anywhere on screen

**Enable Tracking Checkbox**
- Controls global input monitoring
- ✅ **Checked**: Captures mouse and keyboard events system-wide
- ❌ **Unchecked**: Stops all input monitoring to pause tracking

**Clear Log Button**
- Clears the entire keystroke log display
- Useful for starting fresh monitoring sessions
- Does not affect mouse position tracking

**Keystroke Log Area**
- Large scrollable text area showing all captured keyboard events
- Automatically scrolls to display the most recent events
- Uses monospace font for consistent formatting

### Understanding the Log Output

Each keystroke event is logged with the following format:
```
[HH:mm:ss.fff] EVENT_TYPE: Key_Combination
```

**Event Types:**
- `DOWN`: Key press event (when you press a key down)
- `UP`: Key release event (when you let go of a key)

**Key Examples:**
- Individual keys: `A`, `Space`, `Enter`, `Escape`, `F1`, `Delete`
- Number keys: `D1`, `D2`, `D3` (for 1, 2, 3 on main keyboard)
- Numpad keys: `NumPad1`, `NumPad2`, `NumPad3`
- Arrow keys: `Up`, `Down`, `Left`, `Right`

**Modifier Key Combinations:**
- `Ctrl+C` (copy command)
- `Alt+Tab` (window switching)
- `Shift+A` (capital A)
- `Win+R` (run dialog)
- `Ctrl+Alt+Delete` (security screen)

### Sample Log Session

```
[14:30:15.001] DOWN: A
[14:30:15.089] UP: A
[14:30:16.234] DOWN: LControlKey
[14:30:16.245] DOWN: C
[14:30:16.298] UP: C
[14:30:16.301] UP: LControlKey
[14:30:17.456] DOWN: Alt+Tab
[14:30:17.523] UP: Alt+Tab
```

### Common Use Cases

**Debugging Automation Scripts**
- Monitor what keys your automation is actually sending
- Verify timing between key events
- Check if modifier keys are being pressed/released correctly

**Input Analysis**
- Study typing patterns and speeds
- Analyze key combinations used in workflows
- Debug issues with hotkeys or shortcuts

**Testing Applications**
- Verify that your application receives the expected input events
- Test keyboard shortcuts and combinations
- Monitor mouse position for UI element positioning

### Tips for Effective Use

1. **Clear the log** before starting a new debugging session for cleaner output
2. **Disable tracking** when not needed to avoid cluttering the log with unrelated input
3. **Administrator privileges** may be needed for the global hooks to work properly
4. **Focus on timestamps** to understand the timing of input events
5. **Watch for modifier keys** - they often appear as separate DOWN/UP events

### Troubleshooting

**Mouse position not updating:**
- Ensure "Enable Tracking" is checked
- Try running as administrator
- Check if antivirus software is blocking the application

**Keyboard events not appearing:**
- Verify "Enable Tracking" checkbox is enabled
- Run the application as administrator
- Some security software may block global keyboard hooks

**Application won't start:**
- Ensure .NET 6.0 Runtime is installed
- Try running as administrator
- Check Windows Defender or antivirus exclusions

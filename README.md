# Welcome to WindowSill Extension Development! 🎉

Congratulations on creating your first WindowSill extension! This template provides you with everything you need to start building powerful extensions for WindowSill.

## 📚 Documentation

For comprehensive documentation on WindowSill extension development, visit:
**[https://getwindowsill.app/doc/articles/introduction.html](https://getwindowsill.app/doc/articles/introduction.html)**

## 🚀 Getting Started

### What's Included

This template creates a basic WindowSill extension with:
- **MySill.cs** - A sample Sill that activates when `Notepad` gets focus
- **NotepadProcessActivator.cs** - A class that detects when `Notepad` has the focus.

### How It Works

The default extension demonstrates a simple but powerful concept:

1. **Process Activation**: The `MySill` class implements `ISillActivatedByProcess`, which means it will automatically activate when a specific process gets focus on Windows.

2. **Notepad Integration**: By default, this extension activates when **Notepad.exe** gets focus. When Notepad is in the foreground, your Sill will appear in the WindowSill interface.

3. **Command Button**: The Sill provides a button in a list view. When clicked, it simulates pressing `Ctrl+N` in Notepad to create a new document.

### Debugging Your Extension

To debug and test your extension:

1. **Press F5** in Visual Studio to start debugging
2. **Open Notepad** (notepad.exe) on Windows
3. **Give focus to Notepad** by clicking on it
4. **Your Sill will appear** in the WindowSill interface
5. **Click the command button** to trigger the action (sends Ctrl+N to Notepad)

For detailed debugging instructions, see:
**[Debug an Extension](https://getwindowsill.app/doc/articles/extension-development/getting-started/debug-an-extension.html)**

### Requirements

- .NET 9.0 SDK
- Windows 10.0.22621 or later
- Visual Studio 2022 or later (recommended)
- WindowSill application installed

## 📖 Learn More

- [Introduction to WindowSill extension development](https://getwindowsill.app/doc/articles/introduction.html)
- [WindowSill API Documentation](https://getwindowsill.app/doc/api/WindowSill.API.html)

---

Happy coding! 🚀

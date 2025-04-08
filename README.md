# Calculator WPF Application

A WPF-based calculator application implemented in C# that replicates the functionality of the Windows Calculator in both Standard and Programmer modes.

## Requirements
- .NET 8.0
- Windows operating system

## Features

- **Basic Operations**: Addition, subtraction, multiplication, division, modulo, square root, square, sign inversion, and reciprocal
- **Memory Functions**: MC, M+, M-, MR, MS, and M> (memory stack)
- **Clear Functions**: Backspace, CE, and C
- **Clipboard Operations**: Custom implementation of cut/copy/paste functionality
- **Digit Grouping**: Formats numbers according to system culture settings (e.g., 1.000 for Romanian, 1,000 for UK)
- **Programmer Mode**: Base conversion between binary, octal, decimal, and hexadecimal
- **Settings Persistence**: Saves mode preferences, base selection, and digit grouping options
- **Input Methods**: Supports both keyboard and mouse input

## Technical Details

- Built with WPF and C#
- Non-resizable interface
- Menu system with File and Help options
- Settings stored in encoded configuration file
- Input validation for invalid operations (division by zero, etc.)
- Sequential operation support (cascading calculations)

## Screenshots

![Screenshot 2025-04-08 163351](https://github.com/user-attachments/assets/271b65ec-05a0-4ab0-8ed5-ad38d21235d0)
![Screenshot 2025-04-08 163302](https://github.com/user-attachments/assets/acfaec65-7184-4639-a515-d736b738212f)

## Usage

The application supports standard calculator workflow with results displayed when pressing operation buttons, equals button, or the Enter key. The Escape key clears the result (equivalent to the "C" button).

## Installation
1. Ensure you have .NET 8.0 installed on your system
2. Clone this repository
3. Open the solution in Visual Studio
4. Build and run the application

## Development Notes
This application was developed as part of a university project with a focus on implementing the MVVM pattern and data binding in WPF applications.

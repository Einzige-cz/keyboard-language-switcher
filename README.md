# Keyboard Language Switcher

Keyboard Language Switcher is a small Windows utility that automatically switches the keyboard layout depending on which physical keyboard is used for input.

For example, you can assign one keyboard to English and another keyboard to Russian. When you start typing on a selected keyboard, the program switches Windows to the assigned language layout.

## Features

- Detects physical keyboards using the Windows Raw Input API
- Shows a list of connected keyboards
- Lets you assign a Windows input language to each keyboard
- Automatically switches the active window keyboard layout
- Saves settings between launches
- Works as a small Windows desktop utility

## Privacy

This program does not record typed text.

It only detects which physical keyboard generated the input event. The program stores only the selected language assigned to each keyboard.

No data is sent to the internet.

Settings are saved locally on your computer.

## Requirements

- Windows
- .NET Desktop Runtime
- Installed keyboard layouts in Windows settings

## How to Use

1. Start the program.
2. Select a language for each keyboard in the list.
3. Minimize the program or leave it running.
4. Start typing with one of your keyboards.
5. The active Windows input language will switch automatically.

Tip: pressing `Shift` on a keyboard is usually enough for the program to detect that keyboard and switch to the assigned language.

## Notes

The first pressed key may sometimes be typed using the previous layout, because Windows and the program receive keyboard input at nearly the same time.

For best results, press `Shift`, `Ctrl`, or another non-text key before typing.

## Contact

Created by Einzige-cz.

Contact: einzige.cz@gmail.com

## License

This project is licensed under the MIT License.

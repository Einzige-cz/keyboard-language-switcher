# Keyboard Language Switcher

Keyboard Language Switcher is a lightweight Windows utility that automatically switches the keyboard layout depending on which physical keyboard is used for input.

For example, you can assign one keyboard to English and another keyboard to Russian. When you start typing on a selected keyboard, the program automatically switches Windows to the assigned input language.

## Features

- Detects physical keyboards using the Windows Raw Input API
- Displays a list of connected keyboards
- Allows assigning a Windows input language to each keyboard
- Automatically switches the active keyboard layout
- Saves settings between launches
- Lightweight Windows desktop utility

## Privacy

This program does not record typed text.

It only detects which physical keyboard generated the input event and stores the selected language assigned to each keyboard.

No data is sent to the internet.

All settings are stored locally on your computer.

## Requirements

- Windows
- .NET Desktop Runtime
- Installed keyboard layouts in Windows settings

## How to Use

1. Start the application.
2. Assign a language to each keyboard.
3. Minimize the application or leave it running.
4. Start typing on one of your keyboards.
5. Windows will automatically switch to the assigned input language.

Tip: pressing `Shift` on a keyboard is usually enough for the application to detect that keyboard and switch to the assigned layout.

## Notes

The first pressed key may occasionally be typed using the previous layout because Windows and the application process keyboard events at nearly the same time.

For best results, press `Shift`, `Ctrl`, or another non-text key before typing.

## Support

Keyboard Language Switcher is free and open source.

No features are paywalled. Donations are completely optional and help support future development.

☕ Support development:  
paypal.me/KonstantinLiubchich

## Contact

Created by Einzige-cz.

Contact: einzige.cz@gmail.com

GitHub:
https://github.com/Einzige-cz/keyboard-language-switcher

## License

This project is licensed under the MIT License.

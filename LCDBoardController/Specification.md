LCDBoardController Specification

# Overview
This software controls an LCD display board that presents itself as a COM device via a USB cable. Theoretically, this software can be made to control a wide array of LCD boards, but, at the moment, this software is designed to control a 16x2 character LCD display over USB. The board being used is an Adafruit Standard HD44780 LCD 16x2 character display (http://www.adafruit.com/products/181). A tutorial, including a command reference and driver downloads, can be found at https://learn.adafruit.com/usb-plus-serial-backpack/overview.

At the lowest level, the software communicates by sending bytes over the USB port. The LCD display board will display most bytes sent to it as ASCII characters. The display board interprets sequences of bytes beginning with 0xFE as a command character which can be used to change color, scroll settings, and other settings of the board.

The board also has eight in-memory slots and four banks of eight slots for custom-defined characters. On the board, each character is a 5-wide by 8-high monochrome grid of pixels. In software, each character is defined as eight bytes, one for each row, but only the low five bits correspond to visible pixels. Custom characters can be created by a sequence of bytes `FE 4E SS PP PP PP PP PP PP PP PP` where `FE 4E` is the "set custom character" command, `SS` is the slot that the character will be loaded into (a byte from 0 to 7 inclusive), and each `PP` byte represents one row of the character's pixel, starting from the top row to the bottom.

The `LCDBoard` class represents the board in software. It has methods to write characters and strings to the board. It also has methods for sending commands. This is the most direct way to write to the board and it will follow the board's autoscrolling rules. Finally, there are methods to erase the last character, line, and screen. The class will also have a private in-memory representation of the board's state and its custom characters.

The Message class represents a text message that can be printed to a board through an LCDBoard instance. A Message instance will accept any string and will perform pre-display processing before displaying the message. First, any UTF-16 character higher than `0x7F`, plus any character from `0x08` to `0x1F`, will be replaced with 0x3F (ASCII ?). Next, any custom character escape in the form of /c0 through /c7 will be replaced with 0x00 through 0x07 in the processed string. Finally, the string will be made to 16 characters wide by either placing words too long on the next line or by separating words with dashes, at the user's choice. Finally, the string can be displayed. If the string is too long to be displayed in two rows, only the first two rows will initially be displayed. Methods in the Message class can adjust this "cursor" and change what is displayed.

# Command Reference

From [Adafruit's Reference](https://learn.adafruit.com/usb-plus-serial-backpack/command-reference).

* Turn backlight on (`FE 42`): Turns the backlight on.
* Turn backlight off (`FE 46`): Turns the backlight off.
* Set brightness (`FE 99 LL`): Sets the brightness level of the backlight and saves the value to EEPROM. `LL` is a byte from 0 to 255 inclusive.
* Set contrast (`FE 50 LL`): Sets the contrast level of the text against the backlight and saves the value to EEPROM. `LL` is a byte from 0 to 255 inclusive and works best between 180 (`0xB4`) to 220 (`0xDC`).
* Turn autoscroll on (`FE 51`): Turns autoscrolling on. If the board is full, sending new text will force the bottom line to scroll up to the top, freeing a line for the new text.
* Turn autoscroll off (`FE 52`): Turns autoscrolling off. If the board is full, sending new text will overwrite the first line.
* Clear screen (`FE 58`): Clears all text on the screen and resets the cursor to the first character of the first line.
* Change startup splash screen (`FE 40 ...`): Writes a new splash screen to EEPROM. To write a new splash screen, send 32 after the command sequence.
* Set cursor position (`FE 47 XX YY`): Sets the cursor position. `XX` is a byte from 1 to 16 representing the character to set the cursor to. `YY` is a byte of either 1 or 2 representing the line to set the cursor to.
* Go home (`FE 48`): Sets the cursor position to the first character of the first line.
* Move cursor backward (`FE 4C`): Moves the cursor back one character. A cursor at the start of the second line will wrap back to the end of the first line. A cursor at the start of the first line will wrap back to the end of the second line.
* Move cursor forward (`FE 4D`): Moves the cursor forward one character. A cursor at the end of the first line will wrap to the beginning of the second line. A cursor at the end of the second line will wrap to the start of the first line.
* Turn underline cursor on (`FE 4A`): Turns an underline display on for the character at the cursor.
* Turn underline cursor off (`FE 4B`): Turns the underline display off.
* Turn block cursor on (`FE 53`): Turns a blinking block display on for the character at the cursor.
* Turn block cursor off (`FE 54`): Turns a blinking block display off for the character at the cursor.
* Set backlight color (`FE D0 RR GG BB`): Sets the backlight color and saves it to EEPROM. Each byte in `RR GG BB` represents the red, green, and blue components of a 24-bpp color. Each color value is between 0 and 255 inclusive.
* Create custom character (`FE 4E SS PP PP PP PP PP PP PP PP`): Creates a custom character for display on the board. Each custom character is represented using eight bytes, one for each row of 5 pixels. Only the low five bits are used to set pixel data, the top three bits are ignored. `SS` is the slot the character will be saved to, from 0 to 7 inclusive. Each `PP` represents a row of pixel data.
* Save custom characters to EEPROM bank (`FE C1 BB`): Saves eight characters from memory into one of four EEPROM banks. `BB` is either 0-3 inclusive, or 1-4 inclusive.
* Load custom characters from EEPROM bank (`FE C0 BB`): Loads eight characters from one of four EEPROM banks into memory. `BB` is either 0-3 inclusive, or 1-4 inclusive.
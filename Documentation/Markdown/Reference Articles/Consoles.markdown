---
layout: reference-article
title: Consoles
date: 2015-09-03 10:50:00
categories: [ docs, reference ]
parent_name: Displays & Graphics
---

# Introduction

A console is a vital part of any operating system. Whether you like command lines or not, a console will be the first way (and only way, for quite a while), to communicate with your operating system. It is a vitasl control and debugging tool.

## Scope of this article

This article will cover how to implement a console, the difference between a console and a shell and the various software techniques for providing useful features such as scrolling.

---

# Overview

- Consoles, terminals and shells
- Purpose of a console
- Alternatives

## What is a console?
A console is an input/output layer of software for a specific set of input/output hardware. It usually uses a keyboard and screen but a serial connection (to a host computer) is also common. The console handles writing text to the output, colouring that text and also receiving input from the user. It does not, however, understand any of the input (or output) beyond the characters themselves. 

## What is a terminal?
A terminal is a generic software construct for input/output. A console is a type of terminal. Other types of terminal include proper GUIs (such as the Windows Desktop), Refreshable Braille Displays and network prompts. Terminals are not, therefore, limited to just text based input/output in the way that consoles are.

## What is a shell?
A shell is a program which accepts input from a terminal, interprets it (as commands or data) and then provides some output to the terminal. It can also output stuff to the terminal asynchronously i.e. without the user triggering it directly (such as the result of a web request). A shell is the thing which can take text input from a console (which is a type of terminal), interpret it as commands and then execute those commands. It will then print the result of the commands.

## Why are consoles, terminals and shells kept separate?
They are kept separate because it allows any terminal to be "plugged into" any shell (assuming the shell supports the given types of I/O). This means you can have two completely different GUI terminals but with the same logic driving the input commands and data and resultant output. Linux uses this to great effect to provide different UIs for different platforms. 

## Why is a console necessary?
When first starting out in OS dev, you will not be able to get a full graphics driver or other I/O system working quickly (probably not even within a year). A console is simple to set up and provides arguably the most powerful interface to a computer (if you understand how to use it, which you will since you're going to be writing it!). So a console allows you to control your OS without requiring lots of complex coding to support it. Also, consoles are very useful for debugging and in an embedded environment, a display of any form may not be available. Embedded devices usually use serial/UART connections for their consoles.

## How do I implement a console?
The steps to implement a console are fairly simple:

1. Write a VGA text-mode driver (x86) or a serial port driver (x86) or a UART driver (MIPS). All three of these drivers are simple and require very few lines of code.
2. Write a few methods for printing strings to the display (VGA - x86) or the serial/UART (x86/MIPS)
3. Write more complex methods (where necessary) to handle line buffering, input (keyboard or serial - x86/MIPS), and scrolling.
4. Write a shell program that uses your console to receive commands, execute functions and then output a result.

---

# Hardware

## Overview

There are essentially two pieces of hardware that are best suited for consoles: displays and serial connections. (USB/Network connections are also possible but are much more complex). 

On x86 you have the choice of using a VGA text-mode driver (which is easy to set up) or a serial port (for virtual machines, COM 1). VGA text-mode is easiest to see since the output appears as part of the virtual machine's display and requires no additional setup. However, a serial port allows you to save the contents to a file and does not require special code for handling scrolling the screen.

On MIPS you have only one realistic choice, which is to use one of the UART outputs on the board. There are 5 UART outputs; UARTs 0 and 4 are the easiest to access. UART 0 is on the RaspberryPi-compatible Primary Expansion Header and UART 4 is the dedicated UART header in the white connector. Cheap USB to UART converters can be bought online but ensure you buy one which is compatible with your computer. There are two main chips used in USB to UART devices of which one is only Linux compatible and the other is Linux and Windows (>= Win7) compatible.

---

# Software

## Overview
The following sections describe the software required to implement a console. VGA text-mode, keyboard input, serial ports and UART ports are all described in other articles so they will not be covered here.

## Basic outline
The main aims are to print text to the screen and to provide an input gathering system so that a shell using the console jut receives complete command lines. If you're going to be using VGA text-mode as your output, you may also want to retain a buffer (around 200 lines or so) of lines which were printed to the screen. This will allow you to scroll the output to see older messages as the screen can only display 25 lines of 80 characters wide at a time. You may also want to provide text and background colouring which can be achieved for both VGA and serial/UART outputs (assuming the serial client supports ANSI Colour Commands - use something like TeraTerm on Windows.).

The functions you will need are:

- Write / Print
- WriteLine / PrintLine
- SetTextColour
- SetBackgroundColour
- ReadLine
- ReadChar
- ReadKey
- (VGA text-mode) Scroll (accepts a signed integer as number of lines, sign indicates direction).

For VGA text-mode you may also want to be able to subdivide the screen into different areas for separate consoles to reside in which case you will want functions for setting the width, height, x and y properties of the console. Serial-based consoles can simply use extra serial or UART outputs for additional terminals. 

## Implementation details

### Write / Print

Writing/printing to serial simply requires looping every character of the supplied string (excluding any NULL terminator character) and writing each of them to the serial/UART port. 

Writing/printing to VGA text-mode requires a little more work. The console must keep track of its position on the screen and write character/colours byte pairs to the corresponding memory location. More examples of how to do this are provided in the VGA text-mode article. The following is a short sample demonstrating the basic idea.

``` csharp
// This block outputs the string in the current foreground / background colours.
//    (Note: Here a char is an unsigned short i.e. a 16-bit unsigned value)
char* vidMemPtr = vidMemBasePtr + offset; // = 0xB8000 + Offset of current output location
char* strPtr = str.GetCharPointer();      // Gets a pointer to the start of the string
while (strLength > 0)                     // Loops the entire length of the string
{
    *vidMemPtr = (char)((*strPtr & 0x00FF) | colour); // Sets the character of the screen and its colour

    strLength--;    // Track length of remaining string
    vidMemPtr++;    // Move to next character/colour 2-byte pair in memory
    strPtr++;       // Move to next character in the string
    offset++;       // Track the global offset value
}
```

### WriteLine / PrintLine

Simply call your Write or Print method with the newline character ("\n").

### SetTextColour / SetBackgroundColour

To keep track of text colour in VGA mode you must keep track of a colour byte as per the VGA text-mode colour byte specification. In this byte, the low 4 bits represent the text colour and the high four bits represent the background colour. When you print a character to the screen, the byte following the character sets the character's colour. 

``` csharp
/* colour = char, Background and foreground colour combined
 * bg_colour = char, Background colour
 * text_colour = char, Foreground colour
 *
 * All colour values live in the high byte of the char.
 * In this context, a char is an unsigned short i.e. a unsigned 16-bit value.

// Generate component colour values from input Bg/Fg colours.
bg_colour = (char)(ABackgroundColour & 0xF000);
text_colour = (char)(AForegroundColour & 0x0F00);

// Generate complete colour value from bg/text colour components.
colour = (char)(bg_colour | text_colour);
```

To change the colour for a serial console, you will need to output the colour command sequence including the ANSI colour code. The following code demonstrates how to set the text colour to various values. A full colour list (amongst other commands) can be found here: [Wikipedia.org - ANSI Escape Codes](https://en.wikipedia.org/wiki/ANSI_escape_code)

``` csharp
public static void SetColour_Red()
{
    Write((char)27); // Write escape code 0x1B = 27 to mark start of sequence
    Write("[0;31m"); // Write '[' followed by: 
                     //   - Reset command (to clear formatting)
                     //   - Set Colour (30) to Red (1) command 
                     //   - End of sequence 'm'
}
public static void SetColour_Yellow()
{
    Write((char)27);
    Write("[1;33m");
}
public static void SetColour_Green()
{
    Write((char)27);
    Write("[0;32m");
}
public static void SetColour_White()
{
    Write((char)27);
    Write("[1;37m");
}
public static void SetColour_Black()
{
    Write((char)27);
    Write("[0;30m");
}
```

### Output buffering

Output buffering only really applies to VGA text-mode output since most serial clients will automatically buffer the output. To buffer lines of output, create a circular buffer of whatever size you require (though probably at least the size of the screen). When someone prints a line of text, instead of printing straight to the screen, print it to the next entry in the buffer. If the buffer overflows, simply overwrite the old text in the next entry. You should then write a separate Update (or Draw) method which prints text from the current position in the buffer upwards.

### Input buffering

Depending on how much input you want to be received at once, you can use either a single-line buffer of fixed size, or you can use a fixed-array buffer of lines or you can attempt to dynamically allocate. It is also possible to store input data in the output buffer and simply keep track of where the input starts and ends in the output buffer. The disadvantage of this is the original output data is lost and the input/output becomes mixed.

### Scrolling

Scrolling for serial consoles needs no special implementation but if you wish to support the arrow keys you will want to look at the ANSI Cursor Position commands (see [Wikipedia.org - ANSI Escape Codes](https://en.wikipedia.org/wiki/ANSI_escape_code)).

Scrolling for VGA text-mode can be achieved by using buffered output and simply updating the current output position in the buffer followed by re-drawing.

### Screen subdivision

This only applies to VGA text-mode. Screen subdivision can be achieved by using variables to specify the x,y location of the start of the console on the screen and width/height variables to specify the area to print within. Your write, print or draw methods should then only output characters within the specified area. You may also wish to put borders around separate consoles, in which case simply leave a single-line / single-column gap between consoles and use "-" and "|" to output horizontal and vertical dividers respectively.

---

# Example Code

## Overview
TODO

## Download
TODO

---

# Further Reading

- [Wikipedia.org - Command Line Interface](https://en.wikipedia.org/wiki/Command-line_interface)
- [Wikipedia.org - Shell (computing)](https://en.wikipedia.org/wiki/Shell_(computing))
- [Wikiepdia.org - ANSI escape codes](https://en.wikipedia.org/wiki/ANSI_escape_code)
- [Superuser.com - Difference between shell and terminal](http://superuser.com/questions/144666/what-is-the-difference-between-shell-console-and-terminal)
- [AskUbuntu.com - Difference between terminal, console and shell](http://askubuntu.com/questions/506510/what-is-the-difference-between-terminal-console-shell-and-command-line)
- [OSDev.org - Text UI](http://wiki.osdev.org/Text_UI)
- [OSDev.org - Printing to the screen](http://wiki.osdev.org/Printing_To_Screen)
- [OSDev.org - Serial ports](http://wiki.osdev.org/Serial_Ports)
- [OSDev.org - (VGA) Text-mode Cursor](http://wiki.osdev.org/Text_Mode_Cursor)
- [CodeProject.com - Implementing a shell](http://www.codeproject.com/Articles/477782/Implementing-command-execution-in-a-console-applic)

*[VGA]: Video Graphics Array

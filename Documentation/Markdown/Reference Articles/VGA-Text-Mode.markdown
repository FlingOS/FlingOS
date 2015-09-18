---
layout: reference-article
title: VGA Text-mode
date: 2015-09-11 11:21:00
categories: [ docs, reference ]
parent_name: Displays & Graphics
description: Describes VGA text-mode as used on the x86 PC platform. 
---

# Introduction

VGA text-mode is one of the most useful and easiest things to get working on x86-based PCs and acts as one of the main output modes. The main alternative output to a VGA text-mode screen is a serial port and often both the screen and serial are used in combination. To output characters to the screen is as simple as writing character/colour byte pairs to the correct location in memory. 

## Scope of this article

This article will cover VGA text-mode including what it is, why it is used and how to implement it in software. This article does not cover displays, graphics, VGA and consoles since these are covered in separate articles.

---

# Overview

## What is VGA text-mode?

VGA text-mode is a display mode which divides the screen into 80 by 25 (or 40x25) character cells. Each cell is specified by a two-byte pair where the low byte specifies the character (in ASCII encoding) and the high byte specifies the colour according to the VGA colour table (provided in the Software section).

VGA text-mode is one of the earliest display modes that has been supported since MDA and CGA. Later versions (including EGA, VGA, XGA and SVGA) have all retained support for VGA text-mode and a lot of modern PC (often BIOS) firmware has support for emulating VGA text-mode. 

## Why is VGA text-mode useful?

VGA text-mode is very easy to use, requires no configuration (if using a standard bootloader or if the BIOS configures it) and is probably the most reliable, cross-system (i.e. virtual and real hardware) way to output information. 

## How is VGA text-mode used?

VGA text-mode is used to display text which can be used to create a console interface (when keyboard input, line buffering and other techniques are implemented). It is also the origin of ASCII art, in which ASCII characters are used to construct a larger image (often piece of text). FlingOS uses this technique at startup to create a splash screen.

## Are there lower or higher resolutions?
There are alternative resolutions depending on the display mode (or emulated display mode) and the hardware available. Alternative resolutions include:

| Resolution | Hardware |
|:--------------:|:-------------:|
| 40x25     | CGA, EGA |
| 80x25     | CGA, EGA, VGA |
| 80x43     | EGA |
| 80x50     | VGA |
| 80x60     | SVGA |
| 132x25    | SVGA |
| 132x43    | SVGA |
| 132x50    | SVGA |
| 132x60    | SVGA |

---

# Hardware

VGA hardware has a long and winding history. In fact the vast majority of what we refer to as VGA is actually SVGA (Super VGA). The text mode that is now known as VGA text-mode first appeared as part of MDA in a black-and-white format. CGA introduced the colour table and EGA increased the actual resolution at which characters were displayed, along with widening the display.

VGA and SVGA both continued the trend of increasing the resolution and width/height of the display. However, 80x25 is still the most backwards compatible and widely supported mode. This article will assume you have used a bootloader which configured the system to 80x25 mode.

---

# Software

## Overview

## Technical details

The x86 PC memory layout is largely undefined, except for the first 1MiB of RAM. The first 1MiB is the home of real-mode and basic devices memory. 

VGA text-mode uses a 4000 byte section at address 0xB8000. Every two bytes forms a pair in which the low byte is the ASCII character of a cell and the high byte is the colour. In 80x25 resolution, every 160 bytes forms a line from left to right. Lines go from the top of the display downwards and in 80x25 mode there are 25 lines. 

Each character byte is an ASCII character code - the ASCII table is provided below. Each colour byte consists of 4 low bits and 4 high bits which specify the foreground and background colour respectively. The colour table is provided below.

### ASCII Table

*The following table is provided by Wikipedia and is licensed under the Creative Commons Attribution/Share-Alike License v3.0*

![Wikipedia.org - ASCII Table](https://upload.wikimedia.org/wikipedia/commons/thumb/1/1b/ASCII-Table-wide.svg/2000px-ASCII-Table-wide.svg.png)

### Colour table

| Number | Name | Number + bright bit | Name |
|:-----------:|:--------:|:---------------------------:|:---------:|
| 0 | Black | 8 | Grey |
| 1 | Blue  | 9 | Light Blue |
| 2 | Green	| A | Light Green |
| 3 | Cyan	| B | Light Cyan |
| 4 | Red	| C | Light Red |
| 5 | Magenta   | D | Light Magenta |
| 6 | Brown	    | E | Yellow |
| 7 | Light Gray | F | White |

## Implementation details

The following C# code samples demonstrate how to set a character in VGA text-mode memory, set the colour of a character and update the cursor position.

### Set Character

``` csharp
unsafe static void SetCharacter(int line, int col, char value)
{
    byte* DisplayPtr = (byte*)0xB8000;
    DisplayPtr[(line * 80 + col) * 2] = (byte)(value & 0xFF);
}
```

### Set Colour

``` csharp
unsafe static void SetColour(int line, int col, byte colour)
{
    byte* DisplayPtr = (byte*)0xB8000;
    DisplayPtr[(line * 80 + col) * 2 + 1] = (byte)(colour & 0xFF);
}
```

### Set Cursor Position

``` csharp
/// <summary>
/// The command port for manipulating the VGA text-mode cursor.
/// </summary>
protected Hardware.IO.IOPort CursorCmdPort = new Hardware.IO.IOPort(0x3D4);
/// <summary>
/// The data port for manipulating the VGA text-mode cursor.
/// </summary>
protected Hardware.IO.IOPort CursorDataPort = new Hardware.IO.IOPort(0x3D5);

/// <summary>
/// Sets the displayed position of the cursor.
/// </summary>
/// <param name="character">
/// The 0-based offset from the start of a line to the character to display the cursor on.
/// </param>
/// <param name="line">The 0-based index of the line to display the cursor on.</param>
public override void SetCursorPosition(ushort line, ushort col)
{
    //Offset is in number of characters from start of video memory 
    //  (not number of bytes).
    ushort offset = (ushort)((line * ScreenLineWidth) + character);
    //Output the high-byte
    CursorCmdPort.Write_Byte((byte)14);
    CursorDataPort.Write_Byte((byte)(offset >> 8));
    //Output the low-byte
    CursorCmdPort.Write_Byte((byte)15);
    CursorDataPort.Write_Byte((byte)(offset));
}
```

## Alternatives

Instead, or in addition to, outputting to the screen, many developers choose to output to serial/UART ports. This has advantages including being able to store the output for later reference. However, often a serial port is not available or accessible on real hardware, in which case the screen is the only easily available output.

## Compatibility

Almost all PCs (including laptops) support VGA text-mode output since it has been used by BIOS (and UEFI) for a long time as the primary output. VAG text-mode is often emulated by firmware, including HDMI devices.

---

# Example Code

## Overview
FlingOS has multiple VGA text-mode implementations. The BasicConsole is a well-tested, zero-dependency implementation with support for a secondary output to a serial port and setting text colour buyt not scrolling. The AdvancedConsole is a well-tested but more system-dependent implementation which supports scrolling, backspace and other more advanced features.

The BasicConsole can be found at: [https://github.com/FlingOS/FlingOS/blob/master/Kernel/Libraries/Kernel.FOS_System/BasicConsole.cs](https://github.com/FlingOS/FlingOS/blob/master/Kernel/Libraries/Kernel.FOS_System/BasicConsole.cs)

The AdvancedConsole, and its base class Console, can be found at: [https://github.com/FlingOS/FlingOS/blob/master/Kernel/Libraries/Kernel.Core/Consoles/AdvancedConsole.cs](https://github.com/FlingOS/FlingOS/blob/master/Kernel/Libraries/Kernel.Core/Consoles/AdvancedConsole.cs) and [https://github.com/FlingOS/FlingOS/blob/master/Kernel/Libraries/Kernel.Core/Console.cs](https://github.com/FlingOS/FlingOS/blob/master/Kernel/Libraries/Kernel.Core/Console.cs).

---

# Further Reading

- [Wikipedia.org - VGA Compatible text-mode](https://en.wikipedia.org/wiki/VGA-compatible_text_mode)
- [Wikipedia.org - Video Graphics Array](https://en.wikipedia.org/wiki/Video_Graphics_Array)
- [OSDev.org - Text UI](http://wiki.osdev.org/Text_UI)
- [OSDev.org - Text Mode Cursor](http://wiki.osdev.org/Text_Mode_Cursor)
- [OSDever.net - VGA Text](http://www.osdever.net/FreeVGA/vga/vgatext.htm)
- [OSDever.net - Printing](http://www.osdever.net/bkerndev/Docs/printing.htm)
- [Stanford.edu - Text Cursor](http://web.stanford.edu/class/cs140/projects/pintos/specs/freevga/vga/textcur.htm)
- [OSData.com - Memory Maps](http://www.osdata.com/system/physical/memmap.htm)
- [BrokenThorn.com - VGA Text-mode](http://www.brokenthorn.com/Resources/OSDev10.html)

*[MDA]: Monochrome Display Adapter
*[CGA]: Colour Graphics Adapter
*[EGA]: Enhanced Graphics Adapter
*[VGA]: Video Graphics Array
*[SVGA]: Super Video Graphics Array

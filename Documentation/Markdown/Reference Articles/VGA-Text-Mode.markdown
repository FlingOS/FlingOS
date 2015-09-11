---
layout: reference-article
title: VGA Text-mode
date: 2015-09-11 11:21:00
categories: [ docs, reference ]
parent_name: Displays & Graphics
description: A description
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

*The following table is provided by Wikipedia and is licensed under the Creative Commons Attribution/Share-Alike License v3.0*

![Wikipedia.org - ASCII Table](https://upload.wikimedia.org/wikipedia/commons/thumb/1/1b/ASCII-Table-wide.svg/2000px-ASCII-Table-wide.svg.png)


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

- Outputting character
- Setting colours
- Moving cursor

## Alternatives
- Serial

## Compatibility
- Almost all PCs (even HDMI) emulate

---

# Example Code

## Overview
TODO

## Download
TODO

---

# Further Reading

- https://en.wikipedia.org/wiki/VGA-compatible_text_mode
- https://en.wikipedia.org/wiki/Video_Graphics_Array
- http://wiki.osdev.org/Text_UI
- http://wiki.osdev.org/Text_Mode_Cursor
- http://www.osdever.net/FreeVGA/vga/vgatext.htm
- http://www.osdever.net/bkerndev/Docs/printing.htm
- http://web.stanford.edu/class/cs140/projects/pintos/specs/freevga/vga/textcur.htm
- http://www.osdata.com/system/physical/memmap.htm
- http://www.brokenthorn.com/Resources/OSDev10.html

*[MDA]: Monochrome Display Adapter
*[CGA]: Colour Graphics Adapter
*[EGA]: Enhanced Graphics Adapter
*[VGA]: Video Graphics Array
*[SVGA]: Super Video Graphics Array

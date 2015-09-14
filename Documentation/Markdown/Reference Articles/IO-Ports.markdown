---
layout: reference-article
title: IO Ports
date: 2015-08-30 16:12:00
categories: [ docs, reference ]
description: Describes Input/Output ports in general and has specific details for the x86 architecture (including common port assignments).
---

# Introduction

I/O Ports are a mechanism in a variety of architectures used to access registers in attached devices. I/O Ports in this context are attached to the address bus and so may also be known as Isolated I/O (ports). Isolated I/O is not the same as alternatives known as memory mapped and channel I/O. Memory-mapped I/O uses RAM memory locations assigned to devices and channel I/O uses separate I/O processors. 

## Scope of this article

This article looks only at Isolated I/O (a.k.a. I/O Ports) with an emphasis on the x86 architecture (since that is the most likely place a systems developer will come across them). This article does not attempt to explain memory-mapped or channel I/O. This article will also provide a brief list of common, fixed-assignment (and in some cases dynamically assigned) devices for I/O Port addresses but will not provide details of how to use those devices; using particular devices is detailed in separate articles.

---

# History

Outside of a specific architecture there isn't a great deal of history to I/O Ports. The system has remained the same since its conception and any progression has been essentially practical and architecture specific with no modification to how I/O ports work in theory. The following two articles provide some information about the history of I/O Ports for different systems, particularly x86 (which is the only widely-known/widespread system to use I/O Ports).

- [Clemson.edu - I/O History](http://people.cs.clemson.edu/~mark/io_hist.html)
- [KarbosGuide.com - The PC's I/O System](http://www.karbosguide.com/books/pcarchitecture/chapter38.htm)

---

# Overview

## What are I/O Ports?

I/O Ports are just an address on the address bus. They are mapped to one register on one device (or, in poorly implemented systems, more than one devices). I/O Ports is also known as Isolated I/O because devices are isolated from the memory (RAM) but don't use separate I/O Processors (as Channel I/O does). 

## What are the pros &amp; cons of I/O Ports?

I/O Ports allow isolation of devices from memory which offers some protection from rogue devices. I/O Ports also allow a totally separate address space for devices meaning more devices can be attached without running out of space. However, because of the way I/O Ports work, they are slow and often slow down the rest of the system as they block RAM access (since the address bus is shared). In some implementations the data bus is also shared leading to further complications. I/O Port systems also require extra hardware to implement the system and so can't be made as small as memory-mapped I/O systems. Extra hardware also inherently means greater power consumption. 

A final disadvantage is that, I/O ports don't (usually) allow standard operations to act on them in the way they can on RAM. This means values stored in I/O Port registers require more instructions to alter. For example, to add to a value in an I/O Port register requires three instructions: copy to a CPU-local register, add to it, write new value back to I/O port register. In more complex situations, this load-modify-write sequence cannot be optimised where as it can be in memory-mapped systems.

## Why are I/O Ports used?

I/O Ports are only used for legacy devices and some device configuration/setup. Nowadays memory-mapped I/O is preferred as it is much faster, simpler and lower-power. Some common devices (such as PS/2 keyboards and mice) simply do not support memory-mapped I/O since they originate from before x86 supported memory-mapped I/O and so I/O ports is the only way to access such devices.

## How are port numbers assigned?

There are two methods for assigning I/O ports - fixed-assignment and dynamic allocation. The former is done by the hardware designer of a given device and cannot be altered. Only older device types use this method since it can easily result in address conflict (though well-designed/implemented/manufactured systems will avoid such conflict). A list of common port addresses for fixed-assignment device types can be found later in this article. 

Dynamic allocation is done by the first-stage boot process; On PC's dynamic allocation is done by the BIOS (or, in advanced operating systems, the kernel during device enumeration). Commonly this involves enumerating the PCI bus and assigning discovered devices an I/O port. While dynamic allocation avoids address/device conflict, it makes the system programmer's life a little harder as they must have access to the PCI (or alternative) information to determine the port number for a given device. It also means a device cannot be seamlessly (from the driver's perspective) unplugged and plugged back. Dynamic allocation of ports is part of the Plug 'n' Play system (PnP).

---

# Hardware

## Overview

This section will focus on x86 hardware. Other systems which use I/O Ports will follow a similar hardware design. 

The CPU is connected to the address bus which is split into the RAM bus and I/O bus. The address being outputted by the CPU is directed towards one bus or the other by the internal memory management unit (MMU). Special instructions tell the MMU when to switch from RAM access (the default) to I/O Port access. Every device on the I/O bus watches the address value. When the address matches a device's address, the device connects the data lines to the physical register within the device. 

Due to compromises (mostly as a result of cost) not all devices listen to all the address lines (which is obviously daft now but back when the devices were designed saved a lot of money!). Unfortunately, this has means some older devices can end up thinking an address is meant for them which isn't. This can lead to multiple devices hooking up the data lines to registers which causes both electrical and logical conflict. To solve this, many devices started using addresses where the bottom-most address lines were fixed to a value which no other device (at the time) used and simply used changes in the higher bits as valid address lines. Unfortunately, this leads to a dramatic reduction in the number of usable address lines and potentially performance. This is not so much of an issue on modern systems.

## Details : Externals

There are a variety of common external ports which are I/O Port-based devices. These include PS/2 keyboards and mice, serial ports, parallel ports (LPT) and PATA hard drives. Whether a particular device is I/O Port or memory-mapped I/O driven cannot be determined without looking at the specification for the device. However, most devices since the mid-90s have been designed with memory-mapped I/O in mind. It is not uncommon to find devices which use I/O ports for initial configuration/setup (to agree memory mapped location) followed by using memory-mapped I/O for significant data transfer.

---

# Software

## Overview

Architectures which use I/O Ports will have special input and output instructions. The following software section focuses on the x86 architecture.

I/O Port software is very simple; all you need is an address of a device and a value. You do, however, need to use the correct sized value otherwise you may cause unintended read/write. If a register is one byte in size, don't try to read/write a two or four byte value! This means using the correct variation of the in/out instruction along with the correct register subdivision. 

Be aware that the in/out x86 instructions are, by default, privileged instructions so cannot be used from user-mode (ring 3). This behaviour can be overridden, though it is not recommended.

## Technical details

The following table lists common addresses (a mixture of statically and dynamically allocated). Note that in x86, I/O ports only have a 16-bit address bus allowing at most 65,536 ports to be addressed. For addresses listed twice, the no single system should (?!) contain both types of device at the same address. Some devices have hacks to get around this or the address can be dynamically assigned.

*Information in this table has been gathered, merged and adapted from the following two main sources: [Wikipedia.org: Input/Output Base Address](https://en.wikipedia.org/wiki/Input/output_base_address) and [OSDev.org: I/O Ports](http://wiki.osdev.org/I/O_Ports)*

##### Common address assignments 

| Address(es) | Device types |
|:----------------:|:------------------|
| 0x00 - 0x1F | First DMA controller (Commonly 8237 A-5 chip. Often used by floppy disk drives). |
| 0x20 - 0x3F | First Programmable Interrupt Controller (PIC) (Commonly the 8259A chip and usually this PIC is assigned as the Master PIC.) |
| 0x40 - 0x5F | Programmable Interval Timer (Commonly 8254 chip.) |
| 0x60 - 0x6F | PS/2 Keyboard (Always 8042 chip) |
| 0x70 - 0x7F | Real Time Clock, NMI mask |
| 0x80 - 0x9F | DMA Page Registers (Commonly 74LS612  chip) |
| 0xA0 - 0xBF | Second Programmable Interrupt Controller (PIC) (Commonly the 8259A chip and usually this PIC is assigned as the Slave PIC.) |
| 0xC0 - 0xDF | Second DMA controller (Commonly 8237 A-5 chip |
| 0xE9 | Often used for the [Port 0xE9 Hack](http://wiki.osdev.org/Bochs). Used on some emulators to directly send text to the hosts' console. |
| 0xF0 | Math coprocessor (usually 80287) : Clear Busy |
| 0xF1 | Math coprocessor (usually 80287) : Reset |
| 0xF8 - 0xFF | Math coprocessor (usually 80287) |
| 0xF0 - 0xF5 | PCjr Disk Controller |
| 0xF8 - 0xFF | Reserved for future microprocessor extensions |
| 0x100 - 0x10F | Programmable Option Select (POS) (Used by PS/2) |
| 0x110 - 0x1EF | System I/O channel |
| 0x140 - 0x15F | Secondary SCSI host adapter |
| 0x170 - 0x177 | Secondary PATA Disk Controller (often attached to CD/DVD drives or backup HDD.) |
| 0x1F0 - 0x1F7 | Primary PATA Disk Controller (almost always the primary hard disk drive / boot drive if booted from an HDD) |
| 0x200 - 0x20F | Game port |
| 0x210 - 0x217 | Expansion Unit |
| 0x220 - 0x233 | Sound Blaster and most other sound cards |
| 0x278 - 0x27F | Parallel port 3 |
| 0x280 - 0x29F | LCD on Wyse 2108 PC SMC Elite default factory setting |
| 0x2B0 - 0x2DF | Alternate Enhanced Graphics Adapter (EGA) display control |
| 0x2E8 - 0x2EF | Serial port 4 (if available, often COM 4) |
| 0x2E1 | GPIB/IEEE-488 Adapter 0 |
| 0x2E2 - 0x2E3 | Data acquisition |
| 0x2F8 - 0x2FF | Serial port 2 (if available, often COM 2) |
| 0x300 - 0x31F | Prototype Card |
| 0x300 - 0x31F | Novell NE1000 compatible Ethernet network interfaces |
| 0x300 - 0x31F | AMD Am7990 Ethernet network interface, IRQ=5. |
| 0x320 - 0x323 | ST-506 and compatible hard disk drive interface |
| 0x330 - 0x331 | MPU-401 MIDI Processing Unit on most sound cards |
| 0x340 - 0x35F | Primary SCSI host adapter |
| 0x370 - 0x377 | Secondary floppy disk drive controller |
| 0x378 - 0x37F | Parallel port 2 |
| 0x380 - 0x38C | Secondary Binary Synchronous Data Link Control (SDLC) adapter |
| 0x388 - 0x389 | AdLib Music Synthesizer Card |
| 0x3A0 - 0x3A9 | Primary Binary Synchronous Data Link Control (SDLC) adapter |
| 0x3B0 - 0x3BB | Monochrome Display Adapter (MDA) display control. *IBM VGA and direct predecessors* |
| 0x3BC - 0x3BF | Parallel port 1 on MDA card. *IBM VGA and direct predecessors* |
| 0x3C0 - 0x3CF | Enhanced Graphics Adapter (EGA) display control. *IBM VGA and direct predecessors* |
| 0x3D0 - 0x3DF | Color Graphics Adapter (CGA). *IBM VGA and direct predecessors* |
| 0x3E8 - 0x3EF | Serial port 3 (if available, often COM 3) |
| 0x3F0 - 0x3F7 | Primary floppy disk drive controller. Primary IDE controller (slave drive) (3F6–3F7h) |
| 0x3F8 - 0x3FF | Serial port 1 (if available, often COM1 ) |
| 0xCF8 – 0xCFC | PCI configuration space |

##### Common DMA Page Register assignments

| Address | DMA Channel |
|:-----------:|:--------------------|
| 0x87 | DMA Channel 0 |
| 0x83 | DMA Channel 1 |
| 0x81 | DMA Channel 2 |
| 0x82 | DMA Channel 3 |
| 0x8B | DMA Channel 5 |
| 0x89 | DMA Channel 6 |
| 0x8A | DMA Channel 7 |
| 0x8F | (DMA) Refresh |
| 0x92 | A20 Gate register |

*(For the purposes of licensing, the above two tables are licensed under the [Creative Commons Attribution-ShareAlike 3.0 License - http://creativecommons.org/licenses/by-sa/3.0/](http://creativecommons.org/licenses/by-sa/3.0/). The rest of this document remains under the normal license used by FlingOS for its documentation. A link to the license used by FlingOS can usually be found at the bottom of the page.)*

## Implementation details

x86 I/O ports are addressed from 0 to 65,535 (inclusive). However, some forms of the x86 in/out instructions can only access the first 256 addresses. Details are given below.

The in/out instructions always transfer values to/from the EAX register (or one of its subdivisions depending on the size of the data). There are three variants of each operation:

- inb, inw and inl (for byte, word and dword sized values respectively)
- outb, outw and outl (for byte, word and dword sized values respectively)

The correct variant is often inferred from the size of the subdivision register of EAX used. However, for safety and compile-time checking, specifying the exact variant allows the compiler to detect when the wrong EAX subdivision has been used.

There are two ways to use the in/out instructions. The source can either be an immediate value or the DX register. The value register is always one of EAX, AX or AL (note that AH is not allowed). If the source operand (the port address) is an immediate value, only port addresses 0 to 255 can be accessed. If the source operand is DX, any of the 65,536 ports can be accessed. 

NASM code for outputting a 32-bit value:

``` x86asm
; Note: not a real-value example - the actual values used are not valid!
mov eax, 0xDEADBEEF   ; Load value to output
mov dx, 0x00          ; Load address to output to
outl dx, eax          ; Output the full 32-bit value from EAX
```

NASM code for outputting an 8-bit value from the first byte of EAX:

``` x86asm
; Note: not a real-value example - the actual values used are not valid!
mov eax, 0xDEADBEEF   ; Load a value to output. 
                      ;   - Only 0xEF (first byte) will be outputted
mov dx, 0x00          ; Load address to output to
outb dx, al           ; Output first byte of EAX (AL) as an 8-bit value
```

NASM code for inputting a 32-bit value:

``` x86asm
; Note: not a real-value example - the actual values used are not valid!
mov dx, 0x00         ; Load address to output to
inl eax, dx          ; Output the full 32-bit value from EAX
```

---

# FAQ & Common Problems

## In/out instructions cause an exception (interrupt)
In/out instructions cannot, by default, be used from user-mode. This behaviour can be overridden, though it is not recommended. In/out are privileged instructions under the x86 architecture so require the CPL to be 0 (i.e. kernel-mode) or the TSS to specify the instructions are allowed in its I/O Permission bits.

---

# Further Reading

- [Wikipedia.org - Memory-mapped I/O](https://en.wikipedia.org/wiki/Memory-mapped_I/O)
- [Wikipedia.org - Input/output Base Address](https://en.wikipedia.org/wiki/Input/output_base_address)
- [OSDev.org - I/O Ports](http://wiki.osdev.org/I/O_Ports)
- [ComputerHope.com - IO Port](http://www.computerhope.com/jargon/i/ioport.htm)
- [Kids-Online.net - PC Hardware Click-N-Learn](http://www.kids-online.net/learn/click/table.html)
- [Kids-Online.net - PC Hardware Click-N-Learn - I/O Ports](http://www.kids-online.net/learn/click/details/ioport.html)
- [PpearsonITCertification.com - I/O Ports and Devices](http://www.pearsonitcertification.com/articles/article.aspx?p=1681059)
- [ReneJeschke.de - OUT instruction](http://x86.renejeschke.de/html/file_module_x86_id_222.html)
- [ReneJeschke.de - IN instruction](http://x86.renejeschke.de/html/file_module_x86_id_139.html)
- [OSDev.org - Bochs (0xE9 Hack)](http://wiki.osdev.org/Bochs)
- [Stackoverflow.com - What's the difference between “COM”, “USB”, “Serial Port”?](http://stackoverflow.com/questions/27937916/whats-the-difference-between-com-usb-serial-port)
- [Wikibooks.org - In/Out Instructions](https://en.wikibooks.org/wiki/X86_Assembly/Other_Instructions#I.2FO_Instructions)

*[I/O]: Input/Output
*[IO]: Input/Output
*[RAM]: Random Access Memory
*[PCI]: Peripheral Component Interconnect
*[DMA]: Direct Memory Access
*[dword]: Double-word : 4-byte value
*[NMI]: Non-Maskable Interrupt
*[PATA]: Parallel AT Attachment (Parallel ATA)
*[SCSI]: Small Computer System Interface
*[COM]: COMmunication (port)
*[VGA]: Video Graphics Array
*[TSS]: Task State Segment (structure)
*[PS/2]: IBM Personal System/2 (PS/2 keyboards/mice)
*[BIOS]: Basic Input/Output System
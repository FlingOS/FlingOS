---
layout: reference-article
title: Multiboot Standard
date: 2015-09-03 16:16:00
categories: [ docs, reference ]
---

# Introduction

The Multiboot standard is intended to create a common interface between operating systems and bootloaders allowing multiple different OS'es to be booted by the same bootloader. It was created to combat the rise of many proprietary operating systems, each of which came with its own bootloader which meant none of them could be dual-booted (or more generally multi-booted). 

Multiboot itself is only a standard and does not specify how multiple OS'es can be placed onto a single disk in a computer. It specifies only the interface between the OS and the bootloader. Multiboot is primarily an x86 standard though some variants for other architectures may be floating around online. 

## Scope of this article

This article will cover the multiboot standard, how to use it, how it links with the rest of the system and tools for creating bootable disks for multiboot compliant OS'es. This article will only look briefly at the wider, general topic of multiple booting.

---

# History

The Multiboot standard was first developed by the Free Software Foundation in 1995 and the reference implementation for the standard is part of the GNU GRUB bootloader. Version 1 first appeared in 1995 and has evolved since then. At the time of writing (2015-09-03), the latest (official) version was produced in 2009 and is number 0.6.96. Version 0.6.96 is known to work with a wide range of software (and hardware) but specifically the GRUB, GRUB 2 and Syslinux bootloaders all support it (these are probably the most widely referred to amongst the OS Dev community).

There are also references online to Multiboot Version 2 (better known as Version 1.6). However, this is an unofficial version that, while partially supported by GRUB 2, should not be taken for granted. It is unlikely that other bootloaders will support Multiboot 1.6. Multiboot 1.6 is not backwards compatible even to the extent of using different magic values. Judging by the document's copyright (since the change log provides no dated history) the specification was produced in 2010 on behalf of the Free Software Foundation but is part of the "unofficial" section of the GNU project. 
  
---

# Overview

## What is Multiboot?
Multiboot is a standard (also known as a specification) for creating a common interface between operating systems and bootloaders. It allows a single bootloader to load many different operating systems which makes life easier for both users and developers when it comes to installing operating systems. Also, it allows multi operating systems to be installed in a single computer (potentially on a single disk or multiple disks). It also makes creating bootable disk images and bootable USB sticks much easier.

The reference implementation for multiboot is part of the GNU GRUB bootloader but Multiboot is also supported by other popular bootloaders such as Syslinux. Multiboot specifies a header structure which the OS contains to identify it and to specify configuration options that are used by the bootloader to set up the system prior to starting the OS. Multiboot also includes an information structure which is passed to the OS by means of a pointer in a register. The info structure provides useful information about the host system such as available (physical) memory size.

## What is multi-booting/multiple booting?
Multi-booting, properly known as multiple booting, in general is the act of putting multiple operating systems side-by-side in a single computer. Often this achieved by partitioning the primary hard drive and putting each operating system in its own partition. At runtime (i.e. when the computer powers on) one of the operating systems can be chosen by the user to run for that session. It is logically impossible (or at least practically infeasible) to execute two operating systems simultaneously within a single computer. 

A common short-hand term is dual-booting which refers to the specific situation in which exactly two operating systems are installed.

## How does multiple booting link to partitioning?
In order for multiple booting to work, the operating systems (including their file systems) must be kept entirely separate. For this reason, separate installations of operating systems (even of the same version) are placed in separate partitions which allows them to have independent file systems (which may be of completely different formats). For this reason, a disk's partition table must be a standard format so that bootloaders can understand it and find bootable partitions (i.e. partitions flagged as containing an operating system). The two most common partitioning schemes (of any system) are GPT and MBR (& EBR), which are discussed separate articles.

## Why was Multiboot created?
Just read the Background section of the [Multiboot specification](https://www.gnu.org/software/grub/manual/multiboot/multiboot.html#Overview).

## How does Multiboot work?
Multiboot works in two parts. The first part is data which the OS must provide for the bootloader. The second part is data and configuration the bootloader must provide for the OS (along with load and starting the OS). 

The data the OS must provide comes in the form of a header. The header can appear anywhere in the first 8192 bytes of the OS image, provided the given file format allows detection of its location. A.out and ELF formats are both compatible with this. Convention (and possibly best practice) is for the header to appear at the very start of the OS. The header must be 32-byte aligned. More details can be found below and in the specification. The header contains the Multiboot Signature and checksum (consisting of two magic values) along with various configuration including the entry point of the OS, the graphics mode the bootloader should initialise the system to and various other optional configs that can be enabled by the flags.

The data the bootloader must provide the OS comes in the form of a structure passed by a pointer in a general purpose register. The structure contains useful information such as the amount of lower and upper memory. The bootloader is also required to configure aspects of the systems (such as the graphics mode) according to what the OS requested in the header. 

All the implementation of this is system is left to the individual operating systems and bootloaders. However, to avoid small discrepancies, the reference implementation is included in the GRUB bootloader (which is part of the GNU project and also maintained by the Free Software Foundation). 

---

# Software

## Overview

To use multiboot you need only include the header as part of your OS at the start of the .text section. Details of the header format and values are suitably described in the specification to make implementation easy. However, a basic, default configuration is provided below to help get you started. 

Once your OS has booted you need to do two things. The first is to check that the bootloader was multiboot compliant (since a non-compliant loader could still have tried to start your OS). This is done by verifying the EAX register is set to the magic value. The second task is to load any information from the multiboot info structure. The code provided below doesn't bother checking the Flags value and simply assumes the memory values will be available.


## Technical details

The following are some of the more common values you will require to implement the Multiboot specification:

| Name | Value | Description |
|:----------|:---------|:----------------|
| Signature | 0x1BADB002 (464367618 decimal) | The magic value for the multiboot header's signature field. |
| Flags | 0x3 (3 decimal) | Specifies page-aligned (4KiB aligned) loading and requirement of mem_* fields. |
| Checksum | 0xE4524FFB (-464367621 decimal) | The checksum value for the above configuration. It is the sum of the magic and flags fields provided above. |

## Implementation details

The following is a sample multiboot header written in NASM syntax:

``` x86asm
MultibootSignature dd 464367618
MultibootFlags dd 3
MultibootChecksum dd -464367621
```

The following is a sample for checking the multiboot signature, handling it if it is wrong and loading data from the multiboot info structure. It was written for the configuration provided above which has flags set the require the bootloader to always include the Upper and Lower memory information in the multiboot info structure.

``` x86asm
; Put this somewhere in memory away from the code/.text
section .bss
MultiBootInfo_Structure dd 0
MultiBootInfo_Memory_High dd 0
MultiBootInfo_Memory_Low dd 0

; Put this in code/.text as the very first thing your OS does
section .text
Kernel_Start:
  cli ; Clear interrupts to prevent interruption of the following critical sequence
	
	; MultiBoot compliant loader provides info in registers: 
	; EBX=multiboot_info 
	; EAX=0x2BADB002	- check if OS was loaded by a Multiboot compliant bootloader 
	;					        - if true, continue and copy multiboot info
	mov dword ecx, 0x2BADB002
	cmp ecx, eax
	jne (Kernel_Start_HandleNoMultiboot - KERNEL_VIRTUAL_BASE)
	
  ; Load information from the MB structure 
  ;   Note: This ought to check the Flags word first to verify what data is available
  ;         but the configuration requires the bootloader to include the values used.
  
  ; Store pointer to multiboot info structure
	mov dword [MultiBootInfo_Structure - KERNEL_VIRTUAL_BASE], EBX
  ; Move to 2nd dword (i.e. skip over Flags value)
	add dword EBX, 0x4
  ; Load size of lower memory
	mov dword EAX, [EBX]
  ; Store size of lower memory
	mov dword [MultiBootInfo_Memory_Low - KERNEL_VIRTUAL_BASE], EAX
  ; Move to 3rd dword
	add dword EBX, 0x4
  ; Load size of upper memory
	mov dword EAX, [EBX]
  ; Store size of upper memory
	mov dword [MultiBootInfo_Memory_High - KERNEL_VIRTUAL_BASE], EAX
	  
jmp Kernel_Start_HandleNoMultiboot_End ; Skip over this code - we don't want to run it by accident!
Kernel_Start_HandleNoMultiboot:

; Displays a warning message to the user saying "No multiboot" indicating the multiboot signature
; (which should have been in EAX) was not detected so we don't think we have a valid boot setup
; so we are aborting the boot to avoid damage
	
	; Output following text to first bit of vid mem
	; N	  o      M  u    l   t   i   b   o  o   t
	; 78 111 32 109 117 108 116 105 98 111 111 116
	mov byte [0xB8000], 78
	mov byte [0xB8002], 111
	mov byte [0xB8004], 32
	mov byte [0xB8006], 109
	mov byte [0xB8008], 117
	mov byte [0xB800A], 108
	mov byte [0xB800C], 116
	mov byte [0xB800E], 105
	mov byte [0xB8010], 98
	mov byte [0xB8012], 111
	mov byte [0xB8014], 111
	mov byte [0xB8016], 116

	; Set the colour of the outputted text to:
	; Red background (0x4-), 
	; White foreground (0x-F)
	mov dword eax, 0x4F
	mov byte [0xB8001], al
	mov byte [0xB8003], al
	mov byte [0xB8005], al
	mov byte [0xB8007], al
	mov byte [0xB8009], al
	mov byte [0xB800B], al
	mov byte [0xB800D], al
	mov byte [0xB800F], al
	mov byte [0xB8011], al
	mov byte [0xB8013], al
	mov byte [0xB8015], al
	mov byte [0xB8017], al

	cli ; Prevent any more interrupt requests re-awakening us
	hlt ; Halt the OS / execution / etc.
	jmp Kernel_Start_HandleNoMultiboot ; Just in case...

Kernel_Start_HandleNoMultiboot_End:
  ; Continue execution here
```

## Alternatives
As far as alternative go, there aren't any. Your choices are basically to either use multiboot or use a specialist/proprietary/roll-your-own bootloader. However, all of these alternatives will likely be incompatible with other OS'es so you want be able to dual (or multiple) boot your machine with your OS and writing your own bootloader is fraught with difficulties.

## Tools

### Creating bootable USB sticks
There are lots of (free) tools for creating bootable USB sticks which include support for Syslinux, GRUB (1 & 2) and other bootloaders (often they also include automatic download and installation of popular Linux distributions). One such tool which is very successful for Windows users is [YUMI](http://www.pendrivelinux.com/yumi-multiboot-usb-creator/). 

---

# Further Reading

- [GNU.org - Multiboot Specification v0.6.96](https://www.gnu.org/software/grub/manual/multiboot/multiboot.html)
- [OSDev.org - Multiboot](http://wiki.osdev.org/Multiboot)
- [Wikipedia.org - Multiboot Specification](https://en.wikipedia.org/wiki/Multiboot_Specification)
- [BrokenThorn.com - Multiboot](http://www.brokenthorn.com/Resources/OSDevMulti.html)
- [Unofficial GNU - Multiboot Specification v1.6](http://nongnu.askapache.com/grub/phcoder/multiboot.pdf)
- [Wikipedia.org - GNU Savannah (Unofficial GNU)](https://en.wikipedia.org/wiki/GNU_Savannah)
- [GNU.org - GRUB](https://www.gnu.org/software/grub/)
- [Syslinux.org](http://www.syslinux.org/)
- [PendriveLinux.com - YUMI](http://www.pendrivelinux.com/yumi-multiboot-usb-creator/)
- [GNU.org - GNU History (/Overview of GNU)](https://gnu.org/gnu/gnu-history.html)

*[OS]: Operating System
*[GNU]: GNU's Not Unix
---
layout: reference-article
title: Global Descriptors Table
date: 2015-08-27 21:24:00
categories: [ docs, reference ]
parent_name: Memory
description: Describes the x86 Global Descriptors Table and how to use it.
---

# Introduction

The Global Descriptor Table is a construct used by the x86 processor to configure segmented virtual memory. It came long before paging was added to the architecture and as such is a legacy piece of configuration. Very few protected-mode operating systems (which most are) use segmentation. Instead paging is favoured. However, because the x86 processor retains backwards compatibility, in order to use paging, basic segmentation must still be configured. The GDT also contains a TSS entry which has to be configured for task switching.

The GDT is also complemented by the Local Descriptor Table, which is used when utilising hardware task switching. However, hardware task switching is generally not used these days (in favour of software-driven methods which are often faster and allow more control) and thus LDT is defunct. The LDT does not need to be configured - ever - if hardware task switching isn't being used.

## Scope of this article

This article will cover the technical aspects of the Global Descriptor Table but will not explain segmentation, virtual memory or paging. Those are explained in the Virtual Memory and other related articles. Though the LDT is mentioned, it is outside the scope of this article. 

---

# History

The Global Descriptor Table comes from the days when segmented virtual memory was the only way to provide a virtual address space. The purpose of virtual address spaces (at that stage) was to provide security. It allows the kernel to restrain processes and prevent them from overwriting each other (or the kernel). 

Segmentation provided two levels of improvement. One was for security meaning processes couldn't read or write each other meaning they couldn't interfere (unless the kernel set up overlapping segments to create shared memory). The second level was stability improvement. Rogue or bug-prone programs couldn't accidentally trash other program's (or the kernel's) memory if they started overrunning memory. If a program started to do so, it could be detected and stopped as a contained unit before it brought down the rest of the system.

Since segmentation was introduced, paged virtual memory has been added as well. The world has been divided ever since. Some people swear by segmentation, others by paging. Unfortunately, x86 is not a good example of either. Its combined system of the two is unique and, as a result, cumbersome. Most operating systems now just use paging but global segmentation still has to be configured. 

---

# Overview

## What is the GDT?
The Global Descriptor Table is a table of segment descriptors. It contains three types of descriptor: the NULL descriptor (which must always be the first entry), Call Gate descriptors (which are considered as the "normal" type of descriptor) and the Task State Segment (TSS) descriptor.

## What is a segment?
A segment is a designated area of the address space. It consists of a start address and a length which defines the area the segment covers. Segments have access permissions which prevent programs of lower privilege levels from accessing the memory within them.

Segments are also designated as either data or code segments. Data segments can be read or written but not executed (i.e. a data segment selector can go in any segment register except the Code Segment Selector register), code segments can only be executed (i.e. a code segment selector can only go in the Code Segment Selector register). 

Segments can overlap. This means an area of memory can be designated as both data and code and/or be accessible to one or more privilege levels. If segmentation is being avoided (in favour of paging) then it is standard practice to configure four segments. Two code segments and two data segments. One of the two has Descriptor Privilege Level 0 (i.e. Ring 0 meaning Kernel-mode access) and the other with DPL 3 (i.e. Ring 3 meaning User-mode access). Almost no modern operating systems use Rings 1 and 2.

## What is a descriptor?
A descriptor is an entry in the Global Descriptor Table. It contains all the information about the start (/offset) address of segments and their length, their access levels and types along with some other information.

## What is a selector?
A selector, properly known as a segment selector, is an offset in the Global Descriptor Table that specifies a descriptor to use. Selectors are put into the Segment Selector registers. 

A selector is often referred to as an index into the GDT but this is misleading as it makes them sound like an array indices (0, 1, 2, etc.) but they aren't. A selector is the number of bytes from the start of the table to the start of the descriptor (/table entry). For a standard GDT, this means the selector for the second entry (the first valid one after the NULL descriptor) has selector 0x10 (16). 

## What are segment selector registers?
The segment selector registers are registers used by the processor to specify segments for different purposes. The original idea was that different segments would be used for code, private program data, kernel code/data, shared memory and some general purposes. Nowadays, the segment selector registers are just loaded with values to give the most freedom.

After the GDT has been loaded, the segment selector registers must be updated. This just means copying in the new value; except for the Code Segment selector. The CS register cannot be updated directly. It is updated by performing a far-jump to the code which is inside the desired code segment.

The following table lists the segment selector registers:

| Acronym | Name | Description |
|:-------------:|:----------|:----------------|
| CS | Code Segment | Specifies the code segment selector for the executable segment for the current program (inc. the kernel). |
| DS | Data Segment | Specifies the data segment selector for the private data of the current program. |
| SS | Stack Segment | Specifies the data segment selector for the stack memory of the current program. |
| ES | Extra Segment | Specifies a data segment selector for shared memory or kernel memory depending on usage. |
| FS | General purpose segment | Specifies a data segment for any use. |
| GS | General purpose segment | Specifies a data segment for any use. |

## What is a Descriptor Privilege Level?
A descriptor has a privilege level which corresponds the Rings 0 to 3. Rings are a protection mechanism within the x86 processor. 

Ring 0 is the most privileged and can execute any instruction and perform any memory/IO operations it likes. This is called kernel-mode. Rings 1 and 2 are lower privilege and have reduced IO and execution rights. They are broadly unused nowadays. Ring 3 is the user-mode ring and is the most restricted. It cannot perform special instructions (such as loading a new GDT or IDT) and cannot access IO ports arbitrarily. 

## What is the Task State Segment (TSS)?
The Task State Segment is a special segment descriptor which holds information about the currently executing task. It does not need to be configured for a basic OS but is required as soon as the OS starts performing kernel to user or user to kernel mode switches (i.e. as soon as the OS starts doing some proper task management. Tasks are processes or threads). 

The TSS is primarily used for hardware task switching but even for software task switching a basic one is necessary. The TSS (for software task switching) is what allows jumps between Ring 0 and 3 and back again.

## Why do segments exist?
Segments exists because they are one of the two main ways of constructing a virtual address space. Back when processor hardware was limited, memory was small and the idea of saving virtual memory to a hard disk was only for the most expensive systems, segmented memory was the most effective way of creating a virtual address space. 

To this day, some other processor architectures use segmentation but they do it rather better than x86 (or simply do not offer paging). Since x86 introduced, segmentation has been largely redundant. It is kept for backwards compatibility.

## Why is there a NULL descriptor?
The NULL descriptor is a protection mechanism. If any of the segment registers ever contains a selector value of zero, it will refer to the NULL segment which will cause an immediate General Protection Fault (exception/interrupt). This allows the kernel to catch rogue or buggy processes which might intentionally or accidentally try to misconfigure the segment selectors.

## Why do I need to configure the GDT at all?
The GDT must be configured as the processor expects it to exist. A bootloader will put in its own GDT, but the OS will have little to no idea where the bootloader's GDT is in memory. As such, it runs the risk of overwriting it. Destroying the GDT results in an immediate triple fault. To avoid this, the OS must configure its own GDT. This also allows the kernel to create a TSS entry, which bootloaders are unlikely to include.

## Why can't I just use real/physical memory?
You could, but your system would the least secure, least stable thing in the world. Also, if you start running out of memory or want to load dynamically linked executables, you'll run into problems.

## Why can't I just use paging?
You can't "just use paging". Everything runs through the GDT/segmentation model, so it has to be configured. Standard practice is to create segments which cover the entire address space and make enough segments to cover user/kernel mode and code/data combinations. This makes the memory a full-coverage, flat memory model which makes it invisible to the paging system.

## How do I create a GDT?
You allocate some memory as an array of GDT Entry structures (which can be statically allocated in assembly code) and then fill in the entries. For statically allocated GDTs it is possible to define the values in the code without needing a separate method to fill them in.

## How does the CPU know where the GDT is?
The kernel provides the processor with the address of (i.e. a pointer to) a GDT Pointer structure. The GDT Pointer structure is a special structure which must also be allocated in memory. It consists of a two-byte length specifying the length (in bytes) of the GDT Table followed by 4-bytes for the address of the GDT Table itself.

## What's the Local Descriptor Table?
The LDT is much like the GDT in that it specifies memory segments. However, it is designed to be updated with every task switch. As such, many LDT tables can be created and when a task switch occurs, the hardware can automatically switch to the correct LDT for that task.

## What is different about the LDT?
The LDT can be changed very often and is loaded by specifying a GDT Selector. The GDT Entry for that selector is what specifies the offset and length of the LDT in question. To select a different LDT is just a case of switching the selector. 

## Why would I want to use the LDT?
Nowadays you wouldn't. Paging and software task switching have made the LDT totally redundant. You do not even need to configure an LDT to be able to use paging and hardware or software task switching.

---

# Software

## Overview
GDT configuration is relatively simple. The GDT should be set up once at the start of the kernel (usually not long after switching to protected mode). For systems using paging and software task switching, it is possible to set up the GDT once and never touch it again while the system is running.

To configure the GDT requires three steps:

1. Create the GDT table and fill in entries. (This is done statically by hard-coding in assembly or dynamically using functions).
2. Create the GDT Pointer and update the processor
3. Update the segment selector registers

## Technical details
**Enumerations, classes, other such details**

##### GDT
The GDT is an array of GDT Entries. The first entry has to be the NULL descriptor. Other entries can appear in any order (your kernel just needs to keep track of which is which for use when updating the segment selector registers).

##### GDT Entry Format / Descriptor Format
The Limit (/size) value of a segment is a 20-bit value. This can either be a 20-bit address or the top 20-bits of a 32-bit address. Which type is defined by the Granularity bit of the Flags bits.

| Offset (bits) | Size (bits) | Name | Description |
|:-----------------:|:--------------:|:----------|:----------------|
|  0 - 15   | 16      | Limit  | Low two bytes of the segment limit value. The Limit specifies the size of the segment |
| 16 - 39   | 24      | Base  | Low three bytes of the segment base address. The Base Address specifies the offset/address of the start of the segment. |
| 40 - 47   | 8       | Access | The access byte. Specifies options and permissions for the segment. See table below. |
| 48 - 51   | 4       | Limit  | Bits 16 to 19 of the Limit value i.e. the highest 4 bits of the Limit. |
| 52 - 55   | 4       | Flags  | Flag bits. Specifies configuration for the descriptor. See table below. |
| 56 - 63   | 8       | Base  | The highest (4th) byte of the segment base address. | 

##### GDT Entry Flag bits

| Bit | Name | Meaning |
|:-----:|:----------|:-------------|
| 0  | -    | Reserved, always zero. |
| 1  | -    | Reserved, always zero. |
| 2  | Size  | The size bit specifies whether code accessing the segment is in 16-bit or 32-bit protected mode. 0=16-bit, 1=32-bit. This is ignored in real-mode. |
| 3  | Granularity | The granularity bit specifies whether the Limit value is defined in byte or 4KiB units. Byte units makes the limit a 20-bit address. 4KiB units makes the limit a 32-bit address where the lowest 12 bits are always zero. For a standard full-coverage, flat memory model setup, this bit should be set so that segments cover the entire 32-bit address space. |

##### GDT Entry Access byte

| Bit(s) | Acronym | Name | Meaning |
|:-----:|:----------|:-------------|
| 0 | Ac | Accessed | Whether the segment has been accessed or not. Set to zero by programmer/kernel. Set to one by CPU when segment is accessed. |
| 1 | R/W | Read/Write | Whether the segment is unreadable, read-only or read/write. For code segments, this bit specifies whether the segment is unreadable (by the program, set to 0) or read-only (set to 1. Code segments can never be writeable). For data segments, this bit specifies whether the segment is read-only (set to 0) or read-write (set to 1). |
| 2 | DC | Direction/Conforming | Direction for data segments : 0 segment grows up, 1 segment grows down. Segments which grow down effectively have the limit subtracted from the base instead of adding it. Conforming for code segments. 0 indicates DPL and more privileged DPLs can execute the segment. 1 indicates only the specified DPL can execute the segment. |
| 3 | Ex | Executable | Whether the segment can be executed or not i.e. 1 for code segments, 0 for data segments. This also specifies the type of the segment. |
| 4 | - | Reserved, always 1. |
| 5,6 | Privl | Privilege | Value form 0 to 3 corresponding to Rings 0 to 3 (0 = Kernel-mode, 3 = User-mode. 3 is lowest privilege). |
| 7 | Pr | Present | Whether the segment is present or not. Must be 1 for any selectors you intend to actually use. |

##### GDT Pointer structure 

| Byte(s) | Size | Name | Description |
|:----------:|:--------|:----------|:----------------|
| 0,1    | 2   | Size  | Specifies the size of the GDT minus 1. The subtracted 1 is because the processor adds the size of the Offset value and treats that as the maximum (inclusive) address of the table. i.e. the address of the last byte in the table. |
| 2-5    | 4   | Offset | The linear address (/offset) of the GDT table. Paging does apply to this address. |

### Task State Segment
The Task State Segment is not really a segment. It is actually a special structure which holds all the information about the state of a task. A task is a thread (or process, depending on definition of terminology). Multiple TSS structures are used for hardware task switching but for software task switching only one or two are needed. In software task switching, the TSS allows for switching between rings.

A TSS descriptor uses the base address to specify the start of a TSS structure and the Limit to specify the size of the structure. The first bytes of the TSS have defined uses. Any additional bytes are free for the kernel to use (or may have defined uses in future versions of the x86 architecture). The minimum size of a TSS structure is 104 bytes.

##### TSS Structure

| Offset | Bits 31-16 | 15-0 |
|:---------:|:----------------:|:--------:|
| 0x00 | Reserved | LINK |
| 0x04 | ESP0 | |
| 0x08 | Reserved | SS0 |
| 0x0C | ESP1 | |
| 0x10 | Reserved | SS1 |
| 0x14 | ESP2 | |
| 0x18 | Reserved | SS2 |
| 0x1C | CR3 | |
| 0x20 | EIP | |
| 0x24 | EFLAGS | |
| 0x28 | EAX | |
| 0x2C | ECX | |
| 0x30 | EDX | |
| 0x34 | EBX | |
| 0x38 | ESP | |
| 0x3C | EBP | |
| 0x40 | ESI | |
| 0x44 | EDI | |
| 0x48 | Reserved | ES |
| 0x4C | Reserved | CS |
| 0x50 | Reserved | SS |
| 0x54 | Reserved | DS |
| 0x58 | Reserved | FS |
| 0x5C | Reserved | GS |
| 0x60 | Reserved | LDTR |
| 0x64 | IOPB offset | Reserved |

Further explanation of the TSS is given in the separate article discussing multitasking.

## Implementation details

Interrupts should be disabled using the CLI instruction prior to running any of the example code.

### 1. Allocating &amp; constructing the GDT

The following code samples demonstrate how to allocate and configure a full-coverage, flat memory model GDT which is suitable for use with paging and for multitasking (as it includes a TSS entry).

NASM assembly code for allocating GDT and TSS:

``` x86asm
; This is the GDT table pre-filled with the entries required to make the entire address space accessible 
;   from user and kernel mode for both data and code.
GDT_Contents:
  db 0, 0, 0, 0, 0, 0, 0, 0            ; Offset: 0  - Null selector - required 
  db 255, 255, 0, 0, 0, 0x9A, 0xCF, 0  ; Offset: 8  - KM Code selector - covers the entire 4GiB address range
  db 255, 255, 0, 0, 0, 0x92, 0xCF, 0  ; Offset: 16 - KM Data selector - covers the entire 4GiB address range
  db 255, 255, 0, 0, 0, 0xFA, 0xCF, 0  ; Offset: 24 - UM Code selector - covers the entire 4GiB address range
  db 255, 255, 0, 0, 0, 0xF2, 0xCF, 0  ; Offset: 32 - UM Data selector - covers the entire 4GiB address range
  db 0x67,  0, 0, 0, 0, 0xE9, 0x00, 0  ; Offset: 40 - TSS Selector - Pointer to the TSS 

TSS:
  TIMES 104 db 0
TSS_POINTER equ (TSS - KERNEL_VIRTUAL_BASE)  ; Physical address of TSS
```

### 2. Allocating &amp; filling the GDT Pointer

NASM assembly code for allocating GDT pointer:

``` x86asm
; Size - Change iff adding/removing rows from GDT contents
; Size = Total bytes in GDT - 1
GDT_Pointer db 47, 0, 0, 0, 0, 0
```

NASM assembly code for configuring the TSS in the GDT:

``` x86asm
; Setup the primary TSS Selector to point to the TSS
; Only need to enter the base address. Everything else is setup in the allocations
lea eax, [GDT_Contents+40]
lea dword ebx, [TSS_POINTER]
mov byte [eax+2], bl
shr ebx, 8
mov byte [eax+3], bl
shr ebx, 8
mov byte [eax+4], bl
shr ebx, 8
mov byte [eax+7], bl
```

### 3. Loading GDT Pointer to CPU Register

NASM assembly code for filling in GDT Pointer and telling the CPU about it:

``` x86asm
; Fill in GDT Pointer structure
mov dword [GDT_Pointer + 2], GDT_Contents
; Tell CPU about GDT
mov dword eax, GDT_Pointer
lgdt [eax]
```

### 4. Updating Segment Selector registers

NASM assembly code for reloading segment registers:

``` x86asm
LoadSegments:
  mov dword eax, 0x10  ; Load data segment selector for KM Data Segment descriptor
  mov word ds, eax     ; Load data segment selector registers
  mov word es, eax
  mov word fs, eax
  mov word gs, eax
  mov word ss, eax
  ; Force reload of code segment
  jmp 8:FlushCS        ; Far jump to next line of code which causes CS reload.
FlushCS:
  nop
  ; Code continues here
```

## Alternatives
The alternative to segmentation is paging. Paging is the most commonly used virtual memory system in modern operating systems but x86 still requires full-coverage, flat memory model segmentation to be set up before paging can be used.

## Compatibility

Segmentation is not supported in 64-bit (long) mode but must still be configured. Segmentation can (and must) be enabled to be able to enter long mode. Details of how this works can be found in the [Intel Architecture Manuals](http://www.intel.com/content/www/us/en/processors/architectures-software-developer-manuals.html).

Segmentation and paging are compatible and, uniquely to x86, can be used simultaneously. However, such a hybrid system is considered monstrous and the software required to manage it would almost certainly perform worse than using one exclusively. 

---

# Example Code

## Overview
The FlingOS Global Descriptors Table code is mainly contained in its Pre-requisite (i.e. initialisation) ASM code. The main files are available at: [https://github.com/FlingOS/FlingOS/blob/master/Kernel/Kernel/ASM/Descriptor%20Tables/GDT.x86_32.asm](https://github.com/FlingOS/FlingOS/blob/master/Kernel/Kernel/ASM/Descriptor%20Tables/GDT.x86_32.asm) and [https://github.com/FlingOS/FlingOS/blob/master/Kernel/Kernel/ASM/PreReqs/MultibootSignature.x86_32.asm](https://github.com/FlingOS/FlingOS/blob/master/Kernel/Kernel/ASM/PreReqs/MultibootSignature.x86_32.asm) (the latter contains the static allocations for the GDT).


---

# FAQ & Common Problems

## No NULL descriptor
If your GDT doesn't contain a NULL descriptor, the processor will have a hissy fit and crash. Include one. It isn't hard, costs nothing and has been mentioned many times above.

## Sporadic failure (not disabled interrupts)
If you forget to disable interrupts before updating the GDT or segment selectors, then expect the code to crash. Particularly if you're altering the TSS (which, by implication, you will be if you update the GDT).

## Nothing changed? (Not reloaded segment registers)
If you updated the GDT and nothing happened, you probably forgot to reload the segment registers. See the sample code above.

## General protection faults (GPFs)
If you're getting general protection faults then either the segment selectors are set to the NULL segment (which is always invalid) or you set one or more of the selectors to a segment which the current execution or stack is not allowed to access (e.g. setting the stack segment to a kernel-mode-only segment when running in user mode).

---

# Further Reading

- [Intel Architecture Manuals]( http://www.intel.com/content/www/us/en/processors/architectures-software-developer-manuals.html)
- [Wikipedia.org - Global Descriptor Table](https://en.wikipedia.org/wiki/Global_Descriptor_Table)
- [Wikipedia.org - Task State Segment](https://en.wikipedia.org/wiki/Task_state_segment)
- [OSDev.org - Global Descriptor Table](http://wiki.osdev.org/Global_Descriptor_Table)
- [OSDev.org - Segmentation](http://wiki.osdev.org/Segmentation)
- [OSDev.org - Segment Limits](http://wiki.osdev.org/Segment_Limits)
- [OSDev.org - GDT Tutorial](http://wiki.osdev.org/GDT_Tutorial)
- [OSDev.org - Task State Segment](http://wiki.osdev.org/Task_State_Segment)
- [JamesMolloy.co.uk - 4. The GDT &amp; IDT](http://www.jamesmolloy.co.uk/tutorial_html/4.-The%20GDT%20and%20IDT.html)
- [OSDever.net - GDT](http://www.osdever.net/bkerndev/Docs/gdt.htm)
- [CS.CMU.edu - Segments](https://www.cs.cmu.edu/~410/doc/segments/segments.html)
- [MIT.edu - Task State Segment](http://pdos.csail.mit.edu/6.828/2014/readings/i386/s07_01.htm)
- [MIT.edu - TSS Descriptor](http://pdos.csail.mit.edu/6.828/2014/readings/i386/s07_02.htm)
- [Wikibooks.org - Global Descriptor Table](https://en.wikibooks.org/wiki/X86_Assembly/Global_Descriptor_Table)
- [Internals.com - Protected-Mode Memory Management](http://www.internals.com/articles/protmode/protmode.htm)

*[acronym]: details

---
layout: reference-article
title: Interrupt Descriptors Table
date: 2015-08-28 16:54:00
categories: [ docs, reference ]
parent_name: Interrupts
---

# Introduction

The Interrupt Descriptors Table is a construct used by the x86 processor (when it is in protected mode) to configure interrupt handlers and the state of execution during those handlers. It was introduced as a replacement to the Interrupt Vectors Table when Protected Mode was added to the architecture. The IDT is more extensive than the IVT and allows for more types of interrupt handler. The IDT is used to configure ISR handlers for exceptions, software interrupts and IRQs. 

## Scope of this article

This article will detail only the Interrupt Descriptors Table with brief mentions of the Interrupt Vectors Table (the Real-mode almost-equivalent). This includes IDT setup but not how to handle interrupts (ISRs). Details of interrupts in general, ISRs and IRQs are given in their respective articles. 

---

# History

Originally the x86 only had one mode (which later became known as real mode). Real-mode had no memory protection nor execution protection (in the form of privilege rings). As such, only a basic vectors table was required to specify how to handle interrupts. Protected mode introduced memory protection, privilege rings and a few other forms of stability/security features. As a result, the interrupt descriptors table was created to handle what happens when a low-privilege (high-ring e.g. ring 3) process is interrupted and the handler must run in high privilege (low-ring e.g. ring 0) state (usually inside the kernel). 

Alongside the security features, the interrupt gate, trap gate and task gate forms of interrupt handlers were introduced. These are used to handle different types of interrupts such as exceptions, software interrupts and IRQs. Information about this is given in separate Interrupts, ISRs and IRQs articles. 

---

# Overview

## What is the Interrupts Descriptor Table?
The Interrupt Descriptors Table is a table of descriptor structures which specify the type of handler for each interrupt numbers, the location (/address) of the method to call when a particular interrupt occurs and other information such as the privilege level required to execute the interrupt handler. The IDT is used only when the processor is in protected mode. Once configured, the IDT overrides the IVT and prevents access to BIOS interrupt calls.

## What is the Interrupt Vectors Table?
The Interrupt Vectors Table is the original interrupts table which is the real-mode equivalent of the IDT. It consists of only 4-byte, real-mode pointers with no configuration for privilege levels or gate types (since real-mode has no such concepts). The IVT is used by the BIOS and prior to switching to protected mode, interrupt 10 is commonly used for BIOS Video functions.

## What are Interrupt Service Routines?
Interrupt Service Routines are the methods which are jumped to by the processor to handle interrupts. IRQs map to interrupt numbers and thus to ISRs. More details are provided in the Interrupts, ISRs and IRQs articles.

## What are Interrupt Requests?
Interrupt Requests (IRQs) are interrupts (which map to ISRs) which are triggered by external devices. IRQs is often used to refer to the methods which are jumped to by the processor to handle device notification. IRQs map to interrupt numbers and thus to ISRs. More details are provided in the Interrupts, ISRs and IRQs articles.

## What are the types of interrupt gate?
There are three types of interrupt gate known as: Interrupt Gates, Trap Gates and Task Gates.

Interrupt and Trap gates are the most commonly used. An IDT entry which is an interrupt or trap gate uses a segment selector and offset as a pointer to the interrupt handler method. The difference between an interrupt gate and a trap gate is that an interrupt gate will disable maskable interrupts prior to the handler being called where as a trap gate won't. This makes interrupt gates best for handling IRQs and trap gates best for software or exception interrupts. 

Task gates are a special type of gate used for hardware task switching. An interrupt where the IDT entry specifies a task gate does not a handler, it just causes the processor to store current process state in the TSS and then switch to the next TSS in the linked list. Hardware task switching is not widely used (because software task switching can be faster and allows you to do more varied processing) and so Task Gates are not widely used.

## Why are there two different tables?
The IVT and IDT tables exist because of the difference in requirements between real and protected mode. Intel decided to retain backwards compatibility in their hardware so the IVT/real-mode features had to be retained. The IVT does not, however, provide enough space to accommodate all the required features for protected mode and thus the IDT was created for protected-mode interrupts.

## How do I create an IDT?
You allocate some memory as an array of IDT Entry structures and then fill in the entries. It is common to allocate space in assembly code but use a function to actually set the entry values. This is because the IDT contains 256 entries which is a lot of data to try and set by hand. It is also possible to use pre-processor macros to create the setup code for large blocks of the table.

## How does the CPU know where the IDT is?
The kernel provides the processor with the address of (i.e. a pointer to) a IDT Pointer structure. The IDT Pointer structure is a special structure which must also be allocated in memory. It consists of a two-byte length specifying the length (in bytes) of the IDT Table followed by 4-bytes for the address of the IDT Table itself. It is exactly the same as the GDT Pointer structure and is used in the same way except the LIDT instruction is used instead of the LGDT instruction.

---

# Software

## Overview

To configure the IDT requires four steps:

1. Disable maskable interrupts (cli)
2. Create the IDT table and fill in entries.
3. Create the IDT Pointer and update the processor
4. Re-enable maskable interrupts (sti)

## Technical details

##### IDT Layout
The IDT is an array of IDT Entries. Each entry's index corresponds to the interrupt number the entry is for. Each entry is 8-bytes in size and there are (always) 256 entries meaning the table is 2048 bytes in size.

##### IDT Entry format

| Name | Bit(s) | Full Name | Description |
|:----------|:-----:|:---------------|:-----------------|
| Offset | 48 - 63 | Offset bits 16 - 31 | High two bytes of the offset (/pointer) to the interrupt handler. Should be set to 0 for Task Gates since it is ignored for them. |
| P | 47 | Present | Set to 0 for unused interrupts. Allows the kernel to simply not configure some interrupt handlers. If a not-present interrupt occurs, a General Protection Fault occurs instead so the GPF handler should never be not-present. |
| DPL | 45 - 46 | Descriptor Privilege Level | Specifies the privilege level required to invoke the interrupt. This only really applies for software interrupts. If the code invoking the interrupt is running in a privilege level less than or equal to the DPL, the interrupt happens. Otherwise, a GPF occurs. This is so hardware and CPU interrupts can be protected from being invoked from user-mode. Current privilege level is known as CPL and comes from the ring-mode in the CS selector register. |
| S | 44 | Storage Segment | Always 0. This name is found from other sources online. The Intel Architecture Manual does not give this bit a name but has it set to zero for every format of descriptor. I can only presume this bit was made obsolete by Intel at some stage. |
| Type | 40 - 43 | Gate Type | See table below for possible gate types. |
| 0 | 32 - 39 | Unused | Always 0. |
| Selector | 16 - 31 | Selector 0 - 15 | Segment Selector for destination code segment (i.e. selector for Code Segment that contains the interrupt handler). In most systems this will always be 0. |
| Offset | 0 - 15 | Offset 0 - 15 | Low two bytes of the offset (/pointer) to the interrupt handler. Should be set to 0 for Task Gates since it is ignored for them. |

##### Gate Types

| Binary Value | Hex Value | Name |
|:------------------:|:---------------:|:----------|
| 0b0101 | 0x5 | 80386 32 bit task gate |
| 0b0110 | 0x6 | 80286 16-bit interrupt gate |
| 0b0111 | 0x7 | 80286 16-bit trap gate |
| 0b1110 | 0xE | 80386 32-bit interrupt gate |
| 0b1111 | 0xF | 80386 32-bit trap gate |

##### IDT Pointer structure

| Byte(s) | Size | Name | Description |
|:----------:|:--------|:----------|:----------------|
| 0,1    | 2   | Size  | Specifies the size of the IDT minus 1. The subtracted 1 is because the processor adds the size of the Offset value and treats that as the maximum (inclusive) address of the table. i.e. the address of the last byte in the table. |
| 2-5    | 4   | Offset | The linear address (/offset) of the IDT table. Paging does apply to this address. |

## Implementation details

Steps one and four are done by the single instructions cli and sti to disable and enable maskable interrupts respectively.

### 2. Allocating &amp; constructing the IDT

NASM code for statically allocating an IDT:

``` x86asm
IDT_Contents: TIMES 2048 db 0        ; Allocate table : 8 * 256 = 2048 bytes
IDT_Pointer db 0xFF, 0x7, 0, 0, 0, 0 ; Size: 0x7FF = 2047 = (8 * 256) - 1
```

NASM code for setting an IDT entry. This block of code can be chained to set sequential entries.

``` x86asm

mov dword ebx, IDT_Contents       ; Load pointer to IDT table

; Set the IDT entry
mov dword eax, Interrupt0Handler  ; Load pointer to interrupt handler
mov byte [ebx], al                ; Set 1st byte of handler pointer
mov byte [ebx+1], ah              ; Set 2nd byte of handler pointer
shr dword eax, 0x10               ; Shift handler pointer 16-bits right
mov byte [ebx+6], al              ; Set 3rd byte of handler pointer
mov byte [ebx+7], ah              ; Set 4th byte of handler pointer
mov word [ebx+2], 0x8             ; Set Code Segment selector to 8 
                                  ;   - see GDT article for matchig GDT setup
mov byte [ebx+4], 0x0             ; Set 5th byte to 0 as required
mov byte [ebx+5], 0x8F            ; Set P=1, DPL=0, S=0, Type=0xE (32-bit trap gate)

add ebx, 8                        ; Move to next IDT entry

```

### 3. Creating IDT Pointer and updating processor

NASM assembly code for filling in IDT Pointer and telling the CPU about it:

``` x86asm
; Fill in IDT Pointer structure
mov dword [IDT_Pointer + 2], IDT_Contents
; Tell CPU about IDT
mov dword eax, IDT_Pointer
lidt [eax]
```

## Alternatives

There are only two real alternatives to interrupts which are polling and hardware-based deactivation/reactivation schemes (which often internally use a similar hardware design to interrupts). Polling is described in more detail in articles about devices where polling is possible.

---

# Example Code

## Overview
TODO

## Download
TODO

---

# FAQ & Common Problems

### How to test IDT : Divide by zero
The IDT can be tricky to set up. The first entry is for Interrupt 0 which is the Divide by Zero exception. It is common, therefore, to write code to cause a divide by zero exception and output text to the screen (using VGA text-mode by writing to 0xB8000 and onwards) to test whether the interrupt occurs properly. However, causing a divide by zero exception within the kernel is an irrecoverable error (unless you have a try-catch system) so don't expect to be able to continue execution after the interrupt using IRet unless you change the return address (which is what a try-catch system usually does).

### How to test IRQs : Keyboard or Timer
IRQs add extra complications (particularly with the PIC End of Interrupt command) so it is common to use the keyboard or timer interrupts to test IRQ setup. The keyboard interrupt has the advantage that you can control when it happens (by pressing a key) and it requires no configuration beyond enabling the interrupt in the PIC mask. However, during the interrupt the scancode must be read in order for further keyboard IRQs to occur. The timer interrupt requires more initial configuration (which can go wrong/cause more problems) but doesn't require special processing during the interrupt handler.

### I get General Protection Faults endlessly
You probably have the Segment Selector, DPL or GDT set incorrectly. Make sure you're not setting selectors to zero since that is the null segment (except the Storage Segment value which should always be zero). Also ensure the GDT is in the order you expect and that your segment selectors are pointing to kernel-mode segments (privilege level 0). 

## I get Double or Triple Faults endlessly
You have misconfigured an interrupt handler and, if you are getting triple faults, your double fault handler is misconfigured. The diagnosis is probably the same as endless GPFs.

---

# Further Reading

- [Intel Architecture Manual](http://www.intel.co.uk/content/dam/www/public/us/en/documents/manuals/64-ia-32-architectures-software-developer-manual-325462.pdf)
- [Wikipedia.org - Interrupt Descriptor Table](https://en.wikipedia.org/wiki/Interrupt_descriptor_table)
- [Independent-Software.com - IDT](http://www.independent-software.com/writing-your-own-toy-operating-system-setting-up-the-interrupt-descriptor-table-idt/)
- [OSDev.org - Interrupt Descriptor Table](http://wiki.osdev.org/Interrupt_Descriptor_Table)
- [OSDev.org - IDT Problems](http://wiki.osdev.org/IDT_problems#Problems_with_IDTs)
- [MIT.edu - Interrupt Descriptor Table](http://pdos.csail.mit.edu/6.828/2008/readings/i386/s09_04.htm)
- [OSDever.net - IDT](http://www.osdever.net/bkerndev/Docs/idt.htm)
- [JamesMolloy.com - GDT and IDT](http://www.jamesmolloy.co.uk/tutorial_html/4.-The%20GDT%20and%20IDT.html)
- [MIT.edu - Task Switching](http://pdos.csail.mit.edu/6.828/2008/readings/i386/s07_05.htm)
- [StackOverflow.com - Question about gate types](http://stackoverflow.com/questions/3425085/the-difference-between-call-gate-interrupt-gate-trap-gate) *(Note: Stack Overflow is rather more unreliable than other sources.)*
- [Duartes.org - CPU Rings, Privilege, and Protection](http://duartes.org/gustavo/blog/post/cpu-rings-privilege-and-protection/)

*[IVT]: Interrupt Vectors Table
*[IDT]: Interrupt Descriptors Table
*[GDT]: Global Descriptor Table
*[ISR]: Interrupt Service Routine
*[ISRs]: Interrupt Service Routines
*[IRQ]: Interrupt Request
*[IRQs]: Interrupt Requests
*[BIOS]: Basic Input/Output System
*[DPL]: Descriptor Privilege Level
*[CPL]: Current Privilege Level
*[CS]: Code Segment selector (register)
*[CPU]: Central Processing Unit
*[TSS]: Task State Segment
*[GPF]: General Protection Fault
*[GPFs]: General Protection Faults
---
layout: reference-article
title: IRQs
date: 2015-08-17 12:05:00
categories: [ docs, reference ]
parent_name: Interrupts
description: Describes Interrupt Requests on the x86 and MIPS architectures and has a lists of IRQs with descriptions.
---

# Introduction
Interrupt Requests, henceforth referred to as IRQs, are methods (/routines) which execute when an interrupt from a device occurs. In the Interrupts article, these are described as device notifications. They usually originate from an external interrupt controller (such as the Programmable Interrupt Controller (PIC) in x86 architectures). The external interrupt controller handles queueing, sequencing and prioritisation of interrupts from devices so that the main processor just receives them one by one, in order. IRQs can usually be enabled and disabled globally and individually (unlike exceptions and Non-Maskable Interrupts : NMIs). 

IRQs are usually mapped to a fixed or programmable set of ISRs, in one contiguous block. Thus IRQ numbers do not match the ISR numbers which actually handle them. IRQs are usually numbered from 0. A device notification might be anything from a PCI device to a timer overflow to keyboard signals. 

For a general description of interrupts please read the Interrupts article. For specific detail about how to configure interrupts on a given platform, please see that platform's setup article.

## Scope of this article
This article will look at IRQs for the MIPS Creator CI20 and x86 platforms. It provides a list of IRQs for each platform followed by a description of each. It also includes sample code for generic handling of IRQs.

---

# Overview
This section briefly explains what an IRQ is, how it is different from an ISR and how IRQs are generally configured. For more detail on these topics, please read the Interrupts article.

## What are IRQs?
An IRQ is an interrupt which is requested by an external device. They are called requests because IRQs can be disabled. In other words, the request can be ignored if the kernel doesn't want to know about it. An IRQ is generally mapped to an ISR (Interrupt Service Routine) but requires special processing. An IRQ is very distinct from an exception (such as a page fault) or software interrupt (such as a system call). However, an IRQ handler still has many of the requirements of an ISR such as preserving processor state.

In general, a single IRQ is allocated to one or more types of device. This means multiple devices can trigger a single IRQ. Thus when an IRQ interrupt occurs, the kernel must scan a list of devices (which it has to have loaded previously by scanning PCI bus or similar sub system) to check each device to see if the device caused the interrupt. Due to interrupt coalescing, the kernel must check every device since a single interrupt can occur for more than one requested interrupts.

## How are IRQs configured?
IRQs are normally configured via the external controller chip (on x86, the PIC - Programmable Interrupt Controller). The interrupt controller allows configuration of whether all interrupts are enabled or not and often a secondary level of control that allows enabling or disabling individual IRQs. Sometimes, the interrupt controller will be programmable such that the kernel can control which ISR numbers are triggered by which IRQs. Also, all IRQs may go to a single ISR, in which case the interrupt controller will contain registers to allow the kernel to determine which IRQ number triggered the interrupt. 

IRQs are generally configured in the early stages of OS initialisation. When an IRQ occurs, it is usually necessary for the kernel to inform the interrupt controller that the IRQ has been handled prior to returning from the IRQ interrupt handler. Failing to do so can result in one of two behaviours (depending on the hardware design). 

* Either no further IRQs will be sent to the processor so no more interrupts will occur, until the interrupt controller is reset or informed the correct IRQ has completed.
* Or, the IRQ will immediately reoccur every time the kernel attempts to return from the interrupt.

The former case will make it appear as though a single IRQ occurs once and then all IRQs (including other IRQ numbers) are blocked. The latter case will make the system spend so much time handling the same IRQ that the rest of the system will appear to be frozen.

---

# Hardware

## Overview
A processor generally only has one physical interrupt line. However, to connect lots of devices to a single line would cause a number of problems. For starters, competing hardware standards for signalling interrupts are not electrically compatible. Then there is the issue that hardware would have to be built into the processor to manage simultaneous IRQs. While all of these is surmountable, it is easier to simply have an external interrupt controller which is board-specific which acts as a middle-man between the processor and the devices.

## Interrupt Controllers
Interrupt controllers come in many variations, since they are all board specific. The only architecture with real consistency is the x86 which, for historical reasons, will always have a Programmable Interrupt Controller with the same configuration. The PIC is described in more detail below. Some interrupt controllers are so inflexible that not only are IRQs not re-mappable, but the actual address that the IRQ interrupt handler has to be at is fixed. This is the case on MIPS based systems. The MIPS-based Creator CI20 system is  described in more detail below.

## MIPS Interrupt Controller
TODO: Handler addresses
TODO: Configuration addresses

## x86 Programmable Interrupt Controller
The PIC on x86 is a complex piece of hardware. In fact, for legacy reasons, the PIC actually consists of two separate interrupt controllers which are chained as a master and a slave device. The PIC allows the kernel to enable or disable all IRQs, enable or disable individual IRQs and to map IRQs to any ISRs as two sets of 8 interrupt vectors. The PIC has 16 IRQs which are described in more detail later. 

The master PIC is what actually signals an interrupt to the processor. IRQ 2 (of the master PIC) is reserved and is used by the slave PIC. Thus IRQ 2 should never be seen by the processor. If it is, it is a spurious IRQ. When the slave PIC receives an interrupt request and wishes to pass it on, it makes an interrupt request to the master PIC. The master PIC then decides whether to pass that on to the processor or not. The master and slaves PICs also queue (/buffer) interrupts. When processing of one completes, the PIC selects the next highest priority IRQ on the queue. The IRQ default priority ordering is detailed in the IRQ descriptions.

By default, the BIOS maps IRQs to the following ISRs:

| ISR # | Assignment |
|:-----:|:------------|
| 0 - 31 | Protected-Mode Exceptions |
| 8 - 15 | IRQs 0-7 |
| 112 - 120 | IRQs 8-15 |
| Other | Free |

Most kernels remap this to the following:

| ISR # | Assignment |
|:-----:|:------------|
| 0 - 31 | Protected-Mode Exceptions |
| 32 - 39 | IRQs 0-7 |
| 40 - 47 | IRQs 8-15 |
| Other | Free |

The PIC can be accessed using the following IO ports:

| Port | Description |
|:----:|:------------|
| 0x20 | Control port of the master PIC |
| 0x21 | Data port of the master PIC (IRQ enable/disable mask) |
| 0xA0 | Control port of the slave PIC |
| 0xA1 | Data port of the slave PIC (IRQ enable/disable mask) |

The interrupt enable/disable mask ports contain a single byte bit mask where each bit enables or disables the IRQ of the same number (relative to the actual PIC device). Setting a bit to 1 disables the respective IRQ. Setting a mask bit to 0 enables the respective IRQ. Code for enabling and disabling and remapping can be found below under "x86 IRQ Setup".

---

# Software

## Overview

## MIPS

### Creator CI20 - IRQ Setup

### Creator CI20 - IRQ List

## x86

### IRQ Setup
PIC setup is a several step process:

0. Disable maskable interrupts (IRQs) using `cli`
1. Configuration of interrupt (ISR) handlers
2. PIC remap
3. *(Can be done at a later stage)* Enable/disable required IRQs
4. Enable maskable interrupts (IRQs) using `sti`

For configuration of ISR handlers, please see the ISRs article and the x86 Interrupts Descriptor Table article.

PIC remap involves remapping the master and slave IRQs separately. The following code sample demonstrates how to remap the PIC, assign master and slave PIC devices and also disable all IRQs.

***Note:***

 - ICW = Initialisation Command Word
 - OCW = Operational Command Word

``` x86asm
PIC_Remap:
    ; Remap IRQs 0-7   to ISRs 32-39
    ; and   IRQs 8-15  to ISRs 40-47

    cli ; Disable maskable interrupts
		
    ; Interrupt Vectors 0x20 for IRQ 0 to 7 
    ;               and 0x28 for IRQ 8 to 15
    mov al, 0x11        ; INIT command (ICW1 + ICW4)
    out 0x20, al        ; Send INIT to PIC1
    out 0xA0, al        ; Send INIT to PIC2

    mov al, 0x20        ; PIC1 interrupts start at 0x20
    out 0x21, al        ; Send the value to PIC1 DATA
    mov al, 0x28        ; PIC2 interrupts start at 0x28
    out 0xA1, al        ; Send the value to PIC2 DATA

    mov al, 0x04        ; MASTER identifier
    out 0x21, al        ; set PIC1 as MASTER
    mov al, 0x02        ; SLAVE identifier
    out 0xA1, al        ; set PIC2 as SLAVE

    mov al, 0x01        ; This is the x86 mode code for both 8259 PIC chips
    out 0x21, al        ; Set PIC1 mode
    out 0xA1, al        ; Set PIC2 mode
    
    mov ax, 0xFFFF      ; Set interrupt mask to disable all interrupts
    out 0x21, al        ; Set mask of PIC1_DATA
    xchg al, ah	        ; Switch low and high byte of the mask so we can send the high byte
                        ;	In this particular case, 0xFF is switched with 0xFF so it has no consequence
    out 0xA1, al        ; Set mask of PIC2_DATA

    sti                 ; Enable maskable interrupts
    nop                 ; Required - STI takes effect after the next instruction runs
```

The following code snippet demonstrates how to enable a particular IRQ number:

``` x86asm
EnableIRQ:
	; Assumes the number of the IRQ to enable is in ECX register
	
	; Load existing mask
	in al, 0xA1
	xchg al, ah
	in al, 0x21
	
	; Clear the relevant bit
	mov ebx, 1
	shl bx, cl
	not bx
	and ax, cx
	
	; Set the new mask
	out al, 0x21
	xchg al, ah
	in al, 0xA1
```

The following code snippet demonstrates how to disable a particular IRQ number:

``` x86asm
DisableIRQ:
	; Assumes the number of the IRQ to enable is in ECX register
	
	; Load existing mask
	in al, 0xA1
	xchg al, ah
	in al, 0x21
	
	; Set the relevant bit
	mov ebx, 1
	shl bx, cl
	or ax, cx
	
	; Set the new mask
	out al, 0x21
	xchg al, ah
	in al, 0xA1
```

Prior to returning from an IRQ handler, the PIC must be notified that the interrupt has been handled (otherwise no further IRQs will be signalled). This is called the End of Interrupt notification. For IRQs 0-7, only the master PIC must be notified. For IRQs 8-15, both the master and slave PICs must be notified. The following code demonstrates how to send the end of interrupt command:

``` x86asm
EndOfInterrupt:
	; Assumes the number of the IRQ being handled is in EAX register
	
	; 0x20 is EoI command number
	
	cmp eax, 8 ; Determine whether the EoI is for Slave + Master PICs or just Master PIC
	jl .MasterOnly
	out 0xA0, 0x20 ; Send EoI to Slave PIC command port
	.MasterOnly:
	out 0x20, 0x20 ; Send EoI to Master PIC command port
```

For more detail, please see the Programming the 8259 section of [this specification document](http://pdos.csail.mit.edu/6.828/2014/readings/hardware/8259A.pdf).

### IRQ List

*0 = Highest priority, 15 = Lowest priority*

| IRQ | Priority | Use |
|:---:|:--------:|:------------|
| 0 | 0 | Programmable Interval Timer |
| 1 | 1 | Keyboard (PS2) (often emulated from USB) |
| 2 | 2 | Cascade / Internal |
| 3 | 11 | Serial Port 1 (COM1) (if enabled) |
| 4 | 12 | Serial Port 2 (COM2) (if enabled) |
| 5 | 13 | Parallel Port 2 & 3 (if enabled) or Sound Card |
| 6 | 14 | Floppy Disk |
| 7 | 15 | Parallel Port 1, Printer or Secondary Sound Card |
| 8 | 3 | Real-time clock (RTC) (if enabled) |
| 9 | 4 | ACPI on Intel, Any on non-Intel e.g. PCI / legacy SCSI / NIC devices |
| 10 | 5 | Any e.g. PCI / SCSI / NIC devices  |
| 11 | 6 | Any e.g. PCI / SCSI / NIC devices |
| 12 | 7 | Mouse (PS2) (often emulated from USB) |
| 13 | 8 | FPU / Coprocessor / Inter-processor |
| 14 | 9 | Primary ATA |
| 15 | 10 | Secondary ATA |

### Spurious IRQs
Spurious interrupts are discussed in more detail in the Interrupts article. This section discusses only specific spurious IRQs.

When an IRQ occurs, ideal code would check the PIC to verify the IRQ is supposed to currently be triggered. If it is, processing continues as normal. If not the IRQ is ignored and, crucially, the End of Interrupt signal is not sent. Send the EoI to either PIC would result in the next IRQ from that PIC being skipped. Any IRQ 2 received by the processor is guaranteed to be spurious since IRQ 2 is used purely internally and is never passed to the processor. 

Spurious IRQ 7s occur in two common situations. IRQ 7s are sent in by the PIC in response to invalid conditions (such as sending an interrupt acknowledgement for a different spurious IRQ). The first situation is when the processor's interrupt line is the subject of electrical interference. This can cause the processor (or kernel) to send an incorrect IRQ acknowledgement and so the PIC responds with a spurious IRQ 7. The second case occurs if an IRQ is unmasked prior to the same IRQ input being deasserted, a spurious IRQ can end up being passed through to the processor.

---

# Example Code

## Overview

## Download

---

# FAQ & Common Problems

## x86 : Testing IRQ setup
Testing IRQs on x86 can be a tricky process. The first step is to ensure your interrupt handlers are configured properly. This is generally achieved by causing divide by zero exceptions to test your IDT Entry structure setup followed by using "int x" (where x is a number from 0 to 255) to trigger software interrupts. This lets you test IDT entry setting and test individual interrupt configuration (since not all of them will have the same configuration). 

The keyboard IRQ is often used to test IRQs as it requires no additional configuration beyond enabling the IRQ. Most hardware emulates PS2 signals/interrupts for USB keyboards while the USB device isn't being driven by OS software. This means that after booting and initialising the PIC, just typing on the keyboard should generate keyboard IRQs (if the IRQ has been enabled). This is sufficient to receive a single keyboard IRQ. To receive further keystrokes requires reading the key stroke from the PS2 Keyboard Data port. This involves reading a single byte from the port 0x60. 

---

# Further Reading

*[acronym]: details

---
layout: reference-article
title: IRQs
date: 2015-08-17 12:05:00
categories: docs reference
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
Interrupt controllers come in many variations, since they are all board specific. The only architecture with real consistency is the x86 which, for historical reasons, will always have a Programmable Interrupt Controller with the same configuration. The PIC is described in more detail below. Some interrupt controllers are so inflexible that not only are IRQs not remappable, but the actual address that the IRQ interrupt handler has to be at is fixed. This is the case on MIPS based systems. The MIPS-based Creator CI20 system is  described in more detail below.

## MIPS Interrupt Controller
TODO: Handler addresses
TODO: Configuration addresses

## x86 Programmable Interrupt Controller
The PIC on x86 is a complex piece of hardware. In fact, for legacy reasons, the PIC actually consists of two separate interrupt controllers which are chained as a master and a slave device. The PIC allows the kernel to enable or disable all IRQs, enable or disable individual IRQs and to map IRQs to any ISRs as two sets of 8 interrupt vectors. Tbe PIC has 16 IRQs which are described in more detail later. 

The master PIC is what actually signals an interrupt to the processor. IRQ 2 (of the master PIC) is reserved and is used by the slave PIC. Thus IRQ 2 should never be seen by the processor. If it is, it is a spurious IRQ. When the slave PIC receives an interrupt request and wishes to pass it on, it makes an interrupt request to the master PIC. The master PIC then decides whether to pass that on to the processor or not. The master and slaves PICs also queue interrupts. When processing of one completes, the PIC selects the next highest priority IRQ on the queue. The IRQ default priority ordering is as follows:

| IRQ # | Notes |
|:-----:|:--|
| 0 | Highest priority |
| 1 | |
| 2***\**** | (Chained / Invisible to processor) |
| 8 | |
| 9***\**** | (Chained / Invisible to processor) |
| 10 | |
| 11 | |
| 12 | |
| 13 | |
| 14 | |
| 15 | |
| 3 | |
| 4 | |
| 5 | |
| 6 | |
| 7 | Lowest priority |

By default, the BIOS maps IRQs to the following ISRs:

| ISR # | Description |
|:-----:|:------------|
| 0 - 31 | Protected-Mode Exceptions |
| 8 - 15 | IRQs 0-7 |
| 112 - 120 | IRQs 8-15 |
| Other | Free |

Most kernels remap this to the following:

| ISR # | Description |
|:-----:|:------------|
| 0 - 31 | Protected-Mode Exceptions |
| 32 - 39 | IRQs 0-7 |
| 40 - 47 | IRQs 8-15 |
| Other | Free |

The PIC can be accessed using the following IO ports:

| Port | Description |
|:----:|:------------|
| 0x20 | Control port of the master PIC |
| 0x21 | IRQ enable/disable mask  port of the master PIC |
| 0xA0 | Control port of the slave PIC |
| 0xA1 | IRQ enable/disable mask  port of the slave PIC |

The interrupt enable/disable mask ports contain a single byte bit mask where each bit enables or disables the IRQ of the same number (relative to the actual PIC device). Setting a bit to 1 disables the respective IRQ. Setting a mask bit to 0 enables the respective IRQ. Code for enabling and disabling and remapping can be found below under "x86 IRQ Setup".

---

# Software

## Overview

## MIPS

### Creator CI20 - IRQ Setup

### Creator CI20 - IRQ List

### Creator CI20 - IRQ Descriptions

## x86

### IRQ Setup

### IRQ List

| IRQ | Priority | Use |
|:---:|:--------:|:------------|
| 0 | Programmable Interval Timer |
| 1 | Keyboard (PS2) (often emulated from USB) |
| 2 | Cascade / Internal |
| 3 | Serial Port 1 (COM1) (if enabled) |
| 4 | Serial Port 2 (COM2) (if enabled) |
| 5 | Parallel Port 2 & 3 (if enabled) or Sound Card |
| 6 | Floppy Disk |
| 7 | Parallel Port 1, Printer or Secondary Sound Card |
| 8 | Real-time clock (RTC) (if enabled) |
| 9 | ACPI on Intel, Any on non-Intel e.g. PCI / legacy SCSI / NIC devices |
| 10 | Any e.g. PCI / SCSI / NIC devices  |
| 11 | Any e.g. PCI / SCSI / NIC devices |
| 12 | Mouse (PS2) (often emulated from USB) |
| 13 | FPU / Coprocessor / Inter-processor |
| 14 | Primary ATA |
| 15 | Secondary ATA |

### IRQ Descriptions

### Spurious IRQs
TODO: General spurious IRQs
TODO: Spurious IRQ 2
TODO: Spurious IRQ 7 (Usually spurious whenever this is triggered)

---

# Example Code

## Overview

## Download

---

# FAQ & Common Problems

## x86 : Testing IRQ setup
TODO: Keyboard-based testing
| 0x60 | Data port from the keyboard controller (used for reading characters) |
| 64h | command port for keyboard controller - use to enable/disable kbd interrupts, etc. |


---

# References

*[acronym]: details

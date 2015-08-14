---
layout: reference-article
title: ISRs
date: 2015-08-14 00:04:00
categories: docs reference
---

# Introduction
Interrupt Service Routines, henceforth referred to as ISRs, are the methods (/routines) which execute when an interrupt occurs. ISRs vary from platform to platform in terms of what they are triggered for, how many are available and how they must be handled. This article will look in detail at the MIPS and x86 platforms' ISRs and describe how to handle each of them. For a general description of interrupts please read the Interrupts article. For specific detail about how to configure interrupts on a given platform, please see that platform's setup article.

## Scope of this article
This article will look at ISRs for the MIPS Creator CI20 and x86 platforms. It provides a list of interrupts for each platform followed by a description of each. It also includes specific processing details required for each ISR.

---

# Overview
This section briefly explains what an ISR is, how it is different from an IRQ and how ISRs are generally configured. For more detail on these topics, please read the Interrupts article.

## What are ISRs?
An ISR is a method which executes when an interrupt is triggered. An ISR can be for an exception interrupt or for software interrupts. Device interrupts are generally called IRQs and in some architectures (e.g. MIPS Creator CI20) are handled separately from ISRs. An ISR has to preserve all the required processing state such that when the ISR returns, the program which was interrupted cannot tell. This includes saving all temporary registers since a program could be interrupted mid way through using one of them. For security reasons, even unused registers (e.g. k0 and k1 in MIPS) should be restored after the ISR has completed but prior to returning. This is so that later ISRs (or IRQs) cannot access the temporary data.

## What are the types of ISR?
There are three types of ISR:
 - Exception (also called a trap or fault)
 - Device notification (usually handled as an IRQ, please see the IRQs article)
 - Software interrupt (e.g. system call) (also called a gate or trap gate)

Exceptions generally require some change of state in the program which was interrupted. If the program was a user-mode app or driver, then the exception can either be passed to the program or the program must be terminated. Some exceptions are an expected part of processing (such as page faults) and so the program may never need to know the exception occurred. If the program which caused the exception was the kernel then one of two things can happen. A well programmed kernel will be able to handle the exception and either cancel the task it was trying to perform or continue in some way. A less stable kernel will not be able to handle the exception and will result in a kernel panic (or Blue Screen of Death on Windows). One situation in which exceptions are almost never handled in the kernel (and always result in kernel panic) is when an exception occurs during another interrupt (for example, during an IRQ). Please also be aware that kernel panics can be caused by software detecting an invalid state, without ever reaching a hardware fault, so an exception interrupt is not the only cause of panics or BSODs.

## How are ISRs configured?
Generally, ISRs are configured by telling the processor where the handler for each ISR is in memory. When the interrupt occurs, the processor may or may not save some state information before jumping to the interrupt handler. The processor will always save, at a minimum, the value of the instruction pointer before jumping to the handler. 

---

# Software

## Overview
Most software is structured to handle exceptions separately from system calls and IRQs. IRQs are dealt with in a separate article. Kernel software often has separate methods for different exceptions and a single method for system calls (and possibly IRQs). For the MIPS32 Creator CI20 platform, only a single exception handler is supported for ISRs and another handler for IRQs. The ISR handler is left the task of calling relevant methods within the kernel. 

## MIPS

### Creator CI20 - ISR Setup
In the MIPS32 Creator CI20 architecture, a single ISR handler is used for all ISRs (and IRQs unless otherwise configured). MIPS interrupt handlers must be located at 0x80000180 in memory. If configured, IRQs can be located at 0x80000200 in memory. The MIPS Coprocessor is used to configure interrupts and also has registers to allow the kernel to know which ISR vector (/number) was triggered. The task of calling separate methods for different ISRs is left to the kernel software. 

TODO: How to put a method at a particular location
TODO: How to handle IRQs and ISRs separately 
TODO: How to enable global interrupts
TODO: How to enable / configure timer interrupt (example)
TODO: Returning from an interrupt

### Creator CI20 - Interrupt Vector List
TODO: Table of vectors

### Creator CI20 - Interrupt Vector Descriptions
TODO: Table of vectors with descriptions


## x86

### ISR Setup Overview
In the x86 architecture, ISRs are configured through the Interrupt Descriptor Table, which is covered in a separate article. It is common for the exception interrupts to be handled individually and all other interrupts to be handled by a single method. This single method then calls relevant methods within the kernel for each different ISR number (/vector). This is closer to how MIPS and ARM interrupts operate. 

### Interrupt Numbers List
For extreme detail of x86 interrupts, their uses (by BIOS, operating systems, drivers and applications), bugs and workarounds, please refer to Ralf Brown's Interrupt List (TODO: Link) which is probably the most comprehensive documentation of interrupts out there. Ralf Brown's list does not, however, include information about how to handle every interrupt. For the definitive guide on interrupts, please read the Intel x86 Architecture Manual available here: TODO: Link.

TODO: Table of interrupt numbers

### Interrupt Number Descriptions
TODO: Table of interrupt numbers with descriptions

---

# Example Code

## Overview
The following downloads provide sample code for configuring interrupts on any x86 platform or the MIPS32-based Creator CI20 platform. The handlers themselves are left mostly blank since the exact handling is kernel-dependent. 

## Download

### Creator CI20
TODO: CI20 download

### x86
TODO: x86 download

---

# FAQ & Common Problems

## Interrupts immediately re-enter/re-occur after returning
A common problem when first setting up ISRs and IRQs is that once you return from the interrupt, it immediately re-triggers. This is usually caused by not notifying an external interrupt controller (such as the PIC in x86) that the interrupt has been handlded. This is usually not required for exceptions but may be required for IRQs and system calls.

## Spurious interrupts
Spurious interrupts are moderately common though if you are having trouble configuring interrupts, this is not likely to be your problem. Attributing an error to a spurious interrupt is an absolute last-resort, as it usually isn't the case. Spurious interrupts are much more common for IRQs than ISRs. Please read the Interrupts article for more detail.

---

# References

*[acronym]: details
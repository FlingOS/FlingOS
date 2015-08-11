---
layout: reference-article
title: Interrupts
date: 2015-08-10 15:15:00
categories: docs reference
---

# Introduction

## Overview 
Interrupts, also known as exceptions, traps, gates or (in a debugging context) breakpoints, are an absolute essential to OS development. They are so critical that the vast majority of devices can't even be configured without them. This article will look at what interrupts are and, in a general context, how they are used.

## Scope of this article
Interrupts and how interrupts work varies a lot from architecture to architecture but the basic concepts remain the same. This article will cover general ideas about interrupts. Other articles (e.g. the Interrupts Descriptor Table or Programmable Interrupts Controller articles) cover architecture specific implementations of interrupts.

---

# Overview

## What are interrupts?
An interrupt is an event which causes the processor to pause execution, save state, switch to a handler for the event and once that handler is complete, restore the state and resume execution of the original task. This occurs as soon as the event is signalled. 

There are three main types of events. External device notifications, internal processor exceptions (these are both hardware interrupts) and software triggered interrupts.  

### Hardware interrupts
Hardware interrupts are interrupts that are caused by hardware devices either devices attached to the processor or the processor itself. Attached device interrupts are known as external interrupts, processor interrupts are known as internal interrupts.

External device notifications occur when an attached device (for example a hard disk or USB device) wants to notify the processor that it needs attention. This may be because the device is ready to transfer data or because the device has encountered an error or any other of a number of reasons. These types of interrupts are sometimes referred to as gates.

Processor exceptions (also known as traps) occur when the processor encounters a problem. For example, if a program attempts to divide a number by zero an exception will occur. The interrupt handler for the exception enables the programmer to determine how the exception is handled. While we call them exceptions, they are in fact used as regular practice. This is discussed in more detail later.

### Software interrupts
Software interrupts are interrupts that are caused by a program i.e. the programmer (or high-level compiler) uses a special interrupt instruction which causes the processor to behave as though a hardware interrupt occurred. Software generated interrupts are also referred to as traps but often also as trap gates or just gates.


## Why are interrupts useful?

### Device notifications
Primarily interrupts are useful because they allow us to avoid polling devices to find out if they need attention. Polling involves requesting state more often than necessary, which in and of itself is inefficient. Couple that with the fact that often polling involves looping and blocking a program and it immediately has a drastic effect on performance. Interrupts solve this by allowing the kernel to pause a program (or, on some architectures, entirely halt the processor, which gives energy savings) until the interrupt occurs. 

### Exception handling
Interrupts are also useful as they allow programmers to specify what should happen when an exception occurs. Exception happens at several levels in a program but the lowest level is an interrupt exception. This is also known as a hardware exception because the interrupt is caused by the hardware reaching an invalid or impassable state. The easiest example of this is a divide by zero exception. 

If a divide by zero exception occurs, the interrupt handler for the exception is invoked. This handler is usually part of the kernel. The kernel can then decide whether to terminate the program that caused the exception or, if available, ask the program what it wants to do. Well written programs (and/or most high-level programs) will have an exception handling system which allows the program to tell the kernel to change the execution point of the program. This means, when the kernel returns from the interrupt handler, the program does not return to the same instruction as when the interrupt occurred. Usually, it will return to an exception handling method which then constructs an exception object and jumps to a "catch" block.

Other types of exceptions are used frequently and as standard practice. This is because they are not really exceptions. They are better thought of as notifications that something needs to change before execution can continue. For example, a page fault exception occurs when a block of memory has not been mapped. The memory must be mapped in before execution can continue. This is the normal case for a page fault. Page faults can also occur if a rogue process attempts to access memory which it shouldn't. In this case, the process would almost always be terminated by the kernel.

### Pre-emption (multitasking)
One other critical use of device notifications are timer interrupts. These are interrupts which are driven by a timer device (which is usually part of the motherboard and often there is more than one). Timer interrupt occur at a set interval and allow the kernel to pre-empt programs. This means the kernel can switch between programs very quickly, to give the illusion that they are running simultaneously (ignoring modern multi-core processors). The timer interrupt also allows the kernel to sleep threads for given lengths of time.


## How are interrupts generated/used?
Interrupts are always caused by hardware holding the processor's interrupt line at a high or low voltage level (depending on the specific processor). In between micro-instructions (also known as atomic- or sub-instructions) the processor checks the interrupt line to see if an interrupt is being signalled. If one is being signalled, it then proceeds with the steps previously described.

### External
An external interrupt is always a device notification and is caused by hardware external to the processor holding the interrupt line at the signal level. Since there are lots of devices and only one interrupt signal line, the signal line must be carefully controlled. This is done by an interrupt controller (also known as an interrupt driver or interrupt device). Commonly these devices are configurable, which is known as programmable. So some interrupt controllers are called Programmable Interrupt Controllers (PICs) or Programmable Interrupt Drivers (PIDs). However, the name PIC is usually used to refer to the x86/x64 IBM/PC Programmable Interrupt Controller, which is a specific piece of hardware in PCs.

An interrupt controller takes interrupts from all external devices and handles synchronising, prioritising and buffering them so that only one is sent to the processor at a time. It is possible for an interrupt to occur during another interrupt, if that interrupt is of higher priority. In general, processor exceptions (which are internal interrupts) are higher priority than external interrupts (device notifications) and device notifications themselves may also have a priority system.

### Internal
An internal interrupt is either an exception or a software interrupt. It is caused by the internal hardware of the processor holding the interrupt line at the signal level. This can be caused by an invalid condition being reached or by a special interrupt instruction.

As has been described, there are two types of internal interrupts. Hardware ones and software ones. The internal hardware interrupts are exceptions. What is less obvious, however, is why software would generate an interrupt or how it would be used. There are two main examples of this. The first example is debugging. Software would use a particular interrupt of the processor to break its own execution in order for debugging code (or a separate debugging process) to inspect the state of the program. The second reason, which is more frequently used, is for a process to perform a system call. This is where a user or kernel mode program calls a function in the kernel. It does this by setting up some parameters and then signalling the interrupt associated with systems calls. The interrupt used depends entirely upon the particular kernel (for example Windows and Linux use different interrupts for this).

---

# History
Interrupts have a relatively straightforward history. They first came to the scene in 1951 in the form of hardware interrupts. The aim was to reduce amount of CPU time being wasted on polling external devices. By 1955 these had become proper Input/Output interrupts and by 1960 vectored interrupts had been developed. 

By 1960 the majority of systems had some support for interrupts. Throughout the 1960s deferred interrupt handling became increasingly popular and with this came the idea of interrupt coalescing. 1971 saw the first of what was to become hundreds of patents relating to interrupt coalescing (most of which started being produced by high speed network engineering companies during the 1990s and onwards). Techniques for preventing or reducing interrupt storms and were also patented during the 1980s and onwards.

By the time the new millennium came along interrupts were a pretty mature concept, with the primary focus of development being how to manage them and prevent excessive interruption that would otherwise prevent actual processing that the user needs done. A large proportion of the development has been and continues to be, driven by network and infrastructure engineering companies because they have probably one of the highest performance requirements on interrupts. For networking, interrupts are vital to notify the kernel or applications that data is ready to be received or has been sent or similar events. Since the internet and LANs now underpin almost all our day to day use of computers, it is obvious that networking is in high use and so performance is vital.

---

# Hardware

## Overview

## Details : Internals

## Details : Externals

## Alternatives

## Compatibility

---

# Software

## Overview

## Basic outline

## Technical details
**Enumerations, classes, other such details**

## Implementation details
**Methods, steps, etc.**

## Alternatives

## Compatibility

---

# FAQ & Common Problems

---

# References

*[acronym]: details
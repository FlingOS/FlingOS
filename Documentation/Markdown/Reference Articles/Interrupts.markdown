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
Interrupts and how interrupts work varies a lot from architecture to architecture but the basic concepts remain the same. This article will cover general ideas about interrupts. Other articles (e.g. the Interrupts Descriptor Table or Programmable Interrupts Controller articles) cover architecture specific implementations of interrupts. Many of the general topics touched upon in this article also have their own dedicated articles (e.g. ISRs and IRQs).

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
In general, the processor has in input pin called the interrupt line. By varying the voltage level on the pin, hardware external to the processor can signal an interrupt. Internal interrupts may or may not be generated in the same way - that depends entirely on the internal design of the processor. The processor will also contain a mechanism for specifying the number of interrupt that was to be invoked. This is often only an 8-bit value. Between the processor and actual devices, there may also be an interrupt controller which handles prioritising interrupts from different devices and prevent re-interruption of a device interrupt.

The interrupt line itself can use one of several different hardware standards fro detecting an interrupt signal. These techniques all have the same effect and are all subject to issues such as spurious interrupts (which are explained in more detail in the software section).

The first hardware mechanism is level-triggering. This is when holding the interrupt line at a particular voltage level signals the interrupt. This level is considered to be the opposite of the default level and need not be higher, it can be lower. The standard voltage levels used in modern hardware are 0V and 3.3V or 5V (rarely are 3.3V and 5V used as part of the same subsystem of hardware). 0V and 5V are defined to be 0 and 1 or 1 and 0 respectively, depending on the logic convention. Irrespective of the actual voltage value, holding the interrupt line of the processor at '1' will signal an interrupt. 

The second hardware mechanism is edge-triggering. This is where transitioning the interrupt line from 0 to 1 or 1 to 0 (one or the other or both) will signal an interrupt. It is called an edge trigger because a diagram of the voltage signal shows a rising or falling line, which looks like the edge of a cliff. When the processor detects the transition the interrupt is triggered - usually only one direction of transition is detected, the leading or trailing edge. 

It is possible to have a hybrid system in which the processor looks for the edge followed by a level hold for a period of time. 

All of the above systems work on the basis that the processor samples the interrupt line at a given frequency. What this means is that is multiple devices share the same interrupt line, then if they are not designed to collaborate, the devices can conflict. In fact, the problem goes a step further because it is a hardware conflict not just a software or timing conflict. This means that devices could physically damage each other if they were not designed to share an interrupt line. 

There are two more mechanisms for signalling interrupts which do not make use of the interrupts line. These are message signalling and doorbells. Messaging signalling works by a device sending a short message over a communication line which the hardware or software reads and interprets as an interrupt trigger. This interrupt notification system can be shared between devices to the extent that the underlying communication medium can be shared, with no additional effort required. Typically, multiple interrupt messages received in quick succession will be merged into a single interrupt signal. PCI, for example, uses message signalled interrupts exclusively. PCI hardware on x86 attaches to the PIC to pass on the signalled interrupts so they appear on the x86 interrupt line.

The second and final mechanism is doorbells. A doorbell works by a device writing to a pre-agreed location in memory when it wants to signal an interrupt. The hardware or software periodically reads that location to check its value. The presence of a pre-agreed value signals an interrupt. Sometimes the doorbell region is hard-wired away form memory and directly to device or processor registers. This allows the interrupt to be signalled directly instead of having to poll a memory region. This is very similar to an interrupt line. 

There is one category of interrupts which has not yet been mentioned. This category is Inter-processor Interrupts. These are interrupts sent between cores of a CPU (or GPU or other multi-core processor) or between physically separate CPUs (or GPUs or other processors). They are used generally for synchronisation such as when memory needs to be flushed or the processor is being shut down. 

---

# Software

## Overview
Software for dealing with interrupts has two main tasks - configuration and handling. Configuration involves specifying which method the processor should call when an interrupt occurs and also settings such as which privilege levels are allowed to handle (or invoke) which interrupts. 

Handling an interrupt generally involves three steps. The first is to save any processor state or context information which might be changed during the interrupt. The next step is to actually handle the interrupt, which may involve a second stage (implemented in kernel software) determining which methods to call and might also involve notifying devices or the processor that an interrupt has been handled. The final step is to restore any saved state before returning from the interrupt.

In most systems, interrupts are assigned numbers (or vectors) which are used to refer to them. More often than not, these numbers are actually indexes into some form of table which holds all the configuration for each interrupt as an array of information structures. These numbers are referred to as interrupt numbers or ISR numbers.

An ISR is an Interrupt Service Routine. It refers to the method (or "routine") called when an interrupt is invoked ("serviced"). Device interrupts often have a secondary layer of numbering (through an interrupt controller) which numbers the interrupts according to the controller's perspective. These interrupts are known as IRQs meaning Interrupt Requests. An interrupt request number is mapped to an actual ISR number on the processor. There are usually of the order of 16 IRQs and 256 ISRs. Internal interrupts are usually at fixed ISR numbers but IRQs can be remapped if the interrupt controller is programmable (which is usually the case). This means IRQ0 could be mapped to ISR32 (as is often the case on x86 processors).

In many architectures, the name Interrupt Vector is used. This is equivalent to an Interrupt Number so where the term ISR is used below, Interrupt Vector Handler/Routine (IVH/IVR) could replace it.

## Basic outline
Generally software is organised into three parts. 

The primary part is the interrupt manager which handles registering handlers with the hardware for ISRs. It also handles mapping IRQs to ISRs. 

The second part are the interrupt handlers themselves. Often, separate methods are used for each exception but all device interrupts will go through a single method. This single method will then use one or more lists to call registered methods. This allows multiple methods to handle a single interrupt. 

The third and final part are the actual handler methods for each interrupt. For device interrupts, these will usually be part of drivers and will have to register themselves with the general interrupt handler's list (as described above). Exception and software interrupt handlers will usually be part of the kernel, though they don't have to be.

Since interrupts can occur at any time, the interrupt manager must make sure all updates to the hardware configuration (or to the second-level list of methods) are either atomic, thread-safe or done while interrupts are disabled. Since exception interrupts can't (usually) be disabled, it is best to use thread-safe programming styles. Many architectures don't offer useful atomic operation support and atomic operations may not work in a multi-core environment without also locking the memory bus, which is slow and devastating to performance.

## Spurious Interrupts
One phenomenon that often catches developers out is spurious interrupts. These are also known as ghost interrupts because they do not originate from a device, the processor or software. The most likely cause of such an interrupt is electrical interference causing the interrupt line to appear to signal an interrupt when it hasn't. Modern hardware is much better than it used to be so this is a fairly rare occurrence but Linux statistics do suggest that in a typical day at least one or two will occur. 

Spurious interrupts must be checked for and handled specially. For example, you may not want to send an End of Interrupt signal to an interrupt controller if a spurious interrupt has occurred as it may cause the interrupt controller to skip the next real interrupt (on the basis that it will "already have been handled"). Furthermore, a spurious interrupt could cause a driver to read from or (possibly worse) write to a device which would have unknown consequences as the device would not be expecting it. 

Spurious interrupts can be checked for at two levels. For device interrupts, the interrupt controller can be checked to see if it fired the interrupt. The second level of checking can be done by checking each device that could have caused the interrupt to see if it triggered it. Notably, this second step should be done anyway as multiple devices can trigger the same IRQ (and so same ISR = same interrupt). 

Spurious interrupts should never (and probably don't ever) occur for exception or software interrupts. For exception interrupts, it would be very difficult to check if it was spurious or not. For software interrupts, you could use a register or memory location to store whether a software interrupt was supposed to occur, but this is neither fool-proof nor efficient and also probably unnecessary.

## Device interrupts
Device interrupts require device specific handling. However, some general points can be made. If an interrupt controller exists, it will almost certainly need notifying when the interrupt has finished being handled (but prior to returning from the interrupt).

Device interrupts are usually grouped by type. That is to say, all devices of a given category will trigger the same IRQ number and thus the same interrupt on the processor. This is for both efficiency and space-saving in the hardware design. What this means is that when an IRQ occurs, each device driver's interrupt handler must check which device caused the interrupt. Only devices which have flagged themselves as causing an interrupt should be dealt with. Due to some interrupt coalescing techniques, a single interrupt can occur for multiple devices. Thus all devices must be checked not just until one is found. If none are found to have caused the interrupt, then either:

1. The kernel does not have a complete list of devices attached to the processor which could have caused the interrupt request,
2. Or, the interrupt was a spurious interrupt, as described in the previous section.

Device interrupt numbers are often programmable and can be found by inspecting the device's configuration at runtime. For example, any PCI devices which use interrupts will have a register specifying the ISR or IRQ number for the device.

Many device interrupts are just notifications where the main processing must then be deferred to later on (usually to a separate thread). See Deferred Interrupts below.

## Exceptions
Exception interrupts fall into three categories:

- Expected
- Unexpected but recoverable
- Unexpected and irrecoverable
 
If the language you are programming in has support for try-catch-finally sections then you'll probably find that the majority of exceptions are recoverable. However, that does assume you make proper use of them.

Expected exceptions are exceptions such as page faults and debug breaks which occur as part of normal execution and can be handled such that execution can return and continue without causing another fault. In some, rare cases, an expected exception may occur at an unexpected time, in which case it may cause an irrecoverable situation. For example, such an event usually causes a Blue Screen of Death in Windows or Kernel Panic in Linux.

Unexpected exceptions are exactly as they sound. An exception handling subsystem such as try-catch-finally may allow a program or the kernel to recover. If such a system does not exist then either the program must be terminated or, if the exception was caused by the kernel, the kernel must panic. an irrecoverable error is what causes kernel panic (Linux) or Blue Screen of Death (Windows).

Try-catch-finally systems are generally organised by a program registering a catch-all exception handler method with the kernel. When an exception occurs, the kernel calls this method with information about the exception. The handler method is then internal to the program and can decide what to do. Typically, the program will register and unregister try-catch-finally blocks as it enters or leaves them and, when an exception occurs, the handler method can look up the current closest finally or catch block and jump to it. If none is found, then the handler method would inform the kernel that the exception was unhandled.

## System Calls
System calls are the most frequent use of software interrupts. They are the primary way for a program to make calls to the kernel. They are normally triggered by using a special interrupt instruction (such as 'int _num_' on x86). These are usually called interrupt gates because programs run in privilege ring 3 (or equivalent on non x86) and kernel runs in ring 0. The gate allows the transition from ring 3 to ring 0.

A system call works by the program putting arguments into registers and then issuing the interrupt instruction. The interrupt instruction will also specify which interrupt number to invoke. This number is kernel-specific and decided by whoever developed the kernel. On Linux the interrupt number (a.k.a. vector) on x86 and x64 systems is 0x80 (128). On Windows the vector is 0x2E (46). 

Typically the first argument to a system call is a number known as the system call number. This number identifies what action or function the program is requesting the kernel to run. The remaining arguments are function-specific, as we would expect. The kernel can usually return one piece of data to the program (as is normal with a function call) and does so using the first parameter register. Often the return value is a handle to a kernel object (i.e. a piece of kernel memory) which the program can use with other system calls to access the data it required or to pass to other system calls.

If more parameters for the system call are required than there are registers, then the mechanism changes slightly. Instead of passing parameters in registers, the program will allocate a chunk of memory for a particular structure. The structure then contains all the parameters. The system call then has only two actual parameters - the call number and a pointer to the structure. This work because kernel mode can access all memory (where as user-mode can usually only access its own memory).

## Deferred interrupts
Interrupts are wonderful but they create some significant problems. The most significant problems caused are those of cross-thread and locking issues, largely relating to memory. Say, for example, you had a program writing 1024 bytes to memory. An interrupt could occur in the middle of that write, so only 560 bytes had actually been written. If the interrupt code were to try and access that memory, it would read (or write) incorrect values. This means the memory must be locked while it is being accessed. 

Unfortunately, this creates a new problem. If a program locks something and then an interrupt handler waits on that lock to become free, the whole system becomes permanently stalled. This is because while an interrupt handler is executing no (non-exception) interrupts can occur and the program holding the lock can't continue executing. This means the program will never be able to free the lock so the interrupt will be waiting forever. This same problem occurs if you try to allocate memory from a heap. 

There are two important conclusions to this:

1. Interrupt handlers must:
	- Not attempt to allocate memory
	- Must be programmed to be thread-safe (so they don't depend on locks)
	
	And all associated software must also be made thread-safe.
2. Since some interrupts require more complex processing which will require allocating memory, there has to be a method of handling interrupts that won't block programs. This technique is called deferred interrupts.

A deferred interrupt is not really an interrupt at all. What happens is, when the actual interrupt occurs, a record is made of its occurrence and any programs dependent on the result of the interrupt are paused (for example, a program making a system call would be paused). 

A separate thread of the kernel is then used to check the records of interrupts which have occurred. For each record it processes the interrupt and then resumes any waiting programs. In this way, the interrupt processing occurs inside of a thread, meaning it is scheduled alongside all the other programs. Thus the thread can utilise locks and so can do things like memory allocation.
 
Deferred interrupts are comparatively slow which is why for performance critical systems, such as networking and graphics, drivers (or the kernel) will always make sure required memory is allocated ahead of time. The allocated memory can then be used inside the actual interrupt handler.

An interrupt which has to be handled without deferring it is called a critical interrupt. 

## Performance considerations
Interrupts introduce a lot of performance considerations. The main problem to avoid is called Interrupt Storm. This is when so many interrupts occur that the hardware buffers cannot keep track of all of them and some are lost. It is also when the system spends so much time handling interrupts that normal processing cannot continue causing the system to freeze/lock up/become unresponsive.

There are many patents covering techniques for managing interrupts. These techniques come in three basic forms:

1. Coalescing of interrupts (combining multiple interrupts into a single event to allow more processing to be done per interrupt handler invocation)
2. Reducing the number of interrupts (applying optimisations to software and hardware to reduce the number of interrupts required)
3. Reducing required computation time of interrupts (applying optimisations to both hardware and software to reduce the amount of work (and thus time) required by each interrupt. This frees up time for normal processing).
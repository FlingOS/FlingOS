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

Exceptions generally require some change of state in the program which was interrupted. If the program was a user-mode app or driver, then the exception can either be passed to the program or the program must be terminated. Some exceptions are an expected part of processing (such as page faults) and so the program may never need to know the exception occurred.

If the program which caused the exception was the kernel then one of two things can happen. A well programmed kernel will be able to handle the exception and either cancel the task it was trying to perform or continue in some way. A less stable kernel will not be able to handle the exception and will result in a kernel panic (or Blue Screen of Death on Windows).

One situation in which exceptions are almost never handled in the kernel (and always result in kernel panic) is when an exception occurs during another interrupt (for example, during an IRQ). Please also be aware that kernel panics can be caused by software detecting an invalid state, without ever reaching a hardware fault, so an exception interrupt is not the only cause of panics or BSODs.

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

TODO: How to handle an ISR (Note: Has to be done in assembly code because compiler won't understand inline assembly properly / won't understand the stack manipulation
TODO: How to return from an ISR

### Interrupt Numbers List
For extreme detail of x86 interrupts, their uses (by BIOS, operating systems, drivers and applications), bugs and workarounds, please refer to [Ralf Brown's Interrupt List](http://www.ctyme.com/rbrown.htm), which is probably the most comprehensive documentation of interrupts out there. Ralf Brown's list does not, however, include information about how to handle every interrupt. For the definitive guide on interrupts, please read the [Intel x86/x64 Architecture Manual](http://www.intel.com/content/dam/www/public/us/en/documents/manuals/64-ia-32-architectures-software-developer-manual-325462.pdf).

Exceptions are classified as:

- **Faults**: Kernel or program can recover from these and continue executing. *(The saved instruction points to the faulting instruction)*
- **Traps**: Trap interrupts are triggered immediately after the instruction which caused them. *(The saved instruction points to the instruction after faulting instruction)*
- **Aborts**: Caused by a severe, unrecoverable state - the program will have to terminate or the kernel will panic.

| # | Name | Type | Error code as param? |
|:----:|:----------|:--------:|:---------------------------------:|
| 0 | Divide by zero | Fault | No |
| 1 | Debug | Trap/Fault | No |
| 2 | Non-maskable Interrupt | Interrupt | No |
| 3 | Breakpoint | Trap | No |
| 4 | Overflow | Trap | No |
| 5 | Bound Range Exceeded | Fault | No |
| 6 | Invalid Opcode | Fault | No |
| 7 | Device Not Available | Fault | No |
| 8 | Double Fault | Abort | Yes |
| 9 | _(Obsolete)_ Coprocessor Segment Overrun | Fault | No |
| 10 | Invalid TSS | Fault | Yes |
| 11 | Segment Not Present | Fault | Yes |
| 12 | Stack-segment Fault | Fault | Yes |
| 13 | General Protection Fault | Fault | Yes |
| 14 | Page Fault | Fault | Yes |
| 15 | _Reserved_ | - | No |
| 16 | x87 Floating-Point Exception | Fault | No |
| 17 | Alignment Check | Fault | Yes |
| 18 | Machine Check | Abort | No |
| 19 | SIMD Floating-Point Exception | Fault | No |
| 20 | Virtualisation Exception | Fault | No |
| 21-29 | _Reserved_ | - | No |
| 30 | Security Exception | Fault/Abort | Yes |
| 31 | _Reserved_ | - | No |
| 32-47 | Commonly used for IRQS, otherwise free for use | - | No |
| 31-255 | Free for use | - | No |
|=========================|
| **256** | | | |

### Interrupt Number Descriptions

#### 0 : Divide by zero
**Type:** Fault
**Error code:** No

This type of exception occurs when a DIV or IDIV instruction executes and the divisor is zero. The saved instruction pointer points to the instruction which caused the exception. 

This type of exception will generally require the faulting program to jump to a catch handler( possibly via one or more finally handlers) in order to deal with the exception. If no such handler is available, then the program must be terminated. In the case of the kernel, this situation would result in a kernel panic / BSOD. 

An immediate return from this kind of exception would result in a near-instant, re-occurring divide by zero exception or a double fault (depending on the exact timing / handling which is affected by processor version). This is because the saved instruction pointer points to the divide instruction. With no change of state during the exception handler, returning from the handler results in re-execution of the divide instruction. It is unwise to dodge round this fact by attempting to increase the saved instruction pointer by the size of the DIV or IDIV instructions.

Many developers choose to use this exception to test that their interrupt handling system (and/or exception handling sub-system such as try-catch-finally blocks) is working. This is for two reasons. One, this is the first entry in the Interrupt Descriptor Table (IDT) so won't be affected by incorrect structure size / array accesses (which would cause the entry offset for later elements to be incorrect). Secondly, it is easy to make a compiler such as GCC produce a divide by zero case. To do so requires the developer to mark two integers as volatile, assign one to zero and the other to a value then divide the second by the first. GCC will issue a warning but won't prevent the code. Without the volatile markers, GCC is likely to optimise out the local variables and thus automatically avoid the divide by zero exception.

#### 1 : Debug
**Type:** Trap/Fault
**Error code:** No

Debug exceptions occur for several reasons. The exception is a trap or a fault, depending on the reason for the exception. The main difference between the two types (in this instance) is that for traps, the saved instruction pointer points to the instruction after the one which caused the exception. For faults, the saved instruction pointer points to the instruction that caused the exception.

Debug exceptions occur for the following reasons:

| Reason | Type | Notes |
|:----------|:--------:|:---------|
| Instruction Fetch Breakpoint | Fault | |
| General detect condition | Fault | |
| Data read or write breakpoint | Trap | This is used with Debug Registers 0 to 3 to create memory address breakpoints (a.k.a. watches). |
| I/O read or write breakpoint | Trap | This is used with Debug Registers 0 to 3 to create I/O address breakpoints. |
| Single-step | Trap | This is used with the Single-Step flag to generate an interrupt after every instruction which executes. It allows instruction-by-instruction debugging. |
| Task-switch | Trap | This is used when hardware task switching is being used. Many kernels use software task switching (as it is considered faster, easier and more portable) so this type of trap may or may not be useful. |

The reason for the exception can be determined by using the Debug Registers. This type of exception never pushes an error code, regardless of the reason.

#### 2 : Non-maskable Interrupt
**Type:** Interrupt
**Error code:** No

Non-maskable interrupts occur for one of two reasons. The main two are hardware failures and watchdog timers. On the latest Intel chipsets there is also a third reason which is a specialist form of watchdog timer used for remote monitoring of the system. So far as I can tell, this third case is a "big ol' mess" that nobody has the time to deal with so I won't be covering it here.

For hardware failures, no mechanism is provided for detecting which piece of hardware failed. The best general-case handling an OS can do is to warn the user their hardware failed and then enter a kernel panic state. If possible, logging the error in permanent storage then performing a hardware scan at next startup may be a good idea.

For watchdog timers, the timer must have been configured by the OS. Watchdog timers are used for two purposes:

1. To detect when the kernel has locked up and thus to either free it or give it the opportunity to enter kernel panic
2. For performance evaluation. An NMI is enabled even when IRQs are disabled so it can be used as a timer interrupt to sample EIP even for device drivers. Sampling EIP allows the observer to keep track of how far a program has progressed and approximately how much time it has spent in different areas of the code. 

When an NMI occurs, you can disable the NMI controller to prevent further exceptions temporarily. This can be done by setting the high bit of a single byte read from port 0x70 and then writing the value back. To re-enable NMI (which must be done prior to returning from the interrupt), simply clear the bit and write the value to the same port. 

During an NMI interrupt, the following ports can be read to help determine what caused the interrupt. 

**System Control Port A (0x92)**
*All bits are read/write*

| Bit | Description | Notes |
| 0 | Alternate CPU reset | Value of 1 indicates the alternate CPU reset line was pulsed. |
| 1 | Alternate gate A20 | Value of 1 indicates A20 mode is active. |
| 2 | Reserved | - |
| 3 | Security Lock | |
| *4* | Watchdog timer status | Value of 1 indicates a watchdog timeout occurred. This is the only case in which the interrupt is not (necessarily) unexpected. |
| 5 | Reserved | |
| 6 | HDD 2 drive activity | This (along with bit 7) is tied to the activity light. A value of 1 in either bit switches the light on. |
| 7 | HDD 1 drive activity | This (along with bit 6) is tied to the activity light. A value of 1 in either bit switches the light on. |

**System Control Port B (0x61)**
*Some of these bits are read/write. Reading gets the status, writing enables or disables.*

| Bit | Description | Notes |
| 0 | PIT Timer 2 : Gate to speaker status/enable (R/W) | |
| 1 | Speaker data status/enabled (R/W) | |
| 2 | Parity check status/enabled (R/W) | |
| 3 | Channel check status/enabled (R/W) | |
| 4 | Refresh request | Toggles with each request. |
| 5 | Timer 2 output | Matches the current PIT Timer 2 state. |
| *6* | Channel check | Value of 1 indicates a failure occurred on the I/O bus which is most likely caused by a device such as a modem, sound card or similar |
| *7* | Parity check | Value of 1 indicates a failure occurred when reading or writing memory. |

#### 3 : Breakpoint
**Type:** Trap
**Error code:** No

Breakpoint exceptions occur immediately after an INT3 instruction occurs (also written INT 3 - identical instruction, slightly different syntax). It is sued to create pre-defined points within a program to break execution in order to analyse the program's state. Like As with debug exceptions, the Debug Registers can be used to obtain more debugging information and program state information.

Some debugging software manually replaces instructions in a program with INT3 instructions. There are two ways to do this. INT3 is a single-byte instruction like a NOP, so many compilers leave NOPs in code with the explicit purpose of them being place holders for INT3s, to be injected by the debugger. The alternative is to replace actual instructions. In this case the debugging software must keep track of the original byte and, when the INT3 occurs, replace the INT3 with the real instruction and decrement the saved instruction pointer so execution returns to the correct place. This is significantly more complex and requires the debugger to understand all the boundaries between instructions.

#### 4 : Overflow
**Type:** Trap
**Error code:** No

This type of exception occurs when the INTO instruction is executed and the Overflow (OF) flag is set in the EFLAGS (or on x86, RFLAGS) register. The INTO instruction is a special instruction meaning "Interrupt if Overflow" i.e. interrupt if the overflow flag is set. The overflow flag is set when an arithmetic operation overflows i.e. the result of the operation would not fit in the 32 or 64-bit space available (depending on the architecture register size).

#### 5 : Bound Range Exceeded
**Type:** Fault
**Error code:** No

This type of exception occurs when the BOUND instruction is executed and the specified array index is not within the required limits. The BOUND instruction detects whether the signed index value is greater than or equal to a signed lower limit and less than or equal to a signed upper limit. The limits are specified using words or dwords in memory and the second parameter to the BOUND instruction specifies the address (/pointer to) that memory.

Generally this instruction would be used to detect if an index is within the bounds of an array. With increasing software complexity, the use of try-catch-finally blocks and the relative expense of a hardware interrupts, many compilers no longer make use of the BOUND instruction, preferring instead to detect the failure condition and handle it at a higher level in software. 

#### 6 : Invalid Opcode
**Type:** Fault
**Error code:** No

This type of exception occurs when the processor attempts to execute and invalid or undefined opcode or an instruction with invalid prefixes. 

There are two situations in which an invalid opcode can be detected. The first is if the opcode is undefined which can be due to program corruption. The second is due to invalid prefixes which can occur if the faulting program was compiled for a different variant of the x86 or x64 architectures.

#### 7 : Device Not Available
**Type:** Fault
**Error code:** No

The Device Not Available exception occurs when an FPU instruction is attempted but there is no FPU. This is not likely, as modern processors have built-in FPUs. However, there are flags in the CR0 register that disable the FPU/MMX/SSE instructions, causing this exception when they are attempted. This feature is useful because the operating system can detect when a user program uses the FPU or XMM registers and then save/restore them appropriately when multitasking.

#### 8 : Double Fault
**Type:** Abort
**Error code:** Yes

This type of exception occurs when another exception is unhandled or if an exception occurs while the CPU is executing another exception handler. Normally, two simultaneous exceptions are handled separately, one after another. However, in a few cases that is not possible. For example, if a page fault occurs but the page-fault exception handler is located in a not-present page, two page faults would occur and neither can be handled. In this situation, a double fault exception would occur. (If the double fault exception handler is also paged-out, a triple fault world occur. See further down in this article for more detail about Triple faults).

A program cannot recover from a double fault. Any program which causes a double fault must be terminated. For applications and drivers it is possible to save some state information or present the user with a choice of action prior to terminating the program. However, if the kernel causes the double fault, it is generally only possible to kernel panic, perhaps present a message to the user but eventually the kernel will have to halt entirely.

If you haven't reprogrammed the PIC, IRQ 0 may be mapped to ISR 8 in which case the interrupt may be misinterpreted as a double fault when it is not. This is common in hobby OS development when developers are first starting out though good tutorials should demonstrate remapping the PIC prior to enabling IRQ 0. IRQ 0 is the Programmable Interval Timer : Timer 1 interrupt request.

#### 9 : _(Obsolete)_ Coprocessor Segment Overrun
**Type:** Fault
**Error code:** No

This interrupt is obsolete. The following description is taken from the Microsoft website (https://support.microsoft.com/en-us/kb/117389):

"Occurs when a page or segment violation is detected while transferring the middle portion of a coprocessor operand to the NPX."

#### 10 : Invalid TSS
**Type:** Fault
**Error code:** Yes

This type of exception occurs when an invalid segment selector is referenced during a task which, or when a gate descriptor is used and it contains an invalid stack segment reference.

If the exception occurred before the segment selectors were loaded from the TSS, then the saved instruction pointer points to the instruction which caused the exception. Otherwise, as is more often the case, the saved instruction pointer points to the first instruction in the new task. The error code is the selector index which caused the error (i.e. the value of the invalid index).

#### 11 : Segment Not Present
**Type:** Fault
**Error code:** Yes

This type of exception occurs when the processor tries to load a segment or gate in which the Present bit is set to 0. There is one exception to this. If the processor tries to load a stack segment selector which references a non-present descriptor then a Stack Segment exception occurs instead of a Segment Not Present exception. The error code is the segment selector index of the segment descriptor which caused the exception.

#### 12 : Stack-segment Fault
**Type:** Fault
**Error code:** Yes

This type of exception occurs when the processor tries to:

* Load a stack segment referencing a segment descriptor which is not present,
* Executes any instruction that uses the value of ESP or EBP as an address (directly or indirectly) and the stack address is not in canonical form,
* Check the stack limit and it fails

When the exception is due to a non-present segment, the error code is the segment selector index of the non-present segment descriptor that was referenced. When the exception is not due to a non-present segment, the error code is 0.

#### 13 : General Protection Fault
**Type:** Fault
**Error code:** Yes

This type of exception occurs for many reasons. It is the catch-all exception that covers all the possible types of exception that do not have their own dedicated interrupt. The most common reasons for a general protection fault to occur are:

* Segment error (accessing a segment under incorrect privilege, type, read/write rights or outside its limits).
* A program executing a privileged instruction when it is not in kernel mode i.e. while the Current Privilege Level (CPL) is not equal to zero (i.e. executed an privileged instruction while inside rings 1, 2 or 3).
* Writing a 1 in a reserved register field that was not supposed to be 1.
* Referencing or accessing a null segment descriptor.

If the exception is related to a segment, the error code is the segment selector index that caused the fault. Otherwise it is zero.

#### 14 : Page Fault
**Type:** Fault
**Error code:** Yes

**Page faults are discussed in much more detail in the x86 Paging article.**

This type of exception occurs when:

* A page directory or table entry is not present in physical memory.
* Attempting to load the instruction TLB with a translation for a non-executable page.
* A protection check (privileges, read/write) failed.
* A reserved bit in the page directory or table entries is set to 1.

In addition to the error code, Control Register 2 (CR2) is set to the virtual address which caused the page fault.

##### Error code

The page fault error code has the following format. The bits referred to by acronym are described in the table following the format.

``` bash
       31-5      4   3   2   1   0
+---+--  --+---+---+---+---+---+---+
|   Reserved   | I | R | U | W | P |
+---+--  --+---+---+---+---+---+---+
```

| Acronym | Name | Description |
|:------------:|:--------:|:---------------|
| P | Present | Value of 1 indicates the cause of the exception was a page-protection violation. When not set, it was caused by a non-present page. |
| W | Write | Value of 1 indicates the cause of the exception was a page write. When not set, it was caused by a page read. |
| U | User | Value of 1 indicates the exception occurred while CPL = 3 (i.e. the program was in user-mode). This does not necessarily mean that the page fault was a privilege violation (for example, the kernel could have deliberately paged out a sleeping program's memory to free up space). |
| R | Reserved write | Value of 1 indicates the cause of the exception was by writing a 1 to one or more of the paging system's reserved fields. |
| I | Instruction Fetch | Value of 1 indicates the cause of the exception was an instruction fetch. |

#### 15 : _Reserved_
**Type:** -
**Error code:** No

*This interrupt is reserved for future use so currently has no meaning.*

#### 16 : x87 Floating-Point Exception
**Type:** Fault
**Error code:** No

This type of exception occurs when the FWAIT or WAIT instructions, or any floating-point instruction that utilises waiting, is executed and at least one of the following conditions is true:

* CR0 register, NE bit is 1,
* Or an unmasked x87 floating point exception is pending (i.e. the exception bit in the x87 floating point status-word register is set to 1),

The saved instruction pointer points to the instruction which was about to be executed when the exception occurred. The x87's instruction pointer register contains the address of the instruction which caused the exception. Information about the exception is available in the x87's Status (word) register.

#### 17 : Alignment Check
**Type:** Fault
**Error code:** Yes

This type of exception occurs when alignment checking is enabled and an unaligned memory reference is used. Alignment checking is only performed for user mode (CPL 3) and is disabled by default. To enable it, set both the CR0 register AM bit and the RFLAGS register AC bit to 1.

#### 18 : Machine Check
**Type:** Abort
**Error code:** No

This type of exception is hardware specific and architecture implementations are not required to support it. Machine check exceptions occur when the processor detects internal errors, such as bad memory, bus errors, cache errors, etc. By default this interrupt is disabled but it can be enabled by setting the CR4 register MCE bit to 1. 

The value of the saved instruction pointer depends on the implementation and the exception. Model-specific registers are used to provide error information.

Generally a machine check exception will result in a total system crash (a kernel panic) because it means something in hardware has gone wrong. There is no way to detect what nor to recover from the error. In fact, if the fault is internal to the processor, the processor itself may crash, freeze or otherwise even during the processing of the interrupt. The best general-case course of action is to display an error message to the user then kernel panic (which should involve as clean a shut down as is possible).

#### 19 : SIMD Floating-Point Exception
**Type:** Fault
**Error code:** No

This type of exception occurs when an unmasked 128-bit media floating-point exception occurs and the CR4 register OSXMMEXCPT bit is set to 1. If the OSXMMEXCPT flag is not set, then SIMD floating-point exceptions will cause an Invalid Opcode exception instead. Exception information is available in the MXCSR register.

#### 20 : Virtualisation Exception
**Type:** Fault
**Error code:** No

This type of exception is used by Intel's VT-x Extended Page Table system. VT-x is a virtualisation technology developed by Intel enabling guest operating systems to run natively as though the host OS wasn't present i.e. it makes the host invisible. According to the specification, when Extended Page Tables are in use, some addresses that would normally be treated as physical addresses are instead treated as guest-owned physical addresses. Guest-owned physical addresses are translated by traversing a set of EPT paging structures to produce the physical addresses that are used to access memory. 

#### 21-31 : _Reserved_
**Type:** -
**Error code:** No

*These interrupts are reserved for future use so currently have no meaning.*

#### 32-47 : Commonly used for IRQS, otherwise free for use
**Type:** -
**Error code:** No

Though these interrupts are free for any use, at least 32 of the interrupts in the range 32 to 255 must be used for IRQs after remapping the PIC. It is most common for ISRs 32 through 47 (inclusive) to be reserved for the 16 IRQs (0 to 15 inclusive).

#### 32-255 : Free for use
**Type:** -
**Error code:** No

*These interrupts are not reserved and have no special meaning. They can be used for any purpose. See previous section for minor caveat.* 

One or more of the remaining ISRs (after IRQs have been taken into account) are usually chosen by the kernel for use as the system call interrupt.


### Triple Faults
TODO: Description

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

## What is a bug check?
A bug check is a Windows term for an irrecoverable error that was caused by a software condition (generally a theoretical paradox/problem) not a hardware fault. For example, attempting to allocate memory within a critical interrupt would be referred to as a bug check because it is not a hardware problem, it is purely related to the impossible situation (which is a software construct).

## Interrupts immediately re-enter/re-occur after returning
A common problem when first setting up ISRs and IRQs is that once you return from the interrupt, it immediately re-triggers. This is usually caused by not notifying an external interrupt controller (such as the PIC in x86) that the interrupt has been handled. This is usually not required for exceptions but may be required for IRQs and system calls.

## Spurious interrupts
Spurious interrupts are moderately common though if you are having trouble configuring interrupts, this is not likely to be your problem. Attributing an error to a spurious interrupt is an absolute last-resort, as it usually isn't the case. Spurious interrupts are much more common for IRQs than ISRs. Please read the Interrupts article for more detail.

---

# References

*[acronym]: details

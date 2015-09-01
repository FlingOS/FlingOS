---
layout: reference-article
title: Stacks
date: 2015-09-01 11:14:00
categories: docs reference
---

# Introduction

Stacks are a vital part of operating system development. They are used for temporary storage of (usually small - 4 byte) pieces of data such as return addresses during method calls and arguments/local variables. Conceptually they are fairly simple, items go on the top of a stack (like plates would pile up) and the top-most items are taken off the top of the stack first (like people collecting plates from a pile). 

## Scope of this article
This article will explain the brief history of stacks and a range of concepts surrounding stacks. Priimarily this article will focus on hardware stacks not software stacks because the former is much more relevant to OS development. There is also lots of good documentation already available elsewhere online about software stacks. This article also cover setting up hardware stacks in x86 and MIPS architectures.

---

# History

The concept of a stack was first proposed by Alan M. Turing in 1946 (using the terms "bury" and "unbury" instead of modern day "push" and "pop"). Alan Turing's suggested application was for storing the return address of a method call to implement subroutines (which had been invented by Konrad Zuse a year earlier as part of the Z4). In 1955 the idea resurfaced with Dr. Friedrich Ludwig Bauer and Dr. Klaus Samelson (30 March 1957). In 1957 they patented the use of stacks in computers in Germany under "Verfahren zur automatischen Verarbeitung von kodierten Daten und Rechenmaschine zur Ausübung des Verfahrens" (Munich: Deutsches Patentamt. Retrieved 2010-10-01.). This title translates as "The method of automatically processing coded data and computers for implementing the method". 

---

# Overview

## What is a stack?
The term "stack" may have originated from the idea of a plate stack, commonly used in restaurants. Plates are piled up on a spring-loaded base in a holder. Plates put on the pile cause plates underneath to be pushed downards. When a waiter needs a plate, he takes one off the top of the pile and the others all rise up one more place. This is exactly how a stack (in software or hardware) works. Putting things on the top of the stack is called pushing. Taking things off is called popping.

More formally, a stack is described as a first-in-last-out (FILO) or equivalently last-in-first-out (LIFO) structure. This means items put on the stack most recently are the ones taken off first.

## What are the essential operations?
Essential operations are ones which cannot be done via combinations of any other operations. They are independent operations. For a stack, the essential operations are push and pop which put items on and take items off the top of the stack respectively. 

## What are non-essential operations?
Non-essential operations are ones which are useful but can be expressed as a combination of essential (or other non-essential) operations. Non-essential operations can often be optimised to achieve better performance than their equivalent combination of essential operations. The only real non-essential operation for a stack is called Peek, which gets the top value of the stack but without removing it from the stack. This is a non-essential operation since it could be implemented as a pop then push. 

## What is a software stack?
A software stack is a stack which is implemented and controlled entirely in software. It does not use any special hardware registers and the memory allocated for it is part of general application memory. Software stacks also require software methods to be implemented for pushing, popping and any other operations. 

## What is a hardware stack?
A hardware stack is stack which is implemented and controll primarily (but not entirely) in hardware. It uses special hardware registers to keep track of the location and size of the stack and stack memory is generally kept separate from other general purpose memory. For example, an application stack is usually allocated separately from heap memory. A hardware stack can also use special instructions for pushing and poppping, though not all architectures provide such instructions. x86 has push/pop (and other more complex/specialist variants) where as MIPS provides no stack management instructions (but does provide a stack pointer register). This is a perfect example of the CISC vs. RISC approach to processor architecture.

## What is stack overflow?
A stack (at some level) has a fixed maximum size. A stack overflow occurs when a program (or hardware) tries to put more data on the stack than can fit in the allotted space. Stack overflow raises both stability and security concerns which are discussed further down. In general, a stack overflow in the kernel is an irrecoverable error which would require either an exception handling system or a kernel panic/BSOD to handle. x86 has a specific Stack Overflow exception. Under MIPS, stack overflow is caught by one of the other exceptions, depending on the exact situation ()more research is needed on this).

## What is a stack frame?
When a method is called, the address to return to when the method has finished is stored on the stack (as a pointer). A few of the items before the return address on the stack are the method's arguments. Items after the return address (i.e. higher on the stack i.e. pushed after the return address) are local variables and temporary values used in the method. The arguments, return address, locals and temporaries a referred to as a stack frame. When another method call happens, a "new" stack frame is formed. When a method returns, the stack frame is destroyed. 

## What is stack unwinding?
When an exception occurs, items may still be on the stack which need removing before execution can/should continue elsewhere in the code (the "elsewhere" generally being an exception handler / "catch" block). Some of the items which need removing may also be reference types where a garbage collector must be informed before the value is lost (or where memory must be freed). Stack unwinding is the process of removing items from the stack between where it currently is and where it needs to be, performing any necessary cleanup along the way.

## What alternatives are there to a stack?

## What is a stack machine?

## Why is a stack useful?

## Why is the stack necessary?

## How is a stack managed?

## How is stack unwinding achieved?

---

# Hardware

## Overview

## MIPS Stack
- Setup
- Use 
- Alignment

## x86 Stack
- Setup
- Use
- Optional alignment

---

# Software

## Overview

## Security considerations

## Technical details

- Typical stack frame structure
- x86 stack frame for interrupts

## Implementation details

- Setting up stack x86 & MIPS
- Pushing/popping to/from stack in x86
- Pushing/popping to/from stack in MIPS

---

# FAQ & Common Problems

---

# References

- https://en.wikipedia.org/wiki/Stack_(abstract_data_type)
- https://en.wikipedia.org/wiki/Stack_machine
- 

*[acronym]: details

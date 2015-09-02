---
layout: reference-article
title: Stacks
date: 2015-09-01 11:14:00
categories: docs reference
---

# Introduction

Stacks are a vital part of operating system development. They are used for temporary storage of (usually small - 4 byte) pieces of data such as return addresses during method calls and arguments/local variables. Conceptually they are fairly simple, items go on the top of a stack (like plates would pile up) and the top-most items are taken off the top of the stack first (like people collecting plates from a pile). 

## Scope of this article
This article will explain the brief history of stacks and a range of concepts surrounding stacks. Primarily this article will focus on hardware stacks not software stacks because the former is much more relevant to OS development. There is also lots of good documentation already available elsewhere online about software stacks. This article also cover setting up hardware stacks in x86 and MIPS architectures.

---

# History

The concept of a stack was first proposed by Alan M. Turing in 1946 (using the terms "bury" and "unbury" instead of modern day "push" and "pop"). Alan Turing's suggested application was for storing the return address of a method call to implement subroutines (which had been invented by Konrad Zuse a year earlier as part of the Z4). In 1955 the idea resurfaced with Dr. Friedrich Ludwig Bauer and Dr. Klaus Samelson (30 March 1957). In 1957 they patented the use of stacks in computers in Germany under "Verfahren zur automatischen Verarbeitung von kodierten Daten und Rechenmaschine zur Ausübung des Verfahrens" (Munich: Deutsches Patentamt. Retrieved 2010-10-01.). This title translates as "The method of automatically processing coded data and computers for implementing the method". 

---

# Overview

## What is a stack?
The term "stack" may have originated from the idea of a plate stack, commonly used in restaurants. Plates are piled up on a spring-loaded base in a holder. Plates put on the pile cause plates underneath to be pushed downwards. When a waiter needs a plate, he takes one off the top of the pile and the others all rise up one more place. This is exactly how a stack (in software or hardware) works. Putting things on the top of the stack is called pushing. Taking things off is called popping.

More formally, a stack is described as a first-in-last-out (FILO) or equivalently last-in-first-out (LIFO) structure. This means items put on the stack most recently are the ones taken off first.

## What are the essential operations?
Essential operations are ones which cannot be done via combinations of any other operations. They are independent operations. For a stack, the essential operations are push and pop which put items on and take items off the top of the stack respectively. 

## What are non-essential operations?
Non-essential operations are ones which are useful but can be expressed as a combination of essential (or other non-essential) operations. Non-essential operations can often be optimised to achieve better performance than their equivalent combination of essential operations. There are four main non-essential operations for a stack which are: 

1. Peek: Gets the top value of the stack but without removing it from the stack. (Expressed as essential ops: Pop x, Push x)
2. Duplicate: Pushes a copy of the top item to the stack. (Expressed as essential ops: Pop x, Push x, Push x. This is also: Peek x, Push x)
3. Swap (or Exchange): Top-most two items on the stack exchange places. (Expressed as essential ops: Pop x, Pop y, Push x, Push y. This is also: Rotate 2)
4. Rotate: the n top-most items on the stack are rotated on the stack. A rotate of 2 is equivalent to "swap". The two most common variations of this are clockwise rotate and anticlockwise rotate. 

## What is a software stack?
A software stack is a stack which is implemented and controlled entirely in software. It does not use any special hardware registers and the memory allocated for it is part of general application memory. Software stacks also require software methods to be implemented for pushing, popping and any other operations. 

## What is a hardware stack?
A hardware stack is stack which is implemented and control primarily (but not entirely) in hardware. It uses special hardware registers to keep track of the location and size of the stack and stack memory is generally kept separate from other general purpose memory. For example, an application stack is usually allocated separately from heap memory. A hardware stack can also use special instructions for pushing and popping, though not all architectures provide such instructions. x86 has push/pop (and other more complex/specialist variants) where as MIPS provides no stack management instructions (but does provide a stack pointer register). This is a perfect example of the CISC vs. RISC approach to processor architecture.

## What is stack overflow?
A stack (at some level) has a fixed maximum size. A stack overflow occurs when a program (or hardware) tries to put more data on the stack than can fit in the allotted space. Stack overflow raises both stability and security concerns which are discussed further down. In general, a stack overflow in the kernel is an irrecoverable error which would require either an exception handling system or a kernel panic/BSOD to handle. x86 has a specific Stack Overflow exception. Under MIPS, stack overflow is caught by one of the other exceptions, depending on the exact situation (more research is needed on this).

Overflow is also known as overrun.

## What is a stack frame?
When a method is called, the address to return to when the method has finished is stored on the stack (as a pointer). A few of the items before the return address on the stack are the method's arguments. Items after the return address (i.e. higher on the stack i.e. pushed after the return address) are local variables and temporary values used in the method. The arguments, return address, locals and temporaries a referred to as a stack frame. When another method call happens, a "new" stack frame is formed. When a method returns, the stack frame is destroyed. 

## What is stack unwinding?
When an exception occurs, items may still be on the stack which need removing before execution can/should continue elsewhere in the code (the "elsewhere" generally being an exception handler / "catch" block). Some of the items which need removing may also be reference types where a garbage collector must be informed before the value is lost (or where memory must be freed). Stack unwinding is the process of removing items from the stack between where it currently is and where it needs to be, performing any necessary clean-up along the way.

## What do "grow(ing) up" and "grow(ing) down" mean?
A stack which grows downwards is the most common system. It means that when you push an item to the stack, the address of the stack pointer is decremented (by the number of bytes pushed). This means high-bytes of values are pushed first (popped last) and it is a very convenient system for little-endian architectures. Stack which grow upwards do the opposite and are less common. 

On both x86 and MIPS architectures, the stack grows downwards. In x86 this is forced if you use the push and pop instructions (which is recommended) but in MIPS the decision for upwards or downwards growth is arbitrary. However, a downwards growing stack will usually give greater compatibility particularly with any existing software (unless you wish to recompile from source all the time).

## What alternatives are there to a stack?
The main alternative to a hardware stack, which is very widely used, is register-based value passing. Computers which used this are called register-machines. While architectures based on registers still (generally) contain a stack, their default place to store temporary values is registers. Generally, a set of registers will be allocated as arguments, return value (sometimes two or more but rarely), return address and temporaries (local variables). All the values of the registers just described must be preserved during a called method meaning that if a method wants to use them (or such as for calling another method/subroutine) it must save the register values. Saving them is done by placing them on the stack. The compiler usually works out which registers will be used and saves the necessary values at the start of the method.

In higher level languages (C and above) stack vs register use is sorted out by the compiler. It is important to understand, however, that not all arguments will necessarily be on the stack - they may be in registers! 

All major architectures (x86/x64, MIPS and ARM) use the register-first approach described above. 

## What is a stack machine?
A stack machine is a theoretical computer design which pushes all operands for instructions to the stack and operations pop them off the stack followed by pushing the result of the operation (if any). Such a system generally has very few registers. An example of a practical stack machine is the Transputer (which has recently been updated to modern hardware by the [OpenTransputer](http://www.opentransputer.org) team). 

## Why is a stack useful?
The stack is useful because it provides the easiest, most reliable way to store temporary values such as arguments, locals and return addresses. It is conceptually simple and requires very little hardware (or software) to implement making it energy, physical-space (/board-area/silicon) and software efficient. Even old hardware had enough RAM for a stack. Nowadays, stack-based programming is so much more energy efficient that even the Java Virtual Machine for Android (Dalvik VM) was written as a stack-based VM because the register-based standard JVM was too power hungry for mobile devices.

## Why is the stack necessary?
If you don't have the stack to store temporary values, where else are we going to put them? Registers? What happens when we run out of register space or want to reuse a register (which will happen eventually)? Ultimately, any temporary value storing system that is memory efficient will end up being equivalent to a stack (even if it doesn't quite look like it). It would be impossible (and insane) to pre-allocate (at compile time) space for arguments and locals in program memory since the model breaks down (or becomes obstructively complex) if you consider the effect of a method calling itself (or re-entrant methods in general). 

## How is a stack managed?
There are four aspects to stack management: maintaining the current location of the stack, maintaining and checking boundaries of the stack (top and bottom to prevent under and overflow) and pushing/popping to/from the stack. Generally, a hardware register (or, in a software stack, pointer) is used to maintain the current location of the top of the stack (i.e. the last item pushed) in memory. To push an item simply requires shifting the pointer value (incrementing/decrementing as per architecture) and copying the value in. Popping does the inverse (usually copying the value to a register). Checking stack boundaries for underflow and overflow can be tricky but many architectures provide exceptions (interrupts) for overflow. Underflow is much rarer and requires the kernel to limit a program's access to the address space (/memory space) to prevent the pointer from being decremented too far.

## How is stack unwinding achieved?
Generally a linked-list of structures (which can also be on the stack but will require at least one fixed-location pointer) keeps track of information about the current stack frame and what clean-up steps are required. If an exception occurs, execution jumps to a handler method which inspects the current stack frame and uses the information in the structure(s) to unwind the stack (performing clean-up in the process) and then continue at the appropriate point. 

For languages which support exception handling (generally using try-catch-finally blocks) library functions will be required to handle exceptions, handle stack unwinding and handle adding/removing stack frame structures to/from the linked list. For many languages this is handled for you in standard libraries and the necessary calls to the libraries are injected by the compiler.

There are some situations when stack-unwinding and exception-handling is impossible. For example, exception handling during an interrupt handler can lead to further, unforeseen complications since the interrupt handler may not have performed all the processing required for the OS to continue executing correctly. Often, an exception during an interrupt will force the kernel to "panic" (which in Windows is one possible cause of Blue Screen of Dead (BSOD)).

---

# Hardware

## Overview
Most architectures have a stack pointer registers which contain a pointer to the top of the stack in RAM. Architectures may or may not (although most do) provide security features such as stack segments, paging or other stack boundary specifiers to provide overflow and underflow protection. 

Some architectures (generally CISC-based designs rather than RISC-based designs) provide special push and pop instructions for manipulating the stack. The most common such architecture is x86 which has "push" and "pop" instructions along with other specialist ops (such as pushad, popad, pushfd and popfd) for performing large (/batch) push and pop instructions or (in the case of pushfd and popfd) providing special register access. 

## MIPS Stack
Setting up a stack in MIPS is very simple. It merely requires you to put a word (4-byte) aligned address in the stack pointer register ($sp = $29). MIPS requires addresses to be access-size aligned i.e. to access a byte in memory the address needs no alignment, for halfwords the address must be 2-byte aligned and words must be 4-byte aligned. It is good practice to keep the stack 4-byte aligned (and a lot of code will assume this) meaning values are usually expanded to multiples of 4 bytes in size when being pushed to the stack.

## x86 Stack
Setting up a stack in x86 is very simple. It merely requires you to put an address in the stack pointer register (ESP). It is good practice to use a double word (dword, 4-byte) aligned address but is not required. x86 alignment checks are optional (and must be turned on by the kernel if desired). It is good practice to keep the stack 4-byte aligned (and a lot of code will assume this) meaning values are usually expanded to multiples of 4 bytes in size when being pushed to the stack.

---

# Software

## Overview
Software for controlling hardware stacks (including setting them up) is pretty simple. This section will not discuss, however, how to make proper use of a hardware stack. A separate article is dedicated to calling conventions which deals with stack frames in more detail.

## Security considerations
There are two main types of security risk when it comes to hardware (and potentially software) stacks. These are:

1. Overflow attack (known as Stack Smashing, very common, moderately easily avoided)
2. Underflow attack (not very common, easily avoided)

An overflow attack, known as Stack Smashing, is the most common type of attack. It involves supplying more data to push onto the stack than there is actually space for. The attacker's hope is that the stack overflows and starts overwriting itself or other areas of program memory. With a bit of effort, he or she can devise a chunk of data to push to the stack which will cause the return address of a function to point to their own data. This allows the attacker to make the program jump to their data, which can be code, and thus run malicious code that wasn't originally part of the program. 

An overflow attack can be avoided by a number of techniques (other than having the OS check the stack pointer). The first technique is to detect the attack and stop execution before anything goes wrong. This is commonly done using Stack Canaries (a.k.a. canary values). The second technique is to use a non-executable stack, which generally involves placing the stack in a memory area which can be marked as non-executable (such as a segment or page). Under this technique, it is also common to make the executable code non-writeable. The final technique is to randomise a program's memory layout which prevents an attacker from easily working out the layout. However, this must be combined with a non-executable stack for it to be effective.

Stack canary values are integers chosen randomly at the start of a program and are placed before every return address on the stack. Prior to using a return address, the program checks the canary value to see if it correct. If it is not, a stack overflow has occurred and handles the exception.

An underflow attack is not very common since it does not give the attacker the ability to change program execution (since no additional data is added). However, variations might still be possible. An underflow can be detected simply by checking the stack pointer but also by using memory controls such as segments or paging to prevent the stack being used if the stack pointer goes out of bounds. Canary values and the other techniques described may be applicable. The direction in which the stack grows can also make a difference depending on the memory layout.

## Technical details

A typical stack frame will look like this:

| \[Top\] |
| (If about to call a method) Arguments for function to call |
| Temporaries |
| Locals |
| Base/Frame Pointer |
| Return Address |
| Arguments for current function |
| .... |
| \[Bottom\] |

The stack frame on entering an x86 interrupt service routine will look like this:

| \[Top\] |
| Old SS (iff privilege level changed) |
| Old ESP (iff privilege level changed) |
| EFlags (or just Flags if 16-bit mode) |
| CS |
| EIP (or just IP if 16-bit mode, Return address) |
| Error code (if any) |
| \[Bottom\] |

If the handler is in 16-bit mode then all values are 16-bit, otherwise they are all 32-bit.

## Implementation details

### MIPS

To set up a stack in MIPS simply requires allocating space and setting $sp, as shown here:

``` armasm
Stack_Top:
.space 0x1000
Stack_Bottom:

# At some point later during processor initialisation 

la $sp, Stack_Bottom
```

MIPS does not have push or pop instructions so it must be done manually, as shown here:

``` armasm
# Push
addi $sp, $sp, -4  # Decrement stack pointer by 4
sw   $r3, 0($sp)   # Save $r3 to stack
```

``` armasm
# Pop
lw   $r3, 0($sp)   # Load $r3 from stack
addi $sp, $sp, 4   # Increment stack pointer by 4
```

### x86

To set up a stack in x86 simply requires allocating space and setting ESP, as shown here:

``` x86asm
Stack_Top: 
  TIMES 65535 db 0
Stack_Bottom:

; At some point later during processor initialisation 

mov dword ESP, Stack_Bottom
```

x86 does have special push and pop instructions, which can be used as follows:

``` x86asm
; Push 4-byte value
push dword EAX
; Pop 4-byte value
pop dword EAX

; Quickly "pop" 4-byte value
add ESP, 4

; Other special push/pop instructions:
pushfd ; Push EFlags register value
popfd  ; Pop value into EFlags register

pushad ; Push all/many of the register values as dwords
popad  ; Pop values into all/many of the registers
```

---

# FAQ & Common Problems

## MIPS push/pop not working or exceptions occur
Check that your stack is properly aligned. 

## Was anyone crazy enough to write a LIFO programming language?
Unsurprisingly, given the number of people in the world, there were some who thought a LIFO-based programming language was a good idea (and perhaps it was - I'll leave that for you to decide). In any case, if you're curious, take a look at [Forth](http://www.forth.com/forth/).

---

# References

- [Wikipedia.org - Stack (abstract data type)](https://en.wikipedia.org/wiki/Stack_(abstract_data_type))
- [Wikipedia.org - Stack Machine](https://en.wikipedia.org/wiki/Stack_machine)
- [EspaceNet.com - Patent](http://worldwide.espacenet.com/publicationDetails/originalDocument?CC=DE&NR=1094019&KC=&FT=E)
- [CMU.edu - Stacks and Queues](http://www.cs.cmu.edu/~adamchik/15-121/lectures/Stacks%20and%20Queues/Stacks%20and%20Queues.html)
- [I-Programmer.info - Stacks](http://www.i-programmer.info/babbages-bag/263-stacks.html)
- [Allisons.org - Stack](http://www.allisons.org/ll/AlgDS/Stack/)
- [OSDev.org - Stack](http://wiki.osdev.org/Stack)
- [Wikipedia.org - Register machine](https://en.wikipedia.org/wiki/Register_machine)

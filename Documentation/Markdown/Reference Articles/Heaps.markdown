---
layout: reference-article
title: Heaps
date: 2015-09-05 12:47:00
categories: [ docs, reference ]
parent_name: Memory
description: Describes heaps, their usage and provides implementation details for x86 and MIPS architectures.
---

# Introduction

A heap, for the purposes of OS development, is a memory management construct which allows the division of a large area of memory into smaller, variable size units which can then be used for structures, arrays, strings, objects and perhaps some other theoretical formats. The division of the assigned area of memory is done dynamically upon request so that the executing program (in this context, the kernel) can allocate sizes of memory to meet its requirements. 

A heap usually sits inside of a large-scale memory management system (such as a paged-memory manager on x86). The large-scale manager takes care of allocating big blocks of memory to applications, drivers and the kernel heap. The heap then manages dividing the large blocks (usually 4KiB i.e. one page) into smaller bits as per whatever kernel code requests.

## Scope of this article

Heaps, heap algorithms and heap-sort is a huge field which no single article could hope to cover in its entirety. Even a presentation of a single algorithm would consume an article in its own right. This article aims, therefore, to provide a detailed introduction to heaps, present the most important concepts, problems and their solutions. It should hopefully be a good guide and starting point for OS developers looking to implement a heap in their own OS. As such, this article covers heaps as a memory management construct and does not cover heap algorithms or other uses such as heap-sort.

---

# Overview

## General advice

*The following is general advice that serves as a useful guide to putting all the information in this article together into one, coherent understanding that will enable the reader to implement their own heap in their kernel.*

There are many questions, problems and solutions presented below. Understanding all of them from the outset would be very difficult because it is likely that if you're only just implementing a heap, you probably haven't hit critical versus deferred interrupts problems (which are described below). It is probably best, therefore, that you read the whole article through a couple of times and just ignore bits you don't understand yet or that don't seem relevant (but keep them in mind for your future work when they will become relevant). 

The key points to understand are what a heap is, what it's for and how to create and use one. You can discover all the other problems and solutions along the way. If you've used a good kernel design and heap implementation, most of the problems are easily solved (or at the very least, easy to implement existing solutions). We advise that you try to understand what a heap is, how it is used and then just head to the Software section for how to implement one. Come back and read the other sections once you've got a working heap and can start to experiment with the problems by hitting them yourself. 

The information is split into key information, which contains everything you need (or should) know before implementing a heap in your kernel and extended information, which contains extra information about more advanced topics or surrounding features that may be required later on.

## Key information

### What is a heap?
A heap is a memory management system that allows the allocation of small amounts of memory (usually in the range 1 to 4096 bytes). The heap is assigned large blocks of memory by the main memory manager and the heap then works out how to split that up into the small blocks requested by other bits of the kernel. The large blocks are usually pages and the requests come in as calls to an allocate function.

### What creates a heap?
A heap is created by a program and is used only by that program. The heap usually requests a number of large blocks of memory from the main memory manager (almost always contained within the kernel) to use initially and then expands later if it needs more space. In the case of the kernel, the heap is created by the kernel very early on in the initialisation process.

### What manages the heap?
Roughly speaking, the heap manages itself. It works out when it is out of memory (and may or may not request more) and how to allocate and free memory. One management requirement is placed on callers of the heap: they must free memory that they request allocations for once the memory is no longer in use. If they do not, the heap will rapidly run out of memory and crash the program (or, in the case of the kernel, the entire operating system). If the kernel heap truly runs out of memory (and can't request any more) the kernel must panic (known as Blue Screen of Death or Bug Check in a Windows environment). 

One solution to manually calling alloc and free that is used in object-oriented languages is a garbage collector. This handles creation and destruction of memory automatically for the programmer and thus greatly reduces the chance of memory leaks. Unfortunately, a garbage collection layer decreases performance and increases the amount of meta-data which can be a significant slowing factor in an operating system. This is because performance of the heap is so critical to system performance as it underpins nearly every aspect of the system.

### What provides memory space for a heap?
The broader memory manager assigns large areas (usually a page i.e. 4KiB) of memory to heap. The memory manager is often referred to as a virtual memory manager as it assigns virtual pages of mapped memory not physical memory.

### What is a heap algorithm?
A heap algorithm is one of two things. Either it is an algorithm for implementing a heap as described (i.e. for implementing memory management) or it is an algorithm for implementing a generic heap structure which is used for hundreds of different things in computing. Heap-sort is a specific algorithm for sorting data which is often treated slightly separately from other heap-related algorithms.

### What is the best heap implementation?
This is impossible to answer. Every heap implementation is a compromise of the following main factors:

- Allocate speed for small allocations
- Allocate speed for large allocations
- Free speed for small allocations
- Free speed for large allocations
- Amount of global metadata
- Amount of metadata per allocation
- Degree of fragmentation over time
- Stability
- Real-time performance (e.g. guaranteed maximum number of cycles to allocate)
- Implementation complexity

So which implementation is best is determined by your requirements. As a hobby/beginner level, a low-complexity, high-stability heap with reasonable small-allocation speed is probably best suited since it will give reasonable performance but be easiest to debug.

### What is (page) alignment?
Alignment is when an address (/pointer) sits on a particular boundary i.e. if you were to divide the address by the alignment value, there would be no remainder. Alignment is used by hardware as an easy way to optimise memory access performance. Many drivers (and even the CPU itself) will require data alignment. A common requirement is page alignment, which is where the data allocated as to start of a page boundary (in x86 systems, a page is usually 4KiB in size and thus 4KiB aligned). 

### What is a memory pool?
It is a collection of blocks of memory e.g. all the large blocks of memory assigned to the heap is referred to as the heap's (memory) pool.

### Why is a heap necessary?
Most programming requires a way to be able to create and store data at runtime when the amount of data is not know at the design or compilation stages. The heap is used to solve this by allocating variable amounts of data at runtime that can be referred to by passing around a pointer. Memory (i.e. space for data) allocated on the heap is maintained until the memory is freed (and thus can be re-allocated for different data) or until the system reboots. 

For example, code enumerating a PCI bus cannot know at design or compile time how many devices will be attached to the bus. It will, however, want to be able to keep track of separate data about all the devices it finds. This is where the heap is used. The code can allocate space for a new structure for each device it finds (and store the resulting pointers in a linked list or similar). A linked list is also an example of when the heap is required.

### Why doesn't everything go on the stack?
While the stack can be used for small amounts of data, it is not suitable for large allocations such as big arrays. Allocating such large areas of memory from the stack would rapidly cause a stack overflow. Also data on the stack only available within a function or sub-routine call. The data on the stack would not be easily available to a calling or parallel method. The heap solves these issues. The heap can manage large areas of memory and the data is not associated with a particular method. Memory on the heap is always referred to by a pointer and so methods can simply pass the pointer around. Care must be taken, however, to ensure that the memory is freed when no method (or other structure) holds a pointer to it. 

### Why is (page) alignment needed?
Data alignment is needed because hardware requires it in many cases. Sometimes software will use data alignment techniques as a performance optimisation but this isn't actually _necessary_ (i.e. the software could be changed).

### How does the heap fit into memory management?
The heap is used for managing a large area of memory (multiple pages) by splitting it up into small areas. Each separate program (kernel and drivers included) has a separate heap managed by code within the program. This there are many heaps within a single operating system. The heap can request data from a (virtual) memory manager (either by a direct call or a system call) to request more large blocks to add to the heap memory. Often the large blocks are pages. 

There is usually only one (virtual) memory manager in the entire system which manages the entire virtual address space and handles splitting it up into large blocks which are assigned to programs (drivers, apps or the kernel). The (virtual) memory manager does not deal with small allocations - that is a heap's job. Equally, the heap should not handle splitting up and assigning the (virtual) address space - that is the (virtual) memory manager's job.

### How does the heap fit with the stack?
The heap is used for allocations of data larger than the word size and which need to be passed between functions or stored for later use. The heap returns pointers which are stored on the stack (as arguments/return values) or in heap memory inside other structures. The stack is used for temporary (usually word-sized) variables as arguments, locals and return values. 

Data on the stack is usually only available to one or two methods and cannot be passed around. Once data is popped from the stack it is deaned to no longer be in memory. In contrast, if a pointer to heap memory is popped from the stack, the heap memory is still allocated. If all pointers to the heap memory are lost then the heap memory will remain allocated until the system restarts. This is a common cause of out of memory exceptions since un-freed memory prevents it from being used again later and so the heap runs out of space very quickly.

### How is the heap used before the (virtual) memory manager is set up?
This is a bit of a "chicken and egg" problem which has to be solved by the programmer. Either the programmer allocates some fixed space in the kernel image which can be used as a small initial heap (thus not requiring the memory manager). Or the programmer writes all the initialisation code up to and including the (virtual) memory manager, such that it doesn't need the heap. The former option is the easiest and probably most common. See the Software section for further details.

### Where can I find a heap implementation?
FlingOS has a heap implementation as part of the main kernel which can be found in Kernel\Libraries\Kernel.FOS_System\Heap.cs or you can try searching online. The [OSDev.org website](http://wiki.osdev.org/Heap) has a number of C implementations which are freely available for use.

### When should the heap be created?
A program's heap should be created very early on in the initialisation process (this includes the kernel). For the kernel, the heap should be created once the processor, stack and virtual memory (if used/required) have been initialised. The heap will underpin just about every aspect of your code so it will be required very early on.

### When should memory be allocated from the heap?
Whenever data must be stored that will be used again later and which is either too large to go on the stack or must be preserved between calls to the same or other methods.

### When should memory be freed from the heap?
Whenever the memory is no longer being used which is usually when there are no more pointers to the memory held by methods. However, care must be taken when freeing memory. It is common to place pointers to structures within other structures. So long as at least one pointer to the root structure exists, everything is fine. When the program goes to free the root structure, it must also check and free (if necessary) any pointers contained within the root structure i.e. it must free any child structures. Unfortunately, it is common to end up in a ring situation (imagine a triangle of pointer references between structures) in which case the programmer must be aware of the fact and break the ring in some way before freeing.

## Extended information

### What is heap inspection?
Heap inspection is when a program goes through all the memory the heap is managing to find out what state its in. Heap inspection can include calculation of fragmentation, determination of whether any data-overlap/overwrite or overflow has occurred and evaluation of amount of used or free memory. 

### What are the common heap performance metrics?
Common performance metrics for heaps include:

- Allocation speed: This is usually calculated by performing a large number of fixed size allocations as quickly as possible and timing how long each allocation takes. An average allocation time is then determined. 

  Allocation speed can be affected by a number of factors including: size of the allocation, amount of free memory, number of existing allocations, size of memory pool.
- Free speed: This is usually calculated by performing the inverse of the allocation speed test (described above).
- Allocation path length: The maximum number of instructions that must execute before an allocation is returned or failure to allocate is detected.
- Degree of fragmentation/wasted space: Usually a measure of the amount of wasted space when lots of variable-size allocations are made and freed over a period of time. After a while, small gaps will appear in the heap which cannot be used for many/any allocations. These gaps are treated as wasted space. The more of them that appear, the more fragmented the memory space is.
- Metadata bloat: Size of global metadata after a period of time and/or size of metadata per allocation.

### What is heap overflow?
Heap overflow is a form of buffer overflow which occurs when the heap runs out of space and starts allocating memory from beyond its limits. Both underflow and overflow should be impossible in a well-written implementation. If the heap runs out of memory, it should either return a null pointer, cause a program crash (/kernel panic) or (ideally) request more memory for the memory pool from the memory manager.

### Why does OS development treat a heap and a heap algorithm as separate things?
OS developers treat the heap and heap algorithm as separate things because OS development comes with lots of extra problems that surround the sue of the heap, almost irrespective of the actual implementation. These problems don't occur as a user program level (and may or may not be hidden from drivers). One such example is the problem of (not) allocating memory during critical (i.e. non-deferred) interrupts.

### Why shouldn't the kernel heap be used for shared memory?
The kernel heap is a critical data store for the kernel. As such, any access (which might lead to corruption or manipulation) of the heap from outside the kernel is a major stability and security hole. In most architectures, protecting small areas of memory is not possible so protecting a small section of kernel heap memory for use as shared memory would not be possible. As such, heap memory should not be used for shared memory as it creates a bug security and stability risk.

### How many heaps are there?
One for the kernel and then for every separate process within the system (Note: process not thread!).

### How does managed memory (objects) use the heap?
In a managed memory system, a garbage collector (or equivalent) handles creation and destruction of objects. Thus the garbage collector handles all the calls to the heap to allocate and free memory. The garbage collector handles taking a type, working out its size and then requesting the necessary amount of memory from the heap. The amount of memory allocated is usually the size of the new object plus some space for a header or footer data structure. Usually a header structure is used as it is better protected.

### How can data overwrite be detected?
All allocations can be padded by header and footer bytes which have fixed, known signature values. By looking at all the allocations in the heap and checking their header/footer bytes, data overwrite can be detected since if overwriting has occurred, the padding bytes will (probably) have changed value.

### How is heap performance tested?
See "What are the common heap performance metrics?" above. Exact implementation details of heap testing are beyond what this article can include. 

### How can heap overflow be avoided?
Generally, allocate a sufficiently large amount of memory to the heap pool in the first place and also detect overflow conditions and, in the case of overflow, request more memory from the memory manager.

### When should the heap be expanded?
Whenever heap overflow occurs (or is about to occur). However, some limit should be placed on how large the heap is allowed to grow before an error is raised otherwise the entirety of system memory could be consumed. Also, there is a limit to available physical RAM which will prevent the heap growing indefinitely.

### When should the heap be contracted?
If a significant portion of the heap pool goes unused for a long period of time then the heap can be contracted. It is unwise the contract the heap as soon as a block of the pool goes unused since it is likely that a new allocation will occur fairly quickly that will require the heap pool to be larger.

## Extended information - Threading &amp; Interrupts

This section includes some extended information about problems relating to the use of the heap in multi-threaded situations and during interrupts.

### What is thread (/asynchronous) safety?
Code which is thread safe is code which can be called by two threads simultaneously without theoretically causing conflict. For example, a thread safe list is one where items can be added and removed by two threads at once without the risk of one thread's new item overwriting the other's or both threads removing the same item. Most heap implementations are not thread safe and either require a locking mechanism around them or intelligent programming to avoid conflicts (though the latter is very difficult, often solved by locks elsewhere).

### Why can't I allocate or free memory during an interrupt routine?
Suppose your kernel code is executing normally and starts making a memory allocation i.e. running the heap's allocation function. The allocation function (if it isn't thread safe) could succeed in finding a block of memory but not marking it as allocated when an interrupt then occurs. If the interrupt hander (or any sub-routine) then tried to allocate memory, it may receive the same block of memory. When the interrupt ends the original allocation call will be none-the-wiser and thus the same block of memory will be allocated twice.

Thread-safe heaps generally use some form of locking mechanism which relies on being able to pause one thread while another completes its allocation request then resume the original thread. If such a locking mechanism is used and an interrupt occurs during an allocation call and then the interrupt handler (or sub-routine) tries to request an allocation, it will get stuck on the lock. But an interrupt prevents other (normal) threads running until it returns (IRet) so the interrupt handler will become stuck on the lock indefinitely. Even if the timer interrupt is allowed to continue, returning to the interrupt handler after continuing a thread is extremely problematic, if not impossible on some architectures.

### How does thread safety affect the heap?
Generally, some form of locking mechanism will be required to keep the heap thread-safe. Unfortunately, this reduces heap allocation (and possibly free) performance. Genuinely thread-safe heaps (that don't use locking) are usually more complex to implement and require more metadata. Even a thread-safe heap may still not be usable inside of an interrupt (sub-)routine.

### How can I avoid allocating or freeing memory in an interrupt routine?
You should allocate memory ahead of time from a normal thread (e.g. the kernel or driver thread) and deallocate (free) after the interrupt (if desired). The allocated memory should be stored somewhere accessible to the interrupt handler. You will need to implement mechanisms for detecting when pre-allocated memory has been used and more is required and/or mechanisms for allocating memory when an interrupt is expected to occur. If an unexpected interrupt occurs, you program/driver/kernel should probably crash/panic because unexpected interrupts mean you didn't write the program properly.

---

# Hardware

Different architectures have different requirements for memory management. The following sections provide some notes on those requirements as experienced by FlingOS developers.

## Important Notes
MIPS and x86 use different conventions for naming sizes of data thus the following sections use different naming conventions. The following table summarises:

| Size (bytes) | x86 Name | MIPS(32) Name |
|:-------:|:---------------:|:-----------------:|
| 1 | Byte | Byte |
| 2 | Word | Halfword |
| 4 | Double-word (Dword) | Word |
| 8 | Quad-word (Qword) | Double word |

## MIPS
This information pertains to MIPS 32-bit architectures. It is unknown whether the same information is correct for 64-bit versions of the MIPS architecture.

### Memory Layout
The MIPS memory layout is not as straightforward as x86 because some areas of mapped to the same physical memory but through different levels of caching. Our advice is, if you don't quite know what you're doing and just want to get something working, allocate your heap as part of your kernel's .bss section and work from that. Once you understand the MIPS architecture better, you can then look at dynamic allocation of blocks of memory to the heap pool.

### Data Alignment
MIPS requires data to be aligned to the size you intend to access. I.e. accessing one byte requires no alignment, accessing a halfword (2-bytes) requires two 2-byte alignment and accessing a word (4-bytes) requires 4-byte alignment. If you are using the GCC compiler, it will try to keep data and fields aligned for you. However, your heap is a dynamic, runtime structure that GCC knows nothing about so cannot help you with. Best practice is to keep all allocations 4-byte aligned so far as possible. When unaligned access is required you will have to use software to do individual/split-up loads/stores of the bytes of your value.

MIPS includes a misaligned access exception (interrupt). This can be used to catch misaligned accesses and correct them in software. However, the optimised Linux software for doing this claims to be 1000 times slower than a properly aligned access to catching misaligned accesses like this is a bad thing to rely on. (See [Linux-mips.org - Alignment])(https://www.linux-mips.org/wiki/Alignment#Transparent_fixing_by_the_kernel)).

## x86

### Memory Layout
x86 has no required memory layout though most systems use paged virtual memory. Thus pages are the large-blocks added to the heap pool. Though it is uncommon, you may at some stage want to allocate data larger than a page from the heap, in which case, the heap pool will need to contain contiguous pages.

### Data Alignment
x86 does not require data alignment but it can be switched on in the processor configuration. In any case, best performance is achieved if data is kept double word (dword, 4-byte) aligned.

---

# Software

## Overview

### Typical usage description
The following lists the various typical steps when setting up and using a heap (it assumes the x86 architecture is being used):

1. \[System Initialisation\]
2. Memory manager creates heap &amp; allocate it some pages of memory
3. Function calls alloc for 16-byte structure, stores resulting pointer in a global variable
4. Function calls alloc for another 16-byte structure, stores resulting pointer in a field in the previously allocated structure.
  
  At this stage a linked-list has been formed.
5. Further calls to alloc and extension of the linked list.
6. Function decides to remove an item from the linked list. Copies the "next" pointer field of the element to remove into the "next" pointer field of the element previous to the element to remove. Then calls free (a.k.a. dealloc) on the pointer to the element to remove.
7. Further adds/removes occur
8. System decides to clean up the linked list. Free (a.k.a. dealloc) is called for all the pointers to elements in the list including the root element. The global variable for the list is set to null.

### Considerations for microkernels
You will want to keep all the parts of the memory management nicely separated. If you don't, the system will probably end up slow, buggy, unstable and impossible to work with. Make the main address space manager a dedicated part of the microkernel. You will want a generic heap implementation that can be used as part of the kernel, drivers or programs. Create a dedicated (protected) heap for the kernel and a separate one for shared memory and allocate them pages using your virtual memory manager. For drivers and programs, use system calls to call the virtual memory manager to get it to add pages to the program's address space for use in the heap.

### Standard library implementations
There are hundreds (if not thousands) of implementations to choose from that come built into various libraries. Most of these are written in C but you can find some C# or Java examples online. Most C implementations can easily be converted (license permitting). Here is a short list of available standard libraries (originally from [OSDev.org - Memory Allocation](http://wiki.osdev.org/Memory_Allocation)):

- [liballoc](https://github.com/blanham/liballoc/) - Excellent allocator that was originally a part of the Spoon Operating System and designed to be plugged into hobby OS's.
- [dlmalloc](http://g.oswego.edu/dl/html/malloc.html) - Doug Lea's Memory Allocator. A good all purpose memory allocator that is widely used and ported.
- [TCMalloc](http://goog-perftools.sourceforge.net/doc/tcmalloc.html) - Thread-Caching Malloc. An experimental scalable allocator.
- [nedmalloc](http://www.nedprod.com/programs/portable/nedmalloc/) - A very fast and very scalable allocator. These two properties have made it somewhat popular in multi-threaded video games as an alternative to the default provided allocator.
- [ptmalloc](http://www.malloc.de/en/) - A widely used memory allocator included with glibc that scales reasonably while being space efficient.

### Does the kernel or driver or program create the heap?
They each create their own heap(s) and request blocks of memory for the heap pool from the kernel's (virtual) memory manager (either via direct calls or via a system call).

## Implementation details

### Creation
All heaps will require some form of creation which sets up global data (for example, to keep track of the blocks/pages in the heap pool). The basic initialise function should take in a single block and use some or all of it to create a global data structure. The rest can be used for normal allocation. You will need two additional functions for expanding and contracting the heap. An expand function can be called after initialisation prior to any allocs to create a large initial heap size. Expansion and contraction are described in more detail below.

### Allocation
The allocate function will have to search through (by some algorithm) all the blocks in the heap's pool looking for a large enough, empty area of memory that can be allocated. The allocate function must be careful not to assume blocks in the pool are contiguous.

The allocate function will usually take two parameters, one for the size of the requested amount of memory and one for any alignment requirements. Zero-byte or one-byte alignment are considered equivalent and are essentially no alignment. Most heaps will only accept power-of-two alignment requests meaning 3-byte alignment requests or similar are not possible. The allocate function searches through the available (i.e. unallocated) memory to find a sufficiently large area for the requested size (that is also correctly aligned). The allocate function returns a pointer to the start of the allocated block of memory. 

For debugging, it is also useful to have a third parameter which is an identifier (a string if possible) which identifies what code made the allocation request. This can be printed to a console and/or stored in metadata for the allocation which allows a debugger (or super-dedicated programmer) to determine what every allocation was for and where it came from. This can significantly help when trying to track memory leaks (places where free is not called for allocated memory) or over-freeing (places where memory is mistakenly freed twice). The latter is rarer but often very hard to notice and/or realise what is happening because it is very obscure. 

If the heap is full or overflow, the allocate function has three choices (in order of best to worst):

1. Call the (virtual) memory manager to request an extra block for the heap pool. Then attempt to allocate again.
2. Return a null pointer and let the caller handle the error.
3. Crash the program (if the program is the kernel, this is a kernel panic - Blue Screen of Death in Windows).

### Freeing / Deallocation
The allocate function is complemented by a "free" (a.k.a. deallocate) function which takes a pointer to a block of allocated memory and "deallocates" it i.e. marks it as available for allocation again. This will only require the pointer to the start of the memory to free. It shouldn't need to the size to free. If it does then your heap is probably poorly implemented and it is also a security risk since programs could try to free less or more memory than they should which is a potential point of weakness for an attacker to exploit.

### Expansion
Expanding the heap means expanding the heap pool by adding additional blocks of memory to it. In the x86 architecture, blocks are usually added as pages. Expanding should be fairly easy and fast. Note that if you continuously add non-contiguous blocks to the heap pool, your heap may run slower as it tries to find contiguous blocks of memory. For the same reason, a non-contiguous heap pool can also increase heap fragmentation. If a program requests an allocation larger than the largest, free, contiguous part of the heap pool then the allocation request will fail or more, contiguous blocks will have to be added to the pool.

Expansion of the heap should be limited to a maximum (either fixed or dynamic depending on the environment such as amount of available RAM). This is to allow for detection of memory leaks and to prevent all of system memory being consumed by the kernel (or a program's) heap.

### Contraction
Contraction is a tricky problem and timing it correctly so that it doesn't affect heap performance is trickier still. To be able to remove a block from the heap pool, it must be completely empty i.e. not contain any allocated memory (in whole or in part) In a heap being used continuously this can be hard to achieve (particularly if a managed memory system isn't being used). 

One approach is to mark a block of the pool as "not to be used" which means that it shouldn't be used for new allocations. Thus, over time the block will have all allocations within it freed (hopefully). Once the block becomes empty of allocations it can then be safely removed from the pool. Note that the pool containing the heap global data can not be removed unless the global data is copied to another block in the heap pool. Doing so is probably excessively complex compared to just not trying to contract the block containing the global data.

### Inspection
Inspecting the heap has three main parts:

1. Evaluating allocated/free memory : This should be simple - just sum up the amount of allocated or free memory (and you can subtract from the total pool size to obtain the other value).
2. Viewing allocation locations/sizes : This should also be simple as it is a core part of what any heap must keep track of.
2. Evaluating fragmentation : This is a difficult thing to do as what counts as wasted-space is rather subjective. However, some heap implementations rely on splitting up the pool into 16-byte (or similar sized chunks) of memory. Evaluating how many of these lie between allocations and/or how many bytes within each block were allocated by actually pad the original requested size, can provide a useful metric of wasted space.
3. Detecting data overwrite : Again this is difficult and will require extra padding bytes before and after allocations in order to detect when memory was written to which shouldn't have been.

### Inclusion in the kernel build: .bss vs. .data
To create a small (or large) statically allocated heap at compile time for the kernel to initialise/use prior to full (virtual) memory management being initialised, the following sample code can be used.

#### MIPS (GAS)
``` armasm
.bss
/* 104,857,600 bytes = 100MiB
KernelFixedHeap_Start: 
.space 104857600
KernelFixedHeap_End:
```

#### x86 (NASM)
``` x86asm
SECTION .bss
; 104,857,600 bytes = 100MiB
KernelFixedHeap_Start: resb 104857600
KernelFixedHeap_End:
```

---

# Example Code

## Overview
TODO

## Download
TODO

---

# FAQ & Common Problems

## Out of memory
Running out of memory is a common problem if the initial heap pool isn't big enough. Either create a bigger initial pool or implement expansion. If you've used up all of the system's memory, what the hell were you doing?!? On a modern system with gigabytes of RAM, how did you do that? You probably have rogue code or memory leaks. 

If you're in an embedded environment you may only have a few kibibytes of RAM available, in which case you will want to think carefully about your design to ensure the memory management system isn't using up lots of memory needlessly. Embedded devices often compromise on features in order to optimise to the required size. Compromises include security features such as separated heaps / isolation being ditched, on the basis that an embedded device is usually written and installed once with no additional software installed by a user and thus no risk from foreign applications. However, in the modern Internet of Things environment, this argument is becoming less strong as many embedded devices now allow installation of apps and scripts.

## Overflow
Memory overflow can be detected in two ways:

1. Data written to blocks not allocated to the heap and/or page-faults/access-violations for segments/pages following a block of memory in the heap pool.
2. Heap reporting out of memory when a separate counter tracking calls to alloc/free says memory should be available.

Case two requires more detailed thought and explanation. If separate (signed integer) counters are used to keep track of how much memory has been allocated/freed just by calls to alloc/free it can show if the heap is getting out of sync. For example, if free isn't actually freeing allocations properly. What can occur is the heap thinking its allocating a pointer to a block but then accidentally returning a pointer to somewhere else. Often the pointer value ends up multiplied by 4 due to incorrect use of pointers / bad pointer arithmetic. As a result, the heap thinks its allocating blocks properly, but calls to free will be made with invalid pointers and normal code will be reading/writing invalid pointers which can present as heap overflow.

## Data overwrite
This can occur in three situations:

1. Buggy programs not using pointers properly. 
2. Heap not returning pointers properly due to incorrect pointer arithmetic
3. Multiple freeing of the same block of memory meaning two separate alloc calls return the same block of memory meaning the memory gets used by two separate things simultaneously.

## Misalignment
This is most likely to occur on architectures for the embedded market such as MIPS and ARM. You will need to check both your heap allocation code to check returned addresses are aligned and the program code to check it is using aligned offsets from the pointer.

Note that the returned address must be aligned, not the address of the allocation. Some heaps prefix allocations with metadata about the allocation. It is common to see mis-implementations where the prefix data is aligned but the actual pointer returned to the program is not. 

---

# Further Reading

- [OSDev.org - Heap](http://wiki.osdev.org/Heap)
- [OSDev.org - Memory allocation](http://wiki.osdev.org/Memory_Allocation)
- [OSDev.org - Simple Heap Implementation(s)](http://wiki.osdev.org/User:Pancakes/SimpleHeapImplementation)
- [Wikipedia.org - Heap](https://en.wikipedia.org/wiki/Heap)
- [Wikipedia.org - Memory Management](https://en.wikipedia.org/wiki/Memory_management)
- [Wikipedia.org - Heap Overflow](https://en.wikipedia.org/wiki/Heap_overflow)
- [Wikipedia.org - Heap (data structure)](https://en.wikipedia.org/wiki/Heap_(data_structure))
- [JamesMolloy.co.uk - 7. The Heap](http://www.jamesmolloy.co.uk/tutorial_html/7.-The%20Heap.html)
- [CProgramming.com - Heap](http://www.cprogramming.com/tutorial/computersciencetheory/heap.html)
- [GribbleLab.org - 7. Memory : Stack vs Heap](http://gribblelab.org/CBootcamp/7_Memory_Stack_vs_Heap.html)
- [StackOverflow.com - What and where are the stack and heap](http://stackoverflow.com/questions/79923/what-and-where-are-the-stack-and-heap)
- [YouTube.com - Introduction to a heap (algorithm)](https://www.youtube.com/watch?v=c1TpLRyQJ4w)
- [OSDever.net - Memory Management 1](http://www.osdever.net/tutorials/view/memory-management-1) (also see part 2)
- [Wikipedia.org - Data structure alignment](https://en.wikipedia.org/wiki/Data_structure_alignment)
- [Wikipedia.org - Fragmentation](https://en.wikipedia.org/wiki/Fragmentation_(computing))

---
layout: reference-article
title: Memory
date: 2015-09-05 12:47:00
categories: [ docs, reference ]
description: Describes general concepts about memory and some specifics of x86 and MIPS architectures.
---

# Introduction

Memory is a broad topic covering many key concepts at the kernel, driver and user mode levels. Ultimately, a computer's task is to process data from one form into another and since data is stored in memory, a computers main task is to manage memory and move it around. 

At the kernel level, there are three main types of memory management: large-area management (including security), small-area management and the stack. Large-area management generally means managing areas of memory that are a page or larger (4KiB or larger). This is achieved using a central memory manager and often virtual memory. Small-scale area management is generally anything less than a page and is achieved using a heap. 

Large-scale management usually requires managing the entire address space and often uses a virtual memory system. Since it manages the whole address space, there is generally only a single memory manager that lives within the kernel and is protected from the rest of the system (for security). Small-scale management usually happens per-process using a heap. A stack is an even smaller level of management for temporary values and fast access and exists per-thread.

## Scope of this article

This article will focus on memory and memory management for OS development. It will cover basic hardware, physical memory, virtual memory, x86 real and protected modes, the concepts of segmentation and paging and descriptions of software for managing memory.

---

# Overview

## Memory Management

Memory management is the system of dividing up memory at runtime when another part of the system makes a request. Memory management is split into three levels: large-scale (generally a virtual memory manager), small scale (generally a heap) and stack.

Large scale memory management generally manages the entire address space and there is only one manager for the entire system. Small scale memory management generally requests large blocks from the large scale manager and then handles splitting it up into small areas (e.g. for structs). A small scale manager generally exists per process and is called a heap. 

Stacks (which exist per thread) are generally allocated by the large scale manager (since a virtual memory manager can provide memory protection) and are treated separately from the heap. Stacks are used for subroutine calls and storing small (typically 4 bytes) temporary values.

## Physical vs. Virtual memory

Physical memory refers to the real bits and bytes in hardware. It also means that when you use an address, it refers directly to the real byte in hardware with no translation in between. 

Virtual memory refers to any system where by when you use an address, it is transformed or translated (by hardware or software, whether fixed or programmable) to a different, physical address. Virtual memory generally allows any address to map to any physical address, though often addresses have to be mapped in blocks. This also allows multiple virtual addresses to be mapped to the same physical address. Often there is also a present/not-present system which allows virtual addresses to map to "no address" which causes a hardware exception if the virtual address is used.

Virtual memory has great benefits for both performance and security. From a performance point of view, it allows a process to have an entirely separate addresses space from all other processes. This means the process code, data and shared libraries can appear to overlap in the virtual address space but be entirely separate in the physical address space. It also allows more efficient use of physical memory since non-contiguous physical memory can be made contiguous in the virtual address space.

Virtual memory brings security and stability benefits since separate address spaces for processes means that a process can be restricted to only accessing its own memory. Hardware exceptions triggered by accessing memory not mapped in the process' virtual address space allow the kernel to terminate malicious or buggy programs.

A final benefit of virtual memory, which can only be utilised if the operating system has disk drivers, is swapping memory to disk. This is when parts of physical memory that contain program data for a program which is not executing, are saved to external storage such as a hard disk. When the program begins to execute again, the memory is copied back from disk to physical memory. Swapping memory to and from the hard disk is commonly confused with being the only or main feature of virtual memory; It is not. It is an additional system and it only exists if OS software implements it; It is not performed by hardware. It also is not a necessary feature when using virtual memory and could (technically) be implemented even if you are only using physical memory.

Unfortunately, virtual memory does come at a cost. Every address is being checked and translated for every operation which uses a memory address. This means that there is an overhead for memory access which slows the system down. To reduce this effect, most architectures come with caches on the CPU which cache the most recently used translations. This means whenever a virtual address mapping is changed, the cache must be invalidated (also known as flushed). This increases system complexity. Also, using virtual memory for processes increases the context switching overhead, especially if the process has been swapped to disk and needs to be swapped back in. 

At the end of the day, the benefits of virtual memory significantly outweigh the costs and its security improvements are an absolute necessity in modern systems.
 
## Types of virtual memory

There are two distinct types of virtual memory: segmentation and paging. Segmentation is largely used by embedded systems while paging has all but replaced segmentation on full-scale processors.

### Segmentation

Segmentation divides the virtual address space into variable size blocks which have a start address and length. A block is mapped by setting a physical start address for the block. There is usually a limit on the total number of blocks which can be specified in a system.

Some hardware (such as the MIPS architecture) uses fixed hardware segmentation. For example, kseg0 and kseg1 in MIPS map to the same physical memory but kseg0 goes through the processor caches while kseg1 does not. 

Segmentation is simpler (both conceptually and for software) to configure and manage and requires less hardware and memory for small systems. When a programmable system is used, the hardware is supplied a pointer to a segmentation table which has descriptors for all the segments (blocks) defined by the kernel. The table need only be as big as required to describe all the required segments. This means there is no wasted memory and the amount of required memory grows proportionally with the number of segments. This makes it very efficient for small embedded systems.

However, for large numbers of segments (which rapidly occurs in large scale systems with GiB of memory) the speed of access under segmentation drops off significantly. This means that it is slow especially in comparison to paged memory systems. Furthermore, a performance efficient segmented memory system does not allow swapping only parts of a program's address space to and from disk. For these reasons, segmentation has been replaced by paging in large scale systems with GiB of memory.

### Paging

Paging divides the virtual address space into fixed sized blocks called pages. Each page is assigned a physical page (/physical address) to translate to. Every page in the virtual address space must be either marked as unmapped (often called not-present) or be given a physical address. 

Paging is usually implemented using a page directory and page tables. The page directory holds pointers to page tables, and the page tables hold the physical addresses the pages map to. For a 4GiB address space split into 4KiB blocks, the page directory would contain 1024 pointers to page tables and each page table would contain 1024 entries. (This can be shown to be correct by 1024 * 1024 * 4KiB = 4GiB). Using 32-bit addresses, the top 10 bits would specify the entry in the page directory and the next ten the entry in the page table pointed to by the page directory. The bottom twelve bits are discarded since 4KiB blocks are being used. In x86, the bottom twelve bits are used for attributes/flags.

Paging allows more detailed control over the address space but at the same time makes managing a large address space (>=4GiB) efficient (when compared to equivalent segmentation). It also allows bits of programs to be split up meaning programs can be swapped to/from hard disk independently from their shared libraries. Swapping pages to/from hard disk is what gives rise to the (in)famous page file. 

Many systems (e.g. USB) use 4KiB blocks as a base size for buffers since they expect the host system to be using paged virtual memory. A lot of optimisation goes into making page-size and page-aligned access very fast.

## x86 Real vs. Protected mode

Real mode is the original mode (or design) of the x86 processor. In real mode you cannot use the full extent of memory and virtual memory is not available. 

Protected mode can (and should) be switched to after your OS boots. In protected mode, virtual memory is available and other processor features too. Protected mode was introduced to allow isolation of programs from one another and to allow the kernel to strictly control what programs can and can't do; Hence why it is called Protected mode.

Protected mode was necessary because in real mode, any program could read or write another program's memory. This made systems very susceptible to viruses and just plain buggy programs. A simple buffer overrun could be sufficient to crash a real-mode system. Protected mode allows the kernel to prevent this and to get early warning notices (i.e. interrupts) when a program starts to go wrong.
  
---

# Hardware

## RAM vs other types

There are lots of types of data storage within a computer. Fast, temporary storage used for program execution and data manipulation is RAM but long-term (often referred to as permanent) storage, used usually for large volumes of data, includes Hard Disk Drives, Flash memory sticks and CD/DVDs. The two key differences between long term and short term (which can also be described as non-volatile and volatile) storage are that with volatile memory you can access any single byte at any time very quickly but the value will only stay there for as long as the memory has power. Non-volatile storage does not require constant power but you can usually only access blocks (512 bytes or larger) at a time. 

One technology which might start to change this is Solid State Drives (SSDs). These use a form of flash storage for very fast read/write access. However, they still use large block sizes for accessing so cannot be used for RAM yet.

## Types of RAM hardware

RAM actually comes in various physical forms. There are two fundamental types of Random Access Memory: Dynamic RAM (DRAM) and Static RAM (SRAM). The latter is more expensive and physically complex (meaning it can achieve as high data density) but is faster much faster and so is usually used for CPU caches. DRAM requires more power, is a bit slower but is cheaper and (because of its physical simplicity) can achieve higher data density. 

 The most basic DRAM commonly in use was SDRAM (Synchronous DRAM) but this has been rapidly replaced by Double Data Rate SDRAM (DDR SDRAM). There have been multiple versions of DDR up to and including the latest DDR4. Rambus DRAM is the most recent, popular development which is even more expensive but significantly faster than DDR.
  
## Memory Management Unit (MMU)

The Memory Management Unit is the piece of hardware which translates a virtual address to a physical address. Every memory reference made by CPU instructions is passed through the MMU. On x86 hardware, the MMU also handles I/O port addressing by accepting extra information from in/out instructions. The MMU may also be linked to a cache known as a Translation Lookaside Buffer (TLB for short) which acts as a fast cache of the most recently looked up virtual to physical translations. 

The MMU requires to aspects of programming. The first is the configuration of whatever virtual memory system is in use whether that be segmentation or paging. This is what configures the translations from virtual to physicall addresses. The second aspect is flushing of the TLB (or equivalent cache). This has to occur whenever a virtual to physical translation is changed in the translation table since the cache may contain a copy of the translation. On some architectures the cache is automatically flushed for some changes. Also, it is often possible to flush a particular translation rather than the entire table (which would be a costly process).

## Caches

CPU caches are generally made of SRAM and sit between the processor and the actual memory. How caches function depend very much on the system on chip (SoC). It is common for three levels of caches to exists, known as Level 1 (L1), Level 2 (L2) and Level 3 (L3) caches. They are layered and generally increase in size and effective distance from the processor. Thus L3 is slightly slower than L1. 

One or more levels may be split for data fetches and instruction fetches. This means there are effectively two separate caches. One is used for instruction fetches and the other for data read/write. This means that modifying code in memory (which is done via data fetches) goes through a separate set of caches to instruction fetches. Thus both the data and instruction caches must be flushed or cleared in order for new code to be fetched.

Some architectures provide special flush or sync instructions to flush the instruction and/or data caches. On MIPS, the Synci instruction can be used to flush the data cache and clear the instruction cache. MIPS also has a segment (kseg1) which maps to the same area of physical memory as kseg0 but doesn't go through the processor caches. On x86, the caches are maintained automatically.

## Detecting memory size

Detecting memory size is a common task that an OS has to perform (since the amount of physical RAM is going to be one of the biggest constraints on the system). Unfortunately, there aren't many easy or reliable ways to do this. The recommended route is to use BIOS calls to get the BIOS to tell you how much RAM is available. Unfortunately, if your OS switches to protected mode almost as soon as it starts (which is also recommended) then you won't be able to use BIOS calls. The only alternative is to use a bootloader which supports reporting of RAM size. 

There are alternative techniques which involve probing RAM (which is best done in ASM) but it is not guaranteed to work on all systems especially older ones in which ISA video cards might be present (which extend the amount of apparently accessible RAM).

A practical article covering this topic in detail can be found on OSDev.org : [http://wiki.osdev.org/Detecting_Memory_(x86)](http://wiki.osdev.org/Detecting_Memory_(x86)).

---

# Software

## Overview

Memory management software, as has been described, comes in three layers. The central, large scale manager that should reside within the kernel. Small-scale managers (called heaps) which exist per-process (including the kernel process) and are allocated from the large scale manager. Lastly, stacks (which exist per thread and are allocated by the large-scale manager). 

## Basic outline

When your operating system first starts it will no doubt be running in real mode and without virtual memory. Thus your initial tasks are the following:

1. Switch to protected mode
2. Initialise a stack using statically allocated memory (i.e. memory allocated in the .bss section of your binary)
2. Configure a GDT (with segments covering all attribute types and DPLs across the entire 4GiB address space (even if using 64bit mode))
3. Set up page directory and page table structures
4. Load the process with the page directory

If you're using a higher-half kernel, you will probably need multiple stages of virtual memory initialisation. Once to temporarily patch the memory followed by jumping to the higher half and then another stage to load your actual virtual memory setup.

Once virtual memory is executing normally, your system should follow these steps:

1. Initialise a small heap using some statically allocated memory (i.e. memory allocated in the .bss section of your binary)
2. Initialise your large-scale manager using memory from the small heap
3. Mark your existing small heap memory and stack memory as used within the large scale manager
4. Grow the small heap by allocating pages from your large scale manager

Your system should now be set up to function following normal princples (as described in other articles covering stacks, heaps and memory management). 

## Garbage Collection

It would be wrong of us not to mention garbage collection, since it is core to any C# system. There are many people who argue that garbage collection at the OS/kernel-level is impossible. It is obvious that FlingOS (and Microsoft's own Singularity C# OS) have disproven this from a "yes it can be done" stand point.

There are still valid arguments against garbage collection at the low-level. At least in comparison to well-written code which manually manages memory, garbage collection will never perform as well. GCs also imply more wasted memory since the GC has to keep track of stuff which in and of itself uses more memory.

However, a lot of time and effort goes into debugging manually memory managed software and a well written garbage collector can help eliminate this. Furthermore, there has yet to be any definitive research into whether a garbage collector on a modern, full-scale processor actually has a noticeable performance penalty. It is clear though that on embedded devices, where only small (kibibyte) amounts of RAM are available, a garbage collected system would be vastly inefficient.

## Technical details
**Enumerations, classes, other such details**

The x86 standard memory (usage) map can be found at: [wiki.osdev.org/Memory_Map_(x86)](http://wiki.osdev.org/Memory_Map_(x86)) and its external links. 

A very useful reference for the MIPS address space can be found at: [JonLoomis.org - PIC32 Memory](http://www.johnloomis.org/microchip/pic32/memory/memory.html).
  
## Implementation details
**Methods, steps, etc.**

Large scale managers will need the following basic functions:

- Internal: Allocate physical page (allocates (i.e. gets and marks used) a free page of the physical address space)
- Internal: Allocate virtual page (allocates (i.e. gets and marks used) a free page of the virtual address space)
- External: Map free page (maps a free page of virtual memory to a free page of physical memory)
- External: Unmap page
- Internal or external: Map virtual to physical page (may be required for mapping memory-mapped devices)

Small-scale managers (heaps) are described in the [Heaps article.](/docs/reference/Heaps) Stacks are described in [their respective article](/docs/reference/Stacks) too.

---

# Further Reading

- [OSDev.org - Memory Management](http://wiki.osdev.org/Memory_management)
- [OSDev.org - Detecting memory (x86)](http://wiki.osdev.org/Detecting_Memory_(x86))
- [OSDev.org - Memory Management Unit](http://wiki.osdev.org/Memory_Management_Unit)
- [OSDev.org - Page Frame Allocation](http://wiki.osdev.org/Page_Frame_Allocation)
- [OSDev.org - Writing a memory manager](http://wiki.osdev.org/Writing_a_memory_manager)
- [OSDev.org - Garbage Collection](http://wiki.osdev.org/Garbage_collection)
- [OSDev.org - Paging](http://wiki.osdev.org/Paging)
- [Hardvard.edu - Virtual Memory](http://www.eecs.harvard.edu/~mdw/course/cs61/mediawiki/images/0/0b/Lectures-virtmem.pdf)
- [Wikipedia.org - Virtual Memory](https://en.wikipedia.org/wiki/Virtual_memory)
- [PCMag.com - Types of memory](http://www.pcmag.com/encyclopedia/term/46788/memory-types)
- [Geek.com - Deifference between real mode and protected mode](http://www.geek.com/chips/difference-between-real-mode-and-protected-mode-574665/)
- [BrokenThorn.com - Processor Modes](http://www.brokenthorn.com/Resources/OSDev4.html)
- [On-time.com - Real Address Mode](http://www.on-time.com/rtos-32-docs/rttarget-32/programming-manual/x86-cpu/real-address-mode.htm)
- [RedHat.com - Virtual memory](https://access.redhat.com/documentation/en-US/Red_Hat_Enterprise_Linux/3/html/Introduction_to_System_Administration/s1-memory-virt-details.html)
- [TutorialsPoint.com - Virtual memory](http://www.tutorialspoint.com/operating_system/os_virtual_memory.htm)
- [Duartes.org - Memory translation and segmentation](http://duartes.org/gustavo/blog/post/memory-translation-and-segmentation/)
- [C-Jump.com - Memmory modes](http://www.c-jump.com/CIS77/ASM/Memory/lecture.html)
- [Columbia.edu - Segmentation and Paging](http://www.cs.columbia.edu/~junfeng/12sp-w4118/lectures/l05-mem.pdf)
- [ComputerMemoryUpgrade.net - Types of computer memory and common uses](http://www.computermemoryupgrade.net/types-of-computer-memory-common-uses.html)
- [Wikipedia.org - Dynamic random access memory](https://en.wikipedia.org/wiki/Dynamic_random-access_memory#Double_data_rate_.28DDR.29)
- [PCMag.com - Types of RAM](http://uk.pcmag.com/cpus-components-products/66029/feature/ddr-vs-ddr2-vs-ddr3-types-of-ram-explained)
- [Technibble.com - Types of RAM](https://www.technibble.com/types-of-ram-how-to-identify-and-their-specifications/)

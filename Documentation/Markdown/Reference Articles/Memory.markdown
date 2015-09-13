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

  - What is real mode
  - What is protected mode
  - Why is protected mode necessary
  
---

# Hardware

## RAM vs other types

## Types of RAM hardware

  - Flash
  
## Memory Management Unit (MMU)

## Caches

## Detecting memory size

---

# Software

## Overview

## Basic outline

- Memory Management
    - Large area management
        - Pages/segments
    - Small area management
        - Heap
        - Stack
        - Garbage Collection
- Configuring virtual memory
- Cache flushing

## Technical details
**Enumerations, classes, other such details**

- Memory Maps
  - x86
  - MIPS
  
## Implementation details
**Methods, steps, etc.**

---

# Further Reading

*[acronym]: details

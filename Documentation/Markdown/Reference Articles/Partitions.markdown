---
layout: reference-article
title: Partitions
date: 2015-07-22 22:44:00
categories: [ docs, reference ]
parent_name: Disk Devices
---

# Introduction

## Overview
Partitions in this context mean disk partitions. There are similar concepts in computing for databases, memory and virtual/logical partitions which will not be discussed in this article.

Partitions are an essential part of techniques for storing data on a disk such that it can be reliably retrieved (or even recovered if corrupted). As such, it is essential knowledge for any low-level developer who intends to use a hard disk or other similar storage medium (including USB sticks).

## Scope of this article
This article will cover disk partitioning in general and make mention of the various common partitioning schemes. However, each specific partitioning scheme has its own article.

---

# History
Partitions have a modest amount of history, most of which relates to any single partitioning scheme (particularly MBR on PCs). However, there is something to be said for where partitions came from.

Before hard disk drives existed there were essentially two alternative storage mediums; these were floppy discs and tape drives. Neither of the two mediums could store large amounts of data (by modern standards - I'm sure it was large at the time!) so there were two significant things nobody wanted to store on them:

1. More data than necessary - File systems were often highly optimised to minimise the amount of space wasted on file and folder descriptors.
2. A second operating system - Dual booting wasn't that widespread and was very expensive. Add to that the fact that despite the wide range of operating systems, they would often only work on one model of hardware and dual booting wasn't really an option.

So the idea of a system whereby you could divide a disc up into multiple, logically totally separate sections each for their own file system (or operating system) was simply not floated. Partitioning Schemes were just not necessary or sensible given how much space they would waste relative to the size of the disc.

This changed, however, when hard disks came along and the maximum amount of storage rapidly started to increase. People wanted to be able to split up their disk into multiple separate sections (for security, efficiency and probably partly because common file systems didn't support covering large disk sizes).

---

# Overview

## What is a partitioning scheme?
A partitioning scheme a system for dividing up the space on a storage device into one or more logically separate sections. Each section is called a partition. A partition usually contains a file system (or occasionally just an operating system). A single partition can cover the entirety of the storage space or the storage space can be divided into multiple partitions. Partitions do not have to cover the entire storage space i.e. there can be unallocated/unused sections.

## Why is a partitioning scheme useful? (Advantages)
Partitions are logically entirely separate (even if they reside next to each other on disk). This means you can have different file systems in each partition. This is useful for a number of reasons:

1. Multiple operating systems, which may require different file systems, can be installed.
2. Certain sets of data can be kept separate thus reducing risk of corruption or security breaches.
3. One partition could contain ordinary data while another contains an encrypted file system. One for the OS (or bootcode) and another for the user's files.
4. A system can optimise itself by storing frequently accessed or small files in a fast-read (or fast-write) file system in one partition, while storing larger or less frequently accessed files in another file system. The difference being that different file systems use different amounts of metadata for storing files so are better or worse at storing small/large files and/or accessing those file quickly.
5. Further optimisation can be achieved by keeping partitions small which keeps file systems small thus decreasing access times. Sensible positioning and accessing of partitions on disks can also increase average throughput of a disk.

## How is a partitioning scheme constructed?
Typically, a partitioning scheme consists of a table of entries where each entry specifies the starting location and length of a partition. Entries may also include additional information such as identifiers for the partition and also identifiers for the type of file system contained within the partition. It is common to have a partition for each installed operating system (usually only one, possibly two, rarely three or more). Separate partitions for users or applications to store data are also common.

## Disadvantages
There are several disadvantages to partitioning schemes, particularly if using lots of partitions. These are:

1. The partition table (and partition alignment requirements) reduce the space available on the disk for use by file systems.
2. If the OS (or user) frequently accesses two or more partitions on a single disk, it can reduce performance as the read/write head has to move relatively large distances back and forth to read from the separate areas.
3. Disk fragmentation (which is a measure of the largest available contiguous block of storage space) is made worse since the maximum contiguous block is reduced the to size of the largest partition (or even the maximum size of a file inside any of the file systems in any of the partitions).
4. Reduces portability as not all hardware (see below) or software (see below) will support a given partitioning scheme. It also increases the propensity for multiple types of file system (since the barrier to creating a new file system is reduced) and thus further decreases compatibility.

## Alternatives
There are alternatives to partitioning schemes, the main one being simply don't have partitions and store the file system directly on the disk. This possibly sounds silly but it forms the basis of one use of RAID (redundant array of independent disks) disks.

The idea is that each disk in the RAID array can store the equivalent of a partition, but physically there is only one "partition" per disk. A partition may span multiple disks but two partitions cannot cover any part of the same disk. This essentially means partitions correspond to disks internally but hides that fact from the OS (except during formatting). It significantly reduces some of the disadvantages described above while retaining many of the advantages of partitions (and RAID) ovcer just having lots of disks.

Another alternative is to store all the partition table information and file system tables (be there one or more, even of different types) at the start of the disk and thereby leave the entire rest of the disk for data. This does seem like only a modest change but increases levels of complexity with potential conflict between file systems. It also offers little advantage of conventional partitioning schemes.

---

# Hardware

## Overview
While partitioning schemes seem like they are the realm of software, they have a significant role to play in hardware too. Disk formatting primarily affects hardware that handles the boot process.

## Some Specific Hardware
On PCs, the BIOS (which is considered to be firmware and so rarely updated on a given machine that it is practically hardware) will expect a storage device such as a hard disk or USB stick to be MBR formatted. Similarly, on Apple Macs (pre-dating their switch to Intel processors), the Apple Partition Map (APM) format was expected.

Modern PCs (since around 2007) have started using UEFI in place of BIOS. UEFI expects a disk to be GPT formatted instead of MBR. However, GPT uses a protective MBR so older, legacy software (hopefully) can't accidentally corrupt the drive.

## Compatibility
There is little to no compatibility between most partitioning schemes. The only semblance of compatibility is GPT's use of a protective MBR, but that does not mean MBR software can read a GPT formatted drive. It merely means that older MBR software is unlikely to corrupt the data on the storage device.

---

# Software

## Overview
Partitioning software consist of three significant parts:

1. Reader: Reading partition tables to access file systems
2. Formatter: Formatting a drive (or reformatting a drive) to a new partitioning scheme or to add new partitions
3. Recovery: Recovering corrupted or otherwise lost data from a drive (which can include corruption of the partition table itself)

At a hobby OS level, only steps 1 and 2 need be implemented (and often step 2 can be avoided by using virtual disk editing software). Step 1 is clearly necessary. Step 2 is only necessary if there is no alternative way to format the storage device in question. Examples of alternative methods for formatting include:

* Piggy-backing a hard drive on a Linux, Windows or OSX machine to format it
* Plugging in a USB stick and formatting it as normal
* Using virtual disk software to format a virtual hard drive for use in a virtual machine

## List of Partitioning Schemes

The following is a list of some common partitioning schemes and the types of device they are found on. I have only included ones which developers are likely to come across in ordinary OS programming. If you're developing for a specialist manufacturer or for embedded devices, there may be many more that you will come across.

| Name | Device type(s)     |
| :------------- | :------------- |
| MBR       | PCs (prior to around 2007, still used on many) |
| GPT       | PCs (post-2007) and Macs-OSX (post switch to Intel processors around 2005)
| APM       | Macs (prior to switch to Intel processors around 2005)

## Compatibility
There is little support across operating systems for different partitioning schemes. OSX originally only accepted APM and will now only accept GPT (by default though there are guides on how to make it work with APM). Windows used to only accept MBR and modern versions will now only work with GPT (out of the box). Linux is probably the most compatible with versions available that support any of APM, MBR or GPT.

There are a few alternative partitioning schemes used by Linux and other Unix-like systems that you may come across if you encounter a machine that previously ran (or currently runs) one of those systems.

---

# FAQ & Common Problems

## Confusion with virtual memory
It seems strange but a significant number of developers confuse partitioning with virtual memory. This may not be helped by the fact that there is such a thing as memory partitioning but it is not the same as hard disk partitioning. A common confusion is the idea that partitioning allows the processor to save pages (or segments) of memory directly to a hard disk. It doesn't. Paging and disk partitioning are totally separate things. Furthermore, to save pages of RAM to a file on a hard disk requires a lot of software in between the processor and the disk!

---

# References

*[GPT]: GUID Partition Table
*[MBR]: Master Boot Record
*[APM]: Apple Partition Map

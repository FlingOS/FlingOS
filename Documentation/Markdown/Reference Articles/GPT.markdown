---
layout: reference-article
title: GPT
date: 2015-09-04 00:52:00
categories: [ docs, reference ]
parent_name: Disk Devices
description: Describes the GUID Partition Table partitioning scheme (commonly referred to as GPT).
---

# Introduction

## What is GPT?
GPT stands for GUID Partition Table (where GUID means Global Unique IDentifier). GPT is a method for splitting a disk up into sections (partitions) each of which is uniquely identified by its ID. Each partition also has a type ID which uniquely identifies the type of file system within the partition.

## How difficult is GPT to understand?
GPT is fairly simple to understand though an understanding of data layout on a hard disk in general is useful advance knowledge. Familiarity with the MBR partitioning scheme is also useful as GPT retains (vital) legacy support for MBR.

## How important / useful is GPT?
GPT is fast becoming the default / most popular way to format hard disks (especially in modern laptops / tablets) so it is both important and useful.

## Scope of this article
This article will cover background as to what GPT is, where it comes from and what problems GPT solves which previous partitioning scheme failed to. This article will then move on to a practical understanding of GPT concepts, how GOT organizes data on a disk and lastly software implementation details.

---

# Background

## Why are partition tables necessary?
Partition tables are necessary because frequently you want more than one file system to be physically present on a single disk. You may want multiple file systems simply for security and protection or as it allows you to use different file system types.

Also, multiple file systems allows you to install multiple operating systems side by side on the same disk, so long as both understand the disk formatting (i.e. the partition table). This is the case even if the two OSes can't understand each other's file systems.

## What came before GPT?
Before GPT there were two partitioning schemes in common use. The main one was MBR (Master Boot Record) which remains in use today. GPT has legacy support for MBR since MBR is built into nearly every PC boot-sequence in existence.

The second significant partitioning scheme was the Apple Partition Map (APM), which was in use on Apple's PowerPC based hardware. They have subsequently switched to GPT on their Intel based hardware. However, Intel Macs do have support for APM.

## What problems does GPT solve?
GPT solves a number of issues, most of which were issues with MBR. The key issues tackled are:
 
- 32-bit Logical Block Addresses
  
  GPT allows for a variable bit-lengths for Logical Block Addresses (LBAs) which are used for accessing sectors of data on the disk. This allows larger disks to be support. 
- Maximum 4 partitions
  
  GPT allows a minimum of 128 partitions to be defined in the partition table though more can be added (at the expense of "real" data on the disk).
- 512-byte sectors
  
  GPT improves support for 4096-byte size sectors where as much MBR software only supports (or assumes) MBR partitions to be on 512-byte sector-size disks.
- Partition identification
  
  GPT uses GUIDs to uniquely identify each partition. This means that no two partitions can become confused by identical naming (or at least, not by the computer).
- File system identification
  
  The GPT standard sets out GUIDs to identify all the types of file system which a GPT partition holds. These are well defined and easily accessible values. This an improvement over the often conflicted MBR file system identifiers.
  
- Use of CRC

  MBR was easily corrupted and there was no way to detect the corruption other than trying it and failing. Trial-and-error at runtime is a dangerous technique that also leads to security holes.

  GPT solves this issue by introducing the use of Cyclic Redundancy Checks (CRC32s). CRCs allow  software to read the GPT header and verify its integrity. If the CRC is wrong then the header is corrupted. The secondary GPT header (which is hopefully not corrupted but must also be checked) can hopefully be used to correct the primary GPT header.

---
  
# Overview
      
## General partition tables
In general a partition table simply splits a disk into one or more logical, contiguous chunks of data. What lies within a partition needn't be understood by an OS for it to be able to identify the partition. Equally, a partition does not have to contain a conventional file system, though more often than not it does. Notably, if a partition does not contain a valid file system, it usually means the partition is unformatted or corrupted.

Note that an unformatted partition is not the same as an unused partition. Unformatted means the partition has a chunk of data assigned to it, but the data itself is not in a valid or recognisable format. An unused partition means either the partition entry does not assign any data from the disk to that partition or the chunk of data being referred to has no partition entry associated with it. The latter case can occur if the total size of partitions on the disk doesn't cover the full disk or, where partition size boundaries cause cut off leaving unused areas of the disk.

## GPT Header
GPT has a header structure near the start of the disk which provides global information about
the GPT. This data includes information such as GOT signature, GPT Version, LBA of the Secondary
(/Backup/Recovery) GPT Header, Disk GUID, partition table entry size (usually 128 bytes) and more.


| Offset | Length | Contents |
|:---------:|:-----------:|:-------------|
| 0 (0x00) | 8 | Signature ("EFI PART", 45h 46h 49h 20h 50h 41h 52h 54h or 0x5452415020494645ULL[a] on little-endian machines) |
| 8 (0x08) | 4 | Revision (for GPT version 1.0 (through at least UEFI version 2.3.1), the value is 00h 00h 01h 00h) |
| 12 (0x0C) | 4 | Header size in little endian (in bytes, usually 5Ch 00h 00h 00h meaning 92 bytes) | 
| 16 (0x10) | 4 | CRC32 of header (offset +0 up to header size), with this field zeroed during calculation |
| 20 (0x14) | 4 | Reserved; must be zero | 
| 24 (0x18) | 8 | Current LBA (location of this header copy) |
| 32 (0x20) | 8 | Backup LBA (location of the other header copy) | 
| 40 (0x28) | 8 | First usable LBA for partitions (primary partition table last LBA + 1) |
| 48 (0x30) | 8 | Last usable LBA (secondary partition table first LBA - 1) |
| 56 (0x38) | 16 | Disk GUID (also referred as UUID on UNIXes) |
| 72 (0x48) | 8 | Starting LBA of array of partition entries (always 2 in primary copy) |
| 80 (0x50) | 4 | Number of partition entries in array |
| 84 (0x54) | 4 | Size of a single partition entry (usually 128) |
| 88 (0x58) | 4 | CRC32 of partition array |
| 92 (0x5C) | * | Reserved; must be zeroes for the rest of the block (420 bytes for a sector size of 512 bytes; but can be more with larger sector sizes) |
| | | Original source: [Wikipedia : GPT article](https://en.wikipedia.org/wiki/GUID_Partition_Table#Partition_table_header_.28LBA_1.29) |
| | | This table is licensed under Creative Commons Attribution-ShareAlike v3.0 License |

## Protective (Legacy) MBR
          
The Primary GPT Header is preceded on disk by a "protective" MBR. The protective MBR protects the
disk from corruption by older software (e.g. old BIOS) which does not understand GPT. The
protective MBR has a single entry which covers the entire disk (or as much as possible under
32-bit LBAs).

## GUID Partition Table

The GUID partition table can be treated as an array of structures. Typically, each entry is a
structure of 128 bytes but a larger size can be specified in the GPT Header. Each entry has a
specific format, as presented in the table below in the Data Layout section.

Each entry contains the GUID of the partition which should be used to identify the partition
in all cases. It also contains the GUID of the file system type within the partition. This can
be compared to the list of IDs provided by the GPT standard to find out the type of file system
with the partition and thus whether that partition can be read properly or not.

Usual information is also included such as first and last LBA of the partition along with
attributes and a human-readable name string (UTF16-LE).

## Data Redundancy / Recovery (Secondary GPT Header)
GPT has a reasonable level of redundancy built in to help with data recovery should the disk
become corrupted. This duplicate information comes in the form of the Secondary GPT Header
and Secondary GPT Partition Table both of which are located in the opposite order at the end
of the disk.

## OS Support
The vast majority of modern OSes support GPT and some of the latest bootloader standards
(including popular UEFI) have made GPT a requirement.

GPT has been supported in Windows since Windows Vista. Modern (Intel-based) Macs use GPT by
default. Linux and, importantly, GRUB have support for GPT along with various Linux drivers
and applications supporting GPT. A very useful tool for editing GPT is "gparted" on Linux.

# Data Layout

## Overall
The following figure presents the basic layout of data on a disk which is GPT formatted:

![Taken from Wikipedia.org](/docs/images/GUID_Partition_Table_Scheme.png)
            
([Source: Wikipedia.org - GPT article. This image is licensed under Creative Commons Attribution-ShareAlike v3.0](https://en.wikipedia.org/wiki/GUID_Partition_Table#/media/File:GUID_Partition_Table_Scheme.svg))
              
## Protective MBR
The protective MBR resides at the first LBA on the disk (LBA0). If the disk is GPT formatted, it will
indicate a single partition of type EEh (0xEE) which will cover the "entire" drive. In this situation,
"entire" actually means as much of the drive as MBR allows, since MBR uses 32-bit LBAs, larger disks
cannot be fully covered. In any case, the size of the MBR partition should be ignored.

If multiple partitions are specified, a lot of software will not treat a partition as GPT even if it
is marked with 0xEE.

For legacy BIOS support (i.e. non EFI/UEFI), the first sector also contains the first stage bootloader.
For GPT formatted disks, this first stage bootloader must be GPT aware and also not assume a 512-byte
sector size.

## Primary Header

The primary GPT header is the main copy of the GPT header. If it becomes corrupted, it can be compared
to (and/or restored using) the secondary GPT header (see Secondary Header). The GPT header contains
global information about the disk and data on the disk.

## Primary Partition Table
The primary partition table contains the main copy of the partition table. It can be found at the LBA
specified in the header (offset 72, size 8 bytes). However, the standard sets this value at 2 for the
primary partition table and header. The table can be treated as an array of partition entries
(typically 128). Each entry has a specific format. The actual size of each entry and the number of
entries can be found in the GPT header. Each entry is usually 128 bytes in size.

The following table specifies the format of a partition entry. The table shows the minimum size of a
partition entry:
            
| Offset | Length | Contents |
|:---------:|:-----------:|:-------------|
| 0 (0x00) | 16 | Partition type GUID | 
| 16 (0x10) | 16 | Unique partition GUID | 
| 32 (0x20) | 8 | First LBA (little endian) | 
| 40 (0x28) | 8 | Last LBA (inclusive, usually odd) | 
| 48 (0x30) | 8 | Attribute flags (e.g. bit 60 denotes read-only) | 
| 56 (0x38) | 72 | Partition name (36 UTF-16LE code units)
| | 128 bytes total | Original source: [Wikipedia : GPT article](https://en.wikipedia.org/wiki/GUID_Partition_Table#Partition_entries). |
| | | This table is licensed under Creative Commons Attribution-ShareAlike v3.0 License |

The following table specifies the partition attributes:

| Bit | Contents |  
|:-----:|:-------------|
| 0 | System partition (disk partitioning utilities must preserve the partition as is) |
| 1 | EFI firmware should ignore the content of the partition and not try to read from it | 
| 2 | Legacy BIOS bootable (equivalent to active flag (typically bit 7 set) at offset +0h in partition entries of the MBR partition table) | 
| 3-47 | Reserved for future use |
| 48-63 | Defined and used by the individual partition type |
| | Original source: [Wikipedia : GPT article](https://en.wikipedia.org/wiki/GUID_Partition_Table#Partition_entries)
| | This table is licensed under Creative Commons Attribution-ShareAlike v3.0 License |

The following is a reasonably comprehensive list of the numerous Partition Type Guids: [Wikipedia - Partition Type Guids](https://en.wikipedia.org/wiki/GUID_Partition_Table#Partition_type_GUIDs)

        
## Secondary Partition Table
The secondary partition table has the same format as the primary partition table. In normal operation,
the secondary partition should be identical to the primary partition table. In the event that the disk
becomes corrupted, the secondary partition can be used to restore the primary partition table. Various
methods, not discussed here, can be employed for reconciling corrupted primary and secondary partiton
tables.

The secondary partition table is always stored at the end of the disk just prior to the secondary
header. The starting LBA can be found by reading the Secondary Header and using the Current LBA value.
You can also verify this value by comparing it to the value of "Last usable LBA + 1" from the primary
header.

## Secondary Header
The secondary header has the same format as the primary header. Unlike the Secondary Partition Table,
it should not be an identical copy of the primary header. It should, however, verify all the data in
the primary header.

The secondary header is stored in the last LBA sectors on the disk. The starting LBA should be read
from Backup LBA in the primary header. However, in the event that the primary header is corrupted,
the value can be calculated by subtracting the size of the header from the size of the disk
(with all size units in whole/integer sectors not bytes).

---

# Software

## GPT Detection
Detecting GPT involves steps of reading and data verification. The GPT specification requires a
protective MBR and single partition covering the whole of the disk. So the first step of GPT
detection is to read the first sector of the disk and look for/read the MBR partition table.

Software should then verify the MBR is valid/uncorrupted, then check that there is one and only one
partition, covering as much of the disk as 32-bit LBAs allow. Finally, the single partition should
have a System ID (see MBR for details) of 0xEE. If all these conditions pass, then the disk may have
a valid GPT on it.

The next step is to read the second sector of the disk (LBA 1). This should be the primary GPT header.
It should start with the GPT Signature/ID. Software should verify the signature is correct.

At this point, software can safely assume a valid GPT partitioning scheme exists on the disk. However,
software should not assume the headers, partition tables or partitions themselves are not corrupted.

## GPT Global Data
The next step software should take is to read in the global GPT data from the primary GPT header. Lazy
software can assume it is uncorrupted and proceed as normal. Good software will verify the header
against the secondary header and provide the user with a choice of what to do next in the event that
the primary header is corrupted.

## CRC32 Values
At this stage, good software will check the CRC32 values of the partition tables match the expected
values. This should be the primary way of detecting a corrupted disk. If either partition table
is corrupted, the software must decide whether to ignore the error and attempt to use the disk,
auto-correct the error by some algorithm or, if possible, present the user with a choice. The safest
option in the event that the user can't be given a choice is to display and error message and abandon
using the disk. This ensures no further corruption can occur (or at least, no corruption due to
software. Clearly hardware corruption is still a possibility).

## Partition Entries
The last thing software should do is read the individual partition entries in the partition table
and fill in structures to represent these. At this point, the partition structures should be
passable to other I/O software. Notably, there is no reason why the same partition structure
cannot be used for representing GPT and MBR partitions for general I/O use.

---

# Example Code

## Overview
The FlingOS GPT implementation can be found at: [https://github.com/FlingOS/FlingOS/blob/master/Kernel/Libraries/Kernel.FOS_System.IO/Disk/GPT.cs](https://github.com/FlingOS/FlingOS/blob/master/Kernel/Libraries/Kernel.FOS_System.IO/Disk/GPT.cs)

---

# Further Reading
The following links were valid as of 19th March 2015.

- [Wikipedia.org - Guid Partition Table](http://en.wikipedia.org/wiki/GUID_Partition_Table)
- [Wikipedia.org - Apple Partition Map](http://en.wikipedia.org/wiki/Apple_Partition_Map)
- [OSDev.org - GPT](http://wiki.osdev.org/GPT)
- [MSDN - Using GPT Drives](https://msdn.microsoft.com/en-us/library/windows/hardware/dn653580(v=vs.85).aspx)
- [IBM.com - GPT](http://www.ibm.com/developerworks/linux/library/l-gpt/)

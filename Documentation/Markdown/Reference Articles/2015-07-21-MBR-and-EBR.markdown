---
layout: ref_article
title: MBR and EBR
date: 2015-07-21 11:38:00
categories: docs reference
---

# Introduction

## What is MBR?
MBR stands for Master Boot Record. MBR is a partitioning scheme used by every PC worldwide until the late 2000s / early 2010s. MBR allows you to split a hard disk into four partitions, each of which contains its own file system. It is known as the Master Boot Record for two reasons. One is because it allows space for a primary bootloader alongside the partition table. The second reason is because, for a bootable hard drive, at least one partition will have the "boot" flag set. This will be discussed in more detail later on.

## What is EBR?
EBR stands for Extended Boot Record. EBR is an addition to the MBR scheme which allows additional partitions to be created in the form of sub-partitions within one (and only one) of the MBR partitions. It is essentially an extension which adds sub-partitioning.

## How difficult is MBR/EBR to understand?
MBR and EBR are relatively easy to understand because their data formats are simple. An understanding of partitioning and disk data layout is useful prior knowledge. In fact, EBR uses the same header format as MBR so it is very simple to extend MBR support to add EBR.

## Importance of MBR
MBR is probably the most important partitioning scheme that you need to know about it. Not least because it is now an integral part of the newer GPT standard but also because every PC in the world supports it. It is fundamental to the boot process, the BIOS and most disks you use on a day to day basis (including USB sticks).

## Scope of this article
This article will cover the background of what MBR is and where it comes from. It will then move on to a practical understanding of MBR/EBR concepts and look at how to implement MBR and EBR driver software.

---

# History

## Why are partition tables necessary?
Partition tables are necessary because frequently you want more than one file system to be physically present on a single disk. You may want multiple file systems simply for security and protection or as it allows you to use different file system types.

Also, multiple file systems allows you to install multiple operating systems side by side on the same disk, so long as both understand the disk formatting (i.e. the partition table). This is the case even if the two OSes can't understand each other's file systems.

## What came before MBR?
There is little to be said for what came before MBR. Most of the storage mediums around at the time were small hard disks or prior to that floppy disks and cassettes. As such, anything that might have resembled a partitioning scheme was proprietary and/or not widely adopted. Prior to MBR machines used custom disk formats, of which there were many even within one single company's products. As such, many tools for chaining and converting formats were developed. I found the following section of the article on the CP/M system developed by Digital Research most informative: https://en.wikipedia.org/wiki/CP/M#Disk_formats Readers older than myself are warmly invited to send me anecdotes of their experiences "back in the day" (but please, limit them to stories about disk formats...)

## Is there anything newer than MBR?
GPT (Guid Partition Table) is the latest partioning scheme which is more complex but still retains a protective MBR which has to be supported. There are also alternative standards that originate from outside the PC market such as the Apple Partition Map (though after Apple Macs shifted to Intel processors from PowerPC, APM is no longer supported).

---

# Overview

## General partition tables
In general a partition table simply splits a disk into one or more logical, contiguous chunks of data. What lies within a partition needn't be understood by an OS for it to be able to identify the partition. Equally, a partition does not have to contain a conventional file system, though more often than not it does. Notably, if a partition does not contain a valid file system, it usually means the partition is unformatted or corrupted.

Note that an unformatted partition is not the same as an unused partition. Unformatted means the partition has a chunk of data assigned to it, but the data itself is not in a valid or
recognisable format. An unused partition means either the partition entry does not assign any data from the disk to that partition or the chunk of data being referred to has no partition entry associated with it. The latter case can occur if the total size of partitions on the disk doesn't cover the full disk or, where partition size boundaries cause cut off leaving unused areas of the disk.

## MBR structure
MBR has a very simple structure. The first sector of an MBR formatted disk will always have the following layout:

##### First sector (LBA 0) structure
| Offset      | Description           | Size |
|:-----------:|:----------------------|:----:|
| 0x000 (000) | Bootstrap code        |  446 |
| 0x1BE	(446)	| Partition entry #1	  |  16  |
| 0x1CE	(462)	| Partition entry #2	  |  16  |
| 0x1DE	(478)	| Partition entry #3	  |  16  |
| 0x1EE	(494)	| Partition entry #4	  |  16  |
| 0x1FE	(510)	| MBR signature - always 0x55 |  1   |
| 0x1FF	(511)	| MBR signature - always 0xAA |  1   |
|============================================|
|             | Total size:           |  512 |

The last two bytes of the first sector will always be 0xAA55 (where 0xAA is the last/high byte and 0x55 is the penultimate byte). These are the MBR signature bytes and should be checked to verify that the disk is MBR formatted. Note, however, that a GPT formatted disk will appear to be MBR formatted to an MBR driver due to its protective MBR.

Note also that the bootstrap code area was originally entirely code but subsequent versions of the MBR standard have added additional areas that use particular parts of the bootstrap code area. None of the additions which use the bootstrap code area are particularly standard nor compatible, so for complete reliability and backwards compatibility it is generally best to ignore the newer versions.

Each "Partition entry" has the following format:

##### Partition entry structure
| Offset | Size (bits) | Description |
|:------:|:-----------:|:------------|
| 0      | 8           | Bootable indicator flag: 0x00 = inactive, 0x80 = active
| 1      | 8           | CHS - Starting head
| 2	     | 6           | CHS - Starting sector (Bits 6-7 are the upper two bits of the  Starting cylinder)
| 3	     | 10          | CHS - Starting cylinder
| 4	     | 8	         | System ID - identifies file system type that is within the partition
| 5	     | 8	         | CHS - Ending head
| 6	     | 6           | CHS - Ending sector (Bits 6-7 are the upper two bits for the ending cylinder field)
| 7      | 10 	       | CHS - Ending cylinder
| 8      | 32          | LBA - Partition's starting sector (relative to MBR's sector num, relevant for EBR)
| 12     | 32	         | LBA - Partition's total sectors

CHS fields are redundant now since practically all disks support (the much better) LBA addressing mode. However, for disks less than 8GiB (max size that CHS can address) the CHS fields must be consistent with the LBA fields and visa-versa. For disks larger than 8GiB, the CHS fields must be set to their maximum values. CHS fields set to their maximum values should always be considered invalid by an MBR driver. CHS fields may not be set to 0.

## EBR structure
EBR has the same structure as MBR except that the table sector only contains two partition entries instead of four. EBRs reside inside of an MBR partition. Only one MBR partition may contain EBRs. If an MBR partition contains EBRs, the first sector of the MBR partition will always contain an EBR (which includes the same signature bytes as an MBR).

 There can be multiple EBRs in the MBR partition. The first EBR will always appear in the first sector of the MBR partition. The EBR will specify only one actual partition, in the first partition entry. If the second partition entry is zero, there are no more EBRs in the MBR partition. If the second partition entry is non-zero, it specifies a new partition which will also start with an EBR as the first sector. Thus EBR partitions can be chained to give a large number of partitions.

## Boot sector
The boot sector is the first sector on disk (which is also the sector which contains the MBR). The boot sector provides bootstrap code which the BIOS or other firmware can load as the start of the boot process. Typically this boot sector contains a primary bootloader which, due to its size limitation, is only capable of reading a simple FAT formatted partition. There is more detail on the booting process in other articles.

## Bootable partitions
Bootable partitions are partitions flagged as containing a valid secondary bootloader or operating system. Under official MBR, only one partition may be marked as bootable. The primary bootloader (mentioned above) usually looks for the bootable partition then proceeds to search it for a secondary bootloader or operating system. This process is described in more detail in other articles.

Bootable partitions are often referred to as active partitions and have the Active bit set in their partition entry (see table above).

## Partition file systems
Partitions on a disk usually contain a file system. File system driver software is given a helping hand identifying the type of file system inside of a partition by the System ID found in the partition entry. Equivalents of the System ID field also exist in other partition table standards. The System ID is a (theoretically unique though sometimes abused) identifier that matches a particular file system type. Some of the common System IDs and their respective file system types are listed below. A decently accurate, complete list can be found at [Wikipedia - Partition Type](https://en.wikipedia.org/wiki/Partition_type#List_of_partition_IDs)

##### Common File System Identifiers (System IDs)
|   ID   | File System Type                                                    |
|:------:|:--------------------------------------------------------------------|
| 0x00   | Empty / No Partition
| 0x01   | FAT12 (inside first 32MiB)
| 0x04   | FAT16 (size < 32MiB)
| 0x05   | EBR (CHS addressing, inside first 8GiB)
| 0x06   | FAT16(B) (size > 32MiB, inside first 8GiB) or FAT12/FAT16 (outside first 32MiB)
| 0x07   | IFS or HPFS or NTFS or exFAT
| 0x08   | FAT12/FAT16 (logical sectored) or various others
| 0x0B   | FAT32 (CHS addressing)
| 0x0C   | FAT32 (LBA addressing)
| 0x0E   | FAT16(B) (LBA addressing)
| 0x0F   | EBR (LBA addressing)
| 0x11   | FAT12/FAT16 (logical sectored) or Hidden FAT12
| 0x14   | FAT12/FAT16 (logical sectored) or Hidden FAT16 or Omega File System
| 0x15   | Hidden EBR (CHS addressing)
| 0x16   | Hidden FAT16(B)
| 0x17   | IFS or HPFS or NTFS or exFAT
| 0x1B   | Hidden FAT32
| 0x1C   | Hidden FAT32 (LBA addressing)
| 0x1E   | Hidden FAT16 (LBA addressing)
| 0x1F   | Hidden EBR (LBA addressing)
| 0x24   | FAT12/FAT16 (logical sectored)
| 0x27   | FAT32 or NTFS (rescue partition)
| 0x56   | FAT12/FAT16 (logical sectored)
| 0x83   | Any native Linux file system
| 0x88   | Linux plaintext partition table
| 0x8D   | Hidden FAT12
| 0x90   | Hidden FAT16
| 0x91   | Hidden EBR (CHS addressing)
| 0x92   | Hidden FAT16(B)
| 0x96   | ISO9660 partition (found largely on CDs/DVDs)
| 0x97   | Hidden FAT32
| 0x98   | Hidden FAT32 or bootable FAT
| 0xED   | Disk is GPT / MBR hybrid formatted
| 0xEE   | Disk is GPT formatted with protective MBR
| 0xEF   | EFI System Partition (FAT12/16/32 or other)
| 0xF7   | EFAT
| 0xFB   | VMWare VMS
| 0xFC   | VMWare swap / VMKCORE kernel dump

## BIOS Compatibility
MBR is and remains one of if not the widest used partitioning scheme in the world, most likely because of its compatibility with BIOS. BIOS (generally) only understands MBR formatted disks so if you want to have a PC that actually boots, the disk is going to have to be MBR formatted. (Though MBR and BIOS are rapidly being replaced by GPT and UEFI). The tight relationship between BIOS, MBR and Windows means that a lot of the file system IDs are used by FAT and a lot of the terminology for BIOS, MBR and FAT is shared between them with similar if not identical meanings.

## GPT Compatibility
GPT retains limited compatibility with MBR in that it uses a protective MBR. The protective MBR serves to make sure that no GPT formatted disk can accidentally be overwritten by old software which only understands or expects MBR formatted disks. However, do not expect to be able to read or write a GPT formatted disk using only the protective MBR - it is exceedingly unlikely to work as most GPT software does not create valid MBR partition entries. Again, this is for protection reasons.

---

# Software

## Overview
MBR software is relatively simple due to the relatively simple nature of the structure of MBR. By the time an OS comes to read a disk it (hopefully) won't need to worry about the bootcode area. All it really needs to worry about is checking for the signature, checking for GPT and, if it's plain MBR, reading the partition entries. Beyong that point, other OS driver software should handle the various possible file system types within a partition.

## Software outline
The basic outline of MBR software is as follows:

1. Read first sector of disk
2. Check for signature bytes:
    * If not 0xAA55, abort as the disk isn't MBR formatted.
3. Attempt to parse each partition entry in turn (offsets 0x1BE, 0x1CE, 0x1DE, 0x1EE)
    1. Read SystemID byte
    2. Check System ID:
        * If zero, partition entry is not valid.
        * If matching an EBR ID, this partition entry should not be used but should be scanned for EBR tables.
        * Otherwise, read start sector LBA and sector count and any other properties (e.g. active/inactive)

There really isn't much more to it than that. For EBR partitions, you need to read the first sector, read the first partition entry and use that as an actual partition. You should then read the second partition entry. If it is non-zero then it will point to another EBR partition. Good software structure will make the recursive nature of this entirely inevitable without any extra work.

## Software structure

The following classes are recommended:
* File System Manager - to begin scanning of a particular disk by reading in sector data and to follow-up on EBR partitions.
* MBR - to handle checking signature and methods for interpretting/checking partition entries.
* EBR - child class of MBR for handling EBR partitions. Shoudl reuse the interpretting/checking methods of parent MBR class.
* PartitionInfo - to represent partition information (e.g. start sector, sector count and active/inactive)

---

# Example Code

## Overview
TODO

## Download
TODO

---

# References

*[MBR]: Master Boot Record
*[EBR]: Extended Boot Record
*[GPT]: GUID Partition Table
*[APM]: Apple Partition Map
*[CHS]: Cylinder, Head, Sector address
*[LBA]: Logical Block Address
*[FAT]: File Allocation Table (12, 16 or 32 bit)
*[NTFS]: New Technology File System
*[IFS]: Installable File System
*[HPFS]: High Performance File System
*[ISO]: International Organisation for Standardisation
*[CD]: Compact Disc
*[DVD]: Digital Versatile Disc

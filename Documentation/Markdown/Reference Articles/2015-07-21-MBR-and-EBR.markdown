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
MBR is porbbably the most important partioning scheme that you need to know about it. Not least because it is now an integral part of the newer GPT standard but also because every PC in the world supports it. It is fundamental to the boot process, the BIOS and most disks you use on a day to day basis (including USB sticks).

## Scope of this article
This article will cover the background of what MBR is and where it comes from. It will then move on to a practical understanding of MBR/EBR concepts and look at how to implement MBR and EBR driver software.

---

# History

## Why are partition tables necessary?
Partition tables are necessary because frequently you want more than one file system to be physically present on a single disk. You may want multiple file systems simply for security and protection or as it allows you to use different file system types.

Also, multiple file systems allows you to install multiple operating systems side by side on the same disk, so long as both understand the disk formatting (i.e. the partition table). This is the case even if the two OSes can't understand eachother's file systems.

## What came before MBR?
There is little to be said for what came before MBR. Most of the storage mediums around at the time were small hard disks or prior to that floppy disks and cassettes. As such, anything that might have resembled a partioning scheme was proprietary and/or not widely adopted. Prior to MBR machines used custom disk formats, of which there were many even within one single company's products. As such, many tools for chaining and converting formats were developed. I found the following section of the article on the CP/M system developed by Digital Research most informative: https://en.wikipedia.org/wiki/CP/M#Disk_formats Readers older than myself are warmly invited to send me anecdotes of their experiences "back in the day" (but please, limit them to stories about disk formats...)

## Is there anything newer than MBR?
GPT (Guid Partition Table) is the latest partioning scheme which is more complex but still retains a protective MBR which has to be supported. There are also alternative standards that originate from outside the PC market such as the Apple Partition Map ()

---

# Overview

## General partition tables

## MBR structure

## Boot sector

## Bootable partitions

## Partition file systems

## BIOS Compatibility

## GPT Compatibility

---

# Software

## Overview

## Basic outline

## Technical details
**Enumerations, classes, other such details**

## Implementation details
**Methods, steps, etc.**

## Compatibility

---

# Example Code

## Overview

## Download

---

# References

*[MBR]: Master Boot Record
*[EBR]: Extended Boot Record
*[GPT]: Guid Partition Table

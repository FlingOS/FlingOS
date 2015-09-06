---
layout: reference-article
title: Disk Devices
date: 2015-09-04 00:39:00
categories: [ docs, reference ]
description: This article covers disk devices in general with brief overviews of common disk devices.
---

# Introduction

## Scope of this article
This article covers disk devices in general with brief overviews of common disk devices. The overviews provide sufficient information for you to understand the differences between the specific types of disk device and thus to decide what to learn more about. Links to further reading about a specific type of disk device are provided after the relevant overview.

This article also covers abstract disk devices (as FlingOS describes them) such as partitions. While it is not apparently obvious that a data storage system is a disk device, upon inspection it is clear why treating a partition as a disk device is not only useful, but also creates a clean code structure. This is discussed more further on.

## How this article is structured
The article is structured into a section of general description followed by sections for the specific types of disk device. Each device-type specific section consists of an overview followed by links to further reading. The majority of the further reading links point to more detailed articles on this site which not only describe and explain in more detail, but also provide code samples and practical tips. The article finishes with a few recommendations for your own code structure.

# What are disk devices?

## Generic definition
As a very broad and generic definition, a modern disk device is any device which stores data. This includes hard disk drives, flash memory (e.g. USB sticks) and perhaps we can even include modern cloud storage. Essentially anything which is accessed as though data is stored permanently and in some form of byte order. It is important to note that for a disk device we need raw byte (or block) level access to the storage medium not just file-level access. However, we could treat a raw binary file as a disk device.

## Historical naming
The name "disk device" comes from the fact that the original storage mediums (post magnetic-tape era) were magnetic disks and/or compact discs (CDs). This meant they got know by their commonality - the fact that they used discs.

The name "disk device" is now historical given the new forms of storage available (e.g. flash memory, solid-state storage devices). However, many storage devices still use discs (HDD, CDs, DVDs) so the name is still relevant.

## Modern developments in storage
Modern developments in storage have blurred what can be thought of as a disc device. For instance, SSDs don't store data anything like a disc does, but we still refer to it as a disc device because, from an OS perspective, it still has all the same properties of a normal hard disk drive (excluding complex things like spin-up time).

Other developments include multi-level partitioning schemes and cloud storage. Both of these abstract the "disk device" that the OS interacts with away from the physical device. For instance, cloud storage can be used to store one large binary file which the computer simply treats as though it were a large hard disc.

For the sake of simplicity (and because it is mostly unnecessary to complicate things), we simply refer to most modern storage technology as disk devices and, so far as possible, treat it as such. This allows easy abstraction into file systems which hide the everyday developer from the physical differences between mediums.

The main issue that affects users when it comes to different types of disk device is ejection / disconnection. For instance, cloud storage can become disconnected or, as another example, USB sticks need ejecting before being pulled out. This results in a level of roughness for the user as they must consciously consider which type of device they use for what.

Modern OSes such as Chrome OS have tried to make progress in this area by offering services such as a local cache that later uploads to cloud storage when the user has a stable connection. USB mass storage devices are also built with flush commands which, if utilised properly, can mitigate the risk of unejected disconnection.

## Common types of disk device
There are several common types of disk device which a hobby OS developer is likely to come across and need or want to support. These are:

1. Hard disks (ATA, IDE, SATA, RAID) - FlingOS, and consequently this documentation, currently only supports ATA.
2. Partitions (MBR-, EBR-, GPT-based) - These are an abstract form of disc device, all of which are supported by FlingOS and are discussed later.
3. USB Mass Storage Devices - For example USB Flash Memory Sticks, USB External Hard Drives. These are discussed in significantly greater detail in the USB reference articles. FlingOS currently supports only USB 2.0 MSDs that support SCSI Over USB commands.
 
# ATA devices

## Overview
ATA (AT Attachment) devices (or derivatives) are probably the second most common type of storage device now in existence (the first being Flash memory). ATA was the major standard (still supported today) by computers the world over for reading and writing to hard disk drives. Nowadays ATA has largely been replaced by SATA or RAID systems. However, most SATA devices still support ATA and most PCs still contain the ATA controller hardware required. (For the sake of simplicity, this article will ignore the term IDE. IDE is discussed more in the detailed article on ATA.)

ATA is the easiest way for a hobby OS developer to access the main hard disk drive (and CD drive if present) in a PC. Getting ATA working is often one of the first things to do so you can begin reading in more OS code and setting up processes.

## Further reading
ATA, ATAPio and IDE are discussed in more detail in the [main ATA article](/docs/reference/ATA) in the Disk Devices section.
 
# USB devices

## Overview
USB (Universal Serial Bus) devices are now probably the most common type of device that users consciously use and recognise. USB Mass Storage Devices (USB MSDs) have become the most common way of using portable storage to the point where booting from a USB stick is now the easiest way to test-boot a Hobby OS. As a result, supporting reading and writing USB MSDs is tremendously helpful. However, a USB stack (even just for one version and only MSD type devices) is a complex and difficult thing to write. FlingOS currently only supports USB 2.0 MSDs (excl. 1.0 or 1.1). It is recommended that a hobby developer get ATA (much easier!) and a file system driver working before setting up USB so that they can be sure USB is their issue when testing and not the file system.

## USB Structure
A basic USB stack consists of the Host Controller Driver (UHCI/OHCI = v1.0/1.1, EHCI = v2.0, xHCI=v3.0). On top of this is the USB driver and on top of that is the USB Device Driver (e.g. Mass Storage Device driver). On top of the device driver you might put a file system driver or application level software.
Developing and testing all this for FlingOS took a month alone and a weekend or two of very long hours. If you are going to attempt USB support you will need a good level of commitment. However, the rewards are well worth it.

## USB Mass Storage Devices
USB MSDs are anything from external hard drives to flash memory sticks. It is easiest to provide support for SCSI Over USB devices. SCSI Over USB will allow you to read, write and manage the majority of USB MSDs if implemented properly. However, you may not achieve support for the most advanced features of some USB MSDs which would require their own specialist driver. Often specialist drivers are implemented such that they sit on top of the generic device driver, making use of the generic code as far as possible.

## Further Reading
USB (inc. host controllers, USB and USB MSDs) are discussed in more detail in the USB section.

# Abstract devices

## Overview
Abstract disk devices are a concept used to describe non-physical (/virtual) disk devices. They are constructs which vary wildly in how they actually manage storage but, to the developer, look and behave exactly like a normal disk device.

## Partitions
Partitions are the most common abstract disk device since they are almost always found as the lowest level data structure on a storage device. Partitions can be thought of just like a storage device since they merely describe the location of a large block of data on another (abstract or otherwise) disk device. The partition as the same properties and functions as a disk device, namely read, write and information such as name. Overall, therefore, we can see how a partition is a form of abstract disk device.

Mast Boot Record (MBR), Extended Boot Record (EBR) and Guid Partition Table (GPT) are all ways of managing partitions. It is mentioned here only as a note that they are the only major ways of storing partition data in desktop computers.

Partitions in general and MBR/EBR and GPT are all discussed in their respective articles:

- [Partitions](/docs/reference/Partitions)
- [MBR and EBR](/docs/reference/MBR-and-EBR)
- [GPT](/docs/reference/GPT)

# Code structure advice
Disk devices form a core part of OS operation as they underpin file systems. However, in general, the file system used is independent of the disk device type. Furthermore, most of the time, the specific type of disk device is irrelevant. All that really matters is the read, write and get size functions. So, it is highly recommended that you structure your code so that you have a generic disk device class, from which all specific disk device types are inherit. This will allow you to pass any disk device into a file system to be used for storage, without the file system code having to worry about the specific type of disk device.

# See Also

## Other Resources

- [ATA](/docs/reference/ATA)
- [USB](/docs/reference/USB)
- [Partitions](/docs/reference/Partitions)
- [MBR and EBR](/docs/reference/MBR-and-EBR)
- [GPT](/docs/reference/GPT)

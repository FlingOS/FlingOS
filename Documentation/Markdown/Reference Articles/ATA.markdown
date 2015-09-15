---
layout: reference-article
title: ATA
date: 2015-07-20 11:40:00
categories: [ docs, reference ]
parent_name: Disk Devices
description: Describes ATA in general and gives detailed description of how to use Parallel ATA (PATA) drives.
---

# Introduction

## Scope of this article
This article covers all the aspects of the ATA specification to at least a general/broad level. It goes into detail about ATA as PATA/IDE but provides limited information for PATAPI, SATA and SATAPI drives.

This article includes a useful but relatively brief history of ATA and includes basic information about the hardware that ATA specifies. The article then includes a detailed description of how to implement a PATA driver along with complete sample code.

---

# History

## Foreword
ATA has a long and winding history which is reasonably well documented across the internet (see references and Google search). So for the purposes of this article I will provide only a brief summary of its history. Note, the information in this article is drawn largely from the references provided at the end of this article, so specific citations are not being provided.

## Originally called AT Attachment
ATA is the old name for what is now called "Parallel ATA" (PATA). ATA originally stood for AT Attachment and ATAPI stood for AT Attachment Packet Interface. These two standards are described in more detail throughout this article.

Notably, the "AT" in "AT Attachment" stood for "Advanced Technology", however, to avoid possible trademark infringement with IBM, current specifications simply use AT. (And developers would be well advised to follow suit...)

## AT Attachment, AT Attachment Packet Interface
ATA is a hardware interface standard for connecting storage devices to a PC (originally via the 16-bit ISA bus created by IBM for the IBM PC/AT). It also includes the basic Parallel ATA transfer protocol.

ATAPI is a software protocol which was layered on top of ATA to allow it to connect a wider range of devices than the original protocol allowed for. It is essentially a way to send SCSI commands over the existing ATA protocol.

## Started with Western Digital's IDE
The first version of what would later become ATA was developed by Western Digital in around 1983. They developed a standard called Integrated Drive Electronics (IDE). The first hardware for IDE arrived in 1986. Over time ATA and IDE have become used somewhat interchangeably, since the versions of IDE have essentially always been implementations of the versions of ATA (though often Western Digital released new IDE drives before the official release of a new ATA standard).

## Turned into ATA
Western Digital co-developed, along with Control Data Corporation and Compaq Computer, the ATA standard and in 1994 it was officially adopted as ANSI standard "X3.221-1994, AT Attachment Interface for Disk Drives". Since then there have been 7 major versions of the ATA standard (up to Dec 2014) with the last one introducing SATA (Serial ATA) in 2005. Parallel ATA drives and Serial ATA drives are not compatible but all IDE drives are compatible with at least one version of ATA. The current working version (8) will standardise so-called Hybrid drives (which utilise non-volatile caches for frequently accessed or high priority files).

## ATA now has many acronyms
Due to its long history and many versions, ATA now has many acronyms associated with it. The common ones have already been mentioned (ATA, ATAPI and IDE) but it is worth mentioning these few too:

* EIDE - Enhanced IDE (Western Digital, IDE version 2, ATA 2 and 3 compatible)
* Fast ATA, Fast IDE - All ATA-2 related versions developed by manufacturers.
* Ultra ATA - associated with ATA version 2 and 4 to 7 with assorted suffixes.

## ATA now actually Parallel ATA (PATA)
Since the introduction of ATA/ATAPI-7 it has become technically incorrect to refer to anything as an ATA or IDE drive. Devices are now either Parallel ATA devices (the old way) or Serial ATA devices (the new way). These are referred to as PATA and SATA, though frequently PATA is simply ATA and SATA is SATA.

The original ATA standard cable had 40 wires to create a connection which could transfer 16-bits in parallel. Later, this increased to 80 wires, with the additional 40 wires interleaved with the original 40 to act as ground wires. This was required to reduce capacitive coupling between data wires when running at higher bit rates. The connector remained the same size (40 pins) since the additional wires could be connected to the existing ground pin.

As with most hardware, PATA (and SATA) only communicate with one device at once. It is a common misconception that Parallel means you can communicate with multiple devices at once. This is not true and has never been true. A consequence of this is that many driver developers forget that you have to select which drive you wish to communicate with at any given moment. This is described in more detail later.

## Serial ATA (2003) (SATA)
While this article does not provide hardware or software description for SATA, it is worth mentioning since it is probably the most common type of drive (though many SATA drives are IDE compatible). SATA stands for Serial ATA and is a very different hardware and software standard.

For hobby OS developers, SATA is initially much harder to implement than PATA. It is worth getting a PATA driver working which you can use to load driver executables from a hard disc or CD drive which can the be executed to control SATA drives.

## ATA as Parallel ATA (PATA)
PATA is much simpler than SATA. There are also some other things worth discussing.

### PATA and IDE now broadly equivalent
As has been mentioned earlier, ATA, now known at Parallel ATA (PATA), is used now broadly equivalent to IDE due to the standards long, intertwined history with IDE devices. Equally, due to the historical naming, most information refers to PATA as ATA since PATA is the original, old hardware standard, as opposed to the latest ATA standard Serial ATA (SATA).

### At a hobbyist level, what you program will work with most stuff
Since ATA and IDE are broadly equivalent, and both are the de-facto old-school hard drive standard, if you as a hobbyist choose to write ATA drivers, you will find that your code work with almost all virtualisation technologies and a vast number of real hard drives.

### Limitations of PATA
PATA is simple, very simple. Much, much simpler than SATA. This is possibly because its old, from a time when software had to be simpler or because its a more mature technology (by nearly 20 years). However, the simplicity comes at a slight penalty (by modern standards). The penalty is primarily speed. PATA disks are significantly slower than SATA disks (how much slower depends on version etc. It is left to the reader to research the potential difference, especially given the ever increasing speed of SATA disks).

## ATA as Serial ATA (SATA)

### Improvements with SATA
The key improvement is the increase in data transfer rate (in both directions). PATA has a maximum data transfer rate of 133MBps where as SATA (latest revision 3.2 - SATA Express) can do up to 2,000MBps which is 15 times faster.

SATA also offers physical improvements such as allowing more disks to be connected to the motherboard and thinner cables making them easier to use. Interestingly, it is possible that the thinner cable allows increased air flow in laptops and high-performance PCs allowing better cooling of important components such as CPUs and GPUs.

### SATA much more complex than PATA
SATA drives offer significantly faster disk access times and much higher data transfer rates along with high burst access speeds and the ability to connect more devices to the bus than PATA allows. However, this comes at the cost of complexity. To use a SATA drive you must support the significantly more complex protocol along with the Advance Host Controller Interface hardware that Intel developed to allow SATA bus control.

### Not always backwards compatible
SATA is also often not backwards compatible. Older SATA devices would present themselves as PATA devices and then require the BIOS or OS to switch the bus to SATA mode. However, newer SATA devices are either in SATA mode or PATA mode and only a BIOS setting can be used to switch (if available). However, switching will often cause the pre-installed OS to stop working since it will expect a SATA drive. As a consequence, to run a hobby OS on a very new laptop or desktop, SATA support will be required, unless USB support or similar is added.

### Get PATA working first
Due to PATA's comparative simplicity, wide support in virtualisation and high stability, it is worth getting PATA working before moving on to SATA. This will allow you to develop partition and file system drivers that work and are stable, thus eliminating two big areas of potential issue when getting a SATA driver to work.

---

# Overview

## What does ATA do?
ATA is a hardware standard which sets out the physical cable, electronic signal and hardware requirements for connecting storage devices. For PATA, the cable is a 40 or 80 pin connector which can transfer 16 bits at a time. The later versions of PATA used an 80-pin connector to increase the maximum transfer speed. All the additional wires were shielding wires connected to the ground pin. Thus, the connectors (plugs/sockets) remained 40-pins wide, thereby ensuring backwards compatibility.

ATA also sets out standards for handling hardware faults, unrecoverable errors and one possible standard for connecting (P)ATA devices to the motherboard and processor. However, other possibilities do exist. Many PATA (and SATA) devices now appear on the PCI bus.

Lastly, ATA defines a basic protocol for communicating with disk devices. Essentially a basic software protocol. This is the protocol used by IDE drives such as hard disks. For CD Drives it is usually replaced by ATAPI.

## What does ATAPI do?
ATAPI is a software standard ("packet interface") which sets out new command and data transfer protocols that sit on top of (but essentially replace) the basic ATA protocol. This is still separate from the hardware standard though. The hardware standard sets out how to transfer individual bits and bytes. The software standard sets out what those bits and bytes actually mean and in what order they must be sent. The ATAPI protocol makes used of only one of the original ATA commands (Identify packet). It replaces all other commands by SCSI
commands.

In general, hard disks use plain PATA, CD Drives used PATAPI. PATAPI is itself essentially SCSI commands.

## How common is it?
ATA is extremely common. It has been included in PCs and laptop for over a decade. This means most PCs and laptops that are more than 3 years old will have PATA support. However, with the recent introduction of SATA, some laptops and PCs no longer include PATA connectors or support. The Sony Vaio Fit Multiflip 15A, for example, only has a SATA drive with no detectable backwards compatibility for PATA.

Note, many manufacturers or specifications list drives as IDE. As discussed previously, this is broadly equivalent to a PATA drive. If your PC or laptop says it has an IDE drive, then it has PATA support.

## Why is it important?
ATA is important as it has long been the standard for hard disk and CD drive control. So much so, that the only successful alternative connector for storage mediums (excluding Ethernet, for obvious reasons) has been USB.

For a hobby OS developer, ATA is important as it (meaning PATA) is the easiest route to accessing permanent storage. The only 4 other viable alternatives are:

* Network stack to access cloud files,
* Serial connection to a file stored on a connected or host computer,
* Firewire connection to a file stored on a connected or host computer,
* USB Mass Storage support (which is recommended (necessary?) anyway)

ATA (as PATA or SATA) is also important as the hard disk remains the primary boot device in most tablets, laptops and PCs. To install an operating system in a way which is user friendly essentially requires you to install it to the primary boot device.

Finally, for hobby OS developers, ATA support is useful as ATA is reliable and fairly easy to implement. This makes developing partition and file system drivers significantly easier as it eliminates a whole subsystem of potential error. Having tested, reliable file system drivers means writing USB and USB Mass Storage support is easier.

## ATA for hard drives vs. CD drives
The key difference is that hard drives use plain old PATA (or newer SATA). CD Drives and other types of drive use PATAPI (or newer SATAPI). ATA only allows for drives that are hard drives. The Packet Interface adds the additional protocol which allows commands for other drive types to be controlled.

FlingOS currently only supports PATA (hard disk) devices and not PATAPI (CD ROM) devices. However, many modern laptops and PCs do not come with CD drives and external drives are usually attached via USB. This means that CD drive support is fairly unnecessary for a hobby OS.

## How complex to program?
PATA support is relatively simply to program. It requires only a basic knowledge of IO ports, hard disk structure and the protocol. There is plenty of information on the first two but details of the protocol are hard to come by in a useful way. This article will cover basic hard disk structure (ignoring legacy CHS structure wherever possible) and cover the practical implementation of the protocol in detail.

## Basic ideas
ATA devices each come with a number of registers. These registers are accessed through IO ports. The IO port numbers are determined by which ATA bus the device is attached to. ATA supports a Primary bus and a Secondary bus, each of which can have at least a Master device and up to one Slave device. This allows a maximum of 4 devices to be attached. Up to 2 devices share the same set of ports. As a consequence, you can only access one of the two devices on a single bus at the same time.

There are a number of different things you can do with an ATA device. But to do anything you must first detect what devices are present and then select the one you want to communicate with. This utilizes the Device Select register (/port). Once you have selected a device you can send it commands through the Command register. Data is sent though the Data register and there are a few other registers for extra important information.

Accessing a memory location on a disk is done in blocks. The block size used depends on the disk but in general the minimum block size if 512 bytes. This means you can only read or write a minimum of 512 bytes at a time. Each block has an address. Not so long ago, disks were accessed based on their real-world physical structure. Addresses were supplied as three numbers to select which Cylinder, Head and Sector. However, with advances in modern hardware, disks now used Logical Block Addressing (LBA for short).

There are a fixed number of LBAs on a disk and the block size is fixed. So if the block size is 512, and you want the byte 1024, you would read LBA 2 from the disk. Nowadays, all disks are accessed via LBA not CHS addresses so in this article we will focus on LBA addresses.

## PIO mode : What does PIO stand for?
PIO stands for Programmed Input/Output. It is a common way of connecting an external device to the CPU. It allows data to be transferred by using input/output commands to addresses in the I/O address space. These are often referred to as IO ports.

PIO is in contrast to DMA (Direct Memory Access) where data is transferred by directly reading/writing memory addresses. PIO is significantly more costly than DMA since it requires the CPU to transfer every byte of data not via memory. PIO mode is still important because when a CPU is just beginning to boot it must be able to load from a boot device without knowledge of what memory is available.

## PIO mode : Which types of device?
ATA PIO mode is now the default mode at startup for all ATA compliant devices. It is what allows the CPU to use any ATA device as a boot device. However, many modern devices which are ATA compliant also support switching to more advanced modes such as DMA.

---

# Hardware

## Overview
ATA hardware works by having a controller chip on the motherboard which links the CPU IO ports or links memory to the pins of the IDE connector (the physical cable). Each attached device also has a controller chip that processes the signals on the cable and responds appropriately.

## Cables & Connectors
Old ATA cables are 40-wires with 40-pin connectors at either end. Modern ATA cables use the same size connector but have 80 wires in the cable. The additional wires are in between the existing 40 wires and are connected to the existing ground pin in the connector. The additional ground wires create a shield between the data wires allowing them to use a higher data rate (as the cross-wire inductive effects are significantly reduced).

## Backwards Compatibility : Versions of PATA / IDE
Hardware that is PATA or IDE is usually fully backwards compatible. The new 80-wire cables are fully compatible with devices designed to use the old 40-wire cable. However, new devices will not work with the old 40-wire cables if they use the higher data rates introduced in newer PATA devices.

## Backwards Compatibility : SATA to PATA
Some SATA devices support a PATA compatible mode but most new SATA devices do not. All SATA devices support the IDENTIFY command, as required by the specification. This allows your PATA driver to detect a SATA device but not retrieve any data from it.

## Virtual hardware / virtual disks / virtual machines
Most VMs will give you a choice of device type to add. The choices are generally listed as: IDE, SCSI and SATA. IDE means PATA, SCSI means PATAPI or SATAPI (probably given the version choice later) and SATA means, well, SATA.

---

# Software

## Introduction
This software section will cover how to implement a PATA-only driver but one which is capable of detecting and cleanly handling PATAPI, SATA and SATAPI devices.

From the information provided in earlier sections, the remaining technical and practical information shouldn't be difficult to understand. If it is, please provide feedback via the link at the bottom of the page so the article can be improved.

This implementation of a PATA driver will use Programmed I/O mode - the oldest way of accessing PATA devices and the default way at startup. Regardless of what technique you may wish to switch to (e.g. Direct Memory Access mode - DMA) you need to use PIO mode initially to detect and configure devices. However, PIO mode is slower than DMA mode (the only other mode available) since it uses CPU IO Ports. For a fast, more modern PATA driver, you should consider adding DMA mode to this driver.

Finally, please note this implementation uses disk polling instead of IRQs (interrupts) which makes it simpler but slower. It is not difficult to work out how the code could be improved to use IRQs and non-blocking method calls if that is what is desired. The IRQ numbers required are IRQ14 for the primary bus and IRQ15 for the secondary bus. IRQs are also required for DMA mode. The following section from OSDev may be useful: [OSDev - PIO Mode - IRQs](http://wiki.osdev.org/ATA_PIO_Mode#IRQs)

## Basic outline
The following will outline all the steps, and in what order they may or must occur, for a working PATA driver that can read and write hard disks:

1. Disable IRQs
2. Enumerate buses:
	1.  Discover a particular drive (based on which bus and position on bus)
	2. Check drive type (PATA/PATAPI/SATA/SATAPI)
	3. Initialise the drive
		1. Read version / model information
		2. Read block count (determines drive size)
3. For a particular drive, A and B can be done in either order:
	1. **A.** To read:
		1. Select sector (sets block number to read and size of read)
		2. Send the read command
		3. Read the data
	2. **B.** To write:
		1. (Read in partial blocks to fill data to write to a whole number of blocks)
		2. Select sector (sets block number to read and size of read)
		3. Send the write command
		4. Write the data
		5. (Clean drive caches to ensure data written to disk)

*Please note: The sample driver provided only supports LBA28 mode and not LBA48 mode. This limits the maximum disk size it is capable of accessing to 2^28 bits. LBA48 mode is, essentially, an extension and can be added if required. Some LBA48 information is provided in this article.*

## Main classes
The recommended classes are:

* *ATA* : Abstract base class for all ATA drive types. Contains properties common to all devices (i.e. bus, bus position (often called Controller ID))
* *ATAIOPorts* : Groups together IO Port objects needed to read/write the ATA specific IO ports.
  This allows an instance of ATAIOPorts to be passed to the driver so it needn't worry about which IO Ports to use. It just accesses them through this class. This class creates IO Port objects which point to the correct ports when it is initialised. The ports for primary and secondary bus are different.
* *ATAManager* : Handles overall ATA functionality such as bus enumeration and
initialising all drive types not just PATA.
* *PATABase* : Half of the main PATA driver. Contains code which is common to both PATA and PATAPI devices (.e.g drive select and send command code).
* *PATA* : The other half of the main PATA driver. Represents a PATA drive and has code for handling a PATA drive. Also contains the base code for detecting drives and handling non-PATA drives.
* *PATAPI* : A stub class for a PATAPI drive.
* *SATA* : A stub class for a SATA drive.
* *SATAPI* : A stub class for a SATAPI drive.

Stub classes allow you to keep track of unsupported drives so they can be displayed to the user, even if you aren't able to use them. This is helpful during debugging and setup.

## Enumerables
A number of enumerables are used in the implementation so their details are presented here. All of these enumerations can be found in the places specified in the FlingOS code.


##### ControllerID (inside ATA class)

| Name | Value | Description |
|:---|:---:|:---|
| Primary |  0 | Value is arbitrary |
| Secondary | 1 | Value is arbitrary |


##### BusPosition (inside ATA class)

| Name | Value | Description |
|:---|:---:|:---|
| Master | 0 | Value is arbitrary |
| Slave  | 1 | Value is arbitrary |

##### Status (Flags, inside PATA class)

| Name | Value |
|---:|:---:|
| None       | 0x00 |
| Busy       | 0x80 |
| ATA_SR_DRD | 0x40 |
| ATA_SR_DF  | 0x20 |
| ATA_SR_DSC | 0x10 |
| DRQ        | 0x08 |
| ATA_SR_COR | 0x04 |
| ATA_SR_IDX | 0x02 |
| Error      | 0x01 |

##### Error (Flags, inside PATA class)

| Name | Value |
|---:|:---:|
| ATA_ER_BBK   | 0x80 |
| ATA_ER_UNC   | 0x40 |
| ATA_ER_MC    | 0x20 |
| ATA_ER_IDNF  | 0x10 |
| ATA_ER_MCR   | 0x08 |
| ATA_ER_ABRT  | 0x04 |
| ATA_ER_TK0NF | 0x02 |
| ATA_ER_AMNF  | 0x01 |

##### DriveSelectValue (Flags, inside PATA class)

| Bit(s) | Name | Value(s) |
|:---:|:---|:---|
| 0-3 | Head Number for CHS. | |
| 4 | Slave Bit | 0: Select Master drive, 1: Select Slave drive |
| 5 | Obsolete (unused) | Always 1 |
| 6 | LBA mode selector | 0: CHS address mode, 1: LBA address mode |
| 7 | Obsolete (unused) | Always 1 |

| Name | Value |
|---:|:---:|
| Slave | 0x10 |
| LBA | 0x40 |
| Default | 0xA0 |

##### Cmd (Commands, inside PATA class)

| Name | Value | Notes |
|---:|:---:|:---|
| ReadPio        | 0x20 | |
| ReadPioExt     | 0x24 | |
| ReadDma        | 0xC8 | |
| ReadDmaExt     | 0x25 | |
| WritePio       | 0x30 | |
| WritePioExt    | 0x34 | |
| WriteDma       | 0xCA | |
| WriteDmaExt    | 0x35 | |
| CacheFlush     | 0xE7 | |
| CacheFlushExt  | 0xEA | |
| Packet         | 0xA0 | |
| IdentifyPacket | 0xA1 | PATAPI devices only. |
| Identify       | 0xEC | |
| Read           | 0xA8 | |
| Eject          | 0x1B | &nbsp; |

##### SpecLevel (Specification level = drive type and identifier, inside PATA class)

| Name | Value | Notes |
|:---|:---:|:---|
| Null   | 0x0000 | *This value is arbitrary and has no meaning in the ATA spec.* |
| PATA   | 0x0001 | *This value is arbitrary and has no meaning in the ATA spec.* |
| SATA   | 0xC33C | *This value is NOT arbitrary.* It is the SATA device identifier. |
| PATAPI | 0xEB14 | *This value is NOT arbitrary.* It is the PATAPI device identifier. |
| SATAPI | 0x9669 | *This value is NOT arbitrary.* It is the SATAPI device identifier. |

## IO Ports
There are four base address register ports (BARs for short). Reading the correct two provides the two base addresses for the IO ports used by either the primary or secondary bus. The IO ports used by ATA are calculated as fixed offsets from these base addresses. The offsets are the same for both the primary and secondary bus.

The following lists the port names and their respective addresses on the primary and secondary buses. Not all ports are necessary for a basic PATA driver.

| Port Name | Address (Primary) | Address (Secondary)
|:---:|:---:|:---:|
| BAR 0 | 0x01F0 | 0x0170 |
| BAR 1 | 0x03F4 | 0x0374 |

| Port Name | Relative address | Notes | Necessary? |
|:---|:---|:---:|:---:|
| Data            | BAR0 + 0 | | Yes |
| Error           | BAR0 + 1 | Read only | No
| Features        | BAR0 + 1 | Write only | No
| Sector Count    | BAR0 + 2 | | Yes
| LBA0            | BAR0 + 3 | | Yes
| LBA1            | BAR0 + 4 | | Yes
| Device Select   | BAR0 + 6 | | Yes
| LBA2            | BAR0 + 5 | | Yes
| Command         | BAR0 + 7 | Write only | Yes
| Status          | BAR0 + 7 | Read only | Yes
| Sector Count 1  | BAR0 + 8 | Used for LBA48 mode. | No
| LBA3            | BAR0 + 9 | Used for LBA48 mode. | No
| LBA4            | BAR0 + 10 | Used for LBA48 mode. | No
| LBA5            | BAR0 + 11 | Used for LBA48 mode. | No
| Control         | BAR1 + 2 | | Yes
| Device Address  | BAR1 + 3 | Unknown purpose. Please contact via link at the bottom of the page if you know more about this register. | No |

You will notice that some port addresses are used twice. They are always separated by read / write only conditions. This means there are actually two registers at the same address but only one can be written to and the other read from. Take care when using these ports that you don't accidentally try to write to something which is read only since it can overwrite other data without throwing an error.

## Disabling IRQs
Simply write 0x02 to the Control register (/port) to disable ALL ATA IRQs.

## Waiting 400ns
Due to PATA and IO port latency, various parts of the implementation call for a 400ns delay. The widely accepted way to do this is to read the Status register (one byte) four times which creates an approximately 400ns delay. The value returned by reading the port should be ignored as it may well be a transitioning value and so be inaccurate.

## Sending commands
To send a command to a drive (after setup such as selecting the drive) the following steps must be performed:

1. Write the command byte to Command port
2. Wait for the "busy" status bit to clear or "error" bit to set or an arbitrary timeout condition is met. This involves a loop which must occur at least once (this is best implemented using a do-while loop) and contains these steps:
  1. Wait for 400ns (see "Waiting for 400ns" above).
  2. Read "Status" register (one byte)
  3. Check for an error by checking "error" bit
		* If the error bit is set, you must handle the error as you see fit. Aborting the command will require your code to fail gracefully or crash the OS.
3. Return the Status (for use by the caller)

The Status register returns a value which corresponds to the Status enumerable. (See section above.)

## Initialisation / Bus Enumeration
Initialisation is bus enumeration. Whatever is managing all ATA devices should attempt to discover a drive on all the buses and positions. This means going through primary and secondary buses and master and slave positions on them one by one. Four simple calls really.

The driver should try to initialise a device by calling the PATA class init code (probably a constructor). This should perform the Discover Drive process. If a PATA drive is discovered, the Drive Init process should then be performed. If not, the drive type information (PATAPI/SATA/SATAPI) should be passed up to the ATA driver for it to handle.

If the Discover Drive process returns SpecLevel.Null, it indicates no drive is attached and bus enumeration should move to the next position on the bus or to the next bus.

A drive can be attached to the slave position on a bus without a master drive being attached. This means all positions on all buses should be enumerated regardless of the state of other positions.

## Drive Select

All operations which follow will require drive selection to be done at some point prior to the operation occurring. Drive selection is performed by writing to the Drive Select register. Drive Select tells the drives on the ATA bus which one is being sent commands or data. You should not try to issue a Drive Select while there is a pending or ongoing operation (such as read/write).

To perform Drive Select you should write the selection byte to the Drive Select register then wait 400ns. The selection byte is determined as follows.

* If the next command is not the Identify command, the LBA (Logical Block Address) bit should be set and the value used as follows:

```
DriveSelectValue.Default | DriveSelectValue.LBA | (busPosition == BusPosition.Slave ? DriveSelectValue.Slave : 0)) | LBAHigh4Bits
```

* Otherwise, the selection byte should be this:

```
DriveSelectValue.Default | (busPosition == BusPosition.Slave ? DriveSelectValue.Slave : 0)) | LBAHigh4Bits
```

Where LBAHigh4Bits is the value of the high four bits of the 28-bit sector number to be accessed. This should be set to 0 during Drive Discovery. I am uncertain how these bits are treated in LBA48 mode. If you know more, please provide feedback via the link at the bottom of the page.

## Drive Discovery
Drive Discovery should be a very simple process but it is made more complicated by the fact that some manufacturers failed to stick to the specification when developing PATAPI, SATA and SATAPI devices. This means the process is a bit more convoluted to ensure all non-PATA deices are detected properly.

The following steps should be followed for Drive Discovery. Drive Discovery should be immediately followed by Drive Init if a PATA device is found. There should be no commands sent over ATA between Drive Discovery and Drive Init if a PATA device is detected.

1. Select Drive (set `LBAHigh4Bits` to 0, LBA bit should NOT be set)
2. Write "0" to SectorCount, LBA0, LBA1 and LBA2 registers
3. Send the Identify command, store the Status value returned by Send Command
4. Check to see if the Error bit is set. If it is:
   The device is not a PATA drive. An error flag is the expected response from PATAPI, SATA and SATAPI devices.
   
   * Return the combined value from LBA1 and LBA2 (e.g. LBA2 << 8 &#124; LBA1). The combined value tells you the Specification Level of the device (see SpecLevel enumerable).
5. Check to see if Status is "0". If it is:
	
   * No drive is attached. Return SpecLevel.Null.
6. Check the Status value for other SpecLevel Id's:
   This is due to non-conformant devices which don't throw an error even if they are non-PATA devices.
   
   * Compare the combined value from LBA1 and LBA2 (e.g. LBA2 << 8 &#124; LBA1) to non-arbitrary
values in the SpecLevel enumerable. If one is found, that is the spec level of the device
so return it.
   * If the combined value from LBA1 & LAB2 is non-zero, return SpecLevel.Null.
   * Otherwise, continue.
6. Do-while loop:
  1. Wait 400ns
  2. Read Status register (one-byte)
  3. Condition: Loop while DRQ (Data Request bit) and Error bits are not set in Status. May also want to apply an arbitrary timeout to this.
7. Check if Error bit is set
8. Check if DRQ bit is not set
9. Return SpecLevel.PATA - This IS a PATA drive :D
10. (Immediately call Drive Init now that a PATA drive has been found).

## Drive Init / Drive Info
Drive Init involves reading drive information (such as firmware revision) and drive size. IMPORTANT: The information MUST be read from the drive before anything can continue. This is because the data is "in the pipeline" so to speak. Nothing else can come through until the information is cleared (i.e. read).

The information read includes the Serial Number string, Firmware revision string, Model Number string and disk size. The data is an array of 256 UInt16s. The read must be read as words (UInt16s) not single bytes. At an assembly level, this means calling "in word ax, dx" instead of "in byte al, dx".

The strings are one-byte ASCII characters at specific offsets in the data read. The Block Count (i.e. sector count) is "the value one greater than the total number of user-addressable sectors".

The following details the indices (in the UInt16 array) and sizes of the values (in terms of number of UInt16s):

| Name | Type | Index | Length | Mode |
|:---|:---:|:---:|:---:|
| Serial Number      | ASCII String | 10 | 20 | Either |
| Firmware Revision  | ASCII String | 23 | 8  | Either |
| Model Number       | ASCII String | 27 | 40 | Either |
| Block Count (size) | ULong        | 60 | 2  | LBA28  |
| Block Count (size) | ULong        | 100 | 4 | LBA48 - See below  |

In LBA48 mode, the Block Count LBA28 Mode above should be ignored if:

```
(UInt16Array[82] & 0x0400) != 0
```

Thus:

```
bool LBA48Capable = (buffer[83] & 0x400) != 0;
if (LBA48Capable)
{
  blockCount = (buffer[103] << 48 | buffer[102] << 32 | buffer[101] << 16 | buffer[100]) - 1;
  LBA48Mode = true;
}
else
{
  blockCount = (buffer[61] << 16 | buffer[60]) - 1;
}
```

## Select Sector

To be able to read/write a drive, the drive needs to know which sectors to read or write. Selecting a sector is the process of selecting a drive, telling it the sector to start at and the number of sectors to read/write.

To select a sector, the following steps must be performed:
1. Let the starting sector number be SectorNum and the number of sectors to read/write to be SectorCount
2. Perform the Select Drive process.

  *Note:* LBAHigh4Bits should be set to (byte)(SectorNum >> 24).

  *Note:* The LBA bit should be set.
3. Write the sector count (as a byte) to the SectorCount register.
4. Write the sector number (LBA) to the LBA registers. The LBA is written as single bytes to each LBA port. For LBA28 the following code sample demonstrates the necessary steps:

```
IO.LBA0.Write_Byte((byte)(aSectorNo & 0xFF));
IO.LBA1.Write_Byte((byte)((aSectorNo & 0xFF00) >> 8));
IO.LBA2.Write_Byte((byte)((aSectorNo & 0xFF0000) >> 16));
```

## Reading & Writing

To read or write one or more sectors, the following steps should be performed:

1. Perform the Select Sector process for the sectors to be read/written
2. Send the ReadPio or WritePio command as appropriate
3. Read / write the data from / to the Data port.

  *Note:* The data needs to be read or written in words (NOT bytes). At an assembly level, this means calling "in word ax, dx" instead of "in byte al, dx".

---

# Example Code

## Overview
TODO
The sample code provided here contains everything required to implement PATA support for hard disks. It also includes code for cleanly handling PATAPI, SATA and SATAPI devices.

The code is a direct copy of the FlingOS source code from 3rd February 2015. Most is in C# but for reference the IO Port x86-32bit assembler code is included.

## Download
TODO

---

# Common Problems

## Drive Selection (often on secondary bus)
A common problem with ATA is drive/device selection. This is a bit of a misleading trail, as I experienced. The problem is often not device selection. If you can select and use a PATA hard drive then your code is working. The issue is more likely to be the device is actually PATAPI (i.e. SCSI ATA) or SATA/SATAPI. For example, a CD Drive is a PATAPI device.

Unfortunately, common samples of "PATA only" code on the internet fail to handle PATAPI/SATA/SATAPI devices properly and so hang when one is encountered during bus enumeration. This gives the impression that an entire bus is failing to work, often the secondary bus.

To fix the issue, take a look at the sample code. The code correctly handles PATAPI/SATA/SATAPI devices without hanging.

## CD Drives
CD Drives are PATAPI devices and often cause "PATA only" code samples to hang during device enumeration. Failing that, PATA only code simply cannot retrieve data from PATAPI devices. Take a look at the sample code which demonstrates how to detect and avoid PATAPI/SATA/SATAPI devices.

## Disk Lock / Security
Some hard-disk and CD drives come with locking or security mechanisms which prevent them being read and/or written to until the lock is switched off or a security code is passed. FlingOS currently does not handle these devices. However, I have yet to come across one "in the wild" so if you've found one, good work! Google just became your best friend... ;)

## SATA / RAID Drives
Take a look at the sample code which demonstrates how to detect and avoid PATAPI/SATA/SATAPI devices. If you want PATAPI/SATA/SATAPI/RAID support, please check the other FlingOS reference articles or research further online.

---

# Further Reading
The following links were valid as of 27th January 2015.

* [Wikipedia - Parallel ATA](http://en.wikipedia.org/wiki/Parallel_ATA)
* [Wikipedia - ATA Packet Interface](http://en.wikipedia.org/wiki/ATA_Packet_Interface)
* [Wikipedia - Programmed Input/Output (PIO)](http://en.wikipedia.org/wiki/Programmed_input/output)
* [OSDev - ATA PIO Mode](http://wiki.osdev.org/ATA_PIO_Mode)
* [OSDev - ATA read/write sectors](http://wiki.osdev.org/ATA_read/write_sectors)
* [OSDev - LBA](http://wiki.osdev.org/LBA)
* [OSDev - PCI IDE Controller](http://wiki.osdev.org/PCI_IDE_Controller)
* [OSDev - SATA](http://wiki.osdev.org/SATA)
* [Computer Hope - Advantages of SATA over PATA](http://www.computerhope.com/issues/ch001325.htm)
* [Wikipedia - List of device bit rates](http://en.wikipedia.org/wiki/List_of_device_bit_rates)


*[ATA]: AT Attachment
*[PATA]: Parallel ATA
*[PATAPI]: Parallel ATA Packet Interface
*[SATA]: Serial ATA
*[SATAPI]: Serial ATA Packet Interface
*[IDE]: Integrated Drive Electronics
*[IRQ]: Interrupt Request
*[IRQs]: Interrupt Requests
*[SCSI]: Small Computer System Interface
*[IO]: Input/Output
*[PCI]: Peripheral Component Interconnect
*[PIO]: Programmed Input/Output
*[CD]: Compact Disc
*[CHS]: Cylinder-Head-Sector (address)
*[LBA]: Logical Block Address
*[BAR]: Base Address Register
*[RAID]: Redundant Array of Independent Disks

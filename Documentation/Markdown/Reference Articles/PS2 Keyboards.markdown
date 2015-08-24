---
layout: reference-article
title: PS2 Keyboards
date: 2015-08-24 00:15:00
categories: docs reference
---

# Introduction
The PS/2 keyboard is probably one of the most significant devices in computer history. For at least a decade and a half it was the most used interface for providing input to a computer and even provided a means for communicating between computers. PS/2 is a hardware and software standard for connecting a computer keyboard (or mouse) to a computer. It also includes control of things like the caps-lock status light. The PS/2 keyboard standard is so widely used and supported that even most newer USB hardware supports PS/2 emulation.
 
## Scope of this article
This article will cover the PS/2 computer keyboard as created by IBM in 1987 as part of their Personal System/2 computer along with its newer advances. This article will also include information about USB enulation of PS/2 and how to write PS/2 drivers. It will not, however, include general information about keyboards and Human Input Devices. For more broad topical information and background on Human Input Devices, please read the Human Input Devices article.

---

# History
The PS/2 standard was first introduced in 1987 by IBM as part of their Personal System/2 personal computer (which is where it gets its name from). The PS/2 connector replaced older RS232 serial-based connectors for mice and large 5-pin DIN connector for keyboards. While PS/2 itself was largely incremental, progressing from older serial keyboard standards released for the IBM Personal Computer, the maturity of the design is probably what lead to its success. 

The previous connectors based on large 5-pin DIN were incompatible with existing mouse connectors and were large, chunky connectors. The PS/2 connector improved on this by using a smaller mini-DIN connector and by using the same electrical standard for both mouse and keyboard devices. During the 90s the PS/2 standard's robustness, good design and backwards compatbility to older DIN-based keyboards lead to it taking over. It is probably true to say that every PC shipped between the late 90s and 2005 had both PS/2 mouse and keyboard ports on the back which were the primary input devices for the computer. 

In the past decade USB has slowly taken over from PS/2. However, USB still maintains backwards compatbility for PS/2 by supporting PS/2 emulation from USB keyboards and mice. PS/2 remains popular though for connecting keyboards in low-power situations and for fast-typists and users who require high-levels of key combination. This is because PS/2 supports n-key rollover natively (where as most USB software only supports 6-key rollover) and uses an interrupt driven mechanism (as opposed to USB's polling system) so consumes less power.

---

# Overview

## What is PS/2?
PS/2 is a hardware and software standard for connecting keyboards (and mice) to a computer. It specifies the connector type, electrical signals and the communication protcol (which also impacts the internal mechanism of the host device, usually a PC). 

## Why does PS/2 exist?
PS/2 has existed from 1987 and was designed to replace the large 5-pin DIN connectors. It took over as the de-facto keyboard (and mouse) connector for PCs during the 1990s and early 2000s. In the last decade it has largely been replaced bu USB but there are still plenty of places where PS/2 can be found. USB also support legacy emulation for PS/2. PS/2 is still the most reliable way to get keyboard input to a PC.

## How does PS/2 work?
PS/2 works through an interrupts mechanism. PS/2 is not enumerated on a PCI or USB bus so cannot be detected. As such, PS/2 device cannot be hot-plugged - the host system must be shut down when a PS/2 device is attached or detached. When the system starts, the system will attempt to initialise all PS/2 ports and will use any devices that respond at that time. From then on, PS/2 works by interrupting the host processor when the user provides input (for instance, when a key is pressed). The host system is required to process the data. For example, if a key press is not processed, no further interrupts will occur until the host system reads the data about which key was pressed. This is a common cause of complaint by OS developer who are just starting out as it can make it appear as though the PS/2 keyboard interrupt will only ever occur once. Please read the FAQ section for more detail on this issue.

---

# Hardware

## Overview
A PS/2 keyboard has reasonably simple hardware design (ignorning more generic arguments about general keyboard design and history). A PS/2 keyboard consists of keys which sit on top of springs or a rubber-dome layer. When you press a key, the spring or dome depresses and the key plunger pushes down on the switch membrance beneath. The switch membrane is a circuit split into two layers. The top and bottom layers are kept apart by a middle layer. When the plunger pushes down on the top layer, it pushes it onto the bottom layer. Small pads of metal on the two layers touch, forming a completed circuit. The completed circuit allows a small current to flow which the controller processor (which is built into the keyboard) detects and uses to determine which key was pressed. The circuit (and how the controller processor scans it) means that each key can be uniquely identified. It converts the unique identification of each key into a scancode which it sends to the host system. The host system is then interrupted and can read the scancode from the keyboard. The scancode provides information about which key was pressed and which of the modifier keys were also pressed at the same time. 

## Details : Internals
A PS/2 keyboard consists of several key parts described below.

| Component | Description |
|:----------|:------------|
| Keys | The keys are the obvious part of the hardware on a keyboard. They are what the user physically makes contact with. Due to the adverse conditions that keyboards are subjected to (fingers, nails and the oils people use are very abrasive both physically and chemically) the keys have to be made very durable. For years this was full of problems since manufacturing keys out of ABS was expensive and prone to issues with keys sticking as ABS could not be manufactured accurately enough. |
| Domes or springs | The rubber domes or proper springs are the thing underneath the key which causes it to spring rise back up and also massively affect the feel and sound of the key. The amount of pressure required to depress a key is an important metric, particularly for high-volume typists. The Cherry MX red, blue, black and brown standards have been defined for proper mechanical (spring-based) switches for a long time and define the basic machanisms along with their metrics. Key pressure required to type, whether the actuation point is noticeable or not and how loud the key is are all part of the Cherry MX standards. |
| Switch membrane | The switch membrane sits underneath the key plungers and consists of two halves of a single circuit. The two layers are essentially the two halves of lots of switches. Pressing a key causes the two layers to touch, thus closing one of the switches on the membrane. |
| Controller processor | The controller processor is the microprocessor (or microcontroller) built into the keyboard which handles both scanning the switch membrane (to determine which keys are pressed) and communicatin with the host system. It also handles switching on/off status lights (such as the caps-lock light) and, where present, controlling backlighting of the keyboard. |
| Status lights | These are small, low-current, low-power LEDs that usually indicate whether caps-lock is on, whether num-lock is on and whether scroll-lock is on. A lit light indicates the feature is switched on. |
| Backlighting | Some keyboards come with backlighting. For modern USB keyboards, this may be configurable by the host but will require special USB drivers. PS/2 does not include backlight support so one of two approaches is taken. Either a specialist PS/2 keyboard driver is installed in the host (to remove/override the defaukt one) or the controller processor in the keyboard has firmware which handles controlling the backlight. In this case, the host has no control over the keyboard backlight. The keyboard may include special function keys for enabling/disabling the backlight. These keys will likely not be sent to the host system when pressed. |

## Details : Externals
- PS/2 connector (& colour coding)
- Electrical standard
- Data path inside PC

## Alternatives
- USB keyboards
- Microphones
- Cameras
- Pens and touchscreens

## Compatibility
- Electrical compatbility of PS/2 to DIN
- Software incompatibility of PS/2 to DIN devices
- No hot plugging

---

# Software

## Overview
- Main purpose is to handle receiving and buffering keys for other drivers/apps to use
- Must maintain compatbility with other drivers in the system : Coomon HID data access API/ABI
- Pre-allocation or deferring of interrupts to be able to queue keys

- Difference between scancode, key(code) and (key)char

## Basic outline
- Driver initialises
- Registers IRQ handler
- IRQ happens and scancode is bufferred
- Other application/driver uses ABI/API to dequeue a scancode (or keycode or char)
- Driver keeps track of capitalisation and status lights

## Technical details
**Enumerations, classes, other such details**

## Implementation details
- Driver initialisation:
  - Preallocate key buffer (if that approach is being taken)
  - Register & enable IRQ
- Handle IRQ:
  - Defer if going to be using queuing which needs memory allocation 
  - Deferred or not: 
    - Read scancode (necessary to receive further interrupts)
    - Queue/buffer scancode
- Read scancode/key/char methods
- Caps-lock, num-lock, scroll-lock tracking
- Changing caps-lock, num-lock and scroll-lock lights

**Methods, steps, etc.**

## Alternatives
- Voice command
- Text recognition
- Handwriting recognition

## Compatibility
- Common scancode sets to most keyboard devices dating back to ???
- International keyboard software

---

# Example Code

## Overview

## Download

---

# FAQ & Common Problems
- Only receiving one scancode
- Infinite locks, exceptions or similar

---

# References
- https://en.wikipedia.org/wiki/PS/2_port
- https://en.wikipedia.org/wiki/Mini-DIN_connector
- https://en.wikipedia.org/wiki/DIN_connector
- https://en.wikipedia.org/wiki/Computer_keyboard#Control_processor
- http://www.computerhope.com/jargon/p/ps2.htm
- http://www.computer-engineering.org/ps2protocol/
- http://www.computer-engineering.org/ps2keyboard/
- https://www.pjrc.com/teensy/td_libs_PS2Keyboard.html

*[acronym]: details

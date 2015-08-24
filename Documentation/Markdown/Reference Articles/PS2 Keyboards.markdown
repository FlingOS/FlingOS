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

The switch membrane and controller processor use a system generically called a Keyboard matrix. These are used in both musical keyboards and number/QWERTY keyboards. The keyboard keys are divided up into rows and columns (though when looking at the actual this may not be apparent). Each switch is placed on the intersection of a column wire and a row wire. Scanning a row or column (as will be mentioned in a moment), means applying a test voltage to the wire for that row or column. When a row or column is not being tested, it is not connected (it is said to be floating. Note: It is not connected to 0V).

The controller processor scans each column at a time. For each column, if a key has been pressed, a small current will be flowing as at least one of the switches will be pressed. If a switch has been pressed for a given column, it must then work out how many and which. It does so by scanning down the rows (looking only at the one column in question) till it finds a pressed row. It then knows the row/column of the pressed key(s) and so which key was pressed. It translates this into a scancode which is then sent to the host.

This technique of scanning can lead to ghost keys (when a key is pressed but the controller does not register it) or phantom presses (when a key is not pressed but the controller registers it mistakenly). This can be solved by adding diodes between switches in the switch membrance but this generally is only done on more expensive keyboards. Without the ghost and phantom key protection hardware, a keyboard cannot properly support n-key rollover. However, most USB keyboards only use the 6-key rollver which is part of the USB standard. While PS/2 supports n-key rollover, the actual keyboard hardware may be a limiting factor.

## Details : Externals
The PS/2 connector is a 6-pin mini-DIN connector which uses serial, synchronous, bi-directional communication. What this means is that the basic protocol is serial i.e. every byte is sent in order and received in order. The protocol is synchronous meaning you cannot read and write simultaneously and bi-directional means bytes can be sent both to and from the keyboard. The six pins are assigned as follows (with the slot of the female connector at the bottom, numbers run across the rows, left to right, down the rows and have 1-based indexing).

| Pin | Name | Use |
|:-----:|:---------|:-------|
| 1  | +Data | Data pin for primary device (either mouse or keyboard) |
| 2  | Not connected  | Not connected except on some systems which allow the use of a splitter cable. In that case, this is the data pin for the secondary (opposite type to primary) device. |
| 3  | GND | Ground / 0V pin used as a reference |
| 4  | Vcc | +5V reference pin at 275mA |
| 5  | +CLK | Clock signal (for synchronising communication) |
| 6  | Not connected | Not connected except on some systems which allow the use of a splitter cable. In that case, this is the clock signal pin for the secondary (opposite type to primary) device. |

Convention dictates that keyboard ports are coloured purple and mouse ports are coloured green. While the two are identical in firmware and hardware, the actual port numbers used to communicate from the processor to the device will be different. This means most software won't, for example, be able to understand a keyboard device plugged into a mouse port. However, because both keyboard and mouse are (usually) handled by a single micro-controller (to save hardware costs) inside the host PC, if either device behaves erratically resulting in confusion at both ends, both keyboard and mouse may appear to be broken. This can lead to misdiagnosis, however, it is a rare situation to be in. An easy test is to shutdown, unplug one of the devices and then restart.

Most PS/2 connectors were not designed to be unplugged and plugged back in frequently. This means frequent use causes the pins to break which are next to impossible to replace. As a result, leaving a device plugged in is recommended where possible. 

## Alternatives
The main alternative to a PS/2 keyboard is a USB keyboard. The only real differences are the connection type (which also affects the amount of key-rollover) and the fact that USB keyboards can have additional features such as backlight control and special hotkeys. 

Hardware such as microphones, cameras, pens and touchscreens can all be used to provide different input interfaces for creating text. For example, microphones for voice command and pens for handwriting recognition. More description of these is given in the Software - Alternative section.

## Compatibility
PS/2 is electrically compatible with old, large, 5-pin DIN connectors and a passive wiring converter could be used to change from one connector type to other. However, the software protocols and pin ordering were completely different. Some PS/2 keyboards were designed to autodetect which type of port (PS/2 or DIN) they were connected to (based on the clock signal) and switch as appropriate. However, it is doubtful that any of these types of device still exist nowadays. A host with only DIN or only PS/2 ports cannot be made to support the other type without adding extra hardware (such as a special PCI card to do the job!).

Shutting down and then unplugging is necessary because PS/2 is not (generally) hot pluggable. While for most modern hardware this will not cause any physical problems (though it could easily damage old hardware), most software will not detect the change of device. This means that switching device types on a port or unplugging a mouse/keyboard and plugging in a different model will usually result in the device not working. Due to the PS/2 Reset command (a legacy protocol allowing a single keyboard combination to reset the processor), hot plugging devices can confuse the on-board micro-controller unintentionally resulting in a reset command. 

---

# Software

## Overview
The main function of PS/2 keyboard driver software is to handle receiving scancodes from a PS/2 keyboard and bufferring (/queueing) them until other drivers or applications request them. In order to be compatible with other HID driver software, the kernel or drivers must provide a common functions in the API or ABI for accessing HID data such as queued scancodes. PS/2 driver software should be designed so it can work alongside USB driver software as (for whatever reason the user may wish to plug in multiple keyboards.

There are two main techniques for handling data from a keyboard. The first (more common one) is to pre-allocate an array for use as a buffer. When a keyboard interrupt occurs, the driver reads in the scancode and then puts it in the next empty slot in the array. If the array is full then it is likely that the system has frozen or crashed but in any case, the scancode should still be read and then discarded. 

The second approach is to defer the interrupt and then use some form of list-like system to create an ever-expanding queue of scancodes. This has two disadvantages: it is slower and the user pressing and holding a key when the system is not frequently dequeueing scancodes can quickly eat up large amounts of memory.

A scancode is the unique number that identifies which key was pressed (along with whether the shift key was pressed. The caps lock key must be kept track of separately by the driver.) The scancode is not actually what most applications won't, however. Instead they want a code representing just the key, or the character for the key, that was pressed. This gives rise to two other representations of pressed keys: keycodes and characters. Keycodes just represent the key that was pressed without modifier keys and covers all keys, not just alphanumeric keys. Characters cover only the alpha-numeric and escaped-character keys (such as 'a', 'b' and return (being '\n')). A keyboard driver should provide methods for accessing all three forms of the repersentation. Internally, onlt scancodes should be stored and they should be dequeued from a single buffer. If the key code or character is required, a scancode should be dequeued and then converted and returned. This prevents calls to getting scancodes and other forms from becoming out of sync.

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
TODO

## Download
TODO

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
- https://en.wikipedia.org/wiki/Keyboard_matrix_circuit

*[acronym]: details

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

The previous connectors based on large 5-pin DIN were incompatible with existing mouse connectors and were large, chunky connectors. The PS/2 connector improved on this by using a smaller mini-DIN connector and by using the same electrical standard for both mouse and keyboard devices. During the 90s the PS/2 standard's robustness, good design and backwards compatibility to older DIN-based keyboards lead to it taking over. It is probably true to say that every PC shipped between the late 90s and 2005 had both PS/2 mouse and keyboard ports on the back which were the primary input devices for the computer. 

In the past decade USB has slowly taken over from PS/2. However, USB still maintains backwards compatibility for PS/2 by supporting PS/2 emulation from USB keyboards and mice. PS/2 remains popular though for connecting keyboards in low-power situations and for fast-typists and users who require high-levels of key combination. This is because PS/2 supports n-key rollover natively (where as most USB software only supports 6-key rollover) and uses an interrupt driven mechanism (as opposed to USB's polling system) so consumes less power.

---

# Overview

## What is PS/2?
PS/2 is a hardware and software standard for connecting keyboards (and mice) to a computer. It specifies the connector type, electrical signals and the communication protocol (which also impacts the internal mechanism of the host device, usually a PC). 

## Why does PS/2 exist?
PS/2 has existed from 1987 and was designed to replace the large 5-pin DIN connectors. It took over as the de-facto keyboard (and mouse) connector for PCs during the 1990s and early 2000s. In the last decade it has largely been replaced bu USB but there are still plenty of places where PS/2 can be found. USB also support legacy emulation for PS/2. PS/2 is still the most reliable way to get keyboard input to a PC.

## How does PS/2 work?
PS/2 works through an interrupts mechanism. PS/2 is not enumerated on a PCI or USB bus so cannot be detected. As such, PS/2 device cannot be hot-plugged - the host system must be shut down when a PS/2 device is attached or detached. When the system starts, the system will attempt to initialise all PS/2 ports and will use any devices that respond at that time. From then on, PS/2 works by interrupting the host processor when the user provides input (for instance, when a key is pressed). The host system is required to process the data. For example, if a key press is not processed, no further interrupts will occur until the host system reads the data about which key was pressed. This is a common cause of complaint by OS developer who are just starting out as it can make it appear as though the PS/2 keyboard interrupt will only ever occur once. Please read the FAQ section for more detail on this issue.

---

# Hardware

## Overview
A PS/2 keyboard has reasonably simple hardware design (ignoring more generic arguments about general keyboard design and history). A PS/2 keyboard consists of keys which sit on top of springs or a rubber-dome layer. When you press a key, the spring or dome depresses and the key plunger pushes down on the switch membrane beneath. The switch membrane is a circuit split into two layers. The top and bottom layers are kept apart by a middle layer. When the plunger pushes down on the top layer, it pushes it onto the bottom layer. Small pads of metal on the two layers touch, forming a completed circuit. The completed circuit allows a small current to flow which the controller processor (which is built into the keyboard) detects and uses to determine which key was pressed. The circuit (and how the controller processor scans it) means that each key can be uniquely identified. It converts the unique identification of each key into a scancode which it sends to the host system. The host system is then interrupted and can read the scancode from the keyboard. The scancode provides information about which key was pressed and which of the modifier keys were also pressed at the same time. 

## Details : Internals
A PS/2 keyboard consists of several key parts described below.

| Component | Description |
|:----------|:------------|
| Keys | The keys are the obvious part of the hardware on a keyboard. They are what the user physically makes contact with. Due to the adverse conditions that keyboards are subjected to (fingers, nails and the oils people use are very abrasive both physically and chemically) the keys have to be made very durable. For years this was full of problems since manufacturing keys out of ABS was expensive and prone to issues with keys sticking as ABS could not be manufactured accurately enough. |
| Domes or springs | The rubber domes or proper springs are the thing underneath the key which causes it to spring rise back up and also massively affect the feel and sound of the key. The amount of pressure required to depress a key is an important metric, particularly for high-volume typists. The Cherry MX red, blue, black and brown standards have been defined for proper mechanical (spring-based) switches for a long time and define the basic mechanisms along with their metrics. Key pressure required to type, whether the actuation point is noticeable or not and how loud the key is are all part of the Cherry MX standards. |
| Switch membrane | The switch membrane sits underneath the key plungers and consists of two halves of a single circuit. The two layers are essentially the two halves of lots of switches. Pressing a key causes the two layers to touch, thus closing one of the switches on the membrane. |
| Controller processor | The controller processor is the microprocessor (or microcontroller) built into the keyboard which handles both scanning the switch membrane (to determine which keys are pressed) and communicating with the host system. It also handles switching on/off status lights (such as the caps-lock light) and, where present, controlling backlighting of the keyboard. |
| Status lights | These are small, low-current, low-power LEDs that usually indicate whether caps-lock is on, whether num-lock is on and whether scroll-lock is on. A lit light indicates the feature is switched on. |
| Backlighting | Some keyboards come with backlighting. For modern USB keyboards, this may be configurable by the host but will require special USB drivers. PS/2 does not include backlight support so one of two approaches is taken. Either a specialist PS/2 keyboard driver is installed in the host (to remove/override the default one) or the controller processor in the keyboard has firmware which handles controlling the backlight. In this case, the host has no control over the keyboard backlight. The keyboard may include special function keys for enabling/disabling the backlight. These keys will likely not be sent to the host system when pressed. |

The switch membrane and controller processor use a system generically called a Keyboard matrix. These are used in both musical keyboards and number/QWERTY keyboards. The keyboard keys are divided up into rows and columns (though when looking at the actual this may not be apparent). Each switch is placed on the intersection of a column wire and a row wire. Scanning a row or column (as will be mentioned in a moment), means applying a test voltage to the wire for that row or column. When a row or column is not being tested, it is not connected (it is said to be floating. Note: It is not connected to 0V).

The controller processor scans each column at a time. For each column, if a key has been pressed, a small current will be flowing as at least one of the switches will be pressed. If a switch has been pressed for a given column, it must then work out how many and which. It does so by scanning down the rows (looking only at the one column in question) till it finds a pressed row. It then knows the row/column of the pressed key(s) and so which key was pressed. It translates this into a scancode which is then sent to the host.

This technique of scanning can lead to ghost keys (when a key is pressed but the controller does not register it) or phantom presses (when a key is not pressed but the controller registers it mistakenly). This can be solved by adding diodes between switches in the switch membrane but this generally is only done on more expensive keyboards. Without the ghost and phantom key protection hardware, a keyboard cannot properly support n-key rollover. However, most USB keyboards only use the 6-key rollover which is part of the USB standard. While PS/2 supports n-key rollover, the actual keyboard hardware may be a limiting factor.

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
The main alternative to a PS/2 keyboard is a USB keyboard. The only real differences are the connection type (which also affects the amount of key-rollover) and the fact that USB keyboards can have additional features such as backlight control and special hot-keys. 

Hardware such as microphones, cameras, pens and touch screens can all be used to provide different input interfaces for creating text. For example, microphones for voice command and pens for handwriting recognition. More description of these is given in the Software - Alternative section.

## Compatibility
PS/2 is electrically compatible with old, large, 5-pin DIN connectors and a passive wiring converter could be used to change from one connector type to other. However, the software protocols and pin ordering were completely different. Some PS/2 keyboards were designed to auto-detect which type of port (PS/2 or DIN) they were connected to (based on the clock signal) and switch as appropriate. However, it is doubtful that any of these types of device still exist nowadays. A host with only DIN or only PS/2 ports cannot be made to support the other type without adding extra hardware (such as a special PCI card to do the job!).

Shutting down and then unplugging is necessary because PS/2 is not (generally) hot pluggable. While for most modern hardware this will not cause any physical problems (though it could easily damage old hardware), most software will not detect the change of device. This means that switching device types on a port or unplugging a mouse/keyboard and plugging in a different model will usually result in the device not working. Due to the PS/2 Reset command (a legacy protocol allowing a single keyboard combination to reset the processor), hot plugging devices can confuse the on-board micro-controller unintentionally resulting in a reset command. 

---

# Software

## Overview
The main function of PS/2 keyboard driver software is to handle receiving scancodes from a PS/2 keyboard and buffering (/queueing) them until other drivers or applications request them. In order to be compatible with other HID driver software, the kernel or drivers must provide a common functions in the API or ABI for accessing HID data such as queued scancodes. PS/2 driver software should be designed so it can work alongside USB driver software as (for whatever reason the user may wish to plug in multiple keyboards.

There are two main techniques for handling data from a keyboard. The first (more common one) is to pre-allocate an array for use as a buffer. When a keyboard interrupt occurs, the driver reads in the scancode and then puts it in the next empty slot in the array. If the array is full then it is likely that the system has frozen or crashed but in any case, the scancode should still be read and then discarded. 

The second approach is to defer the interrupt and then use some form of list-like system to create an ever-expanding queue of scancodes. This has two disadvantages: it is slower and the user pressing and holding a key when the system is not frequently dequeuing scancodes can quickly eat up large amounts of memory.

A scancode is the unique number that identifies which key was pressed (along with whether the shift key was pressed. The caps lock key must be kept track of separately by the driver.) The scancode is not actually what most applications won't, however. Instead they want a code representing just the key, or the character for the key, that was pressed. This gives rise to two other representations of pressed keys: keycodes and characters. Keycodes just represent the key that was pressed without modifier keys and covers all keys, not just alphanumeric keys. Characters cover only the alpha-numeric and escaped-character keys (such as 'a', 'b' and return (being '\n')). A keyboard driver should provide methods for accessing all three forms of the representation. Internally, only scancodes should be stored and they should be dequeued from a single buffer. If the key code or character is required, a scancode should be dequeued and then converted and returned. This prevents calls to getting scancodes and other forms from becoming out of sync.

## Basic outline
PS/2 driver software follows several basic steps. They are as follows:

1. Driver initialises. This involves:
  - Pre-allocating any memory as necessary
  - Registering the IRQ handler
2. IRQ happens and scancode is buffered
3. Driver keeps track of capitalisation and status lights
4. Other application/driver uses ABI/API to dequeue a scancode (and receives either the scancode itself or keycode or char)

## Technical details
There is relatively little technical detail required for a PS/2 driver. The tables which follow provide technical reference information for both the PS/2 Controller (which is the chip in the host) and the PS/2 Keyboard.

##### Controller Ports

| Name | Number | Access | Description |
|:---------:|:------------:|:----------:|:----------------|
| Data | 0x60 | Read/Write | Used for reading/writing data from/to devices. Writing to this port clears bit 3 of the Status Register (indicating the byte is device data) and the value is written to the controller input buffer. |
| Status | 0x64 | Read | Reads status from the PS/2 controller (not PS/2 devices). | 
| Command | 0x64 | Write | Sends commands to the PS/2 controller (not PS/2 devices). Writing to this port sets bit 3 of the Status Register (indicating the byte is a command) and the value is written to the controller input buffer. |

##### Status Register

| Bit | Meaning |
|:----:|:--------------|
| 0 | Controller output buffer status (0=Empty, 1=Full). Driver should check this bit is set before attempting to read data from the Controller Data port. |
| 1 | Controller input buffer status (0=Empty, 1=Full). Driver should check this bit is clear before attempting to write data to the Controller Data or Controller Command ports. |
| 2 | System Flag. Cleared after power on reset and set by firmware if the system passes self tests (POST) |
| 3 | Controller input buffer contents type : Command or Data (0=Data written to input buffer is data for PS/2 device, 1=Data written to input buffer is a command for PS/2 controller) |
| 4 | Keyboard lock switch (1=Keyboard enable, 0=Disabled) |
| 5 | Transmit time-out error (0=No error, 1=Error). Data transmit not complete.  |
| 6 | Receive time-out error (0=No error, 1=Error). Data receive not complete. |
| 7 | Parity error (0=Odd parity received which is correct so No error, 1=Even parity received which is incorrect so an Error) |

##### Command Register

This register is write-only and is written with the controller command bytes listed below.

##### Controller Command Bytes

| Command | Meaning | Response Byte (in Data port) |
|:--------------:|:-------------|:---------------------:|
| 0x20 | Read Command byte ("byte 0") from controller. Value placed in Data port. | Controller Configuration Byte (see below) |
| 0x21 to 0x3F | Read "byte N" from controller internal RAM (where 'N' is the command byte & 0x1F) | Unknown (only the first byte of internal RAM has a standard purpose) |
| 0x60 | Write next byte as Command byte ("byte 0") of controller. This should be a Controller Configuration Byte, see below. | None |
| 0x61 to 0x7F | Write next byte as "byte N" of controller internal RAM (where 'N' is the command byte & 0x1F) | None |
| 0xA4 | Password Installed Test: returned data can be read from Data port. (0xFA=Password installed, 0xF1=No password) | None |
| 0xA5 | Load Security: bytes written to Data port will be read until a null (0) is found. | None |
| 0xA6 | Enable Security: works only if a password is already loaded. | None |
| 0xA7 | Disable second PS/2 port (only if 2 PS/2 ports supported) | None |
| 0xA8 | Enable second PS/2 port (only if 2 PS/2 ports supported) | None |
| 0xA9 | Test second PS/2 port (only if 2 PS/2 ports supported) | 0x00=Test passed, 0x01=Clock line stuck low, 0x02=Clock line stuck high, 0x03=Data line stuck low, 0x04=Data line stuck high |
| 0xAA | Test PS/2 Controller | 0x55=Test passed, 0xFC=Test failed |
| 0xAB | Test first PS/2 port | 0x00=Test passed, 0x01=Clock line stuck low, 0x02=Clock line stuck high, 0x03=Data line stuck low, 0x04=Data line stuck high |
| 0xAC | Diagnostic dump (read all bytes of internal RAM) | 16 bytes in scancode format containing current input port state, current output port state and controller program status word. |
| 0xAD | Disable first PS/2 port | None |
| 0xAE | Enable first PS/2 port | None |
| 0xC0 | Read controller input port. Output register should be empty before issuing this command. | Bits: 7=Keyboard not inhibited, 6: Set=Primary display is MDA, Clear=Primary display is CGA, 5=Manufacturing jumper not installed, 4=Enable 2nd 256K of motherboard RAM, 3 to 0=*Undefined* |
| 0xC1 | Copy bits 0 to 3 of input port to status bits 4 to 7 | None |
| 0xC2 | Copy bits 4 to 7 of input port to status bits 4 to 7 | None |
| 0xD0 | Read Controller Output Port | Controller Output Port (see below) |
| 0xD1 | Write next byte to Controller Output Port (see below). Check if output buffer is empty first. | None |
| 0xD2 | Write next byte written to Data port to first PS/2 port output buffer (only if 2 PS/2 ports supported). This makes it look like the byte written was received from the first PS/2 port. | None |
| 0xD3 | Write next byte written to Data port to second PS/2 port output buffer (only if 2 PS/2 ports supported). This makes it look like the byte written was received from the second PS/2 port. | None |
| 0xD4 | Write next byte written to Data port to second PS/2 port input buffer (only if 2 PS/2 ports supported). This sends next byte to the second PS/2 port. | None |
| 0xF0 to 0xFF | Pulse output line low for 6 ms. Bits 0 to 3 are used as a mask (0=Pulse line, 1=Don't pulse line) and correspond to 4 different output lines. Note: Bit 0 corresponds to the CPU "reset" line. The other output lines don't have a standard/defined purpose. | None |

##### Controller Configuration (Byte)

| Bit | Meaning |
|:----:|:--------------|
| 0 | First PS/2 port output register full interrupt (1=Enabled, 0=Disabled) |
| 1 | Second PS/2 port output register full interrupt (1=Enabled, 0=Disabled, only if 2 PS/2 ports supported) |
| 2 | System flag (1=System passed POST, 0=Your OS shouldn't be running) |
| 3 | 1=Override keyboard inhibit, 0=Allow inhibit. Probably should always be zero. |
| 4 | First PS/2 port clock enable (1=Disabled, 0=Enabled) |
| 5 | Second PS/2 port clock enable (1=Disabled, 0=Enabled, only if 2 PS/2 ports supported) |
| 6 | First PS/2 port scancode translation (1=Enabled, tranlsate to PC/XT, 0=Disabled, standard AT) |
| 7 | Reserved. Must be zero. |

##### Controller Output Port

| Bit | Meaning |
|:-----:|:-------------|
| 0 | System reset line. WARNING: Always set to 1. Do not use this to reset the system. Instead, pulse the reset line (e.g. using command 0xFE). Setting this bit to 0 can lock the computer up ("reset forever") if the PS/2 controller isn't reset. |
| 1 | A20 gate output |
| 2 | Second PS/2 port clock (output, only if 2 PS/2 ports supported) |
| 3 | Second PS/2 port data (output, only if 2 PS/2 ports supported) |
| 4 | Output buffer full with byte from first PS/2 port (connected to IRQ1) |
| 5 | Output buffer full with byte from second PS/2 port (connected to IRQ12, only if 2 PS/2 ports supported) |
| 6 | First PS/2 port clock output |
| 7 | First PS/2 port data output |

The A20 gate is a very significant and important bit. For anyone writing their own bootloader, setting the A20 gate bit will be necessary to be able to access memory above 1MiB. This is a piece of legacy nonsense in the x86 architecture dating back to the 8086 processor. 

The 8086 processor only had 20 address line, so any addresses above 1MiB wrapped back round to zero. Short-sited programmers decided to use this as a "feature" and so when newer processors were developed, IBM had to include a method to emulate the old address behaviour. Their solution was to include an override that would hold the 21st address line (A20 - zero-based numbering) at zero. 

For bizarre reasons, the decision was made to route the A20 line through the PS/2 controller (because the PS/2 controller had a spare pin). This has lead to the odd situation where the PS/2 controller configures CPU/memory bus functionality, despite the two being totally unrelated. 

More detail on the A20 gate and methods for enabling/disabling it can be found here: [OSDev.org - A20 Line](http://wiki.osdev.org/A20_Line).

##### Keyboard Commands

Values for ACK, Resend and other special bytes which are referred to in this table are listed in the table below.

| Command | Data Byte/s | Meaning | Response |
|:--------------:|:-----------------:|:-------------|:--------------|
| 0xED | Status lights/LEDs. Bits: 0=Scroll-lock, 1=Num-Lock, 2=Caps-lock. Other bits may be used in international keyboards for other purposes (e.g. a Japanese keyboard might use bit 4 for a "Kana mode" LED). | Set status light on/off. | ACK or Resend |
| 0xEE | None | Echo (for diagnostic purposes, and useful for device removal detection) | Echo (0xEE) or Resend |
| 0xF0 | Sub-commands: 0=Get current scan code set, 1=Set scan code set 1, 2=Set scan code set 2, 3=Set scan code set 3. | Get/set current scan code. | ACK or Resend if scan code is being set; ACK then the scan code set number, Resend if you're getting the scancode. |
| 0xF2 | None | Identify keyboard | ACK followed by zero or more ID bytes (used when detecting device types). |
| 0xF3 | Typematic byte:
Bit/s | Meaning
0 to 4 | Repeat rate (00000b = 30 Hz, ..., 11111b = 2 Hz) |
| 5 to 6 | Delay before keys repeat (00b = 250 ms, 01b = 500 ms, 10b = 750 ms, 11b = 1000 ms)
7 | Must be zero
Set typematic rate and delay | 0xFA (ACK) or 0xFE (Resend) |
| 0xF4 | None | Enable scanning (keyboard will send scan codes) | 0xFA (ACK) or 0xFE (Resend) |
0xF5	None	| Disable scanning (keyboard won't send scan codes)
Note: May also restore default parameters
0xFA (ACK) or 0xFE (Resend)
0xF6 | None | Set default parameters | 0xFA (ACK) or 0xFE (Resend) |
| 0xF7 | None | Set all keys to typematic/autorepeat only (scancode set 3 only) | 0xFA (ACK) or 0xFE (Resend) |
| 0xF8 | None | Set all keys to make/release (scancode set 3 only) | 0xFA (ACK) or 0xFE (Resend) |
| 0xF9 | None | Set all keys to make only (scancode set 3 only) | 0xFA (ACK) or 0xFE (Resend) |
| 0xFA | None | Set all keys to typematic/autorepeat/make/release (scancode set 3 only) | 0xFA (ACK) or 0xFE (Resend) |
| 0xFB | Scancode for key | Set specific key to typematic/autorepeat only (scancode set 3 only) | 0xFA (ACK) or 0xFE (Resend) |
| 0xFC | Scancode for key | Set specific key to make/release (scancode set 3 only) | 0xFA (ACK) or 0xFE (Resend) |
| 0xFD | Scancode for key | Set specific key to make only (scancode set 3 only) | 0xFA (ACK) or 0xFE (Resend) |
| 0xFE | None | Resend last byte | Previously sent byte or 0xFE (Resend) |
| 0xFF | None | Reset and start self-test | 0xAA (self-test passed), 0xFC or 0xFD (self test failed), or 0xFE (Resend) |


##### Keyboard Special Bytes

The keyboard sends bytes to the host system. Most of the bytes are scancodes but there are a few special bytes. These special bytes are detailed below.

| Response | Meaning |
|:-------------:|:-------------|
| 0x00 | Key detection error or internal buffer overrun |
| 0xAA | Self test passed (sent after reset command or keyboard power up) |
| 0xEE | Response to echo keyboard command |
| 0xFA | Keyboard command acknowledged (ACK) |
| 0xFC, 0xFD | Self test failed (sent after reset command or keyboard power up) |
| 0xFE | Resend keyboard command (Indicates the controller should repeat the last command it sent.) |
| 0xFF | Key detection error or internal buffer overrun |

##### Scancode Sets

## Implementation details

### Basic keyboard driver
- Driver initialisation:
  - Preallocate key buffer (if that approach is being taken)
  - Register & enable IRQ
- Handle IRQ:
  - See below
- Read scancode/key/char methods
  - See below
- Caps-lock, num-lock, scroll-lock tracking
  - See below
- Changing caps-lock, num-lock and scroll-lock lights
  - See below

### Driver using polling
- Driver initialisation
  - See below
- Polling &amp; timeout
- Read scancode/key/char methods
  - See below
- Caps-lock, num-lock, scroll-lock tracking
  - See below
- Changing caps-lock, num-lock and scroll-lock lights
  - See below

### Driver using interrupts
- Driver initialisation:
  - [Check for USB]
  - Reset &amp; disable
  - Flush buffers
  - Perform PS/2 controller self-test
  - Determine number of channels
  - Perform channel (/interface) tests
  - Preallocate key buffer (if that approach is being taken)
  - Register & enable IRQ
  - Enable devices
  - Reset devices
- Device type detection:
  - Disable scanning
  - Use identify command
- Handle IRQ:
  - Defer if going to be using queuing which needs memory allocation 
  - Deferred or not: 
    - Read scancode (necessary to receive further interrupts)
    - Queue/buffer scancode
- Read scancode/key/char methods
- Caps-lock, num-lock, scroll-lock tracking
- Changing caps-lock, num-lock and scroll-lock lights
- USB Legacy Support

Reading the data port (after an IRQ has occurred) provides the single-byte scancode of the key that was pressed (or released).

### CPU Reset

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

## I only ever receive one key press / scancode?

## My IRQ handler hangs or enters an infinite loop or locking condition?


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
- http://pcbheaven.com/wikipages/How_Key_Matrices_Works/
- https://en.wikipedia.org/wiki/Keyboard_matrix_circuit
- http://wiki.osdev.org/%228042%22_PS/2_Controller
- http://wiki.osdev.org/PS/2_Keyboard
- http://stanislavs.org/helppc/8042.html

*[acronym]: details

---
layout: reference-article
title: PS2 Keyboards
date: 2015-08-24 00:15:00
categories: docs reference
---

# Introduction
The PS/2 keyboard is probably one of the most significant devices in computer history. For at least a decade and a half it was the most used interface for providing input to a computer and even provided a means for communicating between computers. PS/2 is a hardware and software standard for connecting a computer keyboard (or mouse) to a computer. It also includes control of things like the caps-lock status light. The PS/2 keyboard standard is so widely used and supported that even most newer USB hardware supports PS/2 emulation.
 
## Scope of this article
This article will cover the PS/2 computer keyboard as created by IBM in 1987 as part of their Personal System/2 computer along with its newer advances. This article will also include information about USB emulation of PS/2 and how to write PS/2 drivers. It will not, however, include general information about keyboards and Human Input Devices. For more broad topical information and background on Human Input Devices, please read the Human Input Devices article.

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
PS/2 has existed from 1987 and was designed to replace the large 5-pin DIN connectors. It took over as the de-facto keyboard (and mouse) connector for PCs during the 1990s and early 2000s. In the last decade it has largely been replaced by USB but there are still plenty of places where PS/2 can be found. USB also support legacy emulation for PS/2. PS/2 is still the most reliable way to get keyboard input to a PC.

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
| 0x20 | Read Configuration byte ("byte 0") from controller. Value placed in Data port. | Controller Configuration Byte (see below) |
| 0x21 to 0x3F | Read "byte N" from controller internal RAM (where 'N' is the command byte & 0x1F) | Unknown (only the first byte of internal RAM has a standard purpose) |
| 0x60 | Write next byte as Configuration byte ("byte 0") of controller. This should be a Controller Configuration Byte, see below. | None |
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
| 6 | First PS/2 port scancode translation (1=Enabled, translate to PC/XT, 0=Disabled, standard AT) |
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
| 0xF3 | Typematic byte: Bits: 0 to 4=Repeat rate (00000b = 30 Hz, ..., 11111b = 2 Hz), 5 to 6=Delay before keys repeat (00b = 250 ms, 01b = 500 ms, 10b = 750 ms, 11b = 1000 ms), 7=Must be zero | Set typematic rate and delay | ACK or Resend |
| 0xF4 | None | Enable scanning meaning the keyboard will send scancodes when a key is pressed. | ACK or Resend |
| 0xF5 | None | Disable scanning meaning the keyboard won't send scancodes when a key is pressed. Note that this may also restore default parameters. | ACK or Resend |
| 0xF6 | None | Set default parameters | ACK or Resend |
| 0xF7 | None | Set all keys to typematic/autorepeat only (scancode set 3 only) | ACK or Resend |
| 0xF8 | None | Set all keys to make/release (scancode set 3 only) | ACK or Resend |
| 0xF9 | None | Set all keys to make only (scancode set 3 only) | ACK or Resend |
| 0xFA | None | Set all keys to typematic/autorepeat/make/release (scancode set 3 only) | ACK or Resend |
| 0xFB | Scancode for key | Set specific key to typematic/autorepeat only (scancode set 3 only) | ACK or Resend |
| 0xFC | Scancode for key | Set specific key to make/release (scancode set 3 only) | ACK or Resend |
| 0xFD | Scancode for key | Set specific key to make only (scancode set 3 only) | ACK or Resend |
| 0xFE | None | Resend last byte | Previously sent byte or Resend |
| 0xFF | None | Reset and start self-test | 0xAA followed by 0xFA (self-test passed) or 0xFC/0xFD (self test failed) or Resend |

##### Keyboard Special Bytes

PS/2 devices send bytes to the host system. Most of the bytes are scancodes but there are a few special bytes. These special bytes are detailed below.

| Response | Meaning |
|:-------------:|:-------------|
| 0x00 | Key detection error or internal buffer overrun |
| 0xAA | Self test passed (sent after reset command or device power up) |
| 0xEE | Response to echo command |
| 0xFA | Command acknowledged (ACK) |
| 0xFC, 0xFD | Self test failed (sent after reset command or device power up) |
| 0xFE | Resend command (Indicates the controller should repeat the last command it sent.) |
| 0xFF | Key detection error or internal buffer overrun |

### Scancode Sets

Notes: 

* *There is no scan code for "pause key released" (it behaves as if it is released as soon as it's pressed)*
* *For most character codes, the basic form means the key is pressed. If the high bit is set it indicates the key was released.*

##### Scancode Set 1

| Scancode | Key | Scancode | Key | Scancode | Key | Scancode | Key |
|:-------------:|:-------:|:-------------:|:-------:|:-------------:|:-------:|:-------------:|:------:|
| 0x01 | Esc pressed | 0x02 | 1 pressed | 0x03 | 2 pressed | 0x04 | 3 pressed |
| 0x05 | 4 pressed  | 0x06 | 5 pressed | 0x07 | 6 pressed | 0x08 | 7 pressed | 
| 0x09 | 8 pressed  | 0x0A | 9 pressed | 0x0B | 0 pressed | 0x0C | - pressed | 
| 0x0D | = pressed  | 0x0E | Backspace pressed | 0x0F | Tab pressed | 0x10 | Q pressed |
| 0x11 | W pressed  | 0x12 | E pressed | 0x13 | R pressed | 0x14 | T pressed | 
| 0x15 | Y pressed  | 0x16 | U pressed | 0x17 | I pressed | 0x18 | O pressed | 
| 0x19 | P pressed |  0x1A | \[ pressed | 0x1B | \] pressed | 0x1C | Enter pressed 
| 0x1D | Left control pressed | 0x1E | A pressed | 0x1F | S pressed | 0x20 | D pressed |
| 0x21 | F pressed |  0x22 | G pressed | 0x23 | H pressed | 0x24 | J pressed | 
| 0x25 |  K pressed |  0x26 | L pressed | 0x27 |  ; pressed | 0x28 | ' pressed | 
| 0x29 |  \` pressed | 0x2A | Left shift pressed | 0x2B | \ pressed | 0x2C | Z pressed | 
| 0x2D | X pressed | 0x2E | C pressed | 0x2F | V pressed | 0x30 | B pressed |
| 0x31 | N pressed | 0x32 | M pressed | 0x33 | , pressed | 0x34 | . pressed |
| 0x35 | / pressed | 0x36 | Right shift pressed | 0x37 | Keypad * pressed | 0x38 | left alt pressed |
| 0x39 | Space pressed | 0x3A | Caps-lock pressed | 0x3B | F1 pressed | 0x3C | F2 pressed |
| 0x3D | F3 pressed | 0x3E | F4 pressed | 0x3F | F5 pressed | 0x40 | F6 pressed |
| 0x41 | F7 pressed | 0x42 | F8 pressed | 0x43 | F9 pressed | 0x44 | F10 pressed |
| 0x45 | Num-lock pressed | 0x46 | Scroll-lock pressed | 0x47 | Keypad 7 pressed | 0x48 | Keypad 8 pressed | 
| 0x49 | Keypad 9 pressed | 0x4A | Keypad - pressed | 0x4B | Keypad 4 pressed | 0x4C | Keypad 5 pressed | 
| 0x4D | Keypad 6 pressed | 0x4E | Keypad + pressed | 0x4F | Keypad 1 pressed | 0x50 | Keypad 2 pressed |
| 0x51 | Keypad 3 pressed | 0x52 | Keypad 0 pressed | 0x53 | Keypad . pressed | 0x54 | | 
| 0x55 | | 0x56 | | 0x57 | F11 pressed | 0x58 | F12 pressed |
| 0x59 | | 0x5A | | 0x5B | | 0x5C | | 
| 0x5D | | 0x5E | | 0x5F | | 0x60 | | 
| 0x61 | | 0x62 | | 0x63 | | 0x64 | | 
| 0x65 | | 0x66 | | 0x67 | | 0x68 | | 
| 0x69 | | 0x6A | | 0x6B | | 0x6C | | 
| 0x6D | | 0x6E | | 0x6F | | 0x70 | | 
| 0x71 | | 0x72 | | 0x73 | | 0x74 | | 
| 0x75 | | 0x76 | | 0x77 | | 0x78 | | 
| 0x79 | | 0x7A | | 0x7B | | 0x7C | | 
| 0x7D | | 0x7E | | 0x7F | | 0x80 | |
| 0x81 | Escape released | 0x82 | 1 released | 0x83 | 2 released | 0x84 | 3 released | 
| 0x85 | 4 released | 0x86 | 5 released | 0x87 | 6 released | 0x88 | 7 released | 
| 0x89 | 8 released | 0x8A | 9 released | 0x8B | 0 released | 0x8C | - released | 
| 0x8D | = released | 0x8E | Backspace released | 0x8F | Tab released | 0x90 | Q released | 
| 0x91 | W released | 0x92 | E released | 0x93 | R released | 0x94 | T released | 
| 0x95 | Y released | 0x96 | U released | 0x97 | I released | 0x98 | O released | 
| 0x99 | P released | 0x9A | \[ released | 0x9B | \] released | 0x9C | Enter released | 
| 0x9D | Left control released | 0x9E | A released | 0x9F | S released | 0xA0 | D released | 
| 0xA1 | F released | 0xA2 | G released | 0xA3 | H released | 0xA4 | J released | 
| 0xA5 | K released | 0xA6 | L released | 0xA7 | ; released | 0xA8 | ' released | 
| 0xA9 |  \` released | 0xAA | Left shift released | 0xAB | \ released | 0xAC | Z released | 
| 0xAD | X released | 0xAE | C released | 0xAF | V released | 0xB0 | B released |
| 0xB1 | N released | 0xB2 | M released | 0xB3 | , released | 0xB4 | . released |
| 0xB5 | / released | 0xB6 | Right shift released | 0xB7 | Keypad * released | 0xB8 | Left alt released | 
| 0xB9 | Space released | 0xBA | Caps-lock released | 0xBB | F1 released | 0xBC | F2 released | 
| 0xBD | F3 released | 0xBE | F4 released | 0xBF | F5 released | 0xC0 | F6 released | 
| 0xC1 | F7 released | 0xC2 | F8 released | 0xC3 | F9 released | 0xC4 | F10 released | 
| 0xC5 | Num-lock released | 0xC6 | Scroll-lock released | 0xC7 | Keypad 7 released | 0xC8 | Keypad 8 released |
| 0xC9 | Keypad 9 released | 0xCA | Keypad - released | 0xCB | Keypad 4 released | 0xCC | Keypad 5 released | 
| 0xCD | Keypad 6 released | 0xCE | Keypad + released | 0xCF | Keypad 1 released | 0xD0 | Keypad 2 released | 
| 0xD1 | Keypad 3 released | 0xD2 | Keypad 0 released | 0xD3 | Keypad . released | 0xD4 | |
| 0xD5 |  | 0xD6 |  | 0xD7 | F11 released | 0xD8 | F12 released |  
| 0xD9 |  | 0xDA |  | 0xDB |  | 0xDC |  |
| 0xDD |  | 0xDE |  | 0xDF |  | 0xE0 | This is a prefix byte and is followed by an additional byte |
|  |  |  |  |  |
| 0xE0, 0x1C | Keypad enter pressed | 0xE0, 0x1D | Right control pressed | 0xE0, 0x35 | Keypad / pressed | 0xE0, 0x38 | Right alt (or altGr) pressed |
| 0xE0, 0x47	| Home pressed | 0xE0, 0x48 | Cursor up pressed | 0xE0, 0x49 | Page up pressed | 0xE0, 0x4B | Cursor left pressed |
| 0xE0, 0x4D | Cursor right pressed | 0xE0, 0x4F | End pressed | 0xE0, 0x50 | Cursor down pressed | 0xE0, 0x51 | Page down pressed | 
| 0xE0, 0x52 | Insert pressed | 0xE0, 0x53 | Delete pressed | 0xE0, 0x5B | Left GUI pressed | 0xE0, 0x5C | Right GUI pressed | 
| 0xE0, 0x5D | "Apps" pressed | 0xE0, 0x9C | Keypad enter released | 0xE0, 0x9D | Right control released | 0xE0, 0xB5 | Keypad / released | 
| 0xE0, 0xB8 | Right alt (or altGr) released | 0xE0, 0xC7	| Home released | 0xE0, 0xC8 | Cursor up released | 0xE0, 0xC9 | Page up released |
| 0xE0, 0xCB | Cursor left released | 0xE0, 0xCD | Cursor right released | 0xE0, 0xCF | End released | 0xE0, 0xD0 | Cursor down released | 
| 0xE0, 0xD1 | Page down released | 0xE0, 0xD2 | Insert released | 0xE0, 0xD3 | Delete released | 0xE0, 0xDB | Left GUI released |
| 0xE0, 0xDC | Right GUI released | 0xE0, 0xDD | "Apps" released | | | | |
|  |  |  |  |  |
| 0xE0, 0x2A, 0xE0, 0x37 | Print screen pressed |  |  |  |  |  |  |
| 0xE0, 0xB7, 0xE0, 0xAA | Print screen released |  |  |  |  |  |  |
| 0xE1, 0x1D, 0x45, 0xE1, 0x9D, 0xC5 | Pause pressed |  |  |  |  |  |  |

##### Scancode Set 2

| Scancode | Key | Scancode | Key | Scancode | Key | Scancode | Key |
|:-------------:|:-------:|:-------------:|:-------:|:-------------:|:-------:|:-------------:|:------:|
| 0x01 | F9 pressed | 0x02 |   | 0x03 | F5 pressed | 0x04 | F3 pressed |
| 0x05 | F1 pressed | 0x06 | F2 pressed | 0x07 | F12 pressed | 0x08 |   |
| 0x09 | F10 pressed | 0x0A | F8 pressed | 0x0B | F6 pressed | 0x0C | F4 pressed |
| 0x0D | Tab pressed | 0x0E | \` pressed | 0x0F |  | 0x10 |   |
| 0x11 | Left alt pressed | 0x12 | Left shift pressed | 0x13 |   | 0x14 | Left control pressed |
| 0x15 | Q pressed | 0x16 | 1 pressed | 0x17 |   |  0x18 |   |
| 0x19 |   | 0x1A | Z pressed | 0x1B | S pressed | 0x1C | A pressed |
| 0x1D | W pressed | 0x1E | 2 pressed | 0x1F |   | 0x20 |   |
| 0x21 | C pressed | 0x22 | X pressed | 0x23 | D pressed | 0x24 | E pressed |
| 0x25 | 4 pressed | 0x26 | 3 pressed | 0x27 |   | 0x28 |   |
| 0x29 | Space pressed | 0x2A | V pressed | 0x2B | F pressed | 0x2C | T pressed |
| 0x2D | R pressed | 0x2E | 5 pressed | 0x2F |   |  | 0x30 |
| 0x31 | N pressed | 0x32 | B pressed | 0x33 | H pressed | 0x34 | G pressed |
| 0x35 | Y pressed | 0x36 | 6 pressed | 0x37 |   | 0x38 |   |
| 0x39 |   | 0x3A | M pressed | 0x3B | J pressed | 0x3C | U pressed |
| 0x3D | 7 pressed | 0x3E | 8 pressed | 0x3F  |   | 0x40 |   |
| 0x41 | , pressed | 0x42 | K pressed | 0x43 | I pressed | 0x44 | O pressed |
| 0x45 | 0 pressed | 0x46 | 9 pressed | 0x47 |   | 0x48  |   |
| 0x49 | . pressed | 0x4A | / pressed | 0x4B | L pressed | 0x4C |  ; pressed |
| 0x4D | P pressed | 0x4E | - pressed | 0x4F |   | 0x50  |   |
| 0x51 |   | 0x52 | ' pressed | 0x53  |   | 0x54 | [ pressed |
| 0x55 | = pressed | 0x56 |   | 0x57  |   | 0x58 | Caps-lock pressed |
| 0x59 | Right shift pressed | 0x5A | enter pressed | 0x5B | ] pressed | 0x5C  |  |
| 0x5D | \ pressed | 0x5E  |   | 0x5F |   | 0x60 |   |
| 0x61 |  | 0x62  |   | 0x63 |   | 0x64 |   |
| 0x65 |   | 0x66 | Backspace pressed | 0x67 |   | 0x68 |   |
| 0x69 | Keypad 1 pressed | 0x6A |   | 0x6B | Keypad 4 pressed | 0x6C | Keypad 7 pressed |
| 0x6D |   | 0x6E |   | 0x6F |   | 0x70 | Keypad 0 pressed |
| 0x71 | Keypad . pressed | 0x72 | Keypad 2 pressed | 0x73 | Keypad 5 pressed | 0x74 | Keypad 6 pressed |
| 0x75 | Keypad 8 pressed | 0x76 | Escape pressed | 0x77 | Num-lock pressed | 0x78 | F11 pressed |
| 0x79 | Keypad + pressed | 0x7A | Keypad 3 pressed | 0x7B | Keypad - pressed | 0x7C | Keypad * pressed |
| 0x7D | Keypad 9 pressed | 0x7E | Scroll-lock pressed | 0x7F  |   | 0x80 |   |
| 0x81 |  | 0x82  |  | 0x83 | F7 pressed | 0x84 |  |
|  |  |  |  |  |  |  |  |
| 0xE0, 0x10 | WWW search pressed | 0xE0, 0x11 | Right alt pressed |
| 0xE0, 0x14 | Right control pressed | 0xE0, 0x15 | Previous track pressed |
| 0xE0, 0x18 | WWW favourites pressed |
| 0xE0, 0x1F | Left GUI pressed |
| 0xE0, 0x20 | WWW refresh pressed | 0xE0, 0x21 | Volume down pressed | 0xE0, 0x22 |  | 0xE0, 0x23 | Mute pressed |
| 0xE0, 0x27 | Right GUI pressed |
| 0xE0, 0x28 | WWW stop pressed |
| 0xE0, 0x2B | Calculator pressed |
| 0xE0, 0x2F | Apps pressed |
| 0xE0, 0x30 | WWW forward pressed | 0xE0, 0x31 |  | 0xE0, 0x32 | Volume up pressed |
| 0xE0, 0x34 | Play/pause pressed | 0xE0, 0x35 |  | 0xE0, 0x36 |  | 0xE0, 0x37 | (ACPI) Power pressed |
| 0xE0, 0x38 | WWW back pressed | 0xE0, 0x39 |  | 0xE0, 0x3A | WWW home pressed | 0xE0, 0x3B | Stop pressed |
| 0xE0, 0x3F | (ACPI) sleep pressed | 0xE0, 0x40 | My computer pressed |
| 0xE0, 0x48 | Email pressed | 0xE0, 0x49 |  | 0xE0, 0x4A | Keypad / pressed |
| 0xE0, 0x4D | Next track pressed	| 
| 0xE0, 0x50 | Media select pressed |
| 0xE0, 0x5A | Keypad enter pressed |
| 0xE0, 0x5E | (ACPI) Wake pressed |
| 0xE0, 0x69 | End pressed |
| 0xE0, 0x6B | Cursor left pressed | 0xE0, 0x6C | Home pressed |
| 0xE0, 0x70 | Insert pressed | 0xE0, 0x71 | Delete pressed | 0xE0, 0x72 | Cursor down pressed |
| 0xE0, 0x74 | Cursor right pressed | 0xE0, 0x75 | Cursor up pressed |
| 0xE0, 0x7A | Page down pressed |  | 
| 0xE0, 0x7D | Page up pressed |
|  |  |  |  |  |  |  |  |
| 0xF0, 0x01 | F9 released | 0xF0, 0x02 |  | 0xF0, 0x03	| F5 released | 0xF0, 0x04 | F3 released | 
| 0xF0, 0x05 | F1 released | 0xF0, 0x06 | F2 released | 0xF0, 0x07 | F12 released |
| 0xF0, 0x09 | F10 released | 0xF0, 0x0A | F8 released | 0xF0, 0x0B	| F6 released | 0xF0, 0x0C | F4 released | 
| 0xF0, 0x0D | Tab released | 0xF0, 0x0E | \` released |
| 0xF0, 0x11 | Left alt released | 0xF0, 0x12 | Left shift released |
| 0xF0, 0x14 | Left control released | 0xF0, 0x15 | Q released | 0xF0, 0x16 | 1 released |  |  |
| 0xF0, 0x1A | Z released | 0xF0, 0x1B | S released | 0xF0, 0x1C | A released | 0xF0, 0x1D | W released | 
| 0xF0, 0x1E | 2 released |
| 0xF0, 0x21 | C released | 0xF0, 0x22 | X released | 0xF0, 0x23 | D released | 0xF0, 0x24 | E released | 
| 0xF0, 0x25 | 4 released | 0xF0, 0x26 | 3 released |
| 0xF0, 0x29 | Space released | 0xF0, 0x2A | V released | 0xF0, 0x2B | F released | 0xF0, 0x2C | T released | 
| 0xF0, 0x2D | R released | 0xF0, 0x2E | 5 released |
| 0xF0, 0x31 | N released | 0xF0, 0x32 | B released | 0xF0, 0x33 | H released | 0xF0, 0x34 | G released | 
| 0xF0, 0x35 | Y released | 0xF0, 0x36 | 6 released |
| 0xF0, 0x3A | M released | 0xF0, 0x3B | J released | 0xF0, 0x3C | U released | 0xF0, 0x3D | 7 released | 
| 0xF0, 0x3E | 8 released |
| 0xF0, 0x41 | , released | 0xF0, 0x42 | K released | 0xF0, 0x43 | I released | 0xF0, 0x44 | O released | 
| 0xF0, 0x45 | 0 released | 0xF0, 0x46 | 9 released |
| 0xF0, 0x49 | . released | 0xF0, 0x4A | / released | 0xF0, 0x4B | L released | 0xF0, 0x4C | ; released | 
| 0xF0, 0x4D | P released | 0xF0, 0x4E | - released |
| 0xF0, 0x52 | ' released |
| 0xF0, 0x54 | \[ released | 0xF0, 0x55 | = released |  |  |  |  |
| 0xF0, 0x58 | Caps-lock released | 0xF0, 0x59 | Right shift released | 0xF0, 0x5A | Enter released | 0xF0, 0x5B | \] released |
| 0xF0, 0x5D | \ released |
| 0xF0, 0x66 | Backspace released |
| 0xF0, 0x69 | Keypad 1 released | 0xF0, 0x6B | Keypad 4 released |
| 0xF0, 0x6C | Keypad 7 released |
| 0xF0, 0x70 | Keypad 0 released | 0xF0, 0x71 | Keypad . released | 0xF0, 0x72 | Keypad 2 released | 0xF0, 0x73 | Keypad 5 released |
| 0xF0, 0x74 | Keypad 6 released | 0xF0, 0x75 | Keypad 8 released | 0xF0, 0x76 | escape released | 0xF0, 0x77 | Num-lock released |
| 0xF0, 0x78 | F11 released | 0xF0, 0x79 | Keypad + released | 0xF0, 0x7A | Keypad 3 released | 0xF0, 0x7B | Keypad - released |
| 0xF0, 0x7C | Keypad * released | 0xF0, 0x7D | Keypad 9 released | 0xF0, 0x7E | ScrollLock released |  |  |
| 0xF0, 0x83 | F7 released |
| 0xE0, 0x12, 0xE0, 0x7C | Print screen pressed |
| 0xE0, 0xF0, 0x10 | WWW search released | 0xE0, 0xF0, 0x11 | Right alt released |
| 0xE0, 0xF0, 0x14 | Right control released | 0xE0, 0xF0, 0x15 | Previous track released |
| 0xE0, 0xF0, 0x18 | WWW favourites released |
| 0xE0, 0xF0, 0x1F | Left GUI released | 0xE0, 0xF0, 0x20 | WWW refresh released | 0xE0, 0xF0, 0x21 | Volume down released |
| 0xE0, 0xF0, 0x23 | Mute released |
| 0xE0, 0xF0, 0x27 | Right GUI released | 0xE0, 0xF0, 0x28 | WWW stop released |
| 0xE0, 0xF0, 0x2B | Calculator released |
| 0xE0, 0xF0, 0x2F | Apps released |
| 0xE0, 0xF0, 0x30 | WWW forward released |  |  | 0xE0, 0xF0, 0x32 | Volume up released |
| 0xE0, 0xF0, 0x34 | Play/pause released |  |  |  |  | 0xE0, 0xF0, 0x37 | (ACPI) Power released |
| 0xE0, 0xF0, 0x38 | WWW back released |  |  | 0xE0, 0xF0, 0x3A | WWW home released | 0xE0, 0xF0, 0x3B | Stop released |
| 0xE0, 0xF0, 0x3F | (ACPI) sleep released |
| 0xE0, 0xF0, 0x40 | My computer released |
| 0xE0, 0xF0, 0x48 | Email released |  |  | 0xE0, 0xF0, 0x4A | Keypad / released |
| 0xE0, 0xF0, 0x4D | Next track released | 
| 0xE0, 0xF0, 0x50 | Media select released |
| 0xE0, 0xF0, 0x5A | Keypad enter released |
| 0xE0, 0xF0, 0x5E | (ACPI) Wake released |
| 0xE0, 0xF0, 0x69 | End released |  |  | 0xE0, 0xF0, 0x6B | Cursor left released | 0xE0, 0xF0, 0x6C | Home released |
| 0xE0, 0xF0, 0x70 | Insert released | 0xE0, 0xF0, 0x71 | Delete released | 0xE0, 0xF0, 0x72 | Cursor down released |
| 0xE0, 0xF0, 0x74 | Cursor right released | 0xE0, 0xF0, 0x75 | Cursor up released |
| 0xE0, 0xF0, 0x7A | Page down released |  |  |  |  | 0xE0, 0xF0, 0x7D | Page up released |
| 0xE0, 0xF0, 0x7C, 0xE0, 0xF0, 0x12 | Print screen released |
| 0xE1, 0x14, 0x77, 0xE1, 0xF0, 0x14, 0xF0, 0x77 | Pause pressed |

##### Scancode Set 3

A table for scancode set 3 can be found here: [http://www.computer-engineering.org/ps2keyboard/scancodes3.html](http://www.computer-engineering.org/ps2keyboard/scancodes3.html)

## Implementation details

### Basic keyboard driver

It is possible to implement a basic PS/2 keyboard driver that works without needing to do all the complex initialisation described below. The basic driver performs only the following steps (which are described in detail lower down as part of the full driver description).

1. Driver initialisation:
  1. Preallocate key buffer (if that approach is being taken)
  2. Register & enable IRQ
2. Handle IRQ
3. Read scancode/key/char methods
4. Caps-lock tracking

### Driver using polling

A driver which intends to use polling needs to perform all the same steps as an IRQ-based driver except for enabling and registering the IRQ handler. Furthermore, the driver must have a polling method (which will be blocking) and which is either called by a separate thread or when a scancode is requested.

1. Driver initialisation (see Driver using interrupts)
2. Polling (with timeout)
3. Read scancode/key/char methods (see Driver using interrupts)
4. Caps-lock, num-lock, scroll-lock tracking (see Driver using interrupts)
5. Changing caps-lock, num-lock and scroll-lock lights (see Driver using interrupts)

#### 2. Polling (with timeout)
A polling-based driver needs to have a method that will poll the PS/2 controller to see if data is available (i.e. if a key has been pressed). This method can be run in a separate thread or called when a scancode is requested (remembering that requesting a keycode or char will need to request a scancode for conversion). 

To poll the PS/2 controller for data, read the status register and wait for the Controller Output Buffer Full bit (bit 0) to become set, indicating that data is available in the Data port. Read the byte of data from the Data port (port 0x60) and repeat until the complete scancode has been read.

Unfortunately, aside from the usual issues with polling, there is another issue for PS/2 polling. If the PS/2 controller has two devices attached (mouse and keyboard) they are both able to send data simultaneously. The data will arrive in the controller output buffer mixed together. Using polling, there is no way to distinguish the two reliably. Even using the 2nd port's output buffer status does not eliminate the race condition. The only two options are to disable one of the two devices (in this case, it would probably be the mouse) or to use IRQs (which guarantee you know which device's data will be read next from the buffer).

### Driver using interrupts
1. Driver initialisation:
    0. [Check for USB]
    1. Reset &amp; disable
    2. Flush buffers
    3. Set Controller Configuration
    4. Perform PS/2 controller self-test
    5. Determine number of channels
    6. Perform channel (/interface) tests
    7. Preallocate key buffer (if that approach is being taken)
    8. Register & enable IRQ
    9. Enable devices
    10. Reset devices
    11. Device type detection:
        1. Disable scanning
        2. Use identify command
2. Handle IRQ:
    1. Defer if going to be using queuing which needs memory allocation 
    2. Deferred or not: 
        1. Read scancode (necessary to receive further interrupts)
        2. Queue/buffer scancode
3. Read scancode/key/char methods
4. Caps-lock, num-lock, scroll-lock tracking
5. Changing caps-lock, num-lock and scroll-lock lights
6. USB Legacy Support

Reading the data port (after an IRQ has occurred) provides the single-byte scancode of the key that was pressed (or released). Note that if two devices are attached to the PS/2 controller, data from either device could be received mixed together from the data port. The IRQ is the only reliable way to know which device's data will be read next from the buffer. 

#### Sending commands to the PS/2 Controller
Whenever the PS/2 driver wishes to send a command to the PS/2 controller, it should wait for the Input Buffer Full flag to clear in the Status register. This may require some form of timeout and error condition in case the PS/2 controller hangs/fails to processor a command. Commands are written as single bytes for the Command Register via the Command Port (0x64) which is write-only. 

#### Sending commands to PS/2 Devices
Whenever the PS/2 driver wishes to send a command to a PS/2 device, it must wait for the Controller Input Buffer to be empty. This means polling the Input Buffer Full flag bit (bit 1) in the Status register until it clears. 

Once the input buffer is empty, to send a command to a device on the first port, just write the command to the Data port. To send a command to a device on the second port, you must first send the Write to Second Channel Input Buffer command (0xD4) then write the command to the data port. 

In almost all cases, the driver should then wait for the command to send the ACK response (the exception is for device reset commands or commands with no response). In all cases, the driver should check for the Resend response (0xFE). If Resend is received from the device, the last command sent to the device should be resent.

If you send the Write to Second Channel Input Buffer command (0xD4) when a second channel is not supported by the PS/2 controller, the command will be ignored. Any data subsequently written to the Data port will then be sent to the first PS/2 channel, which could cause confusion. Make sure, therefore, that the PS/2 Controller actually supports two channels before trying to write to the second one.

#### 1. Initialisation

0. ***Check for USB*** - The PS/2 controller should check whether USB drivers are enabled and, if so, whether they support a USB Keyboard HID driver. If they do, USB Legacy Support should be disabled. The PS/2 controller can then continue as normal. If there are no USB drivers, then the PS/2 controller can assume USB Legacy Support is enabled.
1. ***Reset &amp; disable*** - The PS/2 controller could be in any state (after the BIOS and bootloader have run) so it is important to reset it. In practice, this just means disabling any/all devices (/ports) so they can't interfere with the rest of the initialisation process. To disable devices send the disable first and disable second PS/2 port commands (0xAD and 0xA7 respectively). PS/2 controller with only a single port will ignore the second command. There is no way to determine at this stage, whether there are one or two ports attached to the PS/2 controller.
2. ***Flush buffers*** - Since the driver is in the processor of clearing the PS/2 controller setup, it is possible that the PS/2 controller was sent some data between the bootloader ending and the driver starting. This data would be sitting in the output buffer without us knowing and we would not receive any further data (or IRQs) until it is cleared. This can be achieved by polling the Output Buffer Full bit (bit 0) of the status register until it becomes clear (/zero). While the bit remains set, the driver should read bytes from the output buffer (by reading the Data port) and just discarding the data read.
3. ***Set Controller Configuration*** - The PS/2 Controller is in an unknown configuration so the driver has two options. Either to rely on the configuration set by the bootloader or BIOS, or to reconfigure the controller for itself. If you want to leave the configuration as-is then fine but it may produce variable results on different hardware or in different virtual machines. To update the controller configuration, the driver should read the existing configuration byte, change the necessary bits then write the new value back. 
    
    This involves sending the Read Configuration command (0x20) then reading the response from the Data port. This gives you the existing config byte. At this stage, you should disable all IRQs and disable translation by clearing bits 0, 1 and 6. Write the new value back by sending the Write Configuration command (0x60) followed by the config byte written to the Data port.
4. ***PS/2 controller self-test*** - This is to make sure the PS/2 controller is functioning properly and prevents the driver from becoming stuck later if the PS/2 controller is faulty. Perform the self-test by sending the self-test command (0xAA) then reading the response. It should be 0x55. If it is not, abort the driver initialisation and disable the PS/2 driver in the kernel.
5. ***Determine number of channels*** - This tells the driver whether one or two PS/2 devices (/channels) are supported by the PS/2 controller. First the driver must enable the second port then read the Controller Configuration byte to determine if a second device is supported (but not necessarily attached). If the second channel exists, the driver should disable it again afterwards.
    
    To enable the second port, the driver should send the Enable Second Port command (0xA8). It should then read the config byte (as before) by sending the Read Configuration command (0x20) and read the value from the data port. The Second PS/2 Port Clock Enable bit (bit 5) should be clear if the second PS/2 device is supported. If the bit is clear, the driver should send the Disable Second PS/2 Port command (0xA7) to re-disable the port (so that it doesn't interfere with the remainder of the setup process). 
6. ***Perform channel (/interface) tests*** - This determines whether any of the interfaces are broken or not. If one interface is broken, the other can still be used. If all interfaces are broken then the PS/2 driver should abort. 
    
    Test the first interface by send the Test First PS/2 Port command (0xAB) and checking the response. It should be 0x00 if the test passed. Otherwise, keep track that the first port is broken. If a second port exists (as determined by previous step) then send the Test Second PS/2 command (0xA9) and again check the result is zero. 
7. ***Preallocate key buffer*** (if that approach is being taken) - If the driver will be using a pre-allocated scancode buffer, it should be allocated at this stage. 
8. Register & enable IRQ - If the driver is going to use IRQs (which ideally it should), this is the point at which the IRQ handler should be registered and enabled. This will rely on the kernel's IRQ and interrupt handling system, which is described in separate articles. The IRQ for the first PS/2 port (which is almost always the keyboard) is IRQ1 and the IRQ for the second PS/2 channel is IRQ12 (almost always the PS/2 mouse). 
9. ***Enable devices*** - At this stage the driver should enable one or both PS/2 ports (depending on how many are available and whether polling is being used). Do so by sending the relevant PS/2 Port Enable commands (0xAE for 1st, 0xA8 for second) and, if IRQs are being used, enable the channel interrupts by reading the config byte (using the Read command - 0x20), setting the interrupt enable bits (bit 0 for 1st, bit 1 for second) and then writing the config byte (using the Write command - 0x60). 
10. ***Reset devices*** - All attached PS/2 devices should be reset to make sure they are starting from a clean state. This stage also allows the driver to detect if any devices are actually plugged in or not. If hot plugging is supported, this step (and the next) can be repeated later to detect newly plugged-in devices. 
    
    To reset a device, send the Reset Device command (which is a command for the device, not the Controller). The Reset Device command (0xFF) also starts a self test. If the device is attached it will respond with 0xAA. If 0xAA is not received, then no device is plugged in and the channel should be ignored. If 0xAA is received then a device is attached and one more byte should be received. This additional byte is the result of the self-test and should be Self-Test Passed (which is also the ACK response - 0xFA). A value of 0xFC or 0xFD indicates the self-test failed and the device should be disabled.
11. ***Device type detection*** - If a device resets and self-tests successfully then the driver can proceed to test the device type using the following steps.
      1. ***Disable scanning*** - This prevents the device from sending scanning data mixed in with the command response data (which would otherwise be indistinguishable). To disable scanning, send the Disable Scanning command (0xF5) and wait for the ACK response.
      2. **Identify device*** - Do this by sending the Identify Device command (0xF2). Wait for the ACK response and then read zero, one or two ID bytes. The driver will need to use a timeout to determine how many bytes have been sent. The values of the identification bytes are shown in the table below.
      3. ***Re-enable scanning*** - To enable scanning again, send the Enable Scanning command (0xF4) to the device and wait for the ACK response.
  
| Byte(s) | Device Type |
|:----------:|:------------------|
| None  | Very old AT keyboard with translation enabled in the PS/Controller (which is not possible for the second PS/2 port.) |
| 0x00  | Mouse |
| 0x03  | Mouse with scroll wheel |
| 0x04  | 5-button mouse |
| 0xAB, 0x41 or 0xAB, 0xC1 | MF2 keyboard with translation enabled in the PS/Controller (which is not possible for the second PS/2 port.) |
| 0xAB, 0x83 | MF2 keyboard |

#### 2. Handling the IRQ
Handling the keyboard IRQ is straightforward. Alongside all the normal IRQ handling (such as End of Interrupt notifications), the IRQ handler must read the scancode byte(s) from the Data port.

The IRQ handler should read a single byte from the Data port. It should then check this to see if the scancode is a multi-byte scancode or not. If it is a multi-byte scancode, the remaining scancode bytes should be read. Once the complete scancode has been read, the IRQ handler should queue (/buffer) the scancode. No further work is necessary.

Later steps will expand upon the IRQ handler to show how modifier keys can be tracked and status lights updated.

#### 3. Reading a scancode
Reading a scancode (after the IRQ handler has read all the scancode bytes and queued them) involves checking the scancode bytes to determine which key was involved and whether the key was pressed or released. It may also be desired that the shift-key modifier be removed from the scancode. 

To read a scancode, check it against the relevant scancode table above. Most modern keyboards use scancode set 2 (sometimes 3) by default. International or non-US keyboards may require a different mapping. There is no easy way to auto-detect key mapping without user-input at runtime. 

If the highest bit of the scancode byte is set, then the key was released, otherwise it was pressed. This bit can be cleared and its value kept track of elsewhere. The resulting scancode will only match "pressed" scancodes. 

To keep track of whether a key was pressed while the shift key was pressed, it is possible to expand the scancode to a 32-bit value (rather than just one byte). Then, if the shift key was pressed, left shift the value by 16 bits. This allows the keyboard driver to distinguish between upper-case and lower-case key presses. The raw scancode from the keyboard does not include any modification for the shift key.

#### 4. Caps-lock, num-lock and scroll-lock tracking
Caps-lock, num-lock and scroll-lock must be kept track of by the driver. These key's scancodes should be detected during the IRQ and flags in the driver set/cleared as appropriate. 

#### 5. Status lights
When the caps-lock, num-lock or scroll-lock on/off status changes, it is useful (for the user) if the driver switches on/off the relevant status lights. This can be achieved by sending the Status Lights command with the relevant option bits set. See the Keyboard Commands table for details.

#### 6. USB Legacy Support
Many modern PCs do not come with PS/2 ports (or even Controllers). The USB Host Controller built onto the motherboard emulates USB Keyboards and Mice as PS/2 keyboards/mice. If USB Host Controller drivers exist and USB Keyboard (or Mouse) HID drivers exist, they should be initialised before the PS/2 driver. USB Legacy Support should be disabled by the USB Host Controller driver (prior to device driver initialisation) if USB HID drivers exist.

Contrary to popular opinion, it is not required that USB Legacy Support be disabled if a USB driver exists. The USB driver must simply be well-coded. The USB driver must keep track of which USB ports are being used by devices which are under PS/2 emulation and simply take precautions not to reset those ports at any stage after the PS/2 driver has been initialised. It must also avoid resetting the entire host controller at any stage after the PS/2 driver has been initialised (unless the PS/2 driver is disabled before the reset and re-initialised afterwards).

### CPU Reset
CPU Reset can be achieved by pulsing the CPU Reset line. This is line 0 of the PS/2 controller output (the other lines do not have standardised connections). The pulse line 0, send the Pulse Output Line command (0xF0) to the PS/2 controller, with bit 0 clear and bits 1 to 3 set. A set bit indicates a line should not be pulsed, a clear bit indicates a line should be pulsed. Thus the actual value to send should be 0xFE. This should be written as a single byte to the command register. It will cause the CPU to reset. When the pulse ends, the CPU will restart to the BIOS and continue as though it had just powered-on for the first time.

The following code snippet demonstrates how to send the CPU Reset pulse in NASM assembly code. As with any command, the code waits for the PS/2 controller input buffer to be empty before sending the command.

``` x86asm
CPUReset:
  ; Wait for the PS/2 controller Input Buffer to be empty
  Wait:
    in AL, 0x64   ; Read the Status register value
    test AL, 0x2  ; Test bit 1 of the Status register (Input Buffer Full bit. 0=Empty, 1=Full)
    jnz Wait      ; While it's still full, loop back around

  ; Send the Pulse Output Lines command to the PS/2 controller.
  ; Command=0xF0, Options=0x0E thus command value = 0xFE
  mov AL, 0xFE   ; Load the value to send
  out 0x64, AL   ; Write the value to the command register
  
  ; This method will never return because the CPU will reset.
```

The following is from the FlingOS Kernel.Hardware.Keyboards.PS2.Reset method and provides a C# sample.

``` csharp
[Compiler.NoGC]
[Compiler.NoDebug]
public void Reset()
{
    // If the driver is enabled
    if (enabled)
    {
        // Wait for the Input Buffer Full flag to clear
        byte StatusRegValue = 0x02;
        while ((StatusRegValue & 0x02) != 0)
        {
            StatusRegValue = CommandPort.Read_Byte();
        }

        // Send the command | options 
        //          (0xF0   | 0x0E    - pulse only line 0 - CPU reset line)
        CommandPort.Write_Byte(0xFE);
    }
}
```

## Alternatives
There are several alternatives for text input to the traditional keyboard. A microphone can be used for voice recognition, cameras and touch screens can be used for text and handwriting recognition.

---

# Example Code

## Overview
TODO

## Download
TODO

---

# FAQ & Common Problems

## I only ever receive one key press / scancode?
You are probably not reading any or enough of the scancode bytes from the Data port when the first IRQ occurs. If you don't empty the Data port buffer, the PS/2 controller won't signal another interrupt when another key is pressed.

## My IRQ handler hangs or enters an infinite loop or locking condition?
You probably tried to perform a memory allocation (or use some other forbidden function) from within the IRQ handler. You must either pre-allocate memory or defer the interrupt handling using a separate thread. Alternatively, use polling.

---

# References
All links referenced were valid as of 2015-08-26.

- [Wikipedia.org - PS/2 Port](https://en.wikipedia.org/wiki/PS/2_port)
- [Wikipedia.org - Mini-DIN connector](https://en.wikipedia.org/wiki/Mini-DIN_connector)
- [Wikipedia.org - DIN connector](https://en.wikipedia.org/wiki/DIN_connector)
- [Wikipedia.org - Computer Keyboard : Control Processor](https://en.wikipedia.org/wiki/Computer_keyboard#Control_processor)
- [ComputerHope.com - PS2](http://www.computerhope.com/jargon/p/ps2.htm)
- [Computer-engineering.org - PS2 Protocol](http://www.computer-engineering.org/ps2protocol/)
- [Computer-engineering.org - PS2 Keyboard](http://www.computer-engineering.org/ps2keyboard/)
- [Pjrc.com - PS2 Keyboard](https://www.pjrc.com/teensy/td_libs_PS2Keyboard.html)
- [PCBHeaven.com - How Key Matrices Work](http://pcbheaven.com/wikipages/How_Key_Matrices_Works/)
- [Wikipedia.org - Keyboard Matrix Cicuit](https://en.wikipedia.org/wiki/Keyboard_matrix_circuit)
- [OSDev.org - "8042" PS/2 Controller](http://wiki.osdev.org/%228042%22_PS/2_Controller)
- [OSDev.org - PS/2 Keyboard](http://wiki.osdev.org/PS/2_Keyboard)
- [Stanislavs.org - 8042 PS/2 Controller](http://stanislavs.org/helppc/8042.html)
- [Computer-engineering.org - Scancode Set 3](http://www.computer-engineering.org/ps2keyboard/scancodes3.html)

*[PS2]: IBM Personal System/2 (or PlayStation 2 depending on context)
*[PS/2]: IBM Personal System/2
*[USB]: Universal Serial Bus
*[FAQ]: Frequently Asked Question(s)
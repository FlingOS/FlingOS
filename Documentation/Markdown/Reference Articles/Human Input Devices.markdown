---
layout: reference-article
title: Human Input Devices
date: 2015-08-17 23:51:00
categories: docs reference
---

# Introduction
Human Input Devices refers to the generic set of devices which humans can use for Input/Output (IO) tasks, with a primary focus on input-based devices such as mice and keyboards (as opposed to output-based devices such as a screen). Human Input Devices is not the same as Human Interface Devices, which is usually used to refer to the USB HID specification. USB HID will be discussed briefly below but is not the main content of this article. 

## Scope of this article
This article will cover information about a range of Human Input Devices and will also briefly explain the USB Human Interface Device specification and how it ties in with Human Input Devices. 

---

# History

Human Input Devices date back to long before true computers were invented. Our interactions with machines have become increasingly complex centuries prior to 1900 going from levers and pulleys to entire steam engines and even the humble typewriter. The enigma machine and its enemy machine the Bombe demonstrated two new forms of human input to a machine : plug wires and rotors (respectively). By the 1950s things had advanced a lot further and Ivan Sutherland first demonstrated pen-based drawing on a computer screen which became his PhD thesis in 1963. His program, SketchPad, was pioneering in graphics (see Displays &amp; Graphics article for more detail) and human interaction. The program allowed drawing, resizing, dragging and constraining objects. 

The mouse was first developed by the Standard Research Laboratory (now called SRI) in 1965 (not by Apple, as is commonly believed). It was intended to be a cheap replacement for Sutherland's light pens and many of the current mouse interactions were demonstrated by Doug Engelbert in 1968. Since then the mouse has not changed a great deal in terms of how it functions. Extra side buttons have been added to some devices, laser light instead of roller balls for movement tracking, ergonomic changes and some changes in software use (such as pointer speed, immersive gaming interaction and similar) but ultimately it still just moves a small icon around the screen. Arguably the biggest change to the mouse was the introduction of the touch pad on laptop devices. 
Xerox and Apple were the first commercial organisations to use and distribute the mouse as part of a computer package. Their machines the Xerox Star (1981), the Apple Lisa (1982) and Apple Macintosh (1984) lead the way in human interaction and Apple, to this day, have a reputation for good UI, UX and human input device designs.

The keyboard was really originally a typewriter but the first origins of the modern computer keyboard are really in teletype and punchcard technologies from the late 1800s to early 1900s. The modern keyboard layout originates from these devices and they were the first electronic systems where a user could type something and it would be printed out. The first teletype systems were commercially used for transmitting stock market data. The user would type and the text would be printed out on ticker tape at the destination. The first computer keyboard as we know it, however, was targeted at programmers so was almost entirely functional and aesthetically awful but was sufficient for inputting programs on the text-only displays of the 1970s. In the late 1970s Radio Shack, Commodore and Apple all saw the potential of the keyboard and started mass production. Since then, output functions, such as capslock lights, have been added and the look and feel of keyboards has changed wildly. Just like the mouse, however, the basic function (inputting keystrokes) has remained the same.

You might imagine that is the end of the story. What other devices do we really use to input to a computer? Plenty others. Joysticks and, more commonly these days, game controllers are a necessity for using games consoles and for PC gaming. IR remotes for TVs, wireless pointing devices such as pens, Wii remotes and even Ultra Haptics (developed at the University of Bristol, UK) and of course, touch-screens have all become or are soon to be major input devices. Most of these technologies (with the exclusion of Wii remotes and Ultra Haptics) have origins in work from the 1950s and 1960s. It is important to realise that many of the seemingly innovative input technologies nowadays are not actually that new. The difference now to back then is the graphical interaction is vastly improved, the accuracy of devices is much higher and the speed and low-cost of the devices means they can be sold to a mass market. Technologies that were previously confined to the lab are being brought out and produced in large volumes.

---

# Overview

## What is a Human Input Device?
A human input device is any device which is primarily aimed at taking user input and passing it to the machine. Such devices include mice, keyboards, game controllers, TV remotes and more. The devices may have some limited output features as well (such as lights and vibration) but they will not usually constitute the primary output device. One exception to this would be Braille keyboards which have both the keyboard input and character output in a single device. 

A human input device is not the same as a human interface device. "Human interface device (HID)" refers to two possible meanings:

1. Any device which acts as an interface (/interaction platform) between humans and machines. This includes output devices as well as input devices. 
2. The USB HID specification and class of devices (which largely deals with input-based devices many of which are Human Input Devices.)

## How do Human Input Devices work?
This is a very broad question and the answer varies wildly from one device (or type of device) to another. However, there are a few general points which can be made:

1. Generally, an input device consists of between zero and three axis of movement (a keyboard is zero, a scroll wheel is one, a mouse is two, a game controller has 3 (or, depending on how you think about it, 6))
2. A significant number of input devices also have buttons which are either inputs in their own right or act as modifiers. Often there is a common set of buttons between types of device. (e.g. keyboard layouts, game controller pads, mouse forward/backward buttons)
3. Generally, input devices have to be very fast to contend with the real-time response expectation that humans have. (This is not just due to impatience. Humans actually feel nauseous (think: sea-sick) if the delay between an input action and the visual response is a fraction too delayed (e.g. a 5ms to 30ms delay) but may not appear to lag noticeably. If you delay it even further the feeling goes away and the whole thing just appears to lag.)
4. Generally, input devices will take only the latest input. If some piece of input fails to transmit, it will be left out entirely. However, inputs from a user will (or should) always arrive in order.

---

# Hardware

## Overview
There are two major bits of hardware to know about when it comes to input devices: PS2 (for mice &amp; keyboards) and USB (for everything). Anything that doesn't use these two is proprietary so without access to the original spec, you won't succeed in programming a driver for it. PS2 is now legacy but is much easier to set up than USB and most USB hardware (inside the PC) have support for PS2 emulation.

## PS2 (Mouse &amp; Keyboard)
PS2, properly written as PS/2, gets its name from IBM's Personal System/2 which was the computer system that introduced the standard for the first time in 1987. PS/2 was electrically compatible with and the communication protocol was the same as, the existing 5-pin DIN connectors. However, keyboards and mice for DIN used a different set of software commands to PS/2 so the two may not have worked together, depending on the particular system and and keyboard/mouse pair. 

The PS/2 connector is a 6-pin mini-DIN connector which uses serial, synchronous, bi-directional communication. What this means is that the basic protocol is serial i.e. every byte is sent in order and received in order. The protocol is synchronous meaning you cannot read and write simultaneously and bi-directional means bytes can be sent both to and from the keyboard. The six pins are assigned as follows (with the slot of the female connector at the bottom, numbers run across the rows, left to right, down the rows and have 1-based indexing).

| Pin | Name | Use |
|:-----:|:---------|:-------|
| 1  | +Data | Data pin for primary device (either mouse or keyboard) |
| 2  | Not connected  | Not connected except on some systems which allow the use of a splitter cable. In that case, this is the data pin for the secondary (opposite type to primary) device. |
| 3  | GND | Ground / 0V pin used as a reference |
| 4  | Vcc | +5V reference pin at 275mA |
| 5  | +CLK | Clock signal (for synchronising communication) |
| 6  | Not connected | Not connected except on some systems which allow the use of a splitter cable. In that case, this is the clock signal pin for the secondary (opposite type to primary) device. |

Convention dictates that keyboard ports are coloured purple and mouse ports are coloured green. While the two are identical in firmware and hardware, the actual ports used to communicate from the processor to the device will be different. This means most software won't, for example, be able to understand a keyboard device plugged into a mouse port. However, because both keyboard and mouse are (usually) handled by a single micro-controller (to save hardware costs) inside the host PC, if either device behaves erratically resulting in confusion at both ends, both keyboard and mouse may appear to be broken. This can lead to misdiagnosis, however, it is a rare situation to be in. An easy test is to shutdown, unplug one of the devices and then restart.

Shutting down and then unplugging is necessary because PS/2 is not (generally) hot pluggable. While for most modern hardware this will not cause any physical problems (though it could easily damage old hardware), most software will not detect the change of device. This means that switching device types on a port or unplugging a mouse/keyboard and plugging in a different model will usually result in the device not working. Due to the PS/2 Reset command (a legacy protocol allowing a single keyboard combination to reset the processor), hot plugging devices can confuse the on-board micro-controller unintentionally resulting in a reset command. 

Most PS/2 connectors were not designed to be unplugged and plugged back in frequently. This means frequent use causes the pins to break which are next to impossible to replace. As a result, leaving a device plugged in is recommended where possible. 

PS/2 mice and keyboards have basically gone now, though many traditional desktop machines still come with PS/2 connectors. The history and use of PS/2 keyboards and mice is discussed in more detail in their respective articles. (At the time of writing, only the "PS/2 Keyboards" article was available.) Sufficed to say, PS/2 keyboards and mice are still the easiest and fastest way for an OS developer to get input from a user (particularly keyboards). Most USB hardware retains support for emulating PS/2 keyboards and mice from USB mice/keyboards (provided the individual devices are never initialised or reset by a USB driver).

## USB (HID)


---

# Software

## Overview
Most Human Input Device driver software is interrupt driven. This means that when the user provides input, the input device signals an Interrupt Request (IRQ) notifying the host processor. The processor calls the interrupt routine which has choice. Either to handle the input data from the device immediately or to defer processing to later. This all happens within the driver.

Software stacks vary a lot but in general drivers queue input data and can then notify applications the data is available. Applications will generally have a separate thread reserved entirely for input and user-interface processing. The thread will be woken by the notification from the driver and the scheduler will subsequently switch execution to that thread. The thread will then dequeue the input data from the driver and handle it.

## Mice &amp; Keyboards
PS/2 mice and keyboards have relatively simple software. The key parts are:

1. Initialisation
2. Handling interrupts from the device
3. Reading and interpreting data from the device
4. Sending back any necessary responses or output (e.g. caps lock light enable/disable)

A keyboard, for example, will send an interrupt request whenever a key is pressed, released or held down for a period of time. The keyboard software can then read something called a scancode to determine which key was pressed. The scancode is often a single byte which uniquely identifies all the keys and whether the shift key was pressed. The scancode usually  consist of bits denoting the row of the key and bits denoting the column of the key. Given a full sized keyboard has around 6 rows an 21 columns (including the numpad and home group) the total number of keys (without shift) is 104. Double that for the shift key and you get 208, which easily fits inside the range of a single byte (0-255). The scancodes are generally mapped by software to a kernel keycode representation and the modifier keys are kept track of. Those keycodes can then be translated into character codes (taking into account the modifier keys).

Thus far the function of the keyboard and mouse have been described as per what PS/2 would do. An observant reader will realise that many keyboards have additional features (for example, volume control). Clearly such a function goes outside the normal range of a PS/2 keyboard. Such keyboards will be USB keyboards. While their main functionality can be emulated to PS/2, additional functions cannot. Furthermore, the functions may be non-standard, in which case a USb driver specifically for the particular device will be required.

The key part of a mouse is clearly its 2D movement function. A mouse generally sends a movement as an x/y vector indicating how much the mouse was moved by. It does this at a high frequency meaning even small movements can be detected. 

Mice have DPI (Dots per Inch) values which can be configured in two places - the mouse itself (often this value is fixed) and in software. Software (often the mouse driver) can scale the value received from the mouse as it chooses. A higher DPI means the mouse will indicate a greater distance moved for the same physical movement. 

To draw a cursor on the screen requires a moderately complex graphics driver capable of handling at least two rendering layers (also known as buffers) which can exist in hardware or software. One layer holds the normal, unmodified screen image. The upper-most layer holds the cursor image which consists of a transparent (or solid-colour depending on the blending mode) layer along with the cursor itself. The two layers are composited to produce the final image for the screen. When the cursor moves, the graphics driver's cursor layer is updated without needing to redraw the entire rest of the screen.

## USB HID

## Compatibility

---

# FAQ & Common Problems

## How do I display a mouse cursor?
Write a graphics driver. Please read the Displays and Graphics article (particularly the FAQ section. If you had to read this FAQ for how to display a cursor, you probably need to read the FAQ in Displays and Graphics).

## How do I support my mouse's special buttons?
Find the specification for your specific mouse's hardware then read it and work out what to do. No spec? Tough luck. You might be able to ask the manufacturer or design company, but the reason there isn't a spec is usually because they want to keep their designs secret. Failing that, try online forums, see if you can find someone else who's written a driver for your device and ask if you can port it.

## How do I support my keyboard's special keys?
Depends on which keys you mean. The normal function keys and such like should be part of the standard keyboard mapping. Extra keys (such as macro keys on gaming keyboards) may well require specialist USB drivers. In which case you need the spec for the keyboard. No spec? Touch luck. You might be able to ask the manufacturer or design company, but the reason there isn't a spec is usually because they want to keep their designs secret. Failing that, try online forums, see if you can find someone else who's written a driver for your device and ask if you can port it.

## How do I support my keyboard's special lights and whizzy things?
Caps lock, num lock and scroll lock lights are easy to support (even PS/2 emulation supports them). Just read the PS/2 keyboard article for basic examples. Supporting any additional lights (or, for example, controlling keyboard backlight) will usually require special USB drivers. In some cases, the keyboard is a totally separate device (when examined on the PCI bus) from the backlighting device. Either way, you will need the spec for the specific device.  No spec? Tough luck. You might be able to ask the manufacturer or design company, but the reason there isn't a spec is usually because they want to keep their designs secret.  Failing that, try online forums, see if you can find someone else who's written a driver for your device and ask if you can port it.

---

# References

* [Hexus.net - Human Input Devices](http://hexus.net/tech/tech-explained/peripherals/18459-human-input-devices-hids/)
* [Wikipedia - PS/2 Port](https://en.wikipedia.org/wiki/PS/2_port)
* [Wikipedia - Human Interface Device](https://en.wikipedia.org/wiki/Human_interface_device)
* [USB Human Interface Device class](https://en.wikipedia.org/wiki/USB_human_interface_device_class)
* [MSDN - HID Architecture](https://msdn.microsoft.com/en-us/library/windows/hardware/jj126193(v=vs.85).aspx)
* [USB.org - HID](http://www.usb.org/developers/hidpage/)
* [Techopedia - Human Interface Device](http://www.techopedia.com/definition/19781/human-interface-device-hid)
* [HowStuffWorks.com - How computer mice work](http://computer.howstuffworks.com/mouse.htm)
* [HowStuffWorks.com - How computer keyboards work](http://computer.howstuffworks.com/keyboard.htm)
* [Youtube.com - What inside a keyboard and How a keyboard works? by Surrounding Science](https://www.youtube.com/watch?v=chSzoovWtzU)

*[acronym]: details
---
layout: reference-article
title: Timers
date: 2015-09-06 11:50:00
categories: [ docs, reference ]
description: A description of timer devices (interval and watchdog), their operation and uses.
---

# Introduction

Timers are a core part of every computer (embedded or otherwise) in the world. Timers drive everything from the clock signals for the processor and memory to the pulse width modulation systems for motor controllers. As such, understanding the various types of timer device, common modes of operation and their uses is vital for all operating system and embedded development.

## Scope of this article

This article looks at timers in general, common modes of operation and how timers can be used by operating system software. Many useful links are provided for specific devices at the bottom of this article (including the Intel Programmable Interval Timer (8253/8254/8254-2), MIPS JZ4780 Timers and Operating System Timer and ARM mbed watchdog timer). 

---

# Overview

## What are timers?
Timers (in some cases, referred to as clocks) are devices which use an oscillator (external or internal) as a frequency input and can generate an output signal relative to that frequency. Timers are often connected to counter registers and interrupt pins which allow measuring of passing time and periodic events to be performed. 

## What are interval timers?
An interval timer is a timer device specifically designed to generate a signal at a specific interval. The interval is usually specified by a divider of the input frequency (pre-scaler) and a counter value. The counter value is decremented for each complete cycle of the input frequency (after division by the pre-scaler). Once the counter hits zero, the output signal is pulsed or held at the opposite logic level (depending on the operating mode). The counter may or may not be automatically refilled by hardware, resulting a repeated pulse generation known as rate generation. Pulse signals can be connected to interrupt lines to create a periodic event signal that is often used for scheduling or similar tasks. Operating modes where the logic level only flips each cycle (as opposed to a sudden pulse) can be used for pulse-width modulation (PWM) and other applications of square waves.

## What are watchdog timers (WDTs)?
A watchdog timer is a timer device designed to detect lock ups and errors in a system. Like interval timers, watchdog timers are loaded with a counter value and pre-scaler which they count down from. However, under normal operation, they are regularly reset by software, indicating the system is still operational. Thus the watchdog timer (assuming all is working correctly) should never hit zero. In the event of a system lock up (e.g. infinite loop) or hardware fault, the watchdog timer will hit zero, generating an output signal. There are several recovery actions that may be hard-wired into the system such as signalling a non-maskable interrupt (NMI) or reset the processor.

## What is a phase-locked loop (PLL)?
A phase-locked loop, PLL for short, is an add-on circuit for generating an output signal that is very accurately tied to an input frequency. The output can be used for synchronisation or it can be a high multiple of the input. The latter case allows a PLL to be used for demodulation, clock signal distribution and frequency synthesis.

## What is an interval timer used for?

Interval timers have a wide range of uses (which may or may not also require a PLL in the circuit). The following list briefly summarises the major uses of an interval timer:

- Processor clock cycle - The output of an interval timer is often linked to a PLL to create a higher frequency internal clock signal allowing processor circuitry to operate at a higher frequency than external circuitry.
- Real-time clock (RTC) - The output of an interval timer can be linked to a (usually 64-bit) counter register which can be used to keep track of real world time reasonably accurately (often with periodic synchronisation to an external source)
- PWM - Pulse-width modulation is a huge application of interval timers which generate square wave outputs. For example, PWM is used motor speed and power control.
- Event-driver - A pulse signal from a timer (either one-shot or periodic) can be used to drive events such as delays, scheduling, time-out conditions and many more.
- (PC) speaker driver - One specific application is one of the square wave outputs of the Intel PIT is (until very recently) always connected to the PC Speaker allowing rudimentary tones to be outputted.

## What is a watchdog timer used for?

A watchdog timer can be used for a range of error and recovery systems. For example:

- Software lock-up recovery
- Hardware fault detection 
- Hard-reset (of a system or processor)

## How do interval timers operate?

Most interval timers operate in a similar way, using the following steps:

1. Setup - The timer is, if necessary, told its input signal source; given a pre-scaler value (which acts as a divider of the input frequency); told its operating mode (e.g. rate generation, square wave generation) and finally enabled.
2. Reload - The timer is given an initial counter value (which can be used by hardware for automatic reload if the operating mode requires).
3. Poll or interrupt - The timer decrements the counter value for each complete cycle of the scaled input frequency. When the counter hits zero it pulses or (logically) inverts the output signal.
4. End or reload - At this stage what happens depends on the operating mode. Some modes will reload the counter value with its initial value and repeat (creating a periodic pulse or square wave). Some modes will just stop there. Others may allow the counter value to overflow to its maximum (unsigned) value.

## How do watchdog timers operate?
  
Most watchdog timers operate along the following lines:

- Initialisation / restart - The timer is configured with an input frequency, pre-scaler and counter. It may also be told what inputs to treat as counter resets. Finally, the timer is enabled and an interrupt may be enabled (if desired/available). 
- Reset - If the system is operational the watchdog timer should be sent a reset signal regularly to prevent it reaching any of its error conditions/states.
- Time-out (Single or multi-stage) - If the system fails to reset the counter value, it will go below a threshold (or just hit zero). One or multiple stages of time-out may be supported.
- Corrective action(s) - Once a time-out stage is reached, a recovery action is triggered. In multi-stage systems, these will become progressively extreme corrections to try and restore system operation. On x86 systems, the non-maskable interrupt or SMI interrupt is triggered when the watchdog timer expires.

Note that it is impossible t recover properly if any of the core hardware (including the watchdog timer itself) is damaged. The watchdog timer is best suited to software bug recovery (such as accidental infinite loops) and to one-time hardware fault recovery (such as cosmic rays causing bit flips). f the watchdog timer itself id damaged then the system may just continuously crash/be reset.

## How are phase-locked loops (PLLs) used for timers?

Phase-locked loops are used in conjunction with (interval) timers to:

- Multiply primary clock frequency - PLLs can produce much higher output frequencies from a low input frequency. The higher output frequency can be used to drive internal circuitry so it can run faster than external circuits.
- Multiple-signals / distribution - PLLs can be used to drive clock signal distribution circuits (which split the signal to several devices) because they isolate the clock signal input from the output and eliminate any additional drift that might be caused by external device feedback.

---

# Hardware

## Overview

Timer devices can be considered as black boxes. The section below deal briefly with what is inside and outside of the "black box" timer device. From an OS/embedded programmer's perspective, understanding the physics of the hardware is probably unnecessary but knowing what standard outward-facing features are available is very important.

## Details : Internals

Within a timer is generally two parts: an oscillator (such as a quartz crystal) and a control circuit. The oscillator is known as an inverse piezoelectric device because when a voltage is applied across the quartz crystal oscillator, it physically distorts (that's the mechanical part - piezo is Greek meaning to squeeze or press). When the external voltage is switched off, the crystal goes back to its original state. As it does so, it generates a voltage (this is the electric part). This (comparatively) small voltage is then fed through an amplifier and then re-applied to the crystal. This forms a feedback circuit which behaves like an RLC (resistor-capacitor-inductor) circuit with a specific resonant frequency. 

Getting the oscillation started is done by the control circuit. It applies a voltage which puts the oscillator in an unstable state which creates noise in the feedback loop which rapidly turns into a pure feedback signal with the dominant frequency being the resonant frequency.

Quartz crystal oscillators are preferred because they are not greatly affected by temperature, which with ever smaller devices and international sales, is very important. However, because the oscillator relies on the properties of the specific quartz crystal, not only is the frequency generated fixed but it will always be slightly different. There is a margin of error in every device. 

To combat the variations between devices, timers now include special registers that configure the control circuitry so it can try to counteract the variation. This is usually configured by the manufacturer but it may be necessary for the OS to reconfigure occasionally (which is a complex process).
  
## Details : Externals

Timers generally use oscillators with relatively high resonant frequencies. This means that the raw signal from a timer device is probably too fast for any uses an OS or embedded developer may need it for (except driving the processor and/or other high speed circuits). 

To reduce the frequency of the raw signal, timers include two registers which contain the pre-scaler and the counter (a.k.a reload) values. The pre-scaler value is a divider which divides the input frequency of the timer thus slowing it down. The counter value is used to determine when the output signal should transition. 

The counter value is decremented each time the (pre-)scaled input signal completes a cycle. When the counter value hits zero, the logic level of the output is either pulsed or (logically) inverted, depending on the operation mode of the timer. The operation mode is configured before the timer is enabled (and usually before the counter register is loaded) via a command register. 

Some timers also include separate compare and counter registers, in which case the previous counter register is actually known as a reload register. The (new) counter register increments on each pre-scaled input cycle. When it matches the value in the compare register, the output logic level transitions (according to the operation mode). Depending on the operation mode, the counter register may reset or it may continue to count up and overflow back to zero.
    
## Details : Operation

The following basic modes exist in most timers:

- Overflow (interrupt) - This is a signal (sometimes fed to an interrupt) which occurs when the counter register overflow.
- One-shot trigger - This is a signal which pulses when a count value hits zero. The value is not reloaded hence why this a one-shot trigger.
- Rate generation - This is a signal which pulses when a count value hits zero. However, unlike one-shot triggers, the count value is reloaded to its initial value and the process repeats indefinitely. This is often used as a signal to a real time clock or interrupt pin (for use as a scheduler interrupt or similar).
- Square wave generation - This is a signal where the output logic level inverts each time a comparison value matches the internal counter or a separate counter value reaches zero. The separate counter value is automatically reloaded. The process is periodic and occurs indefinitely thus creating a square wave output. This can be used for PWM.

---

# Software

## Overview

A timer signal has many uses in OS and embedded development. The following sections briefly outline the most common uses.

## Polling for transition
The most basic way to use a timer is just to enable it and inspect its internal counter register. By looping endlessly you can monitor the counter and wait for it to go past a certain point. This can be used as a crude delay loop. However, polling the counter like this is costly for performance (since it blocks the executing thread), for efficiency (since a continuous loops keeps the processor very active and involves lots of I/O meaning high power consumption) and can prevent the rest of the system operating properly (because spin loops consume processing time which reduces the time slices available for other threads/processes). 

## Interrupt on transition
The standard way to use a timer is in interrupt on transition mode. This is where the pulse (one-shot or rate generator) output of a timer device is connected to an interrupt request pin. This allows periodic interruption of the processor which can be used for efficient sleeps and for scheduling (amongst other less common things). 

To enable timer interrupts generally requires three levels of configuration. The processor's global interrupts enable flag must be set/clear as appropriate, the individual IRQ Enable/Disable flag must be set/clear as appropriate in the Interrupt Controller mask (PIC on x86, Coprocessor 0 on MIPS) and lastly the interrupt must be enabled/configured in the timer.

## Processor Clock Signal
Timers always drive the processor's clock signal, which is what allows it to operate at all. It is likely that the input timer signal is fed through a PLL circuit for the processor since most internal processor circuitry operates at a much higher frequency than external circuits. Modern processors usually provide registers to control the processor's clock frequency which allows it to be over or under clocked. Over clocking is dangerous but under clocking is common place. Under clocking, particularly in mobile and embedded devices (including laptops, phones and IoT devices) allows the processor to run slower and therefore consume less power which massively improves battery performance. 

For laptops and PCs, under-clocking occurs frequently if the processor begins to overheat. The CPU (and/or GPU) is throttled back to consume less power and therefore produce less heat and thus give it the chance to cool off before it melts (but without requiring a total system shutdown). Older processors used to just cut-out entirely if they overheated which caused lots of issues with file corruption, particularly in summer!

## Real-time Clock
Timer signals are often fed to separate real-time clock devices which are accurate counter-only devices which keep track of the real-world time. These devices can sometimes generate interrupts using comparison values (e.g. to generate an interrupt at a particular time of day). The OS usually has to synchronise a real-time clock to an external source (by human input or, more commonly, via the internet. Embedded devices often use radio signals which carry a local-region time signal). Once initialised, the counter value can be inspected at any time to obtain the time. 

The counter value may be stored in various different formats. Common options are a 64-bit value - specifying the number of nano or milliseconds since a chosen date - or separate seconds, minutes, hours, weekday, day of month, month, year and century values (as is the case with the x86 RTC CMOS).

## PWM
Timer signals are often used to generate a square wave which can be used for pulse-width modulation (though embedded systems may have separate PWM modules which allow rectangular waves to be generated). PWM is used for anything where an analogue-like signal can be achieved by simply varying how long power is supplied for. For example, if you switch a DC motor on and off very quickly, the relative proportions of the on/off periods determine the proportion of the maximum power (or sometimes speed) of the motor. A similar effect is often used as a "Hello, world!" test producing a fading an LED driven by an embedded processor.

## Event driver
The timer can generically be used to attach to external devices or as an interrupt to drive external or internal events. For example, you may want to provide a clock signal to an external device or to set a one-time delay event that the processor can detect when it receives the special interrupt.

## Scheduling
A rate generator signal from a timer fed to a processor in the form of an interrupt is often used for scheduling. More specifically, this kind of multi-tasking is known as pre-emptive multi-tasking because the timer interrupt pre-empts the execution of the next program instruction and stops it. The kernel then has the opportunity to switch execution to a different thread by changing the return point to that of a different thread. By switching threads very quickly, the kernel can make it appear as though all the threads are running simultaneously. 

With multiple processors (or cores), pre-emptive multi-tasking becomes pre-emptive multi-processing, where two or more threads can actually be running simultaneously (on separate cores/processors). This makes the scheduling and synchronisation problems a little trickier at the low level but generally improves performance (assuming programmers designed their programs to make proper use of threading and asynchronous design).

## (PC) Speaker
In PCs, the third timer output of the PIT was often connected to a speaker fitted to the motherboard. This allowed the kernel (or other software) to output a very electronic-sounding tone but became the infamous PC Beep (which drove librarians everywhere, totally crazy when people started their laptops up un-muted). 

The PC speaker is a very simple device to configure which relies only on I/O ports (with no RAM, ACPI or processor-config dependency). As such, it has long been one of the first devices initialised by the BIOS and many OS'es. It's ease of use and reliability meant it could be used to output error codes as tones or patterns of bleeps before even the display was initialised (which relied on RAM).

Sadly, many new laptops and PCs do not include a PC speaker on the motherboard, so there goes one of the most fun outputs and the easiest output for hobbyist OS developers. We'd better start heading to the embedded (Internet of Things) world is we want little annoying bleeps and buzzes... 

---

# FAQ & Common Problems

## No interrupt
If you're not receiving an interrupt when you expect one then there are several possibilities:

1. You haven't enabled interrupts correctly - most systems have three places interrupts must be enabled: A global interrupts enable flag, an individual interrupt enable/disable mask in the interrupt controller and one or more interrupt enable flags in the timer device itself.
2. You haven't configured the timer operating mode correctly - not all modes generate an interrupt signal.
3. You haven't enabled the timer output
4. You haven't set a reload value
5. You set a pre-scale and counter value incorrectly - It is possible you chose values which take much longer to expire than you expected. 

## Only one interrupt
If you're receiving an interrupt only once, there are several possibilities:

1. You have picked an operating mode that requires software reload and you aren't reloading the counter value (in the timer)
2. You aren't acknowledging the interrupt request properly so the interrupt controller is blocking other requests
3. You've performed a software reload but the timer needs to be re-enabled.

## Incorrect frequency
If you're timer appears to be outputting an incorrect frequency then:

- For large differences, you probably miscalculated or misconfigured the pre-scale and counter (/reload) values
- For small differences, be aware that because of the integer nature of pre-scalers and counters, and slight variations in the timer hardware itself, you cannot expect to achieve an exact frequency. Most generated frequencies will be what you intended plus a few decimal places of difference. It all depends on whether your intended frequency is an exact factor of the timer's input frequency or not.

## Interrupt skipping
If you're receiving interrupts but the software appears to be skipping/missing a few then your software is probably taking longer to run than the period of the timer resulting in the timer IRQs being buffered and then overflowing so some are lost. Either improve your interrupt handler (by optimising it or rethinking your design so it does less work) or extend the period (lower the frequency) of the timer output.

---

# Further Reading

## General reading

### Watchdog timers

- [Ganssle.com - Watchdogs](http://www.ganssle.com/watchdogs.htm)
- [Wikipedia.org - Watchdog Timer](https://en.wikipedia.org/wiki/Watchdog_timer)
- [Embedded.com - Introduction to watchdog timers](http://www.embedded.com/electronics-blogs/beginner-s-corner/4023849/Introduction-to-Watchdog-Timers)

### Interval timers

- [ScriptoriumDesigns.com - Timers](http://www.scriptoriumdesigns.com/embedded/timers.php)

### Phase-locked loops (PLLs)

- [Wikipedia.org - Phase-locked loop](https://en.wikipedia.org/wiki/Phase-locked_loop)
- [Freescale.com - Phase-locked loop design fundamentals](http://www.freescale.com/files/rf_if/doc/app_note/AN535.pdf)
- [CircuitsToday.com - PLLs](http://www.circuitstoday.com/pll-phase-locked-loops)

## Specific hardware

### Watchdog timers 

- [mbed.org - mbed Watchdog Timer](https://developer.mbed.org/cookbook/WatchDog-Timer)
- [ImgTec.com - JZ4780 Programmer's Manual](http://mipscreator.imgtec.com/CI20/hardware/soc/JZ4780_PM.pdf) (JZ4780 is the Creator CI20 processor. Manual contains good information about OS Timer and other Timers/Counters)

### Interval timers

- [ImgTec.com - JZ4780 Programmer's Manual](http://mipscreator.imgtec.com/CI20/hardware/soc/JZ4780_PM.pdf) (JZ4780 is the Creator CI20 processor. Manual contains good information about OS Timer and other Timers/Counters)

#### x86/Intel Programmable Interval Timer

*The following list is entirely PC/Intel focused since they are the most likely devices for an OS dev'er to be dealing with. For embedded devices, you will need the programmer's manual or data sheet for the specific SoC you are dealing with.*

- [Wikipedia.org - Programmable Interval Timer](https://en.wikipedia.org/wiki/Programmable_interval_timer)
- [Wikipedia.org - High Precision Event Timer](https://en.wikipedia.org/wiki/High_Precision_Event_Timer)
- [Wikipedia.org - Real-time Clock](https://en.wikipedia.org/wiki/Real-time_clock)

- [OSDev.org - Programmable Interval Timer](http://wiki.osdev.org/Programmable_Interval_Timer)
- [OSDev.org - RTC](http://wiki.osdev.org/RTC)
- [OSDev.org - CMOS](http://wiki.osdev.org/CMOS)
- [OSDev.org - Time and Date](http://wiki.osdev.org/Time_And_Date)
- [OSdev.org - PC Speaker](http://wiki.osdev.org/PC_Speaker)

- [ScriptoriumDesigns.com - Timers](http://www.scriptoriumdesigns.com/embedded/timers.php)
- [Intel.com - 8254/82C54: Introduction to Programmable Interval Timer](http://www.intel.com/design/archives/periphrl/docs/7203.htm)
- [Stanford.edu - Programmable Interval Timer (8254)](http://www.scs.stanford.edu/10wi-cs140/pintos/specs/8254.pdf)
- [BrokenThorn.com - PIT](http://www.brokenthorn.com/Resources/OSDevPit.html)
- [Slideshare.net - Vijay Kumar - PIT](http://www.slideshare.net/VijayKumar486/8254-programmable-interval-timer-by-vijay)

*[PLL]: Phase-locked loop
*[PLLs]: Phase-locked loops
*[PWM]: Pulse-width modulation

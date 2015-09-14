---
layout: reference-article
title: Typical Design Routes
date: 2015-09-02 12:04:00
categories: [ docs, reference ]
parent_name: Getting Started
description: Describes typical design routes taken by new OS developers.
---

# Introduction

When you first start programming your operating system you're going to have lots of choices, mostly about what to spend time doing yourself and what to use pre-built (and some bits to leave out entirely). To help you make those decisions, it is helpful to know some of the typical design routes and paradigms that people take.

## Scope of this article

This article outlines some of the extreme versions of each design route and paradigm to make clear the possible design routes. Most of the time, the actual design route taken will be (and should be) a combination of all of them. In the end, a full operating system will have to support all the features of all the design routes. 

---

# First steps / Getting Started

The first steps you should take must be well guided to ensure that you have a stable, solid and correct set up. The first few bits of code you write for you OS will be very prescribed because there is a basic set of stuff every OS has to do initially. The possible variations are fairly limited.

To get started, please read the Getting Started article and follow the video tutorial series produced by FlingOS.

The most core functions you will want are for printing strings (to the screen or serial/UART), interrupt and exception handlers and lastly, heap management functions (alloc and free). 

---

*The following titles loosely describe the type of developer who would take the described design route.*

***Please note that some of the design routes are highlighting ones which people try to take but which are annoying for other OS developers or simply do not work in practice.***

---

# Feature Design Routes
This section describes the typical design routes for what features are implemented in what order.

## Command Line Lover
Command Line Lovers are people who implement everything as commands through a console. They start by just generating screen output for text (or perhaps just a serial port) and then accepting keyboard or serial input. Once they've got their shell working they head straight to the file system so they can save their text somewhere. All they need is a text editor and file system driver.

Beware though, command line lovers often implement things without abstraction, without mutli-tasking and with high-levels of interdependency. They risk forgetting that later they'll need to support threads, multiple stacks, memory allocation and the rest and end up with a DOS-like mono-tasking system that can't be extended.

Development route:

0. Getting Started stuff
1. Console output to screen or serial
2. Console input from keyboard or serial
3. Console with buffers to allow history, scrolling, auto-complete and erasing. COnsole should not include actual command handling.
4. Shell to allow commands. Shell should be abstracted so it can use a screen or serial console without caring which.
5. PATA/PATAPI driver for reading a disk
6. FAT32 driver for reading/writing file system on a disk
7. FAT File Streams driver for reading/writing files in a FAT32 file system
8. Commands in Shell for handling files
9. Multi-tasking / multi-threading to allow loading and starting of programs

## Ping-pong Pirate
Ping-pong Pirates love being able to send network pings to and from their machine. They probably don't even have a command line interface. They start by implementing powerful multithreading and follow it up with a full network (IP/TCP/UDP/etc.) stack. By the end of it they have basically built a router, but they don't mind so long as it can respond to pings faster than any other machine on the network.

Development route:

0. Getting Started stuff
1. Multi-tasking / multi-threading base layer of software on which drivers sit
2. PCI driver for device discovery
3. Network card driver for specific network card (i.e. your specific hardware or specific virtual hardware)
4. Internet Protocol (IP) driver 
5. TCP driver or UDP driver
6. What other protocols do you want to support? Implement drivers for those.
7. HTTP driver? Start making some web requests?

## WYSIWYG Wonder
What You See Is What You Get people develop a system with a full graphics stack and wonderful UI but their program icons, those are just dummy test data. They don't have proper multitasking or program loading but that doesn't matter, so long as it looks cool. 

Development route:

0. Getting Started stuff
1. Multi-tasking / multi-threading base layer of software on which drivers sit
2. Display driver (for whatever specific hardware / virtual hardware you are working on)
3. Graphics driver (may or may not make use of graphics card. Graphics processing on CPU is slow but possible)
4. Basic program for generating /using graphics (entirely software generated imagery)
5. PATA/PATAPI driver for reading a disk
6. FAT32 driver for reading/writing file system on a disk
7. FAT File Streams driver for reading/writing files in a FAT32 file system
8. Image file library for loading image files through file streams
9. And so on and so forth for more graphics technologies.
10. At some point, PS/2 mouse and keyboard drivers to receive user input
11. Basic game program
12. GUI libraries / drivers / software stack

After implementing file system drivers you will then have what you need to load programs from disk so you may want to stop including demo programs as a part of the OS and spend some time looking at loading programs from disk.

## Multi-tasking Master
Multi-tasking Masters go way further than Ping-pong pirates. So far that they forget the network stack entirely but implement everything required for multi-tasking even for situations that are never likely to arise. They have a system which evolves a lot from year to year but really only outputs bunches of seemingly random data that they say proves all their features work. 

Development route:

0. Getting Started stuff
1. Setup threads and scheduler
2. Setup processes (optional)
3. Synchronisation primitives
4. User-mode threads/processes
5. System calls
6. Virtual memory management
7. Shared memory
8. Inter-process Communication (IPC) 

## Bilingual Beauty
Bilingual Beauties are people who don't see why C is still the language of choice for OS and low-level programming. In extreme cases, they don't think any language is good enough! They spend as much time designing their new, better language (and maybe compiler for it) as they do on OS development. 

It has been said about alternative-language people that their ultimate goal is to have a language so powerful that they don't even have to program in it - the compiler will generate the entire OS from a single space. They risk falling into the belief that if their language is powerful enough, the OS will simply follow without any work. 

Development route:

0. Getting Started stuff
1. Choose a language or design your own
2. Oh, you got this far? Well then, use an existing compiler or write your own
3. Either decide your language of choice isn't good enough and go back to step 1 or continue with a different design route

## Standard Bearer
Standard Bearers read, learn ,understand and stick to standards all the time. They know exactly the way things should be done and if there isn't already a standard for it, you can bet they're already writing one. Their code for the standards they know about is beautiful and fits together perfectly (except for bits where the standards are poor, kludged or incompatible but standard bearers will never show you those bits). 

Unfortunately, their supporting code for standards they haven't studied yet is poor and the system as a whole will probably lack useful features. At the end of the day they end up with yet another clone OS that has no personality other than "boring" because standard bearers won't include fancy bits or neat quirks. 

Development route:

0. Getting Started stuff
1. Pick one of the other design routes
2. Start implementing the other design route, researching, learning and implementing the proper standards along with way.
3. Can't find a standard for what you want to do? You're going to want to write one...aren't you?

---

# Implementation Methods
This section describes the typical approach to implementing OS features. 

## Sensible Searcher
This is the ideal OS developer; one who just uses existing online and offline resources to find all the information they need. They never ask on online forums because they know their question has already been answered somewhere, they just need to find out where. 

Sadly though, not everything can be found online. Answers in forums get buried or links become broken. Documentation is often poor and rarely provides satisfactory explanations. Eventually, most OS developers need to ask a question or two to have something explained to them.
 
## Crazy Copycat / Botch Job Bob
Crazy Copycats and Botch Job Bobs just copy code from anywhere and everywhere, linking it all together by asking Stack Overflow questions or botching some code for themselves. Their system is built entirely off other people's code and will never progress past what others have already made available online.

Developers like this are a menace in the open source community. They often ignore licenses and frequently ask already-answered, trivial or time-wasting questions because they don't understand what they're doing and don't want to spend time programming for themselves. 

It is a struggle to understand why crazy copycat people exist in the tough world of OS dev because ultimately they learn very little, build a system which isn't very useful and don't impress anyone. For genuinely technical people, they are obvious to spot. OS dev to non-technical people is generally unintelligible and, as a result, unimpressive so copy-cat developers achieve no tangible result.

We strongly advises that you check and maintain licenses and do plenty of online research prior to asking a question in an online forum. It will save other developers valuable time, reduces the number of online threads (thus reducing the amount of information which gets buried) and prevents you from receiving negative responses. When posting a question, always remember to include what you have tried and what places you have already researched so that people realise you've put effort in.

## DIFM Drain
The Do It For Me Resource-Drain people are probably the worst. They don't even copy other people's code, they just head straight for the forums asking for others to implement everything for them. They generally don't understand what they are doing and they waste a lot of other people's time. 

We advise that if you feel yourself hitting a DIFM way of thinking, you switch to just developing stuff based off Linux or a different embedded OS. At least then you can just include the bits of the OS you don't want to do and write the rest yourself. Whatever you do, don't ask other people online to do the work for you. (Unless of course you're willing to pay them - in which case you're employing people to help with your OS. Please only post job offers in appropriate online forums).

## Paper Pusher
Paper Pushers are not necessarily unintelligent individuals. They are people who spend most of their time designing and redesigning things on paper. Designs which are perfect and, in theory, solve all the existing OS problems. Unfortunately, people like this often forget to actually implement anything (or decide that no current hardware does what they require). As a result, they have no working result and if you did try to implement their designs, you'd probably find many practical difficulties and flaws.

## Realistic Rob
Realistic Robs are people who implement everything in assembly code running under x86 Real-mode. They like 16-bit and limited RAM and don't see the need for higher level languages, memory protection or any other "new fangled" features. To the rest of the world, these people are not realistic at all. 

## Micro Max
People who implement micro-kernels. Many, many, many OS developers fit into this category.

---

# Tools and SDKs
This section describes the typical sets of tools people use for implementation.

## Animal Antics
People who use the GNU toolchain or one of its derivatives and generally end up with systems that include all sorts of weird and obscure names.

## Visual Smasual
People who use Visual Studio and spend a lot of time getting MSBuild to do what they want. They are often chastised and referred to as "not proper developers" because they don't like VIM and they actually use a mouse. They are frequently criticised for not using Linux, being told that "everything would be much easier" by Linux or Mac developers (who forget how bad Linux's debugging and IDE support is).

## Body-part Bella
People who develop for ARM platforms.

## Imaginative Ian
People who develop for MIPS platforms (at the time of writing MIPS was owned by Imagination Technologies).

## Virtual Vicky
People who only develop for virtual machines and emulators such as VirtualBox, Qemu and VMWare.

---

# FAQ & Common Problems

## Can't decide which paradigm you are?
Try working out what you want your OS to do (this is described as your "realistic aim" in the Getting Started article). Work out what needs to be implemented to get to that aim. Then match the feature set you require with one of the paradigms above.

If you can't decide which implementation route to go with, try re-reading. There is only one which can be considered good. 

If you can't decide which toolchain to use, pick the one you are most familiar with or that is recommended by others online for your design route / target platform. Try it for a while and if it doesn't suit you, you can always try a different one.

---

# Further Reading

- [OSDev.org - What order should I make things in](http://wiki.osdev.org/What_order_should_I_make_things_in) and associated articles for each paradigm.

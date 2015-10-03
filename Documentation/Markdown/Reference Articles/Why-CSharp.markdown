---
layout: reference-article
title: Why teach in C#?
date: 2015-09-18 12:36:00
categories: [ docs, reference ]
description: We explain why we use C# instead of C and why it's not as insane as you might think.
order: 1
---

# Introduction

FlingOS™ is a free, open-source, industry and university supported project to provide learning resources for students and developers to learn OS and low-level development. The project consists of articles, tutorial videos and a sample code base. The articles are designed to consist of comprehensive background information and technical details. The tutorial videos take developers from general programming to an understanding of computer architecture and OS development. However, all of it links to the FlingOS code base, which is primarily a sample kernel written in C#.

In this article we will discuss the unusual decision to use C# to teach OS and low-level development and some of its advantages and disadvantages. 

# Background

In an environment dominated by C and developers, who didn't grow up with object-oriented, high-level and web development as the norm, it can be hard for many current low-level developers to understand a high-level developer's perspective. This has led to a narrow minded, closed attitude in a community which is in increasing need of developers.

Many high-level developers, particularly students, have been taught on the back of huge levels of abstraction and constructs. This makes shifting from high-level languages down to C or C++ remarkably difficult - harder than going the other way! 

Couple the differences between language and design, with the fact that at the low-level little can be taken for granted, and you end up with a huge barrier to entry for low-level development. However, the Internet of Things is turning the tide on development emphasis, with ever growing interest in embedded and low-level development.

At this crucial moment, just before low-level dev explodes and learning resources come back into demand, FlingOS is preparing to ease the transition from high to low level.

# Important Note

It is important for the reader to realise that at no stage are we proposing that C# is a good, correct or even the best language to implement an OS (or driver) in. Nor is FlingOS aiming to be a real OS rivalling the likes of Linux or Windows. As such, to think or say "but C# isn't appropriate for low-level dev" is to totally miss the point. The whole point of using C# is as a learning platform. A stepping stone from the familiar to the unfamiliar.

# Secondary Issue

There is, of course, a secondary issue which FlingOS tackles. This is that the majority of high-level developers do not understand any of the low-level software (let alone hardware). This is leading to an increasingly insecure, unstable hierarchy where high-level developers simply don't know what the impact of their code actually is.

Only earlier this year, at the Black Hat conference, did people begin to take notice of this issue. Specifically, many high-level developers have been using custom-built or standard framework components for implementing cryptography.

Even more specifically, many developers have implemented their own pseudo random number generators or just used a bog-standard framework one. Neither is appropriate for use in a secure system since neither has proper levels of entropy and entropy management built-in. Had more high-level developers understood the low level software, they would have been aware of the built-in functions for generating cryptographically appropriate pseudo random numbers. At the very least an understanding of the hardware and some extra maths would have made the flaw obvious to many.

FlingOS aims to tackle this by providing a platform on which people can learn OS and low-level concepts, without having to learn an entire new language or framework; It will open up a world of understanding.

# Barriers to entry
As has been mentioned, there are a significant number of barriers to entry for high-level developers who wish to start developing at the OS or low-level. These can be summarised as follows:

- Language differences
- Design/engineering differences
- Framework/support differences
- Theoretical and fundamental challenges
- Availability of tools
- Availability of information

## Language Differences

One of the biggest issues when attempting to transition from high to low level development is the difference in language. While many high-level languages (e.g. C#, Java, JavaScript) use a C-based syntax, they have evolved a long way from modern day C. C is the de-facto language for low-level development, particularly for new students.

So while C retains some of the familiar constructs (loops, variables, conditional blocks and so on) it doesn't have many of the key features that high level developers rely upon (e.g. namespaces, classes, declaring variables at any point in a method and so on). There are sufficient, small differences that trying to program in C is more than difficult - for many, it is a big enough challenge in its own right.

Thus to enable high-level developers to begin working in the low-level, the language used must be made more familiar. For both students and professionals alike, being asked to learn a new language as a necessity to learning all the other concepts poses a significant barrier.

## Design/engineering differences

While many design and engineering differences (such as use of static and/or global variables, lack of classes and namespaces, manual memory management) could be considered language differences, they are better thought of as software engineering differences.

Basic stuff like how to structure a program in C, use of header files, different meaning of the static keyword, use of declarations and importance of order of declaration, make programming in C a significant challenge for high-level developers. It requires developers to learn a whole new way of thinking about software structure and implementation. Ultimately, it is a level of unfamiliarity and difficulty that overshadows learning the concepts and theory of low-level development.

Unfortunately, of the many low-level developers I have met, most who started out in C do not seem to be able to understand this challenge. It appears that going from low to high (for example, C to C#) is much easier than high to low. This results in a very negative, dismissive and even hostile environment for high level developers who are attempting to learn low level development. This is something which will only change with time and only if more projects like FlingOS encourage high-level developers to take an interest.  

## Framework/support differences

For those who are low-level developers, the following will seem obvious: There is little to no framework support at the OS or low-level.

While standard libraries do exist, they don't include things like automatic memory management (such as garbage collection), nor easy file handling, or lambda function processing. At the low-level it becomes necessary to do a lot of the work manually. An especially important feature at the high-level, that is often used as part of standard execution, is exception handling. At a low-level, try-catch-finally blocks simply don't exist (even if you use C++ you still need a special library and hooks to make it work).

For example, at a low level, the concept of a generic List doesn't really exist. You either use an array or use a linked-list. If you want to grow an array, you have to do so manually. This means many high-level developers are left floundering, not knowing where to start when they want to implement something.

High-level developers are too used to the complex constructs provided by frameworks, libraries and even the compiler. 

## Theoretical and fundamental challenges

To my mind, the main challenge of learning low-level development should be understanding the theoretical and practical challenges involved in controlling hardware. This is fundamentally what learning OS or low-level dev is about. 

Unfortunately, it is all too easy to get so hung up in all the other differences, that you never get around to actually understanding how an OS is structured, how drivers control actual hardware and theoretical challenges such as attempting to allocate memory during an interrupt handler.

## Availability of tools and information

Lastly, and this is something often drastically underappreciated, the tools and information available for OS and low-level development are extremely limited (especially if, for example, you compare it to web development). 

This means that many high-level developers find low-level development scary and difficult to work with. They find getting hold of vital information next to impossible and trying to get a toolchain to work is a dark-magic art in its own right (let alone write your own make script). 

Compared to the mass of build tools, pre-built libraries and wealth of documentation (including Q&A forums) for C# or web development, low-level development is a barren wasteland with the occasional brain dump from some previous hacker.

# Separating concepts

To try to tackle these barriers to entry, we must separate out the concepts that must be learnt for someone to transition from high to low level development. FlingOS separates the learning into:

1. Language & Software design
2. Computer architecture / hardware design
3. Software-hardware interaction
4. OS architecture 
5. Driver architecture

Language and Software Design covers the C language, how to structure C programs, how to compile and link C programs, use of header files, how to implement basic framework functions and constructs such as linked lists. There are plenty of good University courses and online, free tutorials which can teach C and how to create high-level constructs within it. 

Computer architecture and hardware design covers the fundamental "how does silicon make a device" stuff but also how memory and registers can be used to execute instructions and form some coherent sequence of events. 

Software/hardware interaction covers the mixed-up layer between software executing on a processor and software sending/receiving instructions/data to/from hardware. It covers things like I/O ports, DMA, memory-mapped I/O, device registers and interrupts. 

OS architecture and Driver architecture cover the software structure that is built to manage hardware, abstract it to a common API/ABI and ultimately to support high-level software. Anything from virtual memory managers to USB drivers come under these topics.

# Splitting up the learning

As has been mentioned, the C language and C software design practices can be learnt online already. However, current belief is that this has to be done before you can learn any of the other concepts or before you can even begin to play with the low-level.

FlingOS throws this idea out the window. From an objective view, it is perfectly possible to learn how to manage a piece of hardware, how to send USB requests or perform memory-mapped I/O without needing the C language. In fact, any language that supports memory indirection (i.e. pointers) is appropriate. 

Thus learning low-level development can be split up into three key parts: the appropriate language (almost always C or ASM) and the hardware design and the software control/abstraction layers.

# Reducing the remaining barriers

FlingOS reduces the barriers to entry by implementing the low-level software in a familiar, highly-popular high-level language: C#. This means that any high-level developer can come along and read the code without needing to learn a new language. This allows high-level developers to get hands-on with the nitty-gritty of OS development and architecture much earlier on. 

C# hides some of the memory management (heap only) and other tricky aspects allowing the student to focus on the intention of the code. This lets them understand, for example, the meaning of a sequence of register read/writes for handling a keyboard scancode, without having to endlessly think about specific code.

Teaching low-level development in a high-level language lets developers (/students) focus on what it is the code intended to do and why, rather than getting hung up on exactly how. It provides an intermediate learning platform (that you might call an interface) between the theory and practical OS dev.

## Advantages of C\#

- Easy to read, write and maintain
- Easy to understand
- No vagueness of definitions of types nor custom definitions (as is often the case in C)
- Encapsulation allowing a single component to be viewed in its own right
- Backed-up by good software structure allowing links between components to be understood
- Reduction of software complexity as memory management is handled automatically
- "Safer"/"more protected"/managed learning environment with good which can be tested in a host environment making development and debugging quicker, easier and more reliable. (Bugs are easier to spot)
- Allows study of OS/low-level dev alongside learning new languages (e.g. C and ASM)
- Allows study of what and why of OS software without needing the hard-to-learn "how" step (i.e. without having to learn C)
- Lots of good information online about how to program in C#
- Powerful, easy to use tools such as Visual Studio

## Disadvantages of C\#

- Student will have to learn C eventually
- C# is not appropriate for a commercial full-scale OS nor any embedded OS since it is comparatively slow and bulky
- C# clunks a bit when you need to use assembly code for some aspects of OS dev
- Need to use a custom Ahead of Time (AOT) compiler such as the FlingOS compiler

# Conclusion

Programming an OS in C# doesn't make much sense on the face of it. Particularly not for the embedded market. But if you stop viewing it as an OS, and start viewing it as a stepping stone between high-level development and low-level development, its value is obvious. 

We have just launched 30 new articles, a series of 10 tutorial videos and a new release of our x86 C# kernel. With sponsorship from Imagination Technologies we have also added MIPS support to our compiler and we're developing a cross-platform kernel for x86 and the Creator CI20. While it is yet to be seen if our approach truly works, there is certainly a lot of enthusiasm and early evidence to suggest it will succeed.

The University of Bristol (UK) has also shown support for us and, as such, we will be running a series of lectures and workshops in the coming academic term. Find out more [on the lectures & workshops page](/lectures).
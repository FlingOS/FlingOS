---
layout: reference-article
title: Debugging
date: 2015-09-04 02:14:00
categories: [ docs, reference ]
description: This article looks at a variety of common techniques used in both general and OS-specific debugging.
---

# Introduction

## Scope of this article
This article looks at a variety of common techniques used in both general and OS-specific debugging. It also offers explanation of how basic debugging works and some suggestions and recommendations for your own approach. The information in this article could also be used as a starting point for further research towards writing your own debugger.

## How this article is structured
This article is structured into separate sections for the different types of debugging followed by sections for related, practical information.

---

# What is debugging?

## Definition
The act of detecting and removing errors from a system. In an OS development context, the system is the OS and you must remove mainly logical, lexical and design bugs.

## Explanation
Humans are not infallible. This is a well-known fact (even if it is widely ignored). Programmers must accept that pretty much all code they write will contain errors. These errors are called "bugs". There are several types of bugs. The three main types are:

- Logical/algorithmic
- Lexical/syntax
- Design

Logical or algorithm bugs are mistakes where the code does not do what the developer intended when it is executed. These kinds of bugs require testing by execution. They are tested by passing in a set of inputs and examining the outputs. If the outputs do not match, the steps taken in between (i.e. the actual code) is examined by one of the debugging techniques described later in this article.

Lexical or syntax bugs are mistakes in typing. These are usually found at compile time as the compiler will reject invalid code. They are equivalent to spelling and grammar mistakes in normal languages, except that code must contain 0 errors (where as English grammar is frequently used and abused).

Finally design bugs. These errors are found some time during the development process. They may require only minor tweaks to fix or might require an entire redesign of the software. A design team should aim to avoid all design bugs by following proper, strict design guidelines and thinking things through. A worst case, and one which generally occurs in time regardless of initial effort, is secondary development exposing design issues. Usually these issues are called "backwards compatibility issues". A design which never gets redesigned to accommodate newer technologies is an impressively flexible one.

---

# Types of debugging

## General
There are three (main) types of debugging. Debugging by thinking, by outputting and by interrupting. These types of debugging are described in more detail separately below. However, I will at this stage suggest that no one approach is definitive or better than the other. Each should be used as appropriate to the situation. A developer who focuses on one approach more than another is either not a good developer, or is developing in a very specialist / specific context.

## Recommendations
It is my recommendation that you always debug by leaving your desk first. In all seriousness, most bugs can be fixed fastest by leaving your desk, having a drink then returning to the problem with fresh eyes. Your subconscious is probably the most powerful debugging tool available to you, so give it time to work.

Debugging by thinking is indispensable. You are never going to solve a problem using the other two methods. They will simply help you locate and define what the problem is. Ultimately, thinking is what is going to find you the problem and solution fastest.

Lastly, trust your instincts. As time goes by you will (or already have) developed an instinct as to what a problem is. Trust them and chase a problem by what you feel it might be. However, do be prepared to stop and think and backtrack if you find your instinct was wrong.

# Debugging by thinking
Debugging by thinking is probably the least consciously recognised form of debugging but by far the most important. Debugging by thinking has 3 aspects:

1. Thinking through the logic of what your code does and comparing it to your intentions.
2. Thinking about what the computer's output is telling you versus what you expect. Then thinking about any differences to hypothesize what and where the bug might be.
3. Thinking about a particular bug to come up with a solution

Taking each of these points in turn:

# 1. Logical thinking
As described before, logical thinking is thinking through the logic of what your code does and then comparing it to the logic you intended. This is best done in two stages and usually involves a pen and paper (don't fear old-school tech ;) ).

Start by working out all the (expected or unexpected) possible sets of input values to your code. Write them down somewhere for later reference. Then, taking each set of sets input values in turn, go through your code line by line determining what effect each line has. Pay particularly close attention to branching statements, such as if-blocks, or mathematical calculations involving +- 1. These are most often the places where code is wrong.

If it is helpful, make a copy of your code (printed or in note taking software such as Microsoft OneNote) and annotate what happens on each line. Record the final output that you come up with for each set of inputs. Compare these to the actual outputs when you run the code. If they match, you read your code properly If not, you got lazy and jumped some steps - go back and think through it again.

Once you have the actual outputs, compare what happens on each line to what you'd expect to happen. It is often helpful to draw a flow-chart of what you expect to happen and a flow chart of what is actually happening. While tedious, it offers a different view of your code that doesn't have all the mess of actual code. It can help you to see precisely where your code is logically wrong.

By this stage, for bugs which are logic errors, you should have found your bug. However, it may not be enough to think about what your code does for expected input cases or if your code makes use of external data over which you have no control (e.g. a database). In this case, you may need to utilise the other debugging methods presented here.

# 2. Comparative thinking
As described before, comparative thinking is thinking about the difference between the output of the computer and the outputs you expected. You then hypothesize about what could have generated the output (irrespective of what you expected any inputs to be). It is assumed that you tried logical thinking (1) first. By working backwards from the outputs, you can work out either what the inputs were or where an internal value first stops matching the expected value given the inputs.

Comparative thinking can be tackled in various ways. Experienced programmers often work off instinct, meaning when they see certain output they automatically no what (and, possibly, where) the cause of the error was. At a lower experience level you can either: apply trial and error to your hypotheses by (sensibly) guessing what and where the bug is, then checking to see if you are correct. Or, you can apply logical thinking in reverse and painstakingly work backwards through the code from the outputs to the inputs, comparing internal values / logic as you go. Either is acceptable though the latter is slower but more reliable. If you find your guesses are usually wrong, you should apply the latter method to build up your knowledge.

# 3. Problem solving
The final part of debugging by thinking is probably the most important: problem solving. Finding and refining exactly what the bug is all very well, but that doesn't actually solve the problem. The final stage is to think of a solution (preferably a good one ;)). Often the solution is obvious and only affects a few lines of code but with bigger bugs the solution may involve design changes. In the latter case you will have to communicate with your team about the best solution and the effect of any changes you make on the rest of the software. Very occasionally you will have to program multiple solutions to a problem to allow you or your team to compare them as complexity, efficiency, speed, global effects on the software etc. cannot be predicted beforehand.

However, this isn't the full story to problem solving. A good debugger (/programmer) shouldn't just fix the immediately apparent bug. A good programmer will think where else in the software may have the same bug and see if a change elsewhere in the code can fix all possible cases of the bug. This may even involve a bigger, more significant change. However, if it improves the quality of the overall codebase, it is worthwhile in the long-term. (It should be noted that often the goal of preemptively fixing bugs is idealistic as companies set deadlines which prevent you from having time to fix non-immediate bugs.)

# Debugging by output

## General
Debugging by output is debugging by having code generate a lot of output and reading through the output to see where the code goes wrong. Do not confuse debugging by output with debugging by comparative thinking. The two are not the same. Debugging by output is a brute force approach where you add (often simple, plain-text) messages that say what the code did, variable values and sometimes whether values match expected values or if an exception case was met. Debugging by thinking is a finer art where you look only at the outputs and think about what must have happened to generate them. You should apply debugging by thinking before applying debugging by output, since debugging by output is generally slower due to time taken to program the messages, re-compile and then execute the code. Debug output also usually makes code run an order of magnitude or two slower.

## Recommended styles
In my years of programming I have developed my own style of output message that I recommend to other programmers since it provides the most important information. You may choose to ignore this recommendation but you will probably waste (collectively) hours of your life if you do.

For text-based output messages I use the following format (without quotes):
  
    "File Name (no path): Method name: [Message]" e.g. "Kernel.cs: Main: Unhandled exception occurred."

And for methods where two or more of the same message appear:

    "File Name (no path): Method name: (Repeat no.) [Message]" e.g. "Kernel.cs: Main: (1) Unhandled exception occurred."

This format offers easy traceability since the message tells you precisely where it came from. There are two things to note about this format:

Some languages offer functions to output current execution location in human-readable format (such as file, method name and line number). Use this instead of my format if it is available since it will make the debug messages more reliable and portable.

The format is not portable. The onus is on the programmer to ensure the messages are kept up-to-date if the code or messages change (e.g. repeats of a message added / removed or method moved to a different file). Failure to be vigilant in updating debug messages will lead to even more problems later. However, in a worst case scenario, a developer can always do a global search for the debug message and there will be a limited selection of results to go through to find the correct one.

## Screen
Debugging by output via the screen is probably the most common and easiest method. It provides immediate feedback to the developer allowing you to see in real-time what the computer is doing. It is also fairly easy to set up output to a computer screen using VGA text-mode when an OS first boots. This offers the ability to debug an OS from the very start of the system. The VGA text-mode mechanism also uses the very simple ASCII character set and 1 byte colour system (4 bits for foreground, 4 bits of background). This makes formatting output simple and easy. As an OS gets more advanced, better text output and more advanced graphical output can be supplied (such as loading bars! Though these can be done using text if you're feeling fancy.)

## Log file
Debugging by output to a log file (or just listening to a COM port) is equally common and possibly more useful but can be trickier to set up in the OS. Log files offer you the opportunity to look at the output later and slowly walk through what happened, but you do not get real-time output. Listening to output on a COM port gives you real-time information (which can also be saved for later) and offers the ability to send data back to the OS. But, while in principle it is easy to set up, it can be the cause of many problems in early-stage OSes where interrupts and port management code have not been written or formalised. This means it is harder to get debug-from-boot working and anything written will need modifying later when the OS becomes more advanced (and formalised systems are put in place).

## Making use of the Halt op
When debugging via output, the information can often be overwhelming. This is because the computer runs so fast your messages are outputted too quickly for you to process what is happening. You can solve this in two ways. Either by using a log file or you can make use of the Halt op. By inserting the ASM halt op, you can stop the processor at specific points to give you the chance to read what is happening. If you don't clear interrupts and have the timer interrupt enabled, processing will restart some time later. Otherwise, processing will never restart. This is still useful as you can just go and shift your halt op to later in the code to allow the code to run further before stopping. (Obviously shifting the halt op requires a recompile and re-run. This may not be appropriate for some specific applications but it is useful for most beginner/low-level OS development.)

# Debugging by interrupting

## Break instruction
Debugging by interrupting is debugging by pausing the code, taking a peek, maybe editing some values and then allowing the code to continue. The key steps are:

1. Breaking the execution
2. Pausing indefinitely while still being able to read out values
3. Continuing execution

Most, if not all, CPU architectures have a built-in mechanism for breaking the code in at least one way. Usually, breaking the code involves inserting a break instruction into the code or setting a break flag in a CPU register. In either case, the CPU is informed a break should occur and at the next available point does so. However, it should be obvious the CPU is not going to just stop executing. Instead, it stores some information about the current execution state then jumps to code specified by the programmer.

The CPU is said to be in a "broken" state while the code jumped to continues to run. Said code can do anything from nothing at all to completely replacing the program being run. (This is the reason why debugging modes can be so dangerous, especially at a low level). Assuming the person debugging wishes execution to continue as normal after they have finished inspecting, the code must make sure it restores any values it changes while in the broken state. The code is also likely to output values to a debugging tool (for example Visual Studio). For OS development this is most often done via COM port 0.

When execution is to be continued, the execution state (from before the break) must be restored. The CPU can then continue executing instructions as though nothing had happened. Most CPUs have special instructions for storing and restoring state and for returning from the "break state" code. The "return" op may be the normal interrupt return op.

For details of the x86 break interrupt ops (Int1 and Int3) and state management ops, please see later in this article.

## Registers
Most OS debugging software, alongside showing the current line of code, will show memory and register information. The debugger should provide the register information in both a raw-value format and, where possible, an interpretted value (such as indicating an object pointer or floating point number). The debugger should also be capable of showing values not just for the general purpose registers, but also the CPU state registers and specialised registers.

If the debugger has been designed for debugging code on more than one processor, you will probably have to tell it which processor you are debugging and how to communicate with the OS debug code. Otherwise, it will not know how to send commands to the OS in its "broken state", nor how to interpret data sent back by the OS (such as register values).

When debugging, register values can prove useful for showing the intermediate processing of each assembler instruction. Registers provide, in a sense, the lowest level view of the changes the processor makes and certainly the most immediate view of the results of any op. Most (or at least a high number) of op results are stored in a register before being shifted to the stack or other memory.

## Stack
Most OS debugging software will show you the current stack as a stack of interpreted values and will also only show the stack for the current stack frame (i.e. current method). Often (and most usefully), these values are split into arguments and local variables by the debugger so as to distinguish. A good debugger will also highlight values which change as you debug. (A good debugger does this for all values including registers, tracked memory, etc.).

The stack is probably the most useful information provided by the debugger, second only to the current line of code. It shows you what the inputs and outputs of a method are. This allows you to apply the methods of debugging described in "Debugging by thinking".

## Memory
Some OS debugging software will allow you to access any point in memory (either by virtual or physical address, depending on the level of sophistication). It may even allow you to "watch" or "track" an area in memory and advanced debuggers such as WinDbg or Visual Studio will allow you to create "memory breakpoints" which are hit when the value at a memory location changes.

Regardless of situation, memory access is the most advanced feature as, beyond returning raw numbers, it requires the debugger to understand the values at the memory addresses requested. This requires symbolic information, described below.

## Symbols
Advanced compilers and debuggers will produce or use files called Symbol Files (or Symbol Databases, since they are, in fact, databases). Symbols are a summary of the structure of data within the original code. For instance, they store the names and types of local variables within a method and where in the stack the local variable can be found.

Symbols allow the debugger to understand the raw data and convert it from raw numbers back to what the meanings the programmer originally assigned. For example, it can convert raw object pointers to names of the types they point to or another example, it can convert raw numbers into the enumeration names they were assigned. Symbols also allow the debugger to understand pointers and use them to load data from memory such as object properties.

Symbols are produced by the compiler at compile time. Most compilers are capable of outputting some level of symbol information but each compiler has its own format. Therefore, you must choose a compiler and debugger that are compatible or write a program to convert symbol file formats automatically or write your own compiler or debugger. The latter two options are tough projects in themselves so are not advisable.

When you start your debugger you will need to point it to the symbol file produced by the compiler when you compiled your OS. Some debuggers, such as the default one in Visual Studio, like to try and automatically detect the symbol file based off your build configuration. However, most debuggers (including Visual Studio) have a Symbol File Path list of exact file paths or inexact folder paths to search through for the correct symbol file.

## Virtual Machines (use serial port)
Virtual Machines have obvious value in OS development. However, there is added benefit beyond not ruining your physical machine. Virtual Machines allow you to easily attach to the serial port of the virtual computer which provides an easy-to-use, simple, reliable and thus convenient way of passing commands and data in and out of the OS. Thus most debuggers use the serial port (COM0) to communicate with the OS. Some advanced OS'es (e.g. Windows, Linux) allow debugging over ethernet, but even that isn't as reliable for very low-level (i.e. sub-driver level) debugging.

How you access the COM port within the OS varies from across architectures, however, it is almost universally called COM0. Furthermore, how you communicate over the port is entirely up to you (or the debugger you selected to use). Details of how to use COM0 under the x86 Architecture are provided later along with a communication protocol suggestion.

## Real Hardware
Debugging on real hardware, for OS development, is difficult. For starters, you have to work out how to connect your debugger to the machine physically. This sounds easy but unfortunately you cannot use the convenient serial port method quite so conveniently. Firewire or network debugging would both require drivers which, in the early stages, you will not have. Then you need a debugger capable of interfacing with the connection, you need to know which specific CPU your real hardware contains, you need a way of cycling the power from the debugger (or you're going to get arm / back ache powering the PC on/off all the time) and a host of other issues.

Real hardware has its place, but only for the occasional test to make sure you haven't picked up any VM-specific code (or more likely, hacks). Real hardware also has the issue of how you replace the copy of the OS code on the boot device. Using a bootable USB stick is easiest, but also requires tedious unplugging, reflashing and plugging back in again.

If you still intend to do most of your debugging on real hardware, then I recommend you find a cheap USB to Serial cable - beware the cheap knock-offs of Prolific's USB to Serial chip. Genuine chips work fine, the knock-offs don't. However, the knock offs are more common and very hard to spot! I also recommend you buy an IP Power Bar or solder together your own equivalent as it will save you endlessly pressing and holding the power button.

## GUIs

### General
There are probably hundreds of pre-built debugger GUIs out there, only a fraction of which are going to be aimed at OS development. A couple of common ones are mentioned below, though I have never used them. In general, using a pre-built debugger will cause you more work to set up the debugging stub code in your OS, but save you a lifetime of work writing your own (unless you happen to be writing your own compiler).

### GDB, Visual Studio and alternatives
One popular debugger (which I've heard is very good) is called GDB. It's the GNU Project Debugger and is widely used by hobby OS developers. An alternative that can be used is Visual Studio, however, this locks you in to the Visual Studio way of working and the Microsoft compilers and using Windows as your development platform. Some people object to this. A final place you might look is plugins for or versions of Eclipse aimed at OS dev'ing in your chosen language.

### Write your own
You can write your own debugger. This is usually only recommended if you are also writing the compiler or have some particular issue with existing debuggers (not just that you can't be bothered to interface with them). Writing your own debugger has the advantage that you can easily get a simple debugger working reliably and have limited amounts of stub code in your OS. It also allows you to tailor the debugger to your OS. However, the big downside is that it takes a lot of work and requires you to frequently debug your debugger to work out what is going wrong. Occasionally, the bug may be in your debugger not your OS!

### IDE integration
IDE integration is friendly and very helpful and debuggers such as GDB often have plugins for various common IDEs freely available. Visual Studio comes with the debugger built-in and Eclipse works on a plugins based system. If you are using Visual Studio, I have a word of warning for you. DO NOT try to write your own integrated debugger for Visual Studio (unless you work for Microsoft). The backend of Visual Studio is horrific, documentation is limited and the pain of trying to get even the most basic stuff working is unbelievable. The Cosmos Project has been so stuck in their own Visual debugger development that they have made negligible progress for 3 years (see their Commits over past few years at [https://cosmos.codeplex.com](https://cosmos.codeplex.com)).

---
 
# Virtual machines / emulators

## General
As has been mentioned previously in "Debugging by interrupting", virtual machines (and emulators) offer big advantages over real hardware, not least in their ease of use and high rate of successful recovery. You can connect debuggers easily, you can easily update version of your OS as part of compilation and you can take backups of working versions all fairly easily. Real hardware is a complete pain to do any of these things with. Virtual Machines are now sufficiently good that testing on real hardware is only really necessary in two situations:

1. You are about to deploy your OS on real hardware so need to test your OS on real hardware.
2. You are targeting a specific hardware setup which you cannot perfectly reproduce in a virtual machine.

With the move towards so called "cloud" virtualisation technology, a hobby OS developer may well expect to never have to run their OS on real hardware at all! (But that takes the fun out of it a bit...)

Do not confuse virtual machines with emulators. An emulator "emulates" the hardware by processing each CPU instruction in special software design to interpret many different instruction sets. An emulator makes no real use of the actual hardware on which it is running. A virtual machine is just the opposite. It virtually runs the OS but the physical CPU still does all processing directly. Thus, a virtual machine can only virtualise for CPU architectures on which it is already running. You cannot virtualise ARM on an x86 machine, but you can emulate ARM on x86 hardware.

Virtual machines offer less capability in terms of number of architectures they can simulate but they run significantly faster. In fact, modern x86 machines with Intel's latest virtualisation technology can run a virtual machine in real time as though it were the native OS. Emulators tend to be very slow, clunky and require a lot more setup since you must specify the exact hardware to emulate in very fine detail. I would highly recommend using a virtual machine over an emulator, if possible.

The following sections give brief information about the various virtual machines and emulators that are available. It is by no means comprehensive but it does cover the most common/popular ones.

## VMWare
FlingOS uses VMWare as its main virtual machine (specifically the free, non-commercial version of VMWare Player). VMWare is one of the easiest to use software packages with a reasonable level of configuration and it is easy to add and connect peripherals such as USB hubs and serial ports (the latter being particularly important for debugging). However, it is less versatile than Virtual Box by virtue of the fact that it is a stripped-down version of the full VMWare Workstation. VMWare is also not primarily aimed at OS development. Rather, it is aimed at virtualisation for servers and stable systems which admins need to manage on-mass.

FlingOS uses VMWare because it happens to come with a convenient library which can be referenced / loaded from C\# for controlling VMWare. This made writing an automated debugger that starts and stops the target virtual machine much easier.

## Virtual box
Virtual Box is free virtual machine software from Oracle. It is well maintained, more versatile than VMWare and there is no paid-for version (to my knowledge). It also seems to have more options for configuration and debugging and so is better suited to OS and low-level development. It also has free tools for manipulating virtual hard drives and other peripherals. This makes Virtual Box a better offering to the OS dev world than VMWare.
FlingOS chose not to use Virtual Box due to its lack of easy C# interfacing. This may not be the case or may have changed since the decision was made, but there is little to no gain from changing now (at the time of writing).

## Qemu
Qemu is an emulator software package. It is highly technical, requires text-file and command line setup, has a steep learning curve and is difficult to set up more than the basic emulator. It is, however, extremely good, works on any platform, can emulate virtually all major platforms and is free. It is widely supported with lots said about it online. However, being an emulator, it runs much more slowly and is harder to manage, meaning it is only recommended if you really need an emulation not virtualisation.

---

# Required debugging tools

## Text-based output
Text-based output (either through the screen or serial port or something else) is an absolute must when debugging. Outputting "I reached here" and "My value is X" messages are invaluable and about the best catch-all method of debugging that there is. If you get anything working first in your OS (after the boot sequence) you should get text-based output working.

## Code-line-specific failure output
"Code-line-specific failure output" i.e. output exactly what line of code the execution of your OS failed at. Whether your language supports try-catch or not, you can always output a message after every line and see when it doesn't do what you expected. Languages supporting try-catch or similar may be able to output the specific line of code at which the exception (or other event) occurred based off built-in information.

---

# Useful debugging tools

## Interrupting debugger
Arguably an interrupting debugger is required/essential but I do know some people who do without one so I won't list it as required. However, it is extremely useful. Being able to break into your code and see the values of variables and where the code goes next is highly illuminating in debugging. It can also be the only way to spot compiler bugs by digging into the disassembly).

## Integration with development environment
Integration with your integrated development environment is useful, that's why IDEs were invented. But it is only a convenience. FlingOS's debugger is not integrated (for reasons mentioned earlier in this article, regarding Visual Studio). Integration can be easy or hard to achieve, depending on how your IDE was designed. Probably the most extensible IDE (so much so, it has now become an industry standard piece of software), is Eclipse. If you want an IDE to integrate with, Eclipse is probably your best bet. Eclipse also has plugins for just about every language.

## Register, memory and stack output
Register, memory and stack output are usually included in an interrupting debugger but you can do them separately just using 2-way serial communication with OS. For example, send it a command with a memory address and have it return the 32-bits starting at the supplied address. Register output is only useful if you can pause the code between instructions. Stack output is useful if you can stop inside a method (to see arguments and locals) or to see stack of an exception interrupt.

---

# Unnecessary debugging tools

## Edit and continue
"Edit and continue" would be nigh on impossible to achieve for an OS but I heard rumour of such a project a few months ago (2014-10) so I guess it's possible. However, it's completely unnessescary and you'd be wasting your time. If compile time is your issue, then either get a faster machine, install more RAM, use a better compiler (or improve your own) or start using libraries properly!

## Anything which takes you far too long to program
Anything which takes you as long to program as your OS does, is an unnessescary debugging tool. This is true on the assumptions that:

You are a lone developer so you don't have the time to waste

As a lone developer, anything you are doing has probably already been created. Thus you need only go find and adapt it. Paying for stuff occasionally won't break the bank but might just save your project from grinding to a halt (and probably your soul as well...)

---

# x86 Debugging

## Disassembly : Thought debugging
Disassembly is a partial reverse compilation of the machine code of a program. You can decompile your program/OS machine code into the correct assembly code and then use this for both thought and interrupt debugging. This section will discuss its use for thought debugging. Its use in interrupt debugging will be discussed further on.

Thought debugging disassembly code requires a good understanding of how your original code translates (/compiles) into raw machine code. Without this, you will be lost in the raw data manipulation that the disassembly code exposes you to and never succeed in working out what is actually happening. If you find yourself lost, you should either use disassembly level debugging only as an interrupt debugging tool (see further on) or just stick to your high level code and other methods of debugging.

I recommend that you approach thought debugging disassembly code by working out at a higher level what your specific issue is that you need to trace. Narrow this down to as small an area of disassembly code as possible. Having done this, use interrupt debugging to get the inputs and outputs to your disassembly code (e.g. register and stack values). Now you can begin to think through what the assembly code does and then compare that to what you thought it should do.

It is usually useful to print out (on paper or into OneNote) the assembly code and write down the values of registers and stack (in a structured table format) next to each line of assembler. This allows you to work out carefully and easily look back at the changes your assembly code makes. Also, if you pre-create your table then find part way through filling it in that the table does not cover a situation, you will probably have found a hint as to your bug.

## VGA Text-Mode Video : Output Debugging
VGA text-mode video is probably the most useful debugging tool for x86, second only, perhaps, to an interrupt debugger. VGA text-mode is available on pretty much every PC that has a screen attached, even if that's via HDMI. VGA text-mode video is also incredibly simple (despite what many OS dev articles would have you believe).

VGA text-mode video is so simple it simply involves placing an 8-bit ASCII character into a specific memory address, followed by two 4-bit colour codes. The ASCII character codes can be looked up (if necessary, though any decent compiler will convert for you) from here: ASCII Character code table (Link valid as of 2014-11-03). Any Google search will find you a suitable equivalent. The colour codes can be found here: VGA Basics : Mode 0x13 memory (Link valid as of 2014-11-03).

The trick with VGA text-mode programming is not to get caught up on all the BIOS-interrupt stuff. Half of the BIOS interrupt stuff stops working once you set up your own interrupts and are in protected mode. The following examples will show you how to output text to the screen (in assembly and C\#). FlingOS contains several console implementations of varying complexity which are made available to the reader to learn from. They include complex tasks such as scrolling and backspace. Further information and examples on outputting to the screen can be found in the "Display / Video / Graphics" section.

``` csharp
                     // Microsoft C# .Net 4.5, x86 (32-bit)
/******************** VGA Text-mode : String Output Example ********************/

// Code taken from FlingOS source: Kernel.Core\Consoles\AdvancedConsole.cs
// 2014-11-03

// Notes to the reader:
//      Buffer: - The text to put on the screen.
//              - A field of the container class. 
//              - Type: FOS_System.List containing FOS_System.String
//              - Each string must be 80 characters long.
//              - Buffer can contain as many lines of text as you wish. The bottom
//                25 will be displayed.

/// <summary>
/// A pointer to the start of the (character-based) video memory.
/// </summary>
protected static char* vidMemBasePtr = (char*)0xB8000;

/// <summary>
/// Update the display.
/// </summary>
protected unsafe override void Update()
{
    //Start at the bottom of the screen - 25th line has index 24
    char* vidMemPtr = vidMemBasePtr + (24 * LineLength);
    //Start at the current line then move backwards through the buffer
    //  until we've either outputted 25 lines or reached the start of
    //  the buffer.
    for(int i = CurrentLine; i > -1 && i > CurrentLine - 25; i--)
    {
        //Get a pointer to the start of the current line
        //  We could index into the string each time, but using a pointer
        //  is much faster.
        char* cLinePtr = ((FOS_System.String)Buffer[i]).GetCharPointer();
        //Loop through the entire length of the line. All lines will be of
        //  LineLength even if nothing is written in them because blank
        //  lines are created as a LineLength of spaces.
        for (int j = 0; j < LineLength; j++)
        {
            vidMemPtr[j] = cLinePtr[j];
        }
        //Move backwards through the video memory i.e. upwards 1 line
        vidMemPtr -= LineLength;
    }

    //Clear out the rest of the screen
    while(vidMemPtr >= vidMemBasePtr)
    {
        for (int j = 0; j < LineLength; j++)
        {
            vidMemPtr[j] = (char)(' ' | CurrentAttr);
        }
        vidMemPtr -= LineLength;
    }
}
```

``` x86asm
; NASM, x86 (32-bit)
; - - - - - - - - - - - - - - - - VGA Text-mode : String Output Example - - - - - - - - - - - - - - - -

; Code taken from FlingOS source: Kernel\ASM\WriteDebugVideo.x86_32.asm
; 2014-11-03

; BEGIN - Write Debug Video
; Argument 0 : 32-bit address, pointer to string data, format of data at address:
;               - First 32-bits = Length of string
;               - Remaining data is treated as 16-bit Unicode with no null terminator
; Argument 1 : 32-bit unsigned integer, treated as 8-bit unsigned integer, 
;              the colour to print the characters (background and foreground).
method_System_Void_RETEND_Kernel_PreReqs_DECLEND_WriteDebugVideo_NAMEEND__System_String_System_UInt32_:

; MethodStart
push dword ebp
mov dword ebp, esp

mov dword ebx, 0xB8000 ; Load vid mem base address

; Set num characters to clear 
mov dword eax, 80   ; 80 characters per line
mov dword ecx, 25   ; 25 lines
mul dword ecx       ; eax * ecx, store in eax = numbers of characters to clear
mov dword ecx, eax  ; Store number of characters to clear in ecx for later use in loop

mov byte ah, 0x00   ; Set colour to clear to. Here black for background and foreground
mov byte al, 0      ; Set the character to the null/empty character

method_System_Void_RETEND_Kernel_PreReqs_DECLEND_WriteDebugVideo_NAMEEND__System_String_System_UInt32_.Loop1:
mov word [ebx], ax  ; Move the empty character/colour to vid mem
add dword ebx, 2    ; Move to next character space in vid mem
; Uses ecx - loops till ecx = 0 i.e. till all characters cleared
loop method_System_Void_RETEND_Kernel_PreReqs_DECLEND_WriteDebugVideo_NAMEEND__System_String_System_UInt32_.Loop1


; Load string length
mov eax, [ebp+12]    ; Load string address
mov dword ecx, [eax] ; String length is first dword of string

mov edx, [ebp+12]    ; Load string address
add edx, 4           ; Skip first dword because that is the length not a character

mov dword ebx, 0xB8000 ; Load vid mem base address

mov byte ah, [ebp+8]  ; Load colour

method_System_Void_RETEND_Kernel_PreReqs_DECLEND_WriteDebugVideo_NAMEEND__System_String_System_UInt32_.Loop2:
mov byte al, [edx]    ; Get current character of string
mov word [ebx], ax    ; Move character and colour into vid mem
add dword ebx, 2      ; Move to next character space in vid mem
add dword edx, 1      ; Move to next character in string
; Uses ecx - loops till ecx = 0 i.e. till all characters gone through
loop method_System_Void_RETEND_Kernel_PreReqs_DECLEND_WriteDebugVideo_NAMEEND__System_String_System_UInt32_.Loop2

; MethodEnd
pop dword ebp

ret
; END - Write Debug Video
```

## Int3 & Int1 : Interrupt debugging
Interrupt debugging vies for top-spot as x86's most useful debugging tool. This article has made little attempt at actually explaining the nitty-gritty of how interrupt debugging works, largely because there is no one way. However, the code sample below demonstrate the basic way (which FlingOS uses).

Essentially what happens is when the OS starts, the interrupt table is set up with Int1 and Int3 handlers. When the code hits an Int3 (which is inject by a compiler during a "debug" build), the Int3 handler is fired. The Int3 handler listens to COM port 0 waiting for a "go" signal at which point it "interrupt returns" (IRet's) and execution continues as normal.

This process can be further advanced so that while the Int3 handler can recognise more than just a "go" command. This allows it to accept commands and parameters and return information such as register values.

A further advancement is having the debug code inject Int1 ops (and possibly Int3 ops) which allow op-by-op stepping. However, this is complicated in x86 by instructions having variable size and so often Int3 place-holders (NOP ops) are put in the code by the compiler and those are replaced Int3s when required. Int1 ops are then used to get from an Int3 to a more specific point in the code.

Information and examples for setting up the x86 Interrupts Table can be found under x86 Descriptor Tables.

The code for the x86 ASM Debugging was too long to directly include in this article. The sample code includes everything required for the basic debugger which includes a basic command set, information transferring and Int1 debugging. Under the copyright, you may only use the sample to look at, not as a basis for your own debug code. A fully documented copy of the source from 2014-11-03 is available from here: ASM x86 Interrupt Debugging - 2014-11-03 (Link valid as of 2014-11-03). Unfortunately, there is one known bug in the sample, where it will not return the correct memory data from the get memory command. Otherwise, the code is known to work fully.

---

# Revisions
The following is a list of all the revisions made to this document in order of first to last (oldest to newest).

- 26/10/14 - Initial version.
- 02/11/14 - Spelling and grammar corrections.
- 03/11/14 - Added x86 Debugging sections.
- 04/09/15 - Conversion to Markdown syntax.
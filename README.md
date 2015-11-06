# [FlingOS™](http://www.flingos.co.uk)

***FlingOS launched on September 17th, in Bristol, UK. [Find out more here](http://www.flingos.co.uk/Launch).***

Welcome! This is the main repository for the FlingOS™ project. We used to be over on BitBucket but have recently shifted across. 
The FlingOS project is an educational operating system aiming to provide high-quality resources for learning OS and low-level development. You can find out more on our website over at [www.flingos.co.uk](http://www.flingos.co.uk). There you'll also find our [documentation](http://www.flingos.co.uk/docs/) and links to our tutorial videos. If you're wondering why we use C#, take a look at [this article](http://www.flingos.co.uk/docs/reference/Why-CSharp).

The FlingOS project is a three part approach to teaching OS and low-level development. You're currently looking at just one part - the code itself. The code acts as a sample codebase for people to learn from and compare to. The second part is our [conceptual articles](http://www.flingos.co.uk/docs), which explain all the OS and low-level technology in detail. The third part is our tutorials which are free, ~20min videos with complete resources, [available on YouTube](https://www.youtube.com/playlist?list=PLKbvCgwMcH7BX6Z8Bk1EuFwDa0WGkMnrz). You can find all these links and more on our [main website](http://www.flingos.co.uk).

## Getting Started

### Interested in learning OS/low-level development?
Take a look at our [Getting Started](http://www.flingos.co.uk/docs/reference/Getting-Started) article to learn how to write your own operating system.

### How do I learn from the FlingOS source code?
The FlingOS source code is here for you to look at, read and compare to. By reading the FlingOS articles and taking a look at our implementations, you should be able to write your own fairly easily.

### Interested in developing FlingOS?

[Join the team](http://www.flingos.co.uk/Develop#Join-the-team) and then [setup for development](http://www.flingos.co.uk/docs/reference/FlingOS).

### Interested in our ahead-of-time compiler?

If you'd just like to use our ahead-of-time compiler to write your own C#, VB.Net or F# operating system, please take a look at our [stable releases](http://www.flingos.co.uk/releases).

## Structure

- ***Documentation*** : Contains the SHFB documentation project and reference articles. The latest build of the documentation is available to view [on the website](http://www.flingos.co.uk/docs).
  - ***Markdown*** : Contains the markdown-syntax reference articles.
- ***Drivers*** : Contains code for kernel mode drivers and the main (/new) FlingOS Compiler.
  - ***Compiler*** : Contains the FlingOS Compiler which can compile both drivers and the kernel.
    - ***App*** : Contains a command-line app for starting/executing the compiler.
    - ***Architectures*** : Contains target architecture libraries (currently x86 and MIPS)
    - ***MSBuildTask*** : Contains an MSBuild Task implementation for starting/executing the compiler as an MSBuild post-build task.
    - ***Tools*** : Contains 3rd party tools used by FlingOS during building.
  - ***Debugger*** : Contains the debugger for the kernel and drivers.
    - ***Drivers.Debugger*** : Base library for intreacting with the kernel's debugger.
    - ***Drivers.Debugger.App*** : GUI (Windows Forms) debugger application for debugging the kernel.
  - ***TestDriver*** : Contains a C# kernel-mode test driver which is being used for developing the FlingOS ABI.
  - ***Other*** : Other folders contain old RAW-build, kernel and user mode test drivers.
- ***Kernel*** : Contains the main kernel code.
  - ***Compiler*** : Tools used specifically for compiling the kernel. Contains the ISO Generator wrapper program.
  - ***Kernel*** : Contains the main kernel project which acts as the root of the system. See Kernel.cs:Main for the primary entrypoint.
  - ***Libraries***
    - ***Kernel.FOS_System*** : Contains replacements for the .NET Framework System and System.Collections classes.
    - ***Kernel.FOS_System.IO*** : Contains classes for I/O in a similar way to .NET Framework System.IO namespace does (not direct replacements).
    - ***Kernel.Hardware*** : Contains built-in drivers for specific hardware along with some higher features such as scheduling.
- ***MIPS*** : Contains our two preliminray MIPS-targetted testing kernels. 
  - ***Testing1*** : A very basic kernel for UBoot Kermit-booting over serial that just uses the LED.
  - ***Testing2*** : A second, slightly more advanced kernel for UBoot Kermit or USB OTG booting that implements a timer setup and interrupt handlers.
- ***Releases*** : Contains previous releases of FlingOS. 
- ***Testing*** : Contains unit and behavioural testing of FlingOS.
  - ***FlingOops™*** : Our cross-platform compiler verification kernel (utilises behavioural testing).
- ***Tools*** : Assorted tools written by FlingOS developers to make their lives easier. Speak to Ed Nutting for details.
  - ***CI20Booter*** : USB OTG boot tool (uses LibUsb-Win32) for the Creator CI20 board.

## How do I configure the debugger?

### Setup

First, compile a debug version of the kernel for x86 (debugging other architectures is not supported yet). In your VM, you'll need to set a debuggable virtual machine. This means adding the correct COM (serial) ports to the VM. You will require:

1. COM1 - Direct this to a file. It contains the screen output of the kernel for later reference (but isn't directly used by the debugger)
2. COM2 - Direct this to a named pipe: \\.\pipe\FlingOSDebug_Msg - It should be a client with the other end as an application. This is used for communication (synchronous message passing) between the kernel's debugger thread and the host debugger application.
3. COM3 - Direct this to a named pipe: \\.\pipe\FlingOSDebug_Notif - It should be a client with the other end as an application. This is used for asynchronous notifications to the debugger. It uses the notification 0xFE to tell the debugger a breakpoint has been hit.

### Starting the debugger and VM

At this point, you could connect to serial (COM) port 2 using any valid terminal app on the host. For example, Putty using baud 96200, RTS/CTS and "implicit CR with every LF" selected. However, on Windows, we provide a GUI application for debugging programs which is much more effective.

Build and start the GUI debugger. The root of the named pipe should be "FlingOSDebug", the "_Msg" and "Notif" are appended automatically. The assembly name should be Kernel for debugging the main FlingOS kernel. The Bin path should be the full path to the build directory; You will need to change this according to your setup.

Press the Connect button and then start the virtual machine. If all goes well, the VM should connect the ports and the OS should start executing. Once the OS has loaded and the debugger thread has started, the host debugger (GUI) should detect the connection and allow you to proceed. 

### Using the debugger

To start with, press the Refresh button to retrieve the list of processes and threads. Selecting a process or thread allows you to suspend it and inspect its state. The rest should be reasonably obvious.

To add breakpoints, type at least 10 characters in the Filter box of the name of the method you wish to add a breakpoint for. Select the available debug point (if any) and click "Set". Suspend the thread you wish to use the breakpoint with and then Step the thread until it reaches the breakpoint.

## A note on licenses
FlingOS is released under GPLv2 under UK law. This means you can't just copy and paste our code without keeping our copyright notice and you have to release your work as open-source if it includes our code. Our source code is also released without warranty and we accept no liability (within the restrictions of UK law). Please do not use our code for anything (particularly production or safety critical work) without testing and verifying it yourself.

##### Why did we choose GPLv2 not BSD, MIT or another, more permissive license? 
FlingOS is here for people to learn from by reading and comparing. We are not here to just supply out-of-the-box code so we don't allow people to just reuse our work. Also, we are providing a learning resource not a reference sample. By restricitng the use of our code it helps to prevent the widercommunity accidentally treating us a reference codebase.

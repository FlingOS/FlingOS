# [FlingOS™](http://www.flingos.co.uk)

***FlingOS is launching on September 17th, in Bristol, UK. [Find out more here](http://www.flingos.co.uk).***

*The following documentation is being written in preparation for our launch which includes a new website. As such, some of the links may not work yet.*

Welcome! This is the main repository for the FlingOS™ project. We used to be over on BitBucket but have recently shifted across. 
The FlingOS project is an educational operating system aiming to provide high-quality resources for learning OS and low-level development. You can find out more on our website over at [www.flingos.co.uk](http://www.flingos.co.uk). There you'll also find our [documentation](http://www.flingos.co.uk/docs/) and links to our tutorial videos.

The FlingOS project is a three part approach to teaching OS and low-level development. You're currently looking at just one part - the code itself. The code acts as a reference codebase for people to learn and compare to. The second part is our reference articles, which explain all the OS and low-level technology is detail. The third part is our tutorials which are free, ~20min videos with complete resources, available on YouTube. You can find the links on our [main website](http://www.flingos.co.uk).

## Getting Started

### Interested in learning OS/low-level development?
Take a look at our [Getting Started](http://www.flingos.co.uk/docs/reference/Getting-Started) article to learn how to write your own operating system.

### Interested in developing FlingOS?

[Join the team](http://www.flingos.co.uk/Develop#Join-the-team) and then [Setup for Development article](http://www.flingos.co.uk/docs/reference/FlingOS).

### Interested in our ahead-of-time compiler?

If you'd just like to use our ahead-of-time compiler to write your own C#, VB.Net or F# operating system, please take a look at our [stable releases](http://www.flingos.co.uk/releases).

## Structure

- ***Documentation*** : Contains the SHFB documentation project and reference articles. The latest build of the documentation is available onto view [on the website](http://www.flingos.co.uk/docs).
  - ***Markdown*** : Contains the markdown-syntax reference articles.
- ***Kernel*** : Contains the main kernel code.
  - ***Compiler*** : *Deprecated* This is the old FlingOS Compiler. It will be deleted soon. Instead, see Drivers/Compiler for the new compiler which can compile both the kernel and drivers.
  - ***Debug*** : *Deprecated* This is the old debugger used in conjunction with the old compiler. A new debugger is being written for use with the new compiler.
  - ***Drivers*** : Contains code for kernel mode drivers and the main (/new) FlingOS Compiler.
    - ***Compiler*** : Contains the FlingOS Compiler which can compile both drivers and the kernel.
      - ***App*** : Contains a command-line app for starting/executing the compiler.
      - ***Architectures*** : Contains target architecture libraries (currently x86 and MIPS)
      - ***MSBuildTask*** : Contains an MSBuild Task implementation for starting/executing the compiler as an MSBuild post-build task.
      - ***Tools*** : Contains 3rd party tools used by FlingOS during building.
    - ***TestDriver*** : Contains a C# kernel-mode test driver which is being used for developing the FlingOS ABI.
    - ***Other*** : Other folders contain old RAW-build, kernel and user mode test drivers.
  - ***Kernel*** : Contains the main kernel project which acts as the root of the system. See Kernel.cs:Main for the primary entrypoint.
  - ***Libraries***
    - ***Kernel.Core*** : Contains Core classes but which implement high-level features (such as the main shell).
    - ***Kernel.FOS_System*** : Contains replacements for the .NET Framework System and System.Collections classes.
    - ***Kernel.FOS_System.IO*** : Contains classes for I/O in a similar way to .NET Framework System.IO namespace does (not direct replacements).
    - ***Kernel.Hardware*** : Contains built-in drivers for specific hardware along with some higher features such as scheduling.
    - ***Kernel.Shared*** : Contains classes shared between the kernel and drivers. This is essentially the ABI/API from kernel to drivers.

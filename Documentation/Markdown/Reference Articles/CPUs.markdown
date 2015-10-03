---
layout: reference-article
title: CPUs
date: 2015-09-03 22:44:00
categories: [ docs, reference ]
description: Describes CPUs in detail including a comparison of RISC vs. CISC architectures.
---

# Introduction

## Scope of this article
This article covers two broad topics. The first is CPU Architecture which provides an explanation of what is meant by CPU Architecture, followed by a discussion of general information. This is further followed by discussion of the main two CPU architectures which developers are likely to be working with, namely, ARM and x86. A limited discussion of assembler programming is also included.

The second topic covered by this article is CPUs as Devices. This provides a different perspective on a CPU, looking at it from the OS developer's point of view once the kernel is running. It looks at the CPU as a device which must be managed and can be used to perform specific processor functions.

## How this article is structured
This article is structured into the two sections described above. These are split into sub sections, (where each item listed is only included in a given sub section if it is appropriate), which follow the general pattern presented here:

1. Overview (of what the sub-section will discuss)
2. General discussion
3. Specific examples
4. Discussion of selected specific examples
5. One example vs. another
6. Code examples
6. Conclusion
8. Links to useful resources

--- 
 
# CPU Architecture

## Overview

### Clarifying what a CPU is

#### Definition
The Central Processing Unit (CPU or just "processor" for short), is the primary piece of hardware which executes program instructions. A CPU performs the fundamental arithmetic, logic and input/output operations required to manipulate data in a useful way.

#### Explanation
The CPU acts as the "brains" of a computer system (be it a PC, tablet or mobile). Everything you want to do, has to start somewhere in the CPU. In the simplest case, your PC would have only one CPU, with one logical processing unit on that CPU. However, modern CPUs consist of multiple processing units and modern PCs can contain multiple CPUs. It is probably helpful to think of a processing unit as a CPU in its own right. While this is not strictly correct, it is sufficient to realise that each processing units acts, in the simple cases, mostly independently of the other units.

A processing unit is a piece of hardware which (ignoring how it does the following) loads a set of program instructions from memory and then executes each instruction sequentially, until either reaching the end of those instructions or faulting. The instructions a CPU executes can be broken down into 5 main categories:

- Data transfer: Ops for transferring data to/from memory or other devices.
- Arithmetic: Add, subtract, multiply, divide and various other operations involving one or more numbers.
- Logical: Bit-wise logical AND, OR, XOR, NOT and various other operations involving one or more binary numbers.
- Control flow: Branch, jump, loop and various other ops which control which instructions are executed next.
- Other: A category which encompasses a wide range of ops, from those which control Interrupts, to processor specific ops, to binary shift/rotate ops.

It is key to understand that everything a computer does can be made up of a combination of these types of operation. For instance, if you wish to multiply two numbers x and y together, you must perform:

1. A data transfer op to load x from memory
2. A data transfer op to load y from memory
3. An arithmetic op to multiply x by y
4. A data transfer op to store the result

Or, if you wish to load an element from an array you must perform:

1. A data transfer op to load the address of the start of the array
2. A data transfer op to load the index of the element to get
3. A data transfer op to load the size, in bytes, of each element in the array
4. An arithmetic op to multiply the index by the size to get the offset of the element from the start of the array
5. An arithmetic op to add the offset to the address of the array
6. A data transfer op to load the value at the newly calculated address
7. A data transfer op to store the loaded value

Of course, depending on your architecture, the above steps may or may not be optimised to fewer steps. None the less, the same basic operations are being performed. This will be discussed further in later topics covering CPU Architectures.

The key point to take away from this explanation, is that a CPU executes instructions which control what the computer does. It is we, humans, who assign meaning to the data passing through the CPU and how that data is manipulated.

#### As a target
The first perspective of a CPU that a developer must understand when developing an OS is that the CPU is a target. You pick a particular CPU (or a particular group of CPUs) which you want your OS to run on. This is what you are targeting. Each CPU (or group of CPUs) will have its own set of features, its own particular set of instructions and its own requirements from the OS. There is no one-size-fits-all way of programming the low-level parts of an OS. Once you settle on which CPU(s) to target, you must stick to that choice until you at least get your basic kernel working. After that, you can consider porting your code to other targets.

#### As a device

The second perspective of a CPU, which a developer will need to understand when developing an OS, is that the CPU is a device. It is a device as much as your mouse, keyboard and screen are devices. It requires managing while your OS is running. This management ranges from the very simple (like telling it where to get the next instruction from) to the very complex (such as managing multiple processing units).

While you could think of the CPU device as a target device - a device which your OS will support management of - it would be a little unwise to do so. Thinking of it as a target would lead to the idea that methods of management applied to one CPU, cannot be applied to another. This is not the case. If you were to write a CPU manager to manage multiple processing units, you could probably use the same code on another CPU too. The difference is that your compiler would need to target a different CPU. The code remains logically the same (i.e. the CPU as a device, remains logically the same) but the actual outputted instructions change to the ones required for the specific CPU you targeted.

It is important to realise that the CPU as a target affects what the compiler produces. The CPU as a device affects what you program. If you are programming in assembler, then clearly the target CPU will also affect what you program, but the logic of what you program can (usually) remain the same.
For more information about the CPU as a device, see "CPUs as Devices" sections further down in this article.

### What is meant by "architecture"?

#### Definition
A CPU architecture is a CPU design (but this statement is hardly any help). A CPU architecture lays out designs for every aspect of the CPU. What this encompasses, of course, varies from CPU to CPU, which makes a definition rather difficult. However, we can say the following. A CPU architecture usually encompasses the following designs/specifications:

- Instruction Set
- Hardware features (e.g. registers, virtual memory)
- Software conventions (e.g. register uses)

#### Explanation
Looking at these areas in more detail:

- Instruction set - This is the set of assembler ops. This has been mentioned and detailed before in "Clarifying what a CPU is - Explanation". The CPU instructions and the instruction set are (to all intents and purposes) the same thing. The instruction set lays out precisely what each operation does, any side effects and any required or optional parameters (often in the form of registers) to the op. The instruction set also defines the identifiers for the registers available in the CPU.
- Hardware features - The hardware features define what the CPU is capable of doing. This ranges from what registers are available (and for what purposes), through to how IO ports are accessed, to how the Memory Management Unit is controlled. The basic hardware features of CPUs are fairly similar across all CPUs these days, meaning you need only really learn the instruction set to find out how to begin programming the basics. More complex features obviously begin to vary a lot, as they are what set different CPUs apart.
- Software features - The software features define how the CPU designers expect it to be used (and may also impose some practical restrictions that the compiler or hardware enforce). For example, software features might include specifying which registers should be used for return values from a function call. This isn't necessarily enforced by anything, so in theory you could probably work against the software design features, but you'd be foolish to do so. The designers probably knew what they were talking about! What is defined in the software features varies wildly and whether you have to stick to them is also variable. You will always have to read the specs for your specific target architecture to find out what they say for this section.

It is probably apparent that each of these areas is closely linked to the other. For example, the specific hardware registers each have names which are then used in the instruction set. Register uses are sometimes strictly defined, which also sets out which instructions can be used thus further defining the instruction set. Thoroughly understanding your target CPU's architecture will put you in good stead when it comes to programming the first bits of code related to booting your OS and initialising the CPU.

#### Alternative Uses
It should also be noted that the term "CPU architecture" may be used in other contexts, for example hardware engineers may use it to refer to the precise design and layout of one particular CPU and ignore the software features entirely.

### What are some general differences between architectures?

#### Little vs. big endian
A fundamental difference between architectures is whether they are little or big endian. This is something which often trips people up when converting examples between architectures. Little and big endian refer to the order in which the most to least significant bytes are stored in memory. In short, little endian means the least significant byte is stored at the lower address. Big endian means the most significant byte is stored at the lower address. For a fuller explanation, please see the following articles:

- [http://en.wikipedia.org/wiki/Endianness](http://en.wikipedia.org/wiki/Endianness)
- [http://www.cs.umd.edu/class/sum2003/cmsc311/Notes/Data/endian.html](http://www.cs.umd.edu/class/sum2003/cmsc311/Notes/Data/endian.html)

(All links were valid as of 10-09-2014)

#### Register sizes
Another common difference between architectures is register sizes. The maximum register size will affect (and possibly limit) the largest number of bytes you can operate on in one operation. You can expect that in a 32-bit architecture, the largest CPU register, (not FPU register), will be 32-bits wide. Similarly, in a 64-bit architecture the largest CPU register is likely to be 64-bits wide. (This may not always hold true, but it is a reasonable rule of thumb.) Most general purpose registers are also subdivided into smaller registers which are 16 and 8 bits wide. For example, on x86 the primary (A) general purpose register can be accessed as follows:

- EAX = 32-bits wide
- AX = 16-bits wide, Lower 16-bits of EAX
- AL = 8-bits wide, Lower 8-bits of AX
- AH = 8-bits wide, Higher 8-bits of AX

#### Register uses
What uses registers are assigned to vary depending on what features a CPU is attempting to support. All CPUs (with perhaps the exception of some specialist units) have general purpose registers that are used for any purpose the programmer desires. CPUs then have various special purpose registers. The following are possible examples of some:

- FPU registers - used by the floating point units (FPUs) for floating point calculations.
- Segment registers - used by the CPU for managing segmented memory.
- Descriptor registers - used by the CPU to store / load the address of descriptors tables used to provide data / guidance (not really instructions) to the CPU on how to process things. For example, these contain the Interrupt Descriptors Table which provides interrupt function addresses.
- Processor registers - used by the CPU for managing the current program execution. For example, the stack, base and current instruction pointers.
- Many more ... but you will come across these as you explore the architecture you pick.

#### Hardware access e.g. IO Ports
Another key difference between architectures is how you access other parts of the hardware, be they part of the motherboard or attached via sockets. The FPU and MMU are now often part of the CPU hardware and are accessed simply by using special registers and special CPU instructions. However, there are a number of standards for communication with attached devices, such as PCI(e) (on top of which also sits USB) and SATA (for hard-drives). However, both of these standards, along with many others, work somewhat independently of how data is actually transferred to and from the CPU and the data buses. In general, this transfer mechanism is called an IO Port. How IO Ports operate from the CPU's perspective depends upon the CPU and motherboard architecture. As an example, the x86 architecture offers the ability to map IO ports to particular memory addresses. You then use the special in/out operations to respectively write/read the ports. (Note: x86 also has memory mapped registers where data buses are mapped directly to address ranges but this is a further form of IO port system.)

#### Memory management
As has been mentioned briefly before, memory management also varies. Older CPU architectures used a segmented virtual memory system, but modern CPUs most often use a paged memory system. The advantages and disadvantages of the two are beyond the scope of this article. However, it should be noted that which option is supported by your target CPU (or possibly both options e.g. x86) can have a big impact on your OS design.

#### Instruction sets
As has been discussed in detail previously, the instruction set of a CPU architecture is very particular to precisely what the CPU supports. Hence, it varies from CPU to CPU. However, as you will see later in this article, the same basic set of instructions can be used across a wide range of CPUs. This will be discussed in more depth later.

### What are the main effects of these differences?
There are several effects of these differences which are covered briefly below, though an in depth discussion of the good/bad aspects of every difference would take more words than it is worth. You need only note the general points to be able to make a reasonably informed decision about which architecture you choose.

#### Ease of learning / Ease of use
The more specialist the CPU architecture you target, the harder it will be to learn. This is for several, broad reasons:

1. The instruction set is (probably) very specific / specialist which will reduce the likelihood of there being resources freely available online
2. Specialist hardware means specialist knowledge and probably a load of standards nobody has ever heard of. This further reduces the likelihood of there being a good pool of resources available to you.
3. Specialist means just that, specialist. It is likely that you will not be able to port code or concepts from other architectures to help you, so you will be very much alone.

On the plus side, specialist CPUs are less and less common and for the average (haha!) OS developer, you need only target popular architectures such as x86 and ARM. (Note: x86 and ARM architectures are discussed in more detail later.)

#### Power Consumption
The larger the instruction set, the more hardware will be required to support the instructions. In general this means more physical silicon which in turn means a higher power consumption. This is broadly the history of the battle between the ARM and Intel architectures, but this will be discussed in more detail later. Either way, the architecture you choose to support will have a big impact on the CPU power consumption. If you are programming for an embedded environment (such as watches or phones), this is a big deciding factor.

#### Speed
Larger instruction sets, more specialist ops, more registers, more sophisticated memory management, etc. All of these things will, if properly and fully utilised, make your OS run much faster. However, you will certainly spend a lot longer developing your OS, as you will have to spend time optimising it here, there and everywhere to gain the full benefit of the aforementioned features. So what you gain in runtime speed, you pay for massively in development speed.

#### Compiler Complexity
You probably don't care about this, unless you are planning on writing your own compiler - a task which is considered as challenging as writing an OS! A large instruction set or specialist ops or few general purpose registers will make writing a good compiler more challenging. This is because your compiler will have to pay much closer attention to what is happening to the data and how/when it can use specialist ops or reuse registers. A discussion of compiler design is not included in the FlingOS articles, but given the existence of GCC, MSBuild and numerous other open-source compilers for just about every language, you should be able to find one that works for you somewhere.

### Specific architecture examples
A few examples of widely available/well known architectures:

- x86 (inc. 32 and 64 bit versions)
- ARM (inc. 32 and 64 bit variants)
- MIPS (3)
- PowerPC

A more comprehensive list (ordered by name) can be found at [http://en.wikipedia.org/wiki/Comparison_of_instruction_set_architectures](http://en.wikipedia.org/wiki/Comparison_of_instruction_set_architectures) (Link valid as of 10-09-2014.)

### Final note before we move on
I should note at this point that I have talked very much as though you have a choice over the CPU architecture you choose to target. This is possibly helpful and misleading at the same time. In most cases you will be forced to target the CPU architecture you have available to you. That is likely to be either your development machine's architecture (most likely x86), or the architecture of a development device, such as the Raspberry Pi (ARM). If you plan on creating your own custom set of hardware, (which may be possible with Google's initiatives in plug-and-play designs), then you are in the fortunate position of truly deciding for yourself!

---

## Intel / AMD x86 Architecture

### Brief summary of key details

#### Origin - Intel
The following is a good article that covers the history of x86 better than my brief summary below: [A brief history of the x86 microprocessor](http://www.computerworld.com/article/2535019/computer-hardware/timeline--a-brief-history-of-the-x86-microprocessor.html).

The x86 architecture originates from the first Intel 8086 CPU (which itself was a 16-bit redesign of the 8008 and 8080 CPUs). The 8086 was a 16-bit CPU but later designs extended the x86 architecture to 32-bit and then 64-bits. AMD also adopted the architecture for their CPUs and did the work to extend the instruction set to 64-bits (which Intel later re-adopted). This has led to the instruction set being known as the Intel/AMD x86 Instruction Set since the (non-processor-model-specific) set of instructions has been co-specified by the two companies. Compilers often refer to the x86 32-bit architecture as IA32 and the x86 64-bit as AMD64 as these are the original/official names for the two architectures.

#### A lot of history = a lot of baggage (see CISC and Segmented memory)
The x86 processor is steeped in history and, because it has always been backwards compatible, the architecture contains some outdated features that are no longer used. Examples include segmented virtual memory which, in the majority of new OS'es, has been replaced with paged virtual memory. You should also note that x86 64-bit processors are fully compatible with 32-bit assembly code. So if you build an x86 32-bit OS, it will also run on x86 64-bit processors. This is useful when considering testing on modern PCs which now mostly come with 64-bit processors.

#### Standard for PCs
x86 processors have long been the standard processor architecture PCs since Windows is built for the x86. Linux now also has many builds for x86. Consequently, if you build an OS targeting x86 (32-bit) you will be able to test it on almost any PC you find. (Though you may have issues with BIOS/UEFI/alternatives that prevent your OS booting, but that is different issue.)

#### Now standard for Macs
The architecture has become so solidly associated with PCs that Apple switched Macs from PowerPC to x86 to increase compatibility. (Note: Apple fan-boys may never admit that Apple gave in to the PC on processor architecture, but that's mostly a matter of opinion and not one which I care about much...)

#### Complex instruction set (see CISC)
x86 follows a CISC (Complex Instruction Set Computing) design. For more information on what this means and its impact, please see later in this article.

#### Few general purpose registers
The x86 architecture has only 4 general purpose registers (which themselves have specialist purposes depending on the specific op). It also has a fair few specialist registers. Many vs. few general purpose registers is discussed further on. The x86 general purpose registers are called A, B, C and D. They have been mentioned previously in "General Differences Between Architectures: Register Sizes". The names of the different sized versions are listed below:

| Lo 8-bits | Hi 8-bits | Lo 16-bits | (Lo) 32-bits | 64-bits (if x64) |
|:---:|:---:|:---:|:---:|:---:|
| AL | AH | AX | EAX | RAX |
| BL | BH | BX | EBX | RBX | 
| CL | CH | CX | ECX | RCX | 
| DL | DH | DX | EDX | RDX |

These registers can be used for broadly anything you like except when you use specific ops (for example the "loop" op).

#### Little endian
Please note that the x86 architecture is always little endian.

#### Traditionally, high power consumption
The x86 architecture, as created by Intel and AMD, has traditionally had high power consumption which is largely the reason it has dominated the PC market but not penetrated the mobile or tablet markets. The reasons for its higher power consumption are discussed later in RISC vs CISC. However, since the x86 architecture is high power:

1. You (in my opinion) needn't trouble yourself too much with writing super-optimised code the first couple of times you try. You should focus more on readability and maintainability. The power of the CPU and the compiler will probably do a better job than any optimisations you try and add.
2. You (in my opinion) needn't spend hours supporting power optimisations for devices (such as sleep mode in USB). You are likely to spend a lot of time programming for no real benefit (even if you are targeting laptops - you are more likely to benefit from writing more useful drivers than power optimised ones!)

#### Easiest to learn 32-bit
It is advisable to learn the 32-bit x86 instruction set and use it extensively before you start on the 64-bit version. The 32-bit version is complex enough with enough specialist ops let alone the 64-bit version. The 64-bit version adds more specialist ops, extended registers and, if you adhere to the design advice closely, an annoyingly complicated calling convention (by comparison to the 32-bit version).

#### High number of resources and examples for x86
The x86 architecture has a significantly large number of resources (especially by comparison to ARM). As an OS developer this will be vital. Additionally, FlingOS currently only targets x86 so all resources and examples are written from an x86 perspective.

#### Easy to test
Since the x86 architecture is so prolific in PCs, testing code written for it is very easy. Real hardware is widely available and USB boot sticks are easy to create and update. Furthermore, virtual machines work on the same architecture as the host PC and, since most PCs have x86 in them, you can test x86 code on most PCs. Also, x86 emulators for Windows, Linux and Mac are widely available. Both virtual machines and emulators are freely available such as VMWare Player, Virtual Box and Qemu.

#### If you have any additional useful information (or just header titles)
Please submit them for addition.

### CISC architecture
As has been mentioned several times previously, x86 is synonymous with CISC architecture design. More information about the theory behind CISC and a comparison to RISC is provided later in this article.

### Specification sources
Links valid as of 2014-09-16

- [Intel x86 specs](http://www.intel.com/content/www/us/en/processors/architectures-software-developer-manuals.html)
- [x86 Instruction set listings](http://en.wikipedia.org/wiki/X86_instruction_listings#Added_with_80386)
- [x86 Instruction set reference site](http://en.wikibooks.org/wiki/X86_Assembly)

### 32-bit or 64-bit?
When deciding which version of the architecture to target, there are a few aspects to consider (most of which have been mentioned at least briefly before):

- 32-bit : Easiest to learn
- 32-bit : Maximum compatibility
- 32-bit : Has simpler calling convention (if you follow design/spec advice)
- 64-bit : Once you are very advanced or if you have special purposes
- 64-bit : Adds a lot of complexity, so only use it if you will really reap the benefits

### Links to x86 development tools
Links valid as of 2014-09-16

- [NASM compiler](http://www.nasm.us/)
- Virtual machines (if running on x86 hardware):
    - [VMWare Player](https://my.vmware.com/web/vmware/free#desktop_end_user_computing/vmware_player/6_0)
    - [VirtualBox](https://www.virtualbox.org/)
- Emulators (if not running on x86 hardware):
    - [Qemu](http://wiki.qemu.org/)
    
Please note: At the time of writing, FlingOS primarily used VMWare Player for testing.

---

## ARM Architecture

### Brief summary of key details

#### Origin - ARM
The ARM architecture originates from the first ARM CPU created by Acorn Computers (the design team later became what is now known as ARM Holdings). ARM was first created in 1985, the same year that the 32-bit version of x86 was introduced (sources: Wikipedia, "ARM Architecture" and "x86" pages, as of 2014-09-16). Prior to what we know as the ARM architecture, the ARM development team created the very successful CPU as part of the BBC Micro. ARM2 was the first ARM architecture CPU produced and became available in 1986. Since then, the architecture has gone through a number of revisions, but has broadly retained the simple instruction set and core design principles.

#### A lot of history = very little baggage (see RISC)
Despite ARMs long history, they have relatively little "baggage" in the architecture (unlike x86, for example). The ARM architecture has had three major advantages:

1. They had the opportunity to learn from the several years of mistakes that Intel and x86 had made with 16-bit to 32-bit changes and similar ideas.
2. The nature of the markets they were selling to (Apple, Mobile, embedded) meant they could afford to produce completely new revisions of the architecture which contained breaking changes compared to previous architectures. This meant they could ditch some of the "baggage" along the way and so have a comparatively less cluttered architecture.
3. RISC design principles keep things simple. Since the ARM architecture was designed to have simple single-step instructions, every op is easy to understand and use. There is also less dependency between ops due to their simpler nature requiring less setup prior to the op. Consequently, the architecture has remained fairly easy to understand and uncluttered.

#### Standard for embedded devices
ARM CPUs have become the standard for embedded, mobile and tablet devices, having a clear majority of the market in 2014. This is because of ARM CPUs' low-power, small-size and low-heat properties (which are a virtue of the RISC design principles). It is also worth noting, ARM was selected by the Raspberry Pi developers as their CPU again for its low-cost, low-power, low-heat and high support by Linux. (There may be other reasons but these are the obvious ones).

#### Simple instruction set (see RISC)
ARM is a RISC architecture (discussed later in this article) which means it has a simple instruction set, which makes it easy to learn, easy to write ASM for and easier to write a compiler for than its x86 32 and 64-bit counterparts. However, because of its simple (single-step) instructions, a lot more operations must be performed to complete the same task (in comparison to an equivalent implementation on a CISC architecture such as x86). This is discussed in more detail in RISC vs CISC later in this article.

#### Lots of general purpose registers
The ARM architecture has a lot of general purpose registers, some of which are grouped into slightly more specific uses. This is useful to assembler programmers and compiler programmers as it offers a lot of freedom when writing code. This reduces headaches when writing code, in comparison to x86, as less must be placed on the stack as temporary storage. This, in turn, improves performance as register access is, generally, faster than memory access.

#### Paged virtual memory
There are many variations of the ARM architecture since ARM have produced many CPUs for many different environments. However, the main 32 and 64-bit ones aimed at mobile, tablet, laptop and desktop computing support paged virtual memory. Note: This may not be true in all cases but it's a reasonable guide. If you think this is a bad guide, please let me know and suggest alternative, more accurate information.

#### What endianness?
As mentioned in the previous section, there have been many variations of the ARM architecture. The endianness is model specific and so is classed as variable. You should try to write your code in a generic way so far as possible. When it is not possible, you will need to check the model of CPU on which you intend to test (/run) your code.

#### 64-bit is simple extension of 32-bit so easy to learn either
Due to the simplicity of the RISC architecture, 64-bit is a simple extension of the 32-bit version which makes learning the 32 and 64-bit version equally easy. This is very different from, for example, the x86 architecture. This gives ARM an advantage over x86 from a developer's perspective.

#### Fewer resources and examples for ARM
Despite the ease of learning, ARM has far fewer resources for hobby developers. This is probably due to the fact that x86 is easier to develop for, so hobby OS'es target x86 so resources are created for x86 development. This is somewhat of a perpetual cycle which ever reinforces the x86 development resources. ARM, however, have caught a lucky break with the Raspberry Pi including it, forcing lots of developers to reconsider their choice. Combined with the growing mobile and tablet markets, which largely contain ARM, (hobby) developers are slowly shifting focus to ARM from x86.

#### Harder to test
ARM is significantly harder to test on for three primary reasons:
1. ARM-based hardware is pretty hard to get hold of cheaply (ignoring the exceptional Raspberry Pi).
2. Without ARM hardware, you are stuck with using emulators not VMs which are slow and less convenient to use.
3. Since every ARM processor is different and not all are ASM compatible, you have to develop for one specific branch of processor which is more constraining than x86.

(Also, it's a lot cooler to be able to boot your OS on anyone's PC than messing around with (and probably damaging) their phone.)

### RISC Architecture
As has been mentioned, the ARM processor is a RISC architecture, which is discussed in more detail later.

---

# RISC vs CISC

## Acronym meanings
Reduced Instruction Set Computing (RISC) and Complex Instruction Set Computing (CISC) are opposing design ideologies applied to the design of computer architecture.

RISC follows the general principle of defining as simple instructions as possible to keep each instruction single-step making them run very fast. CISC, however, follows the general principle of defining multi-step, specialist ops wherever possible to create a complex but fast instruction set. There are no precise definitions of what constitutes RISC and CISC, nor where the boundary between them lies. The two concepts are best understood in tandem by comparing their differences.

Examples of RISC designs are ARM and MIPS processors. An example of CISC design is the x86 processor. ARM vs. x86 is a very similar but subtly different debate discussed after this RISC vs. CISC debate.

## Key differences between the two
(Please note: op = operation = instruction)

- RISC ops are single-step, CISC ops are multi-step.
  
  RISC operations are single-step meaning they only do one thing such as a single load from memory or a single numerical operation like add. CISC operations are multi-step meaning they do complex processes such as load a value from memory, add it to a register and store the result all in one overall operation.
- RISC ops require fewer clock cycles than CISC ops.
  
  CISC ops are multi-step so take longer to run with more sub-operations than RISC's single-step ops. This means CISC ops take more clock cycles to run than RISC ops.
- CISC ops accomplish more.

  CISC ops are multi-step so complete more in one single operation than a single RISC op. This means fewer CISC ops are required to complete an overall task than the equivalent required RISC ops. This offsets the extra time a single CISC op takes compared to a RISC op. The two designs are a trade-off between instruction speed and instruction efficiency. This has been neatly summarised by this equation:
  
      (time / program) = (time / instruction) * (instructions / program)
      
  (Adapted from [http://cs.stanford.edu/people/eroberts/courses/soco/projects/risc/risccisc/](http://cs.stanford.edu/people/eroberts/courses/soco/projects/risc/risccisc/), 2014-09-16)
- RISC ops require fewer transistors than CISC ops.

  A typical RISC op requires far fewer transistors (in hardware) than CISC ops because of their reduced design. This means RISC ops take up less physical silicon and less power (which also means, they create less heat).
- RISC ops are (usually) encoded in a uniform format, CISC are not.

  Since RISC ops are all fairly simple, requiring a minimum of parameters, every op can be encoded in a small amount of data thus meaning it is inexpensive to encode every op in the same amount of data. For example, some ARM processors encode all operations as 32-bits of data. CISC operations use anything from 0 to many parameters and the nature of complex ops means that CISC tends to be encoded using a variable op-size format. Overall, this means RISC uses more program data than CISC does due to:
  
    1.  Total size of the same number of RISC ops is, on average, slightly bigger than or the same as CISC ops (there is some debate on how to measure this).
    2. Total number of ops for RISC is much greater than CISC (this is well-known and accepted) meaning the number of encoded ops is much greater so the total data size is much greater.
- RISC has simple addressing modes, CISC has complex modes.
  
  RISC ops are simple, with only a few operations able to perform load / store operations. This means that addressing modes are very simple as you are either accessing a register, or the op treats the register value as an address (but that decision is based on the op not the parameter). CISC, on the other hand, has more complex addressing modes where parameters can be treated as registers or addresses to memory along with variations and combinations on top of this. The overall result is CISC has complex sets of addressing possibilities. This can make CISC assembly code complex to read, difficult to understand and prone to errors.
- RISC has fewer hardware data types.

  The simpler nature of RISC ops means fewer hardware data types are required by the architecture. This makes assembly code simpler to understand, but does mean software data types are even more abstracted from the hardware representations.
- RISC requires only instruction pointer to describe state, CISC requires hidden state information.
  
  RISC ops are single-step and simple so there is (virtually) no temporary caching of state during an operation. This means you could restart an operation without requiring any state restoration. This means that the only thing required to know the state of a program in a RISC architecture is the instruction pointer and register values. CISC, on the other hand, has lots of state caching during long, complex operations which means that you cannot restart an op without restoring values from the temporary data stores. However, these hardware data stores are usually not accessible to the developer. This means that even if you know the instruction point at a given moment, you do not know the full state of the processor and by extension, the program.
  
## Brief note
The following briefly summarise the advantages and disadvantages of RISC vs. CISC from my research of the topic. This topic has been highly debated over the years and there are many articles available through a simple search of "RISC vs. CISC" that give a more in-depth discussion (and justification) of the various points.

## Advantages of RISC

- Architecture and assembler is simpler to learn
- Assembler is easier to read
- Can have a simpler compiler
- Requires less silicon
- Thus less power and less physical space
- And also less heat, meaning no heat sync (which are not possible in embedded, mobile and tablet applications)

## Advantages of CISC

- Can do complex data handling faster / more efficiently
- Requires fewer lines of assembler (improves compile time & efficiency)
- Easier for programmer to program more complex data handling (in assembler)
- Requires fewer general purpose registers

## Disadvantages of RISC

- Has simpler instructions thus requiring more lines of assembler for the same, overall complex operation
- Can be less efficient compared to a CISC architecture optimised for your task
- More onus on the programmer to figure out how to organise complex data handling
- Requires many general purpose registers

## Disadvantages of CISC

- Architecture and assembler is harder to learn
- Assembler can be harder to read
- Requires a more complex compiler to really make use of the advantages of CISC
- Requires more silicon
- Thus more power and more physical space
- And more heat is produced, requiring a heat sync which makes the processor bulky

## Conclusion
Here follows my own conclusions from the RISC vs. CISC debate. I have found I agree with the widely popular opinions:

- RISC is simpler, easier to learn and use.
- RISC is usually lower power.
- CISC is faster, more powerful.
- Both are useful for a given specific purpose. You must pick the correct architecture for the job.
- RISC is (slightly) more recent and generally liked more so is perhaps better.
- However, x86 still more common than ARM in PCs so CISC is more accessible.
- Overall, I would prefer RISC if it weren't for the practical difficulty to develop for it.

It should be noted that the top-end RISC CPUs and top-end CISC CPUs have both borrowed ideas from each other's designs, meaning top-end RISC and CISC CPUs are almost indistinguishable in terms of RISC/CISC. It has become increasingly meaningless to say a CPU is RISC or CISC. The only true remaining CISC architecture is the x86 CPU, which Intel appear to be trying to replace as they push into mobile and embedded markets.

---

# ARM vs x86

## Drawing from RISC vs CISC
The RISC vs. CISC debate is largely ARM vs. x86 in the modern, high-power devices such as mobiles, tablets, laptops and, of course, PCs. ARM broadly, wins for low power and space efficiency but x86 wins for high power and performance.

## Brief Note
The following is a brief summary of the main points commonly put forward in the x86 vs ARM debate.

## Advantages of ARM

- All the advantages of RISC.
- In the Raspberry Pi, which actually breaks down the high-cost/low-accessibility barrier.
- Rapidly growing as mobile and tablet become prolific so supporting it is very good and future-proof.

## Advantages of x86

- All the advantages of CISC.
- It's in every PC (and many Macs) worldwide making it easy to test and develop for.
- Lots of development resources, examples and tutorials.
- Virtual machines are faster.
- Easier to develop for initially.
- More compilers.
- You can develop x86 on any platform including Windows. Developing for ARM on Windows is still a little difficult, primarily due to lack of tools for testing/development which support ARM.

## Disadvantages of ARM

- All the disadvantages of RISC.
- Hard to access hardware (excl. RPi).
- Hard to set up initially.
- Often must use emulator or you're stuck with Linux on an ARM chip which not all developers like.
- Development tools often limited to Linux only.

## Disadvantages of x86

- All the disadvantages of CISC.
- More complex to learn advanced operations so you can't really take advantage of the x86 CPU.
- Soon to be outdated if you stick to only 32-bit. Intel and AMD are likely to move further away from x86 (32 or 64-bit) as they attempt to penetrate the mobile, tablet and embedded markets.

## Conclusion
x86 wins only because it is currently easier to develop for, but if you have the time & effort, ARM would be well worth learning.

---

# Assembler Syntaxes

## General
There are a number of different syntaxes used for writing assembly code. Which you use is determined by your compiler. The syntax used by FlingOS is primarily NASM (since FlingOS uses the NASM compiler). Which assembler you use is determined by your compiler. Which compiler you used is broadly determined by:

1. Does the compiler support the architecture you wish to target?
2. Do you want/need a compiler capable of cross-platform compiling?
3. Includes: 
    - Does the compiler integrate with your toolchain easily?
    - Do you like the compiler's ASM syntax?
    
(1) and (2) are necessary choices that you must make. (3) are choices which are entirely personal preference. Most assembler compilers have existed for a long time so are stable, well documented and will integrate with at least one environment very well.

## NASM, MASM, GAS
NASM (Netwide Assembler) is an x86 assembler which uses an Intel-like syntax. Alternative compilers/syntaxes include MASM (Microsoft Macro Assembler), which uses Intel syntax, and another is GAS (GNU Assembler) which is a cross-platform assembler used by the GNU project. It is worth repeating: NASM and MASM are x86 assemblers only. If you intend to target ARM, you will not be able to use NASM or MASM.

## Inline assembler
While writing your OS you may need (or just want) to write assembler in with your main code. High-level languages, such as C\#, do not permit this (for a variety of reasons). If your language does not support inline assembler, then working around it is left to you to do. If your language does support inline assembler (such as C), then which assembler syntax you use is, as before, dependent upon your compiler. You will need to look up the spec for your language implementation (such as MS-C) to find out how to write inline assembler in a way the compiler will understand.

---

# NASM

## Notes about NASM & FlingOS
As previously mentioned, FlingOS uses the NASM compiler and thus NASM syntax. Assembler samples are given in NASM syntax. If you are using a different syntax, you must be very careful when converting samples. Particular attention should be paid to parameter order, since a failure to switch parameters will entirely break code.

## A few examples

``` x86asm
; NASM, x86 (32-bit)
; - - - - - - - - - 32-bit Addition - - - - - - - - - 

; This sample adds two numbers and stores the result over 
;   one of the first input

mov eax, 1      ; Load the first number to add
mov ebx, 1      ; Load the second number to add
add eax, ebx    ; Add the two numbers, result stored in eax
```

``` x86asm
; NASM, x86 (32-bit)
;  - - - - - - - - - 32-bit Port Output - - - - - - - - - 

; This sample outputs a 32-bit value to an IO port
mov eax, 0xDEADBEEF     ; Load the value to send
out 0xF0, eax           ; Output the value to port 0xF0
```

``` x86asm
; NASM, x86 (32-bit)
; - - - - - - - - - Conditional Branching - - - - - - - - - 

; This sample loads a number then tests whether it is greater than a specified number.
;   If it is greater, it stores 0 in the primary general purpose register
;   If it is equal or less, it stores a 1 in the primary general purpose register.
; Execution then continues at a common point.

; Note: The greater-than test is a signed test

mov eax, 1      ; Load the number to test against
cmp eax, 10     ; Compare the test number (in eax) to the condition value (10)
jg GreaterThan  ; Jump to the GreaterThan label (defined below) if eax > 10
mov eax, 1      ; Less than or equal so store 1
jmp CommonCode  ; Then jump to the common code (skipping then GreaterThan code so we don't overwrite the
; 1 that we just stored).
GreaterThan:    ; Define the GreaterThan label - this defines a point in the program we can jump to.
mov eax, 0      ; Greater than so store 0
; No need to jump to CommonCode - execution will continue to the next line regardless.
CommonCode:    ; Define the common code label
; Execution continues here
```
                
## Links
All links valid as of 2014-09-17

- [NASM Website](http://www.nasm.us/)
- [Netwide Assembler (Wikipedia)](http://en.wikipedia.org/wiki/Netwide_Assembler)
- [NASM Examples (Loyola Marymount University)](http://cs.lmu.edu/~ray/notes/nasmexamples/)
- [Bare Bones with NASM (OSDev.org)](http://wiki.osdev.org/Bare_Bones_with_NASM)

---

# CPUs As Devices

## Overview
The CPU as a target architecture has been discussed fairly thoroughly above. This section covers the CPU as a device.

While your operating system is running, the CPU is the primary device you must manage. Most of the time it acts transparently or as part of your program (e.g. stack and instruction pointer management). Occasionally, however, you will need to give the CPU specific control instructions to tell it what to do or what to set up. Examples of these cases are when the OS first boots, setting up the segmentation registers and later when you program process switching, handling paged virtual memory.

The following sections provide details of:

- General features supported by the majority of CPUs which are useful (or necessary) when managing the CPU.
- x86 specific code samples for some of the general features.

## General features

### ID
Most CPUs support some form of ID, either in the form of an operation or a register. A CPU ID can take various forms. One of these forms is a revision number, in which case you will need to check the manufacturer website for specific version information. Another form is a bit-field, which supplies information about what the CPU supports. You will need to look at the manufacturer website to know what each bit means, but after that you can write generic code that works across different (including new) CPU versions without adding extra CPU IDs.

### Halt / Reset
All CPUs support a Halt/Reset feature that allows you to programmatically stop or restart the CPU. If you stop the CPU, the only events which will cause the CPU to resume are:

- A non-maskable interrupt occurs.
- An unmasked maskable-interrupt occurs.
- Power to the CPU is cycled.

CPU halt is useful for implementing CPU sleep functionality. You can stop the CPU from executing until a timer interrupt occurs, thus allowing you to wait for a short period of time without using a (busy-)loop.

CPU reset is implemented in a number of ways. For example, the PS2 keyboard reset line has been a long-standing way to hard reset the CPU of a PC. Reset is also achieved through reset ops or the ACPI features.

### Speed
Most modern CPUs support a way of determining the speed of the CPU. This speed can be in clock-frequency, clock-interval or potentially some other measurement. The speed of the CPU is useful in time-based functions, such as CPU time allocation and for outputting to the user, so they can show it off to their mates ;)

### Multiple CPUs / Multicore
For CPUs which have multiple cores or which are designed to work in parallel with other CPUs, functions for synchronising cores, transferring data consistently and other necessary cross-core / cross-CPU functions will be provided.

## x86

### ID
The CPUID op is not supported on all x86 processors. You can, however, reliably test for support. (I would expect most modern x86 processors to support it.) The CPUID op takes one parameter, the function selector. The parameter is stored in EAX prior to the CPUID call. The result of the CPUID call is stored in EAX, EBX, ECX and EDX. More detail for each function is provided below.

The following is a list of all the basic CPUID functions and the required values to use them. The information is valid for both Intel and AMD specifications (links provided below). Other feature numbers exist, but they are model-specific. Please refer to the respective specifications for details of the model-specific features.

- Max Feature Index/Vendor ID String

  EAX = 0x0, Returns Max Feature Index and the Vendor ID string in EBX, EDX and ECX.
  
  The Max Feature Index is the highest basic feature number that can be requested. The value is returned in EAX. Note: Higher feature numbers can be requested but they are extended features which are model specific. The Vendor ID string characters are stored as 1-byte ASCII in the following order: EBX [LSB ... MSB], EDX [LSB ... MSB], ECX [LSB ... MSB].
- Version / Features
  
  EAX = 0x1, Returns Version info in EAX, misc. info in EBX and a bit field in ECX and EDX indicating the CPU's features.
  
  For details of the Version, Misc and bit field values, please see the Intel or AMD CPUID specs. Links provided below.
- Monitor / MWait

  EAX = 0x5, Returns values in EAX, EBX, ECX and EDX.
  
  Please see the Intel spec (easiest to read) for details of the bits / values. Links provided below.
- Thermal and Power Management
  
  EAX = 0x6, Returns values in EAX, EBX, ECX and EDX.
  
  See the respective specifications for more info. Links provided below.
- Structured Extended Feature Identifiers
  
  EAX = 0x7, Returns values in EAX, EBX and ECX.
  
  See the respective specifications for more info. Links provided below.
- Processor Extended State Enumeration (ECX = 0)

  EAX = 0xD, ECX = 0x1, Returns bit fields and values in EAX, EBX, ECX and EDX.
  
  See the respective specifications for more info. Links provided below.
- Processor Extended State Enumeration (ECX > 1)

  EAX = 0xD, ECX = n for n > 1, Returns bit fields and values in EAX, EBX, ECX and EDX.
  
  See the respective specifications for more info. Links provided below.
  
The following example shows how detect if CPUID is supported:

``` x86asm
; NASM, x86 (32-bit)
; - - - - - - - - - - - - - - - - Detect CPUID Support Example - - - - - - - - - - - - - - - -
; Original source: http://wiki.osdev.org/CPUID#CPU_Features

; Returns 1 if CPUID is supported, 0 otherwise (ZF is also set accordingly)
;   This method uses probing of bit 21 (ID bit) to detect if the CPUID instruction
;   is supported.

pushfd                 ; get
pop eax
mov ecx, eax           ; save
xor eax, 0x200000      ; flip
push eax               ; set
popfd
pushfd                 ; and test
pop eax
xor eax, ecx           ; mask changed bits
shr eax, 21            ; move bit 21 to bit 0
and eax, 1             ; and mask others
push ecx
popfd                  ; restore original flags
ret
```
                
The following example gets the Vendor ID string:

``` x86asm
; NASM, x86 (32-bit)
; - - - - - - - - - - - - - - - - CPUID Vendor ID Example - - - - - - - - - - - - - - - -

mov eax, 0x0 ; Set feature number to 0
cpuid        ; Call CPUID op

; The result of this instruction on an Intel processor would be the string GenuineIntel
; stored in EBX, ECX, EDX along with the Max Feature Index in EAX.
;
; The values in EBX, EDX and ECX would be: (Note the register order!!)
;       MSB         LSB
; EBX : 'u' 'n' 'e' 'G'
; EDX : 'I' 'e' 'n' 'i'
; ECX : 'l' 'e' 't' 'n'
```
                
Here are some useful links related to x86 CPUID op:

- [Sandpie.org - An exhaustive list of features](http://sandpile.org/x86/cpuid.htm)
- [Intel CPUID (and other ops) Specification](http://www.intel.com/content/www/us/en/architecture-and-technology/64-ia-32-architectures-software-developer-vol-2a-manual.html)
- [AMD CPUID Specification](http://support.amd.com/TechDocs/25481.pdf)

### Halt
The x86 halt op is very simple. It halts the CPU indefinitely until either an interrupt is triggered or the CPU power is cycled (i.e. the CPU is externally reset).

Please take special note of the following regarding interrupts. You must disable maskable interrupts before using the halt op if you intend to try and permanently halt the CPU. If you do not disable all interrupts, the CPU will restart as soon as an interrupt occurs. You cannot disable non-maskable interrupts.

The Halt op allows you to create a proper sleep function if you have timer interrupts enabled, since the CPU will properly halt until a timer interrupt occurs. When the timer interrupt occurs, you know (approximately) how much time has passed and can either continue execution or go back to the halted state. A proper sleep like this is preferable to a busy-wait (using an empty loop) since it consumes much less power. Note that if you are in a threaded environment, your sleep function will work differently. Instead, your sleep function should inform the scheduler not to allocate the thread execution time until the sleep timeout has expired.

The following example shows how to permanently halt the CPU:

``` x86asm
; NASM, x86 (32-bit)
; - - - - - - - - - - - - - - - - CPU (Permanent) Halt Example - - - - - - - - - - - - - - - -

Halt:    ; Label to go to if the CPU does restart due to non-maskable interrupt
cli      ; Disable maskable interrupts
halt     ; Halt the CPU
jmp Halt ; If the CPU restart, jump back to the halt
```

### Reset
The x86 processor does not have a specific reset op. Instead, you must use one of four methods (in order of best to worst):

1. ACPI - Implement ACPI support and use the shutdown/restart functions. This is overly complex and laborious making it beyond what even the most advanced hobby-OS developers can be bothered to do.
2. Keyboard reset line - A long-standing method that is still very reliable is to pulse the keyboard reset line, which resets the CPU.
3. Tell the user - Tell the user to press and hold the power button or something.
4. Cause a triple fault - This is a fatal exception which causes a shutdown cycle to occur, but it's pretty nasty and will confuse users a lot. Also, causing a triple fault can actually be quite difficult if your OS is programmed decently (i.e. programmed to handle exceptions properly) and you don't want to hack your code to death. Basically, just don't use this method of reset.

The following examples shows how to pulse the keyboard reset line:
                      
``` c
/* C */
/* - - - CPU Reset using Keyboard Reset Line Example - - - */
/*        Adapted from: http://wiki.osdev.org/Reboot       */

/* Keyboard interface IO port: Data and control
 * READ:   status port
 * WRITE:  control register 
 */
#define KBRD_INTRFC 0x64

/* Keyboard interface bits */
#define KBRD_BIT_KDATA 0  /* Keyboard data is in buffer (output buffer is empty)  (bit 0) */
#define KBRD_BIT_UDATA 1  /* User     data is in buffer (command buffer is empty) (bit 1) */

#define KBRD_IO    0x60 /* Keyboard IO port */
#define KBRD_RESET 0xFE /* Reset CPU command */

#define bit(n) (1<<(n)) /* Set bit n to 1 */

/* Check if bit n in flags is set */
#define check_flag(flags, n) ((flags) & bit(n))

void Reset()
{
    uint8_t temp;

    /* Disable all (maskable) interrupts */
    asm volatile ("cli");

    /* Clear all keyboard buffers (output and command buffers) */
    do
    {
        /* Empty user data */
        temp = inb(KBRD_INTRFC);
        /* Check for keyboard data */
        if (check_flag(temp, KBRD_BIT_KDATA) != 0)
        {
            /* Empty keyboard data */
            inb(KBRD_IO);
        }
    }
    while (check_flag(temp, KBRD_BIT_UDATA) != 0);

    /* Pulse the CPU Reset Line */
    outb(KBRD_INTRFC, KBRD_RESET);

    /* If that didn't work,  execution will continue here. */
}
```

# Further Reading
The following links are additional references to those supplied within the main body of the article.

- [OSDev.org - CPUID](http://wiki.osdev.org/CPUID)
- [OSDev.org - Reboot](http://wiki.osdev.org/Reboot)
- [Wikipedia - Intel/AMD x86 Architecture](http://en.wikipedia.org/wiki/X86)
- [Wikibooks - x86 Assembly & Opcodes](http://en.wikibooks.org/wiki/X86_Assembly)
- [Bright Hub - ARM vs x86 Processors: What's the Difference?](http://www.brighthub.com/computing/hardware/articles/107133.aspx)
- [Stanford Education - RISC vs. CISC](http://cs.stanford.edu/people/eroberts/courses/soco/projects/risc/risccisc/)
- [ARM versus x86 - Considerations for the embedded segment](http://www.hectronic.se/website1/embedded/arm-versus-x86/arm-versus-x86.php)

# Revisions
The following is a list of all the revisions made to this document in order of first to last (oldest to newest).

- 25/09/14 - Initial version.
- 27/09/14 - Spelling and formatting corrections.
- 27/09/14 - Added x86 info / examples.
- 26/10/14 - Spelling and grammar corrections.
- 03/09/15 - Conversion to Markdown syntax.
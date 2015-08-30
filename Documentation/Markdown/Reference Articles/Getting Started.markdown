---
layout: reference-article
title: Getting Started
date: 2015-08-30 20:21:00
categories: docs reference
---

# Where to start

Want to create your own operating system? Or just to learn low-level development? To get you started with OS or low-level development, FlingOS has created a series of ***10 tutorial videos, each ~20 minutes long***. These videos will take you from only knowing how to program (in C or C#) to knowing all the basics of OS development and a having basic x86 operating system running on real hardware or in a virtual machine).

Once you've gone through the tutorials to get started with the basics, it's a good idea to decide on what platform you want to build an OS for or work out which platform you will be low-level dev'ing on. The easiest (and most common) choice is x86. The x86 architecture is found in every PC worldwide and all Apple Macs (since 2006). Alternative platforms (which are primarily embedded platforms) are the Creator CI20 (the MIPS-based development board from Imagination Technologies) and the Raspberry Pi (the most popular ARM-based development board).

---

# Next steps

Having decided on your platform, create a basic kernel that matches the feature set of the FlingOS Example Kernel (which you should have created by watching the tutorial videos). After that, there are many different routes you can take. To find out what they are and to decide which you would like to follow, please read the "Typical design routes" article. After you've decided your design route, you can begin to follow the steps for building your own OS.

Each design route starts with the same basic set of steps (as you should have already read). You can work on your OS at this stage without having a primary aim. Our advice is to follow the design route to create a basic kernel and set of interfaces. Following on from that you can create a basic set of drivers which will underpin your entire system. If you're looking to create a serious OS you will need to spend a decent amount of time planning, implementing and optimising the core kernel and drivers.

---

# And after that?

Once you've got a basic kernel matching your desired design route you can start to work towards your bigger goal. You'll need to decide on a realistic aim - something you stand a hope of implementing yourself in a year or two. Don't expect to be running a high-definition graphics system or OS rivalling Linux, Windows or OSX within even a few years. Having said that though, we recommend you decide on a second, unrealistic aim. Decide on a second aim that, while unrealistic, will keep you inspired (or, if you happen to complete your realistic aim, you'll still have something to work towards!)

Once you've set your sights, start reading around the end-goal. Particularly focus on working out what underpins what you're trying to create. Work from the top down until you can plan a low-detail route from your basic kernel to your goal being implemented. Understanding the requirements and layers of software (often called a stack) that sit between your basic OS and your goal will help you keep focused and develop efficiently.

With all that planning and knowledge gathering done, you can start to implement. Work from the bottom up. We highly recommend employing good software engineering practices (even if you just want a hobby OS) because OS development is very prone to bugs. For example, we highly recommend unit and behavioural testing. Furthermore, try to keep things split into neat, self-contained components which will keep dependencies traceable (which makes debugging much easier and massively improves stability). 

To implement what you need will require a fair amount of work. Fortunately, FlingOS is here to make your life easier. We've written (and continue to actively write) articles about a wide range of topics. If you are looking to implement something, try looking at our list of articles first. If there's an article, you can read it to find out most if not all of what you need. Some topics are so large that there may be multiple articles to read for just one component. 

Can't find an article you need? Or find an article that is lacking? You can help us by contributing to articles. You'll need to research online elsewhere to find out what you need to know, but we'd love it if you just send us a link to what you found (or write articles yourself!) 

---

# FAQ & Common Problems

## I'm lost! There's too much and it's so complicated...
OS and low-level development is a huge field. It can take years to become proficient in even one small aspect. Fortunately, if you're prepared to stick at it for a bit and read plenty, you'll soon get the hang of it. Try following the tutorials and reading the articles recommended above. If you're still lost, try looking at the [OSDev.org website](http://wiki.osdev.org) which has some more tutorials and their forums can help provide guidance. Ultimately, you will need to realise two things:

1. ***You must be focused.*** You must decide on an aim but be prepared to do a lot of wider reading around many topics. It helps to read a lot, then go back and re-read and repeat that several times. By the third time you will probably find things start to piece together (particularly if you write things down or draw diagrams).
2. ***Don't expect to know everything*** - there is too much to know. This is perhaps the best and worst thing about OS and low-level development. It is such a huge field of both work and research that you will always have questions. The trick is to decide which are questions you *need* to answer in order to proceed and which are just curiosity. This comes back to being focused. Spending some time on curiosities is fine but too much will mean you never progress.

## I don't want to read. What can I do instead?
At the moment there aren't a great deal of alternatives. You could try listening to articles using Text-to-Speech. YouTube has some series of videos from University lecturers (which tend to be pure theory and hard to do anything practical from) and one or two people who just create a video chock-a-block with unexplained code. FlingOS is continuing to produce high-quality videos on a focused range of topics but we can't cover everything in video format. Ultimately, reading is the most common, detailed and efficient way to learn this stuff.

## Is there a pre-built kernel I can work from?
Yes there are several. You can go online and find them by Googling. We aren't going to link you to them though because we don't see the point. Our articles can provide you with information required to write drivers for any system (since many of the technologies covered are cross-platform). Furthermore, if you want to just use an OS that someone else built, what is wrong with a stripped down version of Linux (e.g. embedded Linux distros)? Or just write drivers for your current operating system? 

## Why are people telling me I'm wasting my time?
This happens a lot when you first start out in OS dev. There are three groups of people who say this:

1. Those who don't care about low-level or OS dev and just stick to high-level work on existing operating systems. These people will probably never understand us so-called "crazy low-level" people.
2. Those who are much more experienced in OS dev. Unfortunately, many of these people forget they too were once inexperienced. Many seem to believe the only way is through a University (and in C). They're wrong and it's totally fine to ignore them. It's totally fine if you're just a high-level developer at the moment. Don't bother arguing with these types of people either though. 
3. People who actually know you and/or your ability. You might want to listen to these people. OS dev is the hardest thing a programmer can tackle (possibly second only to compilers). If you;re outside of a formal teaching program (such as in a University) and if you aren't already an intermediate, advanced or expert programmer in a C-based or C-origin language, you are well-advised to spend a year or two learning before tackling OS or low-level development.

## Why do people online keep directing me to Google?
Mostly because that's the only way to find anything. If you ask on a forum, it is rare that the person responding has first-hand experience or "off-the-top-of-their-head" knowledge. They probably don't have the time to sit down and write an article explaining to you either. Using Google to find stuff that others have already written is your best and only (real) option. 

In our experience, everything you need (or want) to know can be found online. However, it can be very hard to find and sometimes takes weeks (or months) to figure out. FlingOS exists to tackle this problem but we don't write articles about things we haven't done ourselves (because otherwise our content wouldn't be of such good quality). So it's taking us time to implement and then write articles about everything we'd like to include.

## Why is there no sample code?
Most OS and low-level dev is done within the closed walls of companies. This means that sample code for any given device or technology is pretty hard to come by. FlingOS has a significant codebase which can be used as sample or reference code (but it is distributed without warranty or liability and should not be used blindly). Sometimes you can also find someone else's hobby OS project to help you out. You can help others out in the future by contributing to the FlingOS project (even by just providing links to resources you uncover!)

## Why is online documentation so bad?
Largely because developers are lazy and often poor technical writers. Very few developers will (or can) take the time to document things openly and freely online (many choose to do so as part of expensive books). Furthermore, many developers are not very good writers (or English is not their native language) which can mean articles are poorly written, confusing and often misleading purely due to incorrect use of language.

## I don't want to pay for information. What should I do?
Sadly, a lot of the best sources of knowledge about OS and low-level dev are held in expensive books or behind pay walls. You can't get around this (other than perhaps by finding deals of (legal) free versions). You will just have to research online further to find the knowledge for free. Once you've found it, you can make life easier for others by sending a link to FlingOS or contributing to FlingOS by writing.

## Why are specifications so hard to read/useless?
Companies write specifications as technical references (often from a hardware not a software) perspective. Specifications are designed to tell you anything and everything about the features or technical points of a technology but without bias. They are almost never written to explain how to use a technology nor how to implement software for it. It's all very well knowing all the features but companies need to realise they need to write documentation that explains how the features fit together and what control sequences are required to use the technology. If you find good documentation about something, please send a link to FlingOS so we can pass it on to others.

## Is there no alternative to specifications?
Unless someone else has written an article, written sample code or produced a video, there is no alternative. Working from a specification is hard but some manufacturers and IP companies are prepared to offer help if you email them. As a last resort you can try looking at the Linux source code for sample drivers but it is very hard to read and often has high levels of interdependency on Linux features.

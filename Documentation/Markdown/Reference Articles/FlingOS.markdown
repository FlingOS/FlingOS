---
layout: reference-article
title: FlingOS™
date: 2015-09-06 20:05:00
categories: [ docs, reference ]
description: Describes the FlingOS™ operating system.
---

# Introduction

This article is a heading topic to contain articles that specifically relate to the FlingOS™ operating system. The articles in this section are primarily aimed at developers of FlingOS but may be of interest to a wider audience.

*The remainder of this article describes how to set up your development environment for working in FlingOS. However, the FlingOS operating system is undergoing big changes at the moment with the addition of MIPS support and drive towards a better structure and more advanced features. As such, some details of this document change weekly so it may be a tad out of date. Please just contact Edward Nutting directly for support.*

---

# Setup for Development

The following page describes the steps to get your development PC set up for developing FlingOS.

If you are going to be developing mockups, you should not be following the tutorial on this page.

## Become a developer

1. You should already have got in touch with Edward Nutting or another FlingOS coordinator to register your interest in developing FlingOS.
2. If you haven't already, sign up to GitHub.com and send your username to the lead developer Edward Nutting to be added to the source control system and development team.
3. Follow FlingOS on Twitter and Facebook. Please also send your Twitter username to Ed Nutting (or follow via @EdNutting) so he can add you to the Developers list.
4. Add Ed Nutting on Skype. Username / email details provided upon request.
5. Sign up to the [FlingOS Community](http://community.flingos.co.uk)

At this point you should have access to all the bits of the FlingOS project you need to be able to develop the main operating system.

## Pre-requisites
Ensure you have the following required software installed:

1. Visual Studio 2013 (any version)
2. Atlassian Source Tree
3. VMWare Player or VirtualBox or HyperVisor
4. ILSpy (Visual Studio plugin)
5. Skype (optional)

## Keep in contact

1. You should have installed Skype to keep in contact with the team. Please add the lead developer Edward Nutting and he will add you to the relevant group conversations. Feel free to contact him directly for help especially during setup.
2. Add Edward Nutting's email to your contacts and safe-senders list. Also add the other FlingOS email addresses (flingos@outook.com, contact@flingos.co.uk and github@flingos.co.uk).

## Install Sandcastle Help File Builder (SHFB)

1. Download SHFB from the CodePlex project page..
2. Start the installer program
3. Click "Next" until you reach the "HTML Help 1" page. Follow the instructions to install "HTML Help 1". Then click Next.
4. Ignore the Microsoft Help 2 Compiler. Click Next and press "Yes" to continue without it.
5. Install the Sandcastle Help File Builder and Tools
6. Install the SHFB Visual Studio package
7. Skip installing the MAML schemas.
8. Skip installing the MAML snippets.
9. Click Next through to the Finish page then click Close. SHFB is now installed.

## Environment Setup

- Environment Variables :
    - Add the following to your user environment variables:
      
      ``` bash
      MSBUILDDISABLENODEREUSE = 1
      ```
      
    - Make sure the following is set (in either system or user variables):
      
      ``` bash
      SHFBROOT = %Path to Sandcastle Help File Builder exe directory%
      ```
      
      Sample value: C:\Program Files (x86)\EWSoftware\Sandcastle Help File Builder\

## Clone and build the source

1. Clone the FlingOS source from GitHub using SourceTree. To do this, you will need to add the repository in SourceTree. Allow it to save your credentials. Please put your name as either:
    
    - Your Twitter username
    - Or your full name without any spaces
    
    Please remember that all commit messages and code are public, so must be "Politically correct"/polite! Commit messages should also be informative about the feature added, how and why or the issue fixed and how. Try to use proper English grammar (that means full sentences and punctuation).
    
    You can clone it to any local or network path and the path is allowed to contain spaces with the following requirement; your path must end in the following form: "PATH_TO_DEV_FOLDER\FlingOS\FlingOS\CLONE_TO_HERE".
    
    The reason for this duplication of folder/sub-folder name will become clear later.
    
    If later, during the build process, you receive a "path dependency" type of error, please contact Ed Nutting. There aren't any path dependencies, except if the OS is built in the wrong order. Your user account will, however, need read and write access to the "C" drive. (If you haven't got a C drive then...erm...all hell is going to break loose).
2. You should now switch to the "develop" branch then create a new branch from "develop". Call your new branch the same as your GitHub username. This will be referred to as your "personal develop(ment) branch".
3. Locate and open the solution file: "...\FlingOS\FlingOS\FlingOS.sln". The solution should open without any errors (aside from potentially Untrusted Location warnings which can be ignored).
4. The solution contains solution folders which clearly sub-divide the project into its respective areas. For now, we will just want to try and build a debug version of the OS.
  
    In the configurations drop-down select Debug (you should also see Release and Docs modes). Select the "AnyCPU" platform for now. 
5. To build the kernel, follow these steps (in this order!!):

    1. Open Kernel\Debug folder and build Kernel.Debug.Data followed by Kernel.Debug.Debugger.
    2. Open Kernel\Compiler folder and build Kernel.Compiler followed by Architectures\Kernel.Compiler.Architectures.x86_32 followed by Kernel.Compiler.App.
    3. Open Drivers\Compiler folder and build Drivers.Compiler.MSBuildTask
    4. Select the "x86" platform
    5. Then build the Kernel project (in the Kernel folder).
6. The OS should now build. This usually takes 80 seconds (on 1.7 to 2.5GHz processor), Expect at least one of your processor's cores to be maxed-out and high disk activity.
  
    In future you should only need to "rebuild" the Kernel project (or if the compiler is update, Rebuild to Drivers.Compiler.MSBuildTask project). Using Rebuild will be more reliable. If the build fails, you will have to use Rebuild as VS does not recoginse FlingOS Compiler errors as a failure (annoyingly).
7. In windows explorer you should now be able to navigate to "...\FlingOS\FlingOS\Kernel\Kernel\bin\Debug" within which there should be a Kernel.iso. If this is not the case, or you get a build error, contact Edward Nutting for help.

## Setup debug-mode VM in VMWare Player

1. The following steps will not create a release-mode VM. Details on setting up a release mode VM are provided on other pages or contact Edward Nutting for help.
2. Open VMWare Player
3. Select "File->New virtual machine..." from the Player menu.
4. Select "Installer disc image file" from the options then browse for the Kernel.iso file in "...\FlingOS\FlingOS\Kernel\Kernel\bin\Debug\".
5. Ignore the "could not detect OS" message then click Next. Ensure it says "Other" then click Next again.
6. Name the virtual machine "FlingOS - Debug". Change the VM path to "...\FlingOS\VMWare\Debug". A common path for storing files allows other developers to help more easily as they are familiar with the directory structure. Note: This folder is not inside the FlingOS repo folder, it's the one above that.
7. Click Next. Set the maximum disk size to 1GB opt for splitting the disk into multiple files. Click Next.
8. Click Finish. Your new VM should appear in the list.
9. Right click on the new VM and click "Settings".
10. In the Settings window, remove the Network Adapter and Sound Card devices. Then click "Add..." and select "USB Controller" (allow admin permissions if asked). Set "USB compatibility" to "2.0" and check "auto-connect". Set the RAM to 512MB and the number of CPUs (/cores) to 1.
11. Your debug-mode VM should now be set up. You should be able to select it and click "Play virtual machine" and see a load of text printed out. If you get one line of text from Syslinux, just direct input to your VM and hit the "Enter" key to boot the (default) OS (which is FlingOS).
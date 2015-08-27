---
layout: reference-article
title: Global Descriptor Table
date: 2015-08-27 21:24:00
categories: docs reference
---

# Introduction

- What is GDT
- GDT and LDT
- GDT legacy but still needs to be set up

## Scope of this article

- GDT (not LDT though mentioned)
- Not explaining segmentation in detail but will cover how GDT sets up segmentation

---

# History

- Segmentation
- Security 

---

# Overview

## What is the GDT?

## What is a segment?

## What is a descriptor?

## What is a selector?

## What are segment selector registers?

## What is a Descriptor Privilege Level?

## Why do segments exist?

## Why do I need to configure the GDT at all?

## Why can't I just use real/physical memory?

## Why can't I just use paging?

## How do I create a GDT?

## How does the CPU know where my GDT is?

## What's the Local Descriptor Table?

## What is different about the LDT?

## Why would I want to use the LDT?

---

# Software

## Overview

## Technical details
**Enumerations, classes, other such details**

- GDT layout
- GDT Entry format
- GDT Entry Flags
- GDT Entry Access
- GDT Pointer format

## Implementation details
**Methods, steps, etc.**

- Allocate & construct GDT
- Allocate & fill in GDT Pointer
- Load GDT pointer to CPU registers
- Update segment selectors

## Alternatives

- Paging

## Compatibility

- 64-bit compatibility?
- Paging and/or segmentation?

---

# Example Code

## Overview
TODO

## Download
TODO

---

# FAQ & Common Problems

- No Null descriptor
- Sporadic failure (not disabled interrupts)
- Nothing changed? (Not reloaded segment registers)
- General protection faults (GPFs)

---

# References

- https://en.wikipedia.org/wiki/Global_Descriptor_Table
- http://wiki.osdev.org/Global_Descriptor_Table
- http://wiki.osdev.org/Segmentation
- http://wiki.osdev.org/GDT_Tutorial
- http://www.jamesmolloy.co.uk/tutorial_html/4.-The%20GDT%20and%20IDT.html

*[acronym]: details

---
layout: reference-article
title: Interrupts
date: 2015-08-10 15:15:00
categories: docs reference
---

# Introduction

## Overview 
Interrupts, also known as exceptions, traps, gates or (in a debugging context) breakpoints, are an absolute essential to OS development. They are so critical that the vast majority of devices can't even be configured without them. This article will look at what interrupts are and, in a general context, how they are used.

## Scope of this article
Interrupts and how interrupts work varies a lot from architecture to architecture but the basic concepts remain the same. This article will cover general ideas about interrupts. Other articles (e.g. the Interrupts Descriptor Table or Programmable Interrupts Controller articles) cover architecture specific implementations of interrupts.

---

# Overview

## What?

## Why?

## How?

## Basic explanation

---

# History
Interrupts have a relatively straightforward history. They first came to the scene in 1951 in the form of hardware interrupts. The aim was to reduce amount of CPU time being wasted on polling external devices. By 1955 these had become proper Input/Output interrupts and by 1960 vectored interrupts had been developed. 

By 1960 the majority of systems had some support for interrupts. Throughout the 1960s deferred interrupt handling became increasingly popular and with this came the idea of interrupt coalescing. 1971 saw the first of what was to become hundreds of patents relating to interrupt coalescing (most of which started being produced by high speed network engineering companies during the 1990s and onwards). Techniques for preventing or reducing interrupt storms and were also patented during the 1980s and onwards.



---

# Hardware

## Overview

## Details : Internals

## Details : Externals

## Alternatives

## Compatibility

---

# Software

## Overview

## Basic outline

## Technical details
**Enumerations, classes, other such details**

## Implementation details
**Methods, steps, etc.**

## Alternatives

## Compatibility

---

# Example Code

## Overview

## Download

---

# FAQ & Common Problems

---

# References

*[acronym]: details
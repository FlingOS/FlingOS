---
layout: reference-article
title: Interrupts Descriptor Table
date: 2015-08-28 16:54:00
categories: docs reference
---

# Introduction

- What the IDT is

## Scope of this article

- IDT only
- IDT setup only not ISR handling
- Interrupts, ISRs and IRQs described separately

---

# History

- IDT vs IVT
- More detail in Interrupts, ISRs and IRQs articles

---

# Overview

## What is the Interrupts Descriptor Table?

## What is the Interrupts Vector Table?

## What are Interrupt Service Routines?

## What are Interrupt Requests?

## What are the types of interrupt gate?

## Why are there two different tables?

## How do I create an IDT?

## How does the CPU know where the IDT is?

---

# Software

## Overview

## Basic outline

## Technical details

##### IDT Layout


##### IDT Entry format


##### Gate Types


##### IDT Pointer structure


## Implementation details

### 1. Allocating &amp; constructing the IDT


### 2. Allocating &amp; filling IDT Pointer


### 3. Loading IDT Pointer to CPU Register


## Alternatives

- Polling

---

# Example Code

## Overview

## Download

---

# FAQ & Common Problems

- How to test
- 

---

# References

*[acronym]: details
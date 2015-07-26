---
layout: ref_article
title: Displays & Graphics
date: 2015-07-23 12:20:00
categories: docs reference
---

# Introduction

## Overview
Displays and graphics covers everything from putting a simple piece of text on the screen to the high-end, hyper-detailed graphics found in games to the HD (or even 4K) video watched on YouTube. Clearly, no single article could hope to cover all these areas in any depth so that is not what this article will attempt.

Instead, it is more important to understand a range of the basic designs and systems that go together to form a picture on the screen and how that picture can be continuously updated. This dates back to the early days of monochrome CRT screens so that is where this article will begin (from a historical perspective). Hopefully, by the end of it, the reader will have gained sufficient knowledge and understanding that they can begin to think sensibly about how to implement a display and possibly graphics driver and perhaps a video decoder driver (the three things being separate and distinct, as will be explained).

## Scope of this article
As stated, this article does not cover specific display, video or graphics technologies (except where they are used as useful examples or for historical understanding). Instead this article will cover the important points of the long history of digital pictures and also provide the knowledge and understanding of theoretical concepts which applied by a wide range technologies.

---

# Overview
It's important to understand a number of fundamental concepts when looking at displays, graphics and video. Particularly the fact displays, graphics and video are not the same thing.

## What is a display?
A display is the physical hardware and the software techniques for taking data describing an image and forming an actual image that a person can view. There are lots of techniques to do this not all of which involve pixels. A display takes in highly descriptive data about an image (e.g. all the colours of all the pixels, the colours and location of characters or even the location and height of points to raise on a braille display) and converts that into a physical output.

## What is graphics?
Graphics is the process of taking abstract data describing a view (e.g. a 3D model, some 2D sprites or an application's window layout) and converting that into image data which can be sent to a display. For example, graphics software and hardware might take in a 3D model, an array of textures and a set of lighting points and then use ray-tracing to produce a final 2D image. That 2D image is fed to a display which the display then shows to the viewer.

## What is video?
Video encompasses two distinct parts. The first part is the software technologies used to compress arrays of images (known as frames), stream them and then decompress them for output through a display. It also often incorporates audio (which the second type does not). This is technology used by online video providers such as YouTube and Vimeo and implemented in hardware with designs known as video decoders and encoders. The hardware is often built into a graphics card but please remember that video is distinct from graphics.

The second part of video is the transmission of display data from a PC to a physical display. In many ways it is essentially the same as the first part, in that it involves software and hardware for compression, transmission and decompression. However, the actual technologies used are often different. Also, the hardware will also include things like cable specifications (e.g. HDMI cables), which the first part doesn't.

## What are the basic display technologies?
There are a few basic types of display technology, only one of which I imagine the average reader will have thought about. These are:
- Pixel displays (e.g. LED, LCD, Plasma)
- Pin screens (e.g. Braille displays)
- Segment displays (e.g. as used in digital clocks/watches)
- Ink displays (e.g. Electronic paper)

Only the first of these really uses pixel displays. The rest may use a similar ideas but the data won't be RGB brightness values.

## What are the basic graphics technologies?
There are a few basic types of graphics technology in common use nowadays. These are:
- Pixel images (e.g. GIF, JPEG, PNG)
- Sprite graphics (largely hardware & software specific. Only widespread example: Tiles of Icons combined with CSS)
- Vector graphics (e.g. SVG)
- Anti-aliasing (specifically spatial anti-aliasing)
- 3D (inc. modeling (e.g. polygonal modeling, curve modeling and digital sculpting)):
    - 3D projection (orthographic or perspective)
    - Shading
    - Ray tracing
    - Texture-mapping

## What are the basic video technologies?
There are a few basic video technologies for each type. For the first type (media streaming/playback):
 - MP4 (incorporating H.264 and MP3)
 - FLV (flash video)
 - WebM
 - ASF
 - ISMA

 For the second type (video connections):
 - VGA (Video Graphics Array)
 - HDMI (High Definition Media Interface)
 - Display Port
 Older (rapidly less common) technologies include:
 - Composite Video
 - SCART (Syndicat des Constructeurs d'Appareils Radiorécepteurs et Téléviseurs)
 - S-Video (Separate Video / Super Video / Y/C)

## Why so many different technologies?

## How do they all fit together?

## So where to start?

### Pixels in software

### 2D

### Sprites and textures

### 3D

### 3D Projection

### Ray tracing

### Shading

### Anti-aliasing

### Incorporating animation


---

# History

## Introduction
While it may not seem necessary to know the history of displays, video and graphics, I can assure you it is. Like most areas of computing, the terminology and concepts have developed over time and are now baked into everything. Historical terms (even for things which have totally changed meaning) are everywhere so it's useful to know where they came from. History can also teach us many of the lessons learnt over time particularly with graphics, where there are unobvious cases that lead to bad results.


## Displays

### 1922 - Monochrome CRT

### 1954 - Colour CRT

### 1968 - LED

### 1969 - Braille

### 1971 - LCD

### 1995 - Full-colour Plasma

### 2003 - OLED

### 2004 - Electronic Paper


## Graphics

### 1950s

### 1960s

### 1970s

### 1980s

### 1990s

### 2000s

### 2010s


## Video

### Connectors

### Formats

### Encode and decode


---

# Hardware

## Overview

## Current display technologies

## Current graphics technologies

## Current video technologies

## Compatibility between hardware


---

# Software

## Overview


## Displays

### Types of display output

### Interaction with hardware


## Graphics

### Types of image

### Current graphics technologies

### Interaction with hardware


## Video

### Types of video

### Current video technologies

### Interaction with hardware


## Compatibility and Integration


---

# FAQ & Common Problems

---

# References

*[acronym]: meaning

---
layout: reference-article
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

## Caveats
1. This article does use simplifications - it has to. The aim is provide sufficient understanding, not 100% technical detail.
2. This article is forced to touch upon some areas of electronics and physics. Where it does so, it makes some assumptions and generalisations which I know are not strictly true but are more than a reasonable description.

In both cases the word "almost" is often applied to indicate to the reader that what is being said may not be 100% accurate, true or sufficiently detailed.

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
- 3D (inc. modelling (e.g. polygonal modelling, curve modelling and digital sculpting)):
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
In the pursuit quality (and usually to achieve that, performance) hundreds of compromises are made. For the many different applications and situations that displays, graphics and video are used in, different compromises are made. Here are just a few examples of the different compromises and situations to which they apply.

| Technologies     | Compromise / Differences     | Situations      |
| :------------- | :------------- |
| LED, Plasma      | Traditionally (though as of 2015 it may not be true) Plasma displays were able to produce much clearer dark/black images than LED. However, LED displays could consistently create brighter light/white images. | Plasma for home cinema, LED for public displays |
| PNG, JPEG        | Both are based on compressing and saving colours of pixels in an image but they have very different approaches. PNG uses a colour palette and saves data about large areas/blocks of an image that are the same colour. JPEG is a lossy compression format that uses the Discrete Cosine Transform (DCT). JPEG is good for images where the colour of neighboring pixels varies a lot. | JPEG is best for photographs, PNG is best for logos / web images |
| Ray tracing, Shading | Ray tracing uses virtual rays of light from light sources and traces them around in 3D space until they hit the "screen". After tracing thousands of rays, the final colour of the virtual screen is used as the image. Ray tracing is computationally expensive but produces highly realistic looking results. Shading uses objects (with surfaces) covered in colour. The colour is varied to give the look of shadows and 3D objects. It is computationally less expensive but produces less realistic (sometimes innacurate) results. | Ray tracing for movies, shading for games (though the two techniques are gradually merging / shifting usage) |
| VGA, HDMI        | HDMI basically replaces VGA. VGA was analogue, HDMI is digital. VGA had limited resolutions, frame rates and quality, HDMI has higher resolutions, frame rates and quality. Ultimately, HDMI has compromises but in almost every aspect it is less compromising than VGA so is thus better than VGA in almost all cases. This is an example of new technology replacing old rather than just adding another competitor. | &nbsp; |

## How do they all fit together?
Displays, graphics and video all fit together relatively nicely (when you ignore all the incompatible, proprietary interfaces at the technical level). Essentially it's all one big chain of components linked together. It looks roughly like this:

    [Application]-.-->Graphics pipeline (e.g. 2D, 3D rendering)----------------|--->Display controller--->(Video) Cable to display (e.g. HDMI)--->Screen
                  |-->Video pipeline    (e.g. MP4 file decode)---.-->(Video)---^
                                                                 |-->(Audio)------->Speaker controller

What this shows is how an application can use graphics to render 2D and 3D stuff along with video to play an MP4 file. The video is split into video and audio, with audio sent to the speakers. The video part of the MP4 file is merged (by the graphics card) with the application graphics (the image output) and then sent to the display controller. The display controller then uses video (described as type 2 above, e.g. HDMI) to send that to the physical screen.

So graphics and video (type 1) sit alongside eachother. Graphics combines with video to provide the input for display. Display then uses video (type 2) to transmit to the screen and the screen uses a display technology to actually produce the light which a human can see.

## So where to start?
To start working with graphics there are a number of concepts which must be touched upon to provide sufficient understanding for the reader to be able to read other technical documents. The following sections will begin to explain some of the basic terminology.

For starters I'll explain that a pipeline just means a set a functions which data is passed through in order. So a graphics pipeline (often shortened to pipe) just means a set of functions which graphics data is passed through. Such a pipe would consist of shading functions, texture functions, collision detection functions, and render functions that take the abstract graphics data and gradually transform it to produce the final image. Often (in graphics, video and audio at least) the specific functions are implemented in firmware on a specific graphics card, to get the best performance.

### Pixels in software
Pixels in software are distinct from pixels in hardware primarily because hardware pixels may not work in the way software pretends they do. So please pay attention to whether you are dealing with the software end of things or the hardware. Here we will describe pixels as software would describe them.

Pixels are a single dot of colour on the screen. The number of pixels which span the width and height of the screen define the resolution. Modern HD displays have a resolution of 1920x1080 pixels. Each pixel consists of three light components - red, green and blue. Mixing red, green and blue can form (almost) any colour. Each component (RGB) has an intensity value with 0 meaning off (no light - not black!) and 255 (max value of a byte) meaning fully on (max brightness/full intensity - not white!). If all the components are 0, you get black. If all the components are 255, you get white. If just the red component was 255 and the others 0, you get bright red. If all the components were 128 you would get a dull grey colour.

There are alternative ways of representing pixel colour (most of which get translated in RGB at some point). The few main ones are listed below.
| Name | Description |
|:----:|:------------|
| RGB  | Red, Green, Blue components as intensity values from 0 to 255 (normally) |
| HSL  | Hue, Saturation, Luminescence components. Hue from 0 to 360 (distance round colour wheel), Saturation from 0 to 100 (%age) and Luminescence from 0 to 100 (%age) |
| HSB  | Hue, Saturation, Brightness. Commonly confused with HSL but not the same. Notably Photoshop CS3 used a colour picker which used HSB but called it HSL |
| CMYK | Cyan, Magenta, Yellow, Key (black). Commonly used in printing process as it describes proportion of ink required for a point on a page. CMY are the secondary colours (where as RBB are the primary colours). Primary/Secondary colours with respect to light not paint! |
| YUV  | Y (luma), U (chrominance), V (chrominance). Alternative system RGB often used in video which takes into account human perception allowing compression or transmission errors to be masked more easily. |

Each of these colour systems may also be extended to include an alpha component (e.g. ARGB) which allows the specification of transparency. 0 alpha means opaque, 255 alpha means completely transparent so the colour is invisible and only what is behind is shown.

### 2D
2D graphics is conceptually fairly simple. You have an X-Y plane which is blank. Blank in a computing sense just means filled with all one colour. Often this is black but it can be set to any colour you like. During debugging, bright red or blue is often used to make "background leaks" very obvious. The same technique is used in 3D graphics.

2D graphics makes used of the concept of painting. To paint an area, conceptually means to wipe a brush across the entire area. A brush is a thing which defines how to transform the starting value of a pixel to the end value. There are three main types of brush.
| Name     | Description     |
| :------------- | :------------- |
| Solid Brush    | Ignores the existing colour of a pixel and just replaces it with a solid colour. Used to fill pixels with a set colour.       |
| Gradient Brush | Ignores the existing colour of a pixel and just replaces it with a solid colour. The brush keeps track of how far it has brushed vertically and horizontally and gradually changes from a start colour to an end colour, thus producing a gradient across the area to which it is applied. |
| Texture Brush  | May or may not ignore the existing colour of a pixel. Either it paints an image of the application area or it combines the existing values of pixels to create a new output. For instance, a blur texture brush would combine values of surrounding pixels to "blur" the result. |

2D graphics also uses sprites and texture as described in the next section.

2D graphics makes use of two main types of image. These are pixel images (pixel art) and SVG, both of which have been mentioned briefly earlier. Pixel art images are also known as bitmaps because they specify the colour of every pixel in the image. They may also use an alpha component to "leave out" certain pixels from the image. PNG and JPEG are both methods for saving bitmaps in a compressed format. The alternative is SVG - Scalable Vector Graphics. SVG images do not specify the colour of individual pixels. Instead, they desribe the location, colour and size of lines and shapes. SVG is more abstract data about an image (hence why they are called graphics). A graphics card can interpret the data to set areas of pixels to the correct colour. For example, an SVG file might specify a black rectangle at location (5,5) in the image with width and height 100.

The advantage of bitmaps over SVG is that for highly detailed or varied images (such as a photograph) it uses less space to save all the data. However, for an image that consists of large areas of the same colour (such as a logo), SVG may be better. However, SVG has one key advantage. SVG specifies shapes and relative sizes. So to increase the size of an SVG image, you simply increase all the relative sizes and recalculate which pixels are filled in. This allows it to retain 100% sharpness for basic shapes. For a bitmap, however, when you increase or decrease the size of the image, the image has to be resampled and the computer must guess the colour of the extra (or fewer) pixels. This means bitmap images do not scale as well as SVG. However, if you apply some thought, it should be apparent that SVG would be just as (if not more) useless for a photograph. You wouldn't want each pixel scaled as just a square (which is what SVG would do).

2D graphics makes use of one final key concept - layers. Layers are complete images of the screen except that the background is transparent. Each layer forms one (or more) parts of the final image. Layers are stacked on top of one another, which higher images blocking out lower images. This allows more complex outputs to be produced. Here are two examples of using layers.
1. Cursor layer and application layer. Allows the computer cursor to be painted on top of the application without affecting the application's image.
2. Character layer and background layer. Allows a character in a 2D game to be moved around and painted on top of a background.

### Sprites and textures
- Sprites:
	- 2D graphic / image 
	- Layered together 
	- Animated
- Texture:
	- 2D image
	- Layered on top by blending
	- Provides a texture / look 
	- Can be stretched over a 3D object to provide a "surface" look

### 3D
- 3D space / coordinates
- Vertices, edges, surfaces
- Everything is triangles / polygons
- 3D modelling
- Hidden surfaces
- One-sided surfaces
- Circles and spheres

### 3D Projection
- Lighting points & shadows
- Projection
- Shaders
- Ray tracing

### Anti-aliasing
- Example of issue
- Why? 
- How?

### Incorporating animation
- Background animation
- Sprite animation
- Object animation 
- Lighting animation 
- Colour animation 
- Skeletons
- Per-vertex animation
- Animation through physics modelling


---


# History

## Introduction
The history of displays, video and graphics offers us insight into the problems (and their solutions) which have been discovered over time and are now solved. If you are intending on writing your own graphics driver, video decoder or display driver you will need to be aware of the issues. If you are not, you will probably end up with low-quality, flickery results.


## Displays
_The following dates are the earliest date for which a commercial product became available. The specific technology or concept may have be discovered, proposed or successfully implemented some years earlier._

### 1922 - Monochrome CRT
Displays started way back in 1922 with CRT - Cathode Ray Tube. Cathode ray tube screens were in common use right up until around 2000/2005 but (sadly) the most modern generation have probably never used one. They were large, bulky screens by necessity of the technology.

A cathode ray tube screen consisted of a front pane of glass, coated on the inside with a phosphor layer. The phosphor layer was divided into dots, each of which was a monochrome pixel. Behind the screen was an anode (positive electrode). At the back of the display was an electron gun which formed the cathode (negative electrode). Between the cathode and the anode were a set of coils. When powered on, beams of electrons would be emitted from the cathode and fly forwards (very fast) towards the anode. With so much momentum the electrons would fly past the anode and hit the phosphor layer on the screen. An atom in phosphor would absorb the electron's energy causing it to become excited. The phosphor atom would then lose that energy by emitting a beam of light out to the viewer. The set of coils inside the display were used to focus the beam of electrons to particular pixels on the screen. This allowed control over which pixels on the screen were switched on or off and at different intensities.

I won't go into the physics of why this works but sufficed to say the coils created a magnetic field used to bend the beam of electrons. This had the amusing effect (or catastrophic, depending on perspective) that applying a magnetic to the side of the display or the front of the screen would warp then image. If you left a strong magnet near the screen for too long, you could magnetise the phosphor layer, resulting in permanent warping (otherwise known as damage!)

### 1954 - Colour CRT
Thirty two years later and along came the colour CRT. The colour CRT extended the monochrome CRT by adding two extra ray guns and a physical layering mechanism for the phosphor layers. The now three phosphor layers used different phosphor chemicals that emitted red, green or blue light. The three ray guns were used to excite parts of each of the three layers to form RGB light which, to the viewer, looked like a particular colour. It is important to remember that RGB pixels rely on the relative naivety of the human eye. The human eye cannot see the individual RGB components, so sees their combined colour. The light itself, however, is still three separate beams of red, green and blue. It is physically possible to have a yellow beam of light since the colour of light is based on its frequency and frequency is a continuous scale, but yellow seen from a screen is not like this. With a good enough camera, a photograph of any screen (even modern) can be enlarged to reveal the individual RGB components of a pixel.

### 1960s/1970s - LED
It is hard to put an exact date on the first LED display since lots of small developments each attempt to claim the title. It certainly wasn't until the late 80s that commercial, full-colour LED displays became available but monochrome/duochrome displays had existed since the mid 60s. Monochrome LED displays are still in wide use today, most iconically at bus stops and railway stations.

Full-colour LED displays work much more simply than a CRT screen. Each pixel consists of a red, green and blue LED each of which can have variable brightness. An additional backlight boosts the output brightness of the screen allowing the viewer to see the colours.

### 1969 - Braille
Braille displays, though not widely used or even supported, have existed since the late 1960s. They are a pin screen where each "pixel" is a pin which is raised or lowered in the screen. Rather than being an entire screen, Braille displays are almost exclusively seen as a single line of Braille characters attached to the bottom of a keyboard. Each character consists of eight pins, four rows and two columns. This gives 256 possible patterns (2^8 since each pin can either by up or down). Typically a Braille keyboard will have an output Braille display with forty to eighty characters (known as character cells).

Due to manufacturing and construction methods used (and, in my opinion, probably a lack of research rather than demand) Braille displays are very expensive. Also, it was only in 2015 that any commercial company produced a full-page Braille display (see [Tactisplay](http://www.tactisplay.com/product/tactisplay-table)) which had 12,000 pixels (40 cells by 25 cells).

### 1971 - LCD
LCD first made its appearance in 1971 and has since become one of the most popular display technologies. This is probably largely due to three factors:
1. It is more energy efficient than CRT
2. It produces a better quality of image than plain LED (brighter, better gamut amongst other factors)
3. LCD screens can be made thinner and lighter than most other screen technologies.

LCD works in the conventional way with pixels made up of the normal three components: RGB.

### 1995 - Full-colour Plasma
Full-colour Plasma displays have a wider colour gamut, can (traditionally) produce darker black than LCD and have a wider viewing angle. However, particularly older Plasma displays, suffered from loss of quality over time, screen-door effects (especially at larger sizes) and suffer from pressure and radio wave interference problems due to the way the light is produced.

Plasma displays work by having cells (very small containers) of a mixture of gases. Each cell mostly contains noble gases but with some mercury vapour. By applying a current to the cell, electrons fly across it. Occasionally an electron hits a mercury atom in the vapour causing the atom to become exciting. The mercury atom then de-excites, releasing UV radiation. The UV light hits the phosphor layer which is coated on the inside of the cell causing excitation of the phosphor. The phosphor de-excites by releasing visible light. This is much the same way fluorescent lamps work and similar to how old CRT screens work.

To form a pixel, three cells are placed next to each other, one for each of red, green and blue. The phosphor coatings determine the colour outputted by each cell.

One particular issue with this method of light production is called screen burn-in. This occurs on both CRTs and Plasma displays. When the same image is displayed for a long period of time, the phosphor overheats and becomes permanently damanged. The damage causes loss of luminosity which, when the screen is turned off, can be seen as a darkened shadow on the display.

Plasma displays also suffer from another effect due to cell design. The cells build up a storage of charge over time, resulting in them remaining on even after the displayed image has changed. This is called a ghost image and is sometimes confused with burn-in. Unlike burn-in it, can be cleared by switching off the screen for a long period of time, allowing the affected cells to discharge.

### 2003 - OLED
Organic light emitting diodes are the latest and (arguably) greatest in display technologies. Yet it is only now in 2015 that we are starting to see significant recognition and use of OLED in commercial products. The high-end smartphone market is currently the biggest user of OLED displays where as in the TV market, LG is the sole manufacturer of an OLED based product. So despite their improvements, OLED is still not completely replacing LCD. This is largely due to the fact that, like any new technology, OLED is expensive to design and manufacture.

OLED screens are special because each OLED is emissive. This means that the OLED itself produces the bright red, green or blue light that the eye sees. This is completely different to LCD or conventional LED. In LCD and conventional LED, the light seen is produced by a bright backlight which is then filtered to give the correct colour. Using backlight means that brightness control can only happen for either the whole screen or, in some advanced products, patches of the screen. With OLED, brightness control happens per pixel. This means that OLED can produce much darker or higher-contrast images than LCD as it can very accurately select areas of the image to be very bright or very dark. LCD can, at best, only make certain patches very dark and other patches very bright. So OLED produces a better, more natural quality of image. It is also lower power than LCD.

Unfortunately, blue OLEDs suffer from degradation issues which is one of the reasons why Samsung ceased manufacturing its OLED TV. Phones which use OLED do not suffer the issue as badly because the smaller screens use a different variant of the OLED technology.

Ultimately OLEDs look like they will be the way forward but cost amongst a few other issues is a major prohibiter (though cost is likely to reduce roughly according to the widely accepted concept presented by Moore's law).

### 2004 - Electronic Paper
Electronic paper is perhaps more recent than OLED but its use has been fairly limited. This is largely because it is a slow and monochrome (full-colour is only seen in research labs). This has meant it is basically only used for E-Reader products. Little to no attempts have been made to replace traditional paper applications such as newspaper. E-ink displays are also comparatively expensive to produce.

E-ink displays come in essentially two forms. One form of the technology relies on having small cells which contain either a white/black ball or a white liquid and a black liquid. Applying a voltage flips the ball one way up or the other thus revealing the white or black side. The liquid version does the same but the white or black liquids are pushed to the top of the cell to place them one top thus allowing the user to see white or black. These forms of e-ink are called Electrophoretic displays.

The second form of the technology is called Electrowetting. This is somewhat different in that it allows more than just an "on-off" format for pixels. In fact these pixels can achieve gray-scale images and brighter whites and darker blacks than electrophoretic displays. Electrowetting is based upon controlling the shape of a droplet of dark liquid inside each cell. The drop of liquid acts as an optical switch. When no voltage is applied, the droplet sits flat on top of a hydrophobic surface. Thus a dark pixel (i.e. black colour) is seen. When a voltage is applied the droplet no longer sits flat on top of the underlying surface. Instead it forms a ball on top of the surface and is moved to one side of the cell. Thus the cell effectively becomes transparent allow the viewer to see whatever is behind (usually a white or reflective surface).

Electrowetting displays are much faster than electrophoretic displays. For example, electrowetting displays are capable of playing reasonable video quality. However, they are very expensive and suffer from dramatic loss of visibility in bright sunlight or similar situations.

Lastly, some e-ink displays (primarily electrophoretic-based displays) are bendable due to the plastic used to manufacture the grid of cells. Such bendable displays have seen low uptake, however. Ultimately e-ink is an innovative technology that, with time, will no doubt find its place in the market. It is not intended to, nor likely to, ever supplant more conventional display technologies.

## Graphics

### 1950s
- First time computer graphics was seen
- Initial output on oscilloscopes and display scopes (difference?)
- Interactivity limited but present (finger tracking)
- Graphics hardware introduced
- Saving / recalling of images
- Basis point for solving many graphics problems:
	- Ivan Sutherland : Long lasting solutions / principles
	- Picture made up of images of objects not just an image for the whole thing

### 1960s
- "Computer graphics" coined by William Fetter
- Second video game created. First world wide video game to be played.
- Scientists / researchers begin to use graphics for simulation / demonstration
- Renault contribute to study of curves - now called Bézier curves
- IBM 2250 first graphics terminal
- University of Utah forms CompSci department employing David C. Evans. World centre of computer graphics research
- First stereoscopic 3D display : Sword of Damocles
- SIGGRAPH - Special Interest Group on Graphics - formed

### 1970s
- Most important early breakthroughs in this decade - at University of Utah
- Hidden-surface algorithm & Core invented!
- Utah Teapot
- Advances in models, shading and mapping allowing shadows, better 3D and surface textures
- Pong : 1972
- Space Invaders : 1978
	- Both used Intel 8080 microprocessor and Fujitsu MB14241 video shifter to improve 2D sprite rendering

### 1980s
- Graphics become serious commercial feature
- Graphics in standalone workstations
- Orca, Commodore Amiga and Macintosh become popular, serious graphics and design tools
- Full, animated 3D computer graphics now used commercially
- First Ray Tracing graphics done by Japan - LINKS-1 Computer Graphics System - super-computer
- Chroma-keying ("bluescreening") become reasonable / viable
- Shaders introduced by Pixar
- Real-time 3D graphics for arcades became commercially viable

### 1990s
- Massive increase in CGI and 3D performance
- Silicon Graphics decline. Rise of Microsoft Windows and Apple Macintosh
- First computer graphics TV series made in France
- 1995 : Toy Story (Pixar)
- Atari, Nintendo and Sega selling millions
- Quake, Doom
- PS1 and Nintendo 64
- Nvidia - GeForce 256 - First home video card, first actual GPU
- DirectX and OpenGL popular by end of decade

### 2000s
- Graphics, CGI and 3D ubiquitous
- PS2, PS3, XBox, GameCube, Wii, PC games
- Finding Nemo (Pixar), Ice Age, Madagascar, Star Wars
- GPGPU for research improved performance. Bitcoin mining.
- Uncanny valley

### 2010s
- Ultra-HD
- Photorealism

## Video

### Connectors

1956 : Composite video
1979 : S-Video
1987 : VGA
1999 : DVI
2003 : HDMI

### Formats

- Composite
- VHS
- PAL
- NTSC
- 720p / 720i
- 1080p / 1080i
- Ultra-HD TV

### Encode and decode
- Video codec (hardware) for compress / decompress
- Files / streaming
	- H.264 / MP4
	- WMV
	- Quicktime H.264
	- Google On2 codecs (VP9)

---

# Hardware

## Overview
- Many competing standards
- Always a compromise
- VGA most common and probably easiest
- HDMI taking over

## Current display technologies
- Plasma
- LCD LED
- OLED

## Current graphics technologies
- Whatever graphics card you own

## Current video technologies
- VGA : common, easier, more docs
- HDMI : common, taking over

## Compatibility between hardware
- None :)

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

## "I want to write a graphics driver" - Day 1

## "I want to write a graphics driver" - Day 2

## "I want to write a graphics driver" - Year 2

## After the graphics driver (or along the way)
---

# References

*[acronym]: meaning

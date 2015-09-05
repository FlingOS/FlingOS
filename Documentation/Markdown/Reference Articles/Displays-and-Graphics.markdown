---
layout: reference-article
title: Displays & Graphics
date: 2015-07-23 12:20:00
categories: [ docs, reference ]
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
| PNG, JPEG        | Both are based on compressing and saving colours of pixels in an image but they have very different approaches. PNG uses a colour palette and saves data about large areas/blocks of an image that are the same colour. JPEG is a lossy compression format that uses the Discrete Cosine Transform (DCT). JPEG is good for images where the colour of neighbouring pixels varies a lot. | JPEG is best for photographs, PNG is best for logos / web images |
| Ray tracing, Shading | Ray tracing uses virtual rays of light from light sources and traces them around in 3D space until they hit the "screen". After tracing thousands of rays, the final colour of the virtual screen is used as the image. Ray tracing is computationally expensive but produces highly realistic looking results. Shading uses objects (with surfaces) covered in colour. The colour is varied to give the look of shadows and 3D objects. It is computationally less expensive but produces less realistic (sometimes inaccurate) results. | Ray tracing for movies, shading for games (though the two techniques are gradually merging / shifting usage) |
| VGA, HDMI        | HDMI basically replaces VGA. VGA was analogue, HDMI is digital. VGA had limited resolutions, frame rates and quality, HDMI has higher resolutions, frame rates and quality. Ultimately, HDMI has compromises but in almost every aspect it is less compromising than VGA so is thus better than VGA in almost all cases. This is an example of new technology replacing old rather than just adding another competitor. | &nbsp; |

## How do they all fit together?
Displays, graphics and video all fit together relatively nicely (when you ignore all the incompatible, proprietary interfaces at the technical level). Essentially it's all one big chain of components linked together. It looks roughly like this:

``` bash
[Application]
      |
      |
Graphics pipeline ---------> Video pipeline
(e.g. 2D/3D rendering)       (e.g. MP4 file decode)
      |                           |
      |                           |
Display controller <--------------^--------------> Audio pipeline 
      |                                                  |
      |                                                  |
(Video) Cable to display                           Speaker controller
  (e.g. HDMI)
      |
      |
    Screen

```

What this shows is how an application can use graphics to render 2D and 3D stuff along with video to play an MP4 file. The video is split into video and audio, with audio sent to the speakers. The video part of the MP4 file is merged (by the graphics card) with the application graphics (the image output) and then sent to the display controller. The display controller then uses video (described as type 2 above, e.g. HDMI) to send that to the physical screen.

So graphics and video (type 1) sit alongside each other. Graphics combines with video to provide the input for display. Display then uses video (type 2) to transmit to the screen and the screen uses a display technology to actually produce the light which a human can see.

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

The advantage of bitmaps over SVG is that for highly detailed or varied images (such as a photograph) it uses less space to save all the data. However, for an image that consists of large areas of the same colour (such as a logo), SVG may be better. However, SVG has one key advantage. SVG specifies shapes and relative sizes. So to increase the size of an SVG image, you simply increase all the relative sizes and recalculate which pixels are filled in. This allows it to retain 100% sharpness for basic shapes. For a bitmap, however, when you increase or decrease the size of the image, the image has to be re-sampled and the computer must guess the colour of the extra (or fewer) pixels. This means bitmap images do not scale as well as SVG. However, if you apply some thought, it should be apparent that SVG would be just as (if not more) useless for a photograph. You wouldn't want each pixel scaled as just a square (which is what SVG would do).

2D graphics makes use of one final key concept - layers. Layers are complete images of the screen except that the background is transparent. Each layer forms one (or more) parts of the final image. Layers are stacked on top of one another, which higher images blocking out lower images. This allows more complex outputs to be produced. Here are two examples of using layers.

1. Cursor layer and application layer. Allows the computer cursor to be painted on top of the application without affecting the application's image.
2. Character layer and background layer. Allows a character in a 2D game to be moved around and painted on top of a background.

### Sprites and textures
Sprites are small images that are combined with other graphics to form a larger image or the final frame (which is rendered to the screen). For performance, sprites were originally implemented in hardware. The hardware would use separate DMA channels to access the graphics memory which would allow it to "overlay" a sprite onto each frame. This allowed variable-position and/or animated sprites to be rendered. Over time sprites have become largely software-driven. 

All sprites are 2D images. Some sprites are referred to as 3D sprites (or Billboards) because they are 2D images which can be seamlessly incorporated in a 3D rendering. Sprites are layered on top of each other and, more recently, support partial-opacity, allowing the sprites "underneath" to show through. Sprites can consist of a single static image or an animation. For example, an animated GIF file used as a character in a 2D game would constitute an animated sprite. (On its own the GIF animation is just an animation or image).

Sprites are closely related to textures. Textures are also images or animations, but used in very different ways. Unlike a sprite, a texture is only seen in 3D graphics. A texture is applied to a surface (see next section) to give it colour, detail or a particular look. Originally, textures involved wrapping a 2D image around the 3D surface. For instance, imagine printing a picture on a sheet of paper, such that it fitted the net of a cube. Then wrapping the paper (only the net) around the cube. The printed picture would appear to cover the entire cube. This is what happens with textures. Clever image creation can result in the final 3D object wrapper in the image appearing to be highly detailed, even if the model itself is very simple. Textures are blended in much the same sprites are - by layering and using opacity (and, in modern graphics drivers, other more complex blending functions). 

### 3D
3D graphics is probably what everyone thinks of now if you say "graphics" or "CGI" to them. It's sad to say but despite all the impressive work in 2D graphics (that yes, is still happening) 3D graphics seems to be taking over. I imagine it won't be long before we see UIs entirely in 3D (even on 2D displays). So what exactly is 3D graphics?

To start with we need to understand 3D space. A 3D space is, theoretically, just how we perceive the real world. It can be split up into three perpendicular directions. Up/down, left/right and in/out. These directions can be defined in any orientation you like so you can think of up/down as actually going diagonally through your room. The other axes remain perpendicular. These axes form an orthogonal, 3D space. In simple terms, orthogonal means that the axes meet at right angles. It is more useful to note, however, that this means that if you move along one axis, you cannot express that movement as a sum of movements in the other two axes. However, every movement in the space can be described by the combined effect of movements in each of the axes. 

However, an orthogonal set of axis does not on its own provide us with any way to determine location within the space. The best we can do with it is to define a vector, which is defined as the displacement (not distance), in each of the axes, from a point A to a point B. *(Note that displacement is itself a vector quantity - positive or negative distance. Distance is the absolute measurement of the gap between two points).* To create a coordinates system we must define an origin point. All (x,y,z) coordinates are in fact just displacement vectors (as opposed to acceleration vectors or similar) from our chosen origin point. For this reason, the choice of origin is largely irrelevant. A well-chosen origin can simplify the maths (and required computing) but it shouldn't affect the overall result. In fact, it is common to perform animation functions from the origin of a character, and then simply translate the character's origin to the world's origin via a single displacement vector. By doing this, the maths for calculating the character's movements can be both simplified and done in parallel with other processing since it relies on no external knowledge of its own 3D space.

Now that we have defined a 3D space, we can start to think about 3D objects. An object is made up of three things: points (known as vertices or nodes), edges (sometimes called lines) and surfaces. A point is defined by a displacement vector from the origin (or another point). An edge is defined by the shortest straight line joining two points (as opposed to using a curved line or to the theoretically infinite line joining two points by drawing straight lines directly away from the points. Note that in non-Euclidean geometry the concept of a straight line still applies, even if when rendered it appears curved.) A surface is defined as an infinite set of points bounded by a set of edges. In the computer graphics world, the set of points have to form a flat surface. This is generally achieved by defining a surface as the flat surface formed between a set of edges. However, since every 2D shape with four or more edges can be subdivided into triangles, every surface in computer graphics is actually just a set of triangular surfaces i.e. surfaces bounded by three edges. Thus every 3D object in computer graphics is defined by triangles in the 3D space.

It should be apparent then that if every object is made up of flat triangles, you can never achieve a truly circular or smooth object. The best you can hope for is to use lots of very small triangles so that the surface appears smooth to the user. The number of triangles a graphics card is capable of processing in a given time is used as a performance benchmark by many benchmarking software packages.

So what happens when one 3D model is placed behind another? Well, some of the surfaces (or just parts of the surfaces) are hidden. This is called the hidden-surface problem and a solution was pioneered by the University of Utah in the 1970s. Since then a lot of research has been done into both 2D and 3D occlusion algorithms. The problems essentially break down into sorting algorithms, many of which are solved by divide and conquer (for large numbers of primitives that is). The graphics software works out which surfaces are hidden, and then skips rendering those. A similar thing occurs for 2D graphics and sprites - the graphics software will ignore sprites which are hidden by other overlaid sprites.

It is interesting to note that surfaces in 3D graphics are often one-sided. What this means is that if you view it from one side, the graphics software will show you what you'd expect - a solid / coloured / textured surface. If you view it from the "other side", the surface appears transparent. This is because surfaces often define a normal vector which, when combined with lighting (see later), is used to calculate reflection / absorption of the light. If you are looking in line with the normal, the surface will appear totally transparent as the side of the surface you are looking at is non-reflective.

It is useful to note that the University of Utah used the 3D model of an old fashioned teapot as a graphics test which became an industry-wide standard for overall quality and performance testing. The reason being that a teapot has many curves, can be reflective or non-reflective, casts odd shadows and has a number of other graphically-complex features. Essentially, the more realistic your teapot looks, the better your graphics performance or quality! (A rotating teapot was a significant challenge and is still done poorly by badly written software. The current generation of graphics hardware is not able to compensate for poor software by simply applying lots of processing power. This is in contrast to the CPU market where readable, maintainable code is become more popular than optimised code. Though this may also be due to compiler optimisations becoming so good.)

### 3D Projection
In the preceding section we discussed 3D models, surfaces and I briefly mentioned lighting. None of it actually explained how a 3D model gets rendered (i.e. converted) to a 2D image. Rendering involves projection. Projection is when the "view" of a 3D model is traced (including colour) onto a 2D screen. The image formed on the 2D screen becomes the final image for the frame. The 2D screen is called the projection screen, projection plane or projection surface. To understand how projection works, we must first think about light sources and shadows.

There are three types of light source. Spot lights, directional lights and point lights. Spot lights are like spot lights in a theatre - the virtual "rays" of light are emitted at a particular conical angle in a particular direction from the source. Point lights are light sources within the 3D space which emit light in all directions (this is roughly equivalent to a bulb in real life). Directional lights are the strangest. Directional lights are light sources that are an "infinite" distance from the 3D space in question and emit parallel rays of light of equal brightness across the entire space. They are called directional because all the rays of light travel in the same direction i.e. are parallel. In real life, the sun is approximately a directional light as light rays from the sun are approximately parallel for a given small space on earth. Spotlights and point lights emit rays which fall off in intensity with distance from the source. Directional lights have the same intensity everywhere.

The easiest way to understand how lighting works is to imagine rays of light from each light source. These rays of light travel in straight lines. The computer traces the rays from the light source to surfaces of objects. When the rays hit the surface of an object, the rays are either absorbed or partially or wholly reflect. "All" the rays of light from the light sources are traced until either they exit the bounds of 3D space being examined or they hit the projection surface (/screen). When they hit the screen, their colour and position on the screen are recorded to form the colour on that part of the screen. Combining thousands of rays completes the whole image. This method of projection is called ray tracing and is used to produce photo-realistic images. However, due to the number of rays that must be traced and the computation required to trace them, ray tracing is only just becoming used in real-time graphics. Ray tracing is mostly used for CGI or 3D animation/graphics rendered for advertising or films.

There are two alternative methods for rendering a scene. The first is projection, which takes all the vertices of an object and works out where they would appear to the viewer (where the viewer is looking from the projection screen). This introduces the idea of field of view, as the angle out from the edges of the projection screen that the viewer can see. Changing the field of view can, for example, alter the final image to make it appear like the space is being viewed through a fish-eye lens. 

Shaders are (in some ways) a dual concept in graphics. In one sense, they are a technique for colouring, texturing, lighting parts of a 3D model and rendering the model to the screen. However, if you analyse this sense of the word, it is apparent that colouring, texturing, lighting and rendering are all just ways of colouring bits of intermediate images or the final projected image. Thus the second and most accurate meaning of the word "shader", is as a short function for performing a particular algorithm or sequence of mathematical operations. These operations can be used to manipulate any numeric input. Since computers deal only in numbers, shaders can be used to manipulate any data within a computer. Thus shaders can change vertex position, surface colour, lighting or anything else including performing projection/rendering operations.

Shaders have become one of the most important parts of graphics (including 2D graphics) because of their versatility and the fact that they can be highly optimised for graphics hardware. Also, many shaders are designed to be able to work on data in parallel with other shaders. This allows graphics hardware to process lots of shaders simultaneously, thus allowing faster rendering of an entire scene. In engineering and scientific research, shaders have become key to efficient simulation. The shaders are just used as mathematical functions, allowing a simulation engine to run lots of mathematical operations in parallel.

A graphics pipe or pipeline is often expressed as a sequence of shaders (which may be executed synchronously or asynchronously, depending on other requirements and shader compatibility). The many-piped design of a GPU gives it its highly parallel design. This is the key difference between a GPU and a CPU. A CPU is designed to execute steps synchronously, very efficiently. A GPU is designed to execute asynchronous (or parallel) steps very quickly. GPUs are poor general purpose processors because most general processing is a sequence of synchronous steps. Most CPUs are poor mathematical (or graphics) processors, as most mathematical or graphics work can be highly parallelised. 

Lastly, I would like to stress the importance of the fact that (99% of the time) there is no such thing as a "3D image". Images are not 3D, they are 2D. The 2D image may appear to be 3D, but nonetheless the image itself is 2D. The 3D model is the actual 3D thing.

### 4D
The fourth dimension has been interpreted in many ways. Here are a couple of common ones:

1. As time - the fourth axis is detached and is a time axis.
2. As the secondary view used when performing actual 3D rendering/projection. In this case you get two 2D images (projections of the scene) which are taken from slightly different positions and angles. By showing one to one eye of the viewer, and the other to their other eye, what the viewer sees appears truly 3D.
3. As a genuinely fourth dimension - welcome to the crazy world of higher dimensions... (I shall have to leave it to the mathematicians and physicists to explain this one!)

### Anti-aliasing
Aliasing is a significant problem in computer graphics that occurs when rendering 2D images or "3D images" (i.e. 2D projection images). The issue occurs when any line (or edge) is at an angle to vertical or horizontal (which is a lot of the time!). The raw edge looks staggered like a set of steps. This issue also occurs when up or down scaling an image. Anti-aliasing is a process that smooths the edge to mask the stepping effect. Essentially it is a clever blurring function that can be applied to an entire image to smooth edges.

If you zoom in to pixel-level on an anti-aliased image, it is apparent what is happening. Instead of defining a hard edge by having an angled line of pixels set to a single solid colour, an anti-aliased image has the main pixels with the solid colour surrounded by one or two "layers" of "faded" pixels thus blurring the edge of the line. When zoomed out the resulting line appears much smoother to the eye.

### Incorporating animation
Over the years many methods for incorporating animation have been developed. Largely, the techniques used were aimed at providing the most useful form of animation (not necessarily the most realistic) but suited to the limitations of the hardware. Animation was not an after thought of graphics - it has been in there from the beginning. Forms of video playback (such as frame-by-frame sprites) were one of the earliest methods of animating. Prior to even that though, scrolling backgrounds and then moving sprites were considered animation. Now, we consider those just an ordinary part of rendering a scene. This is probably because we use modelling in which we expect to be able to reorient things to obtain a different view. Animation is, after all, just reorientation to obtain a different view. 

So background scrolling and moving sprites were early forms of 2D animation, which are still in use today all forms of graphics but probably most obviously in online games. When 3D modelling came about, object animation be repositioning (as we mentioned) was introduced. This has been followed by lighting and colour (/shading) animations. It is interesting to note that graphic artists and animators are frequently, whether consciously or not, applying theories of relativity which were pioneered in physics. The idea that perceptually, if you move a person in space or if space moves around a person, they will both appear the same to the person (or to the space itself, if you consider the space an observer). 

Despite there being many techniques for moving "the other half" of a scene to create the illusion of animation, none of them have ever captured true motion. This is because ultimately, the simplest method (and so the best looking though this might be arguable) to animating a character or object, is to animate the object in the scene not the scene around it. This is probably most noticeable with animated 3D characters. Old games eft the character in a fixed location on the screen with no motion, and then moved the scene around the character. Modern games allow the character and "camera" to separate and move fluidly making for more natural apparent movement. Of course this becomes a necessity when limb movement is desired since there is no way to animate the scene such that limbs appear to move, if the limbs don't actually move relative to the rest of the character. 

This leads to the last few forms of animation which are the most recently used and still in research. It starts with the idea that for a 3D model (e.g. of a character) you build a skeleton that describes the basic shape of the object. You give the skeleton joints (or hinges) which allow the various bones to move/rotate relative to each other. This allows a character, for example, to move its hands, arms, legs, feet, etc. in a more realistic motion. However, in most games even now, the body of the character will only move in a fixed pattern. Pre-set animation will be played back when certain in-game interactions happen. There will also be no surface reactions such as skin deforming.

This brings us to the latest in animation. Per-vertex animation and physical modelling. Per-vertex animation creates a large number of vertices across a surface and then animates each of them individually in response to contact with other models in a scene. This can lead to more realistic skin or more obscure things like a bouncy ball which deforms properly on impact. Animation through physics modelling also extends to biological / neurological modelling. You pre-program an object or a character with equations that represent real-world physics. Then, instead of playing back pre-created animations for an interaction, the computer uses the equations to work out what would happen. Biological and neurological modelling take this a step further by adding muscles to a skeleton and using genetic/evolutionary algorithms to teach the character how to react. Significant success has been demonstrated with this technique and it is now being used in applications ranging from games to safety testing. 

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
The 1950s was the first time computer graphics was seen. This is rather remarkable really given the first transistor based computer was only just being developed and colour CRT had only just hit the shelves. Initially computer graphics was little more than recognisable shapes and animations played back on an oscilloscope display. This rapidly developed, however, with finger-tracking programs which would draw where you touched being developed (yes, ideas about touch-screens have been around and usable since the 1950s. The Apple iPhone wasn't the first and certainly not the last). Naturally though this kind of finger-based drawing was confined to engineering talks and labs and never saw commercial use. 

Graphics hardware was quickly introduced with noticeable and significant differences which remain even to this day. GPUs and CPUs have long been separate entities. Respectively used for highly parallel processing and synchronous (or general or "central") processing. By the late 1950s boffins had created systems which could load and save images (a remarkable achievement given the low amount of storage and processing available at the time). 

The 1950s also saw the development of long-lasting solutions to many graphics problems. Ivan Sutherland, prior to and during his time at the University of Utah, was a pioneer in the field, alongside David C. Evans and his students. In the following decade he would continue his work. Sutherland is widely credited with inventing the field of computer graphics since he provided the first methods for drawing lines, shapes and also for modelling 2D scenes in terms of the objects and their properties instead of just the image's pixels. He is regarded as having created the world's first interactive program (Sketchpad, 1962) and the world's first non-procedural language and object orientated system (all in one year).

### 1960s
The term "computer graphics" was in fact coined by William Fetter in 1960 while working for Boeing. In this decade the world's first commercial, worldwide video game was produced, Spacewar. After development by Steve Russell at MIT, the game floated around the community of the PDP-1 (the system it was written for). Eventually the creators of the PDP-1, DEC, obtained a version of the game and started using it for testing. As a result, it ended up being distributed with PDP-1s.

It was in this decade that scientists and other researchers began to use graphics for modelling, simulation and demonstration. Rather interestingly, Renault contributed significantly to this field be studying curves. The mathematics they developed have since become known as Bézier curves, after the man who developed the field while working for Renault. 

In 1964 IBM released the IBM 2250 Graphics Display Unit which used vector graphics and could be attached to the System/360. The interface had a pen input (much like modern tablets do but it was wired and less accurate at least in part due to the relatively low display resolutions of the day). The University of Utah continued its excellence solidifying its status as the world centre of computer graphics research. 

3D displays may seem like a relatively recent phenomenon (though they haven't massively taken off outside of cinemas) but the first technology for them was around in the 1960s. Not all that long ago in historical terms but an age ago relative to the age of the field. The first stereoscopic 3D display was created and named "Sword of Damocles" after the testing machine which apparently looked formidable. Though widely considered to also be first augmented reality display, the Sword of Damocles head mounted display may not have been the first AR system. Philco's HMD from 1961 streamed live video from another room to give a teleprescence experience and also tracked user's head movements. For this reason, some people maintain that Philco's system was the first AR system.

In 1969 SICGRAPH, later to become the Association for Computing Machinery's Special Interest Group on Computer Graphics and Interactive Techniques, was formed by ACM, Sam Matsa (IBM/GM) and Van Dam (Brown University), to bring together people from originally across the USA who had an interest in computer graphics and/or interactive techniques. The organisation now  hosts the world's largest conference in the field each year.

### 1970s
The 1970s saw most of the significant breakthroughs in the computer graphics field and a large proportion of them were discovered and developed at the University of Utah. For instance, the hidden surface algorithm, which efficiently works out which surfaces in a 3D space/model are hidden by ones in front. The 3D Core Graphics System (or just, Core) was the first graphical standard developed and was created by 25 experts from ACM SIGGRAPH. 

Also in this decade the famous Utah Teapot was created by the University of Utah. The teapot was widely used as a graphics test because of its complex graphical properties. The quality of teapot rendering is used as a test even to this day.

The 1970s saw huge advances in 3D models, shading, new techniques called mapping which in turn allowed rendering of shadows and lastly better 3D and surface textures enabled more realistic and faster-rendered 3D worlds to be created. All of these 3D advances also translated into improved 2D performance, since 3D rendering eventually renders to a 2D plane.

In 1972 one of the world's most famous games was created - Pong. The ping-pong, tennis-like game has been in arcades and recreated online an immense number of times and to this day school kids around the world know and love it. Despite its relative simplicity, there is still something captivating about the game which keeps it at a top-spot on online gaming websites. Of course, Pong wasn't the only game created in the 1970s that's survived the test of time. Space Invaders in 1978 is probably even more famous though less widely recreated. Both games used the Intel 8080 microprocessor with Fujitsu MB14241 video shifter to improve 2D sprite rendering.

### 1980s
The 1980s was the first truly commercial era of graphics. Graphics started to feature as a selling point on a large number of devices including standalone workstations. Computer systems such as Orca, Commodore Amiga and Macintosh became popular, serious graphics and design tools. Macintosh rapidly became (and still maintains) its standing as the must-have, industry standard tool for graphics and design (though nowadays the tables are slowly turning for Linux and Windows as cross-platform software becomes more prevalent). 

the 1980s also saw the first time animated 3D computer graphics was used commercially with companies such as Pixar created full animated movies. The first ray-tracing system called the LINKS-1 Computer Graphics System was developed by a Japanese company and was also one of the world's earliest super computers. Alongside this chroma-keying (or "bluescreening") and real-time 3D graphics for arcades both became commercially viable. 

In essence, the 1980s saw graphics come out of research and into medium to high cost systems that were commercially viable. For many this was the first time they would use a computer graphics system. Though not strictly related to graphics, it is worth noting that this is around the same time the World Wide Web was invented (1989) by Sir Tim Berners-Lee.

### 1990s
The 90s saw continued commercial growth and along with the investment came yet more research and progression. Massive leaps forward in CGI and 3D performance lead to the rise of 3D graphics for individuals not just arcades and companies. Consoles from companies such as Atari, Nintendo, Sony and Sega sold millions of consoles including the PS1 and Nintendo 64. At the same time graphics cards from Nvidia such as the GeForce 256 (which is widely considered the world's first actual GPU though largely only because of its name) introduced graphics to standard home computers such as PCs. Microsoft Windows and Apple Macintosh began to see off Silicon Graphics based systems and PC games such as Quake and Doom became widely popular. Graphics systems still in use today such as DirectX (though now deprecated) and OpenGL (which has newer versions) were developed. CGI improvements also lead to the world-famous Toy Story film by Pixar amongst others.

### 2000s
By the new millennium graphics, CGI and 3D games were becoming (relatively) cheaply available and most home computer systems used graphical interfaces. Windows XP succeeded in maintaining a lot of backwards compatibility meaning famous games such as Quake and Doom continued to be playable. Newer games such as Sims became widely popular and a large number of children had access to some level of gaming either in an arcade or at home. Internet games (largely built using Adobe Flash) also became widely supported (though they never really reached the same level of graphics as native games).

Consoles such as the PlayStation2, PlayStation3, XBox (multiple versions), GameCube (earlier on), Wii and PC graphics cards became very popular with millions sold worldwide. At least a significant number of people started owning two (or occasionally three) games consoles. 

Further advances in CGI and 3D brought about fantastic films such as Finding Nemo, Ice Age, Madagascar and the prequel-sequel trilogy of Star Wars films which maintained the original films' widely respected standing as leaders in use of CGI. 

The GPGPU was also a new invention which allowed general purpose programming for graphical processing units - GP-GPUs. This lead to famous uses such as Bitcoin Mining which are largely maths based systems. Researchers into various medical fields also started distributing software that would use spare CPU and GPU time to do distributed computing to process the large volumes of data (such as analysing DNA or cancers).

Lastly, the latter half of the 2000s to 2010 saw film and visual graphics producers hit the "uncanny valley" problem. This is a psychological theory that says the more realistic an image looks, the more a human observer will empathise with it and thus like or believe it. However, there is a small region between almost realistic and totally realistic which has the total opposite effect. This is where films and images look only very, slightly different from the real thing - they are uncannily close. Observers of such images are repulsed by them. There are a number of theories as to why this is, such as the evolutionary theory that we avoid close lookalikes because viruses and diseases often try to be close lookalikes in order to infect/attack cells and species. The result of this "uncanny valley" phenomenon is that a number of almost photo-realistic films, which were in all respects very good, were complete disasters at the box office. People hated them. Since then, no producers aim for photo-realism for fear of ending up missing the target and being in the uncanny valley. It is easier to produce high quality, slightly unrealistic films which are guaranteed a good reaction by viewers.

### 2010s
The 2010s are only halfway through and graphics is continuing to progress. This decade has so far seen the rise of desktop-quality graphics for mobile devices (including 3D graphics) and the further development of HD and Ultra-HD graphics (and displays). Photo-realism for static images is widely used though films still avoid it. We are also starting to see films which are entirely CGI generated but include human actors and animals.


## Video

Video has largely kept pace or been ahead of graphics and displays and so has a less interesting history. A lot of the significant points I will ignore as they are largely about companies battling it out for the right to use one technology or another. Similar battles over encoding in different regions of the world have taken place and the infamous digital rights management debates continue in full force to this day.

### Connectors

There are lots of different connectors and transmission standards, all of which try to balance colour quality, frame rate, minimising interference and various other factors. For the purposes of this article I will mention only the most common (and possibly most significant) standards.

#### 1956 : Composite video
In 1956 the composite video standard was introduced. Composite video is an analogue transmission standard that can typically reach 480i or 576i quality. It encodes the video data onto a single channel (so only a single cable is required). The exact encoding varies between NTSC, PAL and SECAM but the basic idea is that the HSL and frame sync signals are modulated (i.e. combined) into a single signal. The actual modulation process is more complex than can be described here.

The composite video standard was so significant that it is still in use today. A significant number of televisions and systems still have composite video connectors and there's probably a fair few buildings with the cables still built in. This seems to only be the case in the TV/DVD/Console market, however, since VGA rapidly replaced S-Video and Composite video for traditional workstations.

#### 1977 : SCART
SCART (meaning Syndicat des Constructeurs d'Appareils Radiorécepteurs et Téléviseurs) was developed in the 1970s and first appeared on TVs in 1977. It was only ever widely used in Europe (in place of Composite video and S-video). It had chunky 21-pin connector, could carry higher quality video than composite or S-video and also carried audio. However, it was still anologue so recent digital standards have almost entirely replaced it. 

SCART was developed to make connecting video devices much easier. It had a connector which made incorrect connections nearly impossible and also had support for transmitting composite and RGB signals (and, by the end of the 1980s, S-video as well). It also had support for waking devices from standby and automatic detection of input sources. Lastly, it was a bi-directional standard which allowed for chaining devices, recording and encrypted signals. Encrypted signals were frequently used for Pay-to-view TV along with providing basic Digital Rights Management during video-playback (though plenty of ways were developed to circumvent this).

#### 1979 : S-Video
In 1979 the S-video standard was introduced as a higher-quality version of composite video. S-video stands for Separate Video and is sometimes called Y/C - Luminance / Chroma. It works in essentially the same way as Composite video does but uses separate wires in the cable for the luminance and chroma signals. This allows it to achieve better quality.

The S-Video standard has also hung around for a long time though in many regions TVs and games consoles did not have S-video input or output ports. This was a significant problem in Europe, where SCART cables and connectors were the norm, because games consoles such as Sony's PlayStation were not fitted with SCART output. 

#### 1987 : VGA
Video Graphics Array (VGA) was introduced in 1987 by IBM alongside the PS2 standard. It was fairly rapidly and widely adopted by the rest of the industry. The name VGA itself has become synonymous with both the hardware and software standards and even the 640x480 resolution which it started with. Later versions of VGA (XGA, SVGA and so on) added higher resolutions but are often referred to simply as VGA. 

VGA became the de-facto standard for graphics and displays for PCs because of its quality and (after a time) widespread support leading to great compatibility. Many manufacturers contributed their own additions and variations but the basic standard and increased levels of quality were always supported. VGA has sufficient bandwidth to support 1080p HD displays and even higher (recently 4K and 8K). However, most current displays remain at 1080p quality.

VGA is so significant that FlingOS dedicates an entire article to just one small subsection of VGA - VGA text-mode. Please see that article for more detail. Other articles, already or will in future, cover VGA graphics in more detail.

#### 2003 : HDMI
While other standards were developed between VGA and HDMI, none of them stuck and gained anything like the same level of popularity. HDMI stands for High Definition Multimedia Interface and is the second but more significant digital video standard.

HDMI is now beginning to replace VGA and all other video standards. HDMI is now the primary output port and input port for most laptops, consoles and TVs. The data transmitted over HDMI has standard electrical signals and a basic transport protocol. However, the data itself can have a variety of different encoding formats, a large proportion of which are supported by most devices. HDMI includes both audio and video transmission along with a variety of features such as Digital Rights Management controls. 

HDMI is also electrically and software backwards compatible with DVI, the first digital video standard (Digital Video Interface). 

### Formats & Quality

- Composite / Y/C / HSL
- VHS
- PAL
- NTSC
- RGB
- sRGB
- 720p / 720i
- 1080p / 1080i
- Ultra-HD TV 
- 4K / 8K

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
- None :) Okay probably something...

---

# Software

## Overview
- Structure of software:
	- Display Drivers (KM)
	- Graphics Drivers (KM)
	- Graphics Drivers (UM)
	- Video Decode driver (KM/UM)
	- Video Encode driver(KM/UM)

## Compatibility and Integration


---

# FAQ & Common Problems

## "I want to write a graphics driver" - Day 1
"Woohoo! Excited for this! I'll get it working in no time and have this amazing UI that's going to be so much better than Windows!"

...

No chance. Even getting low-resolution displays with just rectangles and lines is hard work (and will take a few weeks or months). If you want to support more than just a virtual machine (i.e. actual hardware support) then add even more time for working out how the specific, real hardware that you own works. 

Beyond basic VGA support, you then have to consider the entire graphics stack. As a basic example, displaying lines and text to any reasonable quality will require anti-aliasing support, font support, vector graphics (or similar) and if you want to add the cursor, some degree of buffering/layering. All of that is the "relatively simple" stuff. Don't expect to have an entire graphics driver working in a week because you won't. It will take a very long time.

## "I want to write a graphics driver" - Day 2
"Naah that can't be right. People must be really good at doing this by now, it won't take long."

You'll realise soon enough.

## "I want to write a graphics driver" - Year 2
"Yay! I reached graphics that are vaguely close to Windows graphics. Now then, how do I use the graphics card and/or do video decode and playback...?"

Well done! Impressed you stuck at it. Keep going, I'm sure you don't need advice from me by now.

## After the graphics driver (or along the way)

- All the stuff to make use of the graphics
- Video encode/decode
- Actually making use of a graphics card
- UI / UX design and software stack

---

# Further Reading

*[acronym]: meaning

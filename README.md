# [FlingOS™](http://www.flingos.co.uk)

Welcome! This is the main repository for the FlingOS™ project. We used to be over on BitBucket but have shifted across to GitHub in the latter half of 2015. 
The FlingOS project is an educational operating system aiming to provide high-quality resources for learning OS and low-level development. You can find out more on our website over at [www.flingos.co.uk](http://www.flingos.co.uk). There you'll also find our [documentation](http://www.flingos.co.uk/docs/) and links to our tutorial videos. If you're wondering why we use C#, take a look at [this article](http://www.flingos.co.uk/docs/reference/Why-CSharp).

The FlingOS project is a three part approach to teaching OS and low-level development. You're currently looking at just one part - the code itself. The code acts as a sample codebase for people to learn from and compare to. The second part is our [conceptual articles](http://www.flingos.co.uk/docs), which explain all the OS and low-level technology in detail. The third part is our tutorials which are free, ~20min videos with complete resources, [available on YouTube](https://www.youtube.com/playlist?list=PLKbvCgwMcH7BX6Z8Bk1EuFwDa0WGkMnrz). You can find all these links and more on our [main website](http://www.flingos.co.uk).

## Getting Started

### Interested in learning OS/low-level development?
Take a look at our [Getting Started](http://www.flingos.co.uk/docs/reference/Getting-Started) article to learn how to write your own operating system.

### How do I learn from the FlingOS source code?
The FlingOS source code is here for you to look at, read and compare to. By reading the FlingOS articles and taking a look at our implementations, you should be able to write your own fairly easily.

### Interested in developing FlingOS?

[Join the team](http://www.flingos.co.uk/Develop#Join-the-team) and then [setup for development](http://www.flingos.co.uk/docs/reference/FlingOS).

### Interested in our ahead-of-time compiler?

If you'd just like to use our ahead-of-time compiler to write your own C#, VB.Net or F# operating system, please take a look at our [stable releases](http://www.flingos.co.uk/releases).

## Current progress

Hopefully you'll be able to see from our codebase ([structure is described here](https://github.com/FlingOS/FlingOS/wiki)) that FlingOS has reached a reasonably advanced state. However, since October 2015, we've been working hard to transform FlingOS from a single-tasking OS to a multi-tasking OS. We've reached the final stages of the transformation, with all the significant system calls now working and stable. However, most of the changes are still contained in the `develop` branch. We'll let you know via [our blog](http://blog.flingos.co.uk) when the changes are merged and details of the improvements. We'll also be adding more articles on multi-tasking and blog articles about some of the challenges we've had to overcome during the transformation work.

## A note on licenses
FlingOS is released under GPLv2 under UK law. This means you can't just copy and paste our code without keeping our copyright notice and you have to release your work as open-source if it includes our code. Our source code is also released without warranty and we accept no liability (within the restrictions of UK law). Please do not use our code for anything (particularly production or safety critical work) without testing and verifying it yourself.

##### Why did we choose GPLv2 not BSD, MIT or another, more permissive license? 
FlingOS is here for people to learn from by reading and comparing. We are not here to just supply out-of-the-box code so we don't allow people to just reuse our work. Also, we are providing a learning resource not a reference sample. By restricitng the use of our code it helps to prevent the widercommunity accidentally treating us a reference codebase.

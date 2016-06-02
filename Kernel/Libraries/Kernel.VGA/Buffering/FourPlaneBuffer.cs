#region LICENSE

// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//  Project owner: 
//		Email: edwardnutting@outlook.com
//		For paper mail address, please contact via email for details.
//
// ------------------------------------------------------------------------------ //

#endregion

using Drivers.Compiler.Attributes;
using Kernel.Framework;
using Kernel.Framework.Exceptions;

namespace Kernel.VGA.Buffering
{
    public unsafe class FourPlaneBuffer : Object, IVGABuffer
    {
        public uint Width
        {
            [NoDebug]
            [NoGC]
            get; private set; }
        public uint Height
        {
            [NoDebug]
            [NoGC]
            get; private set; }

        private readonly byte[] Plane1;
        private readonly byte[] Plane2;
        private readonly byte[] Plane3;
        private readonly byte[] Plane4;

        public FourPlaneBuffer(uint Width, uint Height)
        {
            this.Width = Width;
            this.Height = Height;

            // 1 bpp
            uint BufferSize = (Width*Height)/8;
            Plane1 = new byte[BufferSize];
            Plane2 = new byte[BufferSize];
            Plane3 = new byte[BufferSize];
            Plane4 = new byte[BufferSize];
        }

        [NoDebug]
        [NoGC]
        public void SetPixel(int X, int Y, Colour24Bit Colour)
        {
            uint Offset = (uint)(X / 8 + (Width / 8) * Y);

            X = X & 7;

            byte Mask = (byte)(0x80 >> X);
            
            // TODO: Proper colour to palette index translation
            uint ColourUI32 = Colour.Red;

            if ((0x1 & ColourUI32) != 0)
            {
                Plane1[Offset] |= Mask;
            }
            else
            {
                Plane1[Offset] &= (byte)~Mask;
            }

            if ((0x2 & ColourUI32) != 0)
            {
                Plane2[Offset] |= Mask;
            }
            else
            {
                Plane2[Offset] &= (byte)~Mask;
            }

            if ((0x4 & ColourUI32) != 0)
            {
                Plane3[Offset] |= Mask;
            }
            else
            {
                Plane3[Offset] &= (byte)~Mask;
            }

            if ((0x8 & ColourUI32) != 0)
            {
                Plane4[Offset] |= Mask;
            }
            else
            {
                Plane4[Offset] &= (byte)~Mask;
            }
        }

        [NoDebug]
        [NoGC]
        public void Clear(Colour24Bit Colour)
        {
            // TODO: Proper colour to palette index translation
            uint P1Val = (Colour.Red & 0x1) != 0 ? 0xFFFFFFFF : 0x0;
            uint P2Val = (Colour.Red & 0x2) != 0 ? 0xFFFFFFFF : 0x0;
            uint P3Val = (Colour.Red & 0x4) != 0 ? 0xFFFFFFFF : 0x0;
            uint P4Val = (Colour.Red & 0x8) != 0 ? 0xFFFFFFFF : 0x0;

            uint* Plane1Source = (uint*)((byte*)Utilities.ObjectUtilities.GetHandle(Plane1) + Array.FieldsBytesSize);
            uint* Plane2Source = (uint*)((byte*)Utilities.ObjectUtilities.GetHandle(Plane2) + Array.FieldsBytesSize);
            uint* Plane3Source = (uint*)((byte*)Utilities.ObjectUtilities.GetHandle(Plane3) + Array.FieldsBytesSize);
            uint* Plane4Source = (uint*)((byte*)Utilities.ObjectUtilities.GetHandle(Plane4) + Array.FieldsBytesSize);
            
            uint MaxOffset = (Width * Height) / 32;

            for (uint Offset = 0; Offset < MaxOffset; Offset++)
            {
                *Plane1Source++ = P1Val;
                *Plane2Source++ = P2Val;
                *Plane3Source++ = P3Val;
                *Plane4Source++ = P4Val;
            }
        }

        [NoDebug]
        [NoGC]
        public Colour24Bit GetPixel(int X, int Y)
        {
            uint Offset = (uint)(X / 8 + (Width / 8) * Y);

            X = X & 7;

            uint Mask = (byte)(0x80 >> X);
            
            uint Color = 0;

            if ((Plane1[Offset] & Mask) != 0)
            {
                Color |= 1;
            }

            if ((Plane2[Offset] & Mask) != 0)
            {
                Color |= 2;
            }

            if ((Plane3[Offset] & Mask) != 0)
            {
                Color |= 4;
            }

            if ((Plane4[Offset] & Mask) != 0)
            {
                Color |= 8;
            }

            // TODO: Proper palette index to colour translation
            return new Colour24Bit((byte)Color, 0, 0);
        }

        [NoDebug]
        [NoGC]
        public void CopyTo(VGA TheVGA)
        {
            uint* Plane1Source = (uint*)((byte*)Utilities.ObjectUtilities.GetHandle(Plane1) + Array.FieldsBytesSize);
            uint* Plane2Source = (uint*)((byte*)Utilities.ObjectUtilities.GetHandle(Plane2) + Array.FieldsBytesSize);
            uint* Plane3Source = (uint*)((byte*)Utilities.ObjectUtilities.GetHandle(Plane3) + Array.FieldsBytesSize);
            uint* Plane4Source = (uint*)((byte*)Utilities.ObjectUtilities.GetHandle(Plane4) + Array.FieldsBytesSize);

            uint* PlaneDestinationOrig = (uint*)TheVGA.FrameBuffer;
            
            uint MaxOffset = (Width * Height) / 32;

            for (byte Plane = 0; Plane < 4; Plane++)
            {
                TheVGA.SelectPlane(Plane);

                uint* PlaneSource = Plane == 0
                    ? Plane1Source
                    : Plane == 1
                        ? Plane2Source
                        : Plane == 2
                            ? Plane3Source
                            : Plane4Source;
                uint* PlaneDestination = PlaneDestinationOrig;
                for (uint Offset = 0; Offset < MaxOffset; Offset++)
                {
                    *PlaneDestination++ = *PlaneSource++;
                }
            }
        }

        [NoDebug]
        [NoGC]
        public void BlendTo(IVGABuffer Destination, BlendingModes BlendMode)
        {
            if (Destination.Width != Width ||
                Destination.Height != Height)
            {
                ExceptionMethods.Throw(new ArgumentException("Destination buffer is not the same size as the source buffer!"));
            }

            FourPlaneBuffer CastDestination = Destination as FourPlaneBuffer;
            if (CastDestination != null)
            {
                // Pointer-based code is vastly more efficient because it skips the null ref, element type and array bounds checks
                //  - We know we won't be dealing with null refs because the fields are initialised in constructor and are read-only
                //      : A null allocation of any of the planes would have thrown an exception at construction
                //  - We know the types are the same
                //  - We've already checked the bounds (width/height above)

                uint* Plane1Source = (uint*)((byte*)Utilities.ObjectUtilities.GetHandle(Plane1) + Array.FieldsBytesSize);
                uint* Plane2Source = (uint*)((byte*)Utilities.ObjectUtilities.GetHandle(Plane2) + Array.FieldsBytesSize);
                uint* Plane3Source = (uint*)((byte*)Utilities.ObjectUtilities.GetHandle(Plane3) + Array.FieldsBytesSize);
                uint* Plane4Source = (uint*)((byte*)Utilities.ObjectUtilities.GetHandle(Plane4) + Array.FieldsBytesSize);

                uint* Plane1Destination = (uint*)((byte*)Utilities.ObjectUtilities.GetHandle(CastDestination.Plane1) + Array.FieldsBytesSize);
                uint* Plane2Destination = (uint*)((byte*)Utilities.ObjectUtilities.GetHandle(CastDestination.Plane2) + Array.FieldsBytesSize);
                uint* Plane3Destination = (uint*)((byte*)Utilities.ObjectUtilities.GetHandle(CastDestination.Plane3) + Array.FieldsBytesSize);
                uint* Plane4Destination = (uint*)((byte*)Utilities.ObjectUtilities.GetHandle(CastDestination.Plane4) + Array.FieldsBytesSize);

                uint MaxOffset = (Width * Height) / 32;

                //TODO: Erm, applying blend modes to the palette index values stored in the planes...not right but it'll do

                switch (BlendMode)
                {
                    case BlendingModes.Normal:
                        {
                            for (uint Offset = 0; Offset < MaxOffset; Offset++)
                            {
                                *Plane1Destination++ = *Plane1Source++;
                                *Plane2Destination++ = *Plane2Source++;
                                *Plane3Destination++ = *Plane3Source++;
                                *Plane4Destination++ = *Plane4Source++;
                            }
                        }
                        break;
                    case BlendingModes.Multiply:
                        {
                            // See https://en.wikipedia.org/wiki/Blend_modes#Screen
                            //  This "direct" implementation is inefficient
                            for (uint Offset = 0; Offset < MaxOffset; Offset++)
                            {
                                for (int Bit = 0; Bit < 32; Bit++)
                                {
                                    uint Source = (((*Plane4Source >> Bit) & 0x1) << 3) | (((*Plane3Source >> Bit) & 0x1) << 2) | (((*Plane2Source >> Bit) & 0x1) << 1) |
                                                  ((*Plane1Source >> Bit) & 0x1);
                                    uint Dest = (((*Plane4Destination >> Bit) & 0x1) << 3) | (((*Plane3Destination >> Bit) & 0x1) << 2) |
                                                (((*Plane2Destination >> Bit) & 0x1) << 1) | ((*Plane1Destination >> Bit) & 0x1);

                                    Dest = Source * Dest / 15;

                                    uint Mask = ~(1u << Bit);
                                    *Plane1Destination = (*Plane1Destination & Mask) | ((Dest & 0x1) << Bit);
                                    *Plane2Destination = (*Plane2Destination & Mask) | (((Dest & 0x2) >> 1) << Bit);
                                    *Plane3Destination = (*Plane3Destination & Mask) | (((Dest & 0x4) >> 2) << Bit);
                                    *Plane4Destination = (*Plane4Destination & Mask) | (((Dest & 0x8) >> 3) << Bit);
                                }

                                Plane1Source++;
                                Plane2Source++;
                                Plane3Source++;
                                Plane4Source++;

                                Plane1Destination++;
                                Plane2Destination++;
                                Plane3Destination++;
                                Plane4Destination++;
                            }
                        }
                        break;
                    case BlendingModes.Screen:
                        {
                            // See https://en.wikipedia.org/wiki/Blend_modes#Screen
                            //  This "direct" implementation is inefficient
                            for (uint Offset = 0; Offset < MaxOffset; Offset++)
                            {
                                for (int Bit = 0; Bit < 32; Bit++)
                                {
                                    uint Source = (((*Plane4Source >> Bit) & 0x1) << 3) | (((*Plane3Source >> Bit) & 0x1) << 2) | (((*Plane2Source >> Bit) & 0x1) << 1) |
                                                  ((*Plane1Source >> Bit) & 0x1);
                                    uint Dest = (((*Plane4Destination >> Bit) & 0x1) << 3) | (((*Plane3Destination >> Bit) & 0x1) << 2) |
                                                (((*Plane2Destination >> Bit) & 0x1) << 1) | ((*Plane1Destination >> Bit) & 0x1);
                                    
                                    Dest = 15 - ((15-Source)*(15-Dest)/15);

                                    uint Mask = ~(1u << Bit);
                                    *Plane1Destination = (*Plane1Destination & Mask) | ((Dest & 0x1) << Bit);
                                    *Plane2Destination = (*Plane2Destination & Mask) | (((Dest & 0x2) >> 1) << Bit);
                                    *Plane3Destination = (*Plane3Destination & Mask) | (((Dest & 0x4) >> 2) << Bit);
                                    *Plane4Destination = (*Plane4Destination & Mask) | (((Dest & 0x8) >> 3) << Bit);
                                }

                                Plane1Source++;
                                Plane2Source++;
                                Plane3Source++;
                                Plane4Source++;

                                Plane1Destination++;
                                Plane2Destination++;
                                Plane3Destination++;
                                Plane4Destination++;
                            }
                        }
                        break;
                    default:
                        ExceptionMethods.Throw(new NotSupportedException("Unsupported blend mode requested of FourPlaneBuffer!"));
                        break;
                }
            }
            else
            {
                ExceptionMethods.Throw(new ArgumentException("Destination buffer is not also a Four Plane Buffer! (Could use inefficient get/set based copy.)"));
            }
        }
    }
}

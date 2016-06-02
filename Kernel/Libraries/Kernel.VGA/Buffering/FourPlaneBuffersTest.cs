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
using Kernel.Framework.Processes;
using Kernel.VGA.Configurations.Graphical;

namespace Kernel.VGA.Buffering
{
    public static class FourPlaneBuffersTest
    {
        public static void Test1()
        {
            bool PrimaryOutputEnabled = BasicConsole.PrimaryOutputEnabled;
            BasicConsole.PrimaryOutputEnabled = false;

            BasicConsole.WriteLine("Test1 > Configuring...");
            VGA TheVGA = VGA.GetConfiguredInstance(G_640x480x4.Instance, null);
            BasicConsole.WriteLine("Test1 > Setting palettes...");
            TheVGA.SetCGAPalette();

            BasicConsole.WriteLine("Test1 > Getting dimensions...");
            uint Width = TheVGA.Configuration.Width;
            uint Height = TheVGA.Configuration.Height;

            BasicConsole.WriteLine("Test1 > Creating buffers...");
            FourPlaneBuffer BackBuffer = new FourPlaneBuffer(Width, Height);
            FourPlaneBuffer ForeBuffer = new FourPlaneBuffer(Width, Height);
            FourPlaneBuffer ForeInvBuffer = new FourPlaneBuffer(Width, Height);


            BasicConsole.WriteLine("Test1 > Creating back colour...");
            Colour24Bit BackColour = new Colour24Bit(4, 0, 0);
            BasicConsole.WriteLine("Test1 > Clearing back buffer...");
            BackBuffer.Clear(BackColour);

            for (byte Col = 0; Col < 16; Col++)
            {
                BasicConsole.WriteLine("Test1 > Creating colours...");
                Colour24Bit ForeColour = new Colour24Bit(Col, 0, 0);
                int BlendResult = (225 - ((15 - BackColour.Red)*(15 - ForeColour.Red)))/15; // Screen : 12
                Colour24Bit ForeInvColour = new Colour24Bit((byte)((BackColour.Red*15)/BlendResult), 0, 0);
                    // Inverse multiply : 5
                Colour24Bit ForeBlankColour = new Colour24Bit(0, 0, 0);
                Colour24Bit ForeInvBlankColour = new Colour24Bit(15, 0, 0);

                BasicConsole.WriteLine("Test1 > Clearing buffers...");
                ForeBuffer.Clear(ForeBlankColour);
                ForeInvBuffer.Clear(ForeInvBlankColour);

                int Factor = 40;
                int XStep = (int)Width/Factor;
                int YStep = (int)Height/Factor;
                int SqSizeX = XStep;
                int SqSizeY = YStep;

                BasicConsole.WriteLine("Test1 > Running test...");

                int X, Y;
                for (X = 0, Y = 0; X < Width - (SqSizeX - 1) && Y < Height - (SqSizeY - 1); X += XStep, Y += YStep)
                {
                    DrawRectangle(X, SqSizeX, Y, SqSizeY, ForeBuffer, ForeColour, ForeInvBuffer, ForeInvColour,
                        BackBuffer,
                        TheVGA, ForeBlankColour, ForeInvBlankColour);
                }

                for (X = 0, Y = (int)Height - SqSizeY;
                    X < Width - (SqSizeX - 1) && Y < Height - (SqSizeY - 1);
                    X += XStep, Y -= YStep)
                {
                    DrawRectangle(X, SqSizeX, Y, SqSizeY, ForeBuffer, ForeColour, ForeInvBuffer, ForeInvColour,
                        BackBuffer,
                        TheVGA, ForeBlankColour, ForeInvBlankColour);
                }

                for (SqSizeX = 2*XStep, SqSizeY = 2*YStep, X = (int)Width/2 - XStep, Y = (int)Height/2 - YStep;
                    SqSizeX < Width && SqSizeY < Height;
                    SqSizeX += 2*XStep, SqSizeY += 2*YStep, X -= XStep, Y -= YStep)
                {
                    DrawRectangle(X, SqSizeX, Y, SqSizeY, ForeBuffer, ForeColour, ForeInvBuffer, ForeInvColour,
                        BackBuffer,
                        TheVGA, ForeBlankColour, ForeInvBlankColour);
                }

                for (;
                    SqSizeX > 0 && SqSizeY > 0;
                    SqSizeX -= 2*XStep, SqSizeY -= 2*YStep, X += XStep, Y += YStep)
                {
                    DrawRectangle(X, SqSizeX, Y, SqSizeY, ForeBuffer, ForeColour, ForeInvBuffer, ForeInvColour,
                        BackBuffer,
                        TheVGA, ForeBlankColour, ForeInvBlankColour);
                }
            }

            BasicConsole.WriteLine("Test1 > Finished.");
            BasicConsole.PrimaryOutputEnabled = PrimaryOutputEnabled;
        }

        [NoDebug]
        [NoGC]
        private static void DrawRectangle(int X, int SqSizeX, int Y, int SqSizeY, FourPlaneBuffer ForeBuffer,
            Colour24Bit ForeColour, FourPlaneBuffer ForeInvBuffer, Colour24Bit ForeInvColour, FourPlaneBuffer BackBuffer,
            VGA TheVGA, Colour24Bit ForeBlankColour, Colour24Bit ForeInvBlankColour)
        {
            //BasicConsole.WriteLine((String)"X: " + X + ", Y: " + Y);
            int MaxX = X + SqSizeX;
            int MaxY = Y + SqSizeY;

            //Draw square
            //BasicConsole.WriteLine("Test1 > Draw square");
            for (int SqX = X; SqX < MaxX; SqX++)
            {
                for (int SqY = Y; SqY < MaxY; SqY++)
                {
                    ForeBuffer.SetPixel(SqX, SqY, ForeColour);
                    ForeInvBuffer.SetPixel(SqX, SqY, ForeInvColour);
                }
            }

            //BasicConsole.WriteLine("Test1 > Blend (Screen)");
            ForeBuffer.BlendTo(BackBuffer, BlendingModes.Screen);
            // Square in Back Buffer is colour value "13" after blending

            //BasicConsole.WriteLine("Test1 > Copy (1)");
            BackBuffer.CopyTo(TheVGA);

            //BasicConsole.WriteLine("Test1 > Blend (Multiply)");
            ForeInvBuffer.BlendTo(BackBuffer, BlendingModes.Multiply);

            //BasicConsole.WriteLine("Test1 > Copy (2)");
            //BackBuffer.CopyTo(TheVGA);

            //Clear square
            //BasicConsole.WriteLine("Test1 > Clear square");
            for (int SqX = X; SqX < MaxX; SqX++)
            {
                for (int SqY = Y; SqY < MaxY; SqY++)
                {
                    ForeBuffer.SetPixel(SqX, SqY, ForeBlankColour);
                    ForeInvBuffer.SetPixel(SqX, SqY, ForeInvBlankColour);
                }
            }

            //BasicConsole.WriteLine("Test1 > Next...");
        }
    }
}

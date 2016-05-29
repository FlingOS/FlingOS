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


#define EHCI_TESTS
#undef EHCI_TESTS //Note: Also comment out the undef in EHCI.cs

namespace Kernel.USB.HCIs
{
#if DEBUG && EHCI_TESTS
    public static unsafe class EHCITesting
    {
        public static int errors = 0;
        public static int warnings = 0;

    #region Memory Tests

        public static void Test_PointerManipulation()
        {
            Framework.String testName = "Ptr Manipulation";
            DBGMSG(testName, "START");

            errors = 0;
            warnings = 0;

            byte* rootPtr = (byte*)Framework.Heap.Alloc(4096, 32);
            try
            {
                DBGMSG(testName, ((Framework.String)"rootPtr: ") + (uint)rootPtr);

                if (!Validate_PointerBoundaryAlignment(rootPtr, 32))
                {
                    DBGERR(testName, "Pointer not aligned on boundary correctly!");
                }

                byte* retPtr = ReturnPointer(rootPtr);
                if (retPtr != rootPtr)
                {
                    DBGERR(testName, "Passing and returning pointer via method failed.");
                }

                byte* shiftedPtr = (byte*)((uint*)rootPtr + 1);
                if (((uint)shiftedPtr) != ((uint)rootPtr) + 4)
                {
                    DBGERR(testName, ((Framework.String)"Shifted pointer not shifted correctly! shiftedPtr: ") + (uint)shiftedPtr);
                }

                EHCITestingObject testObj = new EHCITestingObject();
                testObj.ptr1 = rootPtr;
                testObj.ptr2 = (uint*)rootPtr;
                testObj.ptr3 = (ulong*)rootPtr;
                testObj.ptr4 = rootPtr;
                if (testObj.ptr1 != rootPtr)
                {
                    DBGERR(testName, ((Framework.String)"Storing test pointer 1 failed! testObj.ptr1: ") + (uint)testObj.ptr1);
                }
                if (testObj.ptr2 != rootPtr)
                {
                    DBGERR(testName, ((Framework.String)"Storing test pointer 2 failed! testObj.ptr2: ") + (uint)testObj.ptr2);
                }
                if (testObj.ptr3 != rootPtr)
                {
                    DBGERR(testName, ((Framework.String)"Storing test pointer 3 failed! testObj.ptr3: ") + (uint)testObj.ptr3);
                }
                if (testObj.ptr4 != rootPtr)
                {
                    DBGERR(testName, ((Framework.String)"Storing test pointer 4 failed! testObj.ptr4: ") + (uint)testObj.ptr4);
                }
            }
            catch
            {
                errors++;
                BasicConsole.SetTextColour(BasicConsole.warning_colour);
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                BasicConsole.SetTextColour(BasicConsole.default_colour);
            }
            finally
            {
                Framework.Heap.Free(rootPtr);
            }

            if (errors > 0)
            {
                DBGERR(testName, ((Framework.String)"Test failed! Errors: ") + errors + " Warnings: " + warnings);
            }
            else
            {
                if (warnings > 0)
                {
                    DBGWRN(testName, ((Framework.String)"Test passed with warnings: ") + warnings);
                }
                else
                {
                    DBGMSG(testName, "Test passed.");
                }
            }

            DBGMSG(testName, "END");

            BasicConsole.DelayOutput(1);
        }
        private static byte* ReturnPointer(byte* aPtr)
        {
            return aPtr;
        }
        
        public static void Test_StructValueSetting()
        {
            Framework.String testName = "Strct Value Setting";
            DBGMSG(testName, "START");

            errors = 0;
            warnings = 0;

            uint structSize = (uint)sizeof(EHCITestingStruct);
            if (structSize != 8 * 4)
            {
                DBGERR(testName, ((Framework.String)"Struct size incorrect! structSize: ") + structSize);
            }
            if (errors == 0)
            {
                EHCITestingStruct* rootPtr = (EHCITestingStruct*)Framework.Heap.Alloc(structSize);
                byte* bRootPtr = (byte*)rootPtr;
                try
                {
                    DBGMSG(testName, ((Framework.String)"rootPtr: ") + (uint)rootPtr);

                    rootPtr->u1 = 0xDEADBEEF;
                    if (rootPtr->u1 != 0xDEADBEEF ||
                       bRootPtr[0] != 0xEF ||
                       bRootPtr[1] != 0xBE ||
                       bRootPtr[2] != 0xAD ||
                       bRootPtr[3] != 0xDE)
                    {
                        DBGERR(testName, "Getting/setting struct u1 failed!");
                    }


                    rootPtr->u2 = 0x12345678;
                    if (rootPtr->u1 != 0xDEADBEEF ||
                       bRootPtr[0] != 0xEF ||
                       bRootPtr[1] != 0xBE ||
                       bRootPtr[2] != 0xAD ||
                       bRootPtr[3] != 0xDE)
                    {
                        DBGERR(testName, "Getting/setting struct u2 failed! Affected u1 value.");
                    }
                    if (rootPtr->u2 != 0x12345678 ||
                       bRootPtr[4] != 0x78 ||
                       bRootPtr[5] != 0x56 ||
                       bRootPtr[6] != 0x34 ||
                       bRootPtr[7] != 0x12)
                    {
                        DBGERR(testName, "Getting/setting struct u2 failed!");
                    }


                    rootPtr->u3 = 0xBEEFDEAD;
                    if (rootPtr->u1 != 0xDEADBEEF ||
                       bRootPtr[0] != 0xEF ||
                       bRootPtr[1] != 0xBE ||
                       bRootPtr[2] != 0xAD ||
                       bRootPtr[3] != 0xDE)
                    {
                        DBGERR(testName, "Getting/setting struct u3 failed! Affected u1 value.");
                    }
                    if (rootPtr->u2 != 0x12345678 ||
                       bRootPtr[4] != 0x78 ||
                       bRootPtr[5] != 0x56 ||
                       bRootPtr[6] != 0x34 ||
                       bRootPtr[7] != 0x12)
                    {
                        DBGERR(testName, "Getting/setting struct u3 failed! Affected u2 value.");
                    }
                    if (rootPtr->u3 != 0xBEEFDEAD ||
                       bRootPtr[8] != 0xAD ||
                       bRootPtr[9] != 0xDE ||
                       bRootPtr[10] != 0xEF ||
                       bRootPtr[11] != 0xBE)
                    {
                        DBGERR(testName, "Getting/setting struct u3 failed!");
                    }


                    rootPtr->u4 = 0x09876543;
                    if (rootPtr->u1 != 0xDEADBEEF ||
                       bRootPtr[0] != 0xEF ||
                       bRootPtr[1] != 0xBE ||
                       bRootPtr[2] != 0xAD ||
                       bRootPtr[3] != 0xDE)
                    {
                        DBGERR(testName, "Getting/setting struct u4 failed! Affected u1 value.");
                    }
                    if (rootPtr->u2 != 0x12345678 ||
                       bRootPtr[4] != 0x78 ||
                       bRootPtr[5] != 0x56 ||
                       bRootPtr[6] != 0x34 ||
                       bRootPtr[7] != 0x12)
                    {
                        DBGERR(testName, "Getting/setting struct u4 failed! Affected u2 value.");
                    }
                    if (rootPtr->u3 != 0xBEEFDEAD ||
                       bRootPtr[8] != 0xAD ||
                       bRootPtr[9] != 0xDE ||
                       bRootPtr[10] != 0xEF ||
                       bRootPtr[11] != 0xBE)
                    {
                        DBGERR(testName, "Getting/setting struct u4 failed! Affected u3 value.");
                    }
                    if (rootPtr->u4 != 0x09876543 ||
                       bRootPtr[12] != 0x43 ||
                       bRootPtr[13] != 0x65 ||
                       bRootPtr[14] != 0x87 ||
                       bRootPtr[15] != 0x09)
                    {
                        DBGERR(testName, "Getting/setting struct u4 failed!");
                    }

                    Test_StructValueSettingAsArg("Strct Val Set by val", *rootPtr);
                    Test_StructValueSettingAsArg("Strct Val Set by ptr", rootPtr, bRootPtr);
                }
                catch
                {
                    errors++;
                    BasicConsole.SetTextColour(BasicConsole.warning_colour);
                    BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                    BasicConsole.SetTextColour(BasicConsole.default_colour);
                }
                finally
                {
                    Framework.Heap.Free(rootPtr);
                }
            }

            if (errors > 0)
            {
                DBGERR(testName, ((Framework.String)"Test failed! Errors: ") + errors + " Warnings: " + warnings);
            }
            else
            {
                if (warnings > 0)
                {
                    DBGWRN(testName, ((Framework.String)"Test passed with warnings: ") + warnings);
                }
                else
                {
                    DBGMSG(testName, "Test passed.");
                }
            }

            DBGMSG(testName, "END");

            BasicConsole.DelayOutput(1);
        }
        private static void Test_StructValueSettingAsArg(Framework.String testName, EHCITestingStruct root)
        {
            byte* bRootPtr = (byte*)&root;

            for (int i = 0; i < sizeof(EHCITestingStruct); i++)
            {
                bRootPtr[i] = 0;
            }
                root.u1 = 0xDEADBEEF;
            if (root.u1 != 0xDEADBEEF ||
               bRootPtr[0] != 0xEF ||
               bRootPtr[1] != 0xBE ||
               bRootPtr[2] != 0xAD ||
               bRootPtr[3] != 0xDE)
            {
                DBGERR(testName, "Getting/setting struct u1 failed!");
            }


            root.u2 = 0x12345678;
            if (root.u1 != 0xDEADBEEF ||
               bRootPtr[0] != 0xEF ||
               bRootPtr[1] != 0xBE ||
               bRootPtr[2] != 0xAD ||
               bRootPtr[3] != 0xDE)
            {
                DBGERR(testName, "Getting/setting struct u2 failed! Affected u1 value.");
            }
            if (root.u2 != 0x12345678 ||
               bRootPtr[4] != 0x78 ||
               bRootPtr[5] != 0x56 ||
               bRootPtr[6] != 0x34 ||
               bRootPtr[7] != 0x12)
            {
                DBGERR(testName, "Getting/setting struct u2 failed!");
            }


            root.u3 = 0xBEEFDEAD;
            if (root.u1 != 0xDEADBEEF ||
               bRootPtr[0] != 0xEF ||
               bRootPtr[1] != 0xBE ||
               bRootPtr[2] != 0xAD ||
               bRootPtr[3] != 0xDE)
            {
                DBGERR(testName, "Getting/setting struct u3 failed! Affected u1 value.");
            }
            if (root.u2 != 0x12345678 ||
               bRootPtr[4] != 0x78 ||
               bRootPtr[5] != 0x56 ||
               bRootPtr[6] != 0x34 ||
               bRootPtr[7] != 0x12)
            {
                DBGERR(testName, "Getting/setting struct u3 failed! Affected u2 value.");
            }
            if (root.u3 != 0xBEEFDEAD ||
               bRootPtr[8] != 0xAD ||
               bRootPtr[9] != 0xDE ||
               bRootPtr[10] != 0xEF ||
               bRootPtr[11] != 0xBE)
            {
                DBGERR(testName, "Getting/setting struct u3 failed!");
            }


            root.u4 = 0x09876543;
            if (root.u1 != 0xDEADBEEF ||
               bRootPtr[0] != 0xEF ||
               bRootPtr[1] != 0xBE ||
               bRootPtr[2] != 0xAD ||
               bRootPtr[3] != 0xDE)
            {
                DBGERR(testName, "Getting/setting struct u4 failed! Affected u1 value.");
            }
            if (root.u2 != 0x12345678 ||
               bRootPtr[4] != 0x78 ||
               bRootPtr[5] != 0x56 ||
               bRootPtr[6] != 0x34 ||
               bRootPtr[7] != 0x12)
            {
                DBGERR(testName, "Getting/setting struct u4 failed! Affected u2 value.");
            }
            if (root.u3 != 0xBEEFDEAD ||
               bRootPtr[8] != 0xAD ||
               bRootPtr[9] != 0xDE ||
               bRootPtr[10] != 0xEF ||
               bRootPtr[11] != 0xBE)
            {
                DBGERR(testName, "Getting/setting struct u4 failed! Affected u3 value.");
            }
            if (root.u4 != 0x09876543 ||
               bRootPtr[12] != 0x43 ||
               bRootPtr[13] != 0x65 ||
               bRootPtr[14] != 0x87 ||
               bRootPtr[15] != 0x09)
            {
                DBGERR(testName, "Getting/setting struct u4 failed!");
            }
        }
        private static void Test_StructValueSettingAsArg(Framework.String testName, EHCITestingStruct* rootPtr, byte* bRootPtr)
        {
            for (int i = 0; i < sizeof(EHCITestingStruct); i++)
            {
                bRootPtr[i] = 0;
            }

            rootPtr->u1 = 0xDEADBEEF;
            if (rootPtr->u1 != 0xDEADBEEF ||
               bRootPtr[0] != 0xEF ||
               bRootPtr[1] != 0xBE ||
               bRootPtr[2] != 0xAD ||
               bRootPtr[3] != 0xDE)
            {
                DBGERR(testName, "Getting/setting struct u1 failed!");
            }


            rootPtr->u2 = 0x12345678;
            if (rootPtr->u1 != 0xDEADBEEF ||
               bRootPtr[0] != 0xEF ||
               bRootPtr[1] != 0xBE ||
               bRootPtr[2] != 0xAD ||
               bRootPtr[3] != 0xDE)
            {
                DBGERR(testName, "Getting/setting struct u2 failed! Affected u1 value.");
            }
            if (rootPtr->u2 != 0x12345678 ||
               bRootPtr[4] != 0x78 ||
               bRootPtr[5] != 0x56 ||
               bRootPtr[6] != 0x34 ||
               bRootPtr[7] != 0x12)
            {
                DBGERR(testName, "Getting/setting struct u2 failed!");
            }


            rootPtr->u3 = 0xBEEFDEAD;
            if (rootPtr->u1 != 0xDEADBEEF ||
               bRootPtr[0] != 0xEF ||
               bRootPtr[1] != 0xBE ||
               bRootPtr[2] != 0xAD ||
               bRootPtr[3] != 0xDE)
            {
                DBGERR(testName, "Getting/setting struct u3 failed! Affected u1 value.");
            }
            if (rootPtr->u2 != 0x12345678 ||
               bRootPtr[4] != 0x78 ||
               bRootPtr[5] != 0x56 ||
               bRootPtr[6] != 0x34 ||
               bRootPtr[7] != 0x12)
            {
                DBGERR(testName, "Getting/setting struct u3 failed! Affected u2 value.");
            }
            if (rootPtr->u3 != 0xBEEFDEAD ||
               bRootPtr[8] != 0xAD ||
               bRootPtr[9] != 0xDE ||
               bRootPtr[10] != 0xEF ||
               bRootPtr[11] != 0xBE)
            {
                DBGERR(testName, "Getting/setting struct u3 failed!");
            }


            rootPtr->u4 = 0x09876543;
            if (rootPtr->u1 != 0xDEADBEEF ||
               bRootPtr[0] != 0xEF ||
               bRootPtr[1] != 0xBE ||
               bRootPtr[2] != 0xAD ||
               bRootPtr[3] != 0xDE)
            {
                DBGERR(testName, "Getting/setting struct u4 failed! Affected u1 value.");
            }
            if (rootPtr->u2 != 0x12345678 ||
               bRootPtr[4] != 0x78 ||
               bRootPtr[5] != 0x56 ||
               bRootPtr[6] != 0x34 ||
               bRootPtr[7] != 0x12)
            {
                DBGERR(testName, "Getting/setting struct u4 failed! Affected u2 value.");
            }
            if (rootPtr->u3 != 0xBEEFDEAD ||
               bRootPtr[8] != 0xAD ||
               bRootPtr[9] != 0xDE ||
               bRootPtr[10] != 0xEF ||
               bRootPtr[11] != 0xBE)
            {
                DBGERR(testName, "Getting/setting struct u4 failed! Affected u3 value.");
            }
            if (rootPtr->u4 != 0x09876543 ||
               bRootPtr[12] != 0x43 ||
               bRootPtr[13] != 0x65 ||
               bRootPtr[14] != 0x87 ||
               bRootPtr[15] != 0x09)
            {
                DBGERR(testName, "Getting/setting struct u4 failed!");
            }
        }

        #endregion


    #region Wrapper Class Tests

        public static void Test_QueueHeadWrapper()
        {
            Framework.String testName = "Queue Head Wrapper";
            DBGMSG(testName, "START");

            errors = 0;
            warnings = 0;

            EHCI_QueueHead qh = new EHCI_QueueHead();
            try
            {
                byte* pQH = (byte*)qh.queueHead;

                //Verifications done via two methods:
                //  1. Check value from pointer & manual shifting to confirm set properly
                //  2. Check value from "get" method to confirm reading properly
                //  3. For boolean types, also test & verify setting to false!

                qh.Active = true;
                if ((pQH[0x18u] & 0x80u) == 0)
                {
                    DBGERR(testName, "Active - Failed to set to true.");
                }
                else
                {
                    if (!qh.Active)
                    {
                        DBGERR(testName, "Active - Failed to read as true.");
                    }
                    else
                    {
                        pQH[0x18u] = 0xFF;
                        qh.Active = false;
                        if ((pQH[0x18u] & 0x80u) != 0)
                        {
                            DBGERR(testName, "Active - Failed to set to false.");
                        }
                        else
                        {
                            if (qh.Active)
                            {
                                DBGERR(testName, "Active - Failed to read as false.");
                            }
                        }
                    }
                }

                qh.ControlEndpointFlag = true;
                if ((pQH[0x07u] & 0x08u) == 0)
                {
                    DBGERR(testName, "ControlEndpointFlag - Failed to set to true.");
                }
                else
                {
                    if (!qh.ControlEndpointFlag)
                    {
                        DBGERR(testName, "ControlEndpointFlag - Failed to read as true.");
                    }
                    else
                    {
                        pQH[0x07u] = 0xFF;
                        qh.ControlEndpointFlag = false;
                        if ((pQH[0x07u] & 0x08u) != 0)
                        {
                            DBGERR(testName, "ControlEndpointFlag - Failed to set to false.");
                        }
                        else
                        {
                            if (qh.ControlEndpointFlag)
                            {
                                DBGERR(testName, "ControlEndpointFlag - Failed to read as false.");
                            }
                        }
                    }
                }

                qh.CurrentqTDPointer = (EHCI_qTD_Struct*)0xDEADBEFFu;
                //- Read back should equal 0xDEADBEE0
                if ((pQH[0x0Cu] & 0xF0u) != 0xF0u ||
                     pQH[0x0Du]          != 0xBEu ||
                     pQH[0x0Eu]          != 0xADu ||
                     pQH[0x0Fu]          != 0xDEu)
                {
                    DBGERR(testName, "CurrentqTDPointer - Failed to set.");
                }
                else
                {
                    if ((uint)qh.CurrentqTDPointer != 0xDEADBEF0u)
                    {
                        DBGERR(testName, "CurrentqTDPointer - Failed to read.");
                    }
                }

                qh.DataToggleControl = true;
                if ((pQH[0x05u] & 0x40u) == 0)
                {
                    DBGERR(testName, "DataToggleControl - Failed to set to true.");
                }
                else
                {
                    if (!qh.DataToggleControl)
                    {
                        DBGERR(testName, "DataToggleControl - Failed to read as true.");
                    }
                    else
                    {
                        pQH[0x05u] = 0xFF;
                        qh.DataToggleControl = false;
                        if ((pQH[0x05u] & 0x40u) != 0)
                        {
                            DBGERR(testName, "DataToggleControl - Failed to set to false.");
                        }
                        else
                        {
                            if (qh.DataToggleControl)
                            {
                                DBGERR(testName, "DataToggleControl - Failed to read as false.");
                            }
                        }
                    }
                }

                qh.DeviceAddress = 0xDE;
                if ((pQH[0x04u] & 0x7Fu) != 0x5Eu)
                {
                    DBGERR(testName, "DeviceAddress - Failed to set.");
                }
                else
                {
                    if ((uint)qh.DeviceAddress != 0x5Eu)
                    {
                        DBGERR(testName, "DeviceAddress - Failed to read.");
                    }
                }

                qh.EndpointNumber = 0xBF;
                //Shift!
                if ((pQH[0x05u] & 0x0Fu) != 0x0Fu)
                {
                    DBGERR(testName, "EndpointNumber - Failed to set.");
                }
                else
                {
                    if ((uint)qh.EndpointNumber != 0x0Fu)
                    {
                        DBGERR(testName, "EndpointNumber - Failed to read.");
                    }
                }

                qh.EndpointSpeed = 0xB3;
                //Shift!
                if ((pQH[0x05u] & 0x30u) != 0x30u)
                {
                    DBGERR(testName, "EndpointSpeed - Failed to set.");
                }
                else
                {
                    if ((uint)qh.EndpointSpeed != 0x03u)
                    {
                        DBGERR(testName, "EndpointSpeed - Failed to read.");
                    }
                }

                qh.HeadOfReclamationList = true;
                if ((pQH[0x05u] & 0x80u) == 0)
                {
                    DBGERR(testName, "HeadOfReclamationList - Failed to set to true.");
                }
                else
                {
                    if (!qh.HeadOfReclamationList)
                    {
                        DBGERR(testName, "HeadOfReclamationList - Failed to read as true.");
                    }
                    else
                    {
                        pQH[0x05u] = 0xFF;
                        qh.HeadOfReclamationList = false;
                        if ((pQH[0x05u] & 0x80u) != 0)
                        {
                            DBGERR(testName, "HeadOfReclamationList - Failed to set to false.");
                        }
                        else
                        {
                            if (qh.HeadOfReclamationList)
                            {
                                DBGERR(testName, "HeadOfReclamationList - Failed to read as false.");
                            }
                        }
                    }
                }

                qh.HighBandwidthPipeMultiplier = 0xDF;
                //Shift!
                if ((pQH[0x0Bu] & 0xC0u) != 0xC0u)
                {
                    DBGERR(testName, "HighBandwidthPipeMultiplier - Failed to set.");
                }
                else
                {
                    if ((uint)qh.HighBandwidthPipeMultiplier != 0x03u)
                    {
                        DBGERR(testName, "HighBandwidthPipeMultiplier - Failed to read.");
                    }
                }

                qh.HorizontalLinkPointer = (EHCI_QueueHead_Struct*)0xDEADBEFE;
                if ((pQH[0x00u] & 0xE0u) != 0xE0u ||
                     pQH[0x01u]          != 0xBEu ||
                     pQH[0x02u]          != 0xADu ||
                     pQH[0x03u]          != 0xDEu)
                {
                    DBGERR(testName, "HorizontalLinkPointer - Failed to set.");
                }
                else
                {
                    if ((uint)qh.HorizontalLinkPointer != 0xDEADBEE0u)
                    {
                        DBGERR(testName, "HorizontalLinkPointer - Failed to read.");
                    }
                }

                qh.HubAddr = 0xBE;
                //Shift!
                if ((pQH[0x0Au] & 0x7Fu) != 0x3Eu)
                {
                    DBGERR(testName, "HubAddr - Failed to set.");
                }
                else
                {
                    if ((uint)qh.HubAddr != 0x3Eu)
                    {
                        DBGERR(testName, "HubAddr - Failed to read.");
                    }
                }

                qh.InactiveOnNextTransaction = true;
                if ((pQH[0x04u] & 0x80u) == 0)
                {
                    DBGERR(testName, "InactiveOnNextTransaction - Failed to set to true.");
                }
                else
                {
                    if (!qh.InactiveOnNextTransaction)
                    {
                        DBGERR(testName, "InactiveOnNextTransaction - Failed to read as true.");
                    }
                    else
                    {
                        pQH[0x04u] = 0xFF;
                        qh.InactiveOnNextTransaction = false;
                        if ((pQH[0x04u] & 0x80u) != 0)
                        {
                            DBGERR(testName, "InactiveOnNextTransaction - Failed to set to false.");
                        }
                        else
                        {
                            if (qh.InactiveOnNextTransaction)
                            {
                                DBGERR(testName, "InactiveOnNextTransaction - Failed to read as false.");
                            }
                        }
                    }
                }

                qh.InterruptScheduleMask = 0xFE;
                //Shift!
                if (pQH[0x08u] != 0xFEu)
                {
                    DBGERR(testName, "InterruptScheduleMask - Failed to set.");
                }
                else
                {
                    if ((uint)qh.InterruptScheduleMask != 0xFEu)
                    {
                        DBGERR(testName, "InterruptScheduleMask - Failed to read.");
                    }
                }

                qh.MaximumPacketLength = 0xDEAD;
                //Shift!
                if ( pQH[0x06u]          != 0xADu ||
                    (pQH[0x07u] & 0x07u) != 0x06u)
                {
                    DBGERR(testName, "MaximumPacketLength - Failed to set.");
                }
                else
                {
                    if ((uint)qh.MaximumPacketLength != 0x06ADu)
                    {
                        DBGERR(testName, "MaximumPacketLength - Failed to read.");
                    }
                }

                qh.NakCountReload = 0xFF;
                //Shift!
                if ((pQH[0x07u] & 0xF0u) != 0xF0u)
                {
                    DBGERR(testName, "NakCountReload - Failed to set.");
                }
                else
                {
                    if ((uint)qh.NakCountReload != 0x0Fu)
                    {
                        DBGERR(testName, "NakCountReload - Failed to read.");
                    }
                }

                qh.NextqTDPointer = (EHCI_qTD_Struct*)0xDEADBEFF;
                //- Read back should equal 0xDEADBEE0
                if ((pQH[0x10u] & 0xF0u) != 0xF0u ||
                     pQH[0x11u] != 0xBEu ||
                     pQH[0x12u] != 0xADu ||
                     pQH[0x13u] != 0xDEu)
                {
                    DBGERR(testName, "NextqTDPointer - Failed to set.");
                }
                else
                {
                    if ((uint)qh.NextqTDPointer != 0xDEADBEF0u)
                    {
                        DBGERR(testName, "NextqTDPointer - Failed to read.");
                    }
                }

                qh.NextqTDPointerTerminate = true;
                if ((pQH[0x10u] & 0x01u) == 0)
                {
                    DBGERR(testName, "NextqTDPointerTerminate - Failed to set to true.");
                }
                else
                {
                    if (!qh.NextqTDPointerTerminate)
                    {
                        DBGERR(testName, "NextqTDPointerTerminate - Failed to read as true.");
                    }
                    else
                    {
                        pQH[0x10u] = 0xFF;
                        qh.NextqTDPointerTerminate = false;
                        if ((pQH[0x10u] & 0x01u) != 0)
                        {
                            DBGERR(testName, "NextqTDPointerTerminate - Failed to set to false.");
                        }
                        else
                        {
                            if (qh.NextqTDPointerTerminate)
                            {
                                DBGERR(testName, "NextqTDPointerTerminate - Failed to read as false.");
                            }
                        }
                    }
                }

                qh.PortNumber = 0xFF;
                //Shift!
                if ((pQH[0x0Au] & 0x80u) != 0x80u ||
                    (pQH[0x0Bu] & 0x3Fu) != 0x3Fu)
                {
                    DBGERR(testName, "PortNumber - Failed to set.");
                }
                else
                {
                    if ((uint)qh.PortNumber != 0x7Fu)
                    {
                        DBGERR(testName, "PortNumber - Failed to read.");
                    }
                }

                qh.SplitCompletionMask = 0xBE;
                //Shift!
                if (pQH[0x09u] != 0xBEu)
                {
                    DBGERR(testName, "SplitCompletionMask - Failed to set.");
                }
                else
                {
                    if ((uint)qh.SplitCompletionMask != 0xBEu)
                    {
                        DBGERR(testName, "SplitCompletionMask - Failed to read.");
                    }
                }

                qh.Terminate = true;
                if ((pQH[0x00u] & 0x01u) == 0)
                {
                    DBGERR(testName, "Terminate - Failed to set to true.");
                }
                else
                {
                    if (!qh.Terminate)
                    {
                        DBGERR(testName, "Terminate - Failed to read as true.");
                    }
                    else
                    {
                        pQH[0x00u] = 0xFF;
                        qh.Terminate = false;
                        if ((pQH[0x00u] & 0x01u) != 0)
                        {
                            DBGERR(testName, "Terminate - Failed to set to false.");
                        }
                        else
                        {
                            if (qh.Terminate)
                            {
                                DBGERR(testName, "Terminate - Failed to read as false.");
                            }
                        }
                    }
                }

                qh.Type = 0xFF;
                //Shift!
                if ((pQH[0x00u] & 0x06u) != 0x06u)
                {
                    DBGERR(testName, "Type - Failed to set.");
                }
                else
                {
                    if ((uint)qh.Type != 0x03u)
                    {
                        DBGERR(testName, "Type - Failed to read.");
                    }
                }

            }
            catch
            {
                errors++;
                BasicConsole.SetTextColour(BasicConsole.warning_colour);
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                BasicConsole.SetTextColour(BasicConsole.default_colour);
            }
            finally
            {
                qh.Free();
            }

            if (errors > 0)
            {
                DBGERR(testName, ((Framework.String)"Test failed! Errors: ") + errors + " Warnings: " + warnings);
            }
            else
            {
                if (warnings > 0)
                {
                    DBGWRN(testName, ((Framework.String)"Test passed with warnings: ") + warnings);
                }
                else
                {
                    DBGMSG(testName, "Test passed.");
                }
            }

            DBGMSG(testName, "END");

            BasicConsole.DelayOutput(1);
        }
        public static void Test_qTDWrapper()
        {
            Framework.String testName = "Queue Transfer Descrip";
            DBGMSG(testName, "START");

            errors = 0;
            warnings = 0;

            EHCI_qTD qTD = new EHCI_qTD();
            try
            {
                byte* pqTD = (byte*)qTD.qtd;

                //Verifications done via two methods:
                //  1. Check value from pointer & manual shifting to confirm set properly
                //  2. Check value from "get" method to confirm reading properly
                //  3. For boolean types, also test & verify setting to false!

                qTD.AlternateNextqTDPointerTerminate = true;
                if ((pqTD[0x04u] & 0x01u) == 0)
                {
                    DBGERR(testName, "AlternateNextqTDPointerTerminate - Failed to set to true.");
                }
                else
                {
                    if (!qTD.AlternateNextqTDPointerTerminate)
                    {
                        DBGERR(testName, "AlternateNextqTDPointerTerminate - Failed to read as true.");
                    }
                    else
                    {
                        pqTD[0x04u] = 0xFF;
                        qTD.AlternateNextqTDPointerTerminate = false;
                        if ((pqTD[0x04u] & 0x1u) != 0)
                        {
                            DBGERR(testName, "AlternateNextqTDPointerTerminate - Failed to set to false.");
                        }
                        else
                        {
                            if (qTD.AlternateNextqTDPointerTerminate)
                            {
                                DBGERR(testName, "AlternateNextqTDPointerTerminate - Failed to read as false.");
                            }
                        }
                    }
                }

                qTD.Buffer0 = (byte*)0xDEADBEEFu;
                //Read back should be 0xDEADB000
                if ((pqTD[0x0Du] & 0xF0u) != 0xB0u ||
                     pqTD[0x0Eu] != 0xADu ||
                     pqTD[0x0Fu] != 0xDEu)
                {
                    DBGERR(testName, "Buffer0 - Failed to set.");
                }
                else
                {
                    if ((uint)qTD.Buffer0 != 0xDEADB000u)
                    {
                        DBGERR(testName, "Buffer0 - Failed to read.");
                    }
                }

                qTD.Buffer1 = (byte*)0x12345678u;
                //Read back should be 0x12345000
                if ((pqTD[0x11u] & 0xF0u) != 0x50u ||
                     pqTD[0x12u] != 0x34u ||
                     pqTD[0x13u] != 0x12u)
                {
                    DBGERR(testName, "Buffer1 - Failed to set.");
                }
                else
                {
                    if ((uint)qTD.Buffer1 != 0x12345000u)
                    {
                        DBGERR(testName, "Buffer1 - Failed to read.");
                    }
                }

                qTD.Buffer2 = (byte*)0xFEEDBEA5u;
                //Read back should be 0xFEEDB000
                if ((pqTD[0x15u] & 0xF0u) != 0xB0u ||
                     pqTD[0x16u] != 0xEDu ||
                     pqTD[0x17u] != 0xFEu)
                {
                    DBGERR(testName, "Buffer2 - Failed to set.");
                }
                else
                {
                    if ((uint)qTD.Buffer2 != 0xFEEDB000u)
                    {
                        DBGERR(testName, "Buffer2 - Failed to read.");
                    }
                }

                qTD.Buffer3 = (byte*)0x09876543u;
                //Read back should be 0x09876000
                if ((pqTD[0x19u] & 0xF0u) != 0x60u ||
                     pqTD[0x1Au] != 0x87u ||
                     pqTD[0x1Bu] != 0x09u)
                {
                    DBGERR(testName, "Buffer3 - Failed to set.");
                }
                else
                {
                    if ((uint)qTD.Buffer3 != 0x09876000u)
                    {
                        DBGERR(testName, "Buffer3 - Failed to read.");
                    }
                }

                qTD.Buffer4 = (byte*)0x24681357u;
                //Read back should be 0x24681000
                if ((pqTD[0x1Du] & 0xF0u) != 0x10u ||
                     pqTD[0x1Eu] != 0x68u ||
                     pqTD[0x1Fu] != 0x24u)
                {
                    DBGERR(testName, "Buffer4 - Failed to set.");
                }
                else
                {
                    if ((uint)qTD.Buffer4 != 0x24681000u)
                    {
                        DBGERR(testName, "Buffer4 - Failed to read.");
                    }
                }

                qTD.CurrentPage = 0xFF;
                //Shift!
                if ((pqTD[0x09u] & 0x70u) != 0x70u)
                {
                    DBGERR(testName, "CurrentPage - Failed to set.");
                }
                else
                {
                    if ((uint)qTD.CurrentPage != 0x07u)
                    {
                        DBGERR(testName, "CurrentPage - Failed to read.");
                    }
                }

                qTD.DataToggle = true;
                if ((pqTD[0x0Bu] & 0x80u) == 0)
                {
                    DBGERR(testName, "DataToggle - Failed to set to true.");
                }
                else
                {
                    if (!qTD.DataToggle)
                    {
                        DBGERR(testName, "DataToggle - Failed to read as true.");
                    }
                    else
                    {
                        pqTD[0x0Bu] = 0xFF;
                        qTD.DataToggle = false;
                        if ((pqTD[0x0Bu] & 0x80u) != 0)
                        {
                            DBGERR(testName, "DataToggle - Failed to set to false.");
                        }
                        else
                        {
                            if (qTD.DataToggle)
                            {
                                DBGERR(testName, "DataToggle - Failed to read as false.");
                            }
                        }
                    }
                }

                qTD.ErrorCounter = 0xFF;
                //Shift!
                if ((pqTD[0x09u] & 0x0Cu) != 0x0Cu)
                {
                    DBGERR(testName, "ErrorCounter - Failed to set.");
                }
                else
                {
                    if ((uint)qTD.ErrorCounter != 0x03u)
                    {
                        DBGERR(testName, "ErrorCounter - Failed to read.");
                    }
                }

                qTD.InterruptOnComplete = true;
                if ((pqTD[0x09u] & 0x80u) == 0)
                {
                    DBGERR(testName, "InterruptOnComplete - Failed to set to true.");
                }
                else
                {
                    if (!qTD.InterruptOnComplete)
                    {
                        DBGERR(testName, "InterruptOnComplete - Failed to read as true.");
                    }
                    else
                    {
                        pqTD[0x09u] = 0xFF;
                        qTD.InterruptOnComplete = false;
                        if ((pqTD[0x09u] & 0x80u) != 0)
                        {
                            DBGERR(testName, "InterruptOnComplete - Failed to set to false.");
                        }
                        else
                        {
                            if (qTD.InterruptOnComplete)
                            {
                                DBGERR(testName, "InterruptOnComplete - Failed to read as false.");
                            }
                        }
                    }
                }

                qTD.NextqTDPointer = (EHCI_qTD_Struct*)0xF2610369;
                //Read back should be 0xF2610360
                if ((pqTD[0x00u] & 0xE0u) != 0x60u ||
                     pqTD[0x01u] != 0x03u ||
                     pqTD[0x02u] != 0x61u ||
                     pqTD[0x03u] != 0xF2u)
                {
                    DBGERR(testName, "NextqTDPointer - Failed to set.");
                }
                else
                {
                    if ((uint)qTD.NextqTDPointer != 0xF2610360u)
                    {
                        DBGERR(testName, "NextqTDPointer - Failed to read.");
                    }
                }

                qTD.NextqTDPointerTerminate = true;
                if ((pqTD[0x00u] & 0x01u) == 0)
                {
                    DBGERR(testName, "NextqTDPointerTerminate - Failed to set to true.");
                }
                else
                {
                    if (!qTD.NextqTDPointerTerminate)
                    {
                        DBGERR(testName, "NextqTDPointerTerminate - Failed to read as true.");
                    }
                    else
                    {
                        pqTD[0x00u] = 0xFF;
                        qTD.NextqTDPointerTerminate = false;
                        if ((pqTD[0x00u] & 0x01u) != 0)
                        {
                            DBGERR(testName, "NextqTDPointerTerminate - Failed to set to false.");
                        }
                        else
                        {
                            if (qTD.NextqTDPointerTerminate)
                            {
                                DBGERR(testName, "NextqTDPointerTerminate - Failed to read as false.");
                            }
                        }
                    }
                }

                qTD.PIDCode = 0xFF;
                //Shift!
                if ((pqTD[0x09u] & 0x03u) != 0x03u)
                {
                    DBGERR(testName, "PIDCode - Failed to set.");
                }
                else
                {
                    if ((uint)qTD.PIDCode != 0x03u)
                    {
                        DBGERR(testName, "PIDCode - Failed to read.");
                    }
                }

                qTD.Status = 0xFF;
                //Shift!
                if (pqTD[0x08u] != 0xFFu)
                {
                    DBGERR(testName, "Status - Failed to set.");
                }
                else
                {
                    if ((uint)qTD.Status != 0xFFu)
                    {
                        DBGERR(testName, "Status - Failed to read.");
                    }
                }

                qTD.TotalBytesToTransfer = 0xFFFF;
                //Read back should be 0x7FFF
                if (pqTD[0x0Au] != 0xFFu ||
                    (pqTD[0x0Bu] & 0x7Fu) != 0x7Fu)
                {
                    DBGERR(testName, "TotalBytesToTransfer - Failed to set.");
                }
                else
                {
                    if ((uint)qTD.TotalBytesToTransfer != 0x7FFFu)
                    {
                        DBGERR(testName, "TotalBytesToTransfer - Failed to read.");
                    }
                }
            }
            catch
            {
                errors++;
                BasicConsole.SetTextColour(BasicConsole.warning_colour);
                BasicConsole.WriteLine(ExceptionMethods.CurrentException.Message);
                BasicConsole.SetTextColour(BasicConsole.default_colour);
            }
            finally
            {
                qTD.Free();
            }

            if (errors > 0)
            {
                DBGERR(testName, ((Framework.String)"Test failed! Errors: ") + errors + " Warnings: " + warnings);
            }
            else
            {
                if (warnings > 0)
                {
                    DBGWRN(testName, ((Framework.String)"Test passed with warnings: ") + warnings);
                }
                else
                {
                    DBGMSG(testName, "Test passed.");
                }
            }

            DBGMSG(testName, "END");

            BasicConsole.DelayOutput(1);
        }

        #endregion
        

    #region Validation

        public static bool Validate_PointerBoundaryAlignment(void* ptr, uint boundary)
        {
            return (((uint)ptr) % boundary) == 0;
        }

        #endregion

        public static void DBGMSG(Framework.String testName, Framework.String msg)
        {
            BasicConsole.WriteLine(testName.PadRight(25, ' ') + " : " + msg);
        }
        public static void DBGWRN(Framework.String testName, Framework.String msg)
        {
            BasicConsole.SetTextColour(BasicConsole.warning_colour);
            DBGMSG(testName, msg);
            BasicConsole.SetTextColour(BasicConsole.default_colour);
        }
        public static void DBGERR(Framework.String testName, Framework.String msg)
        {
            BasicConsole.SetTextColour(BasicConsole.error_colour);
            DBGMSG(testName, msg);
            BasicConsole.SetTextColour(BasicConsole.default_colour);
            errors++;
        }
    }
    internal unsafe class EHCITestingObject : Framework.Object
    {
        public byte* ptr1;
        public uint* ptr2;
        public ulong* ptr3;
        public byte* ptr4;
    }
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]
    internal unsafe struct EHCITestingStruct
    {
        public uint u1;
        public uint u2;
        public uint u3;
        public uint u4;
        public uint u5;
        public uint u6;
        public uint u7;
        public uint u8;
    }
#endif
}
#region Copyright Notice
/// ------------------------------------------------------------------------------ ///
///                                                                                ///
///               All contents copyright � Edward Nutting 2014                     ///
///                                                                                ///
///        You may not share, reuse, redistribute or otherwise use the             ///
///        contents this file outside of the Fling OS project without              ///
///        the express permission of Edward Nutting or other copyright             ///
///        holder. Any changes (including but not limited to additions,            ///
///        edits or subtractions) made to or from this document are not            ///
///        your copyright. They are the copyright of the main copyright            ///
///        holder for all Fling OS files. At the time of writing, this             ///
///        owner was Edward Nutting. To be clear, owner(s) do not include          ///
///        developers, contributors or other project members.                      ///
///                                                                                ///
/// ------------------------------------------------------------------------------ ///
#endregion
    
using System;

namespace Kernel.Hardware.USB.HCIs
{
    public static unsafe class EHCITesting
    {
        public static int errors = 0;
        public static int warnings = 0;

        #region Memory Tests

        public static void Test_PointerManipultation()
        {
            FOS_System.String testName = "Ptr Manipulation";
            DBGMSG(testName, "START");

            errors = 0;
            warnings = 0;

            byte* rootPtr = (byte*)FOS_System.Heap.Alloc(4096, 32);
            try
            {
                DBGMSG(testName, ((FOS_System.String)"rootPtr: ") + (uint)rootPtr);

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
                    DBGERR(testName, ((FOS_System.String)"Shifted pointer not shifted correctly! shiftedPtr: ") + (uint)shiftedPtr);
                }

                EHCITestingObject testObj = new EHCITestingObject();
                testObj.ptr1 = rootPtr;
                testObj.ptr2 = (uint*)rootPtr;
                testObj.ptr3 = (ulong*)rootPtr;
                testObj.ptr4 = rootPtr;
                if (testObj.ptr1 != rootPtr)
                {
                    DBGERR(testName, ((FOS_System.String)"Storing test pointer 1 failed! testObj.ptr1: ") + (uint)testObj.ptr1);
                }
                if (testObj.ptr2 != rootPtr)
                {
                    DBGERR(testName, ((FOS_System.String)"Storing test pointer 2 failed! testObj.ptr2: ") + (uint)testObj.ptr2);
                }
                if (testObj.ptr3 != rootPtr)
                {
                    DBGERR(testName, ((FOS_System.String)"Storing test pointer 3 failed! testObj.ptr3: ") + (uint)testObj.ptr3);
                }
                if (testObj.ptr4 != rootPtr)
                {
                    DBGERR(testName, ((FOS_System.String)"Storing test pointer 4 failed! testObj.ptr4: ") + (uint)testObj.ptr4);
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
                FOS_System.Heap.Free(rootPtr);
            }

            if (errors > 0)
            {
                DBGERR(testName, ((FOS_System.String)"Test failed! Errors: ") + errors + " Warnings: " + warnings);
            }
            else
            {
                if (warnings > 0)
                {
                    DBGWRN(testName, ((FOS_System.String)"Test passed with warnings: ") + warnings);
                }
                else
                {
                    DBGMSG(testName, "Test passed.");
                }
            }

            DBGMSG(testName, "END");

            BasicConsole.DelayOutput(4);
        }
        private static byte* ReturnPointer(byte* aPtr)
        {
            return aPtr;
        }
        
        public static void Test_StructValueSetting()
        {
            FOS_System.String testName = "Strct Value Setting";
            DBGMSG(testName, "START");

            errors = 0;
            warnings = 0;

            uint structSize = (uint)sizeof(EHCITestingStruct);
            if (structSize != 8 * 4)
            {
                DBGERR(testName, ((FOS_System.String)"Struct size incorrect! structSize: ") + structSize);
            }
            if (errors == 0)
            {
                EHCITestingStruct* rootPtr = (EHCITestingStruct*)FOS_System.Heap.Alloc(structSize);
                byte* bRootPtr = (byte*)rootPtr;
                try
                {
                    DBGMSG(testName, ((FOS_System.String)"rootPtr: ") + (uint)rootPtr);

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
                    FOS_System.Heap.Free(rootPtr);
                }
            }

            if (errors > 0)
            {
                DBGERR(testName, ((FOS_System.String)"Test failed! Errors: ") + errors + " Warnings: " + warnings);
            }
            else
            {
                if (warnings > 0)
                {
                    DBGWRN(testName, ((FOS_System.String)"Test passed with warnings: ") + warnings);
                }
                else
                {
                    DBGMSG(testName, "Test passed.");
                }
            }

            DBGMSG(testName, "END");

            BasicConsole.DelayOutput(4);
        }
        private static void Test_StructValueSettingAsArg(FOS_System.String testName, EHCITestingStruct root)
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
        private static void Test_StructValueSettingAsArg(FOS_System.String testName, EHCITestingStruct* rootPtr, byte* bRootPtr)
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
            FOS_System.String testName = "Queue Head Wrapper";
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

                qh.CurrentqTDPointer = (EHCI_qTD_Struct*)0xDEADBEEFu;
                //- Read back should equal 0xDEADBEE0
                if (pQH[0x0Cu] != 0xE0u ||
                    pQH[0x0Du] != 0xBEu ||
                    pQH[0x0Eu] != 0xADu ||
                    pQH[0x0Fu] != 0xDEu)
                {
                    DBGERR(testName, "CurrentqTDPointer - Failed to set.");
                }
                else
                {
                    if ((uint)qh.CurrentqTDPointer != 0xDEADBEE0u)
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
                if (pQH[0x04u] != 0x5Eu)
                {
                    DBGERR(testName, "DeviceAddress - Failed to set.");
                }
                else
                {
                    if ((uint)qh.DeviceAddress != 0x5E)
                    {
                        DBGERR(testName, "DeviceAddress - Failed to read.");
                    }
                }

                qh.EndpointNumber = 0xBE;
                //Shift!
                //TODO: Verify

                qh.EndpointSpeed = 0xFE;
                //Shift!
                //TODO: Verify

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

                qh.HighBandwidthPipeMultiplier = 0xDE;
                //TODO: Verify
                qh.HorizontalLinkPointer = (EHCI_QueueHead_Struct*)0xDEADBEEF;
                //TODO: Verify
                qh.HubAddr = 0xBE;
                //TODO: Verify
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
                //TODO: Verify
                qh.MaximumPacketLength = 0xDEAD;
                //TODO: Verify
                qh.NakCountReload = 0xBE;
                //TODO: Verify
                qh.NextqTDPointer = (EHCI_qTD_Struct*)0xDEADBEEF;
                //TODO: Verify
                qh.NextqTDPointerTerminate = true;
                //TODO: Verify
                qh.PortNumber = 0xDE;
                //TODO: Verify
                qh.SplitCompletionMask = 0xBE;
                //TODO: Verify
                qh.Terminate = true;
                //TODO: Verify
                qh.Type = 0xDE;
                //TODO: Verify
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
                DBGERR(testName, ((FOS_System.String)"Test failed! Errors: ") + errors + " Warnings: " + warnings);
            }
            else
            {
                if (warnings > 0)
                {
                    DBGWRN(testName, ((FOS_System.String)"Test passed with warnings: ") + warnings);
                }
                else
                {
                    DBGMSG(testName, "Test passed.");
                }
            }

            DBGMSG(testName, "END");

            BasicConsole.DelayOutput(4);
        }

        #endregion


        #region Register Tests

        #endregion


        #region EHCI Method Tests

        #endregion


        #region Validation

        public static bool Validate_PointerBoundaryAlignment(void* ptr, uint boundary)
        {
            return (((uint)ptr) % boundary) == 0;
        }

        #endregion

        private static void DBGMSG(FOS_System.String testName, FOS_System.String msg)
        {
            BasicConsole.WriteLine(testName.PadRight(25, ' ') + " : " + msg);
        }
        private static void DBGWRN(FOS_System.String testName, FOS_System.String msg)
        {
            BasicConsole.SetTextColour(BasicConsole.warning_colour);
            DBGMSG(testName, msg);
            BasicConsole.SetTextColour(BasicConsole.default_colour);
        }
        private static void DBGERR(FOS_System.String testName, FOS_System.String msg)
        {
            BasicConsole.SetTextColour(BasicConsole.error_colour);
            DBGMSG(testName, msg);
            BasicConsole.SetTextColour(BasicConsole.default_colour);
            errors++;
        }
    }
    internal unsafe class EHCITestingObject : FOS_System.Object
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
}

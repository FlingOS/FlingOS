using System;

namespace Kernel.FOS_System.IO.Disk
{
    public class GPT : FOS_System.Object
    {
        public static bool IsGPTFormatted(Hardware.Devices.DiskDevice disk)
        {
            //Check for single MBR partition with 0xEE system ID
            byte[] blockData = new byte[512];
            disk.ReadBlock(0UL, 1U, blockData);

            MBR TheMBR = new MBR(blockData);
            if(!TheMBR.IsValid)
            {
                return false;
            }
            else if(TheMBR.NumPartitions == 0)
            {
                return false;
            }
            else if(TheMBR.Partitions[0].SystemID != 0xEE)
            {
                return false;
            }

            //Now we know this is very-likely to be GPT formatted. 
            //  But we must check the GPT header for signature etc.

            disk.ReadBlock(1UL, 1U, blockData);

            //Check for GPT signature: 0x45 0x46 0x49 0x20 0x50 0x41 0x52 0x54
            bool OK =  blockData[0] == 0x45;
            OK = OK && blockData[1] == 0x46;
            OK = OK && blockData[2] == 0x49;
            OK = OK && blockData[3] == 0x20;
            OK = OK && blockData[4] == 0x50;
            OK = OK && blockData[5] == 0x41;
            OK = OK && blockData[6] == 0x52;
            OK = OK && blockData[7] == 0x54;
            
            return OK;
        }
    }
}

using Kernel.Framework;

namespace Kernel.ATA.Exceptions
{
    /// <summary>
    ///     Represents an exception from a disk driver indicating that no disk was found in the device.
    ///     This can occur, for example, with CD drives where the drive is detected but is found to be
    ///     empty. This exception should only be raised if a disk operation is attempted on an empty
    ///     drive.
    /// </summary>
    public class NoDiskException : Exception
    {
        /// <summary>
        ///     Creates a new No Disk Exception with the specified Message appended to "No disk in drive."
        /// </summary>
        /// <param name="Message">The message to append.</param>
        public NoDiskException(String Message)
            : base("No disk in drive. " + Message)
        {
        }
    }
}
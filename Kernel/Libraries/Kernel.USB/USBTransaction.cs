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

using Kernel.Framework;
using Kernel.USB.HCIs;

namespace Kernel.USB
{
    /// <summary>
    ///     The types of USB transaction.
    /// </summary>
    public enum USBTransactionType
    {
        /// <summary>
        ///     Indicates the transaction is a SETUP transaction.
        /// </summary>
        SETUP,

        /// <summary>
        ///     Indicates the transaction is an IN transaction.
        /// </summary>
        IN,

        /// <summary>
        ///     Indicates the transaction is an OUT transaction.
        /// </summary>
        OUT
    }

    /// <summary>
    ///     Represents a transaction from the high-level USB perspective.
    /// </summary>
    public class USBTransaction : Object
    {
        /// <summary>
        ///     The type of the transaction.
        /// </summary>
        public USBTransactionType type;

        /// <summary>
        ///     The implementation-specific transaction that can actually be sent by a specific host controller type.
        /// </summary>
        public HCTransaction underlyingTz;
    }
}
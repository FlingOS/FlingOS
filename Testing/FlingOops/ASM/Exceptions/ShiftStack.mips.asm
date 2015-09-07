/* ---------------------------------- LICENSE ---------------------------------- //
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
// ------------------------------------------------------------------------------ */

.global method_System_Void_RETEND_FlingOops_ExceptionMethods_DECLEND_ShiftStack_NAMEEND__System_Byte__System_UInt32_

.text

.ent method_System_Void_RETEND_FlingOops_ExceptionMethods_DECLEND_ShiftStack_NAMEEND__System_Byte__System_UInt32_
method_System_Void_RETEND_FlingOops_ExceptionMethods_DECLEND_ShiftStack_NAMEEND__System_Byte__System_UInt32_:

/* Load distance */
lw $t0, 0($sp)

/* Load current (i.e. start) pointer */
lw $t1, 4($sp)

method_System_Void_RETEND_FlingOops_ExceptionMethods_DECLEND_ShiftStack_NAMEEND__System_Byte__System_UInt32_.Loop:

/* Load value to copy */
lw $t2, 0($t1)

/* Load / calc pointer to copy to */
move $t3, $t1
add $t3, $t3, $t0
sw $t2, 0($t3)

/* Shift to next dword */
addi $t1, $t1, -4

/* Is current pointer == end pointer
 *		i.e. Is ebx == esp
 */
bne $t1, $sp, method_System_Void_RETEND_FlingOops_ExceptionMethods_DECLEND_ShiftStack_NAMEEND__System_Byte__System_UInt32_.Loop

j $ra
.end method_System_Void_RETEND_FlingOops_ExceptionMethods_DECLEND_ShiftStack_NAMEEND__System_Byte__System_UInt32_

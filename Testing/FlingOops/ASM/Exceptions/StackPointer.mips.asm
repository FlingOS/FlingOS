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

.global method_System_Byte__RETEND_FlingOops_ExceptionMethods_DECLEND_get_StackPointer_NAMEEND___
.global method_System_Void_RETEND_FlingOops_ExceptionMethods_DECLEND_set_StackPointer_NAMEEND__System_Byte__

.text

.ent method_System_Byte__RETEND_FlingOops_ExceptionMethods_DECLEND_get_StackPointer_NAMEEND___
method_System_Byte__RETEND_FlingOops_ExceptionMethods_DECLEND_get_StackPointer_NAMEEND___:
addi $sp, $sp, -4
sw $fp, 0($sp)
move $fp, $sp

move $t0, $sp
addi $t0, $t0, 8
sw $t0, 4($fp)

lw $fp, 0($sp)
addi $sp, $sp, 4
j $ra
.end method_System_Byte__RETEND_FlingOops_ExceptionMethods_DECLEND_get_StackPointer_NAMEEND___

.ent method_System_Void_RETEND_FlingOops_ExceptionMethods_DECLEND_set_StackPointer_NAMEEND__System_Byte__
method_System_Void_RETEND_FlingOops_ExceptionMethods_DECLEND_set_StackPointer_NAMEEND__System_Byte__:
lw $sp, 0($sp)
/* Handles the "addi $sp, $sp, 4" after return */
addi $sp, $sp, -4
j $ra
.end method_System_Void_RETEND_FlingOops_ExceptionMethods_DECLEND_set_StackPointer_NAMEEND__System_Byte__

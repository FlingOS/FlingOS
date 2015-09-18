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

.global method_System_Byte__RETEND_FlingOops_ExceptionMethods_DECLEND_get_BasePointer_NAMEEND___
.global method_System_Void_RETEND_FlingOops_ExceptionMethods_DECLEND_set_BasePointer_NAMEEND__System_Byte__

.text

.ent method_System_Byte__RETEND_FlingOops_ExceptionMethods_DECLEND_get_BasePointer_NAMEEND___
method_System_Byte__RETEND_FlingOops_ExceptionMethods_DECLEND_get_BasePointer_NAMEEND___:
sw $fp, 0($sp)
j $ra
.end method_System_Byte__RETEND_FlingOops_ExceptionMethods_DECLEND_get_BasePointer_NAMEEND___

.ent method_System_Void_RETEND_FlingOops_ExceptionMethods_DECLEND_set_BasePointer_NAMEEND__System_Byte__
method_System_Void_RETEND_FlingOops_ExceptionMethods_DECLEND_set_BasePointer_NAMEEND__System_Byte__:
lw $fp, 0($sp)
j $ra
.end method_System_Void_RETEND_FlingOops_ExceptionMethods_DECLEND_set_BasePointer_NAMEEND__System_Byte__

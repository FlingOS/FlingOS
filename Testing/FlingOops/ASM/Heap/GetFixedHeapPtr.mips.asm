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

.global method_System_UInt32__RETEND_FlingOops_Heap_DECLEND_GetFixedHeapPtr_NAMEEND___
.global method_System_UInt32_RETEND_FlingOops_Heap_DECLEND_GetFixedHeapSize_NAMEEND___

.global KernelFixedHeap_Start
.global KernelFixedHeap_End

.text

.ent method_System_UInt32__RETEND_FlingOops_Heap_DECLEND_GetFixedHeapPtr_NAMEEND___
method_System_UInt32__RETEND_FlingOops_Heap_DECLEND_GetFixedHeapPtr_NAMEEND___:
la $t0, KernelFixedHeap_Start
sw $t0, 0($sp)
j $ra
.end method_System_UInt32__RETEND_FlingOops_Heap_DECLEND_GetFixedHeapPtr_NAMEEND___


.ent method_System_UInt32_RETEND_FlingOops_Heap_DECLEND_GetFixedHeapSize_NAMEEND___
method_System_UInt32_RETEND_FlingOops_Heap_DECLEND_GetFixedHeapSize_NAMEEND___:
la $t0, KernelFixedHeap_Start
la $t1, KernelFixedHeap_End
subu $t1, $t1, $t0
sw $t1, 0($sp)
j $ra
.end method_System_UInt32_RETEND_FlingOops_Heap_DECLEND_GetFixedHeapSize_NAMEEND___

.bss
/* 104 857 600 bytes = 100MiB (using proper powers of 2 not the powers of 10 crap...) */
KernelFixedHeap_Start: 
.space 104857600
KernelFixedHeap_End:

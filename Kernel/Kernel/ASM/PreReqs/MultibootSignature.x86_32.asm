; - - - - - - - - - - - - - - - - - - - LICENSE - - - - - - - - - - - - - - - -  ;
;
;    Fling OS - The educational operating system
;    Copyright (C) 2015 Edward Nutting
;
;    This program is free software: you can redistribute it and/or modify
;    it under the terms of the GNU General Public License as published by
;    the Free Software Foundation, either version 2 of the License, or
;    (at your option) any later version.
;
;    This program is distributed in the hope that it will be useful,
;    but WITHOUT ANY WARRANTY; without even the implied warranty of
;    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
;    GNU General Public License for more details.
;
;    You should have received a copy of the GNU General Public License
;    along with this program.  If not, see <http:;www.gnu.org/licenses/>.
;
;  Project owner: 
;		Email: edwardnutting@outlook.com
;		For paper mail address, please contact via email for details.
;
; - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -  ;
BITS 32

SECTION .text
File_MultibootSignature:

GLOBAL MultibootSignature:data
GLOBAL MultibootFlags:data
GLOBAL MultibootChecksum:data
GLOBAL MultibootGraphicsRuntime_VbeModeInfoAddr:data
GLOBAL MultibootGraphicsRuntime_VbeControlInfoAddr:data
GLOBAL MultibootGraphicsRuntime_VbeMode:data
GLOBAL MultiBootInfo_Memory_High:data
GLOBAL MultiBootInfo_Memory_Low:data

GLOBAL Kernel_MemStart:data
GLOBAL Before_Kernel_Stack:data
GLOBAL Kernel_Stack
GLOBAL MultiBootInfo_Structure:data

GLOBAL _NATIVE_GDT_Contents:data
GLOBAL _NATIVE_GDT_Pointer:data
GLOBAL _NATIVE_IDT_Contents:data
GLOBAL _NATIVE_IDT_Pointer:data
GLOBAL _NATIVE_TSS:data
GLOBAL TSS_POINTER:data

; BEGIN - Multiboot Signature
MultibootSignature dd 464367618
MultibootFlags dd 3
MultibootChecksum dd -464367621
MultibootGraphicsRuntime_VbeModeInfoAddr dd 2147483647
MultibootGraphicsRuntime_VbeControlInfoAddr dd 2147483647
MultibootGraphicsRuntime_VbeMode dd 2147483647
MultiBootInfo_Memory_High dd 0
MultiBootInfo_Memory_Low dd 0


KERNEL_VIRTUAL_BASE equ 0xC0000000					; 3GiB
KERNEL_PAGE_NUMBER equ (KERNEL_VIRTUAL_BASE >> 22)

Kernel_MemStart:

Before_Kernel_Stack: TIMES 65535 db 0
Kernel_Stack:


MultiBootInfo_Structure dd 0

; This is the GDT table pre-filled with the entries we want
_NATIVE_GDT_Contents:
; I have a suspicion that the order of the items in the GDT matters
;	Code and data selectors first then TSS
db 0, 0, 0, 0, 0, 0, 0, 0			; Offset: 0  - Null selector - required 
db 255, 255, 0, 0, 0, 0x9A, 0xCF, 0	; Offset: 8  - KM Code selector - covers the entire 4GiB address range
db 255, 255, 0, 0, 0, 0x92, 0xCF, 0	; Offset: 16 - KM Data selector - covers the entire 4GiB address range
db 255, 255, 0, 0, 0, 0xFA, 0xCF, 0	; Offset: 24 - UM Code selector - covers the entire 4GiB address range
db 255, 255, 0, 0, 0, 0xF2, 0xCF, 0	; Offset: 32 - UM Data selector - covers the entire 4GiB address range
db 0x67,  0, 0, 0, 0, 0xE9, 0x00, 0	; Offset: 40 - TSS Selector - Pointer to the TSS 

;					   Size - Change iff adding/removing rows from GDT contents
;					   Size = Total bytes in GDT - 1
_NATIVE_GDT_Pointer db 47, 0, 0, 0, 0, 0
_NATIVE_IDT_Contents: TIMES 2048 db 0
_NATIVE_IDT_Pointer db 0xFF, 0x7, 0, 0, 0, 0

_NATIVE_TSS:
TIMES 104 db 0
TSS_POINTER equ (_NATIVE_TSS - KERNEL_VIRTUAL_BASE)

; END - Multiboot Signature
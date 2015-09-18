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
GLOBAL File_Kernel_Start:function
File_Kernel_Start:

GLOBAL Kernel_Start:function

EXTERN Kernel_Start_HandleNoMultiboot
EXTERN MultiBootInfo_Structure
EXTERN MultiBootInfo_Memory_Low
EXTERN MultiBootInfo_Memory_High

KERNEL_VIRTUAL_BASE equ 0xC0000000					; 3GiB
KERNEL_PAGE_NUMBER equ (KERNEL_VIRTUAL_BASE >> 22)

; BEGIN - Kernel Start

; Kernel_Start equ (_Kernel_Start - KERNEL_VIRTUAL_BASE)

Kernel_Start:

	cli
	
	; MultiBoot compliant loader provides info in registers: 
	; EBX=multiboot_info 
	; EAX=0x2BADB002	- check if it's really Multiboot loader 
	;					- if true, continue and copy mb info
	; BEGIN - Multiboot Info
	mov dword ecx, 0x2BADB002
	cmp ecx, eax
	jne (Kernel_Start_HandleNoMultiboot - KERNEL_VIRTUAL_BASE)
	
	mov dword [MultiBootInfo_Structure - KERNEL_VIRTUAL_BASE], EBX
	add dword EBX, 0x4
	mov dword EAX, [EBX]
	mov dword [MultiBootInfo_Memory_Low - KERNEL_VIRTUAL_BASE], EAX
	add dword EBX, 0x4
	mov dword EAX, [EBX]
	mov dword [MultiBootInfo_Memory_High - KERNEL_VIRTUAL_BASE], EAX
	
	
	mov dword eax, 0x2F
	mov dword ebx, 0xB8000
	mov dword ecx, 2000
	.ColourSetup:
	mov byte [ebx], 0
	mov byte [ebx+1], al
	add ebx, 2
	loop .ColourSetup

	; Enabling protected mode...
	; 0x45 0x6e 0x61 0x62 0x6c 0x69 0x6e 0x67 0x20 0x70 0x72 0x6f 0x74 0x65 0x63 0x74 0x65 0x64 0x20 0x6d 0x6f 0x64 0x65 0x2e 0x2e 0x2e
	mov byte [0xB8000], 0x45
	mov byte [0xB8002], 0x6e
	mov byte [0xB8004], 0x61 
	mov byte [0xB8006], 0x62 
	mov byte [0xB8008], 0x6c 
	mov byte [0xB800A], 0x69 
	mov byte [0xB800C], 0x6e 
	mov byte [0xB800E], 0x67 
	mov byte [0xB8010], 0x20 
	mov byte [0xB8012], 0x70 
	mov byte [0xB8014], 0x72 
	mov byte [0xB8016], 0x6f 
	mov byte [0xB8018], 0x74 
	mov byte [0xB801A], 0x65 
	mov byte [0xB801C], 0x63 
	mov byte [0xB801E], 0x74 
	mov byte [0xB8020], 0x65 
	mov byte [0xB8022], 0x64 
	mov byte [0xB8024], 0x20 
	mov byte [0xB8026], 0x6d 
	mov byte [0xB8028], 0x6f 
	mov byte [0xB802A], 0x64 
	mov byte [0xB802C], 0x65 
	mov byte [0xB802E], 0x2e 
	mov byte [0xB8030], 0x2e 
	mov byte [0xB8032], 0x2e 
	
	mov ecx, 0x00F00000
	.Loop1:
	loop .Loop1

	; Enable Protected Mode
	mov eax, cr0
	or eax, 0x1
	mov cr0, eax
	
	; done.
	; 0x64 0x6f 0x6e 0x65 0x2e 
	
	mov byte [0xB8034], 0x64 
	mov byte [0xB8036], 0x6f 
	mov byte [0xB8038], 0x6e 
	mov byte [0xB803A], 0x65 
	mov byte [0xB803C], 0x2e 

	mov ecx, 0x00F00000
	.Loop2:
	loop .Loop2

	; END - Multiboot Info
; END - Kernel Start
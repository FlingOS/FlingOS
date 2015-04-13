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

Kernel_Start equ (_Kernel_Start - KERNEL_VIRTUAL_BASE)

_Kernel_Start:

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
	

	; Enable Protected Mode
	mov eax, cr0
	or eax, 0x1
	mov cr0, eax
	
	; END - Multiboot Info
; END - Kernel Start
BITS 32

SECTION .text

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

	xchg bx, bx
	cli

	mov byte [0xB8000], 78
	mov byte [0xB8002], 111
	mov byte [0xB8004], 32
	mov byte [0xB8006], 109
	mov byte [0xB8008], 117
	mov byte [0xB800A], 108
	mov byte [0xB800C], 116
	mov byte [0xB800E], 105
	mov byte [0xB8010], 98
	mov byte [0xB8012], 111
	mov byte [0xB8014], 111
	mov byte [0xB8016], 116

	; Set the colour of the outputted text to:
	; Red background (0x4-), 
	; White foreground (0x-F)
	mov dword eax, 0x4F
	mov byte [0xB8001], al
	mov byte [0xB8003], al
	mov byte [0xB8005], al
	mov byte [0xB8007], al
	mov byte [0xB8009], al
	mov byte [0xB800B], al
	mov byte [0xB800D], al
	mov byte [0xB800F], al
	mov byte [0xB8011], al
	mov byte [0xB8013], al
	mov byte [0xB8015], al
	mov byte [0xB8017], al
	
	; MultiBoot compliant loader provides info in registers: 
	; EBX=multiboot_info 
	; EAX=0x2BADB002	- check if it's really Multiboot loader 
	;					- if true, continue and copy mb info
	; BEGIN - Multiboot Info
	mov dword ecx, 0x2BADB002
	cmp ecx, eax
	jne (Kernel_Start_HandleNoMultiboot - KERNEL_VIRTUAL_BASE)

	mov ecx, 0xFFFFFFFF
	.Loop:
	loop .Loop

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
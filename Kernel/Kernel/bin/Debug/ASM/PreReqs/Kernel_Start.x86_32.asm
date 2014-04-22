; BEGIN - Kernel Start
global Kernel_Start
Kernel_Start:

	xchg bx, bx
	cli

	; MultiBoot compliant loader provides info in registers: 
	; EBX=multiboot_info 
	; EAX=0x2BADB002	- check if it's really Multiboot loader 
	;					- if true, continue and copy mb info
	; BEGIN - Multiboot Info
	mov dword ecx, 0x2BADB002
	cmp ecx, eax
	jne Kernel_Start_HandleNoMultiboot

	mov dword [MultiBootInfo_Structure], EBX
	add dword EBX, 0x4
	mov dword EAX, [EBX]
	mov dword [MultiBootInfo_Memory_Low], EAX
	add dword EBX, 0x4
	mov dword EAX, [EBX]
	mov dword [MultiBootInfo_Memory_High], EAX
	; END - Multiboot Info
; END - Kernel Start
BITS 32

SECTION .text
GLOBAL File_GDT:function
File_GDT:

EXTERN _NATIVE_GDT_Contents
EXTERN _NATIVE_GDT_Pointer
EXTERN TSS_POINTER

; BEGIN - Create GDT
	; See MultibootSignature.x86_32.asm for memory allocations
	
	; See comment at start of Main Entrypoint
	mov dword ecx, 0x0

	; Initialising GDT...
	mov byte [0xB8280], 0x49
	mov byte [0xB8282], 0x6e
	mov byte [0xB8284], 0x69
	mov byte [0xB8286], 0x74
	mov byte [0xB8288], 0x69
	mov byte [0xB828A], 0x61
	mov byte [0xB828C], 0x6c
	mov byte [0xB828E], 0x69
	mov byte [0xB8290], 0x73
	mov byte [0xB8292], 0x69
	mov byte [0xB8294], 0x6e
	mov byte [0xB8296], 0x67
	mov byte [0xB8298], 0x20
	mov byte [0xB829A], 0x47
	mov byte [0xB829C], 0x44
	mov byte [0xB829E], 0x54
	mov byte [0xB82A0], 0x2e
	mov byte [0xB82A2], 0x2e
	mov byte [0xB82A4], 0x2e
	mov ecx, 0x00F00000
	.Loop1:
	loop .Loop1

	; Setup the primary TSS Selector to point to the TSS
	; Only need to enter the base address. Everything else is setup
	;	in the allocations
	lea eax, [_NATIVE_GDT_Contents+40]
	lea dword ebx, [TSS_POINTER]
	mov byte [eax+2], bl
	shr ebx, 8
	mov byte [eax+3], bl
	shr ebx, 8
	mov byte [eax+4], bl
	shr ebx, 8
	mov byte [eax+7], bl

	; Tell CPU about GDT
	mov dword [_NATIVE_GDT_Pointer + 2], _NATIVE_GDT_Contents
	mov dword eax, _NATIVE_GDT_Pointer
	lgdt [eax]
	; Set data segments
	mov dword eax, 0x10
	mov word ds, eax
	mov word es, eax
	mov word fs, eax
	mov word gs, eax
	mov word ss, eax
	; Force reload of code segment
	jmp 8:Boot_FlushCsGDT
Boot_FlushCsGDT:

	
	; done.
	mov byte [0xB82A6], 0x64
	mov byte [0xB82A8], 0x6f
	mov byte [0xB82AA], 0x6e
	mov byte [0xB82AC], 0x65
	mov byte [0xB82AE], 0x2e
	mov ecx, 0x00F00000
	.Loop2:
	loop .Loop2

; END - Create GDT
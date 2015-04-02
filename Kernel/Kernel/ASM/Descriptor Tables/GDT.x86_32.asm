BITS 32

SECTION .text

EXTERN _NATIVE_GDT_Contents
EXTERN _NATIVE_GDT_Pointer
EXTERN TSS_POINTER

; BEGIN - Create GDT
	; See MultibootSignature.x86_32.asm for memory allocations
	
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
; END - Create GDT
BITS 32

SECTION .text
GLOBAL File_GDT:function
File_GDT:

EXTERN _NATIVE_GDT_Contents
EXTERN _NATIVE_GDT_Pointer
EXTERN TSS_POINTER

; BEGIN - Create GDT
	; See MultibootSignature.x86_32.asm for memory allocations
	
	
	mov dword eax, 0x7F
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
	mov ecx, 0x0F000000
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

	
	mov dword eax, 0x8F
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
	mov ecx, 0x0F000000
	.Loop2:
	loop .Loop2

; END - Create GDT
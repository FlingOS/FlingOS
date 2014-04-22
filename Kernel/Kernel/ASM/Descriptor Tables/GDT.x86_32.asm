; BEGIN - Create GDT
	; See MultibootSignature.x86_32.asm for memory allocations
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
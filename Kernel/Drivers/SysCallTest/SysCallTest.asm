BITS 32
Start:
	
	mov dword eax, 0xDEADBEEF

	Continue:
	int 48

	jmp Continue
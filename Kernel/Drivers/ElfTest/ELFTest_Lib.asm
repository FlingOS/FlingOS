BITS 32

GLOBAL SysCall_Sleep:function

SECTION .text
SysCall_Sleep:
	pushad
	
	; Sys Call Num = 1 = Sleep
	mov dword eax, 1
	; Duration = 1000ms
	mov dword ebx, 1000
	int 48

	popad
	ret
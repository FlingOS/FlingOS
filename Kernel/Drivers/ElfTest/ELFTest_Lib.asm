BITS 32

GLOBAL SysCall_Sleep:function
GLOBAL SysCall_PlayNote:function

SECTION .text
SysCall_Sleep:
	pushad
	
	; Sys Call Num = 1 = Sleep
	mov dword eax, 1
	int 48

	popad
	ret

SysCall_PlayNote:
	pushad
	
	; Sys Call Num = 2 = Play Note
	mov dword eax, 2
	int 48

	popad
	ret
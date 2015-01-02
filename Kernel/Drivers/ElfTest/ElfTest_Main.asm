BITS 32

GLOBAL _start

EXTERN SysCall_Sleep
EXTERN SysCall_PlayNote

SECTION .text
_start:

	; Starting colour	
	mov dword ecx, 0x11

	Continue:

	push dword ecx

	mov dword ebx, 261
	mov dword ecx, 16
	mov dword edx, 120
	call SysCall_PlayNote

	mov dword ebx, 10000
	call SysCall_Sleep

	pop dword ecx

	; Output following text to first bit of vid mem
	;  P  r   o   c  e   s   s     5
	; 80 114 111 99 101 115 115 32 54
	mov byte [0xB8000], 80
	mov byte [0xB8002], 114
	mov byte [0xB8004], 111
	mov byte [0xB8006], 99
	mov byte [0xB8008], 101
	mov byte [0xB800A], 115
	mov byte [0xB800C], 115
	mov byte [0xB800E], 32
	mov byte [0xB8010], 49

	; Set the colour of the outputted text to ecx
	mov byte [0xB8001], cl
	mov byte [0xB8003], cl
	mov byte [0xB8005], cl
	mov byte [0xB8007], cl
	mov byte [0xB8009], cl
	mov byte [0xB800B], cl
	mov byte [0xB800D], cl
	mov byte [0xB800F], cl
	mov byte [0xB8011], cl

	add ecx, 0x11
	and ecx, 0xFF

	jmp Continue
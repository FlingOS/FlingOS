BITS 32
Start:
	; Output following text to first bit of vid mem
	;  P  r   o   c  e   s   s     1
	; 80 114 111 99 101 115 115 32 49 
	mov byte [0xB8000], 80
	mov byte [0xB8002], 114
	mov byte [0xB8004], 111
	mov byte [0xB8006], 99
	mov byte [0xB8008], 101
	mov byte [0xB800A], 115
	mov byte [0xB800C], 115
	mov byte [0xB800E], 32
	mov byte [0xB8010], 49

	; Set the colour of the outputted text to:
	; Green background (0x2-), 
	; White foreground (0x-F)
	mov dword eax, 0x2F
	mov byte [0xB8001], al
	mov byte [0xB8003], al
	mov byte [0xB8005], al
	mov byte [0xB8007], al
	mov byte [0xB8009], al
	mov byte [0xB800B], al
	mov byte [0xB800D], al
	mov byte [0xB800F], al
	mov byte [0xB8011], al

	jmp Start
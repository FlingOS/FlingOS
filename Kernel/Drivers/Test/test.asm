BITS 32
Start:
	; Output following text to first bit of vid mem
	;  M   u   l   t   i   b   o  o   t
	; 109 117 108 116 105 98 111 111 116
	mov byte [0xB8000], 109
	mov byte [0xB8002], 117
	mov byte [0xB8004], 108
	mov byte [0xB8006], 116
	mov byte [0xB8008], 105
	mov byte [0xB800A], 98
	mov byte [0xB800C], 111
	mov byte [0xB800E], 111
	mov byte [0xB8010], 116

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
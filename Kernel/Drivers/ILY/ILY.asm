BITS 32
Start:
	; Output following text to first bit of vid mem
	;  P  r   o   c  e   s   s     3
	; 80 114 111 99 101 115 115 32 51
	; mov byte [0xB8000], 80
	
	; White border
	; Light red fill
	; ................................................................................ - 80
	; ................................................................................ - 80
	; ................................................................................ - 80
	; ...................................I LOVE YOU................................... - 35, 10, 35
	; ................................................................................ - 80
	; ................................................................................ - 80
	; ..............................|||||...........|||||............................. - 30, 5, 11, 5, 29
	; ............................||.....|.........|.....||........................... - 28, 2, 5,  1, 9, 1, 5,  2, 27
	; ..........................||........|.......|........||......................... - 26, 2, 8,  1, 7, 1, 8,  2, 25
	; ........................||...........|.....|...........||....................... - 24, 2, 11, 1, 5, 1, 11, 2, 23
	; ......................||..............|...|..............||..................... - 22, 2, 14, 1, 3, 1, 14, 2, 21
	; ....................||.................|||.................||................... - 20, 2, 17, 3, 17, 2, 19
	; ......................|...................................|..................... - 22, 1, 35, 1, 21
	; .......................|.................................|...................... - 23, 1, 33, 1, 22
	; ........................|...............................|....................... - 24, 1, 31, 1, 23
	; .........................|.............................|........................ - 25, 1, 29, 1, 24
	; ..........................|...........................|......................... - 26, 1, 27, 1, 25
	; ...........................||.......................||.......................... - 27, 2, 23, 2, 26
	; .............................||...................||............................ - 29, 2, 19, 2, 28
	; ...............................||...............||.............................. - 31, 2, 15, 2, 30
	; .................................||...........||................................ - 33, 2, 11, 2, 32
	; ...................................||.......||.................................. - 35, 2, 7, 2, 34
	; .....................................||...||.................................... - 37, 2, 3, 2, 36
	; .......................................|||...................................... - 39, 3, 38
	; ................................................................................ - 80

	%assign LoopNum 1
	%macro SetColourLoop 3
	mov dword ebx, 0
	mov dword edx, %1
	mov dword ecx, %2
	.Loop%3:
	mov byte [eax], bl
	mov byte [eax+1], dl
	add eax, 0x2
	loop .Loop%3
	%endmacro

	mov eax, 0xB8000

	SetColourLoop 0x00, 275, LoopNum
	%assign LoopNum LoopNum+1
	
	; I LOVE YOU
	; 73 32 76 79 86 69 32 89 79 85 (Decimal)
	mov edx, 0x0F ; DL = Colour = White on black
	mov byte [eax], 73
	mov byte [eax+1], dl
	add eax, 2
	mov byte [eax], 32
	mov byte [eax+1], dl
	add eax, 2
	mov byte [eax], 76
	mov byte [eax+1], dl
	add eax, 2
	mov byte [eax], 79
	mov byte [eax+1], dl
	add eax, 2
	mov byte [eax], 86
	mov byte [eax+1], dl
	add eax, 2
	mov byte [eax], 69
	mov byte [eax+1], dl
	add eax, 2
	mov byte [eax], 32
	mov byte [eax+1], dl
	add eax, 2
	mov byte [eax], 89
	mov byte [eax+1], dl
	add eax, 2
	mov byte [eax], 79
	mov byte [eax+1], dl
	add eax, 2
	mov byte [eax], 85
	mov byte [eax+1], dl
	add eax, 2
	
	SetColourLoop 0x00, 225, LoopNum
	%assign LoopNum LoopNum+1
		
	SetColourLoop 0xFF, 5, LoopNum
	%assign LoopNum LoopNum+1
		
	SetColourLoop 0x00, 11, LoopNum
	%assign LoopNum LoopNum+1	

	SetColourLoop 0xFF, 5, LoopNum
	%assign LoopNum LoopNum+1
		
	SetColourLoop 0x00, 29, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0x00, 28, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 2, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xCC, 5, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 1, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0x00, 9, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 1, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xCC, 5, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 2, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0x00, 27, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0x00, 26, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 2, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xCC, 8, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 1, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0x00, 7, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 1, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xCC, 8, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 2, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0x00, 25, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0x00, 24, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 2, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xCC, 11, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 1, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0x00, 5, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 1, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xCC, 11, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 2, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0x00, 23, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0x00, 22, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 2, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xCC, 14, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 1, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0x00, 3, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 1, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xCC, 14, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 2, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0x00, 21, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0x00, 20, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 2, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xCC, 17, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 3, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xCC, 17, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 2, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0x00, 19, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0x00, 22, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 1, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xCC, 35, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 1, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0x00, 21, LoopNum
	%assign LoopNum LoopNum+1
	
	
	SetColourLoop 0x00, 23, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 1, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xCC, 33, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 1, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0x00, 22, LoopNum
	%assign LoopNum LoopNum+1
	
	
	SetColourLoop 0x00, 24, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 1, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xCC, 31, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 1, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0x00, 23, LoopNum
	%assign LoopNum LoopNum+1
	
	
	SetColourLoop 0x00, 25, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 1, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xCC, 29, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 1, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0x00, 24, LoopNum
	%assign LoopNum LoopNum+1
	
	
	SetColourLoop 0x00, 26, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 1, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xCC, 27, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 1, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0x00, 25, LoopNum
	%assign LoopNum LoopNum+1
	
	
	SetColourLoop 0x00, 27, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 2, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xCC, 23, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 2, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0x00, 26, LoopNum
	%assign LoopNum LoopNum+1
	
	
	SetColourLoop 0x00, 29, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 2, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xCC, 19, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 2, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0x00, 28, LoopNum
	%assign LoopNum LoopNum+1
	
	
	SetColourLoop 0x00, 31, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 2, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xCC, 15, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 2, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0x00, 30, LoopNum
	%assign LoopNum LoopNum+1
	
	
	SetColourLoop 0x00, 33, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 2, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xCC, 11, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 2, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0x00, 32, LoopNum
	%assign LoopNum LoopNum+1
	
	
	SetColourLoop 0x00, 35, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 2, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xCC, 7, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 2, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0x00, 34, LoopNum
	%assign LoopNum LoopNum+1
	
	
	SetColourLoop 0x00, 37, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 2, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xCC, 3, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 2, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0x00, 36, LoopNum
	%assign LoopNum LoopNum+1
	
	
	SetColourLoop 0x00, 39, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0xFF, 3, LoopNum
	%assign LoopNum LoopNum+1
	
	SetColourLoop 0x00, 38, LoopNum
	%assign LoopNum LoopNum+1
	
	
	SetColourLoop 0x00, 80, LoopNum
	%assign LoopNum LoopNum+1
	
	ret
	jmp Start
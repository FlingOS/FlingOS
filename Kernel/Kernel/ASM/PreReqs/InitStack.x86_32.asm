; - - - - - - - - - - - - - - - - - - - LICENSE - - - - - - - - - - - - - - - -  ;
;
;    Fling OS - The educational operating system
;    Copyright (C) 2015 Edward Nutting
;
;    This program is free software: you can redistribute it and/or modify
;    it under the terms of the GNU General Public License as published by
;    the Free Software Foundation, either version 2 of the License, or
;    (at your option) any later version.
;
;    This program is distributed in the hope that it will be useful,
;    but WITHOUT ANY WARRANTY; without even the implied warranty of
;    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
;    GNU General Public License for more details.
;
;    You should have received a copy of the GNU General Public License
;    along with this program.  If not, see <http:;www.gnu.org/licenses/>.
;
;  Project owner: 
;		Email: edwardnutting@outlook.com
;		For paper mail address, please contact via email for details.
;
; - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -  ;
BITS 32

SECTION .text
GLOBAL File_InitStack:function
File_InitStack:

EXTERN Kernel_Stack

; BEGIN - Init stack

	; See comment at start of Main Entrypoint
	mov dword ecx, 0x0

	; Initialising stack...
	mov byte [0xB81E0], 0x49
	mov byte [0xB81E2], 0x6e
	mov byte [0xB81E4], 0x69
	mov byte [0xB81E6], 0x74
	mov byte [0xB81E8], 0x69
	mov byte [0xB81EA], 0x61
	mov byte [0xB81EC], 0x6c
	mov byte [0xB81EE], 0x69
	mov byte [0xB81F0], 0x73
	mov byte [0xB81F2], 0x69
	mov byte [0xB81F4], 0x6e
	mov byte [0xB81F6], 0x67
	mov byte [0xB81F8], 0x20
	mov byte [0xB81FA], 0x73
	mov byte [0xB81FC], 0x74
	mov byte [0xB81FE], 0x61
	mov byte [0xB8200], 0x63
	mov byte [0xB8202], 0x6b
	mov byte [0xB8204], 0x2e
	mov byte [0xB8206], 0x2e
	mov byte [0xB8208], 0x2e
	mov ecx, 0x00F00000
	.Loop1:
	loop .Loop1

mov dword ESP, Kernel_Stack ; Set the stack pointer to point at our pre-allocated block of memory


	; done.
	mov byte [0xB820A], 0x64
	mov byte [0xB820C], 0x6f
	mov byte [0xB820E], 0x6e
	mov byte [0xB8210], 0x65
	mov byte [0xB8212], 0x2e
	mov ecx, 0x00F00000
	.Loop2:
	loop .Loop2
; END - Init stack
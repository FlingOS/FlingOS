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
File_MultibootSignature:

GLOBAL MultibootSignature:data
GLOBAL MultibootFlags:data
GLOBAL MultibootChecksum:data
GLOBAL MultibootGraphicsRuntime_VbeModeInfoAddr:data
GLOBAL MultibootGraphicsRuntime_VbeControlInfoAddr:data
GLOBAL MultibootGraphicsRuntime_VbeMode:data
GLOBAL MultiBootInfo_Memory_High:data
GLOBAL MultiBootInfo_Memory_Low:data

GLOBAL Before_Kernel_Stack:data
GLOBAL Kernel_Stack
GLOBAL MultiBootInfo_Structure:data

GLOBAL GDT_Contents:data
GLOBAL GDT_Pointer:data
GLOBAL IDT_Contents:data
GLOBAL IDT_Pointer:data
GLOBAL TSS_Contents:data
GLOBAL TSS_Pointer:data

; BEGIN - Multiboot Signature
MultibootSignature dd 464367618
MultibootFlags dd 3
MultibootChecksum dd -464367621
MultibootGraphicsRuntime_VbeModeInfoAddr dd 2147483647
MultibootGraphicsRuntime_VbeControlInfoAddr dd 2147483647
MultibootGraphicsRuntime_VbeMode dd 2147483647
MultiBootInfo_Memory_High dd 0
MultiBootInfo_Memory_Low dd 0


KERNEL_VIRTUAL_BASE equ 0x00000000					; 3GiB
KERNEL_PAGE_NUMBER equ (KERNEL_VIRTUAL_BASE >> 22)

EXTERN Kernel_MemStart

Before_Kernel_Stack: TIMES 65535 db 0
Kernel_Stack:


MultiBootInfo_Structure dd 0

; This is the GDT table pre-filled with the entries we want
GDT_Contents:
; I have a suspicion that the order of the items in the GDT matters
;	Code and data selectors first then TSS
db 0, 0, 0, 0, 0, 0, 0, 0			; Offset: 0  - Null selector - required 
db 255, 255, 0, 0, 0, 0x9A, 0xCF, 0	; Offset: 8  - KM Code selector - covers the entire 4GiB address range
db 255, 255, 0, 0, 0, 0x92, 0xCF, 0	; Offset: 16 - KM Data selector - covers the entire 4GiB address range
db 255, 255, 0, 0, 0, 0xFA, 0xCF, 0	; Offset: 24 - UM Code selector - covers the entire 4GiB address range
db 255, 255, 0, 0, 0, 0xF2, 0xCF, 0	; Offset: 32 - UM Data selector - covers the entire 4GiB address range
db 0x67,  0, 0, 0, 0, 0xE9, 0x00, 0	; Offset: 40 - TSS Selector - Pointer to the TSS 

;					   Size - Change iff adding/removing rows from GDT contents
;					   Size = Total bytes in GDT - 1
GDT_Pointer db 47, 0, 0, 0, 0, 0
IDT_Contents: TIMES 2048 db 0
IDT_Pointer db 0xFF, 0x7, 0, 0, 0, 0

TSS_Contents:
TIMES 104 db 0
TSS_Pointer equ (TSS_Contents - KERNEL_VIRTUAL_BASE)

; END - Multiboot Signature




GLOBAL File_Kernel_Start:function
File_Kernel_Start:

GLOBAL Kernel_Start:function

; BEGIN - Kernel Start

Kernel_Start equ (_Kernel_Start - KERNEL_VIRTUAL_BASE)

_Kernel_Start:

	cli
	
	; MultiBoot compliant loader provides info in registers: 
	; EBX=multiboot_info 
	; EAX=0x2BADB002	- check if it's really Multiboot loader 
	;					- if true, continue and copy mb info
	; BEGIN - Multiboot Info
	mov dword ecx, 0x2BADB002
	cmp ecx, eax
	jne (Kernel_Start_HandleNoMultiboot - KERNEL_VIRTUAL_BASE)
	
	mov dword [MultiBootInfo_Structure - KERNEL_VIRTUAL_BASE], EBX
	add dword EBX, 0x4
	mov dword EAX, [EBX]
	mov dword [MultiBootInfo_Memory_Low - KERNEL_VIRTUAL_BASE], EAX
	add dword EBX, 0x4
	mov dword EAX, [EBX]
	mov dword [MultiBootInfo_Memory_High - KERNEL_VIRTUAL_BASE], EAX
	
	
	mov dword eax, 0x2F
	mov dword ebx, 0xB8000
	mov dword ecx, 2000
	.ColourSetup:
	mov byte [ebx], 0
	mov byte [ebx+1], al
	add ebx, 2
	loop .ColourSetup

	; Enabling protected mode...
	; 0x45 0x6e 0x61 0x62 0x6c 0x69 0x6e 0x67 0x20 0x70 0x72 0x6f 0x74 0x65 0x63 0x74 0x65 0x64 0x20 0x6d 0x6f 0x64 0x65 0x2e 0x2e 0x2e
	mov byte [0xB8000], 0x45
	mov byte [0xB8002], 0x6e
	mov byte [0xB8004], 0x61 
	mov byte [0xB8006], 0x62 
	mov byte [0xB8008], 0x6c 
	mov byte [0xB800A], 0x69 
	mov byte [0xB800C], 0x6e 
	mov byte [0xB800E], 0x67 
	mov byte [0xB8010], 0x20 
	mov byte [0xB8012], 0x70 
	mov byte [0xB8014], 0x72 
	mov byte [0xB8016], 0x6f 
	mov byte [0xB8018], 0x74 
	mov byte [0xB801A], 0x65 
	mov byte [0xB801C], 0x63 
	mov byte [0xB801E], 0x74 
	mov byte [0xB8020], 0x65 
	mov byte [0xB8022], 0x64 
	mov byte [0xB8024], 0x20 
	mov byte [0xB8026], 0x6d 
	mov byte [0xB8028], 0x6f 
	mov byte [0xB802A], 0x64 
	mov byte [0xB802C], 0x65 
	mov byte [0xB802E], 0x2e 
	mov byte [0xB8030], 0x2e 
	mov byte [0xB8032], 0x2e 
	
	mov ecx, 0x00F00000
	.Loop1:
	loop .Loop1

	; Enable Protected Mode
	mov eax, cr0
	or eax, 0x1
	mov cr0, eax
	
	; done.
	; 0x64 0x6f 0x6e 0x65 0x2e 
	
	mov byte [0xB8034], 0x64 
	mov byte [0xB8036], 0x6f 
	mov byte [0xB8038], 0x6e 
	mov byte [0xB803A], 0x65 
	mov byte [0xB803C], 0x2e 

	mov ecx, 0x00F00000
	.Loop2:
	loop .Loop2

	; END - Multiboot Info
; END - Kernel Start




GLOBAL File_InitStack:function
File_InitStack:

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




GLOBAL File_GDT:function
File_GDT:

; BEGIN - Create GDT
	; See MultibootSignature.x86_32.asm for memory allocations
	
	; See comment at start of Main Entrypoint
	mov dword ecx, 0x0

	; Initialising GDT...
	mov byte [0xB8280], 0x49
	mov byte [0xB8282], 0x6e
	mov byte [0xB8284], 0x69
	mov byte [0xB8286], 0x74
	mov byte [0xB8288], 0x69
	mov byte [0xB828A], 0x61
	mov byte [0xB828C], 0x6c
	mov byte [0xB828E], 0x69
	mov byte [0xB8290], 0x73
	mov byte [0xB8292], 0x69
	mov byte [0xB8294], 0x6e
	mov byte [0xB8296], 0x67
	mov byte [0xB8298], 0x20
	mov byte [0xB829A], 0x47
	mov byte [0xB829C], 0x44
	mov byte [0xB829E], 0x54
	mov byte [0xB82A0], 0x2e
	mov byte [0xB82A2], 0x2e
	mov byte [0xB82A4], 0x2e
	mov ecx, 0x00F00000
	.Loop1:
	loop .Loop1

	; Setup the primary TSS Selector to point to the TSS
	; Only need to enter the base address. Everything else is setup
	;	in the allocations
	lea eax, [GDT_Contents+40]
	lea dword ebx, [TSS_Pointer]
	mov byte [eax+2], bl
	shr ebx, 8
	mov byte [eax+3], bl
	shr ebx, 8
	mov byte [eax+4], bl
	shr ebx, 8
	mov byte [eax+7], bl

	; Tell CPU about GDT
	mov dword [GDT_Pointer + 2], GDT_Contents
	mov dword eax, GDT_Pointer
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

	
	; done.
	mov byte [0xB82A6], 0x64
	mov byte [0xB82A8], 0x6f
	mov byte [0xB82AA], 0x6e
	mov byte [0xB82AC], 0x65
	mov byte [0xB82AE], 0x2e
	mov ecx, 0x00F00000
	.Loop2:
	loop .Loop2

; END - Create GDT

GLOBAL File_HandleNoMultiboot:function
File_HandleNoMultiboot:

GLOBAL Kernel_Start_HandleNoMultiboot:function

	; See comment at start of Main Entrypoint
	mov dword ecx, 0x0

; BEGIN - Handle No Multiboot
jmp Kernel_Start_HandleNoMultiboot_End ; Skip over this code - we don't want to run it by accident!

Kernel_Start_HandleNoMultiboot:

; Not entirely sure if we'd ever actually get as far as due to code structure but anyway...
; Displays a warning message to the user saying "No multiboot" indicating the multiboot signature
; (which should have been in eax) was not detected so we don't think we have a valid boot setup
; so we are aborting the boot to avoid damage
	
	; Output following text to first bit of vid mem
	; N	  o      M  u    l   t   i   b   o  o   t
	; 78 111 32 109 117 108 116 105 98 111 111 116
	mov byte [0xB8000], 78
	mov byte [0xB8002], 111
	mov byte [0xB8004], 32
	mov byte [0xB8006], 109
	mov byte [0xB8008], 117
	mov byte [0xB800A], 108
	mov byte [0xB800C], 116
	mov byte [0xB800E], 105
	mov byte [0xB8010], 98
	mov byte [0xB8012], 111
	mov byte [0xB8014], 111
	mov byte [0xB8016], 116

	; Set the colour of the outputted text to:
	; Red background (0x4-), 
	; White foreground (0x-F)
	mov dword eax, 0x4F
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

	cli ; Prevent any more interrupt requests re-awakening us
	hlt ; Halt the OS / execution / etc.
	jmp Kernel_Start_HandleNoMultiboot ; Just in case...

Kernel_Start_HandleNoMultiboot_End:
; END - Handle No Multiboot




GLOBAL File_MainEntryPoint:function
File_MainEntryPoint:


; Hmm... something has to go before the call op otherwise the OS crashes at boot.
;		It can be min. ~12 NOPs or this mov or probably many other things...
;	This is the same phenomenon observed at the start of VirtualMemInit
mov dword ecx, 0x0

; BEGIN - Main Entrypoint
call __MAIN_ENTRYPOINT__ ; Call our main entry point 

EXTERN %KERNEL_CALL_STATIC_CONSTRUCTORS_METHOD%
EXTERN %KERNEL_MAIN_METHOD%

__MAIN_ENTRYPOINT__:
	push dword ebp
	mov dword ebp, esp
	
	
	; Calling static constructors...
	mov byte [0xB83C0], 0x43
	mov byte [0xB83C2], 0x61
	mov byte [0xB83C4], 0x6c
	mov byte [0xB83C6], 0x6c
	mov byte [0xB83C8], 0x69
	mov byte [0xB83CA], 0x6e
	mov byte [0xB83CC], 0x67
	mov byte [0xB83CE], 0x20
	mov byte [0xB83D0], 0x73
	mov byte [0xB83D2], 0x74
	mov byte [0xB83D4], 0x61
	mov byte [0xB83D6], 0x74
	mov byte [0xB83D8], 0x69
	mov byte [0xB83DA], 0x63
	mov byte [0xB83DC], 0x20
	mov byte [0xB83DE], 0x63
	mov byte [0xB83E0], 0x6f
	mov byte [0xB83E2], 0x6e
	mov byte [0xB83E4], 0x73
	mov byte [0xB83E6], 0x74
	mov byte [0xB83E8], 0x72
	mov byte [0xB83EA], 0x75
	mov byte [0xB83EC], 0x63
	mov byte [0xB83EE], 0x74
	mov byte [0xB83F0], 0x6f
	mov byte [0xB83F2], 0x72
	mov byte [0xB83F4], 0x73
	mov byte [0xB83F6], 0x2e
	mov byte [0xB83F8], 0x2e
	mov byte [0xB83FA], 0x2e
	mov ecx, 0x00F00000
	.Loop1:
	loop .Loop1


	call %KERNEL_CALL_STATIC_CONSTRUCTORS_METHOD% ; Call the static constructors - this is a macro used by the kernel compiler.
	
	; done.
	mov byte [0xB83FC], 0x64
	mov byte [0xB83FE], 0x6f
	mov byte [0xB8400], 0x6e
	mov byte [0xB8402], 0x65
	mov byte [0xB8404], 0x2e
	
	; Calling kernel main method...
	mov byte [0xB8460], 0x43
	mov byte [0xB8462], 0x61
	mov byte [0xB8464], 0x6c
	mov byte [0xB8466], 0x6c
	mov byte [0xB8468], 0x69
	mov byte [0xB846A], 0x6e
	mov byte [0xB846C], 0x67
	mov byte [0xB846E], 0x20
	mov byte [0xB8470], 0x6b
	mov byte [0xB8472], 0x65
	mov byte [0xB8474], 0x72
	mov byte [0xB8476], 0x6e
	mov byte [0xB8478], 0x65
	mov byte [0xB847A], 0x6c
	mov byte [0xB847C], 0x20
	mov byte [0xB847E], 0x6d
	mov byte [0xB8480], 0x61
	mov byte [0xB8482], 0x69
	mov byte [0xB8484], 0x6e
	mov byte [0xB8486], 0x20
	mov byte [0xB8488], 0x6d
	mov byte [0xB848A], 0x65
	mov byte [0xB848C], 0x74
	mov byte [0xB848E], 0x68
	mov byte [0xB8490], 0x6f
	mov byte [0xB8492], 0x64
	mov byte [0xB8494], 0x2e
	mov byte [0xB8496], 0x2e
	mov byte [0xB8498], 0x2e
		
	call %KERNEL_MAIN_METHOD% ; Call our main method - this is a macro used by the kernel compiler.
	
	; We shouldn't ever get to this point! But just in case we do...
	jmp method_System_Void_Kernel_PreReqs_Reset__ ; For now this is our intended behaviour

; END - Main Entrypoint




GLOBAL File_Reset:function
File_Reset:

GLOBAL method_System_Void_Kernel_PreReqs_Reset__:function
GLOBAL method_System_Void_RETEND_FlingOops_PreReqs_DECLEND_Reset_NAMEEND___:function

; BEGIN - Reset
method_System_Void_Kernel_PreReqs_Reset__:
method_System_Void_RETEND_FlingOops_PreReqs_DECLEND_Reset_NAMEEND___:
	cli ; Clear all interrupts so we aren't re-awoken
	hlt	; Halt the OS / CPU / etc.
	jmp method_System_Void_Kernel_PreReqs_Reset__ ; Just in case...
; END - Reset




GLOBAL File_WriteDebugVideo:function
File_WriteDebugVideo:

GLOBAL method_System_Void_RETEND_FlingOops_PreReqs_DECLEND_WriteDebugVideo_NAMEEND__System_String_System_UInt32_:function

; BEGIN - Write Debug Video
method_System_Void_RETEND_FlingOops_PreReqs_DECLEND_WriteDebugVideo_NAMEEND__System_String_System_UInt32_:

; MethodStart
push dword ebp
mov dword ebp, esp

mov dword ebx, 0xB8000 ; Load vid mem base address

; Set num characters to clear 
mov dword eax, 80	; 80 characters per line
mov dword ecx, 25	; 25 lines
mul dword ecx		; eax * ecx, store in eax = numbers of characters to clear
mov dword ecx, eax	; Store number of characters to clear in ecx for later use in loop

mov byte ah, 0x00	; Set colour to clear to. Here black for background and foreground
mov byte al, 0		; Set the character to the null/empty character

.Loop1:
mov word [ebx], ax	; Move the empty character/colour to vid mem
add dword ebx, 2	; Move to next character space in vid mem
; Uses ecx - loops till ecx = 0 i.e. till all characters cleared
loop .Loop1


; Load string length
mov eax, [ebp+12]	 ; Load string address
mov dword ecx, [eax] ; String length is first dword of string

mov edx, [ebp+12]	; Load string address
add edx, 4			; Skip first dword because that is the length not a character

mov dword ebx, 0xB8000 ; Load vid mem base address

mov byte ah, [ebp+8] ; Load colour

.Loop2:
mov byte al, [edx]		; Get current character of string
mov word [ebx], ax		; Move character and colour into vid mem
add dword ebx, 2		; Move to next character space in vid mem
add dword edx, 1		; Move to next character in string
; Uses ecx - loops till ecx = 0 i.e. till all characters gone through
loop .Loop2

; MethodEnd
pop dword ebp

ret
; END - Write Debug Video
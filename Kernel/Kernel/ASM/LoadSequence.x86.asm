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
GLOBAL TSS_POINTER:data

; BEGIN - Multiboot Signature
MultibootSignature dd 464367618
MultibootFlags dd 3
MultibootChecksum dd -464367621

MultiBootInfo_Memory_High dd 0
MultiBootInfo_Memory_Low dd 0

Before_Kernel_Stack: TIMES 65535 db 0
Kernel_Stack:

EXTERN Kernel_MemStart
EXTERN Kernel_MemEnd

EXTERN __bss_start
EXTERN __bss_end

KERNEL_VIRTUAL_BASE equ 0xC0000000					; 3GiB
KERNEL_PAGE_NUMBER equ (KERNEL_VIRTUAL_BASE >> 22)

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
TSS_POINTER equ (TSS_Contents - KERNEL_VIRTUAL_BASE)

; END - Multiboot Signature

GLOBAL Kernel_Start:function

; BEGIN - Kernel Start

Kernel_Start:

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
	
	; mov ecx, 0x00F00000
	; .Kernel_Start_Loop1:
	; loop .Kernel_Start_Loop1

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

	; mov ecx, 0x00F00000
	; .Kernel_Start_Loop2:
	; loop .Kernel_Start_Loop2

	; END - Multiboot Info


	; START - Zero out bss

	lea ecx, [__bss_end - KERNEL_VIRTUAL_BASE]
	lea eax, [__bss_start - KERNEL_VIRTUAL_BASE]
	sub ecx, eax
	.ZeroOutBSS:
	mov byte [eax], 0
	add eax, 1
	loop .ZeroOutBSS
	
	
	mov dword eax, 0x5F
	mov dword ebx, 0xB8000
	mov dword ecx, 2000
	.ColourChange1:
	mov byte [ebx+1], al
	add ebx, 2
	loop .ColourChange1

	; END - Zero out bss

; END - Kernel Start

EXTERN Page_Table1
EXTERN Page_Directory

; BEGIN - Virtual Mem Init

; See MultibootSignature for memory allocations

; 1. Map virtual memory for physical address execution
; 2. Map virtual memory for virtual address execution
; 3. Set page directory
; 4. Switch on paging
; 5. Load a continuation label's virtual address
; 6. Jump to the virtual address

; This is so that execution can continue / occur at both the physical address and 
; the virtual address.


; 1. Map virtual memory for physical address execution
;		This means create pages such that:
; 
;		0x00000000			->	0x00000000
;		0x00100000			->	0x00100000
;		...
;		0x0-End of kernel	->	0x0-End of kernel

; Identity map low 1MiB pages to low 1MiB pages
mov ecx, 256
lea eax, [Page_Table1 - KERNEL_VIRTUAL_BASE]
mov ebx, 7
.VirtMem_Loop1:
mov [eax], ebx
add eax, 4
add ebx, 4096
loop .VirtMem_Loop1

; Calculate number of pages for the kernel
mov eax, Kernel_MemEnd
sub eax, Kernel_MemStart
mov edx, 0
mov ebx, 4096
div ebx
mov ecx, eax
add ecx, 1

; Identity map low pages to low pages

; Moves pointer to page table for kernel mem start mark
lea eax, [Kernel_MemStart - KERNEL_VIRTUAL_BASE] ; ((x / (4096*1024)) * (1024*4)) 
mov edx, 0
mov ebx, 1024
div ebx
add eax, (Page_Table1 - KERNEL_VIRTUAL_BASE)

mov ebx, 7
add ebx, (Kernel_MemStart - KERNEL_VIRTUAL_BASE)
.VirtMem_Loop2:
mov [eax], ebx
add eax, 4
add ebx, 4096
loop .VirtMem_Loop2


; 2. Map virtual memory for virtual address execution
;		This means create pages such that:
; 
;		0xC0000000			->	0x00000000
;		0xC0100000			->	0x00100000
;		...
;		0xC-End of kernel	->	0x0-End of kernel

; Map high 1MiB pages to low 1MiB pages
mov ecx, 256
lea eax, [Page_Table1 - KERNEL_VIRTUAL_BASE]
add eax, 0x300000 ; Moves pointer to page table for 3GiB mark ((0xC0000000 / (4096 * 1024)) * (1024*4))
mov ebx, 7
.VirtMem_Loop3:
mov [eax], ebx
add eax, 4
add ebx, 4096
loop .VirtMem_Loop3

; Calculate number of pages for the kernel
mov eax, Kernel_MemEnd
sub eax, Kernel_MemStart
mov edx, 0
mov ebx, 4096
div ebx
mov ecx, eax
add ecx, 1

; Map high pages to low pages

; Moves pointer to page table for kernel mem start mark
lea eax, [Kernel_MemStart] ; ((x / (4096*1024)) * (1024*4)) 
mov edx, 0
mov ebx, 1024
div ebx
add eax, (Page_Table1 - KERNEL_VIRTUAL_BASE)

mov ebx, 7
add ebx, (Kernel_MemStart - KERNEL_VIRTUAL_BASE)
.VirtMem_Loop4:
mov [eax], ebx
add eax, 4
add ebx, 4096
loop .VirtMem_Loop4

; 3. Set page directory
;	- Load page directory entries
lea ebx, [Page_Table1 - KERNEL_VIRTUAL_BASE]
lea edx, [Page_Directory - KERNEL_VIRTUAL_BASE]
or ebx, 7
mov ecx, 1024
.VirtMem_Loop5:
mov [edx], ebx
add edx, 4
add ebx, 4096
loop .VirtMem_Loop5


; Load the physical address of the page directory 
;	then move it into cr3

lea ecx, [Page_Directory - KERNEL_VIRTUAL_BASE]
mov cr3, ecx

; 4. Switch on paging
;		This requires us to enable paging by setting cr0

mov ecx, cr0
or ecx, 0x80000000   ; Set PG bit in CR0 to enable paging.
mov cr0, ecx

; 5. Load a continuation label's virtual address
;		Use lea to load virtual address of a label immediately following the instruction

	; Virtual memory initialised.
	; 0x56 0x69 0x72 0x74 0x75 0x61 0x6c 0x20 0x6d 0x65 0x6d 0x6f 0x72 0x79 0x20 0x69 0x6e 0x69 0x74 0x69 0x61 0x6c 0x69 0x73 0x65 0x64 0x2e 
	mov byte [0xB80A0], 0x56 
	mov byte [0xB80A2], 0x69 
	mov byte [0xB80A4], 0x72 
	mov byte [0xB80A6], 0x74 
	mov byte [0xB80A8], 0x75 
	mov byte [0xB80AA], 0x61 
	mov byte [0xB80AC], 0x6c 
	mov byte [0xB80AE], 0x20 
	mov byte [0xB80B0], 0x6d 
	mov byte [0xB80B2], 0x65 
	mov byte [0xB80B4], 0x6d 
	mov byte [0xB80B6], 0x6f 
	mov byte [0xB80B8], 0x72 
	mov byte [0xB80BA], 0x79 
	mov byte [0xB80BC], 0x20 
	mov byte [0xB80BE], 0x69 
	mov byte [0xB80C0], 0x6e 
	mov byte [0xB80C2], 0x69 
	mov byte [0xB80C4], 0x74 
	mov byte [0xB80C6], 0x69 
	mov byte [0xB80C8], 0x61 
	mov byte [0xB80CA], 0x6c 
	mov byte [0xB80CC], 0x69 
	mov byte [0xB80CE], 0x73 
	mov byte [0xB80D0], 0x65 
	mov byte [0xB80D2], 0x64 
	mov byte [0xB80D4], 0x2e 

	; mov ecx, 0x00F00000
	; .LoopDisp1:
	; loop .LoopDisp1

lea ecx, [StartInHigherHalf]

; 6. Jump to the virtual address
;		Jump to the virtual address of the label (that has just been loaded)

jmp ecx							; NOTE: Must be absolute jump!
 
StartInHigherHalf:
nop

	; Executing in higher half.
	; 0x45 0x78 0x65 0x63 0x75 0x74 0x69 0x6e 0x67 0x20 0x69 0x6e 0x20 0x68 0x69 0x67 0x68 0x65 0x72 0x20 0x68 0x61 0x6c 0x66 0x2e 
	mov byte [0xB8140], 0x45
	mov byte [0xB8142], 0x78
	mov byte [0xB8144], 0x65
	mov byte [0xB8146], 0x63
	mov byte [0xB8148], 0x75
	mov byte [0xB814A], 0x74
	mov byte [0xB814C], 0x69
	mov byte [0xB814E], 0x6e
	mov byte [0xB8150], 0x67
	mov byte [0xB8152], 0x20
	mov byte [0xB8154], 0x69
	mov byte [0xB8156], 0x6e
	mov byte [0xB8158], 0x20
	mov byte [0xB815A], 0x68
	mov byte [0xB815C], 0x69
	mov byte [0xB815E], 0x67
	mov byte [0xB8160], 0x68
	mov byte [0xB8162], 0x65
	mov byte [0xB8164], 0x72
	mov byte [0xB8166], 0x20
	mov byte [0xB8168], 0x68
	mov byte [0xB816A], 0x61
	mov byte [0xB816C], 0x6c
	mov byte [0xB816E], 0x66
	mov byte [0xB8170], 0x2e

	; mov ecx, 0x00F00000
	; .LoopDisp2:
	; loop .LoopDisp2

; We are now working in the higher-half virtual address space :)

; END - Virtual Mem Init

; BEGIN - Init stack

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
	
	; mov ecx, 0x00F00000
	; .Stack_Loop1:
	; loop .Stack_Loop1

mov dword ESP, Kernel_Stack ; Set the stack pointer to point at our pre-allocated block of memory


	; done.
	mov byte [0xB820A], 0x64
	mov byte [0xB820C], 0x6f
	mov byte [0xB820E], 0x6e
	mov byte [0xB8210], 0x65
	mov byte [0xB8212], 0x2e
	
	; mov ecx, 0x00F00000
	; .Stack_Loop2:
	; loop .Stack_Loop2

; END - Init stack

; BEGIN - Create GDT
	
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

	; mov ecx, 0x00F00000
	; .GDT_Loop1:
	; loop .GDT_Loop1

	; Setup the primary TSS Selector to point to the TSS
	; Only need to enter the base address. Everything else is setup
	;	in the allocations
	lea eax, [GDT_Contents+40]
	lea dword ebx, [TSS_POINTER]
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

	; mov ecx, 0x00F00000
	; .GDT_Loop2:
	; loop .GDT_Loop2

; END - Create GDT

EXTERN method_System_Void_RETEND_Kernel_Debug_Debugger_DECLEND_Int1_NAMEEND___
EXTERN method_System_Void_RETEND_Kernel_Debug_Debugger_DECLEND_Int3_NAMEEND___
EXTERN method_System_Void_RETEND_Kernel_Interrupts_Interrupts_DECLEND_CommonISR_NAMEEND__System_UInt32_
EXTERN method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_PageFaultException_NAMEEND__System_UInt32_System_UInt32_System_UInt32_
EXTERN method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_StackException_NAMEEND___
EXTERN method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_DoubleFaultException_NAMEEND__System_UInt32_System_UInt32_
EXTERN method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_OverflowException_NAMEEND___
EXTERN method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_InvalidOpCodeException_NAMEEND___
EXTERN method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_DivideByZeroException_NAMEEND__System_UInt32_
EXTERN staticfield_Kernel_Multiprocessing_ThreadState__Kernel_Multiprocessing_ProcessManager_CurrentThread_State

%define KERNEL_MODE_DPL 0
%define USER_MODE_DPL 3

; START - General interrupt macros

%macro ENABLE_INTERRUPTS 0
sti
%endmacro



%macro DISABLE_INTERRUPTS 0
cli
nop
%endmacro


%assign STORE_STATE_SKIP_NUM 0
%assign RESTORE_STATE_SKIP_NUM 0

%macro INTERRUPTS_STORE_STATE 1
; Store registers on current thread stack
pushad
push ds
push es
push fs
push gs

; Switch the segment selectors to kernel mode selectors
mov ax, 0x10
mov gs, ax
mov fs, ax
mov es, ax
mov ds, ax

; Load pointer to current thread state
mov dword eax, [staticfield_Kernel_Multiprocessing_ThreadState__Kernel_Multiprocessing_ProcessManager_CurrentThread_State]
; Test for null
cmp eax, 0
; If null, skip
jz INTERRUPTS_STORE_STATE_SKIP_%1

; Check for UserMode process. If UM, we are already
;	on the kernel stack so don't change it or we will
;	lose the values saved in pushes above
; This takes the CS pushed by the processor when it
;	invoked the interrupt, gets the DPL then sees
;	if the DPL==3 i.e. User mode
mov dword ebx, [esp+52]
and ebx, 0x3
cmp ebx, 0x3
je INTERRUPTS_STORE_STATE_COPYACROSS_%1

; Save thread's current stack position
mov dword [eax+1], esp
; Load temp kernel stack address
mov dword ebx, [eax+7]
; Switch to temp kernel stack
mov dword esp, ebx

; Now running on a totally empty kernel stack

jmp INTERRUPTS_STORE_STATE_SKIP_%1

INTERRUPTS_STORE_STATE_COPYACROSS_%1:
; Load thread's UM stack position
mov dword ebx, [esp+60]
; Copy across all the values
sub ebx, 4
mov dword ecx, [esp+64]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+60]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+56]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+52]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+48]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+44]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+40]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+36]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+32]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+28]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+24]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+20]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+16]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+12]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+8]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+4]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+0]
mov dword [ebx], ecx

; Store UM stack position
mov dword [eax+1], ebx

; Restore kernel stack to its proper place
add esp, 64

; Now running on a totally empty kernel stack

INTERRUPTS_STORE_STATE_SKIP_%1:

%endmacro



%macro INTERRUPTS_RESTORE_STATE 1
; Load pointer to current thread state
mov dword eax, [staticfield_Kernel_Multiprocessing_ThreadState__Kernel_Multiprocessing_ProcessManager_CurrentThread_State]
; Test for null
cmp eax, 0
; If null, skip
jz INTERRUPTS_RESTORE_STATE_SKIP_%1


; Restore esp to thread's esp
mov dword esp, [eax+1]

; Load address of temp kernel stack
mov dword ebx, [eax+7]
; Update TSS with kernel stack pointer for next task switch
mov dword [TSS_Contents+4], ebx

jmp INTERRUPTS_RESTORE_STATE_SKIP_END_%1

INTERRUPTS_RESTORE_STATE_SKIP_%1:

INTERRUPTS_RESTORE_STATE_SKIP_END_%1:

pop gs
pop fs
pop es
pop ds
popad
%endmacro



; END - General interrupt macros



; BEGIN - Create IDT
; See MultibootSignature.x86_32.asm for memory allocations

; Set the Int1 handler
; Load handler address
mov dword eax, Debug_Int1Handler
; Set low address bytes into entry (index) 1 of the table
mov byte [IDT_Contents + 8], al
mov byte [IDT_Contents + 9], ah
; Shift the address right 16 bits to get the high address bytes
shr dword eax, 0x10
; Set the high address bytes
mov byte [IDT_Contents + 14], al
mov byte [IDT_Contents + 15], ah
; Set the code segment selector
mov word [IDT_Contents + 10], 0x8
; Must always be 0
mov byte [IDT_Contents + 12], 0x0
; Set the type and attributes: 0x8E =	   1111		0			00		1
;										Interrupt Gate	Always 0	DPL		Present
mov byte [IDT_Contents + 13], 0x8E

; Set the Int3 handler
mov dword eax, Debug_Int3Handler
mov byte [IDT_Contents + 24], al
mov byte [IDT_Contents + 25], ah
shr dword eax, 0x10
mov byte [IDT_Contents + 30], al
mov byte [IDT_Contents + 31], ah
mov word [IDT_Contents + 26], 0x8
mov byte [IDT_Contents + 28], 0x0
mov byte [IDT_Contents + 29], 0x8E

; Set remaining interrupt handlers

mov dword ebx, IDT_Contents

mov dword eax, Interrupt0Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8E
add ebx, 8

; Skip Int1 - Set above
add ebx, 8

mov dword eax, Interrupt2Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8E
add ebx, 8

; Skip Int3 - Set above
add ebx, 8
 
mov dword eax, Interrupt4Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8E
add ebx, 8
  
mov dword eax, Interrupt5Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8E
add ebx, 8
  
mov dword eax, Interrupt6Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8E
add ebx, 8
  
mov dword eax, Interrupt7Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8E
add ebx, 8
  
mov dword eax, Interrupt8Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8E
add ebx, 8
  
mov dword eax, Interrupt9Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8E
add ebx, 8
  
mov dword eax, Interrupt10Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8E
add ebx, 8
  
mov dword eax, Interrupt11Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8E
add ebx, 8
  
mov dword eax, Interrupt12Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8E
add ebx, 8

; Skip 13 - Triple Faults occur after every IRet!  
add ebx, 8
  
mov dword eax, Interrupt14Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8E
add ebx, 8

; Skip 15 - Reserved (i.e. empty)
add ebx, 8 

mov dword eax, Interrupt16Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8E
add ebx, 8

%macro CommonInterruptHandler_IDTMacro 2
    ; %1
	; Interrupt gate
    mov dword eax, CommonInterruptHandler%1
    mov byte [ebx], al
    mov byte [ebx+1], ah
    shr dword eax, 0x10
    mov byte [ebx+6], al
    mov byte [ebx+7], ah
    mov word [ebx+2], 0x8
    mov byte [ebx+4], 0x0
	; Use Interrupt gates not Trap gates!! Makes a massive
	; difference! If you use Trap gates, you'll get 
	; double faults as soon as you start using IRQs
	; in-combo with User Mode processes.
	;  And mark all of them as User/Kernel-mode accessible
    mov byte [ebx+5], (0x8E | (%2 << 5))
    add ebx, 8
%endmacro
%assign handlernum 17
%rep (32 - 17)
    CommonInterruptHandler_IDTMacro handlernum, USER_MODE_DPL
    %assign handlernum handlernum+1
%endrep
; IRQs should not be UM accessible
%rep 16
    CommonInterruptHandler_IDTMacro handlernum, KERNEL_MODE_DPL
    %assign handlernum handlernum+1
%endrep
%rep (256 - 48)
    CommonInterruptHandler_IDTMacro handlernum, USER_MODE_DPL
    %assign handlernum handlernum+1
%endrep

mov dword [IDT_Pointer + 2], IDT_Contents
mov dword eax, IDT_Pointer
lidt [eax]
; END - Create IDT
jmp SkipIDTHandlers

; BEGIN - Proper exception handlers (i.e. they use the exceptions mechanism)

Interrupt0Handler:
push dword [esp]
call method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_DivideByZeroException_NAMEEND__System_UInt32_

Interrupt4Handler:
call method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_OverflowException_NAMEEND___
 
Interrupt6Handler:
call method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_InvalidOpCodeException_NAMEEND___

Interrupt8Handler:
call method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_DoubleFaultException_NAMEEND__System_UInt32_System_UInt32_

Interrupt12Handler:
call method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_StackException_NAMEEND___

Interrupt14Handler:

DISABLE_INTERRUPTS

mov dword eax, CR2
push eax
call method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_PageFaultException_NAMEEND__System_UInt32_System_UInt32_System_UInt32_
add esp, 8

ENABLE_INTERRUPTS
IRetd

; END - Proper exception handlers 

; BEGIN - Message-only Interrupt Handlers
 
Interrupt2HandlerMsg db 11, 0, 0, 0, 073, 110, 116, 101, 114, 114, 117, 112, 116, 032, 050
Interrupt2Handler:
	DISABLE_INTERRUPTS
pushad
mov dword eax, Interrupt2HandlerMsg
jmp MessageOnlyInterruptHandler

Interrupt5HandlerMsg db 11, 0, 0, 0, 073, 110, 116, 101, 114, 114, 117, 112, 116, 032, 053
Interrupt5Handler:
	DISABLE_INTERRUPTS
pushad
mov dword eax, Interrupt5HandlerMsg
jmp MessageOnlyInterruptHandler

Interrupt7HandlerMsg db 11, 0, 0, 0, 073, 110, 116, 101, 114, 114, 117, 112, 116, 032, 055
Interrupt7Handler:
	DISABLE_INTERRUPTS
pushad
mov dword eax, Interrupt7HandlerMsg
jmp MessageOnlyInterruptHandler
  
Interrupt9HandlerMsg db 11, 0, 0, 0, 073, 110, 116, 101, 114, 114, 117, 112, 116, 032, 057
Interrupt9Handler:
	DISABLE_INTERRUPTS
pushad
mov dword eax, Interrupt9HandlerMsg
jmp MessageOnlyInterruptHandler
 
Interrupt10HandlerMsg db 12, 0, 0, 0, 073, 110, 116, 101, 114, 114, 117, 112, 116, 032, 049, 048
Interrupt10Handler:
	DISABLE_INTERRUPTS
pushad
mov dword eax, Interrupt10HandlerMsg
jmp MessageOnlyInterruptHandler
 
Interrupt11HandlerMsg db 12, 0, 0, 0, 073, 110, 116, 101, 114, 114, 117, 112, 116, 032, 049, 049
Interrupt11Handler:
	DISABLE_INTERRUPTS
pushad
mov dword eax, Interrupt11HandlerMsg
jmp MessageOnlyInterruptHandler
 
Interrupt16HandlerMsg db 12, 0, 0, 0, 073, 110, 116, 101, 114, 114, 117, 112, 116, 032, 049, 054
Interrupt16Handler:
	DISABLE_INTERRUPTS
pushad
mov dword eax, Interrupt16HandlerMsg
jmp MessageOnlyInterruptHandler
 
Interrupt124HandlerMsg db 13, 0, 0, 0, 073, 110, 116, 101, 114, 114, 117, 112, 116, 032, 049, 050, 052
Interrupt124Handler:
	DISABLE_INTERRUPTS
pushad
mov dword eax, Interrupt124HandlerMsg
jmp MessageOnlyInterruptHandler

MessageOnlyInterruptHandler:

push dword ebp
mov dword ebp, esp

push ds
mov eax, 0x10
mov ds, ax
mov es, ax
mov gs, ax
mov fs, ax

push dword eax
push dword 0x02
call method_System_Void_RETEND_Kernel_Boot_DECLEND_WriteDebugVideo_NAMEEND__System_String_System_UInt32_
add esp, 8

mov ecx, 0x0F0FFFFF
MessageOnlyInterruptHandler.delayLoop1:
	nop
loop MessageOnlyInterruptHandler.delayLoop1

pop eax
mov ds, ax
mov es, ax
mov gs, ax
mov fs, ax

pop dword ebp

popad
IRetd

; END - Message-only Interrupt Handlers

; BEGIN - Common interrupt handlers

%macro CommonInterruptHandlerMacro 1
CommonInterruptHandler%1:

	DISABLE_INTERRUPTS

	INTERRUPTS_STORE_STATE STORE_STATE_SKIP_NUM
	%assign STORE_STATE_SKIP_NUM STORE_STATE_SKIP_NUM+1

	push dword %1
    call method_System_Void_RETEND_Kernel_Interrupts_Interrupts_DECLEND_CommonISR_NAMEEND__System_UInt32_
    add esp, 4

	INTERRUPTS_RESTORE_STATE RESTORE_STATE_SKIP_NUM
	%assign RESTORE_STATE_SKIP_NUM RESTORE_STATE_SKIP_NUM+1
	
    IRetd
%endmacro
%assign handlernum2 17
%rep (256-17)
    CommonInterruptHandlerMacro handlernum2
    %assign handlernum2 handlernum2+1
%endrep

; END - Common interrupt handlers

; START - Debug interrupt handlers

EXTERN staticfield_System_Boolean_Kernel_Debug_Debugger_Enabled
EXTERN staticfield_System_Boolean_Kernel_Interrupts_Interrupts_insideCriticalHandler

Debug_Int1Handler:
	
	DISABLE_INTERRUPTS
	
	push eax
	mov dword eax, [staticfield_System_Boolean_Kernel_Debug_Debugger_Enabled]
	cmp eax, 0
	je Debug_Int1Handler_Skip
	mov dword eax, [staticfield_System_Boolean_Kernel_Interrupts_Interrupts_insideCriticalHandler]
	cmp eax, 1
	je Debug_Int1Handler_Skip
	pop eax
	
	INTERRUPTS_STORE_STATE 301

    call method_System_Void_RETEND_Kernel_Debug_Debugger_DECLEND_Int1_NAMEEND___
    
	INTERRUPTS_RESTORE_STATE 301
	
	jmp Debug_Int1Handler_End
	Debug_Int1Handler_Skip:
	pop eax
	Debug_Int1Handler_End:

IRetd

Debug_Int3Handler:
	
	DISABLE_INTERRUPTS
	
	push eax
	mov dword eax, [staticfield_System_Boolean_Kernel_Debug_Debugger_Enabled]
	cmp eax, 0
	je Debug_Int3Handler_Skip
	mov dword eax, [staticfield_System_Boolean_Kernel_Interrupts_Interrupts_insideCriticalHandler]
	cmp eax, 1
	je Debug_Int3Handler_Skip
	pop eax

	INTERRUPTS_STORE_STATE 303

    call method_System_Void_RETEND_Kernel_Debug_Debugger_DECLEND_Int3_NAMEEND___
	
	INTERRUPTS_RESTORE_STATE 303
    
	jmp Debug_Int3Handler_End
	Debug_Int3Handler_Skip:
	pop eax
	Debug_Int3Handler_End:

IRetd

DoNothing:
ret

; END - Debug interrupt handlers

SkipIDTHandlers:	
pic_remap:
; Remap IRQs 0-7    to    ISRs 32-39
; and   IRQs 8-15    to    ISRs 40-47

    ; Remap IRQ 0-15 to 32-47 (see http://wiki.osdev.org/PIC#Initialisation)
    ; Interrupt Vectors 0x20 for IRQ 0 to 7 and 0x28 for IRQ 8 to 15
    mov al, 0x11        ; INIT command
    out 0x20, al        ; send INIT to PIC1
    out 0xA0, al        ; send INIT to PIC2

    mov al, 0x20        ; PIC1 interrupts start at 0x20
    out 0x21, al        ; send the port to PIC1 DATA
    mov al, 0x28        ; PIC2 interrupts start at 0x28
    out 0xA1, al        ; send the port to PIC2 DATA

    mov al, 0x04        ; MASTER code
    out 0x21, al        ; set PIC1 as MASTER
    mov al, 0x02        ; SLAVE code
    out 0xA1, al        ; set PIC2 as SLAVE

    dec al              ; al is now 1. This is the x86 mode code for both 8259 PIC chips
    out 0x21, al        ; set PIC1
    out 0xA1, al        ; set PIC2
	
	mov ax, 0xFFFF		; Set interrupt mask to disable all interrupts
    out 0x21, al        ; Set mask of PIC1_DATA
    xchg al, ah
    out 0xA1, al        ; Set mask of PIC2_DATA

	sti					; Enable interrupts
	nop					; Required - STI takes effect after the next instruction runs

	
	; IDT initialised.
	mov byte [0xB8320], 0x49
	mov byte [0xB8322], 0x44
	mov byte [0xB8324], 0x54
	mov byte [0xB8326], 0x20
	mov byte [0xB8328], 0x69
	mov byte [0xB832A], 0x6e
	mov byte [0xB832C], 0x69
	mov byte [0xB832E], 0x74
	mov byte [0xB8330], 0x69
	mov byte [0xB8332], 0x61
	mov byte [0xB8334], 0x6c
	mov byte [0xB8336], 0x69
	mov byte [0xB8338], 0x73
	mov byte [0xB833A], 0x65
	mov byte [0xB833C], 0x64
	mov byte [0xB833E], 0x2e
	
	; mov ecx, 0x00F00000
	; .IDT_Loop1:
	; loop .IDT_Loop1

GLOBAL Kernel_Start_HandleNoMultiboot:function

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

	; mov ecx, 0x00F00000
	; .Main_Loop1:
	; loop .Main_Loop1


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
	jmp method_System_Void_Kernel_Boot_Reset__ ; For now this is our intended behaviour

; END - Main Entrypoint

GLOBAL method_System_Void_Kernel_Boot_Reset__:function
GLOBAL method_System_Void_RETEND_Kernel_Boot_DECLEND_Reset_NAMEEND___:function

; BEGIN - Reset
method_System_Void_Kernel_Boot_Reset__:
method_System_Void_RETEND_Kernel_Boot_DECLEND_Reset_NAMEEND___:
	cli ; Clear all interrupts so we aren't re-awoken
	hlt	; Halt the OS / CPU / etc.
	jmp method_System_Void_Kernel_Boot_Reset__ ; Just in case...
; END - Reset

GLOBAL method_System_Void_RETEND_Kernel_Boot_DECLEND_WriteDebugVideo_NAMEEND__System_String_System_UInt32_:function

; BEGIN - Write Debug Video
method_System_Void_RETEND_Kernel_Boot_DECLEND_WriteDebugVideo_NAMEEND__System_String_System_UInt32_:

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

.Reset_Loop1:
mov word [ebx], ax	; Move the empty character/colour to vid mem
add dword ebx, 2	; Move to next character space in vid mem
; Uses ecx - loops till ecx = 0 i.e. till all characters cleared
loop .Reset_Loop1


; Load string length
mov eax, [ebp+12]	 ; Load string address
mov dword ecx, [eax] ; String length is first dword of string

mov edx, [ebp+12]	; Load string address
add edx, 4			; Skip first dword because that is the length not a character

mov dword ebx, 0xB8000 ; Load vid mem base address

mov byte ah, [ebp+8] ; Load colour

.Reset_Loop2:
mov byte al, [edx]		; Get current character of string
mov word [ebx], ax		; Move character and colour into vid mem
add dword ebx, 2		; Move to next character space in vid mem
add dword edx, 1		; Move to next character in string
; Uses ecx - loops till ecx = 0 i.e. till all characters gone through
loop .Reset_Loop2



; Load string length
mov eax, [ebp+12]	 ; Load string address
mov dword ecx, [eax] ; String length is first dword of string

mov edx, [ebp+12]	; Load string address
add edx, 4			; Skip first dword because that is the length not a character

.Reset_Loop3:
mov byte al, [edx]		; Get current character of string
call WaitToWriteSerial
call WriteSerial
add dword edx, 1		; Move to next character in string
loop .Reset_Loop3

; MethodEnd
pop dword ebp

ret
; END - Write Debug Video

WaitToWriteSerial:
	push eax
	push ebx
	push ecx
	push edx

	mov dx, 0x3FD

	.NotEmpty:
	mov ax, [0xB8000]
	add ah, 1
	mov [0xB8000], ax

	in al, dx
	test al, 0x20
	jnz .NotEmpty	

	pop edx
	pop ecx
	pop ebx
	pop eax
ret

WriteSerial:
	push eax
	push edx

	mov dx, 0x3F8
	out dx, al

	pop edx
	pop eax
ret

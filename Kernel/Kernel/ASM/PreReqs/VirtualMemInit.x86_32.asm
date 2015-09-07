BITS 32

SECTION .text

KERNEL_VIRTUAL_BASE equ 0xC0000000					; 3GiB
KERNEL_PAGE_NUMBER equ (KERNEL_VIRTUAL_BASE >> 22)

EXTERN Page_Table1
EXTERN Page_Directory

GLOBAL File_VirtMemInit:function
File_VirtMemInit:

	; See comment at start of Main Entrypoint
	mov dword ecx, 0x0

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

; TODO - Unhack this identity mapping shizzle

lea eax, [Page_Table1 - KERNEL_VIRTUAL_BASE]
mov ebx, 7
mov ecx, (256 * 1024)
.Loop1:
mov [eax], ebx
add eax, 4
add ebx, 4096
loop .Loop1

lea eax, [Page_Table1 - KERNEL_VIRTUAL_BASE]
add eax, 0x300000 ; Moves pointer to page table for 3GiB mark ((0xC0000000 / (4096 * 1024)) * (1024*4))
mov ebx, 7
mov ecx, (256 * 1024)
.Loop2:
mov [eax], ebx
add eax, 4
add ebx, 4096
loop .Loop2


lea ebx, [Page_Table1 - KERNEL_VIRTUAL_BASE]
lea edx, [Page_Directory - KERNEL_VIRTUAL_BASE]
or ebx, 7
mov ecx, 1024
.Loop3:
mov [edx], ebx
add edx, 4
add ebx, 4096
loop .Loop3


; 2. Map virtual memory for virtual address execution
;		This means create pages such that:
; 
;		0xC0000000			->	0x00000000
;		0xC0100000			->	0x00100000
;		...
;		0xC-End of kernel	->	0x0-End of kernel

; The above step is hardcoded in code above.


; 3. Set page directory
;		This requires us to load the physical address of the page directory
;		then move it into cr3

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
	mov ecx, 0x00F00000
	.LoopDisp1:
	loop .LoopDisp1

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
	mov ecx, 0x00F00000
	.LoopDisp2:
	loop .LoopDisp2

; We are now working in the higher-half virtual address space :)

; END - Virtual Mem Init
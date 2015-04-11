BITS 32

SECTION .text
GLOBAL File_VirtMemInit:function
File_VirtMemInit:

EXTERN Page_Table1
EXTERN Page_Directory

KERNEL_VIRTUAL_BASE equ 0xC0000000					; 3GiB
KERNEL_PAGE_NUMBER equ (KERNEL_VIRTUAL_BASE >> 22)

; BEGIN - Virtual Mem Init

VirtualMemInit:
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

; mov dword eax, 0x5530
; mov word [0xB8000], ax
; mov dword eax, 0x6630
; mov word [0xB8002], ax
; mov dword ecx, 0xFFFFFFFF
; LoopXYZ0:
; loop LoopXYZ0

nop
nop
nop
nop

nop
nop
nop
nop

nop
nop
nop
nop

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

lea ecx, [StartInHigherHalf]

; 6. Jump to the virtual address
;		Jump to the virtual address of the label (that has just been loaded)

jmp ecx							; NOTE: Must be absolute jump!
 
StartInHigherHalf:
nop

; We are now working in the higher-half virtual address space :)

; END - Virtual Mem Init
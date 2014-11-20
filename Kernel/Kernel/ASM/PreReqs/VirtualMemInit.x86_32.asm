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
;		This only requires us to be able to execute the next ~50 instructions
;		so we essentially we only need to identity map one page - the one we 
;		are executing within!
;		This means create pages such that:
; 
;		Current IP -> Current IP
; 
;			(Where IP stands for Instruction Pointer)
; 
;		For the purposes of getting this shit working, we will also identity map the first 4MB
;		so that we can see video output etc.

; TODO - Unhack this identity mapping shizzle

lea eax, [Page_Table1 - KERNEL_VIRTUAL_BASE]
mov ebx, 7
mov ecx, (49 * 1024)
.Loop1:
mov [eax], ebx
add eax, 4
add ebx, 4096
loop .Loop1

lea eax, [Page_Table1 - KERNEL_VIRTUAL_BASE]
add eax, 0x300000
mov ebx, 7
mov ecx, (49 * 1024)
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

; The above step is hardcoded in the memory allocations.


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

; We are now working in the higher-half virtual address space :)

; END - Virtual Mem Init
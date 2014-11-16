section .text
; BEGIN - Multiboot Signature
MultibootSignature dd 464367618
MultibootFlags dd 3
MultibootChecksum dd -464367621
MultibootGraphicsRuntime_VbeModeInfoAddr dd 2147483647
MultibootGraphicsRuntime_VbeControlInfoAddr dd 2147483647
MultibootGraphicsRuntime_VbeMode dd 2147483647
MultiBootInfo_Memory_High dd 0
MultiBootInfo_Memory_Low dd 0


KERNEL_VIRTUAL_BASE equ 0xC0000000					; 3GiB
KERNEL_PAGE_NUMBER equ (KERNEL_VIRTUAL_BASE >> 22)

Kernel_MemStart:

global Before_Kernel_Stack
Before_Kernel_Stack: TIMES 65535 db 0
Kernel_Stack:


MultiBootInfo_Structure dd 0

; This is the GDT table pre-filled with the entries we want
_NATIVE_GDT_Contents:
db 0, 0, 0, 0, 0, 0, 0, 0			; Offset: 0  - Null selector - required 
db 255, 255, 0, 0, 0, 154, 207, 0	; Offset: 8  - Code selector - covers the entire 4GiB address range
db 255, 255, 0, 0, 0, 146, 207, 0	; Offset: 16 - Data selector - covers the entire 4GiB address range
; TSS set so that only kernel can perform task switching
db 0x67, 0, 0, 0, 0, 0x89, 0x10, 0	; Offset: 24 - TSS Selector - Pointer to the TSS 

;					   Size - Change iff adding/removing rows from GDT contents
_NATIVE_GDT_Pointer db 31, 0, 0, 0, 0, 0
global _NATIVE_IDT_Contents
_NATIVE_IDT_Contents: TIMES 2048 db 0
_NATIVE_IDT_Pointer db 15, 15, 0, 0, 0, 0

_NATIVE_TSS:
TIMES 104 db 0
TSS_POINTER equ (_NATIVE_TSS - KERNEL_VIRTUAL_BASE)

; END - Multiboot Signature
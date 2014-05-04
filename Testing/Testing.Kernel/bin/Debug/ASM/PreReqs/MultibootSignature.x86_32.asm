; BEGIN - Multiboot Signature
%ifndef ELF_COMPILATION
MultibootSignature dd 464367618
MultibootFlags dd 65539
MultibootChecksum dd -464433157
MultibootHeaderAddr dd MultibootSignature
MultibootLoadAddr dd MultibootSignature
MultibootLoadEndAddr dd _end_code
MultibootBSSEndAddr dd _end_code
MultibootEntryAddr dd Kernel_Start
%endif
%ifdef ELF_COMPILATION
MultibootSignature dd 464367618
MultibootFlags dd 3
MultibootChecksum dd -464367621
%endif
MultibootGraphicsRuntime_VbeModeInfoAddr dd 2147483647
MultibootGraphicsRuntime_VbeControlInfoAddr dd 2147483647
MultibootGraphicsRuntime_VbeMode dd 2147483647
MultiBootInfo_Memory_High dd 0
MultiBootInfo_Memory_Low dd 0

global Before_Kernel_Stack
Before_Kernel_Stack: TIMES 65535 db 0
Kernel_Stack:

MultiBootInfo_Structure dd 0

_NATIVE_GDT_Contents db 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 0, 0, 0, 154, 207, 0, 255, 255, 0, 0, 0, 146, 207, 0
_NATIVE_GDT_Pointer db 23, 0, 0, 0, 0, 0
global _NATIVE_IDT_Contents
_NATIVE_IDT_Contents: TIMES 2048 db 0
_NATIVE_IDT_Pointer db 0, 8, 0, 0, 0, 0

StringLiteral_FlingOSText dw 00, 16, 70, 108, 105, 110, 103, 32, 79, 83, 32, 66, 111, 111, 116, 101, 100, 33
; END - Multiboot Signature
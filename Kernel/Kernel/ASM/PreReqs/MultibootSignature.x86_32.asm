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
KERNEL_PAGE_NUMBER equ (KERNEL_VIRTUAL_BASE >> 22)  ; Page directory index of kernel's 4MiB PTE.

align 0x1000
BootPageDirectory:
    ; This page directory entry identity-maps the first 4MiB of the 32-bit physical address space.
    ; All bits are clear except the following:
    ; bit 7: PS The kernel page is 4MiB.
    ; bit 1: RW The kernel page is read/write.
    ; bit 0: P  The kernel page is present.
    ; This entry must be here -- otherwise the kernel will crash immediately after paging is
    ; enabled because it can't fetch the next instruction! It's ok to unmap this page later.
    dd 0x00000083
    times (KERNEL_PAGE_NUMBER - 1) dd 0                 ; Pages before kernel space.
    ; These page directory entries define identity mapping of 4MiB pages containing the entire kernel and everything after the kernel too.
	%macro BootPageDirectory_KernelVirtEntries 1
		dd %1
	%endmacro
	%assign addr 0x00000083
    %rep (1024 - KERNEL_PAGE_NUMBER - 1)
		BootPageDirectory_KernelVirtEntries addr
		%assign addr addr+0x400000
	%endrep


Kernel_MemStart:

global Before_Kernel_Stack
Before_Kernel_Stack: TIMES 65535 db 0
Kernel_Stack:


MultiBootInfo_Structure dd 0

_NATIVE_GDT_Contents db 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 0, 0, 0, 154, 207, 0, 255, 255, 0, 0, 0, 146, 207, 0
_NATIVE_GDT_Pointer db 23, 0, 0, 0, 0, 0
global _NATIVE_IDT_Contents
_NATIVE_IDT_Contents: TIMES 2048 db 0
_NATIVE_IDT_Pointer db 15, 15, 0, 0, 0, 0



; END - Multiboot Signature
BITS 32

SECTION .text

KERNEL_VIRTUAL_BASE equ 0xC0000000					; 3GiB
KERNEL_PAGE_NUMBER equ (KERNEL_VIRTUAL_BASE >> 22)

GLOBAL method_System_UInt32_RETEND_Kernel_Hardware_VirtMem_x86_DECLEND_GetCR3_NAMEEND___:function
GLOBAL method_System_UInt32_RETEND_Kernel_Hardware_VirtMem_x86_DECLEND_GetKernelVirtToPhysOffset_NAMEEND___:function
GLOBAL method_System_UInt32__RETEND_Kernel_Hardware_VirtMem_x86_DECLEND_GetPageDirectoryPtr_NAMEEND___:function
GLOBAL method_System_UInt32__RETEND_Kernel_Hardware_VirtMem_x86_DECLEND_GetFirstPageTablePtr_NAMEEND___:function
GLOBAL method_System_UInt32__RETEND_Kernel_Hardware_VirtMem_x86_DECLEND_GetKernelMemStartPtr_NAMEEND___:function
GLOBAL method_System_UInt32__RETEND_Kernel_Hardware_VirtMem_x86_DECLEND_GetKernelMemEndPtr_NAMEEND___:function
GLOBAL method_System_Void_RETEND_Kernel_Hardware_VirtMem_x86_DECLEND_InvalidatePTE_NAMEEND__System_UInt32_:function

EXTERN Kernel_MemStart

; BEGIN - x86 Virt Mem

method_System_UInt32_RETEND_Kernel_Hardware_VirtMem_x86_DECLEND_GetCR3_NAMEEND___:

push dword ebp
mov dword ebp, esp

mov dword eax, cr3
mov dword [ebp+8], eax

pop dword ebp
ret


method_System_UInt32_RETEND_Kernel_Hardware_VirtMem_x86_DECLEND_GetKernelVirtToPhysOffset_NAMEEND___:

push dword ebp
mov dword ebp, esp

mov dword [ebp+8], KERNEL_VIRTUAL_BASE

pop dword ebp

ret


method_System_UInt32__RETEND_Kernel_Hardware_VirtMem_x86_DECLEND_GetPageDirectoryPtr_NAMEEND___:

push dword ebp
mov dword ebp, esp

mov dword [ebp+8], Page_Directory

pop dword ebp

ret


method_System_UInt32__RETEND_Kernel_Hardware_VirtMem_x86_DECLEND_GetFirstPageTablePtr_NAMEEND___:

push dword ebp
mov dword ebp, esp

mov dword [ebp+8], Page_Table1

pop dword ebp

ret


method_System_UInt32__RETEND_Kernel_Hardware_VirtMem_x86_DECLEND_GetKernelMemStartPtr_NAMEEND___:

push dword ebp
mov dword ebp, esp

mov dword [ebp+8], Kernel_MemStart

pop dword ebp

ret


method_System_UInt32__RETEND_Kernel_Hardware_VirtMem_x86_DECLEND_GetKernelMemEndPtr_NAMEEND___:

push dword ebp
mov dword ebp, esp

mov dword [ebp+8], Kernel_MemEnd

pop dword ebp

ret


method_System_Void_RETEND_Kernel_Hardware_VirtMem_x86_DECLEND_InvalidatePTE_NAMEEND__System_UInt32_:

push dword ebp
mov dword ebp, esp

mov dword eax, [ebp+8]
invlpg [eax]

pop dword ebp

ret

SECTION .data

GLOBAL Page_Directory:data
GLOBAL Page_Table1:data
GLOBAL Kernel_MemEnd:data

align 4096
Page_Directory: TIMES 1024 dd 0
Page_Table1: TIMES 1048576 dd 0

Kernel_MemEnd:

; END - x86 Virt Mem
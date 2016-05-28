BITS 32

SECTION .text

KERNEL_VIRTUAL_BASE equ 0xC0000000					; 3GiB
KERNEL_PAGE_NUMBER equ (KERNEL_VIRTUAL_BASE >> 22)

GLOBAL method_System_UInt32_RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetCR3_NAMEEND___:function
GLOBAL method_System_UInt32_RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetKernelVirtToPhysOffset_NAMEEND___:function
GLOBAL method_System_UInt32__RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetPageDirectoryPtr_NAMEEND___:function
GLOBAL method_System_UInt32__RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetFirstPageTablePtr_NAMEEND___:function
GLOBAL method_System_UInt32__RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetKernelMemStartPtr_NAMEEND___:function
GLOBAL method_System_UInt32__RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetKernelMemEndPtr_NAMEEND___:function
GLOBAL method_System_Void_RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_InvalidatePTE_NAMEEND__System_UInt32_:function

EXTERN Kernel_MemStart
EXTERN Kernel_MemEnd

; BEGIN - x86 Virt Mem

method_System_UInt32_RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetCR3_NAMEEND___:

push dword ebp
mov dword ebp, esp

mov dword eax, cr3
mov dword [ebp+8], eax

pop dword ebp
ret


method_System_UInt32_RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetKernelVirtToPhysOffset_NAMEEND___:

push dword ebp
mov dword ebp, esp

mov dword [ebp+8], KERNEL_VIRTUAL_BASE

pop dword ebp

ret


method_System_UInt32__RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetPageDirectoryPtr_NAMEEND___:

push dword ebp
mov dword ebp, esp

mov dword [ebp+8], Page_Directory

pop dword ebp

ret


method_System_UInt32__RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetFirstPageTablePtr_NAMEEND___:

push dword ebp
mov dword ebp, esp

mov dword [ebp+8], Page_Table1

pop dword ebp

ret


method_System_UInt32__RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetKernelMemStartPtr_NAMEEND___:

push dword ebp
mov dword ebp, esp

mov dword [ebp+8], Kernel_MemStart

pop dword ebp

ret


method_System_UInt32__RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetKernelMemEndPtr_NAMEEND___:

push dword ebp
mov dword ebp, esp

mov dword [ebp+8], Kernel_MemEnd

pop dword ebp

ret


method_System_Void_RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_InvalidatePTE_NAMEEND__System_UInt32_:

push dword ebp
mov dword ebp, esp

mov dword eax, [ebp+8]
invlpg [eax]

pop dword ebp

ret

EXTERN StaticFields_Start
GLOBAL method_System_UInt32__RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetStaticFields_StartPtr_NAMEEND___:function
method_System_UInt32__RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetStaticFields_StartPtr_NAMEEND___:

push dword ebp
mov dword ebp, esp

mov dword [ebp+8], StaticFields_Start

pop dword ebp

ret

EXTERN __bss_end
GLOBAL method_System_UInt32__RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetStaticFields_EndPtr_NAMEEND___:function
method_System_UInt32__RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetStaticFields_EndPtr_NAMEEND___:

push dword ebp
mov dword ebp, esp

mov dword [ebp+8], __bss_end

pop dword ebp

ret




EXTERN IsolatedKernel_BSS
GLOBAL method_System_UInt32__RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetIsolatedKernelPtr_NAMEEND___:function
method_System_UInt32__RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetIsolatedKernelPtr_NAMEEND___:

push dword ebp
mov dword ebp, esp

mov dword [ebp+8], IsolatedKernel_BSS

pop dword ebp

ret


EXTERN IsolatedKernel_Hardware_Multiprocessing_BSS
GLOBAL method_System_UInt32__RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetIsolatedKernel_Hardware_MultiprocessingPtr_NAMEEND___:function
method_System_UInt32__RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetIsolatedKernel_Hardware_MultiprocessingPtr_NAMEEND___:

push dword ebp
mov dword ebp, esp

mov dword [ebp+8], IsolatedKernel_Hardware_Multiprocessing_BSS

pop dword ebp

ret



EXTERN IsolatedKernel_Hardware_Devices_BSS
GLOBAL method_System_UInt32__RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetIsolatedKernel_Hardware_DevicesPtr_NAMEEND___:function
method_System_UInt32__RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetIsolatedKernel_Hardware_DevicesPtr_NAMEEND___:

push dword ebp
mov dword ebp, esp

mov dword [ebp+8], IsolatedKernel_Hardware_Devices_BSS

pop dword ebp

ret


EXTERN IsolatedKernel_Hardware_VirtualMemory_BSS
GLOBAL method_System_UInt32__RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetIsolatedKernel_Hardware_VirtualMemoryPtr_NAMEEND___:function
method_System_UInt32__RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetIsolatedKernel_Hardware_VirtualMemoryPtr_NAMEEND___:

push dword ebp
mov dword ebp, esp

mov dword [ebp+8], IsolatedKernel_Hardware_VirtualMemory_BSS

pop dword ebp

ret


EXTERN IsolatedKernel_FOS_System_BSS
GLOBAL method_System_UInt32__RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetIsolatedKernel_FOS_SystemPtr_NAMEEND___:function
method_System_UInt32__RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetIsolatedKernel_FOS_SystemPtr_NAMEEND___:

push dword ebp
mov dword ebp, esp

mov dword [ebp+8], IsolatedKernel_FOS_System_BSS

pop dword ebp

ret



EXTERN __bss_start
GLOBAL method_System_UInt32__RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetBSS_StartPtr_NAMEEND___:function
method_System_UInt32__RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetBSS_StartPtr_NAMEEND___:

push dword ebp
mov dword ebp, esp

mov dword [ebp+8], __bss_start

pop dword ebp

ret

EXTERN __bss_end
GLOBAL method_System_UInt32__RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetBSS_EndPtr_NAMEEND___:function
method_System_UInt32__RETEND_Kernel_Hardware_VirtualMemory_x86_DECLEND_GetBSS_EndPtr_NAMEEND___:

push dword ebp
mov dword ebp, esp

mov dword [ebp+8], __bss_end

pop dword ebp

ret


SECTION .bss

GLOBAL Page_Directory:data
GLOBAL Page_Table1:data

Page_Directory: resb 4096
Page_Table1: resb 4194304
				 ; 4194304 = 1024 * 1024 * 4 = Page tables to cover 4GiB
				 
; END - x86 Virt Mem

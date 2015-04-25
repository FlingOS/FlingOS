BITS 32

SECTION .text

GLOBAL method_System_UInt32__RETEND_Kernel_FOS_System_Heap_DECLEND_GetFixedHeapPtr_NAMEEND___:function
GLOBAL method_System_UInt32_RETEND_Kernel_FOS_System_Heap_DECLEND_GetFixedHeapSize_NAMEEND___:function

GLOBAL KernelFixedHeap_Start:data
GLOBAL KernelFixedHeap_End:data

method_System_UInt32__RETEND_Kernel_FOS_System_Heap_DECLEND_GetFixedHeapPtr_NAMEEND___:

push ebp
mov ebp, esp

mov eax, KernelFixedHeap_Start
mov dword [ebp+8], eax

pop ebp

ret


method_System_UInt32_RETEND_Kernel_FOS_System_Heap_DECLEND_GetFixedHeapSize_NAMEEND___:

push ebp
mov ebp, esp

mov dword [ebp+8], KernelFixedHeap_End-KernelFixedHeap_Start

pop ebp

ret

SECTION .bss
; 104 857 600 bytes = 16MiB (using proper powers of 2 not the powers of 10 crap...)
KernelFixedHeap_Start: resb 104857600
KernelFixedHeap_End:
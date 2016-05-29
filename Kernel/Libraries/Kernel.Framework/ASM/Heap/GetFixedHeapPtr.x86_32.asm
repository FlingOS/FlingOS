BITS 32

SECTION .text

GLOBAL method_System_UInt32__RETEND_Kernel_Framework_Heap_Kernel_DECLEND_GetFixedHeapPtr_NAMEEND___:function
GLOBAL method_System_UInt32_RETEND_Kernel_Framework_Heap_Kernel_DECLEND_GetFixedHeapSize_NAMEEND___:function

GLOBAL KernelFixedHeap_Start:data
GLOBAL KernelFixedHeap_End:data

method_System_UInt32__RETEND_Kernel_Framework_Heap_Kernel_DECLEND_GetFixedHeapPtr_NAMEEND___:

push ebp
mov ebp, esp

mov eax, KernelFixedHeap_Start
mov dword [ebp+8], eax

pop ebp

ret


method_System_UInt32_RETEND_Kernel_Framework_Heap_Kernel_DECLEND_GetFixedHeapSize_NAMEEND___:

push ebp
mov ebp, esp

mov dword [ebp+8], KernelFixedHeap_End-KernelFixedHeap_Start

pop ebp

ret

SECTION .bss

; (524288 * 2) bytes = 0.5MiB * 2 = 1MiB
KernelFixedHeap_Start: resb (524288 * 2)
KernelFixedHeap_End:

GLOBAL StaticFields_Start:data
StaticFields_Start:

global KernelFixedHeap_Start
; 524288 bytes = 0.5MB (using proper powers of 2 not the powers of 10 crap...)
KernelFixedHeap_Start: TIMES 524288 db 0
KernelFixedHeap_End:

method_System_UInt32__RETEND_Kernel_Heap_DECLEND_GetFixedHeapPtr_NAMEEND___:

push ebp
mov ebp, esp

mov eax, KernelFixedHeap_Start
mov dword [ebp+8], eax

pop ebp

ret


method_System_UInt32_RETEND_Kernel_Heap_DECLEND_GetFixedHeapSize_NAMEEND___:

push ebp
mov ebp, esp

mov dword [ebp+8], KernelFixedHeap_End-KernelFixedHeap_Start

pop ebp

ret
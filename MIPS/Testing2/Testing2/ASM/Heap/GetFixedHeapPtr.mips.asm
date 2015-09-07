.global method_System_UInt32__RETEND_Testing2_Heap_DECLEND_GetFixedHeapPtr_NAMEEND___
.global method_System_UInt32_RETEND_Testing2_Heap_DECLEND_GetFixedHeapSize_NAMEEND___

.global KernelFixedHeap_Start
.global KernelFixedHeap_End

.text

.ent method_System_UInt32__RETEND_Testing2_Heap_DECLEND_GetFixedHeapPtr_NAMEEND___
method_System_UInt32__RETEND_Testing2_Heap_DECLEND_GetFixedHeapPtr_NAMEEND___:
la $t0, KernelFixedHeap_Start
sw $t0, 0($sp)
j $ra
.end method_System_UInt32__RETEND_Testing2_Heap_DECLEND_GetFixedHeapPtr_NAMEEND___


.ent method_System_UInt32_RETEND_Testing2_Heap_DECLEND_GetFixedHeapSize_NAMEEND___
method_System_UInt32_RETEND_Testing2_Heap_DECLEND_GetFixedHeapSize_NAMEEND___:
la $t0, KernelFixedHeap_Start
la $t1, KernelFixedHeap_End
subu $t1, $t1, $t0
sw $t1, 0($sp)
j $ra
.end method_System_UInt32_RETEND_Testing2_Heap_DECLEND_GetFixedHeapSize_NAMEEND___

.bss
/* 104 857 600 bytes = 100MiB (using proper powers of 2 not the powers of 10 crap...) */
KernelFixedHeap_Start: 
.space 104857600
KernelFixedHeap_End:

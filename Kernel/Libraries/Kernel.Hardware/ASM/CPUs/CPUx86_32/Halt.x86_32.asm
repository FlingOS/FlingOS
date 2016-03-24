BITS 32

SECTION .text

GLOBAL method_System_Void_RETEND_Kernel_Hardware_CPUs_CPUx86_32_DECLEND_Halt_NAMEEND___:function

method_System_Void_RETEND_Kernel_Hardware_CPUs_CPUx86_32_DECLEND_Halt_NAMEEND___:
;TODO Wouldn't this be better as a timer function rather than nops as the number of nops may change according to the speed of the CPU?
; Nops allow time for interrupts to occur
nop
nop
nop
nop
nop
nop
nop
nop
nop
nop
hlt
nop
nop
nop
nop
nop
nop
nop
nop
nop
nop
ret
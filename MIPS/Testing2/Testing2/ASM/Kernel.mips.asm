.extern %KERNEL_MAIN_METHOD%
.extern %KERNEL_CALL_STATIC_CONSTRUCTORS_METHOD%

.globl Kernel_Start
.globl GetEIP

.text

.set noreorder

.ent Kernel_Start
Kernel_Start:
	nop

	/* Set up a stack */
	li $sp, 0x8f800000 
	
	jal %KERNEL_CALL_STATIC_CONSTRUCTORS_METHOD%
	nop

	/*
	jal MainInterruptHandler
	nop
	*/

	jal %KERNEL_MAIN_METHOD%
	nop	
	

	j $ra
	.end Kernel_Start

GetEIP:
	addi $sp, $sp, -4
	sw $ra, 0($sp)
	j $ra

	
.global method_System_Void_RETEND_Testing2_Kernel_DECLEND_EnableInterrupts_NAMEEND___
.ent method_System_Void_RETEND_Testing2_Kernel_DECLEND_EnableInterrupts_NAMEEND___
method_System_Void_RETEND_Testing2_Kernel_DECLEND_EnableInterrupts_NAMEEND___:
	nop
	
	li      $t0, (0xf0000000 | 0xff00 | 0x1) /* CP0_STATUS_CU_ALL | CP0_STATUS_IM_ALL | CP0_STATUS_IE */
	mtc0    $t0, $12 /* CP0_STATUS */
	nop

	li      $t0, 0x800000 /* CP0_CAUSE_INITIALISER */
	mtc0    $t0, $13 /* CP0_CAUSE */
	nop

	/* Enable caching in kseg0 */
	li      $t0, 3 /* CACHE_MODE_CACHABLE_NONCOHERENT */
	mtc0    $t0, $16 /* CP0_CONFIG */
	nop
		
	j $ra
	.end method_System_Void_RETEND_Testing2_Kernel_DECLEND_EnableInterrupts_NAMEEND___
	

.ent ExceptionHandler
ExceptionHandler:
	nop
	nop
	nop
	la $t0, method_System_Void_RETEND_Testing2_Kernel_DECLEND_MainInterruptHandler_NAMEEND___
	jr $t0
	nop
	eret
	ExceptionHandler_End:
	nop
	.end ExceptionHandler
	
.ent IRQHandler
IRQHandler:
	nop
	nop
	nop
	la $t0, method_System_Void_RETEND_Testing2_Kernel_DECLEND_MainInterruptHandler_NAMEEND___
	jr $t0
	nop
	eret
	IRQHandler_End:
	nop
	.end IRQHandler

.global method_System_Void_RETEND_Testing2_Kernel_DECLEND_MainInterruptHandler_NAMEEND___
.extern method_System_Void_RETEND_Testing2_LED_DECLEND_Blue_NAMEEND___
.extern method_System_Void_RETEND_Testing2_LED_DECLEND_Red_NAMEEND___
.ent method_System_Void_RETEND_Testing2_Kernel_DECLEND_MainInterruptHandler_NAMEEND___
method_System_Void_RETEND_Testing2_Kernel_DECLEND_MainInterruptHandler_NAMEEND___:
	
	jal method_System_Void_RETEND_Testing2_LED_DECLEND_Blue_NAMEEND___
	nop
	jal method_System_Void_RETEND_Testing2_Kernel_DECLEND_DelayLong_NAMEEND___
	nop

	jal method_System_Void_RETEND_Testing2_LED_DECLEND_Red_NAMEEND___
	nop
	jal method_System_Void_RETEND_Testing2_Kernel_DECLEND_DelayLong_NAMEEND___
	nop
	
	j method_System_Void_RETEND_Testing2_Kernel_DECLEND_MainInterruptHandler_NAMEEND___
	nop
	.end method_System_Void_RETEND_Testing2_Kernel_DECLEND_MainInterruptHandler_NAMEEND___
	
.global method_System_Void_RETEND_Testing2_Kernel_DECLEND_DoSyscall_NAMEEND___
.ent method_System_Void_RETEND_Testing2_Kernel_DECLEND_DoSyscall_NAMEEND___
method_System_Void_RETEND_Testing2_Kernel_DECLEND_DoSyscall_NAMEEND___:
	
	nop
	syscall
	nop

	j $ra
	nop
	.end method_System_Void_RETEND_Testing2_Kernel_DECLEND_DoSyscall_NAMEEND___

.global method_System_Byte__RETEND_Testing2_Kernel_DECLEND_GetExceptionHandlerStart_NAMEEND___
.ent method_System_Byte__RETEND_Testing2_Kernel_DECLEND_GetExceptionHandlerStart_NAMEEND___
method_System_Byte__RETEND_Testing2_Kernel_DECLEND_GetExceptionHandlerStart_NAMEEND___:
	la $t0, ExceptionHandler
	sw $t0, 0($sp)
	j $ra
	nop
	.end method_System_Byte__RETEND_Testing2_Kernel_DECLEND_GetExceptionHandlerStart_NAMEEND___

.global method_System_Byte__RETEND_Testing2_Kernel_DECLEND_GetExceptionHandlerEnd_NAMEEND___
.ent method_System_Byte__RETEND_Testing2_Kernel_DECLEND_GetExceptionHandlerEnd_NAMEEND___
method_System_Byte__RETEND_Testing2_Kernel_DECLEND_GetExceptionHandlerEnd_NAMEEND___:
	la $t0, ExceptionHandler_End
	sw $t0, 0($sp)
	j $ra
	nop
	.end method_System_Byte__RETEND_Testing2_Kernel_DECLEND_GetExceptionHandlerEnd_NAMEEND___
	
.global method_System_Byte__RETEND_Testing2_Kernel_DECLEND_GetIRQHandlerStart_NAMEEND___
.ent method_System_Byte__RETEND_Testing2_Kernel_DECLEND_GetIRQHandlerStart_NAMEEND___
method_System_Byte__RETEND_Testing2_Kernel_DECLEND_GetIRQHandlerStart_NAMEEND___:
	la $t0, IRQHandler
	sw $t0, 0($sp)
	j $ra
	nop
	.end method_System_Byte__RETEND_Testing2_Kernel_DECLEND_GetIRQHandlerStart_NAMEEND___
	
.global method_System_Byte__RETEND_Testing2_Kernel_DECLEND_GetIRQHandlerEnd_NAMEEND___
.ent method_System_Byte__RETEND_Testing2_Kernel_DECLEND_GetIRQHandlerEnd_NAMEEND___
method_System_Byte__RETEND_Testing2_Kernel_DECLEND_GetIRQHandlerEnd_NAMEEND___:
	la $t0, IRQHandler_End
	sw $t0, 0($sp)
	j $ra
	nop
	.end method_System_Byte__RETEND_Testing2_Kernel_DECLEND_GetIRQHandlerEnd_NAMEEND___

.set reorder

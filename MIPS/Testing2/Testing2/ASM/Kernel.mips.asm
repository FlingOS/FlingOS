.extern %KERNEL_MAIN_METHOD%
.extern %KERNEL_CALL_STATIC_CONSTRUCTORS_METHOD%

.text
.set noreorder

.globl Kernel_Start
.ent Kernel_Start
Kernel_Start:
	nop
	
	/* Enable caching in kseg0 */
	li      $t0, 3 /* CACHE_MODE_CACHABLE_NONCOHERENT */
	mtc0    $t0, $16 /* CP0_CONFIG */
	nop

	/* Set up a stack */
	li $sp, 0x8f800000 
	
	jal %KERNEL_CALL_STATIC_CONSTRUCTORS_METHOD%
	nop
	
	jal %KERNEL_MAIN_METHOD%
	nop	
	
	j $ra
	.end Kernel_Start

.globl GetEIP
.ent GetEIP
GetEIP:
	addi $sp, $sp, -4
	sw $ra, 0($sp)
	j $ra
	.end GetEIP

	
.set noat
ExceptionHandler:
	eret
	ExceptionHandler_End:
	nop
	
IRQHandler:
	sw $at, -4($sp)
	sw $v0, -8($sp)
	sw $v1, -12($sp)
	sw $a0, -16($sp)
	sw $a1, -20($sp)
	sw $a2, -24($sp)
	sw $a3, -28($sp)
	sw $t0, -32($sp)
	sw $t1, -36($sp)
	sw $t2, -40($sp)
	sw $t3, -44($sp)
	sw $t4, -48($sp)
	sw $t5, -52($sp)
	sw $t6, -56($sp)
	sw $t7, -60($sp)
	sw $s0, -64($sp)
	sw $s1, -68($sp)
	sw $s2, -72($sp)
	sw $s3, -76($sp)
	sw $s4, -80($sp)
	sw $s5, -84($sp)
	sw $s6, -88($sp)
	sw $s7, -92($sp)
	sw $t8, -96($sp)
	sw $t9, -100($sp)
	sw $gp, -104($sp)
	sw $fp, -108($sp)
	sw $ra, -112($sp)

	addi $sp, $sp, -112

	jal method_System_Void_RETEND_Testing2_Kernel_DECLEND_MainInterruptHandler_NAMEEND___
	nop

	addi $sp, $sp, 112
	lw $at, -4($sp)
	lw $v0, -8($sp)
	lw $v1, -12($sp)
	lw $a0, -16($sp)
	lw $a1, -20($sp)
	lw $a2, -24($sp)
	lw $a3, -28($sp)
	lw $t0, -32($sp)
	lw $t1, -36($sp)
	lw $t2, -40($sp)
	lw $t3, -44($sp)
	lw $t4, -48($sp)
	lw $t5, -52($sp)
	lw $t6, -56($sp)
	lw $t7, -60($sp)
	lw $s0, -64($sp)
	lw $s1, -68($sp)
	lw $s2, -72($sp)
	lw $s3, -76($sp)
	lw $s4, -80($sp)
	lw $s5, -84($sp)
	lw $s6, -88($sp)
	lw $s7, -92($sp)
	lw $t8, -96($sp)
	lw $t9, -100($sp)
	lw $gp, -104($sp)
	lw $fp, -108($sp)
	lw $ra, -112($sp)
	eret
	IRQHandler_End:
	nop		
.set at

	
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
			
	j $ra
	.end method_System_Void_RETEND_Testing2_Kernel_DECLEND_EnableInterrupts_NAMEEND___


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

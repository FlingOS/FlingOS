.extern %KERNEL_MAIN_METHOD%
.extern %KERNEL_CALL_STATIC_CONSTRUCTORS_METHOD%

.globl Kernel_Start
.globl GetEIP

.ent Kernel_Start
.text

Kernel_Start:
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

	/* Set up a stack */
	li $sp, 0x8f800000 
		
	jal %KERNEL_CALL_STATIC_CONSTRUCTORS_METHOD%
	nop

	jal %KERNEL_MAIN_METHOD%
	nop	

	.end Kernel_Start

GetEIP:
	addi $sp, $sp, -4
	sw $ra, 0($sp)
	j $ra

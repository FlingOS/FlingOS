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

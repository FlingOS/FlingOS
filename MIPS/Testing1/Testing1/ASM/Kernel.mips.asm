.extern %KERNEL_MAIN_METHOD%
.extern %KERNEL_CALL_STATIC_CONSTRUCTORS_METHOD%

.globl Kernel_Start
.globl GetEIP

.ent Kernel_Start
.text

Kernel_Start:
	/* Set up a stack */
	li $sp, 0xa9000000 
		
	jal %KERNEL_CALL_STATIC_CONSTRUCTORS_METHOD%
	nop

	jal %KERNEL_MAIN_METHOD%
	nop	

.InfiniteLoop2:
	
	li	$3,32768
	li	$2,-1342111744
	sw	$3,1352($2)
	
	jal MyFunction
	nop

	li	$3,32768
	li	$2,-1342111744
	sw	$3,1348($2)

	jal MyFunction
	nop

	j .InfiniteLoop2

	.end Kernel_Start

MyFunction:
	addiu	$sp, $sp, -8	# Subtract space for storing args (up to first four only, if any), locals (if any), frame pointer and return address (if necessary)
	sw		$fp, 8($sp)		# Store frame pointer at SP+12 
	move	$fp, $sp		# Set frame pointer to stack pointer

	li		$t0, 100000
	
	.Loop:
	nop
	nop
	nop
	nop
	nop
	addiu	$t0, $t0, -1
	nop
	nop
	nop
	nop
	nop
	bgt		$t0, $0, .Loop

	move	$sp, $fp		# Reset stack pointer to frame pointer
	lw		$fp, 8($sp)		# Set frame pointer to old value
	addiu	$sp, $sp, 8	# Add space that was used for args, locals and frame pointer
	j		$31					# Return to return address

GetEIP:
	addi $sp, $sp, -4
	sw $ra, 0($sp)
	j $ra

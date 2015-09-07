.text
.set noreorder

.globl GetEIP
.ent GetEIP
GetEIP:
	addi $sp, $sp, -4
	sw $ra, 0($sp)
	j $ra
	.end GetEIP

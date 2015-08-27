.global method_System_Void_RETEND_Testing2_ExceptionMethods_DECLEND_ShiftStack_NAMEEND__System_Byte__System_UInt32_

.text

.ent method_System_Void_RETEND_Testing2_ExceptionMethods_DECLEND_ShiftStack_NAMEEND__System_Byte__System_UInt32_
method_System_Void_RETEND_Testing2_ExceptionMethods_DECLEND_ShiftStack_NAMEEND__System_Byte__System_UInt32_:

/* Load distance */
lw $t0, 0($sp)

/* Load current (i.e. start) pointer */
lw $t1, 4($sp)

.Loop:

/* Load value to copy */
lw $t2, 0($t1)

/* Load / calc pointer to copy to */
move $t3, $t1
add $t3, $t3, $t0
sw $t2, 0($t3)

/* Shift to next dword */
addi $t1, -4

/* Is current pointer == end pointer
 *		i.e. Is ebx == esp
 */
bne $t1, $sp, .Loop

j $ra
.end method_System_Void_RETEND_Testing2_ExceptionMethods_DECLEND_ShiftStack_NAMEEND__System_Byte__System_UInt32_

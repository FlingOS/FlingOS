.global method_System_Byte__RETEND_Testing2_ExceptionMethods_DECLEND_get_StackPointer_NAMEEND___
.global method_System_Void_RETEND_Testing2_ExceptionMethods_DECLEND_set_StackPointer_NAMEEND__System_Byte__

.text

.ent method_System_Byte__RETEND_Testing2_ExceptionMethods_DECLEND_get_StackPointer_NAMEEND___
method_System_Byte__RETEND_Testing2_ExceptionMethods_DECLEND_get_StackPointer_NAMEEND___:
addi $sp, $sp, -4
sw $fp, 0($sp)
move $fp, $sp

move $t0, $sp
addi $t0, $t0, 8
sw $t0, 4($fp)

lw $fp, 0($sp)
addi $sp, $sp, 4
j $ra
.end method_System_Byte__RETEND_Testing2_ExceptionMethods_DECLEND_get_StackPointer_NAMEEND___

.ent method_System_Void_RETEND_Testing2_ExceptionMethods_DECLEND_set_StackPointer_NAMEEND__System_Byte__
method_System_Void_RETEND_Testing2_ExceptionMethods_DECLEND_set_StackPointer_NAMEEND__System_Byte__:
lw $sp, 0($sp)
/* Handles the "addi $sp, $sp, 4" after return */
addi $sp, $sp, -4
j $ra
.end method_System_Void_RETEND_Testing2_ExceptionMethods_DECLEND_set_StackPointer_NAMEEND__System_Byte__

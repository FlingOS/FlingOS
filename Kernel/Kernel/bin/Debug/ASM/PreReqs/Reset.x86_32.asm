; BEGIN - Reset
method_System_Void_Kernel_PreReqs_Reset__:
method_System_Void_RETEND_Kernel_PreReqs_DECLEND_Reset_NAMEEND___:
	cli ; Clear all interrupts so we aren't re-awoken
	hlt	; Halt the OS / CPU / etc.
	jmp method_System_Void_Kernel_PreReqs_Reset__ ; Just in case...
; END - Reset
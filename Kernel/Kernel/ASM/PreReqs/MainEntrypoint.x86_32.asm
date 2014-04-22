; BEGIN - Main Entrypoint
call __MAIN_ENTRYPOINT__ ; Call our main entry point 
						 ; - not strictly necessary but good for setting up esp etc.
		
__MAIN_ENTRYPOINT__:

	push dword ebp
	mov dword ebp, esp
	
	call %KERNEL_CALL_STATIC_CONSTRUCTORS_METHOD% ; Call the static constructors - this is a macro used by the kernel compiler.
	call %KERNEL_MAIN_METHOD% ; Call our main method - this is a macro used by the kernel compiler.
	
	; We shouldn't ever get to this point! But just in case we do...
	jmp method_System_Void_Kernel_PreReqs_Reset__ ; For now this is our intended behaviour

	pop dword ebp

	ret 0x0
; END - Main Entrypoint
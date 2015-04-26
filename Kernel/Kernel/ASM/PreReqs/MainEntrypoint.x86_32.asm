BITS 32

SECTION .text
GLOBAL File_MainEntryPoint:function
File_MainEntryPoint:


; Hmm... something has to go before the call op otherwise the OS crashes at boot.
;		It can be min. ~12 NOPs or this mov or probably many other things...
;	This is the same phenomenon observed at the start of VirtualMemInit
mov dword ecx, 0x0

; BEGIN - Main Entrypoint
call __MAIN_ENTRYPOINT__ ; Call our main entry point 

EXTERN %KERNEL_CALL_STATIC_CONSTRUCTORS_METHOD%
EXTERN %KERNEL_MAIN_METHOD%
EXTERN method_System_Void_Kernel_PreReqs_Reset__

__MAIN_ENTRYPOINT__:
	push dword ebp
	mov dword ebp, esp
	
	call %KERNEL_CALL_STATIC_CONSTRUCTORS_METHOD% ; Call the static constructors - this is a macro used by the kernel compiler.
	call %KERNEL_MAIN_METHOD% ; Call our main method - this is a macro used by the kernel compiler.
	
	; We shouldn't ever get to this point! But just in case we do...
	jmp method_System_Void_Kernel_PreReqs_Reset__ ; For now this is our intended behaviour

; END - Main Entrypoint
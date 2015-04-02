BITS 32

SECTION .text

GLOBAL GetEIP:function

EXTERN %KERNEL_CALL_STATIC_CONSTRUCTORS_METHOD%
EXTERN %KERNEL_MAIN_METHOD%
EXTERN method_System_Void_Kernel_PreReqs_Reset__

; BEGIN - Main Entrypoint
call __MAIN_ENTRYPOINT__ ; Call our main entry point 
						 ; - not strictly necessary but good for setting up esp etc.
		
__MAIN_ENTRYPOINT__:

	push dword ebp
	mov dword ebp, esp
	
;	cli
	
;	mov dword eax, 0x2B
;	ltr ax

;	mov dword eax, esp
;	push dword 0x23
;	push dword eax
;	push dword 0x0002
;	push dword 0x1B
;	push dword __MAIN_ENTRYPOINT___Cont
	
;	push dword 0
;	push dword 0
;	push dword 0
;	push dword 0
;	push dword 0xDEADBEEF
;	push dword eax
;	push dword 0
;	push dword 0

;	push dword 0x23
;	push dword 0x23
;	push dword 0x23
;	push dword 0x23

;	mov dword [_NATIVE_TSS+4], eax

;	INTERRUPTS_RESTORE_STATE 666
;	iretd
	
	
; __MAIN_ENTRYPOINT___Cont:
;	push dword 0x22
;	pop dword eax

;	mov byte [0xB8001], al

;	jmp __MAIN_ENTRYPOINT___Cont

	call %KERNEL_CALL_STATIC_CONSTRUCTORS_METHOD% ; Call the static constructors - this is a macro used by the kernel compiler.
	call %KERNEL_MAIN_METHOD% ; Call our main method - this is a macro used by the kernel compiler.
	
	; We shouldn't ever get to this point! But just in case we do...
	jmp method_System_Void_Kernel_PreReqs_Reset__ ; For now this is our intended behaviour

; END - Main Entrypoint

; BEGIN - GetEIP

; NOTE: Leaves a "dirty stack" on purpose. The aim of this method is for EIP
;		to be on top of the stack after it returns.
GetEIP:
	push dword [ESP]
ret

; END  - GetEIP
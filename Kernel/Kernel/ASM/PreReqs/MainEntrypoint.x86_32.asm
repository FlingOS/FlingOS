BITS 32

SECTION .text
GLOBAL File_MainEntryPoint:function
File_MainEntryPoint:

mov dword eax, 0x5530
mov word [0xB8004], ax
mov dword eax, 0x6630
mov word [0xB8006], ax
mov dword ecx, 0xFFFFFFFF
LoopXYZ2:
loop LoopXYZ2

; BEGIN - Main Entrypoint
call __MAIN_ENTRYPOINT__ ; Call our main entry point 

GLOBAL GetEIP:function

EXTERN %KERNEL_CALL_STATIC_CONSTRUCTORS_METHOD%
EXTERN %KERNEL_MAIN_METHOD%
EXTERN method_System_Void_Kernel_PreReqs_Reset__

EXTERN method_System_Void_RETEND_Kernel_BasicConsole_DECLEND__cctor_NAMEEND___
EXTERN method_System_Void_RETEND_Kernel_BasicConsole_DECLEND_Clear_NAMEEND___
EXTERN method_System_Void_RETEND_Kernel_BasicConsole_DECLEND_WriteLine_NAMEEND__Kernel_FOS_System_String_

extern type_Kernel_FOS_System_String
StringLiteral_e5e336c6_42a4_4ec6_be45_088caaf386c3:
dd type_Kernel_FOS_System_String
db 28, 0, 0, 0
dw 116, 121, 112, 101, 95, 75, 101, 114, 110, 101, 108, 95, 68, 101, 98, 117, 103, 95, 66, 97, 115, 105, 99, 68, 101, 98, 117, 103
						 ; - not strictly necessary but good for setting up esp etc.
		
__MAIN_ENTRYPOINT__:
	push dword ebp
	mov dword ebp, esp
	
	call method_System_Void_RETEND_Kernel_BasicConsole_DECLEND__cctor_NAMEEND___
	
mov dword eax, 0x7730
mov word [0xB8008], ax

	push dword StringLiteral_e5e336c6_42a4_4ec6_be45_088caaf386c3

mov dword eax, 0x2230
mov word [0xB800A], ax

	call method_System_Void_RETEND_Kernel_BasicConsole_DECLEND_Clear_NAMEEND___

mov dword eax, 0x3330
mov word [0xB800A], ax

	call method_System_Void_RETEND_Kernel_BasicConsole_DECLEND_WriteLine_NAMEEND__Kernel_FOS_System_String_
	
mov dword eax, 0x4430
mov word [0xB800C], ax

mov dword ecx, 0xFFFFFFFF
LoopXYZ7:
loop LoopXYZ7

	add ESP, 4
	
mov dword ecx, 0xFFFFFFFF
LoopXYZ8:
loop LoopXYZ8

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
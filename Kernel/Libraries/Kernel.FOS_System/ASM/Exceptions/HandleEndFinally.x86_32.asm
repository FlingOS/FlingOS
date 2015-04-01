BITS 32

SECTION .text

GLOBAL method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_HandleEndFinally_NAMEEND___:function

EXTERN staticfield_Kernel_ExceptionHandlerInfo__Kernel_ExceptionMethods_CurrentHandlerPtr
EXTERN staticfield_System_Boolean_Kernel_ExceptionMethods_PendingException
EXTERN method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_HandleException_NAMEEND__

method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_HandleEndFinally_NAMEEND___:

; Leaving a "finally" critical section cleanly
; We need to handle 2 cases:
; Case 1 : Pending exception
; Case 2 : No pending exception

; Case 1 : Pending exception
; We call HandleException to continue passing the exception up the chain
; 1. Set current exception handler to previous handler
; 2. [Optional] Do stack cleanup
; 3. Call HandleException

; Case 2 : No pending exception
; We return control as normal
; 1. Set current exception handler to previous handler
; 2. Do stack cleanup
; 3. Return control as normal


; So overall we:
; 1. Set current handler to previous handler
; 2. Temp store Return Address
; 3. Restore ESP / EBP to that when current (not previous) handler was removed
; 4. Check if there is a pending exception:
; 4.1. If so, Call HandleException
; 4.2. Else, return control to Return Address (arg 0)



; 1. Set current handler to previous handler
; Load address of current handler into eax
; Current Handler points to last element in structure (i.e. "ESP")
mov dword eax, [staticfield_Kernel_ExceptionHandlerInfo__Kernel_ExceptionMethods_CurrentHandlerPtr]
; Load previous handler address
mov dword ebx, [eax+16]
; Move into CurrentHandlerPtr
mov dword [staticfield_Kernel_ExceptionHandlerInfo__Kernel_ExceptionMethods_CurrentHandlerPtr], ebx

; Set InHandler to false
mov dword [eax+20], 0


; 2. Temp store Return Address (arg 0)
mov dword edx, [esp]



; 3. Restore ESP / EBP to that when current (not previous) handler was removed

; No need to load EBP as that won't have changed since we entered the handler

; Load ESP
mov dword ebx, [eax]
; Restore ESP
; By setting ESP then adding 20 to get past this handler's space on stack
mov dword esp, ebx
add esp, 24


; 4. Check if there is a pending exception:
; Zero-out eax
xor eax, eax
; Load boolean indicating if there is a pending exception or not
mov dword eax, [staticfield_System_Boolean_Kernel_ExceptionMethods_PendingException]
; Compare to 0, 0 = false, 1 = true
cmp eax, 0
; If false i.e. no pending exception, jump to 2.2
jz method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_HandleEndFinally_NAMEEND___NoPendingException


; 4.1. If true i.e. there is a pending exception, Call HandleException
call method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_HandleException_NAMEEND__


; 4.2. Else, return control to Return Address (temp stored)
method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_HandleEndFinally_NAMEEND___NoPendingException:
; Push the return address (temp stored)
push dword edx

ret
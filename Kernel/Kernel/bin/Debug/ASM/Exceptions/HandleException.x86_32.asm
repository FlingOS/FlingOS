method_System_Void_RETEND_Kernel_Exceptions_DECLEND_HandleException_NAMEEND__:

; Handling an exception
; We need to get current exception handler
; TODO - Check current handler filter. 
;		 : If not 0x00000000 or 0xFFFFFFFF then execute it and check return status
;		 : Loop until handler found or no handlers left
; If handler found:
; Then restore stack to correct location for that handler
; If handler is a catch-handler, set Pending Exception to false.
; Return control to the handler

; Steps:
; 0. If we are already in handler, shift to previous handler
; 1. Get current exception handler
; 2. Do a check to see if handler address == 0, if it is, this is not a valid handler
;		and so we have an unhandled exception!
; 3. Restore ESP / EBP to current handler locations
; 4. Check filter != 0x00000000, if so, set PendingException to 0
; 5. Return control to handler


; 0. If we are already in handler, shift to previous handler
; Get current exception handler
; Load address of current handler into eax
; Current Handler points to last element in structure (i.e. "ESP")
mov dword eax, [staticfield_Kernel_ExceptionHandlerInfo__Kernel_Exceptions_CurrentHandlerPtr]
; Load whether we are already executing the current handler
mov dword eax, [eax+20]
cmp eax, 0
jz method_System_Void_RETEND_Kernel_Exceptions_DECLEND_HandleException_NAMEEND__.NotInHandler
mov dword eax, [staticfield_Kernel_ExceptionHandlerInfo__Kernel_Exceptions_CurrentHandlerPtr]
mov dword eax, [eax+16]
mov dword [staticfield_Kernel_ExceptionHandlerInfo__Kernel_Exceptions_CurrentHandlerPtr], eax
method_System_Void_RETEND_Kernel_Exceptions_DECLEND_HandleException_NAMEEND__.NotInHandler:


; 1. Get current exception handler
; Load address of current handler into eax
; Current Handler points to last element in structure (i.e. "ESP")
mov dword eax, [staticfield_Kernel_ExceptionHandlerInfo__Kernel_Exceptions_CurrentHandlerPtr]


; 2. Do a check to see if handler address == 0, if it is, this is not a valid handler
;		and so we have an unhandled exception!
mov dword ebx, [eax+8]
cmp ebx, 0
jz method_System_Void_RETEND_Kernel_Exceptions_DECLEND_HandleException_NAMEEND__UnhandledException


; 3. Restore ESP / EBP to current handler locations
; Load EBP
mov dword ebx, [eax+4]
; Restore EBP
mov dword ebp, ebx

; Load ESP
mov dword ebx, [eax]
; Restore ESP
mov dword esp, ebx


; 4. Check filter != 0x00000000, if so, set PendingException to 0
; Load Filter Address
mov ebx, [eax+12]
; Compare Filter Address to 0
cmp ebx, 0
; If it is 0, this is only a finally handler, so the exception is still pending
; i.e. it is yet to actually be handled.
jz method_System_Void_RETEND_Kernel_Exceptions_DECLEND_HandleException_NAMEEND__Step5

; If we get here, this is a catch handler
; So set PendingException to false (0)
mov dword [staticfield_System_Boolean_Kernel_Exceptions_PendingException], 0


; 5. Return control to handler
method_System_Void_RETEND_Kernel_Exceptions_DECLEND_HandleException_NAMEEND__Step5:
; Set InHandler to true
mov dword [eax+20], 1
; Push the handler execution address (to return to)
push dword [eax+8]
ret


method_System_Void_RETEND_Kernel_Exceptions_DECLEND_HandleException_NAMEEND__UnhandledException:
; If we have an unhandled exception, we show an error message and then panic!

mov dword eax, [staticfield_System_String_Kernel_Exceptions_UnhandledException_PanicMessage]
push eax
call method_System_Void_RETEND_Kernel_BasicConsole_DECLEND_WriteLine_NAMEEND__System_String_

call method_System_Void_RETEND_Kernel_Kernel_DECLEND_Halt_NAMEEND___
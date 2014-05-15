method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_HandleLeave_NAMEEND__System_Void__:

; Leaving a critical section cleanly
; We need to handle 2 cases:
; Case 1 : Leaving "try" or "catch" of a try-catch
; Case 2 : Leaving the "try" of a try-finally

; Case 1: Leaving try-catch
; We need to cleanup stack then return control to Continue Address
; 1. Set current exception handler to previous handler
; 2. Restore EBP and ESP to previous places (this will also remove current handler from stack)
; 3. Return control to Continue Address (arg 0)

; Case 2: Leaving try-finally
; We need to cleanup stack then return control to Finally Handler Address (in current handler)
; 1. Set continue address to finally (current) handler address
; 2. Restore EBP and ESP to current handler places
; 3. Return control to Continue Address (arg 0)

; So, overall we:
; 0. If current exception exists, decrement exception ref count and clear current exception
; 1. Check if handler is a finally handler (Filter Address == 0? Then it is try-finally)
; 1.1. If it is, set continue address to handler address
; 1.2. Else, set current exception handler to previous handler
; 2. Temporaily store Continue Address (in edx)
; 3. Restore EBP and ESP to current handler places
; 4. Return control to Continue Address (from temp store)



; 0. If current exception exists, decrement exception ref count and clear current exception
mov dword eax, [staticfield_Kernel_FOS_System_Exception_Kernel_ExceptionMethods_CurrentException]
cmp eax, 0
jz method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_HandleLeave_NAMEEND__System_Void__.NoCurrentException

push dword [staticfield_Kernel_FOS_System_Exception_Kernel_ExceptionMethods_CurrentException]
call method_System_Void_RETEND_Kernel_FOS_System_GC_DECLEND_DecrementRefCount_NAMEEND__Kernel_FOS_System_Object_
add esp, 4
mov dword [staticfield_Kernel_FOS_System_Exception_Kernel_ExceptionMethods_CurrentException], 0

method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_HandleLeave_NAMEEND__System_Void__.NoCurrentException:


; Load address of current handler into eax
; Current Handler points to last element in structure (i.e. "ESP")
mov dword eax, [staticfield_Kernel_ExceptionHandlerInfo__Kernel_ExceptionMethods_CurrentHandlerPtr]

; Set InHandler to false
mov dword [eax+20], 0



; 1. Check if handler is a finally handler 
; Load Filter Address of current handler into ebx
mov dword ebx, [eax+12]
; Check to see if filter address == 0 i.e. if it is finally handler
cmp ebx, 0
jnz method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_HandleLeave_NAMEEND__System_Void__NotFinallyHandler
; If we get to here, it is a finally handler

; 1.1. If it is, set continue address to handler address
; So move Handler Address into Continue Address (arg 0)
; Load Handler Address
mov dword ebx, [eax+8]
; Move into arg 0. Arg 0 is at: ESP + 4 (for return address) = ESP+4
mov dword [esp+4], ebx
; And set InHandler to true
mov dword [eax+20], 1
; Continue at common code
jmp method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_HandleLeave_NAMEEND__System_Void__Common


; 1.2. Else, set current exception handler to previous handler
method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_HandleLeave_NAMEEND__System_Void__NotFinallyHandler:
; If we get to here, it is not a finally handler
; Load previous handler address
mov dword ebx, [eax+16]
; Move into CurrentHandlerPtr
mov dword [staticfield_Kernel_ExceptionHandlerInfo__Kernel_ExceptionMethods_CurrentHandlerPtr], ebx



; Common code for case 1 and 2 after this point
method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_HandleLeave_NAMEEND__System_Void__Common:

; 2. Temporaily store Continue Address (in edx)
; Continue address is arg 0 at ESP+4
mov dword edx, [esp+4]


; 3. Restore EBP and ESP to previous places
; Previous handler is now in current handler thus:

; Load address of current handler into eax
; Current Handler points to last element in structure (i.e. "ESP")
mov dword eax, [staticfield_Kernel_ExceptionHandlerInfo__Kernel_ExceptionMethods_CurrentHandlerPtr]

; Load EBP
mov dword ebx, [eax+4]
; Restore EBP
mov dword ebp, ebx

; Load ESP
mov dword ebx, [eax]
; Restore ESP
mov dword esp, ebx


; 4. Return control to Continue Address
; Continue address in temp store (i.e. in edx - see above)
push dword edx


ret
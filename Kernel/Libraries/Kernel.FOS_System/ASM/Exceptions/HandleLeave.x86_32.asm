BITS 32

SECTION .text

GLOBAL method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_HandleLeave_NAMEEND__System_Void__:function

EXTERN staticfield_Kernel_ExceptionHandlerInfo__Kernel_ExceptionMethods_CurrentHandlerPtr
EXTERN method_System_Void_RETEND_Kernel_FOS_System_GC_DECLEND_DecrementRefCount_NAMEEND__Kernel_FOS_System_Object_

method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_HandleLeave_NAMEEND__System_Void__:

; Leaving a critical section cleanly
; We need to handle 2 cases:
; Case 1 : Leaving "try" or "catch" of a try-catch
; Case 2 : Leaving the "try" of a try-finally

; Case 1: Leaving try-catch
; We need to cleanup stack then return control to Continue Address
; 1. Set current exception handler to previous handler
; 2. Restore EBP and ESP to curent handler places (this will also remove current handler from stack)
; 3. Return control to Continue Address (arg 0)

; Case 2: Leaving try-finally
; We need to cleanup stack then return control to Finally Handler Address (in current handler)
; 1. Set continue address to finally (current) handler address
; 2. Restore EBP and ESP to current handler places
; 3. Remove current handler from stack by adding to esp
; 4. Return control to Finally Handler Address

; So, overall we:
; 0. If catch handler, and if current exception exists, decrement exception ref count and clear current exception
; 1. Temporaily store Continue Address (in edx)
; 2. Restore EBP and ESP to current handler places
; 3. Check if handler is a finally handler (Filter Address == 0? Then it is try-finally)
; 3.1. If it is, set continue address to handler address
; 3.2. Else, set current handler to previous handler and add amount to esp
; 4. Return control to Continue Address (from temp store)



; 0. If catch handler, and if current exception exists, decrement exception ref count and clear current exception
; Check if handler is a finally handler 
; Load current handler ptr into eax
mov dword eax, [staticfield_Kernel_ExceptionHandlerInfo__Kernel_ExceptionMethods_CurrentHandlerPtr]
; Load Filter Address of current handler into ebx
mov dword ebx, [eax+12]
; Check to see if filter address == 0 i.e. if it is finally handler
cmp ebx, 0
jz method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_HandleLeave_NAMEEND__System_Void__IsFinallyHandler1
; If we get to here, it is a catch handler

mov dword eax, [staticfield_Kernel_FOS_System_Exception_Kernel_ExceptionMethods_CurrentException]
cmp eax, 0
jz method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_HandleLeave_NAMEEND__System_Void__.NoCurrentException

push dword [staticfield_Kernel_FOS_System_Exception_Kernel_ExceptionMethods_CurrentException]
call method_System_Void_RETEND_Kernel_FOS_System_GC_DECLEND_DecrementRefCount_NAMEEND__Kernel_FOS_System_Object_
add esp, 4
mov dword [staticfield_Kernel_FOS_System_Exception_Kernel_ExceptionMethods_CurrentException], 0

method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_HandleLeave_NAMEEND__System_Void__.NoCurrentException:


method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_HandleLeave_NAMEEND__System_Void__IsFinallyHandler1:

; Load address of current handler into eax
; Current Handler points to last element in structure (i.e. "ESP")
mov dword eax, [staticfield_Kernel_ExceptionHandlerInfo__Kernel_ExceptionMethods_CurrentHandlerPtr]

; Set InHandler to false
mov dword [eax+20], 0



; 1. Temporaily store Continue Address (in edx)
; Continue address is arg 0 at ESP+4
mov dword edx, [esp+4]


; 2. Restore EBP and ESP to previous places

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


; 3. Check if handler is a finally handler 
; Load Filter Address of current handler into ebx
mov dword ebx, [eax+12]
; Check to see if filter address == 0 i.e. if it is finally handler
cmp ebx, 0
jnz method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_HandleLeave_NAMEEND__System_Void__NotFinallyHandler1
; If we get to here, it is a finally handler

; 3.1. If it is, set continue address to handler address
; So move Handler Address into Continue Address (arg 0)
; Load Handler Address into temp store
mov dword edx, [eax+8]
; And set InHandler to true
mov dword [eax+20], 1
; Continue at common code
jmp method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_HandleLeave_NAMEEND__System_Void__Common

; 3.2. Else, set current exception handler to previous handler
method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_HandleLeave_NAMEEND__System_Void__NotFinallyHandler1:
; If we get to here, it is not a finally handler
; Load previous handler address
mov dword ebx, [eax+16]
; Move into CurrentHandlerPtr
mov dword [staticfield_Kernel_ExceptionHandlerInfo__Kernel_ExceptionMethods_CurrentHandlerPtr], ebx
; And increase ESP to remove handler from stack
add esp, 24


; Common code for case 1 and 2 after this point
method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_HandleLeave_NAMEEND__System_Void__Common:

; 4. Return control to Continue Address
; Continue address in temp store (i.e. in edx - see above)
push dword edx


ret
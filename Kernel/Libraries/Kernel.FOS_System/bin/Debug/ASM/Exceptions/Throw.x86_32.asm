method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_NAMEEND__Kernel_FOS_System_Exception_:
method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_ThrowFromPtr_NAMEEND__System_UInt32__:


; Throwing an exception
; 0. If Current Exception not null, decrement ref count
; 1. Set Current Exception to that specified by arg to this method
; 2. Set Pending Exception to true
; 3. Call HandleException


; 0. If Current Exception not null, decrement ref count
mov dword eax, [staticfield_Kernel_FOS_System_Exception_Kernel_ExceptionMethods_CurrentException]
cmp eax, 0
jz method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_NAMEEND__Kernel_FOS_System_Exception_.NoCurrentException

push dword [staticfield_Kernel_FOS_System_Exception_Kernel_ExceptionMethods_CurrentException]
call method_System_Void_RETEND_Kernel_FOS_System_GC_DECLEND_DecrementRefCount_NAMEEND__Kernel_FOS_System_Object_
add esp, 4

method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_NAMEEND__Kernel_FOS_System_Exception_.NoCurrentException:


; 1. Set Current Exception to that specified by arg to this method
mov dword eax, [esp+4]
mov dword [staticfield_Kernel_FOS_System_Exception_Kernel_ExceptionMethods_CurrentException], eax


; 2. Set Pending Exception to true
mov dword [staticfield_System_Boolean_Kernel_ExceptionMethods_PendingException], 1


; 3. Call HandleException
call method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_HandleException_NAMEEND__


; We never expect HandleException to return!
; If it does accidentally return, we show an error message and 
; then panic!

mov dword eax, [staticfield_System_String_Kernel_ExceptionMethods_Throw_PanicMessage]
push eax
call method_System_Void_RETEND_Kernel_BasicConsole_DECLEND_WriteLine_NAMEEND__System_String_

call method_System_Void_RETEND_Kernel_Kernel_DECLEND_Halt_NAMEEND___
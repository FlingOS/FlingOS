; BEGIN - int Kernel.FOS_System.String.GetLength(string)
method_System_Int32_RETEND_Kernel_FOS_System_String_DECLEND_GetLength_NAMEEND__System_String_:

; MethodStart
push dword ebp
mov dword ebp, esp

; Load string address
mov eax, [ebp+12]
; Load string length
mov eax, [eax]
; Set return value to length
mov [ebp+8], eax

; MethodEnd
pop dword ebp

ret
; END - int Kernel.FOS_System.String.GetLength(string)
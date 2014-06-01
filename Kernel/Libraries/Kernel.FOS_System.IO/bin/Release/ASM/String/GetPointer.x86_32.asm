; BEGIN - byte* Kernel.FOS_System.String.GetBytePointer(string)
; BEGIN - char* Kernel.FOS_System.String.GetCharPointer(string)
method_System_Byte__RETEND_Kernel_FOS_System_String_DECLEND_GetBytePointer_NAMEEND__System_String_:
method_System_Char__RETEND_Kernel_FOS_System_String_DECLEND_GetCharPointer_NAMEEND__System_String_:

; MethodStart
push dword ebp
mov dword ebp, esp

; Load string address
mov eax, [ebp+12]
; Load pointer to first char by skipping over length bytes
add eax, 4
; Set return value to pointer (the address)
mov [ebp+8], eax

; MethodEnd
pop dword ebp

ret
; END - byte* Kernel.FOS_System.String.GetBytePointer(string)
; END - char* Kernel.FOS_System.String.GetCharPointer(string)
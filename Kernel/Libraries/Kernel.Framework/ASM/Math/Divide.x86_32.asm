BITS 32

SECTION .text

GLOBAL method_System_UInt64_RETEND_Kernel_Framework_Math_DECLEND_Divide_NAMEEND__System_UInt64_System_UInt32_:function

method_System_UInt64_RETEND_Kernel_Framework_Math_DECLEND_Divide_NAMEEND__System_UInt64_System_UInt32_:

push dword ebp
mov dword ebp, esp

; Return value low-bits		: EBP+8
; Return value high-bits	: EBP+12
; Arg 2 32-bits				: EBP+16
; Arg 1 low-bits			: EBP+20
; Arg 1 high-bits			: EBP+24

; 64-bit dividend
mov edx, [ebp+24]
mov eax, [ebp+20]

; 32-bit divisor
mov ecx, [ebp+16]

push eax
mov eax, edx
xor edx, edx
div ecx ; get high 32 bits of quotient
xchg eax, [esp] ; store them on stack, get low 32 bits of dividend
div ecx ; get low 32 bits of quotient
pop edx ; 64-bit quotient in edx:eax now

mov [ebp+8], eax
mov [ebp+12], edx

pop dword ebp

ret
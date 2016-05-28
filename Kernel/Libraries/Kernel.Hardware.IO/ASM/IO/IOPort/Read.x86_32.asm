BITS 32

SECTION .text

GLOBAL method_System_Byte_RETEND_Kernel_IO_IOPort_DECLEND_doRead_Byte_NAMEEND__System_UInt16_:function
GLOBAL method_System_UInt16_RETEND_Kernel_IO_IOPort_DECLEND_doRead_UInt16_NAMEEND__System_UInt16_:function
GLOBAL method_System_UInt32_RETEND_Kernel_IO_IOPort_DECLEND_doRead_UInt32_NAMEEND__System_UInt16_:function
GLOBAL method_System_UInt64_RETEND_Kernel_IO_IOPort_DECLEND_doRead_UInt64_NAMEEND__System_UInt16_:function

method_System_Byte_RETEND_Kernel_IO_IOPort_DECLEND_doRead_Byte_NAMEEND__System_UInt16_:
push dword ebp
mov dword ebp, esp
mov dword edx, [ebp+12]
in al, dx
mov [ebp+8], al
pop dword ebp
ret

method_System_UInt16_RETEND_Kernel_IO_IOPort_DECLEND_doRead_UInt16_NAMEEND__System_UInt16_:
push dword ebp
mov dword ebp, esp
mov dword edx, [ebp+12]
in ax, dx
mov [ebp+8], ax
pop dword ebp
ret

method_System_UInt32_RETEND_Kernel_IO_IOPort_DECLEND_doRead_UInt32_NAMEEND__System_UInt16_:
push dword ebp
mov dword ebp, esp
mov dword edx, [ebp+12]
in eax, dx
mov [ebp+8], eax
pop dword ebp
ret

method_System_UInt64_RETEND_Kernel_IO_IOPort_DECLEND_doRead_UInt64_NAMEEND__System_UInt16_:
push dword ebp
mov dword ebp, esp
mov dword edx, [ebp+16]
in eax, dx
mov [ebp+8], eax
in eax, dx
mov [ebp+12], eax
pop dword ebp
ret
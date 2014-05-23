method_System_Void_RETEND_Kernel_Hardware_IO_IOPort_DECLEND_doWrite_NAMEEND__System_UInt16_System_Byte_:
push dword ebp
mov dword ebp, esp
mov dword edx, [ebp+12]
mov dword eax, [ebp+8]
out dx, al
pop dword ebp
ret

method_System_Void_RETEND_Kernel_Hardware_IO_IOPort_DECLEND_doWrite_NAMEEND__System_UInt16_System_UInt16_:
push dword ebp
mov dword ebp, esp
mov dword edx, [ebp+12]
mov dword eax, [ebp+8]
out dx, ax
pop dword ebp
ret

method_System_Void_RETEND_Kernel_Hardware_IO_IOPort_DECLEND_doWrite_NAMEEND__System_UInt16_System_UInt32_:
push dword ebp
mov dword ebp, esp
mov dword edx, [ebp+12]
mov dword eax, [ebp+8]
out dx, eax
pop dword ebp
ret

method_System_Void_RETEND_Kernel_Hardware_IO_IOPort_DECLEND_doWrite_NAMEEND__System_UInt16_System_UInt64_:
push dword ebp
mov dword ebp, esp
mov dword edx, [ebp+16]
mov dword eax, [ebp+8]
out dx, eax
mov dword eax, [ebp+12]
out dx, eax
pop dword ebp
ret
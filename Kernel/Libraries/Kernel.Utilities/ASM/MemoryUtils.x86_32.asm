method_System_UInt32_RETEND_Kernel_Utilities_MemoryUtils_DECLEND_htonl_NAMEEND__System_UInt32_:

push dword ebp
mov dword ebp, esp

mov dword eax, [ebp+12]
bswap eax
mov dword [ebp+8], eax

pop dword ebp

ret
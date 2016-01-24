BITS 32

SECTION .text

GLOBAL method_System_UInt32_RETEND_Drivers_Utilities_MemoryUtils_DECLEND_htonl_NAMEEND__System_UInt32_:function

method_System_UInt32_RETEND_Drivers_Utilities_MemoryUtils_DECLEND_htonl_NAMEEND__System_UInt32_:

push dword ebp
mov dword ebp, esp

mov dword eax, [ebp+12]
bswap eax
mov dword [ebp+8], eax

pop dword ebp

ret
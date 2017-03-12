BITS 32

SECTION .text

GLOBAL method_System_UInt32_RETEND_Kernel_Utilities_MemoryUtils_DECLEND_htonl_NAMEEND__System_UInt32_:function

method_System_UInt32_RETEND_Kernel_Utilities_MemoryUtils_DECLEND_htonl_NAMEEND__System_UInt32_:

push dword ebp
mov dword ebp, esp

mov dword eax, [ebp+12]
bswap eax
mov dword [ebp+8], eax

pop dword ebp

ret



GLOBAL method_System_Void_RETEND_Kernel_Utilities_MemoryUtils_DECLEND_MemCpy_NAMEEND__System_Byte__System_Byte__System_UInt32_:function

method_System_Void_RETEND_Kernel_Utilities_MemoryUtils_DECLEND_MemCpy_NAMEEND__System_Byte__System_Byte__System_UInt32_:

; +4 = Size
; +8 = Src
; +12 = Dst

mov esi, [esp+8]
mov edi, [esp+12]
cld
mov ecx, [esp+4]
rep movsb

ret



GLOBAL method_System_Void_RETEND_Kernel_Utilities_MemoryUtils_DECLEND_MemCpy16_NAMEEND__System_UInt16__System_UInt16__System_UInt32_:function

method_System_Void_RETEND_Kernel_Utilities_MemoryUtils_DECLEND_MemCpy16_NAMEEND__System_UInt16__System_UInt16__System_UInt32_:

; +4 = Size
; +8 = Src
; +12 = Dst

mov esi, [esp+8]
mov edi, [esp+12]
cld
mov ecx, [esp+4]
rep movsw

ret



GLOBAL method_System_Void_RETEND_Kernel_Utilities_MemoryUtils_DECLEND_MemCpy32_NAMEEND__System_UInt32__System_UInt32__System_UInt32_:function

method_System_Void_RETEND_Kernel_Utilities_MemoryUtils_DECLEND_MemCpy32_NAMEEND__System_UInt32__System_UInt32__System_UInt32_:

; +4 = Size
; +8 = Src
; +12 = Dst

mov esi, [esp+8]
mov edi, [esp+12]
cld
mov ecx, [esp+4]
rep movsd

ret



GLOBAL method_System_Void_RETEND_Kernel_Utilities_MemoryUtils_DECLEND_MemSet_NAMEEND__System_Byte__System_Byte_System_UInt32_:function

method_System_Void_RETEND_Kernel_Utilities_MemoryUtils_DECLEND_MemSet_NAMEEND__System_Byte__System_Byte_System_UInt32_:

; +4 = Size
; +8 = Value
; +12 = Dst

mov al, [esp+8]
mov edi, [esp+12]
cld
mov ecx, [esp+4]
rep stosb

ret



GLOBAL method_System_Void_RETEND_Kernel_Utilities_MemoryUtils_DECLEND_MemSet16_NAMEEND__System_UInt16__System_UInt16_System_UInt32_:function

method_System_Void_RETEND_Kernel_Utilities_MemoryUtils_DECLEND_MemSet16_NAMEEND__System_UInt16__System_UInt16_System_UInt32_:

; +4 = Size
; +8 = Value
; +12 = Dst

mov ax, [esp+8]
mov edi, [esp+12]
cld
mov ecx, [esp+4]
rep stosw

ret



GLOBAL method_System_Void_RETEND_Kernel_Utilities_MemoryUtils_DECLEND_MemSet32_NAMEEND__System_UInt32__System_UInt32_System_UInt32_:function

method_System_Void_RETEND_Kernel_Utilities_MemoryUtils_DECLEND_MemSet32_NAMEEND__System_UInt32__System_UInt32_System_UInt32_:

; +4 = Size
; +8 = Value
; +12 = Dst

mov eax, [esp+8]
mov edi, [esp+12]
cld
mov ecx, [esp+4]
rep stosd

ret

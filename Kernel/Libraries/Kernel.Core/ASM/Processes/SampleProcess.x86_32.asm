method_System_Void_RETEND_Kernel_Core_Processes_SampleProcess_DECLEND_Main_NAMEEND___:

; Output following text to first bit of vid mem
; N	  o      M  u    l   t   i   b   o  o   t
; 78 111 32 109 117 108 116 105 98 111 111 116
mov byte [0xB8000], 78
mov byte [0xB8002], 111
mov byte [0xB8004], 32
mov byte [0xB8006], 109
mov byte [0xB8008], 117
mov byte [0xB800A], 108
mov byte [0xB800C], 116
mov byte [0xB800E], 105
mov byte [0xB8010], 98
mov byte [0xB8012], 111
mov byte [0xB8014], 111
mov byte [0xB8016], 116

; Set the colour of the outputted text to:
; Red background (0x4-), 
; White foreground (0x-F)
mov dword eax, 0x4F
mov byte [0xB8001], al
mov byte [0xB8003], al
mov byte [0xB8005], al
mov byte [0xB8007], al
mov byte [0xB8009], al
mov byte [0xB800B], al
mov byte [0xB800D], al
mov byte [0xB800F], al
mov byte [0xB8011], al
mov byte [0xB8013], al
mov byte [0xB8015], al
mov byte [0xB8017], al

; Infinite loop
jmp method_System_Void_RETEND_Kernel_Core_Processes_SampleProcess_DECLEND_Main_NAMEEND___



method_System_Void__RETEND_Kernel_Core_Processes_SampleProcess_DECLEND_GetMainMethodPtr_NAMEEND___:
push dword ebp
mov dword ebp, esp

mov dword [ebp+8], method_System_Void_RETEND_Kernel_Core_Processes_SampleProcess_DECLEND_Main_NAMEEND___

pop dword ebp
ret
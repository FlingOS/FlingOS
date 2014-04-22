; BEGIN - Basic Debug : Receive Commands

BasicDebug_WaitForCommand:

BasicDebug_WaitForCommand_Loop:
call method_System_Byte_Kernel_Debug_BasicDebug_Serial_ReadByte__
cmp al, 0
jz BasicDebug_WaitForCommand_Loop

ret

; END - Basic Debug : Receive Commands
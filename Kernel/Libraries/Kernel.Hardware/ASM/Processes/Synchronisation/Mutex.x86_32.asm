method_System_Void_RETEND_Kernel_Hardware_Processes_Synchronisation_Mutex_DECLEND_Enter_NAMEEND___:

push dword ebp
mov dword ebp, esp

.acquireLock:
mov dword ebx, [ebp+8]
lock bts word [ebx+8], 0
jc .spin_with_pause

.spin_with_pause:
pause

push ebx
push dword 5
call method_System_Void_RETEND_Kernel_Hardware_Processes_Thread_DECLEND_Sleep_NAMEEND___
add esp, 4
pop ebx

test byte [ebx+8], 1
jnz .spin_with_pause
jmp .acquireLock

pop dword ebp
ret



method_System_Void_RETEND_Kernel_Hardware_Processes_Synchronisation_Mutex_DECLEND_Exit_NAMEEND___:

push dword ebp
mov dword ebp, esp

mov dword ebx, [ebp+12]
mov byte [ebx+8], 0

pop dword ebp
ret
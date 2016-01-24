
BITS 32

SECTION .text

GLOBAL method_System_Void_RETEND_Drivers_Framework_Processes_Synchronisation_SpinLock_DECLEND__Enter_NAMEEND___:function
GLOBAL method_System_Void_RETEND_Drivers_Framework_Processes_Synchronisation_SpinLock_DECLEND__Exit_NAMEEND___:function

method_System_Void_RETEND_Drivers_Framework_Processes_Synchronisation_SpinLock_DECLEND__Enter_NAMEEND___:

push dword ebp
mov dword ebp, esp

.acquireLock:
mov dword ebx, [ebp+8]
lock bts word [ebx+8], 0
jc .spin_with_pause
jmp .lockAcquired

.spin_with_pause:
pause
test byte [ebx+8], 1
jnz .spin_with_pause
jmp .acquireLock

.lockAcquired:

pop dword ebp
ret



method_System_Void_RETEND_Drivers_Framework_Processes_Synchronisation_SpinLock_DECLEND__Exit_NAMEEND___:

push dword ebp
mov dword ebp, esp

mov dword ebx, [ebp+8]
mov word [ebx+8], 0

pop dword ebp
ret
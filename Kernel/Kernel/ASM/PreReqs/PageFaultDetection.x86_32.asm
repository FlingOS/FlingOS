BITS 32

SECTION .text

GLOBAL method_System_Void_RETEND_Kernel_PreReqs_DECLEND_PageFaultDetection_NAMEEND___:function

EXTERN staticfield_System_Boolean_Kernel_PreReqs_PageFaultDetection_Initialised
EXTERN staticfield_System_Boolean_Kernel_PreReqs_PageFaultDetection_LoopPrevention
EXTERN staticfield_System_String_Kernel_PreReqs_PageFaultDetection_ErrorString
EXTERN staticfield_System_String_Kernel_PreReqs_PageFaultDetection_SeparatorString
EXTERN method_System_Void_RETEND_Kernel_BasicConsole_DECLEND_WriteLine_NAMEEND___
EXTERN method_System_Void_RETEND_Kernel_BasicConsole_DECLEND_WriteLine_NAMEEND__Kernel_FOS_System_String_
EXTERN method_System_Void_RETEND_Kernel_BasicConsole_DECLEND_Write_NAMEEND__Kernel_FOS_System_String_
EXTERN method_System_Void_RETEND_Kernel_BasicConsole_DECLEND_DelayOutput_NAMEEND__System_Int32_
EXTERN method_Kernel_FOS_System_String_RETEND_Kernel_FOS_System_String_DECLEND_op_Implicit_NAMEEND__System_UInt32_

EXTERN Kernel_MemStart
EXTERN Kernel_MemEnd
EXTERN Kernel_Stack

method_System_Void_RETEND_Kernel_PreReqs_DECLEND_PageFaultDetection_NAMEEND___:

; This method is designed for debugging hard to trace page faults
;	It was originally written for tackling bug "Issue #14 - 0xA5F000C8 Page-fault"
; 
; The following lists the invalid addresses that are being searched for:
;	0xA5F000C8 (0xA5EFFFC8 - 0xA5F001C8)
;	0xF000FF69 (0xF000FE69 - 0xF0010069)
;
; The following lists the range of addresses which are considered valid:
;	Kernel_MemStart to Kernel_MemEnd
; 
; Steps this method takes:
;   0. Check validity of ESP and EBP
;	1. Save ESP and EBP to stack
;   2. Save all registers to stack
;   3. Check validity of all values in registers
;   4. Check validity of values at addresses in registers (taking only register values which are valid addresses)
;   5. Clean up stack
;   6. Return
;
; In the event that any check fails, steps taken are:
;	0. Print failure message to screen
;   1. Print the instruction address of the caller
;   2. Print register value that caused failure
;   3. If applicable: Print value at address that caused failure
;   4. Pause
;   5. Print out the complete stack trace from ESP to the top, 
;			4 dwords per line, pausing between every 4 lines printed
;   6. Pause execution for a long period of time
;   7. Clean up stack
;   8. Return

; ------------------------- VALIDATION -------------------------

;   0. Check validity of ESP and EBP
;		- Compare ESP to the search values
;		(This needs to be done without altering any register values in the no-fail case!)

; cmp esp, 0xA5F000C8
; jne .CHECK_PASSED_1
; mov dword eax, esp
; mov dword ebx, 0
; mov dword ecx, 0
; jmp .FAIL

; .CHECK_PASSED_1:
; cmp esp, 0xF000FF69
; jne .CHECK_PASSED_2
; mov dword eax, esp
; mov dword ebx, 0
; mov dword ecx, 0
; jmp .FAIL

; .CHECK_PASSED_2:
; cmp ebp, 0xA5F000C8
; jne .CHECK_PASSED_3
; mov dword eax, ebp
; mov dword ebx, 0
; mov dword ecx, 0
; jmp .FAIL

; .CHECK_PASSED_3:
; cmp ebp, 0xF000FF69
; jne .CHECK_PASSED_4
; mov dword eax, ebp
; mov dword ebx, 0
; mov dword ecx, 0
; jmp .FAIL

; .CHECK_PASSED_4:

;	1. Save ESP and EBP to stack
;		- Push ESP to stack
push dword esp
;		- Push EBP to stack
push dword ebp

;   2. Save all registers to stack
;		- Push all register values
pushad

;   3. Check validity of all values in registers
;		- Start at bottom of the stack which now consists of (in reverse order):
;			EDI, ESI, EBP, original ESP, EBX, EDX, ECX, EAX
mov dword eax, esp
;		- Load the number of values to check from the stack (8 as per above)
mov dword ecx, 8
;		- Loop through, checking the values
.REGTEST_LOOP_1:
;		- Get the value to check
mov dword ebx, [eax]

;		- Check the value
call method_System_Void_RETEND_Kernel_PreReqs_DECLEND_PageFaultDetection_NAMEEND___CheckEBX
cmp ebx, 0
jz .REGTEST_LOOP_1_NOFAIL
;		- Otherwise, move in the value, and necessary inputs
mov dword eax, ebx
mov dword ebx, 0
mov dword ecx, 1
jmp .FAIL

.REGTEST_LOOP_1_NOFAIL:
;		-  Move to next value
add dword eax, 4
;		- Loop (decrements ecx until 0)
loop .REGTEST_LOOP_1

jmp .END1

;   4. Check validity of values at addresses in registers (taking only register values which are valid addresses)
;		- Start at bottom of the stack which now consists of (in reverse order):
;			EDI, ESI, EBP, original ESP, EBX, EDX, ECX, EAX
mov dword eax, esp
;		- Load the number of values to check from the stack (8 as per above)
mov dword ecx, 8
;		- Loop through, checking the values as addresses and, if valid, the values at the addresses
.REGTEST_LOOP_2:
;		- Get the address to check
mov dword edx, [eax]
;		- Check the address is inside the valid range
;			- Do the compare to low address
cmp edx, Kernel_MemStart
;			- Check if less than mem start, if so, skip (unsigned comparison)
jb .REGTEST_LOOP_2_NOFAIL
;			- Do the compare to high address
cmp edx, Kernel_MemEnd
;			- Check if greater than mem end, if so, skip (unsigned comparison)
ja .REGTEST_LOOP_2_NOFAIL

;			- Load the value at the address
mov ebx, [edx]

;		- Check the value
call method_System_Void_RETEND_Kernel_PreReqs_DECLEND_PageFaultDetection_NAMEEND___CheckEBX
cmp ebx, 0
jz .REGTEST_LOOP_2_NOFAIL
;		- Otherwise, move in the value, and necessary inputs
mov dword eax, edx
;mov dword ebx, ebx
mov dword ecx, 1
jmp .FAIL


.REGTEST_LOOP_2_NOFAIL:
;		-  Move to next value
add dword eax, 4
;		- Loop (decrements ecx until 0)
loop .REGTEST_LOOP_2

.END1:
;   5. Clean up stack
;		: Reverse steps 1 and 2.
;			- Pop all registers from stack
popad
;			- Pop EBP from stack
pop dword ebp
;			- Pop ESP from stack
pop dword esp

;   6. Return
ret



; ---------------------- FAILURE HANDLING ----------------------
.FAIL:
method_System_Void_RETEND_Kernel_PreReqs_DECLEND_PageFaultDetection_NAMEEND___Fail:
; INPUT to this section of code: 
;		EAX = Value of register which caused failure
;		EBX = Value at address which caused failure
;       ECX = Whether ESP, EBP and Registers have been pushed or not
;
mov edx, [staticfield_System_Boolean_Kernel_PreReqs_PageFaultDetection_Initialised]
cmp edx, 0
jz .END2

mov edx, [staticfield_System_Boolean_Kernel_PreReqs_PageFaultDetection_LoopPrevention]
cmp edx, 0
jnz .END2

mov dword edx, 1
mov [staticfield_System_Boolean_Kernel_PreReqs_PageFaultDetection_LoopPrevention], edx

;	0. Print failure message to screen
pushad
mov dword edx, [staticfield_System_String_Kernel_PreReqs_PageFaultDetection_ErrorString]
push edx
call method_System_Void_RETEND_Kernel_BasicConsole_DECLEND_WriteLine_NAMEEND__Kernel_FOS_System_String_
add esp, 4
popad

;   1. Print the instruction address of the caller
;		- Print EIP from stack i.e. [ESP + 8*4 (for pushad) + 4 (for push ebp) + 4 (for push esp)]
;								   =[ESP+40]
;
mov dword edx, [esp+40]
call method_System_Void_RETEND_Kernel_PreReqs_DECLEND_PageFaultDetection_NAMEEND___PrintEDX

;   2. Print register value that caused failure
mov dword edx, eax
call method_System_Void_RETEND_Kernel_PreReqs_DECLEND_PageFaultDetection_NAMEEND___PrintEDX

;   3. If applicable: Print value at address that caused failure
mov dword edx, ebx
call method_System_Void_RETEND_Kernel_PreReqs_DECLEND_PageFaultDetection_NAMEEND___PrintEDX

;   4. Pause
pushad
push dword 10
call method_System_Void_RETEND_Kernel_BasicConsole_DECLEND_DelayOutput_NAMEEND__System_Int32_
add esp, 4
popad

;   5. Print out the complete stack trace from ESP to the top, 
;			1 dword(s) per line, pausing between every 4 lines printed
mov dword eax, esp
mov dword ecx, Kernel_Stack
sub ecx, esp
shr ecx, 2
mov ebx, ecx

mov dword edx, ebx
call method_System_Void_RETEND_Kernel_PreReqs_DECLEND_PageFaultDetection_NAMEEND___PrintEDX

pushad
push dword 20
call method_System_Void_RETEND_Kernel_BasicConsole_DECLEND_DelayOutput_NAMEEND__System_Int32_
add esp, 4
popad

mov edx, 0
mov [method_System_Void_RETEND_Kernel_PreReqs_DECLEND_PageFaultDetection_NAMEEND___PrintEDX_WriteLine], edx

.PRINT_STACK_LOOP:
; mov dword edx, ecx
mov dword edx, [eax]
call method_System_Void_RETEND_Kernel_PreReqs_DECLEND_PageFaultDetection_NAMEEND___PrintEDX

pushad
mov dword edx, [staticfield_System_String_Kernel_PreReqs_PageFaultDetection_SeparatorString]
push edx
call method_System_Void_RETEND_Kernel_BasicConsole_DECLEND_Write_NAMEEND__Kernel_FOS_System_String_
add esp, 4
popad


add eax, 4
dec ebx


mov edx, ebx
and edx, 3
cmp edx, 0
jnz .PRINT_STACK_LOOP_END
pushad
call method_System_Void_RETEND_Kernel_BasicConsole_DECLEND_WriteLine_NAMEEND___
popad

mov edx, ebx
and edx, 15
cmp edx, 0
jnz .PRINT_STACK_LOOP_END
pushad
push dword 10
call method_System_Void_RETEND_Kernel_BasicConsole_DECLEND_DelayOutput_NAMEEND__System_Int32_
add esp, 4
popad

.PRINT_STACK_LOOP_END:
loop .PRINT_STACK_LOOP

mov edx, 1
mov [method_System_Void_RETEND_Kernel_PreReqs_DECLEND_PageFaultDetection_NAMEEND___PrintEDX_WriteLine], edx

;   6. Pause execution for a long period of time
push dword 100
call method_System_Void_RETEND_Kernel_BasicConsole_DECLEND_DelayOutput_NAMEEND__System_Int32_
add esp, 4

mov dword edx, 0
mov [staticfield_System_Boolean_Kernel_PreReqs_PageFaultDetection_LoopPrevention], edx

.END2:
;   7. Clean up stack
;		: Reverse steps 1 and 2.
;			- Pop all registers from stack
popad
;			- Pop EBP from stack
pop dword ebp
;			- Pop ESP from stack
pop dword esp

; 8. Return
ret


method_System_Void_RETEND_Kernel_PreReqs_DECLEND_PageFaultDetection_NAMEEND___CheckEBX:

;	0xA5F000C8 (0xA5EFFFC8 - 0xA5F001C8)
;	0xF000FF69 (0xF000FE69 - 0xF0010069)

cmp ebx, 0xA5EFFFC8
jb .NOFAIL_1
cmp ebx, 0xA5F001C8
ja .NOFAIL_1
ret

.NOFAIL_1:
cmp ebx, 0xF000FE69
jb .NOFAIL_2
cmp ebx, 0xF0010069
ja .NOFAIL_2
ret

.NOFAIL_2:
mov dword ebx, 0
ret



method_System_Void_RETEND_Kernel_PreReqs_DECLEND_PageFaultDetection_NAMEEND___PrintEDX_WriteLine:
	dd 1

method_System_Void_RETEND_Kernel_PreReqs_DECLEND_PageFaultDetection_NAMEEND___PrintEDX:
pushad

; Note: This will cause a GC leak. One or more strings will be dynamically allocated and never released.

push dword edx
push dword 0
call method_Kernel_FOS_System_String_RETEND_Kernel_FOS_System_String_DECLEND_op_Implicit_NAMEEND__System_UInt32_
pop dword edx
add esp, 4

push dword edx
mov edx, [method_System_Void_RETEND_Kernel_PreReqs_DECLEND_PageFaultDetection_NAMEEND___PrintEDX_WriteLine]
cmp edx, 0
jnz .WriteLine
call method_System_Void_RETEND_Kernel_BasicConsole_DECLEND_Write_NAMEEND__Kernel_FOS_System_String_
jmp .Continue
.WriteLine:
call method_System_Void_RETEND_Kernel_BasicConsole_DECLEND_WriteLine_NAMEEND__Kernel_FOS_System_String_
.Continue:
add esp, 4

popad
ret
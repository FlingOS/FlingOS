BITS 32

SECTION .text
GLOBAL File_IDT:function
File_IDT:

EXTERN method_System_Void_RETEND_Kernel_Hardware_Interrupts_Interrupts_DECLEND_CommonISR_NAMEEND__System_UInt32_
EXTERN method_System_Void_RETEND_Kernel_PreReqs_DECLEND_WriteDebugVideo_NAMEEND__System_String_System_UInt32_
EXTERN method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_PageFaultException_NAMEEND__System_UInt32_System_UInt32_System_UInt32_
EXTERN method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_StackException_NAMEEND___
EXTERN method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_DoubleFaultException_NAMEEND__System_UInt32_System_UInt32_
EXTERN method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_OverflowException_NAMEEND___
EXTERN method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_OverflowException_NAMEEND___
EXTERN method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_DivideByZeroException_NAMEEND__System_UInt32_
EXTERN _NATIVE_IDT_Pointer
EXTERN _NATIVE_IDT_Contents
EXTERN staticfield_Kernel_Hardware_Processes_ThreadState__Kernel_Hardware_Processes_ProcessManager_CurrentThread_State
EXTERN staticfield_Kernel_ExceptionState__Kernel_ExceptionMethods_State
EXTERN staticfield_Kernel_ExceptionState__Kernel_Hardware_Interrupts_Interrupts_InterruptsExState
EXTERN staticfield_Kernel_ExceptionState__Kernel_ExceptionMethods_DefaultState
EXTERN _NATIVE_TSS
EXTERN BasicDebug_InterruptHandler


	; See comment at start of Main Entrypoint
	mov dword ecx, 0x0


%define KERNEL_MODE_DPL 0
%define USER_MODE_DPL 3

; START - General interrupt macros

%macro ENABLE_INTERRUPTS 0
sti
%endmacro



%macro DISABLE_INTERRUPTS 0
cli
nop
%endmacro


%assign STORE_STATE_SKIP_NUM 0
%assign RESTORE_STATE_SKIP_NUM 0

%macro INTERRUPTS_STORE_STATE 1
; Store registers on current thread stack
pushad
push ds
push es
push fs
push gs

; Switch the segment selectors to kernel mode selectors
mov ax, 0x10
mov gs, ax
mov fs, ax
mov es, ax
mov ds, ax

; Load pointer to current thread state
mov dword eax, [staticfield_Kernel_Hardware_Processes_ThreadState__Kernel_Hardware_Processes_ProcessManager_CurrentThread_State]
; Test for null
cmp eax, 0
; If null, skip
jz INTERRUPTS_STORE_STATE_SKIP_%1

; Check for UserMode process. If UM, we are already
;	on the kernel stack so don't change it or we will
;	lose the values saved in pushes above
; This takes the CS pushed by the processor when it
;	invoked the interrupt, gets the DPL then sees
;	if the DPL==3 i.e. User mode
mov dword ebx, [esp+52]
and ebx, 0x3
cmp ebx, 0x3
je INTERRUPTS_STORE_STATE_COPYACROSS_%1

; Save thread's current stack position
mov dword [eax+1], esp
; Load temp kernel stack address
mov dword ebx, [eax+7]
; Switch to temp kernel stack
mov dword esp, ebx

; Now running on a totally empty kernel stack

jmp INTERRUPTS_STORE_STATE_SKIP_%1

INTERRUPTS_STORE_STATE_COPYACROSS_%1:
; Load thread's UM stack position
mov dword ebx, [esp+60]
; Copy across all the values
sub ebx, 4
mov dword ecx, [esp+64]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+60]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+56]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+52]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+48]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+44]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+40]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+36]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+32]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+28]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+24]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+20]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+16]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+12]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+8]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+4]
mov dword [ebx], ecx
sub ebx, 4
mov dword ecx, [esp+0]
mov dword [ebx], ecx

; Store UM stack position
mov dword [eax+1], ebx

; Restore kernel stack to its proper place
add esp, 64

; Now running on a totally empty kernel stack

INTERRUPTS_STORE_STATE_SKIP_%1:

; Put in interrupts ex state
mov dword ebx, [staticfield_Kernel_ExceptionState__Kernel_Hardware_Interrupts_Interrupts_InterruptsExState]
mov dword [staticfield_Kernel_ExceptionState__Kernel_ExceptionMethods_State], ebx

%endmacro



%macro INTERRUPTS_RESTORE_STATE 1
; Load pointer to current thread state
mov dword eax, [staticfield_Kernel_Hardware_Processes_ThreadState__Kernel_Hardware_Processes_ProcessManager_CurrentThread_State]
; Test for null
cmp eax, 0
; If null, skip
jz INTERRUPTS_RESTORE_STATE_SKIP_%1


; Restore esp to thread's esp
mov dword esp, [eax+1]

; Load address of temp kernel stack
mov dword ebx, [eax+7]
; Update TSS with kernel stack pointer for next task switch
mov dword [_NATIVE_TSS+4], ebx

; Put in thread ex state
mov dword ebx, [eax+21]
mov dword [staticfield_Kernel_ExceptionState__Kernel_ExceptionMethods_State], ebx

jmp INTERRUPTS_RESTORE_STATE_SKIP_END_%1

INTERRUPTS_RESTORE_STATE_SKIP_%1:

; Put in default ex state
mov dword ebx, [staticfield_Kernel_ExceptionState__Kernel_ExceptionMethods_DefaultState]
mov dword [staticfield_Kernel_ExceptionState__Kernel_ExceptionMethods_State], ebx

INTERRUPTS_RESTORE_STATE_SKIP_END_%1:

pop gs
pop fs
pop es
pop ds
popad
%endmacro



; END - General interrupt macros



; BEGIN - Create IDT
; See MultibootSignature.x86_32.asm for memory allocations

; Set the Int1 handler
; Load handler address
mov dword eax, BasicDebug_InterruptHandler
; Set low address bytes into entry (index) 1 of the table
mov byte [_NATIVE_IDT_Contents + 8], al
mov byte [_NATIVE_IDT_Contents + 9], ah
; Shift the address right 16 bits to get the high address bytes
shr dword eax, 0x10
; Set the high address bytes
mov byte [_NATIVE_IDT_Contents + 14], al
mov byte [_NATIVE_IDT_Contents + 15], ah
; Set the code segment selector
mov word [_NATIVE_IDT_Contents + 10], 0x8
; Must always be 0
mov byte [_NATIVE_IDT_Contents + 12], 0x0
; Set the type and attributes: 0x8F =	   1111		0			00		1
;										Trap Gate	Always 0	DPL		Present
mov byte [_NATIVE_IDT_Contents + 13], 0x8F

; Set the Int3 handler
mov dword eax, BasicDebug_InterruptHandler
mov byte [_NATIVE_IDT_Contents + 24], al
mov byte [_NATIVE_IDT_Contents + 25], ah
shr dword eax, 0x10
mov byte [_NATIVE_IDT_Contents + 30], al
mov byte [_NATIVE_IDT_Contents + 31], ah
mov word [_NATIVE_IDT_Contents + 26], 0x8
mov byte [_NATIVE_IDT_Contents + 28], 0x0
mov byte [_NATIVE_IDT_Contents + 29], 0x8F

; Set remaining interrupt handlers

mov dword ebx, _NATIVE_IDT_Contents

mov dword eax, Interrupt0Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8E
add ebx, 8

; Skip Int1 - Set above
add ebx, 8

mov dword eax, Interrupt2Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8E
add ebx, 8

; Skip Int3 - Set above
add ebx, 8
 
mov dword eax, Interrupt4Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8E
add ebx, 8
  
mov dword eax, Interrupt5Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8E
add ebx, 8
  
mov dword eax, Interrupt6Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8E
add ebx, 8
  
mov dword eax, Interrupt7Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8E
add ebx, 8
  
mov dword eax, Interrupt8Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8E
add ebx, 8
  
mov dword eax, Interrupt9Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8E
add ebx, 8
  
mov dword eax, Interrupt10Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8E
add ebx, 8
  
mov dword eax, Interrupt11Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8E
add ebx, 8
  
mov dword eax, Interrupt12Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8E
add ebx, 8

; Skip 13 - Triple Faults occur after every IRet!  
add ebx, 8
  
mov dword eax, Interrupt14Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8E
add ebx, 8

; Skip 15 - Reserved (i.e. empty)
add ebx, 8 

mov dword eax, Interrupt16Handler
mov byte [ebx], al
mov byte [ebx+1], ah
shr dword eax, 0x10
mov byte [ebx+6], al
mov byte [ebx+7], ah
mov word [ebx+2], 0x8
mov byte [ebx+4], 0x0
mov byte [ebx+5], 0x8E
add ebx, 8

%macro CommonInterruptHandler_IDTMacro 2
    ; %1
	; Interrupt gate
    mov dword eax, CommonInterruptHandler%1
    mov byte [ebx], al
    mov byte [ebx+1], ah
    shr dword eax, 0x10
    mov byte [ebx+6], al
    mov byte [ebx+7], ah
    mov word [ebx+2], 0x8
    mov byte [ebx+4], 0x0
	; Use Interrupt gates not Trap gates!! Makes a massive
	; difference! If you use Trap gates, you'll get 
	; double faults as soon as you start using IRQs
	; in-combo with User Mode processes.
	;  And mark all of them as User/Kernel-mode accessible
    mov byte [ebx+5], (0x8E | (%2 << 5))
    add ebx, 8
%endmacro
%assign handlernum 17
%rep (32 - 17)
    CommonInterruptHandler_IDTMacro handlernum, USER_MODE_DPL
    %assign handlernum handlernum+1
%endrep
; IRQs should not be UM accessible
%rep 16
    CommonInterruptHandler_IDTMacro handlernum, KERNEL_MODE_DPL
    %assign handlernum handlernum+1
%endrep
%rep (256 - 48)
    CommonInterruptHandler_IDTMacro handlernum, USER_MODE_DPL
    %assign handlernum handlernum+1
%endrep

mov dword [_NATIVE_IDT_Pointer + 2], _NATIVE_IDT_Contents
mov dword eax, _NATIVE_IDT_Pointer
lidt [eax]
; END - Create IDT
jmp SkipIDTHandlers

; BEGIN - Proper exception handlers (i.e. they use the exceptions mechanism)

Interrupt0Handler:
push dword [esp]
call method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_DivideByZeroException_NAMEEND__System_UInt32_

Interrupt4Handler:
call method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_OverflowException_NAMEEND___
 
Interrupt6Handler:
call method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_OverflowException_NAMEEND___

Interrupt8Handler:
call method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_DoubleFaultException_NAMEEND__System_UInt32_System_UInt32_

Interrupt12Handler:
call method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_StackException_NAMEEND___

Interrupt14Handler:

DISABLE_INTERRUPTS

mov dword eax, CR2
push eax
call method_System_Void_RETEND_Kernel_ExceptionMethods_DECLEND_Throw_PageFaultException_NAMEEND__System_UInt32_System_UInt32_System_UInt32_
add esp, 8

ENABLE_INTERRUPTS
IRetd

; END - Proper exception handlers 

; BEGIN - Message-only Interrupt Handlers
 
Interrupt2HandlerMsg db 11, 0, 0, 0, 073, 110, 116, 101, 114, 114, 117, 112, 116, 032, 050
Interrupt2Handler:
	DISABLE_INTERRUPTS
pushad
mov dword eax, Interrupt2HandlerMsg
jmp MessageOnlyInterruptHandler

Interrupt5HandlerMsg db 11, 0, 0, 0, 073, 110, 116, 101, 114, 114, 117, 112, 116, 032, 053
Interrupt5Handler:
	DISABLE_INTERRUPTS
pushad
mov dword eax, Interrupt5HandlerMsg
jmp MessageOnlyInterruptHandler

Interrupt7HandlerMsg db 11, 0, 0, 0, 073, 110, 116, 101, 114, 114, 117, 112, 116, 032, 055
Interrupt7Handler:
	DISABLE_INTERRUPTS
pushad
mov dword eax, Interrupt7HandlerMsg
jmp MessageOnlyInterruptHandler
  
Interrupt9HandlerMsg db 11, 0, 0, 0, 073, 110, 116, 101, 114, 114, 117, 112, 116, 032, 057
Interrupt9Handler:
	DISABLE_INTERRUPTS
pushad
mov dword eax, Interrupt9HandlerMsg
jmp MessageOnlyInterruptHandler
 
Interrupt10HandlerMsg db 12, 0, 0, 0, 073, 110, 116, 101, 114, 114, 117, 112, 116, 032, 049, 048
Interrupt10Handler:
	DISABLE_INTERRUPTS
pushad
mov dword eax, Interrupt10HandlerMsg
jmp MessageOnlyInterruptHandler
 
Interrupt11HandlerMsg db 12, 0, 0, 0, 073, 110, 116, 101, 114, 114, 117, 112, 116, 032, 049, 049
Interrupt11Handler:
	DISABLE_INTERRUPTS
pushad
mov dword eax, Interrupt11HandlerMsg
jmp MessageOnlyInterruptHandler
 
Interrupt16HandlerMsg db 12, 0, 0, 0, 073, 110, 116, 101, 114, 114, 117, 112, 116, 032, 049, 054
Interrupt16Handler:
	DISABLE_INTERRUPTS
pushad
mov dword eax, Interrupt16HandlerMsg
jmp MessageOnlyInterruptHandler
 
Interrupt124HandlerMsg db 13, 0, 0, 0, 073, 110, 116, 101, 114, 114, 117, 112, 116, 032, 049, 050, 052
Interrupt124Handler:
	DISABLE_INTERRUPTS
pushad
mov dword eax, Interrupt124HandlerMsg
jmp MessageOnlyInterruptHandler

MessageOnlyInterruptHandler:

push dword ebp
mov dword ebp, esp

push ds
mov eax, 0x10
mov ds, ax
mov es, ax
mov gs, ax
mov fs, ax

push dword eax
push dword 0x02
call method_System_Void_RETEND_Kernel_PreReqs_DECLEND_WriteDebugVideo_NAMEEND__System_String_System_UInt32_
add esp, 8

mov ecx, 0x0F0FFFFF
MessageOnlyInterruptHandler.delayLoop1:
	nop
loop MessageOnlyInterruptHandler.delayLoop1

pop eax
mov ds, ax
mov es, ax
mov gs, ax
mov fs, ax

pop dword ebp

popad
IRetd

; END - Message-only Interrupt Handlers

; BEGIN - Common interrupt handlers

%macro CommonInterruptHandlerMacro 1
CommonInterruptHandler%1:

	DISABLE_INTERRUPTS

	INTERRUPTS_STORE_STATE STORE_STATE_SKIP_NUM
	%assign STORE_STATE_SKIP_NUM STORE_STATE_SKIP_NUM+1

	push dword %1
    call method_System_Void_RETEND_Kernel_Hardware_Interrupts_Interrupts_DECLEND_CommonISR_NAMEEND__System_UInt32_
    add esp, 4

	INTERRUPTS_RESTORE_STATE RESTORE_STATE_SKIP_NUM
	%assign RESTORE_STATE_SKIP_NUM RESTORE_STATE_SKIP_NUM+1
	
    IRetd
%endmacro
%assign handlernum2 17
%rep (256-17)
    CommonInterruptHandlerMacro handlernum2
    %assign handlernum2 handlernum2+1
%endrep

; END - Common interrupt handlers


SkipIDTHandlers:	
pic_remap:
; Remap IRQs 0-7    to    ISRs 32-39
; and   IRQs 8-15    to    ISRs 40-47

    ; Remap IRQ 0-15 to 32-47 (see http://wiki.osdev.org/PIC#Initialisation)
    ; Interrupt Vectors 0x20 for IRQ 0 to 7 and 0x28 for IRQ 8 to 15
    mov al, 0x11        ; INIT command
    out 0x20, al        ; send INIT to PIC1
    out 0xA0, al        ; send INIT to PIC2

    mov al, 0x20        ; PIC1 interrupts start at 0x20
    out 0x21, al        ; send the port to PIC1 DATA
    mov al, 0x28        ; PIC2 interrupts start at 0x28
    out 0xA1, al        ; send the port to PIC2 DATA

    mov al, 0x04        ; MASTER code
    out 0x21, al        ; set PIC1 as MASTER
    mov al, 0x02        ; SLAVE code
    out 0xA1, al        ; set PIC2 as SLAVE

    dec al              ; al is now 1. This is the x86 mode code for both 8259 PIC chips
    out 0x21, al        ; set PIC1
    out 0xA1, al        ; set PIC2

    dec al              ; al is now 0.
    out 0x21, al        ; set PIC1
    out 0xA1, al        ; set PIC2

	mov ax, 0xFFFF		; Set interrupt mask to disable all interrupts
    out 0x21, al        ; Set mask of PIC1_DATA
    xchg al, ah
    out 0xA1, al        ; Set mask of PIC2_DATA

	sti					; Enable interrupts
	nop					; Required - STI takes effect after the next instruction runs

	
	; IDT initialised.
	mov byte [0xB8320], 0x49
	mov byte [0xB8322], 0x44
	mov byte [0xB8324], 0x54
	mov byte [0xB8326], 0x20
	mov byte [0xB8328], 0x69
	mov byte [0xB832A], 0x6e
	mov byte [0xB832C], 0x69
	mov byte [0xB832E], 0x74
	mov byte [0xB8330], 0x69
	mov byte [0xB8332], 0x61
	mov byte [0xB8334], 0x6c
	mov byte [0xB8336], 0x69
	mov byte [0xB8338], 0x73
	mov byte [0xB833A], 0x65
	mov byte [0xB833C], 0x64
	mov byte [0xB833E], 0x2e	
	mov ecx, 0x00F00000
	.Loop1:
	loop .Loop1
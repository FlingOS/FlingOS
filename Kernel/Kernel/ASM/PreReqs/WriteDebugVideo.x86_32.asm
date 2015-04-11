BITS 32

SECTION .text
GLOBAL File_WriteDebugVideo:function
File_WriteDebugVideo:

GLOBAL method_System_Void_RETEND_Kernel_PreReqs_DECLEND_WriteDebugVideo_NAMEEND__System_String_System_UInt32_:function

; BEGIN - Write Debug Video
method_System_Void_RETEND_Kernel_PreReqs_DECLEND_WriteDebugVideo_NAMEEND__System_String_System_UInt32_:

; MethodStart
push dword ebp
mov dword ebp, esp

mov dword ebx, 0xB8000 ; Load vid mem base address

; Set num characters to clear 
mov dword eax, 80	; 80 characters per line
mov dword ecx, 25	; 25 lines
mul dword ecx		; eax * ecx, store in eax = numbers of characters to clear
mov dword ecx, eax	; Store number of characters to clear in ecx for later use in loop

mov byte ah, 0x00	; Set colour to clear to. Here black for background and foreground
mov byte al, 0		; Set the character to the null/empty character

.Loop1:
mov word [ebx], ax	; Move the empty character/colour to vid mem
add dword ebx, 2	; Move to next character space in vid mem
; Uses ecx - loops till ecx = 0 i.e. till all characters cleared
loop .Loop1


; Load string length
mov eax, [ebp+12]	 ; Load string address
mov dword ecx, [eax] ; String length is first dword of string

mov edx, [ebp+12]	; Load string address
add edx, 4			; Skip first dword because that is the length not a character

mov dword ebx, 0xB81E0 ; Load vid mem base address

mov byte ah, [ebp+8] ; Load colour

.Loop2:
mov byte al, [edx]		; Get current character of string
mov word [ebx], ax		; Move character and colour into vid mem
add dword ebx, 2		; Move to next character space in vid mem
add dword edx, 1		; Move to next character in string
; Uses ecx - loops till ecx = 0 i.e. till all characters gone through
loop .Loop2

; MethodEnd
pop dword ebp

ret
; END - Write Debug Video
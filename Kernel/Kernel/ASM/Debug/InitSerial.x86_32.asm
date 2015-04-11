; - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -  ;
;                                                                                ;
;               All contents copyright Edward Nutting 2014                       ;
;                                                                                ;
;        You may not share, reuse, redistribute or otherwise use the             ;
;        contents this file outside of the Fling OS project without              ;
;        the express permission of Edward Nutting or other copyright             ;
;        holder. Any changes (including but not limited to additions,            ;
;        edits or subtractions) made to or from this document are not            ;
;        your copyright. They are the copyright of the main copyright            ;
;        holder for all Fling OS files. At the time of writing, this             ;
;        owner was Edward Nutting. To be clear, owner(s) do not include          ;
;        developers, contributors or other project members.                      ;
;                                                                                ;
; - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -  ;

BITS 32

SECTION .text
GLOBAL BasicDebug_Serial_ComPortMemAddr:data

GLOBAL method_System_Void_RETEND_Kernel_Debug_BasicDebug_DECLEND_InitSerial_NAMEEND___:function


BasicDebug_Serial_ComPortMemAddr dw 0x03F8 ; COM1

method_System_Void_RETEND_Kernel_Debug_BasicDebug_DECLEND_InitSerial_NAMEEND___:

push dword ebp
mov dword ebp, esp

; Load the serial port mem address
mov dx, [BasicDebug_Serial_ComPortMemAddr]

; Disable serial interrupts
mov bx, dx
add dx, 1
mov al, 0
out dx, al

; Enable DLAB (baud rate divisor)
mov dx, bx
add dx, 3
mov al, 0x80
out dx, al

; Set Baud rate
; 115200
; Low byte
mov dx, bx
mov al, 0x01
out dx, al
; High byte
mov dx, bx
add dx, 1
mov al, 0x00
out dx, al

; Set 8 bits, no parity bit, 1 stop bit
mov dx, bx
add dx, 3
mov al, 0x03
out dx, al

; Enable FIFO, clear them
; Set 14-byte threshold for IRQ.
; We dont use IRQ, but you can't set it to 0
; either. IRQ is enabled/diabled separately
mov dx, bx
add dx, 2
mov al, 0xC7
out dx, al

; 0x20 AFE Automatic Flow control Enable - 16550 (VMWare uses 16550A) is most common and does not support it
; 0x02 RTS
; 0x01 DTR
; Send 0x03 if no AFE
mov dx, bx
add dx, 4
mov al, 0x03
out dx, al

pop dword ebp

ret
; END- BasicDebug.InitSerial()
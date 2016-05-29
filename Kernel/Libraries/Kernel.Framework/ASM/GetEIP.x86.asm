BITS 32

SECTION .text

GLOBAL GetEIP:function

; BEGIN - GetEIP

; NOTE: Leaves a "dirty stack" on purpose. The aim of this method is for EIP
;		to be on top of the stack after it returns.
GetEIP:
	push dword [ESP]
ret

; END  - GetEIP
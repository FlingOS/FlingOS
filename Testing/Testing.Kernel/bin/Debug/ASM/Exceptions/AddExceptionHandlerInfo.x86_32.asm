method_System_Void_RETEND_Kernel_Exceptions_DECLEND_AddExceptionHandlerInfo_NAMEEND__System_Void__System_Void__:

; 1. Push new ExceptionHandler structure to stack
; 2. Store return address
; 3. Shift new stack up items up over the top of arguments
; 4. Set CurrentHandlerPtr
; 5. Move ESP down to allow for caller's "add esp, NumArgBytes" after this returns
; 6. Push return address


; 1. Push new ExceptionHandler structure
; Push InHandler (as false i.e. 0)
push dword 0
; Push Previous Handler Address
push dword [staticfield_Kernel_ExceptionHandlerInfo__Kernel_Exceptions_CurrentHandlerPtr]
; Push Filter Address which is arg 1 (0-based indexing)
; Arg 1 = ESP + 4 (for Previous Handler Address) + 4 (for InHandler) + 4 (for ret address) = +12
push dword [ESP+12]
; Push Handler Address which is arg 0 (0-based indexing)
; Arg 0 = ESP + 4 (for Filter Address) + 4 (for PrevHandlerAddr) + 4 (for InHandler)  + 4 (for ret address)  + 4 (for arg 1) = +20
push dword [ESP+20]
; Push current EBP
push dword EBP
; Push ESP - this should be ESP after this function has completed
; So we need to calculate it from current ESP as though structure
; has already been shifted over existing args etc.
; So, current ESP comes from:
; ESP before call 
; - 4 (for arg 0) 
; - 4 (for arg 1) 
; - 4 (for ret address)
; - 4 (for InHandler)
; - 4 (for Prev Handler Address)
; - 4 (for Filter Address)
; - 4 (for Handler Address)
; - 4 (for EBP)
; = -32
; And ESP after this function has completed would be:
; ESP before call
; - 4 (for InHandler)
; - 4 (for Prev Handler Address)
; - 4 (for Filter Address)
; - 4 (for Handler Address)
; - 4 (for EBP)
; - 4 (for ESP)
; = -24
; The difference = ESP after - ESP before = (-24) - (-32) = +8
; Therefore, ESP to push = current ESP + 8
mov dword eax, esp
add dword eax, 8
push dword eax



; 2. Store return address
; Return address is now at: Current ESP + 24 (for exception handler)
mov dword edx, [esp+24]



; 3. Shift new stack items back up over the top of arguments
; ESP of arguments start (i.e. of arg 1) = 
; Current ESP + 24 (for exception handler) + 4 (for return address) + 4 (arg 0) = ESP + 32
; 
; ESP of exception handler start (i.e. of Previous Handler Address) = 
; Current ESP + 20 (for ESP, EBP, Handler Address, Filter Address, InHandler) = ESP + 20
; So, 
; [ESP+32] = [ESP+20]
; [ESP+28] = [ESP+16]
; [ESP+24] = [ESP+12]
; [ESP+20] = [ESP+8]
; [ESP+16] = [ESP+4]
; [ESP+12] = [ESP+0]

mov eax, [ESP+20]
mov [ESP+32], eax

mov eax, [ESP+16]
mov [ESP+28], eax

mov eax, [ESP+12]
mov [ESP+24], eax

mov eax, [ESP+8]
mov [ESP+20], eax

mov eax, [ESP+4]
mov [ESP+16], eax

mov eax, [ESP+0]
mov [ESP+12], eax



; 4. Set CurrentHandlerPtr
; Set to point to bottom of structure i.e. at "ESP"
; "ESP" is at ESP+12 (last moved value), thus:
mov eax, esp
add eax, 12
mov [staticfield_Kernel_ExceptionHandlerInfo__Kernel_Exceptions_CurrentHandlerPtr], eax



; 5. Move ESP down to allow for caller's "add esp, NumArgBytes" after this returns
; Move esp back past our new, now not-needed local variables
; Last move was to ESP+12 so we need to remove 3 dwords = +12
; add esp, 12
; Shift back down for args as per original intentions
; sub esp, 8 ; -4 (for arg0) -4 (for arg1) = -8
; NOTE: Net result of above would be a +4 shift, hence the following line.
add esp, 4


; 6. Push return address
push dword edx

ret
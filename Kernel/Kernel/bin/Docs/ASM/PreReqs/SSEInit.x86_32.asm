; BEGIN - SSE Init
mov dword EAX, CR4
or dword EAX, 0x100
mov dword CR4, EAX
mov dword EAX, CR4
or dword EAX, 0x200
mov dword CR4, EAX
mov dword EAX, CR0
and dword EAX, 0xFFFFFFFD
mov dword CR0, EAX
mov dword EAX, CR0
and dword EAX, 0x1
mov dword CR0, EAX
; END - SSE Init
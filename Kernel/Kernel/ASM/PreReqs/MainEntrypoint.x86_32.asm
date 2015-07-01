BITS 32

SECTION .text
GLOBAL File_MainEntryPoint:function
File_MainEntryPoint:


; Hmm... something has to go before the call op otherwise the OS crashes at boot.
;		It can be min. ~12 NOPs or this mov or probably many other things...
;	This is the same phenomenon observed at the start of VirtualMemInit
mov dword ecx, 0x0

; BEGIN - Main Entrypoint
call __MAIN_ENTRYPOINT__ ; Call our main entry point 

EXTERN %KERNEL_CALL_STATIC_CONSTRUCTORS_METHOD%
EXTERN %KERNEL_MAIN_METHOD%
EXTERN method_System_Void_Kernel_PreReqs_Reset__

__MAIN_ENTRYPOINT__:
	push dword ebp
	mov dword ebp, esp
	
	
	; Calling static constructors...
	mov byte [0xB83C0], 0x43
	mov byte [0xB83C2], 0x61
	mov byte [0xB83C4], 0x6c
	mov byte [0xB83C6], 0x6c
	mov byte [0xB83C8], 0x69
	mov byte [0xB83CA], 0x6e
	mov byte [0xB83CC], 0x67
	mov byte [0xB83CE], 0x20
	mov byte [0xB83D0], 0x73
	mov byte [0xB83D2], 0x74
	mov byte [0xB83D4], 0x61
	mov byte [0xB83D6], 0x74
	mov byte [0xB83D8], 0x69
	mov byte [0xB83DA], 0x63
	mov byte [0xB83DC], 0x20
	mov byte [0xB83DE], 0x63
	mov byte [0xB83E0], 0x6f
	mov byte [0xB83E2], 0x6e
	mov byte [0xB83E4], 0x73
	mov byte [0xB83E6], 0x74
	mov byte [0xB83E8], 0x72
	mov byte [0xB83EA], 0x75
	mov byte [0xB83EC], 0x63
	mov byte [0xB83EE], 0x74
	mov byte [0xB83F0], 0x6f
	mov byte [0xB83F2], 0x72
	mov byte [0xB83F4], 0x73
	mov byte [0xB83F6], 0x2e
	mov byte [0xB83F8], 0x2e
	mov byte [0xB83FA], 0x2e
	mov ecx, 0x00F00000
	.Loop1:
	loop .Loop1


	call %KERNEL_CALL_STATIC_CONSTRUCTORS_METHOD% ; Call the static constructors - this is a macro used by the kernel compiler.
	
	; done.
	mov byte [0xB83FC], 0x64
	mov byte [0xB83FE], 0x6f
	mov byte [0xB8400], 0x6e
	mov byte [0xB8402], 0x65
	mov byte [0xB8404], 0x2e
	
	; Calling kernel main method...
	mov byte [0xB8460], 0x43
	mov byte [0xB8462], 0x61
	mov byte [0xB8464], 0x6c
	mov byte [0xB8466], 0x6c
	mov byte [0xB8468], 0x69
	mov byte [0xB846A], 0x6e
	mov byte [0xB846C], 0x67
	mov byte [0xB846E], 0x20
	mov byte [0xB8470], 0x6b
	mov byte [0xB8472], 0x65
	mov byte [0xB8474], 0x72
	mov byte [0xB8476], 0x6e
	mov byte [0xB8478], 0x65
	mov byte [0xB847A], 0x6c
	mov byte [0xB847C], 0x20
	mov byte [0xB847E], 0x6d
	mov byte [0xB8480], 0x61
	mov byte [0xB8482], 0x69
	mov byte [0xB8484], 0x6e
	mov byte [0xB8486], 0x20
	mov byte [0xB8488], 0x6d
	mov byte [0xB848A], 0x65
	mov byte [0xB848C], 0x74
	mov byte [0xB848E], 0x68
	mov byte [0xB8490], 0x6f
	mov byte [0xB8492], 0x64
	mov byte [0xB8494], 0x2e
	mov byte [0xB8496], 0x2e
	mov byte [0xB8498], 0x2e
		
	call %KERNEL_MAIN_METHOD% ; Call our main method - this is a macro used by the kernel compiler.
	
	; We shouldn't ever get to this point! But just in case we do...
	jmp method_System_Void_Kernel_PreReqs_Reset__ ; For now this is our intended behaviour

; END - Main Entrypoint
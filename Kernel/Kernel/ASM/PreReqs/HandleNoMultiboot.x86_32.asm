; BEGIN - Handle No Multiboot
jmp Kernel_Start_HandleNoMultiboot_End ; Skip over this code - we don't want to run it by accident!

Kernel_Start_HandleNoMultiboot:

; Not entirely sure if we'd ever actually get as far as due to code structure but anyway...
; Displays a warning message to the user saying "No multiboot" indicating the multiboot signature
; (which should have been in eax) was not deteced so we don't think we have a valid boot setup
; so we are aborting the boot to avoid damage
	
	; Output following text to first bit of vid mem
	; N	  o      M  u    l   t   i   b   o  o   t
	; 78 111 32 109 117 108 116 105 98 111 111 116
	mov byte [0xB8000], 78
	mov byte [0xB8002], 111
	mov byte [0xB8004], 32
	mov byte [0xB8006], 109
	mov byte [0xB8008], 117
	mov byte [0xB800A], 108
	mov byte [0xB800C], 116
	mov byte [0xB800E], 105
	mov byte [0xB8010], 98
	mov byte [0xB8012], 111
	mov byte [0xB8014], 111
	mov byte [0xB8016], 116

	; Set the colour of the outputted text to:
	; Red background (0x4-), 
	; White foreground (0x-F)
	mov dword eax, 0x4F
	mov byte [0xB8001], al
	mov byte [0xB8003], al
	mov byte [0xB8005], al
	mov byte [0xB8007], al
	mov byte [0xB8009], al
	mov byte [0xB800B], al
	mov byte [0xB800D], al
	mov byte [0xB800F], al
	mov byte [0xB8011], al
	mov byte [0xB8013], al
	mov byte [0xB8015], al
	mov byte [0xB8017], al

	cli ; Prevent any more interrupt requests re-awakening us
	hlt ; Halt the OS / execution / etc.
	jmp Kernel_Start_HandleNoMultiboot ; Just in case...
Kernel_Start_HandleNoMultiboot_End:
; END - Handle No Multiboot
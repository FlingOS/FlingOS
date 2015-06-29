BITS 32

SECTION .text
GLOBAL File_InitStack:function
File_InitStack:

EXTERN Kernel_Stack

; BEGIN - Init stack

	mov dword eax, 0x5F
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
	mov ecx, 0x0F000000
	.Loop1:
	loop .Loop1

mov dword ESP, Kernel_Stack ; Set the stack pointer to point at our pre-allocated block of memory


	mov dword eax, 0x6F
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
	mov ecx, 0x0F000000
	.Loop2:
	loop .Loop2
; END - Init stack
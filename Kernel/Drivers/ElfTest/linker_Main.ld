OUTPUT_FORMAT(elf32-i386)

GROUP("ELFTest_Main.o")
GROUP(-lELFTest)

SECTIONS {
	.text : AT(ADDR(.text)) {
          * (.text);
          * (.rodata*);
	}

   . = ALIGN(0x1000);
   .data : AT(ADDR(.data)) {
          * (.data*);
	}

   . = ALIGN(0x1000);
   .bss : AT(ADDR(.bss)) {
          * (.bss*);
	}
}


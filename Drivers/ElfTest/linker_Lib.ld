OUTPUT_FORMAT(elf32-i386)

GROUP("ELFTest_lib.o")

SECTIONS {
	. = 0x40000000;
	
	.text : {
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


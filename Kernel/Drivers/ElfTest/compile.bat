@echo off
nasm -f elf ELFTest_Lib.asm
nasm -f elf ELFTest_Main.asm
cygwin\ld -shared -o LibELFTest.a ELFTest_lib.o
cygwin\ld -L . -l ELFTest -o ELFTest.elf ELFTest_Main.o
@echo on
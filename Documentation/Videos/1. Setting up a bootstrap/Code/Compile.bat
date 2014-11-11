@echo OFF

echo Deleting existing files...
del Bootstrap.iso

echo NASM...
NASM\nasm.exe -g -f elf -o "Bootstrap.obj" -DELF_COMPILATION "Bootstrap.asm"

echo Ld...
cygwin\ld.exe -T "cygwin\linker.ld" -e Kernel_Start -o "Bootstrap.bin" "Bootstrap.obj"

echo Copy bin...
copy /y "Bootstrap.bin" "ISO\OS.bin"

echo ISO Generator...
ISOGen\ISO9660Generator.exe 4 "%CD%\Bootstrap.iso" "%CD%\ISO\isolinux.bin" true "%CD%\ISO/"

echo Deleting intermediates...
del Bootstrap.bin
del ISO\OS.bin
del Bootstrap.obj

echo Done.

@echo ON
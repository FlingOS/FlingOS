/* ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//  Project owner: 
//		Email: edwardnutting@outlook.com
//		For paper mail address, please contact via email for details.
//
// ------------------------------------------------------------------------------ */

.extern %KERNEL_MAIN_METHOD%
.extern %KERNEL_CALL_STATIC_CONSTRUCTORS_METHOD%

.text
.set noreorder

.globl Kernel_Start
.ent Kernel_Start
Kernel_Start:
	nop
	
	/* Enable caching in kseg0 */
	li      $t0, 3 /* CACHE_MODE_CACHABLE_NONCOHERENT */
	mtc0    $t0, $16 /* CP0_CONFIG */
	nop

	/* Set up a stack */
	li $sp, 0x8f800000 
	
	jal %KERNEL_CALL_STATIC_CONSTRUCTORS_METHOD%
	nop
	
	jal %KERNEL_MAIN_METHOD%
	nop	
	
	j $ra
	.end Kernel_Start

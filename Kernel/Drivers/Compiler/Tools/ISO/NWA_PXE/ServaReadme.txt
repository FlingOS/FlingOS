Serva (Non-Supporter) v2.1.4 by Patrick Masotta © 2010-2013

This directory has to be populated with Non-Windows Install Distributions: 
Linux, DOS/FreeDOS, etc. 
Each Non-Windows Install Distribution has to be entirely "copied" under a 
user defined "head" directory which name cannot contain non-ASCII characters 
nor spaces either. i.e. 

…\ 
   NWA_PXE\ 
           Ubuntu_12.10.Dsk\ 
                   .disk\... 
                   boot\... 
                   casper\... 
                   ... 
                   ServaAsset.inf 

           Mint_13.Xf\ 
                   .disk\... 
                   boot\... 
                   casper\... 
                   ... 
                   ServaAsset.inf 

           FreeDos\ 
                   ... 
                   ServaAsset.inf 

        ^    ^       ^ 
        |    |       | 
This dir"    |       | 
             |       | 
User defined |       | 
Head dir   --'       | 
                     | 
Non-Windows Install  | 
Distribution   ------' 
 
Head directory names prepended by "off_" (w/o quotes) are ignored by Serva's
BINL engine.

IMPORTANT: 
The BINL management of Non-Windows Assets (NWA) requires the manual creation 
of The file ServaAsset.inf within every head directory containing i.e. 
 
ServaAsset.inf----------------------- 

[PXESERVA_MENU_ENTRY] 
asset    =   	Ubuntu 12.10 Desktop Live 
platform = 	x86 
kernel   =	NWA_PXE/$HEAD_DIR$/casper/vmlinuz 
append   =	showmounts toram root=/dev/cifs initrd=NWA_PXE/$HEAD_DIR$/casper/initrd.lz boot=casper netboot=cifs nfsroot=//$IP_BSRV$/NWA_PXE_SHARE/$HEAD_DIR$ NFSOPTS=-ouser=serva,pass=avres,ro ip=$IP_CLNT$:$IP_BSRV$:$IP_GWAY$:$IP_MASK$:::none ro 

------------------------------------- 
 
The former example considers the directory NWA_PXE is MS shared as 
NWA_PXE_SHARE with user=serva  password=avres 
 
ServaAsset.inf "kernel" and "append" variables are parsed replacing the 
following case insensitive tokens: 
 
Token          Replaced with: 
$COMP_NAME$    Serva Computer Name   i.e. M9 
$HEAD_DIR$     Asset Head Directory  i.e. Ubuntu_12.10.Dsk 
$IP_CLNT$      DHCP obtained (yiaddr) Client's IP 
$IP_BSRV$      DHCP obtained (siaddr) Client's Boot Server IP (Serva) 
$IP_GWAY$      DHCP obtained (opt #3) Client's Default Gateway 
$IP_MASK$      DHCP obtained (opt #1) Client's IP Mask 


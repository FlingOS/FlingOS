Serva (Non-Supporter) v2.1.4 by Patrick Masotta © 2010-2013

This directory has to be populated with WDS Windows Install Distributions: 
Windows Vista, Windows Server 2008, Windows 7, Windows 8, etc (32/64) 
It can also be populated by bootable WIM files. 
Each Windows Install Distribution has to be entirely "copied" under a 
user defined "head" directory which name cannot contain non-ASCII characters 
nor spaces either. 
Bootable WIM files must be accompanied by boot.sdi and also by pxeboot.n12 and 
bootmgr.exe when these files are not already included within the WIM. 
i.e. 
…\ 
   WIA_WDS\ 
           W8_Ent_32\ 
                   BOOT\... 
                   SOURCES\... 
                   SUPPORT\... 
                   ... 

           W7_64\ 
                   BOOT\... 
                   SOURCES\... 
                   SUPPORT\... 
                   ... 

           WinPE_64\ 
                   winpe_64.wim 
                   pxeboot.n12 
                   bootmgr.exe 
                   boot.sdi 

           Unattend.ini 

        ^    ^       ^ 
        |    |       | 
This dir"    |       | 
             |       | 
User defined |       | 
Head dir   --'       | 
                     | 
Windows Install      | 
Distribution or -----' 
Bootable WIM 

Head directory names prepended by "off_" (w/o quotes) are ignored by Serva's
BINL engine.

IMPORTANT: When booting Windows Install Distributions, directory WIA_WDS has 
to be shared as WIA_WDS_SHARE (not required for booting bootable WIMs). 

OPTIONAL: Unattend.ini allows automatic ServaPENet login (only Supporter) 

Unattend.ini ------------------------ 

[windowsPE-Setup-Login-Credentials] 
Domain   = WIA_WDS_SHARE_DomainName (optional) 
Password = WIA_WDS_SHARE_userName 
Username = WIA_WDS_SHARE_userPassword 

------------------------------------- 


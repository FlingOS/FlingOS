Serva (Non-Supporter) v2.1.4 by Patrick Masotta © 2010-2013

This directory has to be populated with RIS Windows Install Distributions: 
Windows 2000, Windows XP (32/64), and Windows Server 2003 (32/64). 
Each Windows Install Distribution has to be entirely "copied" under a 
user defined "head" directory which name cannot contain non-ASCII characters 
nor spaces either. i.e. 

…\ 
   WIA_RIS\ 
           xp_32\ 
                   I386\... 
                   PRINTERS\... 
                   SUPPORT\... 
                   ... 
           XP_64\ 
                   I386\... 
                   AMD64\... 
                   PRINTERS\... 
                   SUPPORT\... 
                   ... 
           W2000AS\ 
                   I386\... 
                   PRINTERS\... 
                   SUPPORT\... 
                   ... 
 
        ^    ^       ^ 
        |    |       | 
This dir"    |       | 
             |       | 
User defined |       | 
Head dir   --'       | 
                     | 
Windows Install      | 
Distribution   ------' 
 
Head directory names prepended by "off_" (w/o quotes) are 
ignored by Serva's BINL engine.


Note: For 64 bit OSs we need to take one extra step 
1) "Copy" the content \AMD64\*.* (about 400 Mb) to I386\ 
 (it implies merging the content of the \LANG directories) 
 
2) Optionally if we want to save HDD space we can:  
 a) Erase the AMD64 directory with all its content. 
 b) Make a junction i.e.  
    C:\>junction.exe C:\xxxxx\WIA_RIS\XP_64\AMD64 C:\xxxxx\WIA_RIS\XP_64\I386 
 
 
IMPORTANT: WIA_RIS' parent directory (TFTP Server Root directory) 
has to be shared as WIA_RIS_SHARE using a "Null Session Share". 
Please consider this will (by default) expose to "ANONYMOUS LOGON" 
users WIA_WDS' content. This probably unwanted situation can be solved 
by editing WIA_WDS' default sharing permits after WIA_RIS_SHARE is 
created. 
 

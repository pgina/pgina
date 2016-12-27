How to compile dependent libs

Best to use a Windows 7 Virtual Machine!

.) Install VC 2010 as described here
      http://mutonufoai.github.io/pgina/develop.html
.) Install NASM
      http://www.nasm.us/
.) Install Active Perl
      add perl into the path variable
      http://www.activestate.com/activeperl/downloads
.) Install cmake
      add cmake into the path variable
      https://cmake.org/download/
.) create a folder and
   .) extract libssh2 into it
         https://www.libssh2.org/download/
   .) extract zlib into it
         http://www.zlib.net/
         remove ZLIB_WINAPI from PreprocessorDefinitions in contrib\vstudio\vc10\zlibstat.vcxproj
         (needed to build win32)
   .) extract openssl into it
         https://www.openssl.org/source/
   .) copy dependency_compile.cmd into it
.) open Visual Studio Command Promp for a 32 bit build
.) drag and drop dependency_compile.cmd into it and hit enter

.) open Visual Studio Command Promp for a 64 bit build
.) drag and drop dependency_compile.cmd into it and hit enter

Then you'll find a x86 and x64 folder with all the libs you need. 
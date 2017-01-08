#
#  Build instructions:
#
#    1. Set the CC := the compiler path
#    2. Set the NUGET path to fetch the dependencies.
#  
#    - To build the executable pget.exe enter $ make exe 
#    - To buld the dll library pget.dll enter $ make lib
#
# 
#


# ============ U S E R - S E T T I N G S =========== #

## F# Compiler Location - Change this for your platform 
# CC := /usr/lib/mono/4.5/fsc.exe # (Mono)
CC := fsc

# Nuget client - Change this for your platform 
#
#NUGET := ~/bin/nuget.exe
NUGET := nuget.exe



# ================ S O U R C E =========================== #

# Source code of Pget.exe application.
exe-src := src/Pget.fs src/FXml.fs  src/AsmAttr.fs src/AssemblyInfo.fs src/PgetCmd.fs


# .NET References 
NuGet.Core        := packages/NuGet.Core.2.12.0/lib/net40-Client/NuGet.Core.dll
Microsoft.Web.Xdt := packages/Microsoft.Web.Xdt.2.1.1/lib/net40/Microsoft.Web.XmlTransform.dll


# ================  R U L E S ============================ # 

all: help

help:
	@echo "Enter $ make lib - to build a library src/bin/pget.dll"
	@echo "Enter $ make exe - to build command line application src/bin/pget.exe"


lib:= bin/pget.dll
lib: $(lib)


## Standalone command line application
##
exe := bin/pget.exe
exe: $(exe)

## Publish release to git orphan-branch build and upload to Github
#
exe-release: exe
	mkdir -p ./release 
	cp $(exe) ./release
	cd ./release && git add pget.exe && git commit -m "New binary release" && git push


# Static link all dependencies and F# runtime to app.
#
$(exe): $(exe-src) $(NuGet.Core) $(Microsoft.Web.Xdt)
	$(CC) $(exe-src)  --out:$(exe) \
	--target:exe \
        --platform:anycpu \
        -r:$(NuGet.Core) \
	-r:$(Microsoft.Web.Xdt) \
	--staticlink:NuGet.Core \
	--standalone

# Install pget.exe to ~/bin 
exe-install: exe
	cp $(exe) ~/bin



# Compile the library using xbuild .fsproj file.
#
xbuild:
	xbuild 


$(NuGet.Core):
	$(NUGET) install NuGet.Core -OutputDirectory packages -Version 2.12.0



$(lib): src/Pget.fs src/PgetCmd.fsx $(NuGet.Core) $(Microsoft.Web.Xdt)
	$(CC) src/Pget.fs --out:$(lib) \
		--target:library \
		--platform:anycpu \
		-r:$(NuGet.Core) \
		-r:$(Microsoft.Web.Xdt)
# --staticlink:NuGet.Core \
# --standalone

# mkdir -p src/bin
	cp -v $(NuGet.Core) src/bin/
	cp -v $(Microsoft.Web.Xdt) src/bin/


clean:
	rm -rf bin/* obj/*

clean-packages:
	rm -rf packages/*

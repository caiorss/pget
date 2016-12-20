
all: help

help:
	@echo "Enter $ make lib - to build a library src/bin/pget.dll"
	@echo "Enter $ make app - to build command line application src/bin/pget.exe"


## F# Compiler Location - Change this for your platform 
#
CC := /usr/lib/mono/4.5/fsc.exe

# Nuget client - Change this for your platform 
#
NUGET := ~/nuget.exe


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



# Compile the library using xbuild .fsproj file.
#
xbuild:
	xbuild 

NuGet.Core        := packages/NuGet.Core.2.12.0/lib/net40-Client/NuGet.Core.dll
Microsoft.Web.Xdt := packages/Microsoft.Web.Xdt.2.1.1/lib/net40/Microsoft.Web.XmlTransform.dll

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


$(exe): src/Pget.fs src/PgetCmd.fs src/AssemblyInfo.fs $(NuGet.Core) $(Microsoft.Web.Xdt)
	$(CC) src/AssemblyInfo.fs src/Pget.fs src/PgetCmd.fs  --out:$(exe) \
	--target:exe \
    --platform:anycpu \
    -r:$(NuGet.Core) \
	-r:$(Microsoft.Web.Xdt) \
	--staticlink:NuGet.Core \
	--standalone

# Install pget.exe to ~/bin 
exe-install: exe
	cp $(exe) ~/bin

release: $(build)
	rm -rf pget.zip
	cp  -r bin  pget
	zip -r pget.zip pget
	rm  -rf pget
	echo "Build release pget.zip Ok."

clean:
	rm -rf bin/* obj/*

clean-packages:
	rm -rf packages/*

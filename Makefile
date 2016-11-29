build := bin/pget.exe

all: $(build)

NuGet.Core        := packages/NuGet.Core.2.12.0/lib/net40-Client/NuGet.Core.dll                  
Microsoft.Web.Xdt := packages/Microsoft.Web.Xdt.2.1.1/lib/net40/Microsoft.Web.XmlTransform.dll

$(NuGet.Core):	
	nuget.exe install NuGet.Core -OutputDirectory packages -Version 2.12.0

$(build): pget.fsx $(NuGet.Core) $(Microsoft.Web.Xdt)
	fsc pget.fsx --out:$(build) \
		--target:exe \
		--standalone \
		--platform:anycpu \
		-r:$(NuGet.Core) \
		-r:$(Microsoft.Web.Xdt)

	mkdir -p bin
	cp -v $(NuGet.Core) bin/
	cp -v $(Microsoft.Web.Xdt) bin/


release: $(build)
	rm -rf pget.zip
	cp  -r bin  pget
	zip -r pget.zip pget
	rm  -rf pgetF
	echo "Build release pget.zip Ok."

clean:
	rm -rf bin/*

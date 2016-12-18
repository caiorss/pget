library := bin/pget.dll

all: $(build)

NuGet.Core        := packages/NuGet.Core.2.12.0/lib/net40-Client/NuGet.Core.dll                  
Microsoft.Web.Xdt := packages/Microsoft.Web.Xdt.2.1.1/lib/net40/Microsoft.Web.XmlTransform.dll

$(NuGet.Core):	
	nuget.exe install NuGet.Core -OutputDirectory packages -Version 2.12.0

lib: $(library)

$(library): src/Pget.fs $(NuGet.Core) $(Microsoft.Web.Xdt)
	fsc src/Pget.fs --out:src/$(library) \
		--target:exe \
		--standalone \
		--platform:anycpu \
		-r:$(NuGet.Core) \
		-r:$(Microsoft.Web.Xdt) \
		--staticlink:NuGet.Core \
		--standalone

	# mkdir -p src/bin
	cp -v $(NuGet.Core) src/bin/
	cp -v $(Microsoft.Web.Xdt) src/bin/


release: $(build)
	rm -rf pget.zip
	cp  -r bin  pget
	zip -r pget.zip pget
	rm  -rf pget
	echo "Build release pget.zip Ok."

clean:
	rm -rf bin/*

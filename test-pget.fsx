#load "pget.fsx"

open Pget

let repo = Repo.localRepository "packages" ;;



> let pack = Option.get <| Repo.findPackageById repo "FSharp.Data" ;;

val pack : IPackage = FSharp.Data 2.3.2

IPack.getDllFilesRefsCompatible "packages" ".NETFramework,Version=v4.0" pack ;;
val it : seq<string> =
  seq
    ["packages/FSharp.Data.2.3.2/lib/net40/FSharp.Data.dll";
     "packages/FSharp.Data.2.3.2/lib/net40/FSharp.Data.DesignTime.dll"]
>


> IPack.getDllFilesRefsCompatible "packages" ".NETFramework,Version=v4.5" pack 
|> Seq.iter (fun p -> Console.WriteLine p)
;;
packages/FSharp.Data.2.3.2/lib/net40/FSharp.Data.dll
packages/FSharp.Data.2.3.2/lib/net40/FSharp.Data.DesignTime.dll
packages/FSharp.Data.2.3.2/lib/portable-net45+sl50+netcore45/FSharp.Data.dll
packages/FSharp.Data.2.3.2/lib/portable-net45+sl50+netcore45/FSharp.Data.DesignTime.dll
packages/FSharp.Data.2.3.2/lib/portable-net45+netcore45/FSharp.Data.dll
packages/FSharp.Data.2.3.2/lib/portable-net45+netcore45/FSharp.Data.DesignTime.dll
packages/FSharp.Data.2.3.2/lib/portable-net45+netcore45+wpa81+wp8/FSharp.Data.dll
packages/FSharp.Data.2.3.2/lib/portable-net45+netcore45+wpa81+wp8/FSharp.Data.DesignTime.dll
val it : unit = ()
> 

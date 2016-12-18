
#if INTERACTIVE
#r "packages/NuGet.Core/lib/net40-Client/NuGet.Core.dll"
#r "packages/Microsoft.Web.Xdt/lib/net40/Microsoft.Web.XmlTransform.dll"
#r "System.Linq.dll"
#endif

open System 

#load "Pget.fs"
open System.Linq 

let packman = new NuGet.PackageManager (Pget.Nuget.nugetV2, "packages")

packman.




Pget.Local.showPackages "/home/arch/nuget"

Pget.RepoLocal.showPackageList "/home/arch/nuget"

Pget.IPack.getDllFilesRefsCompatible  "/home/arch/nuget" ".NETFramework,Version=v4.5" 

Pget.RepoLocal.getPackageRefs "/home/arch/nuget" "OxyPlot.Core" Pget.Framework.net45


let repo = Pget.Local.localRepository "/home/arch/nuget"

Pget.Repo.findPackagesById repo "OxyPlot.Core" 

Pget.IPack.showPackage pack 

let rpath = "/home/arch/nuget"
let repo  = Pget.RepoLocal.localRepository rpath 

let  pack = Pget.RepoLocal.searchPackageById rpath "plot" |> Seq.item 3


let pack = Pget.IPack.getDllFilesPathCompatible pack Pget.Framework.net45 |> Seq.last 




|> Seq.iter Pget.IPack.showPackage


let pack =


Pget.RepoLocal.searchPackageById rpath "google" |> Seq.toList
          

let pack = Pget.RepoLocal.searchPackageById rpath "XPlot.GoogleCharts" |> Seq.last 


Pget.IPack.getDependencies pack |> Seq.toList |> List.map snd


pack |> Pget.IPack.getDependenciesAsString 

let mv = pack.DependencySets |> Seq.item 0

mv.Dependencies |> Seq.map (fun p -> (p.Id, match p.VersionSpec with
                                            | null -> None
                                            | v    -> Some v
                                      )) |> Seq.toList



mv.Dependencies |> Seq.map (fun p -> (p.Id, match p.VersionSpec with
                                            | null -> None
                                            | v    -> Some v
                                      )) |> Seq.toList


pack.DependencySets
|> Seq.map (fun dep -> dep.Dependencies)
|> Seq.collect (Seq.map ( fun p -> (p.Id, match p.VersionSpec with
                                          | null -> None
                                          | v    -> Some v
                                    )))


|> Seq.map (fun (ps: NuGet.PackageDependencySet) -> ps.Dependencies)


Pget.IPack.getDllFilesRefsCompatible "." Pget.Framework.net45


Pget.RepoLocal.getLocalRepoRefs "packages" Pget.Framework.net45
|> Seq.iter (fun p -> Console.WriteLine(p.ToString()))


Pget.RepoLocal.getRefs "packages" Pget.Framework.net45
|> Seq.iter (fun p -> Console.WriteLine(p))

Pget.RepoLocal.getPackageRefsNoVersion "packages" Pget.Framework.net45 "FParsec"



Pget.RepoLocal.installPackageLatest "packages" "FSharp.Charting.Gtk"

NuGet.VersionUtility.TryGetCompatibleItems(
    NuGet.VersionUtility.ParseFrameworkFolderName(".NETFramework,Version=v4.5"),

    )


let repo2 = new NuGet.LocalPackageRepository("/home/arch/Documents/projects/pget.fsharp/packages")


repo2.FindPackages("NuGet")


let repository = NuGet.PackageRepositoryFactory.Default.CreateRepository "https://nuget.org/api/v2"



query {
    for p in repository.GetPackages() do
    where (p.Title.Contains "chart" || p.Title.Contains "plot" || p.Title.Contains "graph")
    sortBy p.DownloadCount 
    take 10 
    // yield p   
    yield p 
    } |> Seq.iter  Pget.IPack.showPackage 



/// Search packages that contains at least one word of a list of words.
/// 
let searchPackageOr listOfWords limit =
    query {
        for p in repository.GetPackages() do
        where (List.exists p.Title.Contains listOfWords)
        take limit
        yield p 
    }

result |> Seq.iter Pget.IPack.showPackage


searchPackageOr ["chart"; "plot"; "graph"; "curve"] 20
|> Seq.iter Pget.IPack.showPackage


let files = Pget.Nupkg.getFile "packages/FSharp.Charting.Gtk.0.90.14/FSharp.Charting.Gtk.0.90.14.nupk"g

files |> Seq.iter (fun p -> Console.WriteLine(p))


Pget.Nupkg.showFiles "packages/FSharp.Charting.Gtk.0.90.14/FSharp.Charting.Gtk.0.90.14.nu pkg"

NuGet.PackageReferenceFile


Pget.RepoLocal.getRefs "packages" Pget.Framework.net45 |> Seq.iter Console.WriteLine




packman.

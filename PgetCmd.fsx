#if INTERACTIVE
#r "packages/NuGet.Core/lib/net40-Client/NuGet.Core.dll"
#r "packages/Microsoft.Web.Xdt/lib/net40/Microsoft.Web.XmlTransform.dll"
#r "System.Linq.dll"
#endif

open System 
open System.Linq 


#load "Pget.fs"
open NuGet



let commandLineArgsInteractive () =
    let args = Environment.GetCommandLineArgs()
    let idx = Array.tryFindIndex (fun a -> a = "--") args
    match idx with
    | None -> [||]
    | Some i -> args.[(i+1)..]


let searchPackageById packageName = 
    packageName
    |> Pget.Nuget.searchPackagesById
    |> Seq.iter Pget.IPack.showPackage

let searchLocalPackage path package =
    Pget.RepoLocal.searchPackage path package
    |> Seq.iter Pget.IPack.showPackage

let showScript framework repoPath  =
    match Pget.Framework.parseFramework framework with
    | Some fr -> Pget.RepoLocal.showScript fr repoPath
    | None    -> Console.WriteLine("Error: Wrong framework parameter")

let showLocalPackageRef framework packageId = 
    match Pget.Framework.parseFramework framework with
    | Some fr -> Pget.RepoLocal.showPackageRefsFsx "packages" fr packageId
    | None    -> Console.WriteLine("Error: Wrong framework parameter")

let showRepoPackageRef framework repo packageId =
    match Pget.Framework.parseFramework framework with
    | Some fr -> Pget.RepoLocal.showPackageRefsFsx repo fr packageId
    | None    -> Console.WriteLine("Error: Wrong framework parameter")
 

let showHelp () =
    Console.WriteLine """
--list                     List all packages in current repository ./package
--list [repo]              List all package in [repo] repository.

--show                     Show all packages in current ./packages repository
--show [repo]              Show all packages in [repo] repository.

--search [package]         Search a package by name.
--search [package] [repo]  Search a pacakge by name in a local repository
--search-local [pacakge]   Search a package in ./packages
--nupkg show [file]        Show metadata of a *.nupkg file
    """


let parseCommands cmdargs =
    match List.ofArray cmdargs with
    | ["--list"       ]                   ->  Pget.RepoLocal.showPackageList "packages"
    | ["--list"; repo ]                   ->  Pget.RepoLocal.showPackageList  repo
    | ["--show";      ]                   ->  Pget.RepoLocal.showPackages "packages"
    | ["--show"; repo ]                   ->  Pget.RepoLocal.showPackages repo
    | ["--nupkg"; "show"; fname]          ->  Pget.Nupkg.read fname |> Pget.IPack.showPackage
    | ["--search"; pack ]                 ->  searchPackageById pack
    | ["--search"; pack ; "--repo"]       ->  searchLocalPackage pack "pacakges"
    | ["--search"; pack ; "--repo"; path] ->  searchLocalPackage pack  path
    
    | ["--ref"; frm   ]                                   ->  showScript frm "packages"
    | ["--ref"; frm  ; "--pack";  pack]                   ->  showLocalPackageRef frm pack
    | ["--ref"; frm  ; "--pack";  pack; "--repo"; path]   ->  showRepoPackageRef frm path pack
    
    | []                                  ->  showHelp ()
    | _                                   ->  Console.WriteLine "Error: Invalid option."


let main() =    
#if INTERACTIVE
    parseCommands <| commandLineArgsInteractive ()
    0
#else
    parseCommands <| Array.tail (Environment.GetCommandLineArgs())
    0
#endif 
main() 

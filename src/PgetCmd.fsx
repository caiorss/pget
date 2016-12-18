#if INTERACTIVE
#r "../packages/NuGet.Core/lib/net40-Client/NuGet.Core.dll"
#r "../packages/Microsoft.Web.Xdt/lib/net40/Microsoft.Web.XmlTransform.dll"
#r "System.Linq.dll"
#r "System.Xml.Linq.dll"
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
 

let showVersion () =
    Console.WriteLine """
Pget - Package Get - Version 1.0 
Copyright (C) 2016 Caio Rodrigues
    """

let showHelp () =
    Console.WriteLine """
Pget - Package Get - Enhanced command line interface to NuGet.Core

Commands                                    Description
------------------------------------------  -------------------------------------------------------------

List Commands:

  --list                                      List all packages in current repository ./package
  --list [repo]                               List all package in [repo] repository.

  --show                                      Show all packages in current ./packages repository
  --show [repo]                               Show all packages in [repo] repository.

Search commands:

  --search [package]                          Search a package by name.
  --search [package] --repo                   Search a pacakge by name in a local repository
  --search [package] --repo [repo]            Search a package in ./packages

Show references for fsx scripts:

  --ref [frm]                                 Show all assembly references from current ./packages
  --ref [frm] --pack [pack]                   Show all assembly references from a package [pack] at ./packages.              
  --ref [frm] --pack [pack] --repo [path]     Show all assembly references from a package at [repo] directory
                                              frm:  .NET Framework  net40 | net45

Install packages:

  --install [pack]                            Install the latest version of package [pack] to ./packages
  --install [pack] --repo [repo]              Install the latest version of package [pack] to a repository [repo] i.e: ~/nuget
  --install [pack] --ver [ver]                Install the version [ver] of package [pack]
  --install [pack] --ver [ver] --repo [repo]  Install the version [ver] of package [pack] to a repository [repo]

Nupkg Files:

  --nupkg show [file]                         Show metadata of a *.nupkg file

--------------------------------------------------------------------------------------------------------------------

Command abbreviations:

  --install   -i
  --repo      -r
  --help      -h
  --version   -v
  --ver       -v
  --list      -l
  --search    -s
  --show      -sh

    """

    showVersion()
    


let parseCommands cmdargs =
    match List.ofArray cmdargs with
    | ["--version" ]                      ->  showVersion ()
    | ["-v" ]                             ->  showVersion ()
    
    | ["--help" ]                         ->  showHelp ()    
    | ["-h" ]                             ->  showHelp ()
    
    | ["--list"       ]                   ->  Pget.RepoLocal.showPackageList "packages"
    | ["--list"; repo ]                   ->  Pget.RepoLocal.showPackageList  repo
    | ["-l"       ]                       ->  Pget.RepoLocal.showPackageList "packages"
    | ["-l"; repo ]                       ->  Pget.RepoLocal.showPackageList  repo


    | ["--show";      ]                   ->  Pget.RepoLocal.showPackages "packages"
    | ["--show"; repo ]                   ->  Pget.RepoLocal.showPackages repo
    | ["-sh";      ]                       ->  Pget.RepoLocal.showPackages "packages"
    | ["-sh"; repo ]                       ->  Pget.RepoLocal.showPackages repo

   
    | ["--search"; pack ]                 ->  searchPackageById pack
    | ["--search"; pack ; "--repo"]       ->  searchLocalPackage pack "pacakges"
    | ["--search"; pack ; "--repo"; path] ->  searchLocalPackage pack  path
    | ["-s"; pack ]                       ->  searchPackageById pack
    | ["-s"; pack ; "-r"]                 ->  searchLocalPackage pack "pacakges"
    | ["-s"; pack ; "-r"; path]           ->  searchLocalPackage pack  path
   
    | ["--ref"; frm   ]                                      ->  showScript frm "packages"
    | ["--ref"; frm  ; "--pack";  pack]                      ->  showLocalPackageRef frm pack
    | ["--ref"; frm  ; "--pack";  pack; "--repo"; path]      ->  showRepoPackageRef frm path pack
    | ["--ref"; frm  ; "-p";  pack]                          ->  showLocalPackageRef frm pack
    | ["--ref"; frm  ; "-p";  pack; "-r"; path]              ->  showRepoPackageRef frm path pack



    | ["--install"; pack ]                                   ->  Pget.RepoLocal.installPackageLatest "packages" pack
    | ["--install"; pack ; "--repo"; path ]                  ->  Pget.RepoLocal.installPackageLatest path pack
    | ["--install"; pack ; "--ver" ; ver  ]                  ->  Pget.RepoLocal.installPackage "packages" (pack, ver)
    | ["--install"; pack ; "--ver" ; ver ; "--repo"; path  ] ->  Pget.RepoLocal.installPackage path (pack, ver)
    | ["-i"; pack ]                                          ->  Pget.RepoLocal.installPackageLatest "packages" pack
    | ["-i"; pack ; "-r"; path ]                             ->  Pget.RepoLocal.installPackageLatest path pack
    | ["-i"; pack ; "-v" ; ver  ]                            ->  Pget.RepoLocal.installPackage "packages" (pack, ver)
    | ["-i"; pack ; "-v" ; ver ; "-r"; path  ]               ->  Pget.RepoLocal.installPackage path (pack, ver)


    // | ["--install-from-file" ; file ]

    | ["--nupkg"; "show"; fname]          ->  Pget.Nupkg.read fname |> Pget.IPack.showPackage
    
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

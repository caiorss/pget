namespace Pget 

// #if INTERACTIVE
// #r "../packages/NuGet.Core/lib/net40-Client/NuGet.Core.dll"
// #r "../packages/Microsoft.Web.Xdt/lib/net40/Microsoft.Web.XmlTransform.dll"
// #r "System.Linq.dll"
// #r "System.Xml.Linq.dll"
// #endif


open System 
open System.Linq 


// #load "Pget.fs"
open NuGet


/// Assembly attributes wrapper
module AsmAttr =
    open System.Reflection

    let optDefault def opt =
        match opt with
        | None    -> def
        | Some x  -> x

    /// Load Assembly File
    let loadFrom (assemblyFile: string) =
        Assembly.LoadFrom assemblyFile

    /// Return assembly name
    let getName (asm: Assembly) =
        asm.GetName().Name

    /// Return assembly full name
    let getFullName (asm: Assembly) =
        asm.GetName().FullName

    /// Return assembly version
    let getVersion (asm: Assembly) =
        asm.GetName().Version

    /// Returns assembly title attribute
    ///
    let getTitle (asm: Assembly) =
        asm.GetCustomAttributes<AssemblyTitleAttribute>()
        |> Seq.tryItem 0
        |> Option.map (fun p -> p.Title)

    /// Return assembly description attribute
    ///
    let getDescription (asm: Assembly) =
        asm.GetCustomAttributes<AssemblyDescriptionAttribute>()
        |> Seq.tryItem 0
        |> Option.map (fun p -> p.Description)

    /// Returns assembly copyright attribute
    ///
    let getCopyright (asm: Assembly) =
        asm.GetCustomAttributes<AssemblyCopyrightAttribute>()
        |> Seq.tryItem 0
        |> Option.map (fun p -> p.Copyright)

    /// Returns assembly culture attribute
    ///
    let getCulture (asm: Assembly) =
        asm.GetCustomAttributes<AssemblyCultureAttribute>()
        |> Seq.tryItem 0
        |> Option.map (fun p -> p.Culture)

    /// Returns assembly product attribute
    ///
    let getProduct (asm: Assembly) =
        asm.GetCustomAttributes<AssemblyProductAttribute>()
        |> Seq.tryItem 0
        |> Option.map (fun p -> p.Product)

    /// Returns assembly company attribute
    ///
    let getCompany (asm: Assembly) =
        asm.GetCustomAttributes<AssemblyCompanyAttribute>()
        |> Seq.tryItem 0
        |> Option.map (fun p -> p.Company)

    /// Returns assembly  attribute
    ///
    let getComVisible (asm: Assembly) =
        asm.GetCustomAttributes<System.Runtime.InteropServices.ComVisibleAttribute>()
        |> Seq.tryItem 0
        |> Option.map (fun p -> p.Value)

    /// Returns assembly Guid  attribute
    ///
    let getGuid (asm: Assembly) =
        asm.GetCustomAttributes<System.Runtime.InteropServices.GuidAttribute>()
        |> Seq.tryItem 0
        |> Option.map (fun p -> p.Value)

    let getRuntimeVersion (asm: Assembly) =
        asm.ImageRuntimeVersion

    let getAsmReferences (asm: Assembly) =
        asm.GetReferencedAssemblies()
        |> Seq.map (fun a ->
                     Map.ofSeq [
                                    ("Name",        a.Name)
                                  ; ("FullName",    a.FullName)
                                  ; ("CultureInfo", a.CultureInfo.Name)
                                  ; ("Version",     a.Version.ToString())
                               ]
                     )

    let showAsmReferences (asmFile: string) =
        let asm = loadFrom asmFile
        asm.GetReferencedAssemblies ()
        |> Seq.iter (fun an -> Console.WriteLine("Name = {0}\t\tVersion = {1}\t\tCulture = {2}",
                                                 an.Name,
                                                 an.Version,
                                                 an.CultureInfo.Name
                                                 ))


    /// Print assembly file attributes
    ///
    let showFile (asmFile: string) =
        let asm = loadFrom asmFile
        printfn "Assembly Attributes"
        printfn "-------------------------------------------"
        printfn "Name         %s" (getName asm)
        // printfn "Full Name    $s" (getFullName asm)
        printfn "Version      %s" <| (getVersion asm).ToString()
        printfn "CLR Version  %s" <| getRuntimeVersion asm
        printfn "Product      %s" (optDefault ""  <| getProduct asm)
        printfn "Culture      %s" (optDefault ""  <| getCulture asm)
        printfn "Company      %s" (optDefault ""  <| getCompany asm)
        printfn "Description  %s" (optDefault ""  <| getDescription asm)
        printfn "Copyright    %s" (optDefault ""  <| getCopyright asm)
        printfn "GUID         %s" (optDefault ""  <| getGuid asm)
        printfn "Com Visible  %s" (optDefault ""  <| (getComVisible asm
                                                      |> Option.map (fun e -> e.ToString())))
        printfn "Codebase     %s" asm.CodeBase


module Main =

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
      --show --pack [pack]                        Show the package [pack] in ./packages directory
      --show [repo] --pack [pack]                 Show the package [pack] in [repo] directory.

    Search commands:

      --search [package]                          Search a package by name.
      --search [package] --repo                   Search a pacakge by name in a local repository
      --search [package] --repo [repo]            Search a package in ./packages

    Show references for F# *.fsx scripts:

      --ref [frm]                                 Show all assembly references from current ./packages.
      --ref [frm] --repo [repo]                   Show all assembly references from current [repo] directory.
      --ref [frm] --pack [pack]                   Show all assembly references from a package [pack] at ./packages.              
      --ref [frm] --pack [pack] --repo [path]     Show all assembly references from a package at [repo] directory
                                                  frm:  .NET Framework  net40 | net45

    Install packages:

      --install [pack]                            Install the latest version of package [pack] to ./packages
      --install [pack] --repo [repo]              Install the latest version of package [pack] to a repository [repo] i.e: ~/nuget
      --install [pack] --ver [ver]                Install the version [ver] of package [pack]
      --install [pack] --ver [ver] --repo [repo]  Install the version [ver] of package [pack] to a repository [repo]

      --install-from-file                         Install all packages listed in the file ./packages.lst to ./packages directory.
      --install-from-file [file]                  Install all packages listed in the file [file] to ./packages
      --install-from-file [file] --repo [repo]    Install all packages listed in the file [file] to [repo] directory.

    Nupkg Files:

      --nupkg show [file]                         Show metadata of a *.nupkg file

    --------------------------------------------------------------------------------------------------------------------

    Command abbreviations:

      --install            -i
      --repo               -r
      --help               -h
      --version            -v
      --ver                -v
      --list               -l
      --search             -s
      --show               -sh
      --install-from-file  -if
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
        | ["--show"; "--pack"; pack]          ->  Pget.RepoLocal.showPackage "packages" pack
        | ["--show"; repo ; "--pack"; pack]   ->  Pget.RepoLocal.showPackage repo pack       
        | ["-sh";      ]                      ->  Pget.RepoLocal.showPackages "packages"
        | ["-sh"; repo ]                      ->  Pget.RepoLocal.showPackages repo
        | ["-sh"; "-p"; pack]                 ->  Pget.RepoLocal.showPackage "packages" pack
        | ["-sh"; repo ; "-p"; pack]          ->  Pget.RepoLocal.showPackage repo pack       

        | ["--search"; pack ]                 ->  searchPackageById pack
        | ["--search"; pack ; "--repo"]       ->  searchLocalPackage pack "pacakges"
        | ["--search"; pack ; "--repo"; path] ->  searchLocalPackage pack  path
        | ["-s"; pack ]                       ->  searchPackageById pack
        | ["-s"; pack ; "-r"]                 ->  searchLocalPackage pack "pacakges"
        | ["-s"; pack ; "-r"; path]           ->  searchLocalPackage pack  path

        | ["--ref"; frm   ]                                      ->  showScript frm "packages"
        | ["--ref"; frm ; "--repo"; path ]                       ->  showScript frm  path
        | ["--ref"; frm  ; "--pack";  pack]                      ->  showLocalPackageRef frm pack
        | ["--ref"; frm  ; "--pack";  pack; "--repo"; path]      ->  showRepoPackageRef frm path pack
        | ["--ref"; frm  ; "-p";  pack]                          ->  showLocalPackageRef frm pack
        | ["--ref"; frm ; "-r"; path ]                           ->  showScript frm  path
        | ["--ref"; frm  ; "-p";  pack; "-r"; path]              ->  showRepoPackageRef frm path pack



        | ["--install"; pack ]                                   ->  Pget.RepoLocal.installPackageLatest "packages" pack
        | ["--install"; pack ; "--repo"; path ]                  ->  Pget.RepoLocal.installPackageLatest path pack
        | ["--install"; pack ; "--ver" ; ver  ]                  ->  Pget.RepoLocal.installPackage "packages" (pack, ver)
        | ["--install"; pack ; "--ver" ; ver ; "--repo"; path  ] ->  Pget.RepoLocal.installPackage path (pack, ver)
        | ["-i"; pack ]                                          ->  Pget.RepoLocal.installPackageLatest "packages" pack
        | ["-i"; pack ; "-r"; path ]                             ->  Pget.RepoLocal.installPackageLatest path pack
        | ["-i"; pack ; "-v" ; ver  ]                            ->  Pget.RepoLocal.installPackage "packages" (pack, ver)
        | ["-i"; pack ; "-v" ; ver ; "-r"; path  ]               ->  Pget.RepoLocal.installPackage path (pack, ver)

        | ["--install-from-file" ]                               ->  Pget.RepoLocal.installPackagesFromFile "packages" "packages.list" 
        | ["--install-from-file" ; file ]                        ->  Pget.RepoLocal.installPackagesFromFile "packages" file
        | ["--install-from-file" ; file; "--repo"; repo ]        ->  Pget.RepoLocal.installPackagesFromFile repo file
        | ["-if" ]                                               ->  Pget.RepoLocal.installPackagesFromFile "packages" "packages.list" 
        | ["-if" ; file ]                                        ->  Pget.RepoLocal.installPackagesFromFile "packages" file
        | ["-if" ; file; "-r"; repo ]                            ->  Pget.RepoLocal.installPackagesFromFile repo file


        | ["--nupkg"; "show"; fname]          ->  Pget.Nupkg.read fname |> Pget.IPack.showPackage

        | []                                  ->  showHelp ()
        | _                                   ->  Console.WriteLine "Error: Invalid option."

    [<EntryPoint>]    
    let main (args) =    
        parseCommands args 
        0


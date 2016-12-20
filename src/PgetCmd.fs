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
        
    /// Display resources from an .NET assembly file 
    let showResurces (asmFile: string) =
        let asm = loadFrom asmFile
        asm.GetManifestResourceNames() |> Seq.iter Console.WriteLine

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

  Commands                                      Description
  -----------------------------                -----------------------------------------------

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

    nupkg --show  [file]                         Show metadata of a *.nupkg file
    nupkg --files [file]                         Show files in nupkg [file]

  Assembly files: *.exe or *.dll

    asm --show           [file]                   Show all assembly attributes from an assembly file.
    asm --show-ref       [file]                   Show all assembly references from an assembly file.
    asm --show-resources [file]                   Show resources from an assembly file.

  Generate Guid - Globally Unique Identifier 

    --guid 

  --------------------------------------------

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

        // ================================= Repository related commands ==================
        //
        | ["repo"; path; "--list"]                        ->  Pget.RepoLocal.showPackageList path
        | ["repo"; "--list"]                              ->  Pget.RepoLocal.showPackageList "packages"
        | ["repo"; path; "-l"]                            ->  Pget.RepoLocal.showPackageList path
        | ["repo"; "-l"]                                  ->  Pget.RepoLocal.showPackageList "packages"

        // Show all packages in repository 
        | ["repo"; path; "--show" ]                       ->  Pget.RepoLocal.showPackages path        
        | ["repo"; "--show" ]                             ->  Pget.RepoLocal.showPackages "packages"    
        | ["repo"; path ; "--show"; pack ]                ->  Pget.RepoLocal.showPackage path pack 
        | ["repo"; "--show"; pack ]                       ->  Pget.RepoLocal.showPackage "packages" pack 


        | ["repo"; path; "-sh" ]                          ->  Pget.RepoLocal.showPackages path        
        | ["repo"; "-sh" ]                                ->  Pget.RepoLocal.showPackages "packages"    
        | ["repo"; path ; "-sh"; pack ]                   ->  Pget.RepoLocal.showPackage path pack 
        | ["repo"; "-sh"; pack ]                          ->  Pget.RepoLocal.showPackage "packages" pack 


        // Show files of a package in project repository
        | ["repo"; "--show-files" ; pack ]                ->   Pget.RepoLocal.showPackageFiles "packages" pack
        | ["repo"; "-sf" ; pack ]                         ->   Pget.RepoLocal.showPackageFiles  "packages" pack       
        | ["repo"; path ; "--show-files" ; pack ]         ->   Pget.RepoLocal.showPackageFiles  path pack
        | ["repo"; path; "-sf" ; pack ]                   ->   Pget.RepoLocal.showPackageFiles  path pack


        // Install package to repository 
        | ["repo"; path; "--install"; pack ]              ->  Pget.RepoLocal.installPackageLatest path pack
        | ["repo"; "--install"; pack ]                    ->  Pget.RepoLocal.installPackageLatest "packages" pack
        | ["repo"; path ; "--install"; pack ; ver ]       ->  Pget.RepoLocal.installPackage path (pack, ver)        
        | ["repo"; "--install"; pack ;  ver  ]            ->  Pget.RepoLocal.installPackage "packages" (pack, ver)
        | ["repo"; path; "-i"; pack ]                     ->  Pget.RepoLocal.installPackageLatest path pack
        | ["repo"; "-i"; pack ]                           ->  Pget.RepoLocal.installPackageLatest "packages" pack
        | ["repo"; path ; "-i"; pack ; ver ]              ->  Pget.RepoLocal.installPackage path (pack, ver)    
        | ["repo"; "-i"; pack ;  ver  ]                   ->  Pget.RepoLocal.installPackage "packages" (pack, ver)

        // Install all packages from a list of package to repository
        | ["repo"; "--install-from-file" ]                ->  Pget.RepoLocal.installPackagesFromFile "packages" "packages.list" 
        | ["repo"; "--install-from-file" ; file ]         ->  Pget.RepoLocal.installPackagesFromFile "packages" file
        | ["repo"; path; "--install-from-file" ; file ]   ->  Pget.RepoLocal.installPackagesFromFile  path file

        | ["repo"; "-if" ]                                ->  Pget.RepoLocal.installPackagesFromFile "packages" "packages.list" 
        | ["repo"; "-if" ; file ]                         ->  Pget.RepoLocal.installPackagesFromFile "packages" file
        | ["repo"; path; "-if" ; file ]                   ->  Pget.RepoLocal.installPackagesFromFile  path file


        // Generate F# include directives (#r) for all packages in a repository 
        | ["repo"; path ; "--ref"; framework  ]         ->  showScript framework  path
        | ["repo"; "--ref"; framework  ]                ->  showScript framework "packages"

        | ["repo"; path ; "--ref"; framework ; pack ]   ->  showRepoPackageRef framework path pack
        | ["repo"; "--ref"; framework ; pack ]          ->  showRepoPackageRef framework "packages" pack

       
        // ============================ NuGet Repository (Remote) ========================== 

        // search package 
        | ["nuget"; "--search" ; pack  ]         ->  searchPackageById pack
        | ["nuget"; "-s" ; pack  ]               ->  searchPackageById pack
        
        // | ["pack"; "--search"; pack ; "--repo"]                   ->  searchLocalPackage pack "pacakges"
        // | ["pack"; "--search"; pack ; "--repo"; path]             ->  searchLocalPackage pack  path
        

        // ======  Commands to Handle NuGet package Archives ============== //
        | ["nupkg"; "--show"; fname]                             ->  Pget.Nupkg.show fname
        | ["nupkg"; "--files"; fname]                            ->  Pget.Nupkg.showFiles fname

        // ==========  Commands to Handle .NET assembly ============== //
        | ["asm" ; "--show" ; asmFile]                           -> AsmAttr.showFile asmFile
        | ["asm" ; "--show-ref" ; asmFile]                       -> AsmAttr.showAsmReferences asmFile         
        | ["asm" ; "--show-resources"; asmFile]                  -> AsmAttr.showResurces asmFile

        | ["--guid" ]                                            -> Console.WriteLine(Guid.NewGuid().ToString() : string)

        | []                                  ->  showHelp ()
        | _                                   ->  Console.WriteLine "Error: Invalid option."


    [<EntryPoint>]    
    let main (args) =    
        parseCommands args 
        0


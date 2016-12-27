namespace Pget 

open System 
open System.Linq 
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

    /// Get types from an assembly
    let getTypes (asm: Assembly) =
        asm.GetTypes()

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

    /// Print all namespaces from an assembly (.exe or .dll)
    let showNamespaces (asmFile: string) =
        let asm = loadFrom asmFile 
        asm.GetTypes() |> Seq.distinctBy (fun t -> t.Namespace)
                       |> Seq.iter (fun t -> Console.WriteLine(t.Namespace))        

    let getPublicTypesInNamespace  (asmFile: string) selector (ns: string) =
        let asm = loadFrom asmFile 
        asm.GetTypes()
        |> Seq.filter (fun (atype: Type) ->
                       atype.Namespace = ns
                       && atype.IsPublic
                       && selector atype
                       )

     

    let showClassesInNamespace (asmFile: string) ns =
        getPublicTypesInNamespace asmFile (fun atype -> atype.IsClass) ns 
        |> Seq.iter Console.WriteLine


    let showMethods bindingFlags (asmFile: string) (className: string) =
        loadFrom asmFile
        |> getTypes
        |> Seq.tryFind(fun atype -> atype.FullName = className)
        |> Option.map(fun atype -> atype.GetMethods(bindingFlags))
        |> Option.iter (Seq.iter (fun m -> Console.WriteLine("")
                                           Console.WriteLine m)
                        )

    let showPublicMethods  (asmFile: string) (className: string) =
        let flags = BindingFlags.Public |||  BindingFlags.Instance ||| BindingFlags.DeclaredOnly
        showMethods flags asmFile className

    let showPublicStaticMethods  (asmFile: string) (className: string) =
        let flags = BindingFlags.Public |||  BindingFlags.Static ||| BindingFlags.DeclaredOnly
        showMethods flags asmFile className

    let showPrivateMethods  (asmFile: string) (className: string) =
        let flags = BindingFlags.Static |||  BindingFlags.Instance ||| BindingFlags.NonPublic
        showMethods flags asmFile className

    let showPrivateStaticMethods  (asmFile: string) (className: string) =
        let flags = BindingFlags.Static |||  BindingFlags.NonPublic
        showMethods flags asmFile className

    let showAllMethods (asmFile: string) (className: string) =
        Console.WriteLine "\n\nPublic Methods"
        Console.WriteLine "--------------------------------------"
        showPublicMethods asmFile className
        Console.WriteLine "\nPublic Static Methods"
        Console.WriteLine "--------------------------------------"
        showPublicStaticMethods asmFile className
        Console.WriteLine "\nPrivate Static Methods"
        Console.WriteLine "--------------------------------------"
        showPrivateStaticMethods asmFile className

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

    
    /// 
    /// Default repository ./packages or the top level 'packages'
    /// directory in a project.
    /// 
    let projectRepo = "packages"

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
        | None    -> Console.WriteLine("Error: Wrong framework parameter.")

    let showLocalPackageRef framework packageId = 
        match Pget.Framework.parseFramework framework with
        | Some fr -> Pget.RepoLocal.showPackageRefsFsx "packages" fr packageId
        | None    -> Console.WriteLine("Error: Wrong framework parameter.")

    let showRepoPackageRef framework repo packageId =
        match Pget.Framework.parseFramework framework with
        | Some fr -> Pget.RepoLocal.showPackageRefsFsx repo fr packageId
        | None    -> Console.WriteLine("Error: Wrong framework parameter.")

    /// Open package's project web site in default web browser    
    let openProjectUrl repoPath packageId =
        let pack = Pget.RepoLocal.findPackageById2 repoPath packageId
        let urlOpt =  Option.bind Pget.IPack.projectUrl pack

        match pack with
        | None        ->  printfn "Error: I can't find the package %s in %s" packageId repoPath
        | Some pack'  ->  match urlOpt with
                          | None     -> printfn "Error: Package doesn't have project URL."
                          | Some url -> printfn "Opening %s" url
                                        ignore <| System.Diagnostics.Process.Start (url: string)
                          
    /// Open package's license URL  in default web browser    
    let openLicenseUrl repoPath packageId =               
        let pack = Pget.RepoLocal.findPackageById2 repoPath packageId
        let urlOpt =  Option.bind Pget.IPack.licenseUrl pack 

        match pack with
        | None        ->  printfn "Error: I can't find the package %s in %s" packageId repoPath
        | Some pack'  ->  match urlOpt with
                          | None     -> printfn "Error: Package doesn't have a licence URL."
                          | Some url -> printfn "Opening %s" url
                                        ignore <| System.Diagnostics.Process.Start (url: string)
                                        
    /// Open NuGet web site - https://www.nuget.org/
    let openNugetWebsite () =
        ignore <| System.Diagnostics.Process.Start ("https://www.nuget.org/")
        
        
    let showVersion () =
        Console.WriteLine """
 Pget - Package Get - Version 1.3.1 
 2016 Public Domain Software
 Repository - https://github.com/caiorss/pget
        """

    let showHelp () =
        Console.WriteLine """
Pget - Package Get - Enhanced command line interface to NuGet.Core

  Commands                                      Description
  -----------------------------                -----------------------------------------------

  List Repository

    repo --list                                 List all packages in current repository ./package
    repo [path] --list                          List all package in [path] repository.

  Show repository 
 
    repo --show                                 Show all packages in current ./packages repository
    repo [path] --show                          Show all packages in [path] repository.
  
  Show package metadata

    repo --show  [pack]                         Show the package [pack] in ./packages directory
    repo [path] --show [pack]                   Show the package [pack] in [repo] directory.

  Show package files 

    repo --show-files [pack]                    Show content files of package [pack] in ./packages
    repo [path] --show-files [pack]             Show content files of package [pack] in [repo]

  Install package to repository  

    repo --install [pack]                       Install the latest version of package [pack] to ./packages
    repo --install [pack] [ver]                 Install the version [ver] of package [pack]
    repo [path] --install [pack]                Install the latest version of package [pack] to a repository [path] i.e: ~/nuget
    repo [path] --install [pack] [ver]          Install the version [ver] of package [pack] to a repository [path]


  Install a list of packages passed as argument
    repo --install-list FParsec NuGet.Core-2.0.0               Install those packages to ./packages
    repo /tmp/repo --install-list FParsec NuGet.Core-2.0.0     Install those packages to /tmp/repository

  Install a list of packages listed in a file

    repo --install-from-file                    Install all packages listed in the file ./packages.list to ./packages directory.
    repo --install-from-file [file]             Install all packages listed in the file ./packages.list to ./packages directory.
    repo [path] --install-from-file [file]      Install all packages listed in the file [file] to [path]


  Open package project URL or Licence URL

    repo --url [pack]                           Browse project URL of a package [pack] in ./packages.
    repo --license [pack]                       Browse licence URL of a package [pack] in ./packages.
    repo [path] --url [pack]                    Browse project URL of a package [pack] in [path]
    repo [path] --license [pack]                Browse licence URL of a package [pack] in [path]

  Show references for F# *.fsx scripts:        [frm]:  .NET Framework  net40 | net45   

    repo --ref [frm]                            Show all assembly references from current ./packages.
    repo --ref  --pack [pack]                   Show all assembly references from a package [pack] at ./packages.              
    repo [path] --ref [frm]                     Show all assembly references from current [repo] directory.
    repo [path] --ref [frm] [pack]              Show all assembly references from a package at [repo] directory
                            
  Nuget commands:

    nuget --search [package]                    Search a package by name.  
    nuget --show   [package]                    Show package information (metadata).
    nuget --open                                Open NuGet web site - https://www.nuget.org

  Nupkg Files:

    nupkg --show  [file]                        Show metadata of a *.nupkg file
    nupkg --files [file]                        Show files in nupkg [file]

  Assembly files: *.exe or *.dll

    asm --info [file]                                    Show all assembly attributes from an assembly [file].
    asm --refs [file]                                    Show all assembly references from an assembly [file].
    asm --resources  [file]                              Show resources from an assembly file.
    asm --namespace|-ns [file]                           Display all assembly namespaces.
    asm --namespace|-ns [file] --classes|-cls [ns]       Show all classes from namespace [ns] in assembly [file]


  Generate Guid - Globally Unique Identifier 

    --guid 

  --------------------------------------------

  Command abbreviations:

    --install            -i
    --install-from-file  -if
    --install-list       -il
    --help               -h
    --version            -v
    --ver                -v
    --list               -l
    --search             -s
    --show               -sh
         """

        showVersion()



    let parseCommands cmdargs =
        match List.ofArray cmdargs with
        | ["--version" ]                                    ->  showVersion ()
        | ["-v" ]                                           ->  showVersion ()

        | ["--help" ]                                       ->  showHelp ()    
        | ["-h" ]                                           ->  showHelp ()

        // ================================= Repository related commands ==================
        //
        | ["repo"; path; "--list"]                          ->  Pget.RepoLocal.showPackageList path
        | ["repo"; "--list"]                                ->  Pget.RepoLocal.showPackageList projectRepo
        | ["repo"; path; "-l"]                              ->  Pget.RepoLocal.showPackageList path
        | ["repo"; "-l"]                                    ->  Pget.RepoLocal.showPackageList projectRepo

        // Show all packages in repository 
        | ["repo"; path; "--show" ]                         ->  Pget.RepoLocal.showPackages path        
        | ["repo"; "--show" ]                               ->  Pget.RepoLocal.showPackages projectRepo    
        | ["repo"; path ; "--show"; pack ]                  ->  Pget.RepoLocal.showPackage path pack 
        | ["repo"; "--show"; pack ]                         ->  Pget.RepoLocal.showPackage projectRepo pack 


        | ["repo"; path; "-sh" ]                            ->  Pget.RepoLocal.showPackages path        
        | ["repo"; "-sh" ]                                  ->  Pget.RepoLocal.showPackages projectRepo    
        | ["repo"; path ; "-sh"; pack ]                     ->  Pget.RepoLocal.showPackage path pack 
        | ["repo"; "-sh"; pack ]                            ->  Pget.RepoLocal.showPackage projectRepo pack  

        // Open project URL 
        | [ "repo"; "--url" ; pack ]                        -> openProjectUrl projectRepo pack
        | [ "repo"; path; "--url" ; pack ]                  -> openProjectUrl path pack

        // Open licence URL 
        | ["repo"; "--license"; pack ]                      -> openLicenseUrl projectRepo pack 
        | ["repo"; path; "--license"; pack ]                -> openLicenseUrl path pack 
        
        // Show files of a package in project repository
        | ["repo"; "--files" ; pack ]                       ->   Pget.RepoLocal.showPackageFiles projectRepo pack
        | ["repo"; path ; "--files" ; pack ]                ->   Pget.RepoLocal.showPackageFiles  path pack

        // Install package to repository 
        | ["repo"; path; "--install"; pack ]                ->  Pget.RepoLocal.installPackageLatest path pack
        | ["repo"; "--install"; pack ]                      ->  Pget.RepoLocal.installPackageLatest projectRepo pack
        | ["repo"; path ; "--install"; pack ; ver ]         ->  Pget.RepoLocal.installPackage path (pack, ver)        
        | ["repo"; "--install"; pack ;  ver  ]              ->  Pget.RepoLocal.installPackage projectRepo (pack, ver)
        | ["repo"; path; "-i"; pack ]                       ->  Pget.RepoLocal.installPackageLatest path pack
        | ["repo"; "-i"; pack ]                             ->  Pget.RepoLocal.installPackageLatest projectRepo pack
        | ["repo"; path ; "-i"; pack ; ver ]                ->  Pget.RepoLocal.installPackage path (pack, ver)    
        | ["repo"; "-i"; pack ;  ver  ]                     ->  Pget.RepoLocal.installPackage projectRepo (pack, ver)

        | "repo" :: "--install-list" :: packageList         -> Pget.RepoLocal.installPackageList projectRepo packageList 
        | "repo" :: path :: "--install-list" :: packageList -> Pget.RepoLocal.installPackageList  path      packageList 
        | "repo" :: "-il" :: packageList                    -> Pget.RepoLocal.installPackageList projectRepo packageList 
        | "repo" :: path :: "-il" :: packageList            -> Pget.RepoLocal.installPackageList  path      packageList 


        // Install all packages from a list of package to repository
        | ["repo"; "--install-from-file" ]                  ->  Pget.RepoLocal.installPackagesFromFile projectRepo "packages.list" 
        | ["repo"; "--install-from-file" ; file ]           ->  Pget.RepoLocal.installPackagesFromFile projectRepo file
        | ["repo"; path; "--install-from-file" ; file ]     ->  Pget.RepoLocal.installPackagesFromFile  path file

        | ["repo"; "-if" ]                                  ->  Pget.RepoLocal.installPackagesFromFile projectRepo "packages.list" 
        | ["repo"; "-if" ; file ]                           ->  Pget.RepoLocal.installPackagesFromFile projectRepo file
        | ["repo"; path; "-if" ; file ]                     ->  Pget.RepoLocal.installPackagesFromFile  path file


        // Generate F# include directives (#r) for all packages in a repository 
        | ["repo"; path ; "--ref"; framework  ]             ->  showScript framework  path
        | ["repo"; "--ref"; framework  ]                    ->  showScript framework projectRepo

        | ["repo"; path ; "--ref"; framework ; pack ]       ->  showRepoPackageRef framework path pack
        | ["repo"; "--ref"; framework ; pack ]              ->  showRepoPackageRef framework projectRepo pack

       
        // ============================ NuGet Repository (Remote) ========================== 

        // search package 
        | ["nuget"; "--search" ; pack  ]                    ->  searchPackageById pack
        | ["nuget"; "-s" ; pack  ]                          ->  searchPackageById pack

        // Show specific package metadata
        | ["nuget"; "--show" ; pack  ]                      ->  Nuget.showPackage  pack
        | ["nuget"; "-sh" ; pack  ]                         ->  Nuget.showPackage  pack

        | ["nuget"; "--open"]                               ->  openNugetWebsite ()           
        
        // | ["pack"; "--search"; pack ; "--repo"]          ->  searchLocalPackage pack "pacakges"
        // | ["pack"; "--search"; pack ; "--repo"; path]    ->  searchLocalPackage pack  path
        

        // ======  Commands to Handle NuGet package Archives ============== //
        | ["nupkg"; "--show"; fname]                        ->  Pget.Nupkg.show fname
        | ["nupkg"; "--files"; fname]                       ->  Pget.Nupkg.showFiles fname

        // ==========  Commands to Handle .NET assembly ============== //
        | ["asm" ; "--info" ;  asmFile]                     -> AsmAttr.showFile asmFile
        | ["asm" ; "--refs" ; asmFile ]                     -> AsmAttr.showAsmReferences asmFile         
        | ["asm" ; "--resources"; asmFile ]                 -> AsmAttr.showResurces asmFile

        | ["asm" ; "--namespace"; asmFile]                  -> AsmAttr.showNamespaces asmFile 
        | ["asm" ; "-ns"; asmFile]                          -> AsmAttr.showNamespaces asmFile 

        | ["asm" ; "--namespace"; asmFile ; "--class"; ns]  -> AsmAttr.showClassesInNamespace asmFile ns
        | ["asm" ; "-ns"; asmFile ; "-cls"; ns]             -> AsmAttr.showClassesInNamespace asmFile ns

        | ["asm" ; "--class" ; asmFile; "--public"; cls]    -> AsmAttr.showPublicMethods asmFile cls
        | ["asm" ; "--class" ; asmFile; "--static"; cls]    -> AsmAttr.showPublicStaticMethods asmFile cls
        | ["asm" ; "--class" ; asmFile; "--methods"; cls]  -> AsmAttr.showAllMethods asmFile cls

        | ["--guid" ]                                       -> Console.WriteLine(Guid.NewGuid().ToString() : string)

        | []                                                ->  showHelp ()
        | _                                                 ->  Console.WriteLine "Error: Invalid option."


    [<EntryPoint>]    
    let main (args) =    
        parseCommands args 
        0


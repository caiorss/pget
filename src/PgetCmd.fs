namespace Pget 

open System 
open System.Linq 
open NuGet

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

    let fsprojGenerateRefs framework repo =
        match Pget.Framework.parseFramework framework with
        | Some fr -> Pget.RepoLocal.generateFsprojInclude repo fr
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

    /// Go to pget online documentation 
    let openOlineDoc () =
        ignore <| System.Diagnostics.Process.Start ("https://caiorss.github.io/pget/")        

    /// Show system information. Useful for debugging.
    let showSystemInfo () =
        let systemVersion = System.Runtime
                                  .InteropServices
                                  .RuntimeEnvironment
                                  .GetSystemVersion()

        let runtimeDir = System.Runtime
                                .InteropServices
                                .RuntimeEnvironment
                                .GetRuntimeDirectory()


        let sysConfFile = System.Runtime
                                .InteropServices
                                .RuntimeEnvironment
                                .SystemConfigurationFile

        let localAppData  = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
        let appData       = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
        let commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)

        Console.WriteLine("System Information\n")
        Console.WriteLine("  System Version            {0}", systemVersion)
        Console.WriteLine("  System Directory          {0}", System.Environment.SystemDirectory)
        Console.WriteLine("  Runtime Directory         {0}", runtimeDir)
        Console.WriteLine("  System Configuration File {0}", sysConfFile)
        Console.WriteLine("  OS Version                {0}", System.Environment.OSVersion)
        Console.WriteLine("  Machine Name              {0}", System.Environment.MachineName)
        Console.WriteLine("  64-bit Operating system   {0}", System.Environment.Is64BitOperatingSystem)
        Console.WriteLine("  Number of processors      {0}", System.Environment.ProcessorCount)

        Console.WriteLine("\nSpecial Directories")
        Console.WriteLine("\n  System.Environment.SpecialFolder.CommonApplicationData \n  {0}", commonAppData)
        Console.WriteLine("\n  System.Environment.SpecialFolder.ApplicationData       \n  {0}", appData)
        Console.WriteLine("\n  System.Environment.SpecialFolder.LocalApplicationData  \n  {0}", localAppData)

        
    let showVersion () =
        let version = System.Reflection.Assembly
                                       .GetExecutingAssembly()
                                       .GetName()
                                       .Version.ToString()
        Console.WriteLine("""
 Pget - Package Get - Version {0} 
 2016 Public Domain Software
 Repository - https://github.com/caiorss/pget
        """, version)


    let showRepoHelp () =
        Console.WriteLine """
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

    repo --files [pack]                    Show content files of package [pack] in ./packages
    repo [path] --files [pack]             Show content files of package [pack] in [repo]

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
        """


    let showNupkgHelp () =
        Console.WriteLine """
  Nupkg Files:

    nupkg --show  [file]                        Show metadata of a *.nupkg file
    nupkg --files [file]                        Show files in nupkg [file]        
        """


    let showNugetHelp () =
        Console.WriteLine """
  Nuget commands:

    nuget --search [package]                    Search a package by name.  
    nuget --show   [package]                    Show package information (metadata).
    nuget --open                                Open NuGet web site - https://www.nuget.org       
        """    


    let showAsmHelp () =
        Console.WriteLine """
  Assembly files: *.exe or *.dll

    asm --info [file]                                    Show all assembly attributes from an assembly [file] or GAC.
    asm --refs [file]                                    Show all assembly references from an assembly [file] or GAC.
    asm --resources  [file]                              Show resources from an assembly file.
    asm --namespace|-ns [file]                           Show all exported namespaces.
    asm --namespace|-ns [file] [nspace]                  Show all types within an exported namespace from an assembly [file].

    asm --type [file]                                    Show all types exported by an assembly [file] or assembly in GAC.
    asm --type [file] [type]                             Show information about [type] exported by assembly [file] or GAC.

    asm --type-info [type]                               Try to find a type and show its information.

    asm --search-method [file] [type] [method]           Search and display all methods of type [type] which contains [method].


    asm --interface [file]                               Show all interfaces exported by assembly [file]. 
    asm --abstract  [file]                               Show all abstract classes exported by assembly [file].
   
    asm --class  [file]                                  Show all classes exported by assembly [file].
    asm --classn [file]                                  Show all non-abstract classes exported by assembly [file]

    asm --docgen [file]                                  Print all types documentation on screen
    asm --docgen [file] [output-file]                    Export all types documentation to org-mode file.
        """

    let showXmlHelp () =
        Console.WriteLine """
  XML query commands.

    xml --show [xmluri]            Show formatted xml human-readable output from uri or file.
    xml --show [xmluri] [file]     Export [xmluri] (file or url) to a xml human-readable file.

    xml --struct [xmluri]          Show xml structure - tags hierarchy
    xml --struct [xmluri] attr     Show xml structure tags hierarchy with attributes names.
    xml --struct [xmluri] ns       Show xml structure tags hierarchy with namespaces


    xml -ns                                  Show all XML namespaces
    xml --namespace                          Show all XML namespaces


    xml --xvalue [xpath] [xmluri]                 Query xml nodes using xpath and show its values.
    xml --xtext  [xpath] [xmluri]                 Query xml nodes using xpath and show its texts.
    xml --xtext --nons [xpath] [xmluri]           Query xml nodes using xpath and show its texts ignoring namespaces.
    xml --xattr  [xpath] [attr] [xmluri]          Query xml nodes using xpath and show the values of a node attribute.
    xml --xattr  --nons [xpath] [attr] [xmluri]   Similar to command above but ignore namespaces.

    """

    let showHelp () =
        Console.WriteLine("Pget - Package Get - Enhanced command line interface to NuGet.Core")
        Console.WriteLine("""
  pget.exe repo                                Show help for repo commands.
  pget.exe nuget                               Show help for nuget related commands                           
  pget.exe asm                                 Show help for assembly related commands.
  pget.exe nupkg                               Show help for Nuget packages related commands.
  pget.exe xml                                 Show help for xml related commands

  --version | -v                               Show version
  --help    | -h                               Show help
  --doc                                        Open online documentation.                          
                          
                          """)
        showRepoHelp()        
        showNugetHelp()
        showNupkgHelp()
        showAsmHelp()
        showXmlHelp()
        Console.WriteLine """
  Fsproj - Helpers for fsproj files.

    fsproj --ref [frm]                           Generate include references tags from all packages in ./packages    
                            

  Open pget online documentation:  https://caiorss.github.io/pget

    --doc 

  Show system information

    --system 

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
        | ["repo"]                                          ->  showRepoHelp ()
        
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

        | ["fsproj"; "--ref"; framework ]                   ->  fsprojGenerateRefs framework projectRepo
       
        // ============================ NuGet Repository (Remote) ========================== 
        | [ "nuget" ]                                       -> showNugetHelp ()
        
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
        | [ "nupkg" ]                                       ->  showNupkgHelp ()
           
        | ["nupkg"; "--show"; fname]                        ->  Pget.Nupkg.show fname
        | ["nupkg"; "--files"; fname]                       ->  Pget.Nupkg.showFiles fname

        // ==========  Commands to Handle .NET assembly ============== //
        | ["asm" ]                                          -> showAsmHelp ()
           
        | ["asm" ; "--info" ;  asmFile]                     -> AsmDisplay.showFile asmFile
        | ["asm" ; "--refs" ; asmFile ]                     -> AsmDisplay.showAsmReferences asmFile         
        | ["asm" ; "--resources"; asmFile ]                 -> AsmDisplay.showResurces asmFile

        /// Show all loaded assemblies and paths by current application
        | ["asm" ; "--show-loaded"]                         -> AsmDisplay.showLoadedAssemblies()

        /// Show location of GAC assembly
        | ["asm" ; "--show-path"; asmName]                  -> AsmDisplay.showAssemblyPath asmName

        // Show Exported namespaces
        | ["asm" ; "--namespace"; asmFile]                  -> AsmDisplay.showExportedNS asmFile
        | ["asm" ; "-ns"; asmFile]                          -> AsmDisplay.showExportedNS asmFile

        // Show types within an exported namespace
        | ["asm" ; "--namespace"; asmFile ; nspace]         -> AsmDisplay.showTypesWithinNS asmFile nspace
        | ["asm" ; "-ns"; asmFile ; nspace]                 -> AsmDisplay.showTypesWithinNS asmFile nspace


        // Show all exported types 
        | ["asm";  "--type" ; asmFile]                     -> AsmDisplay.showTypes asmFile

        | ["asm";  "--type-info" ; tname]                  -> AsmDisplay.showTypeInfo tname 

        // Show information about type exported by an assembly
        | ["asm";  "--type" ; asmFile ; atype]             -> AsmDisplay.showType asmFile atype

        // Show all exported classes  
        | ["asm";  "--class" ; asmFile]                     -> AsmDisplay.showClasses asmFile

        // Show all exported non-abstract class 
        | ["asm";  "--classn" ; asmFile]                    -> AsmDisplay.showClassesNonAbstract asmFile
        
        // Show all exported interfaces 
        | ["asm"; "--interface"; asmFile]                   -> AsmDisplay.showIntefaces asmFile

        // Show all abstract classes 
        | ["asm"; "--abstract" ; asmFile]                   -> AsmDisplay.showAbstractClasses asmFile


        | ["asm"; "--search-method"; asmFile; typeName; methodName]   -> AsmDisplay.searchMethod asmFile typeName methodName
        
        // Print a report with all types categorized by namespace. 
        | ["asm"; "--docgen" ; asmFile]                     -> AsmDisplay.showExportedTypesReport2 asmFile
        | ["asm"; "--docgen" ; asmFile ; reportFile]        -> AsmDisplay.genExportedTypesReport asmFile reportFile

        // =================== F# introspection ====================

        // Show all F# modules private and public
        | ["fsh"; "--modules"; "all"; asmFile ]    -> AsmDisplay.showFsharpModules asmFile false

        // Show only public F# modules
        | ["fsh";  "--modules";  asmFile ]          -> AsmDisplay.showFsharpModules asmFile true

        // Find all F# related Dll assemblies - It will search for all dlls containing F# modules.
        | ["fsh";  "--find-dll"; path ]             -> AsmDisplay.findFsharpDlls path


        // ===================  XML tools ==========================
        | ["xml"]                                                      -> showXmlHelp()
        
        // Pretty print XML 
        | ["xml"; "--show" ; xmlUri ]                                  -> FXml.File.show xmlUri
        | ["xml"; "--show" ; xmlUri ; xmlFile]                         -> FXml.File.save xmlUri xmlFile

        // Save xml without namespace to file
        | ["xml"; "--show-nons"; xmlUri; xmlFile]                 -> FXml.File.saveNoNs xmlUri xmlFile
                      
        // Show XML nodes structure 
        | ["xml"; "--struct" ;  xmlUri]                                -> FXml.File.showStruct xmlUri
        // Show XML nodes structure 
        | ["xml"; "--struct" ; "attr" ; xmlUri]                        -> FXml.File.showStructAttr xmlUri
        // Show XML nodes structure 
        | ["xml"; "--struct" ; "ns";  xmlUri]                          -> FXml.File.showStructNs xmlUri

        // Show XML Namespaces
        | ["xml"; "-ns"; xmlUri]                                       -> FXml.File.showNamespaces xmlUri
        | ["xml"; "--namespace"; xmlUri]                               -> FXml.File.showNamespaces xmlUri 
        
        // Select multiple nodes by xpath and show its values
        | ["xml"; "--xvalue" ; xpath ; xmluri]                         -> FXml.File.showXPathValue xmluri xpath

        // Select multiple modes by xpath and show its inner texts
        | ["xml"; "--xtext" ; xpath ; xmluri]                          -> FXml.File.showXPathInnerText xmluri xpath
        | ["xml"; "--xtext" ; "--nons"; xpath ; xmluri]                -> FXml.File.showXPathInnerTextNoNS xmluri xpath

        | ["xml"; "-ns" ; prefix ; uri ; "--xtext" ; xpath ; xmluri]   -> FXml.File.showXPathInnerTextNs xmluri (prefix, uri) xpath 

        // Select multiple nodes by xpath and show its attributes 
        | ["xml"; "--xattr"; xpath ; attr; xmluri]                     -> FXml.File.showXpathAttr xmluri xpath attr 
        | ["xml"; "--xattr"; "--nons"; xpath ; attr; xmluri]           -> FXml.File.showXpathAttrNoNS xmluri xpath attr

        // Print all xml node attributes in a tabular way
        | ["xml"; "--xattr-all"; xpath; xmluri]                        -> FXml.File.showXPathAttrAll xmluri xpath

        
        | ["xml"; "-ns"; prefix; uri ; "--xattr"; xpath; attr; xmluri] -> FXml.File.showXpathAttrNS xmluri (prefix, uri) xpath attr 


        // Select nodes by xpath and show them
        // | ["xml"; "--xnode"; "--nons"; xpath; xmluri]                           -> FXml.File.showXPathNodesNoNS xmluri xpath 
        
        // =================== Miscellaneous =======================

        // Generate Global Unique Identifier - Used to register com servers, fsproj and etc. 
        | ["--guid" ]                                       -> Console.WriteLine(Guid.NewGuid().ToString() : string)

        // Show system debugging information 
        | ["--system"]                                      -> showSystemInfo ()

        // Got to online documentation - open pget website.
        | ["--doc"]                                         -> openOlineDoc ()

        | []                                                ->  showHelp ()
        | _                                                 ->  Console.WriteLine "Error: Invalid option."


    [<EntryPoint>]    
    let main (args) =    
        parseCommands args 
        0


#if INTERACTIVE
#r "packages/NuGet.Core/lib/net40-Client/NuGet.Core.dll"
#r "packages/Microsoft.Web.Xdt/lib/net40/Microsoft.Web.XmlTransform.dll"
#r "System.Linq"
#endif

open NuGet
open System
open System.Linq

module IPFile =

    type T = NuGet.IPackageFile
   
    let removeSuffix (suffix: string) (s: string) =
        if s.EndsWith(suffix)
        then s.Substring(0, s.Length - suffix.Length)
        else s        

    let name (p: T) =
        removeSuffix ".dll" p.EffectivePath
    
    let effectivePath (p: T): string  =
        p.EffectivePath       

    let path (p: T) =
        p.Path

    let frameworks (p: T) =
        p.SupportedFrameworks
        

/// Package object Assessors 
module IPack =
    type T = NuGet.IPackage

    /// Get summary 
    let summary (pack: T) = pack.Summary

    /// Get package title 
    let title  (pack: T) =   pack.Title
    
    let version (pack: T) = pack.Version

    let versionString (pack: T) = pack.Version.ToString()

    let description (pack: T) = pack.Description

    let references (pack: T) = pack.AssemblyReferences

    let dependencies (pack: T) = pack.DependencySets

    let projectUrl (pack: T) = pack.ProjectUrl.ToString()

    let isReleaseVersion (pack: T): bool = pack.IsReleaseVersion()
   
    let isLastestVersion (pack: T) = pack.IsLatestVersion

    let fullName (pack: T) = pack.Id + "." + pack.Version.ToString()

    let authors (pack: T) = pack.Authors

    let getLibFiles (pack: T): seq<NuGet.IPackageFile> = pack.GetLibFiles()

    let getLibDllFiles (pack: T): seq<NuGet.IPackageFile>  =
        pack.GetLibFiles() |> Seq.filter (fun p -> p.Path.EndsWith(".dll"))

    let getLibDllFilesCompatible (pack: T) framework =
        pack
        |> getLibDllFiles
        |> Seq.filter (fun p -> NuGet.VersionUtility.IsCompatible(framework, p.SupportedFrameworks))
        
    let getLibDllFilesCompatibleByName (pack: T) framework =
        let fmr = new System.Runtime.Versioning.FrameworkName(framework) 
        pack
        |> getLibDllFiles
        |> Seq.filter (fun p -> NuGet.VersionUtility.IsCompatible(fmr, p.SupportedFrameworks))

    let getDllFilesPathCompatible (pack: T) framework =
        let ipackDllFiles = getLibDllFilesCompatibleByName pack framework
        Seq.map IPFile.path ipackDllFiles

    let getDllFilesRefsCompatible repoPath framework (pack: T)   =
        let ipackDllFiles = getLibDllFilesCompatibleByName pack framework
        let fname = fullName pack        
        Seq.map  (fun ip -> System.IO.Path.Combine(repoPath, fname, IPFile.path ip))
                 ipackDllFiles
                 
    let getDllFilesRefsCompatibleUnique repoPath framework (pack: T)   =
        let ipackDllFiles = getLibDllFilesCompatibleByName pack framework
        let fname = fullName pack        
        
        ipackDllFiles
        |> Seq.groupBy (fun (p: NuGet.IPackageFile) -> p.EffectivePath)
        |> Seq.map (fun (k, v) -> System.IO.Path.Combine(repoPath, fname, IPFile.path <| Seq.last v))

                                           
module Repo =
    
    type R = NuGet.IPackageRepository

    let parseVersion (version: string) =
        try
            Some (NuGet.SemanticVersion.Parse(version))
        with
            :? System.ArgumentException -> None
        

    let findPackagesById (repo: R) (packageId: string) =
        repo.FindPackagesById (packageId)

    let findPackageById (repo: R) (packageId: string): NuGet.IPackage option =
        let packs = repo.FindPackagesById (packageId)

        try Some (Seq.item 0 packs)
        with
            :? System.ArgumentException -> None

    let searchPackagesById packageId  (repo: R) =
        repo.GetPackages().Where(fun (p: IPackage) -> p.Id.Contains(packageId))
        |> Seq.groupBy(fun p -> p.Id)             
        |> Seq.map (fun (k, v) -> Seq.last v)     
       

    let findLatestStableVersion (repo: R) (packageId: string) =
        let package = packageId
                      |> findPackagesById repo
                      |> Seq.filter IPack.isReleaseVersion
                      |> Seq.last
        package.Version.ToString()
        // |> Seq.tryFind IPack.isLastestVersion
       
    let createRepository (uri: string): R =
        NuGet.PackageRepositoryFactory.Default.CreateRepository(uri)

    let localRepository (relPath: string): R =
        PackageRepositoryFactory.Default.CreateRepository(System.IO.Path.GetFullPath(relPath))

    let getPackages (repo: R) =
        repo.GetPackages()

    module PM =

        type T = NuGet.PackageManager
        
        let makePackageManager repo path =
            new NuGet.PackageManager(repo, path)

        let installPackage (pm: T) (package, version) =
            let ver = parseVersion version
            match ver with
            | Some v ->  pm.InstallPackage(package, v)
            | None   ->  failwith "Error: Wrong version name"

        let installPackageLatest (pm: T) package repo =
            let ver = findLatestStableVersion repo package
            installPackage pm (package, ver)


module Nuget =

    let nugetV2Repo = "https://packages.nuget.org/api/v2"

    let nugetV2 = Repo.createRepository nugetV2Repo

    let findPackageById = Repo.findPackageById nugetV2

    let findPackagesById = Repo.findPackagesById nugetV2

    let installPackage repoPath (package, version) =
        let pm = Repo.PM.makePackageManager nugetV2 repoPath
        Repo.PM.installPackage pm (package, version)

    // let findLatestPackageById = Repo.findLatestStableVersion nugetV2

    let installPackageLatest repoPath package =
        let version = Repo.findLatestStableVersion nugetV2 package
        printfn "Installing: %s %s" package version
        installPackage repoPath (package, version)


module Cmd =

    let commandLineArgsInteractive () =
        let args = Environment.GetCommandLineArgs()
        let idx = Array.tryFindIndex (fun a -> a = "--") args
        match idx with
        | None -> [||]
        | Some i -> args.[(i+1)..]

    let showPackage (p: NuGet.IPackage) =
        Console.WriteLine("Id:\t\t{0}\nVersion:\t{1}\nTitle:\t\t{2}\nSummary:\t\t{3}\nAuthors:\t{4}\nUrl:\t\t{5}\nDescription:\t{6}\n\n",
                          p.Id,
                          p.Version,
                          p.Title,
                          p.Summary,
                          String.concat ", " (Array.ofSeq p.Authors),
                          p.ProjectUrl,
                          p.Description
                          )

    let showRepository repoPath =
        Repo.localRepository repoPath
        |> Repo.getPackages
        |> Seq.iter showPackage

    let showPackageList repoPath =
        Repo.localRepository repoPath
        |> Repo.getPackages
        |> Seq.iter (fun p -> printfn "%A" p)

    let showPackageRefs repoPath framework packageId =
         let repo =  Repo.localRepository repoPath
         let pack =  Repo.findPackageById repo packageId
         match pack with
         | None       ->  printfn "Error: package not found."
         | Some pack' ->  IPack.getDllFilesRefsCompatibleUnique repoPath framework pack'
                          |> Seq.iter (fun p -> Console.WriteLine p)


    let showLocalRepoRefs repoPath frameWork =
        repoPath
        |> Repo.localRepository
        |> Repo.getPackages
        |> Seq.collect (IPack.getDllFilesRefsCompatibleUnique repoPath frameWork)
        |> Seq.iter (printfn "%s")


    let searchPackageByName packageId =               
        Nuget.nugetV2
        |> Repo.searchPackagesById packageId
        |> Seq.iter showPackage //(fun p -> printfn "%A" p)

    let installPackage repoPath packageId version =
        Nuget.installPackage repoPath (packageId, version)
       
        
    let parseCommands args =
        match args with
        | [||]                                               -> printfn "Error: empty args"
        | [| "--list-packages" |]                            -> showRepository "packages"
        | [| "--list-packages" ; repo |]                     -> showRepository repo      
        | [| "--package-ref" ; repo ; framework ; pack|]     -> showPackageRefs repo framework pack
        | [| "--package-ref-net40" ; repo ; pack|]           -> showPackageRefs repo ".NETFramework,Version=v4.0" pack
        | [| "--package-ref-net45" ; repo ; pack|]           -> showPackageRefs repo ".NETFramework,Version=v4.5" pack
        | [| "--packages" ; repo |]                          -> showPackageList repo        
        | [| "--packages" |]                                 -> showPackageList "packages"
        | [| "--search"; packageId |]                        -> searchPackageByName packageId
        | [| "--local" ; "--install" ; packageId; version |] -> installPackage "packages" packageId version
        | [| "--install"; repo; packageId ; version |]       -> installPackage repo packageId version
        | [| "--install"; repo ; packageId  |]               -> Nuget.installPackageLatest repo packageId
        | [| "--show"; packageId |]                          -> Option.iter showPackage (Nuget.findPackageById packageId)
        | _                                                  -> printfn "Error: Invalid commands"
        


let main() =    
    // printfn "env.cmdline: %A" <| commandLineArgsInteractive ()
    Cmd.parseCommands <| Cmd.commandLineArgsInteractive ()
    0

main() 

// NuGet.SemanticVersion.Parse("a5.0.0")    

// NuGet.PackageManager.Install

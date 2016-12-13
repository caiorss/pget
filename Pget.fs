namespace Pget 

// #if INTERACTIVE
// #r "packages/NuGet.Core.2.12.0/lib/net40-Client/NuGet.Core.dll"
// #r "packages/Microsoft.Web.Xdt.2.1.1/lib/net40/Microsoft.Web.XmlTransform.dll"
// #r "System.Linq.dll"
// #endif

open NuGet
open System
open System.Linq

type EnumIPack =  System.Collections.Generic.IEnumerable<NuGet.IPackage> 

// Framework Version Constants
//
module Framework =
    let net40 =  ".NETFramework,Version=v4.0"
    let net45 =  ".NETFramework,Version=v4.5"

///  Wrapper for Nuget.IPackageFile class
///
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
///        
module IPack =
    
    type T = NuGet.IPackage

    /// Get summary 
    let summary (pack: T) = pack.Summary

    /// Get package title 
    let title  (pack: T) =   pack.Title

    /// Get package version 
    let version (pack: T) = pack.Version

    /// Get package Version as string 
    let versionString (pack: T) = pack.Version.ToString()

    /// Get package Description as String 
    let description (pack: T) = pack.Description

    /// Get package References 
    let references (pack: T) = pack.AssemblyReferences

    /// Get package Dependencies 
    let dependencies (pack: T) = pack.DependencySets

    /// Get project URL 
    let projectUrl (pack: T) = pack.ProjectUrl.ToString()

    /// Returns true if it as release version 
    let isReleaseVersion (pack: T): bool = pack.IsReleaseVersion()
   
    let isLastestVersion (pack: T) = pack.IsLatestVersion

    /// Package full name  <Package ID>.<Version>
    let fullName (pack: T) = pack.Id + "." + pack.Version.ToString()

    /// Get package authors 
    let authors (pack: T) = pack.Authors

    /// Get library files 
    let getLibFiles (pack: T): seq<NuGet.IPackageFile> = pack.GetLibFiles()

    /// Get all *.dll references in the package 
    let getLibDllFiles (pack: T): seq<NuGet.IPackageFile>  =
        pack.GetLibFiles() |> Seq.filter (fun p -> p.Path.EndsWith(".dll"))

    /// Get Dll file compatible with framework     
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


    let zipPackage (nupkgFile: string) =  NuGet.ZipPackage nupkgFile


    let getDependencies (pack: T) =
        pack.DependencySets
        |> Seq.map (fun dep -> dep.Dependencies)
        |> Seq.collect (Seq.map ( fun p -> (p.Id, match p.VersionSpec with
                                                  | null -> None
                                                  | v    -> Some v
                                    )))

    let getDependenciesAsString (pack: T) =
        let deplist = getDependencies pack
                      |> Seq.map (fun (name, ver) -> match ver with
                                                     | None   ->  name
                                                     | Some v ->  name + " " + v.ToString()
                                  )
        String.Join(", ", deplist)

    /// Print package data in Command line     
    let showPackage (p: T) = 
        Console.WriteLine("""
Id            {0}
Title         {2}
Tags          {7}
Version       {1}
Summary       {3}  
Authors       {4}
Project URL   {5}
Dependencies  {8}
Description   {6}                         
                          """,
                          p.Id,
                          p.Version,
                          p.Title,
                          p.Summary,
                          String.concat ", " (Array.ofSeq p.Authors),
                          p.ProjectUrl,
                          p.Description,
                          p.Tags,
                          getDependenciesAsString p
                          )
    /// Print the content of a NuGet package file (*.nupkg) file.
    let showZipPackage nupkg =  nupkg |> zipPackage |> showPackage    


/// This module provides NuGet Repository object assessors
///        
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

    let searchPackageById (repo: R) (packageId: string) =
        repo.GetPackages().Where(fun (p: IPackage) -> p.Id.ToLower().Contains(packageId.ToLower()))
        // |> Seq.groupBy(fun p -> p.Id)
        // |> Seq.map (fun (k, v) -> Seq.last v)


    let searchPackages (repo: R) (input: string)  =
        repo.GetPackages().Where(fun (p: IPackage) -> p.Id.Contains(input)
                                                      || p.Title.Contains(input)
                                                      || p.Description.Contains(input)
                                 )
        |> Seq.groupBy(fun p -> p.Id)               // Remove repeated packages 
        |> Seq.map (fun (k, v) -> Seq.last v)     


    // Doesn't work - Error:   FS0039: The field, constructor or member
    // 'Contatins' is not defined
    // -------------------------
    let filterPackages predicate  (repo: R) =
        // repo.GetPackages().Where(fun (p: IPackage) -> predicate p)
        repo.GetPackages()
        |> Seq.filter (fun (p: IPackage) -> predicate p)
        |> Seq.groupBy(fun p -> p.Id)             
        |> Seq.map (fun (k, v) -> Seq.last v)           
       

    let findLatestStableVersion (repo: R) (packageId: string) =
        let package = packageId
                      |> findPackagesById repo
                      |> Seq.filter IPack.isReleaseVersion
                      |> Seq.last
        package.Version.ToString()
        // |> Seq.tryFind IPack.isLastestVersion

    /// Creates a repository given its uri     
    let createRepository (uri: string): R =
        NuGet.PackageRepositoryFactory.Default.CreateRepository(uri)

    /// Get packages from a repository object     
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

/// Provides functions to deal with local repository (directory with NuGet packages)
///
module RepoLocal =

    type R = NuGet.IPackageRepository

    let findPackageById (repo: R) (packageId: string): NuGet.IPackage option =
        let packs = repo.FindPackagesById (packageId)

        try Some (Seq.item 0 packs)
        with
            :? System.ArgumentException -> None

    /// Creates a local repository
    let localRepository (relPath: string) =
        PackageRepositoryFactory.Default.CreateRepository(System.IO.Path.GetFullPath(relPath))

    //// Returns all packages from a local repository
    let getPackages (relPath: string) =
        let repo = localRepository relPath
        repo.GetPackages ()

    /// Show all details packages from a local repository
    ///
    let showPackages (relPath: string) =
        relPath
        |> getPackages
        |> Seq.iter IPack.showPackage

    /// Show the IDs of all packages in local repository
    ///
    let showPackageList repoPath =
        localRepository repoPath
        |> Repo.getPackages
        |> Seq.iter (fun p -> printfn "%A" p)

    let getPackageRefs repoPath framework packageId =
         packageId
         |> findPackageById (localRepository repoPath)
         |> Option.map (IPack.getDllFilesRefsCompatibleUnique repoPath framework)
    // Search packages which Id has word.
    //
    let searchPackageById repoPath query  =
        Repo.searchPackageById (localRepository repoPath) query

module Nuget =
    
    let private nugetV2Repo = "https://packages.nuget.org/api/v2"

    let nugetV2 = Repo.createRepository nugetV2Repo

    let findPackageById = Repo.findPackageById nugetV2

    let findPackagesById: string -> EnumIPack = Repo.findPackagesById nugetV2

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


    let showRepository repoPath =
        Repo.Local.localRepository repoPath
        |> Repo.getPackages
        |> Seq.iter IPack.showPackage

    let showPackageList repoPath =
        Repo.Local.localRepository repoPath
        |> Repo.getPackages
        |> Seq.iter (fun p -> printfn "%A" p)

//     let showPackageRefs repoPath framework packageId =
//          let repo =  Repo.Local.localRepository repoPath
//          let pack =  Repo.findPackageById repo packageId
//          match pack with
//          | None       ->  printfn "Error: package not found."
//          | Some pack' ->  IPack.getDllFilesRefsCompatibleUnique repoPath framework pack'
//                           |> Seq.iter (fun p -> Console.WriteLine p)


//     let showLocalRepoRefs repoPath frameWork =
//         repoPath
//         |> Repo.localRepository
//         |> Repo.getPackages
//         |> Seq.collect (IPack.getDllFilesRefsCompatibleUnique repoPath frameWork)
//         |> Seq.iter (printfn "%s")

//     let generateLocalRefDirective repoPath frameWork packageId =
//          let repo =  Repo.localRepository repoPath
//          let pack =  Repo.findPackageById repo packageId
//          match pack with
//          | None       ->  printfn "Error: package not found."
//          | Some pack' ->  IPack.getDllFilesRefsCompatibleUnique repoPath frameWork pack'
//                           |> Seq.iter (printfn "#r \"%s\"")      
               

//     let searchPackageByName packageId =               
//         Nuget.nugetV2
//         |> Repo.searchPackagesById packageId
//         |> Seq.iter IPack.showPackage //(fun p -> printfn "%A" p)

//     let installPackage repoPath packageId version =
//         Nuget.installPackage repoPath (packageId, version)
       
        
//     let parseCommands args =
//         match args with
//         | [||]                                               -> printfn "Error: empty args"
//         | [| "--list-packages" |]                            -> showRepository "packages"
//         | [| "--list-packages" ; repo |]                     -> showRepository repo      
//         | [| "--package-ref" ; repo ; "net40";  pack|]       -> showPackageRefs repo ".NETFramework,Version=v4.0" pack
//         | [| "--package-ref" ; repo ; "net45" ; pack|]       -> showPackageRefs repo ".NETFramework,Version=v4.5" pack
//         | [| "--package-ref" ; repo ; framework ; pack |]    -> showPackageRefs repo framework pack
//         | [| "--package-fsx" ; repo ; "net45" ; pack |]      -> generateLocalRefDirective repo ".NETFramework,Version=v4.5" pack
//         | [| "--packages" ; repo |]                          -> showPackageList repo        
//         | [| "--packages" |]                                 -> showPackageList "packages"
//         | [| "--packages-refs"; "net40" ; repo |]            -> showLocalRepoRefs repo ".NETFramework,Version=v4.0"   
//         | [| "--packages-refs"; "net45" ; repo |]            -> showLocalRepoRefs repo ".NETFramework,Version=v4.5"   
//         | [| "--packages-refs"; "net40" |]                   -> showLocalRepoRefs "packages" ".NETFramework,Version=v4.0"
//         | [| "--packages-refs"; "net45" |]                   -> showLocalRepoRefs "packages" ".NETFramework,Version=v4.5"   
//         | [| "--search"; packageId |]                        -> searchPackageByName packageId
//         | [| "--local" ; "--install" ; packageId; version |] -> installPackage "packages" packageId version
//         | [| "--install"; repo; packageId ; version |]       -> installPackage repo packageId version
//         | [| "--install"; repo ; packageId  |]               -> Nuget.installPackageLatest repo packageId
//         | [| "--show"; packageId |]                          -> Option.iter IPack.showPackage (Nuget.findPackageById packageId)
//         | _                                                  -> printfn "Error: Invalid commands"
        


// let main() =    

//     Cmd.parseCommands <| Cmd.commandLineArgsInteractive ()
//     0

// main() 


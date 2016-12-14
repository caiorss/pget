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

// Framework Version related functions
//
module Framework =

    /// Framework .NET 4.0
    let net40 =  ".NETFramework,Version=v4.0"

    /// Framework .NET 4.5
    let net45 =  ".NETFramework,Version=v4.5"

    let makeSemanticVersion (version: string) = new NuGet.SemanticVersion(version)

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

    let packageId (pack: T) = pack.Id

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

    /// Get a package's number of downloads in NuGet package index.
    let downloadCount (pack: T) = pack.DownloadCount

    /// Returns supported frameworks by a package.
    let supportedFrameworks (pack: T) = pack.GetSupportedFrameworks()

    /// Returns true if package is listed in NuGet package index.
    let listed (pack: T) = pack.Listed

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

    // Returns all assemble references in a local repository given its path
    // and framework version.
    //
    let getRefsUniqueNoVersion repoPath framework (pack: T)   =
        let ipackDllFiles = getLibDllFilesCompatibleByName pack framework
        let fname = packageId pack

        ipackDllFiles
        |> Seq.groupBy (fun (p: NuGet.IPackageFile) -> p.EffectivePath)
        |> Seq.map (fun (k, v) -> System.IO.Path.Combine(repoPath, fname, IPFile.path <| Seq.last v))

    /// Get a package's dependencies and its versions
    ///
    let getDependencies (pack: T) =
        pack.DependencySets
        |> Seq.map (fun dep -> dep.Dependencies)
        |> Seq.collect (Seq.map ( fun p -> (p.Id, match p.VersionSpec with
                                                  | null -> None
                                                  | v    -> Some v
                                    )))


     /// Get NuGet package dependencies as string
     ///  - "dependency1.version1, dependency2.version2 ..."
     ///
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
Id             {0}
Title          {2}
Tags           {7}
Version        {1}
Summary        {3}
Authors        {4}
Project URL    {5}
Dependencies   {8}
Download Count {9}

Description    {6}
                          """,
                          p.Id,
                          p.Version,
                          p.Title,
                          p.Summary,
                          String.concat ", " (Array.ofSeq p.Authors),
                          p.ProjectUrl,
                          p.Description,
                          p.Tags,
                          getDependenciesAsString p,
                          p.DownloadCount
                          )

/// Module to deal with NuGet package files -  *.nupkg files
///    
module Nupkg =
    
    /// Read a .nupkg file.
    let read (nupkgFile: string) =  NuGet.ZipPackage nupkgFile

    /// Print the content of a NuGet package file (*.nupkg) file.
    let show (nupkgFile: string) =
        nupkgFile
        |> read
        |> IPack.showPackage

    /// Get files all files of NuGet package archive.
    ///    
    let getFiles (nupkgFile: string) =
        let pack = read nupkgFile
        pack.GetFiles()
           

/// This module provides NuGet Repository object assessors
///        
module Repo =
    
    type R = NuGet.IPackageRepository

    /// Try to parse a package version
    ///
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


module Nuget = 

    /// NuGet Version 2 API 
    let  nugetV2url = "https://packages.nuget.org/api/v2"

    /// NuGet version 3 API 
    let  nugetV3url = "https://packages.nuget.org/api/v3"


    let nugetV2 = Repo.createRepository nugetV2url

    let findPackageById = Repo.findPackageById nugetV2

    let findPackagesById: string -> EnumIPack = Repo.findPackagesById nugetV2

    /// Returns a seq with all F# related packages
    let findFsharpPackages () =
        let repository = NuGet.PackageRepositoryFactory.Default.CreateRepository "https://nuget.org/api/v2"

        query { for p in repository.GetPackages() do
                where (
                        p.Id.ToLower().Contains("fsharp")
                        || p.Id.ToLower().Contains("f#")
                        || p.Tags.ToLower().Contains("f#")
                        || p.Tags.ToLower().Contains("fsharp")
                )
                select p
               }


/// Provides functions to deal with local repository (directory with NuGet packages)
///
module RepoLocal =

    type R = NuGet.IPackageRepository

    /// Find a package by Id in local repository
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


    /// Get relative path to assemblies from a package without
    /// version in the path. It is assumed that only one version
    /// is installed in the local repository.
    ///
    let getPackageRefsNoVersion repoPath framework packageId =
         packageId
         |> findPackageById (localRepository repoPath)
         |> Option.map (IPack.getRefsUniqueNoVersion repoPath framework)

    let searchPackageById repoPath query  =
        Repo.searchPackageById (localRepository repoPath) query

    /// Get references fo all local packages *.dll compatible with given framework.
    ///
    let getRefsNoVersion repoPath frameWork =
        repoPath
        |> getPackages
        |> Seq.collect (IPack.getRefsUniqueNoVersion repoPath frameWork)

    /// Install a package to local repository
    ///    
    let installPackage repoPath (package, version) =
        let pm = Repo.PM.makePackageManager Nuget.nugetV2 repoPath
        Repo.PM.installPackage pm (package, version)

    /// Install the latest version of a package to local repository
    ///     
    let installPackageLatest repoPath package =
        let version = Repo.findLatestStableVersion Nuget.nugetV2 package
        printfn "Installing: %s %s" package version
        installPackage repoPath (package, version)

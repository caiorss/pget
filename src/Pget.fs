/// Pget - F# Functional wrapper to NuGet.Core package.
///
/// <description>
/// The Pget module that stands for Package-Get provides functions
/// to read and manipulate NuGet packages and query NuGet repository.
/// </description>
///
namespace Pget 

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
    /// Parse .NET framewok from string
    let parseFramework s =
        match s with
        | "net35" -> Some ".NETFramework,Version=v3.5"
        | "net40" -> Some ".NETFramework,Version=v4.0"
        | "net45" -> Some ".NETFramework,Version=v4.5"
        |  _      -> None

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
    let summary (pack: T) = Option.ofObj pack.Summary

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
    let projectUrl (pack: T) = Option.ofObj pack.ProjectUrl
                               |> Option.map (fun uri -> uri.ToString())

    /// Return License URL
    let licenseUrl (pack: T) = Option.ofObj pack.LicenseUrl
                               |> Option.map (fun uri -> uri.ToString())

    /// Returns true if it as release version 
    let isReleaseVersion (pack: T): bool = pack.IsReleaseVersion()
   
    let isLastestVersion (pack: T) = pack.IsLatestVersion

    /// Return package Icon URL
    let iconUrl (pack: T) = pack.IconUrl
                            |> Option.ofObj
                            |> Option.map (fun uri -> uri.ToString())

    /// Return package's tags
    let tags (pack: T) = Option.ofObj pack.Tags

    /// Return package's release notes
    let releaseNotes (pack: T) = Option.ofObj pack.ReleaseNotes

    let copyRight (pack: T) = pack.Copyright

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

    /// Get library files path
    let getLibFilesPath (pack: T): seq<string> =
        pack.GetFiles() |> Seq.map (fun p -> p.Path)

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


    let showAllPackages packSeq = Seq.iter showPackage (packSeq: IQueryable<IPackage>)



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

    /// Print all files of NuGet package archive.
    let showFiles (nupkgFile: string) =
        NuGet.ZipPackage(nupkgFile).GetFiles()
        |> Seq.iter (fun p -> Console.WriteLine(p))


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

    let searchPackageById (repo: R) (packageId: string) : EnumIPack =
        repo.GetPackages().Where(fun (p: IPackage) -> p.Id.ToLower().Contains(packageId.ToLower()))
        |> Seq.groupBy(fun p -> p.Id)
        |> Seq.map (fun (k, v) -> Seq.last v)


    let searchPackage (repo: R) (input: string)  =
        repo.GetPackages().Where(fun (p: IPackage) -> let inp = input.ToLower() in
                                                          p.Id.ToLower().Contains(inp)
                                                      || p.Title.ToLower().Contains(inp)
                                                      || p.Tags.ToLower().Contains(inp)
                                                      || p.Description.ToLower().Contains(inp)
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
       
    /// Returns the latest stable version of a package in a given repository object
    ///
    let findLatestStableVersion (repo: R) (packageId: string) =
        let package = packageId
                      |> findPackagesById repo
                      |> Seq.filter IPack.isReleaseVersion
                      |> Seq.tryLast
        package |> Option.map (fun pack -> pack.Version.ToString())
        // |> Seq.tryFind IPack.isLastestVersion

    /// Creates a repository given its uri     
    let createRepository (uri: string): R =
        NuGet.PackageRepositoryFactory.Default.CreateRepository(uri)

    /// Get packages from a repository object     
    let getPackages (repo: R) =
        repo.GetPackages()

    /// PackageManager class wrapper
    module PM =

        type T = NuGet.PackageManager
        
        let makePackageManager repo path =
            new NuGet.PackageManager(repo, path)

        let installPackage (pm: T) (package, version) =
            let ver = parseVersion version
            match ver with
            | Some v ->  pm.InstallPackage(package, v)
            | None   ->  failwith "Error: Wrong version name"

        let installPackageLatest (pm: T) packageId repo =
            let ver = findLatestStableVersion repo packageId
            match ver with
            | Some v -> installPackage pm (packageId, v)
            | None   -> printfn "Error: package %s not found" packageId


module Nuget = 

    /// NuGet Version 2 API 
    let  nugetV2url = "https://packages.nuget.org/api/v2"

    /// NuGet version 3 API 
    let  nugetV3url = "https://packages.nuget.org/api/v3"


    let nugetV2 = Repo.createRepository nugetV2url

    let findPackageById = Repo.findPackageById nugetV2

    /// Try find a package in NuGet repository and print its data
    ///
    let showPackage packageId =
        let pack = Repo.findPackageById nugetV2 packageId
        match pack with
        | Some pk -> IPack.showPackage pk
        | None    -> printfn "Error: I can't find the package %s" packageId

    let searchPackagesById: string -> EnumIPack = Repo.searchPackageById nugetV2

    // let findPackagesById: string -> EnumIPack = Repo.searchPackages nugetV2

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

    /// Creates a repository object given a path to local repository
    let localRepository (relPath: string) =
        PackageRepositoryFactory.Default.CreateRepository(System.IO.Path.GetFullPath(relPath))

    /// Find a package by Id in a repository object
    let findPackageById (repo: R) (packageId: string): NuGet.IPackage option =
        let packs = repo.FindPackagesById (packageId)
        Seq.tryItem 0 packs

    /// Find a package by Id  in a repository given its path.
    let findPackageById2 (repoPath: string) (packageId: string): NuGet.IPackage option =
        findPackageById (localRepository repoPath) packageId

    //// Returns all packages from a local repository
    let getPackages (repoPath: string) =
        let repo = localRepository repoPath
        repo.GetPackages ()


    /// Show a package given its repository path and ID
    let showPackage  (repoPath: string) (packageId: string) =
        let repo = localRepository repoPath
        let pack = findPackageById repo packageId
        match pack with
        | Some p -> IPack.showPackage p
        | None   -> printfn "Couldn't find the package %s" packageId


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

    //// Show all package files of a package in local repository 
    let showPackageFilesRel repoPath packageId =
        let repo = localRepository repoPath
        
        let libfilesOpt = findPackageById repo packageId
                       |> Option.map IPack.getLibFilesPath
                       
        match libfilesOpt with
        | Some libfiles    -> Seq.iter (fun (path: string) -> Console.WriteLine path) libfiles
        | None             -> printfn "Error: Package not found in %s repository." repoPath               


    //// Show all files' paths of a package in local repository.
    ///    
    let showPackageFiles repoPath packageId =
        let repo = localRepository repoPath

       
        let libfilesOpt = findPackageById repo packageId
                       |> Option.map (fun pack ->
                                      let packPath = System.IO.Path.Combine(repoPath, IPack.fullName pack)

                                      IPack.getLibFilesPath pack
                                      |> Seq.map (fun filePath -> 
                                                  System.IO.Path.Combine (packPath, filePath)))
                       
        match libfilesOpt with
        | Some libfiles    -> Seq.iter (fun (path: string) -> Console.WriteLine path) libfiles
        | None             -> printfn "Error: Package not found in %s repository." repoPath               


    /// Get relative path to assemblies from a package
    ///
    let getPackageRefs repoPath framework packageId =
         packageId
         |> findPackageById (localRepository repoPath)
         |> Option.map (IPack.getDllFilesRefsCompatibleUnique repoPath framework)

    let showPackageRefsFsx repoPath framework packageId =
         getPackageRefs repoPath framework packageId
         |> Option.iter (Seq.iter (printfn "#r \"%s\""))


    let searchPackage repoPath query  =
        Repo.findPackagesById (localRepository repoPath) query

    /// Get references fo all local packages *.dll compatible with given framework.
    ///
    let getRefsNoVersion repoPath frameWork =
        repoPath
        |> getPackages
        |> Seq.collect (IPack.getRefsUniqueNoVersion repoPath frameWork)

    /// Get all assembly references path (*.dll files)
    let getRefs repoPath frameWork =
        repoPath
        |> getPackages
        |> Seq.collect (IPack.getDllFilesRefsCompatibleUnique repoPath frameWork)

    // @DONE: Implement function generateFSprojInclude to include references in a fsproj file.
    let generateFsprojInclude repoPath framework = 
        getRefs repoPath framework
        |> Seq.map (fun reference ->
                    let refname = System.IO.Path.GetFileNameWithoutExtension reference
                    let out = sprintf """
<Reference Include="%s"> 
     <HintPath>%s</HintPath>
</Reference>
                                      """ refname  reference

                    out.Trim()
                    )
        |> Seq.iter Console.WriteLine


    /// Generate a script to load all packages from the repository into REPL.
    ///
    let generateScript repoPath framework =
        let reflist = getRefs repoPath framework
                      |> Seq.map(fun r -> sprintf "#r \"%s\"" r)
                      |> Seq.toArray
        String.Join("\n", reflist)

    let showScript framework repoPath =
        generateScript repoPath framework |> Console.WriteLine

    /// Install a package to local repository
    ///    
    let installPackage repoPath (package, version) =
        let pm = Repo.PM.makePackageManager Nuget.nugetV2 repoPath
        Repo.PM.installPackage pm (package, version)

    /// Install a package to local repository, however
    /// instead of throw an exception, it prints an error message
    /// if the package is not found.
    ///
    let installPackageSafe repoPath (package, version) =
        try
            installPackage repoPath (package, version)
        with
            :? System.InvalidOperationException -> printfn "Error: I can't find the package %s-v%s" package version

    /// Install the latest version of a package to local repository
    ///     
    let installPackageLatest repoPath package =
        let version = Repo.findLatestStableVersion Nuget.nugetV2 package
        match version with
        | None   -> printfn "Error: I can't find the package %s" package
        | Some v -> printfn "Installing: %s %s" package v
                    installPackage repoPath (package, v)

    /// Install a list of packages to a given repository path
    /// like [ "FSharp.Data"; "FParsec-1.0.0" ; "NuGet.Core-2.0"; "OxyPlot.Core"]
    ///
    /// If there is no version number it installs the package latest version.
    ///
    let installPackageList repoPath (packageList: string seq) =
        packageList
        |> Seq.iter (fun entry ->
                         let row =  entry.Split([|'-'|], System.StringSplitOptions.RemoveEmptyEntries)
                         match List.ofArray row with
                         | [package; version  ]  -> installPackageSafe repoPath (package, version)
                         | [ package          ]  -> installPackageLatest repoPath package
                         | []                    -> printfn "Error: No packages to install."
                         | _                     -> printfn "Error: Wrong package format."
                     )

    /// Install all packages listed in a file to a given repository
    ///
    /// - repoPath           - Directory the packages will be installed.
    /// - packageListFile    - File containing the list of packages to be installed.
    ///
    let installPackagesFromFile repoPath packageFile =
        packageFile
        |> System.IO.File.ReadAllLines
        |> Array.iter (fun (line: string) ->
                       let row = line.Split([|' '|], System.StringSplitOptions.RemoveEmptyEntries)
                       // printfn "package = %A" row
                       match List.ofArray row with
                       // | [ package ; "-"     ] -> installPackageLatest repoPath package
                       | [ package ; version ] -> installPackage repoPath (package, version)
                       | [ package           ] -> installPackageLatest repoPath package
                       | []                    -> failwith "Error: No package listed"
                       | _                     -> failwith "Error: Wrong package listing."
                       )

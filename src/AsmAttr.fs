namespace Pget

/// Get information about type 
module TInfo =
    open System 
    open System.Reflection

    type T = Type 

    /// Get type information about object
    let typeOf obj = obj.GetType()

    /// Get name of a type
    let getName (t: T) = t.Name

    /// Get full name of a type
    let fullName (t: T) = t.FullName 

    /// Get the namespace of a type 
    let getNamespace (t: T) = t.Namespace 

    let getModule (t: T) = t.Module

    let isPublic (t: T) = t.IsPublic

    let isNotPublic (t: T) = t.IsNotPublic 

    let isPrimitive (t: T) = t.IsPrimitive

    let isClass (t: T) = t.IsClass

    let isClassNonAbstract (t: T) = t.IsClass && not t.IsAbstract

    /// Check if type is Abstract Class
    let isAbstract (t: T) = t.IsAbstract

    let isArray (t: T) = t.IsArray

    let isEnum (t: T) = t.IsEnum

    let isInterface (t: T) = t.IsInterface

    let isVisible (t: T) = t.IsVisible

    /// Test if type is a public class     
    let isPublicClass (atype: T) =
        atype.IsClass && atype.IsPublic

    let getBaseType (t: T) = t.BaseType
        
    /// Get all fields of a type
    let getFields (t: T) = t.GetFields()

    /// Get all constructors of a type (class)
    let getConstructors (t: T) = t.GetConstructors()

    /// Get all methods of a type ignoring properties
    /// (methods which name starts with get_ or set_)
    ///
    let getMethodsNonProp(t: T) =
        t.GetMethods()
        |> Seq.filter(fun minfo ->
                      not (   minfo.Name.StartsWith("set_")
                            || minfo.Name.StartsWith("get_")
                            || minfo.IsSpecialName
                           ))

    /// Get all public methods
    let getMethods (t: T) = t.GetMethods()

    /// Get public properties of a type
    let getProperties (t: T) = t.GetProperties()

    /// Get Methods with flags
    let getMethodsFlags flags (t: T) = t.GetMethods(flags)

    /// Displays only public information about type. 
    let show (t: T) =
        Console.WriteLine("""
Type Info:

  Name:           {0}
  Full Name:      {1}
  Namespace:      {2}
  Module:         {3}
  Base Type:      {11}

Predicates

  Class:          {4}
  Abstract Class: {12}
  Primitive       {5}
  Array:          {6}
  Interface       {7}   
  Enum            {8}
  Public          {9}
  Visible         {10}
  
                        """,
                          t.Name,
                          t.FullName,
                          t.Namespace,
                          t.Module,
                          t.IsClass,
                          t.IsPrimitive,
                          t.IsArray,
                          t.IsInterface,
                          t.IsEnum,
                          t.IsPublic,
                          t.IsVisible,
                          t.BaseType,
                          t.IsAbstract
                          );

         Console.WriteLine("\nFields");
         Console.WriteLine("----------------"); 
         t.GetFields()     |> Seq.iter (printfn "\t%A\n");

         Console.WriteLine("\nProperties");
         Console.WriteLine("----------------"); 
         t.GetProperties() |> Seq.iter (printfn "\t%A\n");

         Console.WriteLine("\nConstructors");
         Console.WriteLine("----------------"); 
         t |> getConstructors
           |> Seq.iter (printfn "\t%A\n");           

         Console.WriteLine("\nMethods");
         Console.WriteLine("----------------"); 
         t |> getMethodsNonProp
           |> Seq.iter (printfn "\t%A\n");

    /// Show information about object type
    let showObj obj = show (obj.GetType())

 
/// Assembly attributes wrapper
module AsmAttr =

    open System.Reflection
    open System

    /// Load Assembly File
    let loadFrom (assemblyFile: string) =
        Assembly.LoadFrom assemblyFile

    let reflectionOnlyLoad (asmFile: string) =
        Assembly.ReflectionOnlyLoad asmFile

    /// Load Assembly from file returning None if it doesn't exist.
    let loadFromOpt (assemblyFile: string) =
        if System.IO.File.Exists(assemblyFile)
        then Some (Assembly.LoadFrom assemblyFile)
        else None

    let getCallingAssembly () =
        Assembly.GetCallingAssembly()

    /// Get types from an assembly
    let getTypes (asm: Assembly) =
        asm.GetTypes()
        |> Seq.ofArray

    let getExportedTypes (asm: Assembly) =
        asm.GetExportedTypes()
        |> Seq.ofArray

        
    /// Get type from assembly      
    let getType (atype: string) (asm: Assembly) =
        match asm.GetType(atype) with
        | null -> None
        | t    -> Some t 


    /// Get types with at least one field. 
    let getTypesWithFields (asm: Assembly) = 
        asm |>  getExportedTypes
            |>  Seq.filter (fun (atype: Type) ->
                            not <| Array.isEmpty (atype.GetFields()))
        
    /// Get all exported namespaces from an assembly object.
    let getExportedNS (asm: Assembly) =
        asm.GetExportedTypes ()
        |> Seq.map (fun (t: Type) -> t.Namespace)
        |> Seq.distinctBy id

    /// Get all types within a exported namespace from an assembly object.
    let getTypesWithinExportedNS nspace predicate (asm: Assembly) =
        asm.GetExportedTypes ()
        |> Seq.filter (fun (t: Type) -> t.Namespace = nspace && predicate t)


    let getPublicTypesInNamespace  (asmFile: string) selector (ns: string) =
        let asm = loadFrom asmFile 
        asm.GetTypes()
        |> Seq.filter (fun (atype: Type) ->
                       atype.Namespace = ns
                       && atype.IsPublic
                       && selector atype
                       )   

/// Assembly metadata information 
module AsmInfo =
    open System.Reflection 

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
                               ] )
      

module AsmDisplay =
    open System
    open System.Reflection 

    let optDefault def opt =
        match opt with
        | None    -> def
        | Some x  -> x

    let optIter2 errorFn actionFn opt: unit  =
        match opt with
        | None    -> errorFn ()
        | Some x  -> actionFn x

    let showType (asmFile: string) (typeName: string) =
        let errorHandler1 () = Console.WriteLine "Error: Assembly file doesn't exist"
        let errorHandler2 () = Console.WriteLine "Error: Type not found in assembly."
        asmFile
        |> AsmAttr.loadFromOpt
        |> optIter2 errorHandler1 ( AsmAttr.getType typeName
                                    >> (optIter2 errorHandler2 TInfo.show)
                                  )

    let showTypeSelector (asmFile: string) predicate =
        asmFile
        |> AsmAttr.loadFrom
        |> AsmAttr.getExportedTypes
        |> Seq.filter predicate
        |> Seq.iter  Console.WriteLine

    /// Print all types exported by an assembly file      
    let showTypes (asmFile: string) =
       showTypeSelector asmFile (fun t -> true)

    /// Print all classes exported by an assembly file.
    let showClasses (asmFile: string) =
        showTypeSelector asmFile TInfo.isClass

    /// Print all interfaces exported by an assembly file.
    let showIntefaces (asmFile: string) =
        showTypeSelector asmFile TInfo.isInterface

    /// Print all non-abstract classes
    let showClassesNonAbstract (asmFile: string) =
        showTypeSelector asmFile TInfo.isClassNonAbstract

    /// Print only abstract classes
    let showAbstractClasses (asmFile: string) =
        showTypeSelector asmFile TInfo.isAbstract

    /// Print assembly file attributes
    ///
    let showFile (asmFile: string) =
        let asm = AsmAttr.loadFrom asmFile
        printfn "Assembly Attributes"
        printfn "-------------------------------------------"
        printfn "Name         %s" (AsmInfo.getName asm)
        // printfn "Full Name    $s" (getFullName asm)
        printfn "Version      %s" <| (AsmInfo.getVersion asm).ToString()
        printfn "CLR Version  %s" <| AsmInfo.getRuntimeVersion asm
        printfn "Product      %s" (optDefault ""  <| AsmInfo.getProduct asm)
        printfn "Culture      %s" (optDefault ""  <| AsmInfo.getCulture asm)
        printfn "Company      %s" (optDefault ""  <| AsmInfo.getCompany asm)
        printfn "Description  %s" (optDefault ""  <| AsmInfo.getDescription asm)
        printfn "Copyright    %s" (optDefault ""  <| AsmInfo.getCopyright asm)
        printfn "GUID         %s" (optDefault ""  <| AsmInfo.getGuid asm)
        printfn "Com Visible  %s" (optDefault ""  <| (AsmInfo.getComVisible asm
                                                      |> Option.map (fun e -> e.ToString())))
        printfn "Codebase     %s" asm.CodeBase
    

    /// Print all exported namespaces
    let showExportedNS (asmFile: string) =
        asmFile |> AsmAttr.loadFrom
                |> AsmAttr.getExportedNS
                |> Seq.iter Console.WriteLine

    let showTypesWithinNS asmFile nspace =
        asmFile |> AsmAttr.loadFrom
                |> AsmAttr.getTypesWithinExportedNS nspace (fun t -> true)
                |> Seq.iter Console.WriteLine

    /// Print all namespaces from an assembly (.exe or .dll)
    let showNamespaces (asmFile: string) =
        let asm = AsmAttr.loadFrom asmFile 
        asm.GetTypes() |> Seq.distinctBy (fun t -> t.Namespace)
                       |> Seq.iter (fun t -> Console.WriteLine(t.Namespace))        

    let showClassesInNamespace (asmFile: string) ns =
        AsmAttr.getPublicTypesInNamespace asmFile (fun atype -> atype.IsClass) ns 
        |> Seq.iter Console.WriteLine


    let showMethods bindingFlags (asmFile: string) (className: string) =
        AsmAttr.loadFrom asmFile
        |> AsmAttr.getType className
        |> Option.map(TInfo.getMethodsFlags bindingFlags)
        |> Option.iter (Seq.iter (fun m -> Console.WriteLine("")
                                           Console.WriteLine m))

    let showPublicMethods  (asmFile: string) (className: string) =
        let flags = BindingFlags.Public
                    ||| BindingFlags.Instance
                    ||| BindingFlags.DeclaredOnly                    
        showMethods flags asmFile className

    let showPublicStaticMethods  (asmFile: string) (className: string) =
        let flags = BindingFlags.Public
                    ||| BindingFlags.Static
                    ||| BindingFlags.DeclaredOnly                    
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

        
    /// Display resources from an .NET assembly file 
    let showResurces (asmFile: string) =
        let asm = AsmAttr.loadFrom asmFile
        asm.GetManifestResourceNames() |> Seq.iter Console.WriteLine

    let showAsmReferences (asmFile: string) =
        let asm = AsmAttr.loadFrom asmFile
        asm.GetReferencedAssemblies ()
        |> Seq.iter (fun an ->
                     Console.WriteLine("Name = {0}\t\tVersion = {1}\t\tCulture = {2}",
                                        an.Name,
                                        an.Version,
                                        an.CultureInfo.Name
                                       ))

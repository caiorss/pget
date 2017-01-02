namespace Pget

/// Get information about type 
module TInfo =
    open System 
    open System.Reflection

    type T = Type 
    
    let getName (t: T) = t.Name

    let fullName (t: T) = t.FullName 

    /// Get the namespace of a type 
    let getNamespace (t: T) = t.Namespace 

    let getModule (t: T) = t.Module

    let isPublic (t: T) = t.IsPublic

    let isNotPublic (t: T) = t.IsNotPublic 

    let isPrimitive (t: T) = t.IsPrimitive

    let isClass (t: T) = t.IsClass

    let isArray (t: T) = t.IsArray

    let isEnum (t: T) = t.IsEnum

    let isInterface (t: T) = t.IsInterface

    let isVisible (t: T) = t.IsVisible

    /// Test if type is a public class     
    let isPublicClass (atype: T) =
        atype.IsClass && atype.IsPublic
        
    let getFields (t: T) = t.GetFields()

    let getMethods (t: T) = t.GetMethods()

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

    let getProperties (t: T) = t.GetProperties()

    let getMethodsFlags flags (t: T) = t.GetMethods(flags)

    /// Displays only public information about type. 
    let show (t: T) =
        Console.WriteLine("""
Type Info:

  Name:      {0}
  Full Name: {1}
  Namespace: {2}
  Module:    {3}
  

Predicates

  Class:     {4}
  Primitive  {5}
  Array:     {6}
  Interface  {7}   
  Enum       {8}
  Public     {9}
  Visible    {10}
  
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
                          t.IsVisible                         
                          );
         Console.WriteLine("\nFields");
         Console.WriteLine("----------------"); 
         t.GetFields()     |> Seq.iter (printfn " %A");
         Console.WriteLine("\nProperties");
         Console.WriteLine("----------------"); 
         t.GetProperties() |> Seq.iter (printfn " %A");
         Console.WriteLine("\nMethods");
         Console.WriteLine("----------------"); 
         t.GetMethods() |> Seq.iter (printfn " %A");
 
/// Assembly attributes wrapper
module AsmAttr =

    open System.Reflection
    open System

    /// Load Assembly File
    let loadFrom (assemblyFile: string) =
        Assembly.LoadFrom assemblyFile

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
    open AsmInfo
    open AsmAttr
    open System
    open System.Reflection 

    let optDefault def opt =
        match opt with
        | None    -> def
        | Some x  -> x

    /// Show all classes exported by an assembly file.
    let showClasses (asmFile: string) =
        loadFrom asmFile
        |> getExportedTypes
        |> Seq.filter TInfo.isPublicClass
        |> Seq.iter  Console.WriteLine    

    /// Print assembly file attributes
    ///
    let showFile (asmFile: string) =
        let asm = AsmAttr.loadFrom asmFile
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
    
    /// Print all namespaces from an assembly (.exe or .dll)
    let showNamespaces (asmFile: string) =
        let asm = loadFrom asmFile 
        asm.GetTypes() |> Seq.distinctBy (fun t -> t.Namespace)
                       |> Seq.iter (fun t -> Console.WriteLine(t.Namespace))        

    let showClassesInNamespace (asmFile: string) ns =
        getPublicTypesInNamespace asmFile (fun atype -> atype.IsClass) ns 
        |> Seq.iter Console.WriteLine


    let showMethods bindingFlags (asmFile: string) (className: string) =
        loadFrom asmFile
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
        let asm = loadFrom asmFile
        asm.GetManifestResourceNames() |> Seq.iter Console.WriteLine

    let showAsmReferences (asmFile: string) =
        let asm = loadFrom asmFile
        asm.GetReferencedAssemblies ()
        |> Seq.iter (fun an ->
                     Console.WriteLine("Name = {0}\t\tVersion = {1}\t\tCulture = {2}",                                      an.Name,
                                        an.Version,
                                        an.CultureInfo.Name
                                       ))

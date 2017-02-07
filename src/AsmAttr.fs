namespace Pget

/// FSharp Types
module FSType =

    open System

    type private FR = Microsoft.FSharp.Reflection.FSharpType
    type private FV = Microsoft.FSharp.Reflection.FSharpValue
    type private UC = Microsoft.FSharp.Reflection.UnionCaseInfo

    type tDisp = Type -> String

    let joinString sep (xs: string seq) =
        System.String.Join(sep, xs)

    let getType obj = obj.GetType()

    let isFSharpModule = FR.IsModule
    let isFSharpTuple  = FR.IsTuple
    let isFSharpFun    = FR.IsFunction
    let isFSharpRecord = FR.IsRecord
    let isFSharpUnion  = FR.IsUnion

    /// Test if type is a FSharp type like function, module, tuple and so on.
    let isFSharpType (t: Type) =
        FR.IsFunction t
        || FR.IsModule t
        || FR.IsExceptionRepresentation t
        || FR.IsTuple t
        || FR.IsRecord t
        || FR.IsUnion t



    let (|FSTuple|FSFun|FSUnion|FSModule|FSRecord|SomeType|)  (t: Type) =
        match t with
        | _ when FR.IsTuple t    -> FSTuple
        | _ when FR.IsModule t   -> FSModule
        | _ when FR.IsFunction t -> FSFun
        | _ when FR.IsUnion t    -> FSUnion
        | _ when FR.IsRecord t   -> FSRecord
        | _                      -> SomeType


    /// Convert a C# type to F# type equivalent
    let rec formatType (t: Type) =
        match t.FullName with
        | "System.Byte"    ->  "byte"
        | "System.SByte"   ->  "sbyte"
        | "System.Int16"   ->  "int16"
        | "System.Int32"   ->  "int"
        | "System.UInt32"  ->  "uint"
        | "System.Int64"   ->  "int64"
        | "System.IntPtr"  ->  "nativeint"
        | "System.Char"    ->  "char"
        | "System.String"  ->  "string"
        | "System.Decimal" ->  "decimal"
        | "System.Single"  ->  "float32"
        | "System.Double"  ->  "float"
        | "System.Void"    ->  "unit"
        | _                ->  t.FullName


    let showTupleType (fn: tDisp) (t: Type)  =
        t |> FR.GetTupleElements
          |> Array.map fn
          |>  joinString " * "


    let getFunTypeList (t: Type) =
        let rec aux acc (t: Type) =
            match t with
            | FSFun -> let (ret, cont) = FR.GetFunctionElements t
                       aux (ret::acc) cont
            | _     -> t::acc

        List.rev <| aux [] t


    let showFnType (fn: tDisp) (t: Type) =
        t |> getFunTypeList
          |> List.map fn
          |> joinString " -> "


    let showUnionDeclarion (fn: tDisp) (t: Type)  =
        let showFields   (t: Reflection.UnionCaseInfo) =
            let fields = Array.map (fun (field: Reflection.PropertyInfo) ->
                                    fn field.PropertyType) <| t.GetFields()
            t.Name + " of " + (joinString " * " fields)

        let tname = "type " + t.Name
        let n = tname.Length
        let spaces = String.Concat(Seq.replicate n " ") + "  "

        FR.GetUnionCases t
        |> Array.map showFields
        |> joinString ("\n" + spaces + "| ")
        |> (fun s -> "type " + t.Name + " = " + s)

    let showUnion (fn: tDisp) (t: Type) =
        let argTypes = t.GenericTypeArguments
                       |> Seq.map fn
                       |> joinString ","
        t.Name + "<" + argTypes + ">"


    let showOption (fn: tDisp) (t: Type) =
        let param = fn t.GenericTypeArguments.[0]
        sprintf "(%s) option" param

    let showList (fn: tDisp) (t: Type) =
        let param = fn t.GenericTypeArguments.[0]
        sprintf "(%s) list" param

    let rec showType  (t: Type) =
        match t with
        | _ when t.Name = "FSharpOption`1" -> showOption showType t
        | _ when t.Name = "FSharpList`1"   -> showList showType t
        | FSTuple                          -> showTupleType showType t
        | FSUnion                          -> showUnion showType t
        | FSFun                            -> showFnType showType t
        | SomeType                         -> formatType t
        | _                                -> failwith "Not implemented"


/// Parameter info - Wrapper around ParameterInfo class
module PInfo =
    open System.Reflection

    type T = ParameterInfo

    let paramType (pi: T) = pi.ParameterType

    let isIn (pi: T) = pi.IsIn

    let isLcid (pi: T) = pi.IsLcid

    let isOut (pi: T) = pi.IsOut

    let hasDefaultValue (pi: T) = pi.HasDefaultValue

    let name (pi: T) = pi.Name

    let position (pi: T) = pi.Position

    let memberInfo (pi: T) = pi.Member

    let toString (pi: T) = pi.ToString()


/// Information about Method - Wrapper around MethodInfo class
module MInfo =
    open System
    open System.Reflection

    type T = MethodInfo

    let name (mi: T) = mi.Name

    let returnType (mi: T) = mi.ReturnType

    let declaringType (mi: T) = mi.DeclaringType

    let reflectedType (mi: T) = mi.ReflectedType

    let paramInfo (mi: T) = mi.ReturnParameter

    let attributes (mi: T) = mi.Attributes

    // ===== Type Predicates or Flags ========== //

    let isGenericMethod (mi: T) = mi.IsGenericMethod

    let isSecurityCritical (mi: T) = mi.IsSecurityCritical

    let isPublic (mi: T) = mi.IsPublic

    let isStatic (mi: T) = mi.IsStatic

    let isFinal (mi: T) = mi.IsFinal

    let isVirtual (mi: T) = mi.IsVirtual

    let isSpecialName (mi: T) = mi.IsSpecialName

    let isConstructor (mi: T) = mi.IsConstructor

    /// Get summarized method information - if it is public, static or private
    /// and its parameters as string
    ///
    let format (mi: T) =
        sprintf "%s %s %s %s (%s)" (if isPublic mi then "Public" else "Private")
                                   (if isStatic mi then "Static" else "")
                                   (mi.ReturnType.ToString())
                                   mi.Name
                                   (let plist =  mi.GetParameters()
                                                  |> Seq.map (fun pi ->
                                                              pi.ParameterType.ToString() + " " + pi.Name)
                                    in  String.Join(", ", plist)
                                    )

    let show (mi: T) = Console.WriteLine(format mi)

/// Information about type
module TInfo =
    open System 
    open System.Reflection
    open System.Xml

    type T = Type 

    /// Get type information about object
    let typeOf obj = obj.GetType()

    /// Try to find type by name
    let getType typeName =
         Option.ofObj <| Type.GetType(typeName)

    /// Get name of a type
    let getName (t: T) = t.Name

    /// Get full name of a type
    let fullName (t: T) = t.FullName 

    /// Get the namespace of a type 
    let getNamespace (t: T) = t.Namespace 

    let getModule (t: T) = t.Module

    /// Check if type is Public
    let isPublic (t: T) = t.IsPublic

    /// Check if type is not public
    let isNotPublic (t: T) = t.IsNotPublic 

    /// Check if type is primitive
    let isPrimitive (t: T) = t.IsPrimitive

    /// Check if type is class
    let isClass (t: T) = t.IsClass

    /// Check if type is non abstract class
    let isClassNonAbstract (t: T) = t.IsClass && not t.IsAbstract

    /// Check if type is Abstract Class
    let isAbstract (t: T) = t.IsAbstract

    /// Check if type is Array
    let isArray (t: T) = t.IsArray

    /// Check if type is Enum
    let isEnum (t: T) = t.IsEnum

    /// Check if type is Interface
    let isInterface (t: T) = t.IsInterface

    /// Check if type is Visible
    let isVisible (t: T) = t.IsVisible

    /// Test if type is a public class     
    let isPublicClass (atype: T) =
        atype.IsClass && atype.IsPublic

    let getBaseType (t: T) = t.BaseType
        
    /// Get all fields of a type
    let getFields (t: T) = t.GetFields()

    /// Get all constructors of a type (class)
    let getConstructors (t: T) = t.GetConstructors()

    /// Get all non-static methods
    let getPublicInstanceMethods (t: T) =
        t.GetMethods()
        |> Seq.filter (fun mi -> MInfo.isPublic mi
                                && not (MInfo.isStatic mi)
                                && not (MInfo.isSpecialName mi)
                         )

    /// Get all public static methods
    let getPublicStaticMethods (t: T) =
        t.GetMethods()
        |> Seq.filter (fun mi -> MInfo.isPublic mi
                                 && MInfo.isStatic mi
                                 && not (MInfo.isSpecialName mi)
                       )


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

    let queryXmlComment (query: string) (doc: XmlDocument): XmlNode option =
        doc |> FXml.Doc.root
            |> FXml.Node.nth 1
            |> FXml.Node.findNode (FXml.Node.nodeAttrTagContains "member" "name" query)

    let queryXmlSummary (query: string) (doc: XmlDocument): string option =
        doc  |> FXml.Doc.root
             |> FXml.Node.nth 1
             |> FXml.Node.findNode (FXml.Node.nodeAttrTagContains "member" "name" query)
             |> Option.bind (FXml.Node.findTextFromNodeTag "summary")
             |> Option.map (fun text -> text.Trim())


    /// Displays only public information about type.
    let show2 (doc: XmlDocument option) (t: T) =
        Console.WriteLine("""
**** Type Info

 - Name:           {0}
 - Full Name:      {1}
 - Namespace:      {2}
 - Module:         {3}
 - Base Type:      {11}

*Predicates*

 - Class:          {4}
 - Abstract Class: {12}
 - Primitive       {5}
 - Array:          {6}
 - Interface       {7}
 - Enum            {8}
 - Public          {9}
 - Visible         {10}

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

         Console.WriteLine("\n**** Fields\n");
         // Console.WriteLine("----------------");

         t.GetFields()
         // |> Seq.iter (printfn "\t%A\n");
         |> Seq.iter (fun fi ->
                      let query = "F:" + fi.DeclaringType.FullName + "." + fi.Name
                      let summary = doc |> Option.bind (queryXmlSummary query)
                      printfn " - %A\n" fi
                      Option.iter (printfn "%s\n")  summary
                      );

         Console.WriteLine("\n**** Properties\n");
         // Console.WriteLine("----------------");

         t.GetProperties()
         // |> Seq.iter (printfn "\t%A\n");
         |> Seq.iter (fun pi ->
                      let query = "P:" + pi.DeclaringType.FullName + "." + pi.Name
                      let summary = doc |> Option.bind (queryXmlSummary query)
                      printfn " - %A\n" pi
                      Option.iter (printfn "%s\n")  summary
                      );


         Console.WriteLine("\n**** Constructors\n");
         // Console.WriteLine("----------------");
         t |> getConstructors
           |> Seq.iter (printfn "\t%A\n");

         Console.WriteLine("\n**** Instance Methods\n");

         t |> getPublicInstanceMethods
           |> Seq.iter (fun mi ->
                        let query = "M:" + mi.DeclaringType.FullName + "." + mi.Name
                        let summary = doc |> Option.bind (queryXmlSummary query)
                        printfn " - %s\n" (MInfo.format mi)
                        Option.iter (printfn "%s\n")  summary
                        );

         Console.WriteLine("\n**** Static Methods\n");

         t |> getPublicStaticMethods
           |> Seq.iter (fun mi ->
                        let query = "M:" + mi.DeclaringType.FullName + "." + mi.Name
                        let summary = doc |> Option.bind (queryXmlSummary query)
                        printfn " - %s\n" (MInfo.format mi)
                        Option.iter (printfn "%s\n")  summary
                        );


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

    let loadFile (asmFile: string) =
        Assembly.LoadFile asmFile

    /// Try load assembly from GAC, if its not in GAC, then it loads the dll file.
    let load (asmFile: string) =
        try // try load from GAC
            Assembly.Load asmFile
        with
            // Load from dll file
            :? System.IO.FileNotFoundException -> Assembly.LoadFile asmFile

    let loadSafe (asmFile: string) cont =
        try // try load from GAC
            cont <| Assembly.Load asmFile
        with
            // Load from dll file
            :? System.IO.FileNotFoundException
            ->
                try
                    cont <| Assembly.LoadFile asmFile
                with
                    | :? System.IO.FileNotFoundException -> printfn "Error: I can't find the file: %s" asmFile
                    | :? System.BadImageFormatException  -> printfn "Error: File %s is not a .NET assembly" asmFile


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

    /// Test if assembly contains any F# type
    let isFsharpAssembly asmFile =
        try
            asmFile |> AsmAttr.load
                    |> AsmAttr.getTypes
                    |> Seq.exists FSType.isFSharpType
        with
            :? System.Reflection.ReflectionTypeLoadException -> false

    let findFsharpDlls path =
        let files = System.IO.Directory.GetFiles(path, "*.dll", System.IO.SearchOption.AllDirectories)
        files |> Seq.filter isFsharpAssembly
              |> Seq.iter (printfn "%s")

    /// Redirect stdout print to a file.
    let withStdoutFile (file: string) fn  =
        let stdout = Console.Out
        let sw = new System.IO.StreamWriter(file)
        Console.SetOut(sw)
        fn ()
        sw.Close()
        Console.SetOut(sw)

    /// Return stdout output as string.
    let withStdout fn =
        let stdout = Console.Out
        let sw = new System.IO.StringWriter ()
        Console.SetOut(sw)
        fn ()
        let out = sw.ToString()
        sw.Close()
        Console.SetOut(sw)
        out


    let optDefault def opt =
        match opt with
        | None    -> def
        | Some x  -> x

    let optIter2 errorFn actionFn opt: unit  =
        match opt with
        | None    -> errorFn ()
        | Some x  -> actionFn x


    /// Show all F# modules with
    let showFsharpModules asmFile flag =
        asmFile |> AsmAttr.load
                |> AsmAttr.getTypes
                |> Seq.filter (fun t -> FSType.isFSharpModule t && (not flag || t.IsPublic))
                |> Seq.iter (fun t -> printfn "%s" t.FullName)

    // let showType (asmFile: string) (typeName: string) =
    //     let errorHandler1 () = Console.WriteLine "Error: Assembly file doesn't exist"
    //     let errorHandler2 () = Console.WriteLine "Error: Type not found in assembly."
    //     asmFile
    //     |> AsmAttr.loadFromOpt
    //     |> optIter2 errorHandler1 ( AsmAttr.getType typeName
    //                                 >> (optIter2 errorHandler2 TInfo.show)
    //                               )

    let showType (asmFile: string) (typeName: string) =
        let xmlFile = System.IO.Path.ChangeExtension(asmFile, "xml")
        let doc = if System.IO.File.Exists xmlFile
                  then Some (FXml.Doc.loadFile xmlFile)
                  else None

        AsmAttr.loadSafe asmFile (AsmAttr.getType typeName
                                  >> Option.iter (TInfo.show2 doc)
                                  )

    let showTypeSelector (asmFile: string) predicate =
        AsmAttr.loadSafe asmFile ( AsmAttr.getExportedTypes
                                   >> Seq.filter predicate
                                   >> Seq.iter  Console.WriteLine
                                  )

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
        let aux asm =
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
        AsmAttr.loadSafe asmFile aux
    

    /// Print all exported namespaces
    let showExportedNS (asmFile: string) =
        AsmAttr.loadSafe asmFile ( AsmAttr.getExportedNS
                                   >> Seq.iter Console.WriteLine
                                 )

    /// Show all types within a exported namespace
    let showTypesWithinNS asmFile nspace =
        let aux asm = asm
                     |> AsmAttr.getTypesWithinExportedNS nspace (fun t -> true)
                     |> Seq.iter Console.WriteLine
        AsmAttr.loadSafe asmFile aux

    /// Print all namespaces from an assembly (.exe or .dll)
    let showNamespaces (asmFile: string) =
        let aux (asm: Assembly) =
            asm.GetTypes() |> Seq.distinctBy (fun t -> t.Namespace)
                           |> Seq.iter (fun t -> Console.WriteLine(t.Namespace))

        AsmAttr.loadSafe asmFile aux

    /// Show all detailed exported types grouped by namespace
    let showExportedTypesReport asmFile =
        let asm = asmFile |> AsmAttr.load
        asm   |> AsmAttr.getExportedNS
              |> Seq.iter (fun ns ->
                           Console.WriteLine ("** {0}", ns);

                           AsmAttr.getTypesWithinExportedNS ns (fun t -> true) asm
                           |> Seq.iter (fun t ->
                                        Console.WriteLine("*** {0}", t.Name)
                                        TInfo.show t;
                                        )
                           )

    /// Show all detailed exported types grouped by namespace
    let showExportedTypesReport2 asmFile =
        let aux asm =
            let xmlFile = System.IO.Path.ChangeExtension(asmFile, "xml")
            let doc = if System.IO.File.Exists xmlFile
                      then Some (FXml.Doc.loadFile xmlFile)
                      else None

            asm   |> AsmAttr.getExportedNS
                  |> Seq.iter (fun ns ->
                               Console.WriteLine ("** {0}", ns);

                               AsmAttr.getTypesWithinExportedNS ns (fun t -> true) asm
                               |> Seq.iter (fun t ->
                                            Console.WriteLine("*** {0}", t.FullName)
                                            TInfo.show2 doc t;
                                            ))

        AsmAttr.loadSafe asmFile aux


    let genExportedTypesReport asmFile outputFile =
        withStdoutFile outputFile  (fun () ->  showExportedTypesReport2 asmFile)

    let showClassesInNamespace (asmFile: string) ns =
        AsmAttr.getPublicTypesInNamespace asmFile (fun atype -> atype.IsClass) ns 
        |> Seq.iter Console.WriteLine


    let showMethods bindingFlags (asmFile: string) (className: string) =
        let aux asm =
            asm |> AsmAttr.getType className
                |> Option.map(TInfo.getMethodsFlags bindingFlags)
                |> Option.iter (Seq.iter (fun m -> Console.WriteLine("")
                                                   Console.WriteLine m))
        AsmAttr.loadSafe asmFile aux

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
        AsmAttr.loadSafe asmFile (fun asm ->  asm.GetManifestResourceNames()
                                              |> Seq.iter Console.WriteLine
                                  )

    let showAsmReferences (asmFile: string) =
        let asm = AsmAttr.load asmFile
        asm.GetReferencedAssemblies ()
        |> Seq.iter (fun an ->
                     Console.WriteLine("Name = {0}\t\tVersion = {1}\t\tCulture = {2}",
                                        an.Name,
                                        an.Version,
                                        an.CultureInfo.Name
                                       ))
    let showLoadedAssemblies () =
        AppDomain.CurrentDomain.GetAssemblies()
        |> Seq.iter (fun a ->
                     try  printfn "Fullname = %s\nIn GAC   = %O\nCodebase = %s\n" a.FullName
                                                                                  a.GlobalAssemblyCache
                                                                                  a.CodeBase

                     with :? System.NotSupportedException -> printfn "Fullname = %s\nIn GAC   = %O\nCodebase =\n"
                                                                      a.FullName
                                                                      a.GlobalAssemblyCache
                      )

    let showAssemblyPath (asmName: string) =
        try   printfn "%s" (Reflection.Assembly.Load asmName).Location
        with  :? System.IO.FileNotFoundException -> printfn "Error: I can't find or load assembly %s" asmName

    let showTypeInfo tname =
        let errHandler () = printfn "Error: I can't find the type: %s" tname 
        tname |> TInfo.getType
              |> optIter2 errHandler (TInfo.show2 None)
              

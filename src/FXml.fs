module FXml 

open System
open System.Xml 


/// Functional wrapper around XmlNode object 
module Node =

    type T = XmlNode 

    /// Get XML node value 
    let value (node: T) =
        node.Value

    /// Get XML node tag name. 
    let name (node: T) =
        node.Name 

    /// Get attribute value from xml node. This function returns
    /// an option None value if the attribute doesn't exist. 
    let attrv (attr: string) (node: T) =
        match node.Attributes.[attr] with
        | null -> None
        | n    -> Some n.Value

    /// Get attribute value from xml node. This function is not
    /// safe and it is susceptible to null exceptions.
    let attrv2 (attr: string) (node: T) =
        node.Attributes.[attr].Value

    /// Get all attributes from a xml node     
    let attributes (node: T) =
        match node.Attributes with
        | null ->  Seq.empty
        | attrs -> seq { for n in attrs  do yield (n.Name, n.Value) }


    /// Get child nodes from a xml node
    let cnodes (node: T) =
        seq {for n in node.ChildNodes do yield n }

    /// Get nth child node from a xml node
    let nth (n: int) (node: T)  =
        node.ChildNodes.[n]

    let first (node: T) =
        node.ChildNodes.[0]

    /// Get all attributes from a group of similar child nodes
    let nodesAttr attr (node: T): string seq =
        node |> cnodes
             |> Seq.map (attrv2 attr)

    let nodesAttrs attrlist (node: T) =
        node |> cnodes
             |> Seq.map (fun node -> List.map (fun attr -> attrv2 attr node) attrlist)

    /// Find a child node that satisfies a predicate
    let findChildNode fn (node: T) =
        node |> cnodes
             |> Seq.tryFind fn

    /// Filter child nodes of a node that satisfies a predicate
    let filterChildNodes fn (node: T) =
        node |> cnodes
             |> Seq.filter fn 

    /// Find child node that has a given tag
    let findChildNodeTag tag (node: T) =
        findChildNode (fun node -> node.Name = tag) node

    let findChildNodeTagText tag (node: T) =
        node |> cnodes
             |> Seq.tryFind (fun node -> node.Name = tag)
             |> Option.map (fun node -> node.InnerText)

    //// Get Xml inner text     
    let innerText (node: T) =
        node.InnerText       

    /// Show all attributes of a xml node
    let showAttributes (node: T) =
        node |> attributes
             |> Seq.iter (printfn "%O")

    let selectNode xpath (node: T) =
        Option.ofObj (node.SelectSingleNode xpath)

    let selectValue xpath (node: T) =
        let child = node.SelectSingleNode(xpath)
        match child with
        | null -> None
        | n    -> Some (n.Value)

    let selectValueCdata xpath (doc: T) =
        let node = doc.SelectSingleNode xpath
        match node with
        | null -> None
        | n    -> Some <| (node.ChildNodes.[0] :?> XmlCDataSection).Data

    let iterValue xpath fn (doc: T) =
        doc |> selectValue xpath
            |> Option.iter fn 

    let iterValueCdata xpath fn (doc: T) =
        selectValueCdata xpath doc |> Option.iter fn 

    let show (node: T) =
        let writer = new XmlTextWriter(Console.Out)
        writer.Formatting <- Formatting.Indented
        node.WriteContentTo(writer)

    let showTop (node: T) =
        printfn "Tag: %s" node.Name;
        printfn "Attributes"
        node |> attributes
             |> Seq.iter (printfn "  %O")

    /// Show only XML tags          
    let showStruct (node: T) =
        let rec aux spaces (node: T)  =
            let childs = node.ChildNodes   
            for ch in childs do
                printfn "%s" (spaces + ch.Name)
                aux (spaces + "\t") ch            
        aux "" node        

/// Functional Wrapper around XmlDocument class 
module Doc =

    type T = XmlDocument

    /// Load XML string 
    let load (xmlString: string) =
        let doc = new T ()
        doc.LoadXml(xmlString)
        doc

    /// Load XML from file or URL.
    let loadFile (uri: string) =
        let doc = new T ()
        doc.Load(uri)
        doc

    let root (doc: T) =
        doc.DocumentElement

    /// Format XML to human readable indented format    
    let toString (doc: T) =
        let builder = new System.Text.StringBuilder()
        let writer  = new XmlTextWriter (new System.IO.StringWriter(builder))
        writer.Formatting <- Formatting.Indented
        doc.Save(writer)    
        writer.Close()
        builder.ToString()

    /// Create a namespacemanager object    
    let makeNs prefix uri (doc: T) =
        let ns = new XmlNamespaceManager(doc.NameTable)
        ns.AddNamespace(prefix, uri)
        ns 

    /// Select single node using xpath. Returns None if node is not found.
    let selectNode xpath (doc: T) =
        Option.ofObj (doc.SelectSingleNode xpath)

    /// Select nodes with a xpath expression
    let selectNodes xpath (doc: T) =
        seq {for n in doc.SelectNodes(xpath) do yield n}

    /// Select nodes with a xpath expression within a XML namespace denoted by ns    
    let selectNodesNs ns xpath (doc: T) =
        seq {for n in doc.SelectNodes(xpath, ns) do yield n}

    /// Select nodes with a xpath expression within a XML namespace denoted by ns   
    let selectNodesNs2 (prefix, uri) xpath (doc: T): XmlNode  seq =
        let ns = new XmlNamespaceManager(doc.NameTable)
        ns.AddNamespace(prefix, uri)
        seq {for n in doc.SelectNodes(xpath, ns) do yield n }

    let selectNodeCdata xpath (doc: T) =
        let node = doc.SelectSingleNode xpath
        match node with
        | null -> None
        | n    -> Some <| (node.ChildNodes.[0] :?> XmlCDataSection)

    let selectValueCdata xpath (doc: T) =
        let node = doc.SelectSingleNode xpath
        match node with
        | null -> None
        | n    -> Some <| (node.ChildNodes.[0] :?> XmlCDataSection).Data      

    let xpathNodesFn (prefix, uri) xpath fn doc =
        let nodes = selectNodesNs2 (prefix, uri) xpath doc 
        Seq.map fn nodes


    let xpathSelectAttr xpath attribute doc =
      let nodes = selectNodes xpath doc
      Seq.map (Node.attrv2 attribute) nodes      

    let xpathSelectAttrNs (prefix, uri) xpath attribute doc =
      let nodes = selectNodesNs2 (prefix, uri) xpath doc
      Seq.map (Node.attrv2 attribute) nodes

    let xpathSelectValue xpath doc =
        let nodes = selectNodes xpath doc
        Seq.map Node.value nodes

    let xpathSelectInnerText xpath doc =
        let nodes = selectNodes xpath doc
        Seq.map Node.innerText nodes

    let xpathSelectInnerTextNs  (prefix, uri) xpath doc =
        let nodes = selectNodesNs2 (prefix, uri) xpath doc
        Seq.map Node.innerText nodes

    let xpathSelectValueNs (prefix, uri) xpath doc =
      let nodes = selectNodesNs2 (prefix, uri) xpath doc
      Seq.map Node.value nodes

    let xpathNodesNsManySeq (prefix, uri) xpath attributes doc =
      let nodes = selectNodesNs2 (prefix, uri) xpath doc
      Map.ofList <| List.map (fun attr -> attr, Seq.map (Node.attrv2 attr) nodes) attributes

    /// Display XML in a human readable format.  
    let show (doc: T) =
        doc |> toString
            |> Console.Write

    let showStruct (doc: T) =
        Node.showStruct doc.DocumentElement  

module File = 

    /// Display Xml file or xml uri (url)
    let show xmlFile =
        xmlFile |> Doc.loadFile 
                |> Doc.show

    let save xmlUri (xmlFile: string) =
        let doc = Doc.loadFile xmlUri
        doc.Save(xmlFile)

    /// Display Xml structure             
    let showStruct xmlFile =
        xmlFile |> Doc.loadFile
                |> Doc.showStruct

    /// <summary>            
    /// format XML from file or URI to a new XML file.
    /// </summary>
    /// 
    /// <code>
    /// FXml.File.formatUri "edition_world.rss" "news.rss"
    /// FXml.File.formatUri "http://rss.cnn.com/rss/edition_world.rss" "news.rss"            
    /// </code>    
    let formatUri xmlUri (xmlFile: string) =
        let doc = Doc.loadFile xmlUri
        doc.Save(xmlFile)

    let showXPathValue xmlFile xpath =
        xmlFile |> Doc.loadFile
                |> Doc.xpathSelectValue xpath
                |> Seq.iter (printfn "%O\n")

    let showXPathInnerText xmlFile xpath =
        xmlFile |> Doc.loadFile
                |> Doc.xpathSelectInnerText xpath
                |> Seq.iter (printfn "%O\n")                

    let showXPathInnerTextNs xmlFile (prefix, uri) xpath =
        xmlFile |> Doc.loadFile
                |> Doc.xpathSelectInnerTextNs (prefix, uri) xpath
                |> Seq.iter (printfn "%O\n")                

    let showXPathValueNs xmlFile (prefix, uri)  xpath =
        xmlFile |> Doc.loadFile
                |> Doc.xpathSelectValueNs (prefix, uri) xpath
                |> Seq.iter (printfn "%O\n")

    let showXpathAttr xmlFile xpath attribute  =
        xmlFile |> Doc.loadFile
                |> Doc.xpathSelectAttr xpath attribute
                |> Seq.iter (printfn "%O\n")

    let showXpathAttrNS xmlFile (prefix, uri) xpath attribute  =
        xmlFile |> Doc.loadFile
                |> Doc.xpathSelectAttrNs (prefix, uri) xpath attribute
                |> Seq.iter (printfn "%O\n")


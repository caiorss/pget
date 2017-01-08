module FXml 

open System
open System.Xml 

/// Predicate combinators
module Pred =

    let pAnd fa fb =
        fun x -> fa x && fb x

    let pOr fa fb =
        fun x -> fa x || fb x

    let pNot fa x = not (fa x)

    module Operators =
        let (<&&>) = pAnd
        let (<||>) = pOr

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

    /// Get all attribute names from a xml node
    let attrNames (node: T) =
        match node.Attributes with
        | null ->  Seq.empty
        | attrs -> seq { for n in attrs  do yield n.Name }

    /// Get child nodes from a xml node
    let cnodes (node: T) =
        seq {for n in node.ChildNodes do yield n }

    /// Get all child nodes ignoring comment nodes
    let cnodesNoComment (node: T) =
        seq {for n in node.ChildNodes do if n.NodeType <> XmlNodeType.Comment
                                         then yield n }

    /// Get nth child node from a xml node
    let nth (n: int) (node: T)  =
        node.ChildNodes.[n]

    /// Get first child node
    let first (node: T) =
        node.ChildNodes.[0]

    /// Get all attributes from a group of similar child nodes
    let nodesAttr attr (node: T): string seq =
        node |> cnodesNoComment
             |> Seq.map (attrv2 attr)

    let nodesAttrs attrlist (node: T) =
        node |> cnodesNoComment
             |> Seq.map (fun node -> List.map (fun attr -> attrv2 attr node) attrlist)

    /// Find a child node that satisfies a predicate
    let findNode fn (node: T) =
        node |> cnodes
             |> Seq.tryFind fn

    /// Filter child nodes of a node that satisfies a predicate
    let filterNodes fn (node: T) =
        node |> cnodes
             |> Seq.filter fn 

    /// Find child node that has a given tag
    let findChildNodeTag tag (node: T) =
        findNode (fun node -> node.Name = tag) node

    let findChildNodeTagText tag (node: T) =
        node |> cnodes
             |> Seq.tryFind (fun node -> node.Name = tag)
             |> Option.map (fun node -> node.InnerText)

    /// Check if node has a given tag(name)
    let nodeHasTag tag (node: T) =
        node.Name = tag 

    /// Check if node has a given attribute     
    let nodeHasAttr (attr: string) (node: T) =
        match node.Attributes.[attr] with
        | null -> false
        | _    -> true

    /// Check if node attribute is equal to a given value    
    let nodeAttrEqual (attr: string) (value: string) (node: T) =
        let check = node.Attributes.[attr]
        match check with
        | null -> false
        | n    -> n.Value = value

    /// Check if node attribute contains a value     
    let nodeAttrContains (attr: string) (value: string) (node: T) =
        let check = node.Attributes.[attr]
        match check with
        | null -> false
        | n    -> n.Value.Contains(value)        
        
    /// Check if at least one child node has tag or name
    /// <tag attr1="v1" attr2="v2" ...>
    let nodeChildHasTag tag (node: T) =
        node |> cnodes
             |> Seq.exists (fun n -> n.Name = tag)

    /// Check if node type is not comment node 
    let isNotComment (node: T) =
        node.NodeType <> XmlNodeType.Comment

    /// Check if node type is element     
    let isElement (node: T) =
        node.NodeType = XmlNodeType.Element            

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

    /// Display node as xml 
    let show (node: T) =
        let writer = new XmlTextWriter(Console.Out)
        writer.Formatting <- Formatting.Indented
        node.WriteContentTo(writer)

    let showTop (node: T) =
        printfn "Tag: %s" node.Name;
        printfn "Attributes"
        node |> attributes
             |> Seq.iter (printfn "  %O\n\n")

    /// Show xml node structure with tags of each node          
    let showStruct (node: T) =        
        let rec aux spaces (node: T) nestlevel  =
            let childs = node.ChildNodes
            let level = ref 0 
            for ch in childs do
                printfn "%s(%d.%d) %s" spaces nestlevel !level ch.Name 
                level := !level + 1
                aux (spaces + "\t") ch (nestlevel + 1)                
        aux "" node 0       

    /// Show xml node structure with attributes of each node 
    let showStructAttr (node: T) =        
        let rec aux spaces (node: T) nestlevel  =
            let childs = node.ChildNodes
            let level = ref 0 
            for ch in childs do
                let attrs = attrNames ch |> Seq.toList 
                printfn "%s(%d.%d) %s %O" spaces nestlevel !level ch.Name attrs 
                level := !level + 1
                aux (spaces + "\t") ch (nestlevel + 1)                
        aux "" node 0       

    /// Show xml node structure with namespace URI of each node 
    let showStructNs (node: T) =        
        let rec aux spaces (node: T) nestlevel  =
            let childs = node.ChildNodes
            let level = ref 0 
            for ch in childs do
                let attrs = attrNames ch |> Seq.toList 
                printfn "%s(%d.%d) %s / %s" spaces nestlevel !level ch.Name ch.NamespaceURI
                level := !level + 1
                aux (spaces + "\t") ch (nestlevel + 1)                
        aux "" node 0       


/// Functional Wrapper around XmlDocument class 
module Doc =

    type T = XmlDocument

    let xmlNodeListToSeq (xs: XmlNodeList) =
        seq {for n in xs do yield n}

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

    /// Get root node (.DocumentElement) from xml document.
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

    let getNodesByTagName tag (doc: T) =
        xmlNodeListToSeq <| doc.GetElementsByTagName(tag)

    /// Display XML in a human readable format.  
    let show (doc: T) =
        doc |> toString
            |> Console.Write


module FXPath =

    type T = XmlDocument 
    
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
                |> Doc.root
                |> Node.showStruct

    let showStructAttr xmlFile =
        xmlFile |> Doc.loadFile
                |> Doc.root
                |> Node.showStructAttr

    let showStructNs xmlFile =
        xmlFile |> Doc.loadFile
                |> Doc.root
                |> Node.showStructNs


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
                |> FXPath.xpathSelectValue xpath
                |> Seq.iter (printfn "%O\n")

    let showXPathInnerText xmlFile xpath =
        xmlFile |> Doc.loadFile
                |> FXPath.xpathSelectInnerText xpath
                |> Seq.iter (printfn "%O\n")                

    let showXPathInnerTextNs xmlFile (prefix, uri) xpath =
        xmlFile |> Doc.loadFile
                |> FXPath.xpathSelectInnerTextNs (prefix, uri) xpath
                |> Seq.iter (printfn "%O\n")                

    let showXPathValueNs xmlFile (prefix, uri)  xpath =
        xmlFile |> Doc.loadFile
                |> FXPath.xpathSelectValueNs (prefix, uri) xpath
                |> Seq.iter (printfn "%O\n")

    let showXpathAttr xmlFile xpath attribute  =
        xmlFile |> Doc.loadFile
                |> FXPath.xpathSelectAttr xpath attribute
                |> Seq.iter (printfn "%O\n")

    let showXpathAttrNS xmlFile (prefix, uri) xpath attribute  =
        xmlFile |> Doc.loadFile
                |> FXPath.xpathSelectAttrNs (prefix, uri) xpath attribute
                |> Seq.iter (printfn "%O\n")

// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.

#r "/Users/dickow/Projects/FsharpTransform/packages/FParsec.1.0.2/lib/net40-client/FParsecCS.dll";;
#r "/Users/dickow/Projects/FsharpTransform/packages/FParsec.1.0.2/lib/net40-client/FParsec.dll";;
#load "AST.fs"
#load "Parser.fs"

open FParsec
open FsharpTransform.AST
open FsharpTransform.Parser

let result = run pdocument "{\"transform\": [{\"src1\" : \"firstname\",\"src2\" : \"first\"},{\"src1\" : \"lastname\",\"src2\" : \"last\"},{\"src1\" : \"fullname\",\"src2\" : \"name\"}]}"

let doc = parseConfiguration @"/Users/dickow/Projects/FsharpTransform/FsharpTransform/transformConfig.json" System.Text.Encoding.UTF8

let traverseDocument (doc:Document) = 
 Seq.map (fun item -> printf "%A" item; item) doc.configuration.properties

match result with 
 | Success(doc, _, _) -> traverseDocument doc |> printf "%A"
 | Failure(error, _, _) -> printf "%A" error

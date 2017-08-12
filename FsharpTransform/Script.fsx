// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.
#r "/Users/dickow/Projects/FsharpTransform/packages/FParsec.1.0.2/lib/net40-client/FParsecCS.dll"
#r "/Users/dickow/Projects/FsharpTransform/packages/FParsec.1.0.2/lib/net40-client/FParsec.dll"
#load "AST.fs"
#load "Parser.fs"
#load "Transform.fs"

open FParsec
open FsharpTransform.AST
open FsharpTransform.Parser
open FsharpTransform.Transform

let transformed = 
 let doc = 
  parseConfiguration @"/Users/dickow/Projects/FsharpTransform/FsharpTransform/transformConfig.json" 
   System.Text.Encoding.UTF8
 let jsonSrc = 
  parseJsonFile @"/Users/dickow/Projects/FsharpTransform/FsharpTransform/input.json" System.Text.Encoding.UTF8
 match (doc, jsonSrc) with
 | (Success(d, _, _), Success(json, _, _)) -> transformJsonToMap d json |> mapToJsonString
 | (_, _) -> System.String.Empty
System.IO.File.Delete(@"/Users/dickow/Projects/FsharpTransform/FsharpTransform/output.json")
let filePath = @"/Users/dickow/Projects/FsharpTransform/FsharpTransform/output.json"
System.IO.File.AppendAllText(path=filePath, contents=transformed)

#r "/Users/dickow/Projects/FJsonTransform/packages/FParsec.1.0.2/lib/net40-client/FParsecCS.dll"
#r "/Users/dickow/Projects/FJsonTransform/packages/FParsec.1.0.2/lib/net40-client/FParsec.dll"
#load "AST.fs"
#load "Parser.fs"
#load "Transform.fs"
#load "PrettyPrint.fs"

open FParsec
open FJsonTransform.AST
open FJsonTransform.Parser
open FJsonTransform.Transform
open FJsonTransform.PrettyPrint

// Example of how to use the library in its current state.
let transformed = 
 let doc = parseConfiguration @"/Users/dickow/git/FJsonTransform/FJsonTransform/TestAssets/transformConfig.json" System.Text.Encoding.UTF8
 let jsonSrc = parseJsonFile @"/Users/dickow/git/FJsonTransform/FJsonTransform/TestAssets/input.json" System.Text.Encoding.UTF8
 match (doc, jsonSrc) with
 | (Success(d, _, _), Success(json, _, _)) -> transformJsonToMap d json |> mapToJsonString
 | (_, _) -> System.String.Empty

System.IO.File.Delete(@"/Users/dickow/git/FJsonTransform/FJsonTransform/output.json")
let filePath = @"/Users/dickow/git/FJsonTransform/FJsonTransform/output.json"
System.IO.File.AppendAllText(path=filePath, contents=transformed)



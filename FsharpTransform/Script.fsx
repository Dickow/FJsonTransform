// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.

#r "/Users/dickow/Projects/FsharpTransform/packages/FParsec.1.0.2/lib/net40-client/FParsecCS.dll";;
#r "/Users/dickow/Projects/FsharpTransform/packages/FParsec.1.0.2/lib/net40-client/FParsec.dll";;

open FParsec

type Property = 
  | Property of string
 type PropertyTransform = 
  | PropertyTransform of (Property * Property)
 type Transformation = 
  | Transformation of PropertyTransform list
  type Document = {configuration:Transformation}

let ws = spaces
let str s = pstring s
let test p str =
    match run p str with
    | Success(result, _, _)   -> printfn "Success: %A" result
    | Failure(errorMsg, _, _) -> printfn "Failure: %s" errorMsg

let stringLiteral =
    let escape =  anyOf "\"\\/bfnrt"
                  |>> function
                      | 'b' -> "\b"
                      | 'f' -> "\u000C"
                      | 'n' -> "\n"
                      | 'r' -> "\r"
                      | 't' -> "\t"
                      | c   -> string c // every other char is mapped to itself

    let unicodeEscape =
        /// converts a hex char ([0-9a-fA-F]) to its integer number (0-15)
        let hex2int c = (int c &&& 15) + (int c >>> 6)*9

        str "u" >>. pipe4 hex hex hex hex (fun h3 h2 h1 h0 ->
            (hex2int h3)*4096 + (hex2int h2)*256 + (hex2int h1)*16 + hex2int h0
            |> char |> string
        )

    let escapedCharSnippet = str "\\" >>. (escape <|> unicodeEscape)
    let normalCharSnippet  = manySatisfy (fun c -> c <> '"' && c <> '\\')

    between (str "\"") (str "\"")
            (stringsSepBy normalCharSnippet escapedCharSnippet)

let fill = test stringLiteral "\"hello\""

// Create functions to create the AST types
let property (s:string) = s |> Property
let propertytransform (result:(Property * Property)) = result |> PropertyTransform
let configuration result = result |> Transformation
let document propertylist : Document = {configuration=propertylist}

// Create the parsers from the bottom up
let psource src = str src >>. ws >>. str ":" >>. (ws >>. stringLiteral) |>> property
let psource1 = psource "\"src1\""
let psource2 = psource "\"src2\""
let pproperty = str "{" >>. ws >>. (psource1) .>> ws .>> str "," .>> ws .>>. (psource2) .>> ws .>> str "}" |>> propertytransform
let ppropertylist = between (str "[") (str "]") (ws >>. sepBy (pproperty .>> ws) (str "," >>. ws) |>> configuration)
let pdocument = str "{" >>. ws >>. str "\"transform\"" >>. ws >>. str ":" >>. ws >>. (ppropertylist) .>> ws .>> str "}" .>> ws .>> eof |>> document

/// Test the parsers
test psource1 "\"src1\" : \"firstname\""
test psource2 "\"src2\" : \"firstname\""

test pproperty "{\"src1\" : \"firstname\",\"src2\" : \"first\"}"
let result = test pdocument "{\"transform\": [{\"src1\" : \"firstname\",\"src2\" : \"first\"},{\"src1\" : \"lastname\",\"src2\" : \"last\"},{\"src1\" : \"fullname\",\"src2\" : \"name\"}]}"

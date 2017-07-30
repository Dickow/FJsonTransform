namespace FsharpTransform

open FParsec
open AST

module Parser = 
 let ws = spaces
 let str s = pstring s
 let quoteString s = between (str "\"") (str "\"") (str s)

 // Code from the FParsec tutorial to parse a string literal
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
         // converts a hex char ([0-9a-fA-F]) to its integer number (0-15)
         let hex2int c = (int c &&& 15) + (int c >>> 6)*9

         str "u" >>. pipe4 hex hex hex hex (fun h3 h2 h1 h0 ->
             (hex2int h3)*4096 + (hex2int h2)*256 + (hex2int h1)*16 + hex2int h0
             |> char |> string
         )

     let escapedCharSnippet = str "\\" >>. (escape <|> unicodeEscape)
     let normalCharSnippet  = manySatisfy (fun c -> c <> '"' && c <> '\\')

     between (str "\"") (str "\"")
             (stringsSepBy normalCharSnippet escapedCharSnippet)
 
 // Create the parsers from the bottom up
 let psource src = quoteString src >>. ws >>. str ":" >>. (ws >>. stringLiteral) |>> property
 let psource1 = psource "src1"
 let psource2 = psource "src2"
 let pproperty = str "{" >>. ws >>. (psource1) .>> ws .>> str "," .>> ws .>>. (psource2) .>> ws .>> str "}" |>> propertytransform
 let ppropertylist = between (str "[") (str "]") (ws >>. sepBy (pproperty .>> ws) (str "," >>. ws) |>> configuration)
 let pdocument = str "{" >>. ws >>. quoteString "transform" >>. ws >>. str ":" >>. ws >>. (ppropertylist) .>> ws .>> str "}" .>> ws .>> eof |>> document

 // Here to remove type inference issues
 let private test p str =
     match run p str with
     | Success(result, _, _)   -> printfn "Success: %A" result
     | Failure(errorMsg, _, _) -> printfn "Failure: %s" errorMsg
 let private value = test pdocument
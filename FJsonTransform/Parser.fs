//
//  Author:
//    Jeppe Dickow Jeppedic@gmail.com
//
//  Copyright (c) 2017
//
//  All rights reserved.
//
//  Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
//     * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in
//       the documentation and/or other materials provided with the distribution.
//     * Neither the name of the [ORGANIZATION] nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
//  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
//  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
//  A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
//  CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
//  EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
//  PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
//  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
//  LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
//  NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//  SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

namespace FJsonTransform

open FParsec
open AST

module Parser = 
 let ws = spaces
 let str s = pstring s


 let listBetweenStrings sOpen sClose pElement delimiter f =
     between (str sOpen) (str sClose)
             (ws >>. sepBy (pElement .>> ws) (str delimiter >>. ws) |>> f)



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
 
 let quoteString s = between (str "\"") (str "\"") (str s)
 let pkey key = quoteString key >>. ws >>. str ":"
 let pruleValue ctor = ws >>. (listBetweenStrings "\"" "\"" (many1Chars (noneOf "\"->")) "->" ctor) .>> ws

 // Custom language using Json format 
 // Create the parsers from the bottom up
 let prule src ctor = pkey src >>. pruleValue ctor
 let psource = prule "source" sourceRule
 let pdestination = prule "destination" destinationRule
 let pproperty = str "{" >>. ws >>. (psource) .>> ws .>> str "," .>> ws .>>. (pdestination) .>> ws .>> str "}" |>> Property
 let ppropertylist = between (str "[") (str "]") (ws >>. sepBy (pproperty .>> ws) (str "," >>. ws)) |>> propertyList
 let pdocument = str "{" >>. ws >>. pkey "transform" >>. ws >>. (ppropertylist) .>> ws .>> str "}" .>> ws .>> eof |>> document

 let parseConfiguration filePath encoding = runParserOnString pdocument () filePath (System.IO.File.ReadAllText(filePath, encoding))

 // End of Custom language parser

 // Json parser using FParsec tutorial
 let pjvalue, pjvalueref = createParserForwardedToRef<Json, unit>()

 // Parse null values in the Json file
 let pjnull = stringReturn "null" JNull

 // Parse true values in the Json file
 let pjtrue = stringReturn "true" (JBool true)

 // Parse false values in the Json file
 let pjfalse = stringReturn "false" (JBool false)

 // Parse strings in the Json file
 let pjstring = stringLiteral |>> JString

 // Parse numbers in the json file
 let pjnumber = pfloat |>> JNumber

 // Parse a Json list
 let pjlist = listBetweenStrings "[" "]" pjvalue "," JList

 // Parse a key value object in the json file
 let pkeyvalue = stringLiteral .>>. (ws >>. str ":" >>. ws >>. pjvalue)

 // Parse a Json object in the Json file
 let pjobject = listBetweenStrings "{" "}" pkeyvalue "," (Map.ofList >> JObject)

 pjvalueref := choice 
     [
     pjnull 
     pjtrue
     pjfalse
     pjnumber
     pjstring
     pjlist
     pjobject
     ]

 let pjson = ws >>. pjvalue .>> ws .>> eof

 let parseJsonString str = run pjson str

 let parseJsonFile filePath encoding = 
  runParserOnString pjson () filePath (System.IO.File.ReadAllText(filePath, encoding))
 // End of Json parser
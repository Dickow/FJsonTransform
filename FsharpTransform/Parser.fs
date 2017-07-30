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
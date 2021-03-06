﻿//
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
open AST
module Transform = 
 // Represent any Json token as a string
 let rec getStrRepr json = 
  match json with
  | JNull -> "null"
  | JNumber(f) -> sprintf "\"%f\"" f
  | JBool(true) -> "true"
  | JBool(false) -> "false"
  | JString(s) -> sprintf "\"%s\"" s
  | JList(l) -> 
   l
   |> List.map (fun item -> getStrRepr item)
   |> String.concat ","
   |> sprintf "[ %s ]"
  | JObject(map) -> 
   map
   |> Map.fold (fun state key value -> sprintf "\"%s\" : %s" key (getStrRepr value) :: state) List.Empty
   |> String.concat ","
   |> sprintf "{ %s }"
 
 let mapToJsonString map = 
  map
  |> Map.fold (fun state key value -> sprintf "\"%s\" : %s" key (getStrRepr value) :: state) List.Empty
  |> String.concat ","
  |> sprintf "{ %s }"
 
 // Try to get the property from the map Some if found otherwise None
 let tryGetProperty prop json = 
  match json with
  | JObject(properties) -> properties.TryFind prop
  | _ -> None
 
 let mapTransformer state (item : PropertyTransform) = 
  let src = fstStr item
  let dst = sndStr item
  let propValue = tryGetProperty src (fst state)
  match propValue with
  | Some(jProp) -> (fst state, (snd state) |> Map.add dst jProp)
  | None -> state
 
 let transformJsonToMap (doc : Document) json = 
  doc.configuration.properties
  |> List.fold mapTransformer (json, Map.empty)
  |> snd
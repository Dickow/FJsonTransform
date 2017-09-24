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
open AST
module Transform = 

 let hasProperty prop json =
  match json with
  | JObject(properties) -> properties.ContainsKey prop
  | _ -> false

 // Try to get the property from the map Some if found otherwise None
 let tryGetProperty prop json = 
  match json with
  | JObject(properties) -> properties.TryFind prop
  | _ -> None

 let getProperty prop json = 
  match json with
  | JObject(properties) -> properties.Item prop
  | JString(_) | JBool(_) | JNull | JNumber(_) -> failwith "Cannot get index into anything but JObjects" 
  | _ -> failwith "key not found"

 let rec tryGetRelationProperty propList json = 
  match propList with
  | [] -> None
  | [x] -> tryGetProperty x json
  | x::xtail when hasProperty x json -> tryGetRelationProperty xtail (getProperty x json)
  | _ -> None

 let insertValue key (value:Json) json =
  match json with
  | JObject(map) -> if (Map.containsKey key map) then json else JObject(Map.add key value map)
  | _ -> json 
  
 let ensureMapPath key json = 
  match json with 
  | JObject(map) -> if (Map.containsKey key map) then json else JObject(Map.add key (JObject(Map.empty)) map)
  | _ -> json
  
 let rec insert key jValue json =
   match key with
   | Flat(value) -> insertValue value jValue json
   | Relation(value) -> match value with
                          | [] -> json
                          | [x] -> insertValue x jValue json
                          | x::xtail -> insertValue x (insert (Relation(xtail)) jValue (getProperty x (ensureMapPath x json))) json
                         
 let testAndInsert dst (prop:Json option) map = 
  match prop with
  | Some(prop) -> insert dst prop map
  | None -> map

 let mapTransformer (json, map) (Property(SourceRule(src), DestinationRule(dst))) = 
  match src with
  | Flat(value) -> (json,testAndInsert dst (tryGetProperty value json) map)
  | Relation(value) -> (json, testAndInsert dst (tryGetRelationProperty value json) map)
 
 let transformJsonToMap (doc : Document) json = 
  doc.configuration 
  |> List.fold mapTransformer (json, JObject(Map.empty)) 
  |> snd
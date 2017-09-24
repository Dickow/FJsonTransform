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

module AST = 

 type PropertyRule = 
  | Flat of string
  | Relation of string list

 type DestinationRule = DestinationRule of PropertyRule
 type SourceRule = SourceRule of PropertyRule
 type Property = Property of (SourceRule * DestinationRule)
 type Document = {configuration:Property list}

 let propertyRule strList = 
  match strList with
  | [] -> failwith "destination rules must have at least one property satisfied"
  | xlist when List.length xlist = 1 -> Flat xlist.[0]
  | xlist -> Relation xlist

 let destinationRule strList = DestinationRule (propertyRule strList)
 let sourceRule strList = SourceRule (propertyRule strList)
 let propertyList (list:Property list) = list
 let document input = {configuration=input}

 // Json AST
 type Json = 
  | JBool of bool
  | JString of string
  | JNumber of float
  | JNull
  | JList of Json list
  | JObject of Map<string, Json>
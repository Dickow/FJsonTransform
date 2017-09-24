namespace FJsonTransformTest

open FJsonTransform
open FJsonTransform.AST
open FJsonTransform.Parser
open FJsonTransform.Transform
open FParsec
open NUnit.Framework
open System

[<TestFixture>]
type TransformTests() = 
    
    [<Test>]
    member this.TestTryGetPropertyWhenExists() = 
        let value1 = JString("hello")
        let value2 = JString("world")
        let map = Map.empty |> Map.add "value1" value1 |> Map.add "value2" value2
        let json = JObject(map)
        let result = tryGetProperty "value1" json
        match result with
        | Some(value) -> Assert.AreEqual(value1, value)
        | None -> failwith "Property was not found"
        
    [<Test>]
    member this.TestTryGetPropertyWhenNotExists() = 
        let value1 = JString("hello")
        let value2 = JString("world")
        let map = Map.empty |> Map.add "value1" value1 |> Map.add "value2" value2
        let json = JObject(map)
        let result = tryGetProperty "value3" json
        match result with
        | Some(value) -> failwith "Expected not to find any properties with value3" 
        | None -> "" |> ignore
        
    [<Test>]
    member this.TestTryGetPropertyWhenExistsInSubMap() = 
        let value1 = JString("hello")
        let value2 = JString("world")
        let submap = Map.empty |> Map.add "value1" value1 |> Map.add "value2" value2 
        let map = Map.empty |> Map.add "submap" (JObject(submap))
        let json = JObject(map)
        let result = tryGetProperty "value1" json
        match result with
        | Some(value) -> failwith "Expected not to find any properties with value1, this should be located in a sub map" 
        | None -> "" |> ignore
        
    [<Test>]
    member this.TestContainsKeyWhenExists() = 
        let value1 = JString("hello")
        let value2 = JString("world")
        let map = Map.empty |> Map.add "value1" value1 |> Map.add "value2" value2
        let json = JObject(map)
        let result = hasProperty "value1" json
        Assert.IsTrue result
        
    [<Test>]
    member this.TestContainsKeyWhenNotExists() = 
        let value1 = JString("hello")
        let value2 = JString("world")
        let map = Map.empty |> Map.add "value1" value1 |> Map.add "value2" value2
        let json = JObject(map)
        let result = hasProperty "value3" json
        Assert.IsFalse result
        
    [<Test>]
    member this.TestContainsKeyWhenExistsInSubMap() = 
        let value1 = JString("hello")
        let value2 = JString("world")
        let submap = Map.empty |> Map.add "value1" value1 |> Map.add "value2" value2 
        let map = Map.empty |> Map.add "submap" (JObject(submap))
        let json = JObject(map)
        let result = hasProperty "value1" json
        Assert.IsFalse result
    
    [<Test>]
    member this.TestContainsHiearchyKeyWhenExistsAtLevel1() =
        let value1 = JString("hello")
        let value2 = JString("world")
        let submap = Map.empty |> Map.add "value1" value1 |> Map.add "value2" value2 
        let map = Map.empty |> Map.add "submap" (JObject(submap))
        let json = JObject(map)
        let result = tryGetRelationProperty ["submap"; "value1"] json
        match result with
        | Some(value) -> Assert.AreEqual(value1, value)
        | None -> failwith "Failed to find the property submap->value1"
    
    [<Test>]
    member this.TestContainsHiearchyKeyWhenExistsAtLevel4() =
        let value1 = JString("hello")
        let value2 = JString("world")
        let submap3 = Map.empty |> Map.add "value1" value1 |> Map.add "value2" value2
        let submap2 = Map.empty |> Map.add "submap3" (JObject(submap3))
        let submap1 = Map.empty |> Map.add "submap2" (JObject(submap2))
        let map = Map.empty |> Map.add "submap1" (JObject(submap1))
        let json = JObject(map)
        let result = tryGetRelationProperty ["submap1"; "submap2"; "submap3"; "value1"] json
        match result with
        | Some(value) -> Assert.AreEqual(value1, value)
        | None -> failwith "Failed to find the property submap1->submap2->submap3->value1"
        
    [<Test>]
    member this.TestContainsHiearchyKeyWhenNotExists() =
        let value1 = JString("hello")
        let value2 = JString("world")
        let submap = Map.empty |> Map.add "value1" value1 |> Map.add "value2" value2 
        let map = Map.empty |> Map.add "submap" (JObject(submap))
        let json = JObject(map)
        let result = tryGetRelationProperty ["submap"; "value3"] json
        match result with
        | Some(value) -> failwith "Expected not to find any values"
        | None -> "" |> ignore
    
    [<Test>]
    member this.TestInsertFlatStringValueInEmptyMap() = 
        let value = JString("Hello World")
        let result = insertValue "key" value (JObject(Map.empty))
        let expected = Map.empty |> Map.add "key" value |> JObject
        Assert.AreEqual(expected, result)
    
    [<Test>]
    member this.TestInsertFlatObjectValueInEmptyMap() = 
        let value = Map.empty |> Map.add "key" (JString("Hello world")) |> JObject
        let result = insertValue "key" value (JObject(Map.empty))
        let expected = Map.empty |> Map.add "key" (value) |> JObject
        Assert.AreEqual(expected, result)
        
    [<Test>]
    member this.TestInsertRelationStringValueInEmptyMap() = 
        let value = JString("Hello World")
        let result = insert (Relation(["dest1"; "dest2"])) value (JObject(Map.empty))
        let expected = (Map.empty |> Map.add "dest1" (JObject(Map.empty |> Map.add "dest2" value))) |> JObject
        Assert.AreEqual(expected, result)
    
    [<Test>]
    member this.TestInsertRelationObjectValueInEmptyMap() = 
        let value = Map.empty |> Map.add "key" (JString "Hello World") |> JObject
        let result = insert (Relation(["dest1"; "dest2"])) value (JObject(Map.empty))
        let expected = (Map.empty |> Map.add "dest1" (JObject(Map.empty |> Map.add "dest2" value))) |> JObject
        Assert.AreEqual(expected, result)
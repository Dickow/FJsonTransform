namespace FJsonTransformTest

open FJsonTransform
open FJsonTransform.AST
open FJsonTransform.Parser
open FParsec
open NUnit.Framework
open System

[<TestFixture>]
type ParserTests() = 
    
    [<Test>]
    member this.ParseManySourceHierarchyKey() = 
        let str = "\"source\" : \"obj->val1->val2\""
        let result = run psource str
        let expected = (SourceRule(Relation([ "obj"; "val1"; "val2" ])))
        match result with
        | Success(value, _, _) -> Assert.AreEqual(expected, value)
        | Failure(fail, _, _) -> failwithf "Failed to parse the string %s" fail
        
    [<Test>]
    member this.ParseManySourceHierarchyKeyWithTrailingCharacters() = 
        let str = "\"source\" : \"obj->val1->val2\", \"this should be ignored\""
        let result = run psource str
        let expected = (SourceRule(Relation([ "obj"; "val1"; "val2" ])))
        match result with
        | Success(value, _, _) -> Assert.AreEqual(expected, value)
        | Failure(fail, _, _) -> failwithf "Failed to parse the string %s" fail        
    
    [<Test>]
    member this.ParseSingleSourceValue() = 
        let str = "\"source\" : \"val1\""
        let result = run psource str
        let expected = (SourceRule(Flat("val1")))
        match result with
        | Success(value, _, _) -> Assert.AreEqual(expected, value)
        | Failure(fail, _, _) -> failwithf "Failed to parse the string %s" fail
    
    [<Test>]
    member this.ParseSingleSourceHiearchyKey() = 
        let str = "\"source\" : \"obj->val1\""
        let result = run psource str
        let expected = (SourceRule(Relation([ "obj"; "val1" ])))
        match result with
        | Success(value, _, _) -> Assert.AreEqual(expected, value)
        | Failure(fail, _, _) -> failwithf "Failed to parse the string %s" fail
    
    [<Test>]
    member this.ParseMalformedSource() = 
        let str = "\"source\" : \"obj->\""
        let result = run psource str
        match result with
        | Success(value, _, _) -> failwithf "Expected failure when parsing input, but got %A" value
        | Failure(fail, _, _) -> fail |> ignore
    
    [<Test>]
    member this.ParseManyDestinationHierarchyKey() = 
        let str = "\"destination\" : \"obj->val1->val2\""
        let result = run pdestination str
        let expected = (DestinationRule(Relation([ "obj"; "val1"; "val2" ])))
        match result with
        | Success(value, _, _) -> Assert.AreEqual(expected, value)
        | Failure(fail, _, _) -> failwithf "Failed to parse the string %s" fail
        
    [<Test>]
    member this.ParseManyDestinationHierarchyKeyWithTrailingCharacters() = 
        let str = "\"destination\" : \"obj->val1->val2\", \"This should be ignored\""
        let result = run pdestination str
        let expected = (DestinationRule(Relation([ "obj"; "val1"; "val2" ])))
        match result with
        | Success(value, _, _) -> Assert.AreEqual(expected, value)
        | Failure(fail, _, _) -> failwithf "Failed to parse the string %s" fail
    
    [<Test>]
    member this.ParseSingleDestinationValue() = 
        let str = "\"destination\" : \"val1\""
        let result = run pdestination str
        let expected = (DestinationRule(Flat("val1")))
        match result with
        | Success(value, _, _) -> Assert.AreEqual(expected, value)
        | Failure(fail, _, _) -> failwithf "Failed to parse the string %s" fail
    
    [<Test>]
    member this.ParseSingleDestinationHiearchyKey() = 
        let str = "\"destination\" : \"obj->val1\""
        let result = run pdestination str
        let expected = (DestinationRule(Relation([ "obj"; "val1" ])))
        match result with
        | Success(value, _, _) -> Assert.AreEqual(expected, value)
        | Failure(fail, _, _) -> failwithf "Failed to parse the string %s" fail
    
    [<Test>]
    member this.ParseMalformedDestination() = 
        let str = "\"destination\" : \"obj->\""
        let result = run pdestination str
        match result with
        | Success(value, _, _) -> failwithf "Expected failure when parsing input, but got %A" value
        | Failure(fail, _, _) -> fail |> ignore
    
    [<Test>]
    member this.ParsePropertyWithManyHierachyKeys() = 
        let str = "{\"source\" : \"val1->val2->val3\" ," + "\n \"destination\" : \"dest1->dest2\"}"
        let result = run pproperty str
        let expected = Property((SourceRule(Relation["val1"; "val2"; "val3"]), DestinationRule(Relation(["dest1"; "dest2"]))))
        match result with
        | Success(value, _, _) -> Assert.AreEqual(expected, value)
        | Failure(fail, _, _) -> failwithf "Failed to parse the string %s" fail
    
    [<Test>]
    member this.ParsePropertyWithSingleKey() = 
        let str = "{\"source\" : \"val1\" , \"destination\" : \"dest1->dest2\"}"
        let result = run pproperty str
        let expected = Property((SourceRule(Flat("val1")), DestinationRule(Relation(["dest1"; "dest2"]))))
        match result with
        | Success(value, _, _) -> Assert.AreEqual(expected, value)
        | Failure(fail, _, _) -> failwithf "Failed to parse the string %s" fail
        
    [<Test>]
    member this.ParseMalformedProperty() = 
        let str = "{\"source\" : \"val1->val2->\" , \"destination\" : \"dest1-dest2\"}"
        let result = run pproperty str
        match result with
        | Success(value, _, _) -> failwithf "Expected failure when parsing input, but got %A" value
        | Failure(fail, _, _) -> fail |> ignore   
    
    [<Test>]
    member this.ParseTransformationForSingleConfiguration() = 
        let str = "{\"transform\": [{\"source\":\"val1\",\"destination\": \"dest1\"}]}"
        let result = run pdocument str
        let expected = { configuration = [ Property((SourceRule(Flat("val1")), DestinationRule(Flat("dest1")))) ] }
        match result with
        | Success(value, _, _) -> Assert.AreEqual(expected, value)
        | Failure(fail, _, _) -> failwithf "Failed to parse the string %s" fail
    
    [<Test>]
    member this.ParseTransformationForManyConfigurations() = 
        let str = 
            "{\"transform\": [{\"source\":\"val1\",\"destination\": \"dest1\"},{\"source\":\"obj->val2->val3\",\"destination\": \"dest1->val1\"},{\"source\":\"obj->val1\",\"destination\": \"dest1\"}]}"
        let result = run pdocument str
        
        let expected = 
            { configuration = 
                  [ Property((SourceRule(Flat("val1")), DestinationRule(Flat("dest1"))));
                    Property((SourceRule(Relation(["obj";"val2";"val3"])), DestinationRule(Relation(["dest1"; "val1"]))));
                    Property((SourceRule(Relation(["obj"; "val1"])), DestinationRule(Flat("dest1")))) ] }
        match result with
        | Success(value, _, _) -> Assert.AreEqual(expected, value)
        | Failure(fail, _, _) -> failwithf "Failed to parse the string %s" fail

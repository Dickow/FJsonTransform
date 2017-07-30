namespace FsharpTransform

module AST = 
 type Property = 
 | Property of string
 type PropertyTransform = 
 | PropertyTransform of (Property * Property)
 type Transformation = 
 | Transformation of PropertyTransform list
 type Document = {configuration:Transformation}

 let property (s:string) = s |> Property
 let propertytransform (result:(Property * Property)) = result |> PropertyTransform
 let configuration result = result |> Transformation
 let document propertylist : Document = {configuration=propertylist}
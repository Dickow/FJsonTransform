# FJsonTransform

### The purpose of this library is to transform between two string representation of Json objects, using a configuration file written in Json. 

The library is intended to work in the following way:

```
{
  "complexObj": {
    "val1": "This is a simple value in an object"
  },
  "simpleValue": 2003,
  "superComplex":{
    "obj1":{
      "obj2":{
        "obj3":{
          "prop1": 390,
          "prop2": "This is cool and 'Hello World' btw!"
        }
      }
    }
  },
  "flatSource": "This was a flat source"
}
```

Transformed to ----> 

```
{
    "flattened": "This is a simple value in an object",
    "fairlyComplex": {
        "dst1": {
            "dst3": {
                "prop2": "This is cool and 'Hello World' btw!",
                "prop1": "390.000000"
            }
        }
    },
    "destination": "2003.000000",
    "complexMapping": {
        "dst1": {
            "dst2": "This was a flat source"
        }
    }
}
```

This should be done using a configuration json file:

```
{
  "transform": [
    {
      "source": "complexObj->val1",
      "destination": "flattened"
    },
    {
      "source": "simpleValue",
      "destination": "destination"
    },
    {
      "source": "superComplex->obj1->obj2->obj3",
      "destination": "fairlyComplex->dst1->dst3"
    },
    {
      "source": "flatSource",
      "destination": "complexMapping->dst1->dst2"
    }
  ]
}
```

### Syntax of FJsonTransform
The syntax of FJsonTransform has been kept very simple.
To perform a transformation from one objec to another simply specify the source property "source".
To query properties that are in nested objects, you can traverse the object tree using the operator "->". 
If the property is not found, it is simply ignored and the transformation will continue with the next property.
 
For the destination mapping, the example above shows that it is also possible to take a flat object and place it anywhere in the destination object. 
Eg. moving a simple string to be part of a complex object, or create new objects in the object tree.

The only required attribute for now is the "transform" attribute, this indicates the configuration for the transformation.


### Next version of FJsonTransform
The next version of FJsonTransform will include the possiblity to use predefined functions during the transformation.
For now the functions that will be included are: 
* Trim
* ToLower
* ToUpper
* Concat (With constant strings)

Other features in the future includes C# wrapper library, to allow usage in the C# language.
New runner classes to ease usage of the library. 
Logging when executing functions during the transformation.
Relational concatenation, eg. use other attributes from the object tree, like Concat(obj1->val1, "@", obj2->val3).
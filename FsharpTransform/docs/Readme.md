# FsharpTransform

### The purpose of this library is to transform between two string representation of Json objects, using a configuration file written in Json. 

The library is intended to work in the following way:

```
{
    "firstname" : "Jeppe",
    "lastname" : "Dickow",
    "fullname" : "Jeppe Dickow"
}
```

Transformed to ----> 

```
{
    "first" : "Jeppe",
    "last" : "Dickow",
    "name" : "Jeppe Dickow"
}
```

This should be done using a configuration json file:

```
{
    "transform": [
        {
            "src1" : "firstname",
            "src2" : "first"
        },
        {
            "src1" : "lastname",
            "src2" : "last"
        },
        {
            "src1" : "fullname",
            "src2" : "name"
        }
    ]
}
```

This is what the version 0.0.1 would look like. 
Future versions would include ignore properties, eg only transforming two properties from one object to another and ignoring the rest. 
Functions to be performed during the transformation is another feature that would be added in a future release.
Eg. transform this property to here and split it from 0 to 10 or transform this property to here and uppercase it or lowercase it.
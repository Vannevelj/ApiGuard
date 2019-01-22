# What is this?

This is a unit testing API that ensures the API of your service remains backwards compatible. When it encounters a difference between the original API and the new one it will try to give an as specific as possible error message. Should you as a maintainer decide to accept the change then it is as simple as removing the generated file and it will re-generate it on the next build.

It creates a json file within the unit test directory under a `generated_api` folder. This file is a representation of your API and specifies the attributes, modifiers, properties and methods it contains.

I have opted not to use a formal json schema as I didn't see the possibility for that to support everything I wanted. I might have missed it though so if the schema definition can support things like attributes and their values, methods on objects, type distinction (interface, class, abstract class) and more then I would be very interested in more info on this however.

The library uses Reflection to read the data in to ensure that it would work even if source code is not available (i.e. if it is defined in an external library). This makes no huge difference in terms of ease of parsing the type into our intermediate representation but it does make it easier by avoiding filesystem interaction.

# How to use

```csharp
using ApiGuard;

[Fact]
public void ExampleService_Api_HasNotChanged() =>
    ApiAssert.HasNotChanged(typeof(MyExampleService));
```
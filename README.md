# What is this?

This is a unit testing API that ensures the API of your service remains backwards compatible.

# How to use

```csharp
[Fact]
public async Task ExampleService_Api_HasNotChanged() => await ApiGuard.ApiAssert.HasNotChanged(typeof(MyExampleService));
```
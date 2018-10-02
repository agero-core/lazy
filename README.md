# Lazy

[![NuGet Version](http://img.shields.io/nuget/v/Agero.Core.Lazy.svg?style=flat)](https://www.nuget.org/packages/Agero.Core.Lazy/) 
[![NuGet Downloads](http://img.shields.io/nuget/dt/Agero.Core.Lazy.svg?style=flat)](https://www.nuget.org/packages/Agero.Core.Lazy/)

Helpers for lazy initialization in .NET applications.

### SyncLazy
[`SyncLazy`](./Agero.Core.Lazy/SyncLazy.cs) is ```System.Lazy<T>``` wrapper which re-creates lazily initiated value when lazy initialization throws exception.   

It supports the same interface as ```System.Lazy<T>```.
```csharp
var counter = 0;
var lazy = new SyncLazy<int>(() => ++counter);

var value1 = lazy.Value;
Assert.AreEqual(1, value1);

var value2 = lazy.Value;
Assert.AreEqual(1, value2);
```
It also supports clearing of lazily initiated value.
```csharp
var counter = 0;
var lazy = new SyncLazy<int>(() => ++counter);

var value1 = lazy.Value;
Assert.AreEqual(1, value1);

lazy.ClearValue();

var value2 = lazy.Value;
Assert.AreEqual(2, value2);
```
It does not cache lazy initialization exceptions.
```csharp
string returnValue = null;
var lazy = new SyncLazy<string>(() => returnValue ?? throw new InvalidOperationException("Error"));

try
{
    var unused = lazy.Value;

    Assert.Fail("Exception is not thrown");
}
catch (InvalidOperationException ex) when (ex.Message == "Error")
{
}

returnValue = "Some value";

var value = lazy.Value;
Assert.AreEqual(returnValue, value);
```

### AsyncLazy
[`AsyncLazy`](./Agero.Core.Lazy/AsyncLazy.cs) is limited async implementation of ```System.Lazy<T>``` with ```System.Threading.Tasks.Task<TResult>``` re-initialization when ```System.Threading.Tasks.Task<TResult>``` is not succefully completed.

It supports basic functionality of ```System.Lazy<T>```.
```csharp
var counter = 0;
async Task<int> ValueFactory() => await Task.Run(() => ++counter);

var lazy = new AsyncLazy<int>(ValueFactory);

var value1 = await lazy.GetValueAsync();
Assert.AreEqual(1, value1);

var value2 = await lazy.GetValueAsync();
Assert.AreEqual(1, value2);
```
It also supports clearing of lazily initiated value.
```csharp
var counter = 0;
async Task<int> ValueFactory() => await Task.Run(() => ++counter);

var lazy = new AsyncLazy<int>(ValueFactory);

var value1 = await lazy.GetValueAsync();
Assert.AreEqual(1, value1);

lazy.ClearValue();

var value2 = await lazy.GetValueAsync();
Assert.AreEqual(2, value2);
```

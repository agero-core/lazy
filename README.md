# Lazy

[![NuGet Version](http://img.shields.io/nuget/v/Agero.Core.Lazy.svg?style=flat)](https://www.nuget.org/packages/Agero.Core.Lazy/) 
[![NuGet Downloads](http://img.shields.io/nuget/dt/Agero.Core.Lazy.svg?style=flat)](https://www.nuget.org/packages/Agero.Core.Lazy/)

Helpers for lazy operations. 

* SyncLazy usage
```csharp
var syncLazy = new SyncLazy<string>(() => 
{
    return "sample string";
});
var value = syncLazy.Value;
```

* AyncLazy usage
```csharp
async Task<string> ValueFactory() => await Task.Run(() =>
{
    return "sample string";
});
var asyncLazy = new SyncLazy<string>(ValueFactory);
var value = asyncLazy.Value;
```

# DeepCloning
Provides a set of methods to deep-copy an instance of any object. Implemented in Common Intermediate Language (.NET Assembly) offering performance equivalent to hand-written code. No library dependencies.

[![](https://img.shields.io/nuget/v/ShereSoft.DeepCloning.svg)](https://www.nuget.org/packages/ShereSoft.DeepCloning/)
[![](https://img.shields.io/nuget/dt/ShereSoft.DeepCloning)](https://www.nuget.org/packages/ShereSoft.DeepCloning/)
[![Actions Status](https://github.com/ShereSoft/DeepCloning/workflows/Build/badge.svg)](https://github.com/ShereSoft/DeepCloning/actions)

* Can deep-clone instances of any object
* Implemented in Common Intermediate Language (.NET Assembly), compiled for each type at runtime, and cached for subsequent use. 
* No use of Reflection for cloning
* Configurable for string handling (deep clone or not) and singleton handling (reuse or clone)
* Thread-safe
* Includes extension methods (ObjectExtensions) for conveniently calling from any object instance (Recommended usage)
* Unit Tested
* Multi-targeted (.NET 6, .NET 5, .NET Core 3.1, .NET Framework 4.8, .NET Framework 4.7, .NET Framework 4.6, .NET Framework 4.5, .NET Framework 4.0)
* No external library dependencies
* No external calls
<br />

## QUICK START
Using the extension methods (included) is recommended.

### .DeepClone()
``` csharp
using ShereSoft.Extensions;
:

Customer customer = new Customer();
:

Customer clone = customer.DeepClone();
```

## DeepCloningOptions 

### .DeepCloneStrings = true
``` csharp
var customer = new Customer();
customer.Name = "John";

var options = new DeepCloningOptions { DeepCloneStrings = true };
Customer clone = customer.DeepClone(options);

Debug.WriteLine(Object.ReferenceEquals(customer, clone));  // False (Default is reuse, NOT deep copy)
```
> Strings are reused by default as they are normally treated as immutable. If your code contains any string manipulation by pointer, use this option to prevent unwanted side effects to the original object.

### .DeepCloneSingletons = true
``` csharp
class Customer
{
    public static readonly Customer Empty = new Customer();  // Static readonly fields as singletons
    :
}

var customer = Customer.Empty;

var options = new DeepCloningOptions { DeepCloneSingletons = true };
Customer clone = customer.DeepClone(options);

Debug.WriteLine(Object.ReferenceEquals(customer, clone));  // False (Default is reuse, NOT deep copy)
```
> "static readonly" fields are assumed to be immutable. If your code is not, which by the way is not the best practice, use this option.
> 

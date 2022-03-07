using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace ShereSoft
{
    abstract class ClonerBase
    {
        protected static readonly MethodInfo ObjectDictionaryByObjectTryGetValueMethodInfo = typeof(Dictionary<object, object>).GetMethod("TryGetValue");
        protected static readonly MethodInfo ObjectGetTypeMethodInfo = typeof(object).GetMethod("GetType");
        protected static readonly MethodInfo TypeOpInequalityMethodInfo = typeof(Type).GetMethod("op_Inequality");
        protected static readonly MethodInfo TypeConcurrentDictionaryByCloneObjectDelegateTryGetValueMethodInfo = typeof(ConcurrentDictionary<Type, CloneObjectDelegate>).GetMethod("TryGetValue");
        protected static readonly MethodInfo TypeConcurrentDictionaryByCloneObjectDelegateTryAddMethodInfo = typeof(ConcurrentDictionary<Type, CloneObjectDelegate>).GetMethod("TryAdd");
        protected static readonly MethodInfo CloneObjectDelegateInvokeMethodInfo = typeof(CloneObjectDelegate).GetMethod("Invoke");
        protected static readonly MethodInfo DeepCloningBuildRedirectMethodInfo = typeof(DeepCloning).GetMethod("BuildRedirect", BindingFlags.NonPublic | BindingFlags.Static);
        protected static readonly MethodInfo DeepCloningOptionsGetDeepCloneSingletonsMethodInfo = typeof(DeepCloningOptions).GetProperty(nameof(DeepCloningOptions.None.DeepCloneSingletons)).GetGetMethod();
        protected static readonly MethodInfo DeepCloningOptionsGetDeepCloneStringsMethodInfo = typeof(DeepCloningOptions).GetProperty(nameof(DeepCloningOptions.None.DeepCloneStrings)).GetGetMethod();
        protected static readonly MethodInfo ObjectDictionaryByObjectAddMethodInfo = typeof(Dictionary<object, object>).GetMethod("Add");
        protected static readonly MethodInfo StringToCharArrayMethodInfo = typeof(string).GetMethod(nameof(String.Empty.ToCharArray), Type.EmptyTypes);
        protected static readonly ConstructorInfo StringCtor = typeof(string).GetConstructor(new[] { typeof(char[]) });
    }
}

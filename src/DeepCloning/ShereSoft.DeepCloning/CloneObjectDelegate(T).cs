using System;
using System.Collections.Generic;

namespace ShereSoft
{
    delegate T CloneObjectDelegate<T>(T source, Dictionary<object, object> reusableClones, DeepCloningOptions options);
}

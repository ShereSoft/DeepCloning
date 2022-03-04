using System;
using System.Collections.Generic;

namespace ShereSoft
{
    delegate T CloneObjectDelegate<T>(T src, Dictionary<object, object> objs, DeepCloningOptions options);
}

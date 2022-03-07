using System;
using System.Collections.Generic;

namespace ShereSoft
{
    delegate object CloneObjectDelegate(object source, Dictionary<object, object> reusableClones, DeepCloningOptions options);
}

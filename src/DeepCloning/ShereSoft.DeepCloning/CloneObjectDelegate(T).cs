using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShereSoft
{
    delegate T CloneObjectDelegate<T>(T src, Dictionary<object, object> objs, DeepCloningOptions options);
}

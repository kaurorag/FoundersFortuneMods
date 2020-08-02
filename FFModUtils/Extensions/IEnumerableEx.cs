using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFModUtils.Extensions {
    public static class IEnumerableEx {
        public static bool In<T>(this T item, params T[] objects) {
            return objects.Contains(item);
        }
    }
}

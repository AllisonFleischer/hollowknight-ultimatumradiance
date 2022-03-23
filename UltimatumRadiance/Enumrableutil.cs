using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace UltimatumRadiance
{
    public static class Enumrableutil
    {
        public static IEnumerable<T> RemoveFirst<T>(this IEnumerable<T> source,Func<T,bool>f)
        {
           using IEnumerator<T> enumerator = source.GetEnumerator();
            while(enumerator.MoveNext())
            {
                if(f(enumerator.Current))
                {
                    break;
                }
                yield return enumerator.Current;
            }
        }
        public static IEnumerable<T> Add<T>(this IEnumerable<T> source,T elem)
        {
            using IEnumerator<T> ir=source.GetEnumerator();
            while(ir.MoveNext())
            {
                yield return ir.Current;
            }
            yield return elem;
        }
        public static IEnumerable<T> Insert<T>(this IEnumerable<T>source,T elem, int index)
        {
            using IEnumerator<T> ir = source.GetEnumerator();
            int i = 0;
            for(;ir.MoveNext();i++)
            {
                if(i== index)
                {
                    yield return elem;
                }
                yield return ir.Current;
            }
            if(i== index)
            {
                yield return elem;
            }
        }
    }
}

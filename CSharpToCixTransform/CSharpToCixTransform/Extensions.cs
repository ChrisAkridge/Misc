using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpToCixTransform
{
	public static class Extensions
	{
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                list.Add(item);
            }
        }

        public static IEnumerable<string> Names(this IEnumerable<VariableDeclaration> declarations)
        {
            foreach (var declaration in declarations) { yield return declaration.VariableName; }
        }
	}
}

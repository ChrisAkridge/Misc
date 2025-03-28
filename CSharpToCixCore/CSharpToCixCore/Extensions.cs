using System.Collections.Generic;

namespace CSharpToCixCore
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

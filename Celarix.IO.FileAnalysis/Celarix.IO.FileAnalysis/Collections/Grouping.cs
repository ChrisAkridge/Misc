using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.Collections
{
    public class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        // https://stackoverflow.com/a/29159251/2709212
        public TKey Key { get; }
        private readonly IEnumerable<TElement> values;

        public Grouping(TKey key, IEnumerable<TElement> values)
        {
            Key = key;
            this.values = values ?? throw new ArgumentNullException(nameof(values));
        }

        public IEnumerator<TElement> GetEnumerator() => values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

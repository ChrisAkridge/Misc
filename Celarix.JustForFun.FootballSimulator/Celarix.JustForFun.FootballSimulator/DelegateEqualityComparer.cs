using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator
{
    internal class DelegateEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T?, T?, bool> equalsDelegate;
        private readonly Func<T, int> getHashCodeDelegate;

        public DelegateEqualityComparer(Func<T?, T?, bool> equalsDelegate, Func<T, int>? getHashCodeDelegate = null)
        {
            this.equalsDelegate = equalsDelegate;

            this.getHashCodeDelegate = getHashCodeDelegate ?? (o => o?.GetHashCode() ?? 0);
        }

        /// <summary>Determines whether the specified objects are equal.</summary>
        /// <param name="x">The first object of type <paramref name="T" /> to compare.</param>
        /// <param name="y">The second object of type <paramref name="T" /> to compare.</param>
        /// <returns>
        /// <see langword="true" /> if the specified objects are equal; otherwise, <see langword="false" />.</returns>
        public bool Equals(T? x, T? y) => equalsDelegate(x, y);

        /// <summary>Returns a hash code for the specified object.</summary>
        /// <param name="obj">The <see cref="T:System.Object" /> for which a hash code is to be returned.</param>
        /// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj" /> is a reference type and <paramref name="obj" /> is <see langword="null" />.</exception>
        /// <returns>A hash code for the specified object.</returns>
        public int GetHashCode(T obj) => getHashCodeDelegate(obj);
    }
}

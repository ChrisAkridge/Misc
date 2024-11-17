using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII.FileDistributions
{
	public struct UInt128WithExtraInterfaces(UInt128 value) :
		INumber<UInt128WithExtraInterfaces>,
		IAdditionOperators<UInt128WithExtraInterfaces, int, UInt128WithExtraInterfaces>,
		IDivisionOperators<UInt128WithExtraInterfaces, int, UInt128WithExtraInterfaces>,
		IMinMaxValue<UInt128WithExtraInterfaces>
	{
		private readonly UInt128 value = value;

		/// <summary>Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.</summary>
		/// <param name="obj">An object to compare with this instance.</param>
		/// <exception cref="T:System.ArgumentException">
		/// <paramref name="obj" /> is not the same type as this instance.</exception>
		/// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings:
		/// <list type="table"><listheader><term> Value</term><description> Meaning</description></listheader><item><term> Less than zero</term><description> This instance precedes <paramref name="obj" /> in the sort order.</description></item><item><term> Zero</term><description> This instance occurs in the same position in the sort order as <paramref name="obj" />.</description></item><item><term> Greater than zero</term><description> This instance follows <paramref name="obj" /> in the sort order.</description></item></list></returns>
		public int CompareTo(object obj) =>
			obj is UInt128WithExtraInterfaces other
				? CompareTo(other)
				: throw new ArgumentException("Object must be of type UInt128WithExtraInterfaces.", nameof(obj));

		/// <summary>Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.</summary>
		/// <param name="other">An object to compare with this instance.</param>
		/// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings:
		/// <list type="table"><listheader><term> Value</term><description> Meaning</description></listheader><item><term> Less than zero</term><description> This instance precedes <paramref name="other" /> in the sort order.</description></item><item><term> Zero</term><description> This instance occurs in the same position in the sort order as <paramref name="other" />.</description></item><item><term> Greater than zero</term><description> This instance follows <paramref name="other" /> in the sort order.</description></item></list></returns>
		public int CompareTo(UInt128WithExtraInterfaces other) => value.CompareTo(other.value);

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
		public bool Equals(UInt128WithExtraInterfaces other) => value == other.value;

		/// <summary>Formats the value of the current instance using the specified format.</summary>
		/// <param name="format">The format to use.
		/// -or-
		/// A null reference (<see langword="Nothing" /> in Visual Basic) to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation.</param>
		/// <param name="formatProvider">The provider to use to format the value.
		/// -or-
		/// A null reference (<see langword="Nothing" /> in Visual Basic) to obtain the numeric format information from the current locale setting of the operating system.</param>
		/// <returns>The value of the current instance in the specified format.</returns>
		public string ToString(string format, IFormatProvider formatProvider) => throw new NotImplementedException();

		/// <summary>Tries to format the value of the current instance into the provided span of characters.</summary>
		/// <param name="destination">The span in which to write this instance's value formatted as a span of characters.</param>
		/// <param name="charsWritten">When this method returns, contains the number of characters that were written in <paramref name="destination" />.</param>
		/// <param name="format">A span containing the characters that represent a standard or custom format string that defines the acceptable format for <paramref name="destination" />.</param>
		/// <param name="provider">An optional object that supplies culture-specific formatting information for <paramref name="destination" />.</param>
		/// <returns>
		/// <see langword="true" /> if the formatting was successful; otherwise, <see langword="false" />.</returns>
		public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider) => throw new NotImplementedException();

		/// <summary>Parses a string into a value.</summary>
		/// <param name="s">The string to parse.</param>
		/// <param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="s" /> is <see langword="null" />.</exception>
		/// <exception cref="T:System.FormatException">
		/// <paramref name="s" /> is not in the correct format.</exception>
		/// <exception cref="T:System.OverflowException">
		/// <paramref name="s" /> is not representable by <typeparamref name="TSelf" />.</exception>
		/// <returns>The result of parsing <paramref name="s" />.</returns>
		public static UInt128WithExtraInterfaces Parse(string s, IFormatProvider provider) => throw new NotImplementedException();

		/// <summary>Tries to parse a string into a value.</summary>
		/// <param name="s">The string to parse.</param>
		/// <param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
		/// <param name="result">When this method returns, contains the result of successfully parsing <paramref name="s" /> or an undefined value on failure.</param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="s" /> was successfully parsed; otherwise, <see langword="false" />.</returns>
		public static bool TryParse(string s, IFormatProvider provider, out UInt128WithExtraInterfaces result) => throw new NotImplementedException();

		/// <summary>Parses a span of characters into a value.</summary>
		/// <param name="s">The span of characters to parse.</param>
		/// <param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
		/// <exception cref="T:System.FormatException">
		/// <paramref name="s" /> is not in the correct format.</exception>
		/// <exception cref="T:System.OverflowException">
		/// <paramref name="s" /> is not representable by <typeparamref name="TSelf" />.</exception>
		/// <returns>The result of parsing <paramref name="s" />.</returns>
		public static UInt128WithExtraInterfaces Parse(ReadOnlySpan<char> s, IFormatProvider provider) => throw new NotImplementedException();

		/// <summary>Tries to parse a span of characters into a value.</summary>
		/// <param name="s">The span of characters to parse.</param>
		/// <param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
		/// <param name="result">When this method returns, contains the result of successfully parsing <paramref name="s" />, or an undefined value on failure.</param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="s" /> was successfully parsed; otherwise, <see langword="false" />.</returns>
		public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider provider, out UInt128WithExtraInterfaces result) => throw new NotImplementedException();

		/// <summary>Adds two values together to compute their sum.</summary>
		/// <param name="left">The value to which <paramref name="right" /> is added.</param>
		/// <param name="right">The value which is added to <paramref name="left" />.</param>
		/// <returns>The sum of <paramref name="left" /> and <paramref name="right" />.</returns>
		public static UInt128WithExtraInterfaces operator +(UInt128WithExtraInterfaces left,
			UInt128WithExtraInterfaces right) =>
			new UInt128WithExtraInterfaces(left.value + right.value);

		/// <summary>Gets the additive identity of the current type.</summary>
		public static UInt128WithExtraInterfaces AdditiveIdentity { get; }

		/// <summary>Compares two values to determine equality.</summary>
		/// <param name="left">The value to compare with <paramref name="right" />.</param>
		/// <param name="right">The value to compare with <paramref name="left" />.</param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="left" /> is equal to <paramref name="right" />; otherwise, <see langword="false" />.</returns>
		public static bool operator ==(UInt128WithExtraInterfaces left, UInt128WithExtraInterfaces right) => throw new NotImplementedException();

		/// <summary>Compares two values to determine inequality.</summary>
		/// <param name="left">The value to compare with <paramref name="right" />.</param>
		/// <param name="right">The value to compare with <paramref name="left" />.</param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="left" /> is not equal to <paramref name="right" />; otherwise, <see langword="false" />.</returns>
		public static bool operator !=(UInt128WithExtraInterfaces left, UInt128WithExtraInterfaces right) => throw new NotImplementedException();

		/// <summary>Compares two values to determine which is greater.</summary>
		/// <param name="left">The value to compare with <paramref name="right" />.</param>
		/// <param name="right">The value to compare with <paramref name="left" />.</param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="left" /> is greater than <paramref name="right" />; otherwise, <see langword="false" />.</returns>
		public static bool operator >(UInt128WithExtraInterfaces left, UInt128WithExtraInterfaces right) => throw new NotImplementedException();

		/// <summary>Compares two values to determine which is greater or equal.</summary>
		/// <param name="left">The value to compare with <paramref name="right" />.</param>
		/// <param name="right">The value to compare with <paramref name="left" />.</param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="left" /> is greater than or equal to <paramref name="right" />; otherwise, <see langword="false" />.</returns>
		public static bool operator >=(UInt128WithExtraInterfaces left, UInt128WithExtraInterfaces right) => throw new NotImplementedException();

		/// <summary>Compares two values to determine which is less.</summary>
		/// <param name="left">The value to compare with <paramref name="right" />.</param>
		/// <param name="right">The value to compare with <paramref name="left" />.</param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="left" /> is less than <paramref name="right" />; otherwise, <see langword="false" />.</returns>
		public static bool operator <(UInt128WithExtraInterfaces left, UInt128WithExtraInterfaces right) => throw new NotImplementedException();

		/// <summary>Compares two values to determine which is less or equal.</summary>
		/// <param name="left">The value to compare with <paramref name="right" />.</param>
		/// <param name="right">The value to compare with <paramref name="left" />.</param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="left" /> is less than or equal to <paramref name="right" />; otherwise, <see langword="false" />.</returns>
		public static bool operator <=(UInt128WithExtraInterfaces left, UInt128WithExtraInterfaces right) => throw new NotImplementedException();

		/// <summary>Decrements a value.</summary>
		/// <param name="value">The value to decrement.</param>
		/// <returns>The result of decrementing <paramref name="value" />.</returns>
		public static UInt128WithExtraInterfaces operator --(UInt128WithExtraInterfaces value) => throw new NotImplementedException();

		/// <summary>Divides one value by another to compute their quotient.</summary>
		/// <param name="left">The value which <paramref name="right" /> divides.</param>
		/// <param name="right">The value which divides <paramref name="left" />.</param>
		/// <returns>The quotient of <paramref name="left" /> divided by <paramref name="right" />.</returns>
		public static UInt128WithExtraInterfaces operator /(UInt128WithExtraInterfaces left, UInt128WithExtraInterfaces right) => throw new NotImplementedException();

		/// <summary>Increments a value.</summary>
		/// <param name="value">The value to increment.</param>
		/// <returns>The result of incrementing <paramref name="value" />.</returns>
		public static UInt128WithExtraInterfaces operator ++(UInt128WithExtraInterfaces value) => throw new NotImplementedException();

		/// <summary>Divides two values together to compute their modulus or remainder.</summary>
		/// <param name="left">The value which <paramref name="right" /> divides.</param>
		/// <param name="right">The value which divides <paramref name="left" />.</param>
		/// <returns>The modulus or remainder of <paramref name="left" /> divided by <paramref name="right" />.</returns>
		public static UInt128WithExtraInterfaces operator %(UInt128WithExtraInterfaces left, UInt128WithExtraInterfaces right) => throw new NotImplementedException();

		/// <summary>Gets the multiplicative identity of the current type.</summary>
		public static UInt128WithExtraInterfaces MultiplicativeIdentity { get; }

		/// <summary>Multiplies two values together to compute their product.</summary>
		/// <param name="left">The value which <paramref name="right" /> multiplies.</param>
		/// <param name="right">The value which multiplies <paramref name="left" />.</param>
		/// <returns>The product of <paramref name="left" /> multiplied by <paramref name="right" />.</returns>
		public static UInt128WithExtraInterfaces operator *(UInt128WithExtraInterfaces left, UInt128WithExtraInterfaces right) => throw new NotImplementedException();

		/// <summary>Subtracts two values to compute their difference.</summary>
		/// <param name="left">The value from which <paramref name="right" /> is subtracted.</param>
		/// <param name="right">The value which is subtracted from <paramref name="left" />.</param>
		/// <returns>The value of <paramref name="right" /> subtracted from <paramref name="left" />.</returns>
		public static UInt128WithExtraInterfaces operator -(UInt128WithExtraInterfaces left,
			UInt128WithExtraInterfaces right) =>
			new(left.value - right.value);

		/// <summary>Computes the unary negation of a value.</summary>
		/// <param name="value">The value for which to compute the unary negation.</param>
		/// <returns>The unary negation of <paramref name="value" />.</returns>
		public static UInt128WithExtraInterfaces operator -(UInt128WithExtraInterfaces value) => throw new NotImplementedException();

		/// <summary>Computes the unary plus of a value.</summary>
		/// <param name="value">The value for which to compute the unary plus.</param>
		/// <returns>The unary plus of <paramref name="value" />.</returns>
		public static UInt128WithExtraInterfaces operator +(UInt128WithExtraInterfaces value) => throw new NotImplementedException();

		/// <summary>Computes the absolute of a value.</summary>
		/// <param name="value">The value for which to get its absolute.</param>
		/// <exception cref="T:System.OverflowException">The absolute of <paramref name="value" /> is not representable by <typeparamref name="TSelf" />.</exception>
		/// <returns>The absolute of <paramref name="value" />.</returns>
		public static UInt128WithExtraInterfaces Abs(UInt128WithExtraInterfaces value) => throw new NotImplementedException();

		/// <summary>Determines if a value is in its canonical representation.</summary>
		/// <param name="value">The value to be checked.</param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="value" /> is in its canonical representation; otherwise, <see langword="false" />.</returns>
		public static bool IsCanonical(UInt128WithExtraInterfaces value) => throw new NotImplementedException();

		/// <summary>Determines if a value represents a complex number.</summary>
		/// <param name="value">The value to be checked.</param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="value" /> is a complex number; otherwise, <see langword="false" />.</returns>
		public static bool IsComplexNumber(UInt128WithExtraInterfaces value) => throw new NotImplementedException();

		/// <summary>Determines if a value represents an even integral number.</summary>
		/// <param name="value">The value to be checked.</param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="value" /> is an even integer; otherwise, <see langword="false" />.</returns>
		public static bool IsEvenInteger(UInt128WithExtraInterfaces value) => throw new NotImplementedException();

		/// <summary>Determines if a value is finite.</summary>
		/// <param name="value">The value to be checked.</param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="value" /> is finite; otherwise, <see langword="false" />.</returns>
		public static bool IsFinite(UInt128WithExtraInterfaces value) => throw new NotImplementedException();

		/// <summary>Determines if a value represents a pure imaginary number.</summary>
		/// <param name="value">The value to be checked.</param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="value" /> is a pure imaginary number; otherwise, <see langword="false" />.</returns>
		public static bool IsImaginaryNumber(UInt128WithExtraInterfaces value) => throw new NotImplementedException();

		/// <summary>Determines if a value is infinite.</summary>
		/// <param name="value">The value to be checked.</param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="value" /> is infinite; otherwise, <see langword="false" />.</returns>
		public static bool IsInfinity(UInt128WithExtraInterfaces value) => throw new NotImplementedException();

		/// <summary>Determines if a value represents an integral number.</summary>
		/// <param name="value">The value to be checked.</param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="value" /> is an integer; otherwise, <see langword="false" />.</returns>
		public static bool IsInteger(UInt128WithExtraInterfaces value) => throw new NotImplementedException();

		/// <summary>Determines if a value is NaN.</summary>
		/// <param name="value">The value to be checked.</param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="value" /> is NaN; otherwise, <see langword="false" />.</returns>
		public static bool IsNaN(UInt128WithExtraInterfaces value) => throw new NotImplementedException();

		/// <summary>Determines if a value represents a negative real number.</summary>
		/// <param name="value">The value to be checked.</param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="value" /> represents negative zero or a negative real number; otherwise, <see langword="false" />.</returns>
		public static bool IsNegative(UInt128WithExtraInterfaces value) => throw new NotImplementedException();

		/// <summary>Determines if a value is negative infinity.</summary>
		/// <param name="value">The value to be checked.</param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="value" /> is negative infinity; otherwise, <see langword="false" />.</returns>
		public static bool IsNegativeInfinity(UInt128WithExtraInterfaces value) => throw new NotImplementedException();

		/// <summary>Determines if a value is normal.</summary>
		/// <param name="value">The value to be checked.</param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="value" /> is normal; otherwise, <see langword="false" />.</returns>
		public static bool IsNormal(UInt128WithExtraInterfaces value) => throw new NotImplementedException();

		/// <summary>Determines if a value represents an odd integral number.</summary>
		/// <param name="value">The value to be checked.</param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="value" /> is an odd integer; otherwise, <see langword="false" />.</returns>
		public static bool IsOddInteger(UInt128WithExtraInterfaces value) => throw new NotImplementedException();

		/// <summary>Determines if a value represents zero or a positive real number.</summary>
		/// <param name="value">The value to be checked.</param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="value" /> represents (positive) zero or a positive real number; otherwise, <see langword="false" />.</returns>
		public static bool IsPositive(UInt128WithExtraInterfaces value) => throw new NotImplementedException();

		/// <summary>Determines if a value is positive infinity.</summary>
		/// <param name="value">The value to be checked.</param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="value" /> is positive infinity; otherwise, <see langword="false" />.</returns>
		public static bool IsPositiveInfinity(UInt128WithExtraInterfaces value) => throw new NotImplementedException();

		/// <summary>Determines if a value represents a real number.</summary>
		/// <param name="value">The value to be checked.</param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="value" /> is a real number; otherwise, <see langword="false" />.</returns>
		public static bool IsRealNumber(UInt128WithExtraInterfaces value) => throw new NotImplementedException();

		/// <summary>Determines if a value is subnormal.</summary>
		/// <param name="value">The value to be checked.</param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="value" /> is subnormal; otherwise, <see langword="false" />.</returns>
		public static bool IsSubnormal(UInt128WithExtraInterfaces value) => throw new NotImplementedException();

		/// <summary>Determines if a value is zero.</summary>
		/// <param name="value">The value to be checked.</param>
		/// <returns>
		/// <see langword="true" /> if <paramref name="value" /> is zero; otherwise, <see langword="false" />.</returns>
		public static bool IsZero(UInt128WithExtraInterfaces value) => throw new NotImplementedException();

		/// <summary>Compares two values to compute which is greater.</summary>
		/// <param name="x">The value to compare with <paramref name="y" />.</param>
		/// <param name="y">The value to compare with <paramref name="x" />.</param>
		/// <returns>
		/// <paramref name="x" /> if it is greater than <paramref name="y" />; otherwise, <paramref name="y" />.</returns>
		public static UInt128WithExtraInterfaces MaxMagnitude(UInt128WithExtraInterfaces x, UInt128WithExtraInterfaces y) => throw new NotImplementedException();

		/// <summary>Compares two values to compute which has the greater magnitude and returning the other value if an input is <c>NaN</c>.</summary>
		/// <param name="x">The value to compare with <paramref name="y" />.</param>
		/// <param name="y">The value to compare with <paramref name="x" />.</param>
		/// <returns>
		/// <paramref name="x" /> if it is greater than <paramref name="y" />; otherwise, <paramref name="y" />.</returns>
		public static UInt128WithExtraInterfaces MaxMagnitudeNumber(UInt128WithExtraInterfaces x, UInt128WithExtraInterfaces y) => throw new NotImplementedException();

		/// <summary>Compares two values to compute which is lesser.</summary>
		/// <param name="x">The value to compare with <paramref name="y" />.</param>
		/// <param name="y">The value to compare with <paramref name="x" />.</param>
		/// <returns>
		/// <paramref name="x" /> if it is less than <paramref name="y" />; otherwise, <paramref name="y" />.</returns>
		public static UInt128WithExtraInterfaces MinMagnitude(UInt128WithExtraInterfaces x, UInt128WithExtraInterfaces y) => throw new NotImplementedException();

		/// <summary>Compares two values to compute which has the lesser magnitude and returning the other value if an input is <c>NaN</c>.</summary>
		/// <param name="x">The value to compare with <paramref name="y" />.</param>
		/// <param name="y">The value to compare with <paramref name="x" />.</param>
		/// <returns>
		/// <paramref name="x" /> if it is less than <paramref name="y" />; otherwise, <paramref name="y" />.</returns>
		public static UInt128WithExtraInterfaces MinMagnitudeNumber(UInt128WithExtraInterfaces x, UInt128WithExtraInterfaces y) => throw new NotImplementedException();

		/// <summary>Parses a span of characters into a value.</summary>
		/// <param name="s">The span of characters to parse.</param>
		/// <param name="style">A bitwise combination of number styles that can be present in <paramref name="s" />.</param>
		/// <param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
		/// <exception cref="T:System.ArgumentException">
		/// <paramref name="style" /> is not a supported <see cref="T:System.Globalization.NumberStyles" /> value.</exception>
		/// <exception cref="T:System.FormatException">
		/// <paramref name="s" /> is not in the correct format.</exception>
		/// <exception cref="T:System.OverflowException">
		/// <paramref name="s" /> is not representable by <typeparamref name="TSelf" />.</exception>
		/// <returns>The result of parsing <paramref name="s" />.</returns>
		public static UInt128WithExtraInterfaces Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider provider) => throw new NotImplementedException();

		/// <summary>Parses a string into a value.</summary>
		/// <param name="s">The string to parse.</param>
		/// <param name="style">A bitwise combination of number styles that can be present in <paramref name="s" />.</param>
		/// <param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
		/// <exception cref="T:System.ArgumentException">
		/// <paramref name="style" /> is not a supported <see cref="T:System.Globalization.NumberStyles" /> value.</exception>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="s" /> is <see langword="null" />.</exception>
		/// <exception cref="T:System.FormatException">
		/// <paramref name="s" /> is not in the correct format.</exception>
		/// <exception cref="T:System.OverflowException">
		/// <paramref name="s" /> is not representable by <typeparamref name="TSelf" />.</exception>
		/// <returns>The result of parsing <paramref name="s" />.</returns>
		public static UInt128WithExtraInterfaces Parse(string s, NumberStyles style, IFormatProvider provider) => throw new NotImplementedException();

		/// <summary>Tries to convert a value to an instance of the the current type, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
		/// <param name="value">The value that's used to create the instance of <typeparamref name="TSelf" />.</param>
		/// <param name="result">When this method returns, contains an instance of <typeparamref name="TSelf" /> converted from <paramref name="value" />.</param>
		/// <typeparam name="TOther">The type of <paramref name="value" />.</typeparam>
		/// <exception cref="T:System.OverflowException">
		/// <paramref name="value" /> is not representable by <typeparamref name="TSelf" />.</exception>
		/// <returns>
		/// <see langword="false" /> if <typeparamref name="TOther" /> is not supported; otherwise, <see langword="true" />.</returns>
		public static bool TryConvertFromChecked<TOther>(TOther value, out UInt128WithExtraInterfaces result) where TOther : INumberBase<TOther> => throw new NotImplementedException();

		/// <summary>Tries to convert a value to an instance of the the current type, saturating any values that fall outside the representable range of the current type.</summary>
		/// <param name="value">The value which is used to create the instance of <typeparamref name="TSelf" />.</param>
		/// <param name="result">When this method returns, contains an instance of <typeparamref name="TSelf" /> converted from <paramref name="value" />.</param>
		/// <typeparam name="TOther">The type of <paramref name="value" />.</typeparam>
		/// <returns>
		/// <see langword="false" /> if <typeparamref name="TOther" /> is not supported; otherwise, <see langword="true" />.</returns>
		public static bool TryConvertFromSaturating<TOther>(TOther value, out UInt128WithExtraInterfaces result) where TOther : INumberBase<TOther> => throw new NotImplementedException();

		/// <summary>Tries to convert a value to an instance of the the current type, truncating any values that fall outside the representable range of the current type.</summary>
		/// <param name="value">The value which is used to create the instance of <typeparamref name="TSelf" />.</param>
		/// <param name="result">When this method returns, contains an instance of <typeparamref name="TSelf" /> converted from <paramref name="value" />.</param>
		/// <typeparam name="TOther">The type of <paramref name="value" />.</typeparam>
		/// <returns>
		/// <see langword="false" /> if <typeparamref name="TOther" /> is not supported; otherwise, <see langword="true" />.</returns>
		public static bool TryConvertFromTruncating<TOther>(TOther value, out UInt128WithExtraInterfaces result) where TOther : INumberBase<TOther> => throw new NotImplementedException();

		/// <summary>Tries to convert an instance of the the current type to another type, throwing an overflow exception for any values that fall outside the representable range of the current type.</summary>
		/// <param name="value">The value which is used to create the instance of <typeparamref name="TOther" />.</param>
		/// <param name="result">When this method returns, contains an instance of <typeparamref name="TOther" /> converted from <paramref name="value" />.</param>
		/// <typeparam name="TOther">The type to which <paramref name="value" /> should be converted.</typeparam>
		/// <exception cref="T:System.OverflowException">
		/// <paramref name="value" /> is not representable by <typeparamref name="TOther" />.</exception>
		/// <returns>
		/// <see langword="false" /> if <typeparamref name="TOther" /> is not supported; otherwise, <see langword="true" />.</returns>
		public static bool TryConvertToChecked<TOther>(UInt128WithExtraInterfaces value, out TOther result) where TOther : INumberBase<TOther> => throw new NotImplementedException();

		/// <summary>Tries to convert an instance of the the current type to another type, saturating any values that fall outside the representable range of the current type.</summary>
		/// <param name="value">The value which is used to create the instance of <typeparamref name="TOther" />.</param>
		/// <param name="result">When this method returns, contains an instance of <typeparamref name="TOther" /> converted from <paramref name="value" />.</param>
		/// <typeparam name="TOther">The type to which <paramref name="value" /> should be converted.</typeparam>
		/// <returns>
		/// <see langword="false" /> if <typeparamref name="TOther" /> is not supported; otherwise, <see langword="true" />.</returns>
		public static bool TryConvertToSaturating<TOther>(UInt128WithExtraInterfaces value, out TOther result) where TOther : INumberBase<TOther> => throw new NotImplementedException();

		/// <summary>Tries to convert an instance of the the current type to another type, truncating any values that fall outside the representable range of the current type.</summary>
		/// <param name="value">The value which is used to create the instance of <typeparamref name="TOther" />.</param>
		/// <param name="result">When this method returns, contains an instance of <typeparamref name="TOther" /> converted from <paramref name="value" />.</param>
		/// <typeparam name="TOther">The type to which <paramref name="value" /> should be converted.</typeparam>
		/// <returns>
		/// <see langword="false" /> if <typeparamref name="TOther" /> is not supported; otherwise, <see langword="true" />.</returns>
		public static bool TryConvertToTruncating<TOther>(UInt128WithExtraInterfaces value, out TOther result) where TOther : INumberBase<TOther> => throw new NotImplementedException();

		/// <summary>Tries to parse a span of characters into a value.</summary>
		/// <param name="s">The span of characters to parse.</param>
		/// <param name="style">A bitwise combination of number styles that can be present in <paramref name="s" />.</param>
		/// <param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
		/// <param name="result">On return, contains the result of succesfully parsing <paramref name="s" /> or an undefined value on failure.</param>
		/// <exception cref="T:System.ArgumentException">
		/// <paramref name="style" /> is not a supported <see cref="T:System.Globalization.NumberStyles" /> value.</exception>
		/// <returns>
		/// <see langword="true" /> if <paramref name="s" /> was successfully parsed; otherwise, <see langword="false" />.</returns>
		public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider provider,
			out UInt128WithExtraInterfaces result) =>
			throw new NotImplementedException();

		/// <summary>Tries to parse a string into a value.</summary>
		/// <param name="s">The string to parse.</param>
		/// <param name="style">A bitwise combination of number styles that can be present in <paramref name="s" />.</param>
		/// <param name="provider">An object that provides culture-specific formatting information about <paramref name="s" />.</param>
		/// <param name="result">On return, contains the result of succesfully parsing <paramref name="s" /> or an undefined value on failure.</param>
		/// <exception cref="T:System.ArgumentException">
		/// <paramref name="style" /> is not a supported <see cref="T:System.Globalization.NumberStyles" /> value.</exception>
		/// <returns>
		/// <see langword="true" /> if <paramref name="s" /> was successfully parsed; otherwise, <see langword="false" />.</returns>
		public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out UInt128WithExtraInterfaces result) => throw new NotImplementedException();

		/// <summary>Gets the value <c>1</c> for the type.</summary>
		public static UInt128WithExtraInterfaces One { get; }

		/// <summary>Gets the radix, or base, for the type.</summary>
		public static int Radix { get; }

		/// <summary>Gets the value <c>0</c> for the type.</summary>
		public static UInt128WithExtraInterfaces Zero { get; }

		/// <summary>Adds two values together to compute their sum.</summary>
		/// <param name="left">The value to which <paramref name="right" /> is added.</param>
		/// <param name="right">The value which is added to <paramref name="left" />.</param>
		/// <returns>The sum of <paramref name="left" /> and <paramref name="right" />.</returns>
		public static UInt128WithExtraInterfaces operator +(UInt128WithExtraInterfaces left, int right) => new(left.value + (UInt128)right);

		/// <summary>Divides one value by another to compute their quotient.</summary>
		/// <param name="left">The value which <paramref name="right" /> divides.</param>
		/// <param name="right">The value which divides <paramref name="left" />.</param>
		/// <returns>The quotient of <paramref name="left" /> divided by <paramref name="right" />.</returns>
		public static UInt128WithExtraInterfaces operator /(UInt128WithExtraInterfaces left, int right) =>
			new(left.value / (UInt128)right);

		/// <summary>Gets the maximum value of the current type.</summary>
		public static UInt128WithExtraInterfaces MaxValue => new(UInt128.MaxValue);

		/// <summary>Gets the minimum value of the current type.</summary>
		public static UInt128WithExtraInterfaces MinValue => new(UInt128.MinValue);
	}
}

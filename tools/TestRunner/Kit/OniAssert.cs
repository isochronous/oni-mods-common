using System;
using System.Collections.Generic;

namespace Isochronous.TestKit
{
	/// <summary>Assertions; each throws OniAssertException with the message and both values.</summary>
	public sealed class OniAssert
	{
		public void Fail(string message)
		{
			throw new OniAssertException(message);
		}

		public void True(bool condition, string message)
		{
			if (!condition)
				throw new OniAssertException(message + " (expected true)");
		}

		public void False(bool condition, string message)
		{
			if (condition)
				throw new OniAssertException(message + " (expected false)");
		}

		public void Eq<T>(T expected, T actual, string message)
		{
			if (!EqualityComparer<T>.Default.Equals(expected, actual))
				throw new OniAssertException(message + " (expected " + Show(expected) + ", got " + Show(actual) + ")");
		}

		public void NotEq<T>(T unexpected, T actual, string message)
		{
			if (EqualityComparer<T>.Default.Equals(unexpected, actual))
				throw new OniAssertException(message + " (got " + Show(actual) + ", which it must not be)");
		}

		public void Null(object value, string message)
		{
			if (!IsNull(value))
				throw new OniAssertException(message + " (expected null, got " + Show(value) + ")");
		}

		public void NotNull(object value, string message)
		{
			if (IsNull(value))
				throw new OniAssertException(message + " (was null)");
		}

		/// <summary>The connection bits include the flag.</summary>
		public void Has(UtilityConnections flag, UtilityConnections value, string message)
		{
			if ((value & flag) != flag)
				throw new OniAssertException(message + " (expected " + flag + " in " + (value == 0 ? "none" : value.ToString()) + ")");
		}

		/// <summary>The connection bits do not include the flag.</summary>
		public void Lacks(UtilityConnections flag, UtilityConnections value, string message)
		{
			if ((value & flag) != 0)
				throw new OniAssertException(message + " (" + flag + " present in " + value + ")");
		}

		/// <summary>Unity objects compare equal to null once destroyed; this honours that.</summary>
		private static bool IsNull(object value)
		{
			return value == null || (value is UnityEngine.Object o && o == null);
		}

		private static string Show(object value)
		{
			if (value == null)
				return "null";
			if (value is UnityEngine.Color c)
				return "Color(" + c.r + ", " + c.g + ", " + c.b + ", " + c.a + ")";
			return value.ToString();
		}
	}
}

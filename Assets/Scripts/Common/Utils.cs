using System;

namespace Common
{
	public static class Utils
	{
		private const string FieldMustBeNullText = "field must be null.";
		private const string FieldMustNotBeNullText = "field must not be null.";

		public static void AssertIsNull(object field)
		{
			if (!IsNull(field))
				throw new InvalidOperationException(FieldMustBeNullText);
		}

		public static void AssertIsNotNull(object field)
		{
			if (IsNull(field))
				throw new InvalidOperationException(FieldMustNotBeNullText);
		}

		private static bool IsNull(object field)
		{
			if (field is UnityEngine.Object unityObject)
				return unityObject == null;

			return field == null;
		}
	}
}

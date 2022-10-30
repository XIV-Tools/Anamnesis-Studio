// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;

public static class Float
{
	public static bool IsValid(float? number)
	{
		if (number == null)
			return false;

		float v = (float)number;

		if (float.IsInfinity(v))
			return false;

		if (float.IsNaN(v))
			return false;

		return true;
	}

	public static float Wrap(float number, float min = 0, float max = 360)
	{
		float range = max - min;

		if (number >= max)
			number -= range;

		if (number < min)
			number += range;

		return number;
	}

	public static bool IsApproximately(float a, float b, float errorMargin = 0.001f)
	{
		float d = MathF.Abs(a - b);
		return d < errorMargin;
	}
}

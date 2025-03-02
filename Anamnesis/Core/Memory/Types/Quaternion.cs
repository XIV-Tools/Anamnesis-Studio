﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Memory;

using System;
using System.Globalization;

public struct Quaternion : IEquatable<Quaternion>
{
	public static readonly Quaternion Identity = new Quaternion(0.0f, 0.0f, 0.0f, 1.0f);

	private static readonly float Deg2Rad = ((float)Math.PI * 2) / 360;
	private static readonly float Rad2Deg = 360 / ((float)Math.PI * 2);

	public Quaternion(float x, float y, float z, float w)
	{
		this.X = x;
		this.Y = y;
		this.Z = z;
		this.W = w;
	}

	public Quaternion(Quaternion other)
	{
		this.X = other.X;
		this.Y = other.Y;
		this.Z = other.Z;
		this.W = other.W;
	}

	public float X { get; set; }
	public float Y { get; set; }
	public float Z { get; set; }
	public float W { get; set; }

	public static bool operator !=(Quaternion lhs, Quaternion rhs)
	{
		return !lhs.Equals(rhs);
	}

	public static bool operator ==(Quaternion lhs, Quaternion rhs)
	{
		return lhs.Equals(rhs);
	}

	public static Quaternion operator *(Quaternion left, Quaternion right)
	{
		float x = (left.X * right.X) - (left.Y * right.Y) - (left.Z * right.Z) - (left.W * right.W);
		float y = (left.X * right.Y) + (left.Y * right.X) + (left.Z * right.W) - (left.W * right.Z);
		float z = (left.X * right.Z) - (left.Y * right.W) + (left.Z * right.X) + (left.W * right.Y);
		float w = (left.X * right.W) + (left.Y * right.Z) - (left.Z * right.Y) + (left.W * right.X);
		Quaternion q = new Quaternion(x, y, z, w);
		return q.Normalize();
	}

	public static Vector operator *(Quaternion left, Vector right)
	{
		float num = left.X * 2f;
		float num2 = left.Y * 2f;
		float num3 = left.Z * 2f;
		float num4 = left.X * num;
		float num5 = left.Y * num2;
		float num6 = left.Z * num3;
		float num7 = left.X * num2;
		float num8 = left.X * num3;
		float num9 = left.Y * num3;
		float num10 = left.W * num;
		float num11 = left.W * num2;
		float num12 = left.W * num3;
		float x = ((1f - (num5 + num6)) * right.X) + ((num7 - num12) * right.Y) + ((num8 + num11) * right.Z);
		float y = ((num7 + num12) * right.X) + ((1f - (num4 + num6)) * right.Y) + ((num9 - num10) * right.Z);
		float z = ((num8 - num11) * right.X) + ((num9 + num10) * right.Y) + ((1f - (num4 + num5)) * right.Z);
		return new Vector(x, y, z);
	}

	public static Quaternion FromString(string str)
	{
		string[] parts = str.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

		if (parts.Length != 4)
			throw new FormatException();

		Quaternion v = default;
		v.X = float.Parse(parts[0], CultureInfo.InvariantCulture);
		v.Y = float.Parse(parts[1], CultureInfo.InvariantCulture);
		v.Z = float.Parse(parts[2], CultureInfo.InvariantCulture);
		v.W = float.Parse(parts[3], CultureInfo.InvariantCulture);
		return v;
	}

	/// <summary>
	/// Convert into a Quaternion assuming the Vector represents euler angles.
	/// </summary>
	/// <returns>Quaternion from Euler angles.</returns>
	public static Quaternion FromEuler(Vector euler)
	{
		return Quaternion.FromEuler(euler.X, euler.Y, euler.Z);
	}

	public static Quaternion FromEuler(float pitch_x, float yaw_y, float roll_z)
	{
		double yaw = yaw_y * Deg2Rad;
		double pitch = pitch_x * Deg2Rad;
		double roll = roll_z * Deg2Rad;

		double c1 = Math.Cos(yaw / 2);
		double s1 = Math.Sin(yaw / 2);
		double c2 = Math.Cos(pitch / 2);
		double s2 = Math.Sin(pitch / 2);
		double c3 = Math.Cos(roll / 2);
		double s3 = Math.Sin(roll / 2);

		double c1c2 = c1 * c2;
		double s1s2 = s1 * s2;

		double x = (c1c2 * s3) + (s1s2 * c3);
		double y = (s1 * c2 * c3) + (c1 * s2 * s3);
		double z = (c1 * s2 * c3) - (s1 * c2 * s3);
		double w = (c1c2 * c3) - (s1s2 * s3);

		return new Quaternion((float)x, (float)y, (float)z, (float)w);
	}

	public bool IsApproximately(Quaternion other, float errorMargin = 0.001f)
	{
		return Float.IsApproximately(this.X, other.X, errorMargin)
			&& Float.IsApproximately(this.Y, other.Y, errorMargin)
			&& Float.IsApproximately(this.Z, other.Z, errorMargin)
			&& Float.IsApproximately(this.W, other.W, errorMargin);
	}

	public bool IsValid()
	{
		bool valid = Float.IsValid(this.X);
		valid &= Float.IsValid(this.Y);
		valid &= Float.IsValid(this.Z);
		valid &= Float.IsValid(this.W);

		return valid;
	}

	public Vector ToEuler()
	{
		Vector v = default;

		double test = (this.X * this.Y) + (this.Z * this.W);

		if (test > 0.4995f)
		{
			v.Y = 2f * (float)Math.Atan2(this.X, this.Y);
			v.X = (float)Math.PI / 2;
			v.Z = 0;
		}
		else if (test < -0.4995f)
		{
			v.Y = -2f * (float)Math.Atan2(this.X, this.W);
			v.X = -(float)Math.PI / 2;
			v.Z = 0;
		}
		else
		{
			double sqx = this.X * this.X;
			double sqy = this.Y * this.Y;
			double sqz = this.Z * this.Z;

			v.Y = (float)Math.Atan2((2 * this.Y * this.W) - (2 * this.X * this.Z), 1 - (2 * sqy) - (2 * sqz));
			v.X = (float)Math.Asin(2 * test);
			v.Z = (float)Math.Atan2((2 * this.X * this.W) - (2 * this.Y * this.Z), 1 - (2 * sqx) - (2 * sqz));
		}

		v *= Rad2Deg;
		v.NormalizeAngles();

		return v;
	}

	public Quaternion Normalize()
	{
		Quaternion q = new(this);
		float num = (q.X * q.X) + (q.Y * q.Y) + (q.Z * q.Z) + (q.W * q.W);
		if (num > float.MaxValue)
		{
			float num2 = 1.0f / Max(Math.Abs(q.X), Math.Abs(q.Y), Math.Abs(q.Z), Math.Abs(q.W));
			q.X *= num2;
			q.Y *= num2;
			q.Z *= num2;
			q.W *= num2;
			num = (q.X * q.X) + (q.Y * q.Y) + (q.Z * q.Z) + (q.W * q.W);
		}

		float num3 = 1.0f / (float)Math.Sqrt(num);
		q.X *= num3;
		q.Y *= num3;
		q.Z *= num3;
		q.W *= num3;

		return q;
	}

	public Quaternion Mirror()
	{
		return new Quaternion(this.Z, this.W, this.X, this.Y);
	}

	public override bool Equals(object? obj)
	{
		return obj is Quaternion quaternion && this.Equals(quaternion);
	}

	public bool Equals(Quaternion other)
	{
		return this.X == other.X
			&& this.Y == other.Y
			&& this.Z == other.Z
			&& this.W == other.W;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(this.X, this.Y, this.Z, this.W);
	}

	public Quaternion Conjugate()
	{
		return new Quaternion(-this.X, -this.Y, -this.Z, this.W);
	}

	public Quaternion Invert()
	{
		return new Quaternion(this.X, -this.Y, -this.Z, -this.W);
	}

	public override string ToString()
	{
		return this.X.ToString(CultureInfo.InvariantCulture) + ", "
			+ this.Y.ToString(CultureInfo.InvariantCulture) + ", "
			+ this.Z.ToString(CultureInfo.InvariantCulture) + ", "
			+ this.W.ToString(CultureInfo.InvariantCulture);
	}

	private static float Max(float a, float b, float c, float d)
	{
		if (b > a)
			a = b;

		if (c > a)
			a = c;

		if (d > a)
			a = d;

		return a;
	}
}

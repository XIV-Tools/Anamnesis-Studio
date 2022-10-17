// © Anamnesis.
// Licensed under the MIT license.

// Ported from Media3D matrix:
// https://github.com/dotnet/wpf/blob/main/src/Microsoft.DotNet.Wpf/src/PresentationCore/System/Windows/Media3D/Matrix.cs

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Description: 3D matrix implementation.
//
//              See spec at http://avalon/medialayer/Specifications/Avalon3D%20API%20Spec.mht
//
// NOTE:
//
// Structs do not have default constructors and all struct variables are initialized to
// zero on construction. For this reason, we cannot simply initialize member variables
// to identity. So we use an auxiliary variable _isNotKnownToBeIdentity. This variable
// will default to false when the struct is initialized, meaning that the matrix is not
// notIdentity (i.e. it is an identity matrix).
//
// All methods that read the _mXX fields on the diagonal (_m11, _m22, _m33, _m44) need
// to be use the IsDistinguishedIdentity property to special case this (which frequently
// turns out to be a nice optimization).
namespace Anamnesis.Core;

using Anamnesis.Memory;
using System;
using System.Diagnostics;

#pragma warning disable

/// <summary>
/// 3D Matrix.
/// The matrix is represented in the following row-vector syntax form:
///
/// [ m11      m12      m13      m14 ]
/// [ m21      m22      m23      m24 ]
/// [ m31      m32      m33      m34 ]
/// [ offsetX  offsetY  offsetZ  m44 ]
///
/// Note that since the fourth column is also accessible, the matrix allows one to
/// represent affine as well as non-affine transforms.
/// Matrices can be appended or prepended to other matrices. Appending A to B denotes
/// a transformation by B and then by A - i.e. A(B(...)), whereas prepending A to B denotes a
/// transformation by A and then by B - i.e. B(A(...)). Thus for example if we want to
/// transform point P by A and then by B, we append B to A:
/// C = A.Append(B)
/// P' = C.Transform(P).
/// </summary>
public partial struct Matrix
{
	private float m11, m21, m31;
	private float m12, m22, m32;
	private float m13, m23, m33;
	private float m14, m24, m34, m44;

	private float offsetX;
	private float offsetY;
	private float offsetZ;

	// Internal matrix representation
	private bool isNotKnownToBeIdentity;

	// The hash code for a matrix is the xor of its element's hashes.
	// Since the identity matrix has 4 1's and 12 0's its hash is 0.
	private const int identityHashCode = 0;

	/// <summary>
	/// Initializes a new instance of the <see cref="Matrix"/> struct.
	/// Constructor that sets matrix's initial values.
	/// </summary>
	public Matrix(float m11, float m12, float m13, float m14,
					float m21, float m22, float m23, float m24,
					float m31, float m32, float m33, float m34,
					float offsetX, float offsetY, float offsetZ, float m44)
	{
		this.m11 = m11;
		this.m12 = m12;
		this.m13 = m13;
		this.m14 = m14;
		this.m21 = m21;
		this.m22 = m22;
		this.m23 = m23;
		this.m24 = m24;
		this.m31 = m31;
		this.m32 = m32;
		this.m33 = m33;
		this.m34 = m34;
		this.offsetX = offsetX;
		this.offsetY = offsetY;
		this.offsetZ = offsetZ;
		this.m44 = m44;

		// This is not known to be an identity matrix so we need
		// to change our flag from it's default value.  We use the field
		// in the ctor rather than the property because of CS0188.
		this.isNotKnownToBeIdentity = true;
	}

	public static readonly Matrix Identity = new Matrix(1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1)
	{
		IsDistinguishedIdentity = true,
	};

	public bool IsAffine => (this.IsDistinguishedIdentity || (this.m14 == 0.0 && this.m24 == 0.0 && this.m34 == 0.0 && this.m44 == 1.0));
	public bool HasInverse => this.Determinant != 0;

	/// <summary>
	/// Sets matrix to identity.
	/// </summary>
	public void SetIdentity()
	{
		this = Matrix.Identity;
	}

	/// <summary>
	/// Returns whether the matrix is identity.
	/// </summary>
	public bool IsIdentity
	{
		get
		{
			if (this.IsDistinguishedIdentity)
			{
				return true;
			}
			else
			{
				// Otherwise check all elements one by one.
				if (this.m11 == 1.0 && this.m12 == 0.0 && this.m13 == 0.0 && this.m14 == 0.0 &&
					this.m21 == 0.0 && this.m22 == 1.0 && this.m23 == 0.0 && this.m24 == 0.0 &&
					this.m31 == 0.0 && this.m32 == 0.0 && this.m33 == 1.0 && this.m34 == 0.0 &&
					this.offsetX == 0.0 && this.offsetY == 0.0 && this.offsetZ == 0.0 && this.m44 == 1.0)
				{
					// If matrix is identity, cache this with the IsDistinguishedIdentity flag.
					this.IsDistinguishedIdentity = true;
					return true;
				}
				else
				{
					return false;
				}
			}
		}
	}

	/// <summary>
	/// Prepends the given matrix to the current matrix.
	/// </summary>
	/// <param name="matrix">Matrix to prepend.</param>
	public void Prepend(Matrix matrix)
	{
		this = matrix * this;
	}

	/// <summary>
	/// Appends the given matrix to the current matrix.
	/// </summary>
	/// <param name="matrix">Matrix to append.</param>
	public void Append(Matrix matrix)
	{
		this *= matrix;
	}

	/// <summary>
	/// Appends rotation transform to the current matrix.
	/// </summary>
	/// <param name="quaternion">Quaternion representing rotation.</param>
	public void Rotate(Quaternion quaternion)
	{
		Vector center = new Vector();

		this *= CreateRotationMatrix(ref quaternion, ref center);
	}

	/// <summary>
	/// Prepends rotation transform to the current matrix.
	/// </summary>
	/// <param name="quaternion">Quaternion representing rotation.</param>
	public void RotatePrepend(Quaternion quaternion)
	{
		Vector center = new Vector();

		this = CreateRotationMatrix(ref quaternion, ref center) * this;
	}

	/// <summary>
	/// Appends rotation transform to the current matrix.
	/// </summary>
	/// <param name="quaternion">Quaternion representing rotation.</param>
	/// <param name="center">Center to rotate around.</param>
	public void RotateAt(Quaternion quaternion, Vector center)
	{
		this *= CreateRotationMatrix(ref quaternion, ref center);
	}

	/// <summary>
	/// Prepends rotation transform to the current matrix.
	/// </summary>
	/// <param name="quaternion">Quaternion representing rotation.</param>
	/// <param name="center">Center to rotate around.</param>
	public void RotateAtPrepend(Quaternion quaternion, Vector center)
	{
		this = CreateRotationMatrix(ref quaternion, ref center) * this;
	}

	/// <summary>
	/// Appends scale transform to the current matrix.
	/// </summary>
	/// <param name="scale">Scaling vector for transformation.</param>
	public void Scale(Vector scale)
	{
		if (this.IsDistinguishedIdentity)
		{
			this.SetScaleMatrix(ref scale);
		}
		else
		{
			this.m11 *= scale.X; this.m12 *= scale.Y; this.m13 *= scale.Z;
			this.m21 *= scale.X; this.m22 *= scale.Y; this.m23 *= scale.Z;
			this.m31 *= scale.X; this.m32 *= scale.Y; this.m33 *= scale.Z;
			this.offsetX *= scale.X; this.offsetY *= scale.Y; this.offsetZ *= scale.Z;
		}
	}

	/// <summary>
	/// Prepends scale transform to the current matrix.
	/// </summary>
	/// <param name="scale">Scaling vector for transformation.</param>
	public void ScalePrepend(Vector scale)
	{
		if (this.IsDistinguishedIdentity)
		{
			this.SetScaleMatrix(ref scale);
		}
		else
		{
			this.m11 *= scale.X; this.m12 *= scale.X; this.m13 *= scale.X; this.m14 *= scale.X;
			this.m21 *= scale.Y; this.m22 *= scale.Y; this.m23 *= scale.Y; this.m24 *= scale.Y;
			this.m31 *= scale.Z; this.m32 *= scale.Z; this.m33 *= scale.Z; this.m34 *= scale.Z;
		}
	}

	/// <summary>
	/// Appends scale transform to the current matrix.
	/// </summary>
	/// <param name="scale">Scaling vector for transformation.</param>
	/// <param name="center">Point around which to scale.</param>
	public void ScaleAt(Vector scale, Vector center)
	{
		if (this.IsDistinguishedIdentity)
		{
			this.SetScaleMatrix(ref scale, ref center);
		}
		else
		{
			float tmp = this.m14 * center.X;
			this.m11 = tmp + scale.X * (this.m11 - tmp);
			tmp = this.m14 * center.Y;
			this.m12 = tmp + scale.Y * (this.m12 - tmp);
			tmp = this.m14 * center.Z;
			this.m13 = tmp + scale.Z * (this.m13 - tmp);

			tmp = this.m24 * center.X;
			this.m21 = tmp + scale.X * (this.m21 - tmp);
			tmp = this.m24 * center.Y;
			this.m22 = tmp + scale.Y * (this.m22 - tmp);
			tmp = this.m24 * center.Z;
			this.m23 = tmp + scale.Z * (this.m23 - tmp);

			tmp = this.m34 * center.X;
			this.m31 = tmp + scale.X * (this.m31 - tmp);
			tmp = this.m34 * center.Y;
			this.m32 = tmp + scale.Y * (this.m32 - tmp);
			tmp = this.m34 * center.Z;
			this.m33 = tmp + scale.Z * (this.m33 - tmp);

			tmp = this.m44 * center.X;
			this.offsetX = tmp + scale.X * (this.offsetX - tmp);
			tmp = this.m44 * center.Y;
			this.offsetY = tmp + scale.Y * (this.offsetY - tmp);
			tmp = this.m44 * center.Z;
			this.offsetZ = tmp + scale.Z * (this.offsetZ - tmp);
		}
	}

	/// <summary>
	/// Prepends scale transform to the current matrix.
	/// </summary>
	/// <param name="scale">Scaling vector for transformation.</param>
	/// <param name="center">Point around which to scale.</param>
	public void ScaleAtPrepend(Vector scale, Vector center)
	{
		if (this.IsDistinguishedIdentity)
		{
			this.SetScaleMatrix(ref scale, ref center);
		}
		else
		{
			float csx = center.X - center.X * scale.X;
			float csy = center.Y - center.Y * scale.Y;
			float csz = center.Z - center.Z * scale.Z;

			// We have to set the bottom row first because it depends
			// on values that will change
			this.offsetX += this.m11 * csx + this.m21 * csy + this.m31 * csz;
			this.offsetY += this.m12 * csx + this.m22 * csy + this.m32 * csz;
			this.offsetZ += this.m13 * csx + this.m23 * csy + this.m33 * csz;
			this.m44 += this.m14 * csx + this.m24 * csy + this.m34 * csz;

			this.m11 *= scale.X; this.m12 *= scale.X; this.m13 *= scale.X; this.m14 *= scale.X;
			this.m21 *= scale.Y; this.m22 *= scale.Y; this.m23 *= scale.Y; this.m24 *= scale.Y;
			this.m31 *= scale.Z; this.m32 *= scale.Z; this.m33 *= scale.Z; this.m34 *= scale.Z;
		}
	}

	/// <summary>
	/// Appends translation transform to the current matrix.
	/// </summary>
	/// <param name="offset">Offset vector for transformation.</param>
	public void Translate(Vector offset)
	{
		if (this.IsDistinguishedIdentity)
		{
			this.SetTranslationMatrix(ref offset);
		}
		else
		{
			this.m11 += this.m14 * offset.X; this.m12 += this.m14 * offset.Y; this.m13 += this.m14 * offset.Z;
			this.m21 += this.m24 * offset.X; this.m22 += this.m24 * offset.Y; this.m23 += this.m24 * offset.Z;
			this.m31 += this.m34 * offset.X; this.m32 += this.m34 * offset.Y; this.m33 += this.m34 * offset.Z;
			this.offsetX += this.m44 * offset.X; this.offsetY += this.m44 * offset.Y; this.offsetZ += this.m44 * offset.Z;
		}
	}

	/// <summary>
	/// Prepends translation transform to the current matrix.
	/// </summary>
	/// <param name="offset">Offset vector for transformation.</param>
	public void TranslatePrepend(Vector offset)
	{
		if (this.IsDistinguishedIdentity)
		{
			this.SetTranslationMatrix(ref offset);
		}
		else
		{
			this.offsetX += this.m11 * offset.X + this.m21 * offset.Y + this.m31 * offset.Z;
			this.offsetY += this.m12 * offset.X + this.m22 * offset.Y + this.m32 * offset.Z;
			this.offsetZ += this.m13 * offset.X + this.m23 * offset.Y + this.m33 * offset.Z;
			this.m44 += this.m14 * offset.X + this.m24 * offset.Y + this.m34 * offset.Z;
		}
	}

	/// <summary>
	/// Matrix multiplication.
	/// </summary>
	/// <param name="matrix1">Matrix to multiply.</param>
	/// <param name="matrix2">Matrix by which the first matrix is multiplied.</param>
	/// <returns>Result of multiplication.</returns>
	public static Matrix operator *(Matrix matrix1, Matrix matrix2)
	{
		// Check if multiplying by identity.
		if (matrix1.IsDistinguishedIdentity)
			return matrix2;
		if (matrix2.IsDistinguishedIdentity)
			return matrix1;

		// Regular 4x4 matrix multiplication.
		Matrix result = new Matrix(
			matrix1.m11 * matrix2.m11 + matrix1.m12 * matrix2.m21 +
			matrix1.m13 * matrix2.m31 + matrix1.m14 * matrix2.offsetX,
			matrix1.m11 * matrix2.m12 + matrix1.m12 * matrix2.m22 +
			matrix1.m13 * matrix2.m32 + matrix1.m14 * matrix2.offsetY,
			matrix1.m11 * matrix2.m13 + matrix1.m12 * matrix2.m23 +
			matrix1.m13 * matrix2.m33 + matrix1.m14 * matrix2.offsetZ,
			matrix1.m11 * matrix2.m14 + matrix1.m12 * matrix2.m24 +
			matrix1.m13 * matrix2.m34 + matrix1.m14 * matrix2.m44,
			matrix1.m21 * matrix2.m11 + matrix1.m22 * matrix2.m21 +
			matrix1.m23 * matrix2.m31 + matrix1.m24 * matrix2.offsetX,
			matrix1.m21 * matrix2.m12 + matrix1.m22 * matrix2.m22 +
			matrix1.m23 * matrix2.m32 + matrix1.m24 * matrix2.offsetY,
			matrix1.m21 * matrix2.m13 + matrix1.m22 * matrix2.m23 +
			matrix1.m23 * matrix2.m33 + matrix1.m24 * matrix2.offsetZ,
			matrix1.m21 * matrix2.m14 + matrix1.m22 * matrix2.m24 +
			matrix1.m23 * matrix2.m34 + matrix1.m24 * matrix2.m44,
			matrix1.m31 * matrix2.m11 + matrix1.m32 * matrix2.m21 +
			matrix1.m33 * matrix2.m31 + matrix1.m34 * matrix2.offsetX,
			matrix1.m31 * matrix2.m12 + matrix1.m32 * matrix2.m22 +
			matrix1.m33 * matrix2.m32 + matrix1.m34 * matrix2.offsetY,
			matrix1.m31 * matrix2.m13 + matrix1.m32 * matrix2.m23 +
			matrix1.m33 * matrix2.m33 + matrix1.m34 * matrix2.offsetZ,
			matrix1.m31 * matrix2.m14 + matrix1.m32 * matrix2.m24 +
			matrix1.m33 * matrix2.m34 + matrix1.m34 * matrix2.m44,
			matrix1.offsetX * matrix2.m11 + matrix1.offsetY * matrix2.m21 +
			matrix1.offsetZ * matrix2.m31 + matrix1.m44 * matrix2.offsetX,
			matrix1.offsetX * matrix2.m12 + matrix1.offsetY * matrix2.m22 +
			matrix1.offsetZ * matrix2.m32 + matrix1.m44 * matrix2.offsetY,
			matrix1.offsetX * matrix2.m13 + matrix1.offsetY * matrix2.m23 +
			matrix1.offsetZ * matrix2.m33 + matrix1.m44 * matrix2.offsetZ,
			matrix1.offsetX * matrix2.m14 + matrix1.offsetY * matrix2.m24 +
			matrix1.offsetZ * matrix2.m34 + matrix1.m44 * matrix2.m44);

		return result;
	}

	/// <summary>
	/// Matrix multiplication.
	/// </summary>
	/// <param name="matrix1">Matrix to multiply.</param>
	/// <param name="matrix2">Matrix by which the first matrix is multiplied.</param>
	/// <returns>Result of multiplication.</returns>
	public static Matrix Multiply(Matrix matrix1, Matrix matrix2)
	{
		return (matrix1 * matrix2);
	}

	/// <summary>
	///  Transforms the given Vector by this matrix, projecting the
	///  result back into the W=1 plane.
	/// </summary>
	/// <param name="point">Point to transform.</param>
	/// <returns>Transformed point.</returns>
	public Vector TransformPoint(Vector point)
	{
		this.MultiplyPoint(ref point);
		return point;
	}

	/// <summary>
	/// Transforms the given vector by the current matrix.
	/// </summary>
	/// <param name="vector">Vector to transform.</param>
	/// <returns>Transformed vector.</returns>
	public Vector TransformVector(Vector vector)
	{
		this.MultiplyVector(ref vector);
		return vector;
	}
	

	/// <summary>
	/// Matrix determinant.
	/// </summary>
	public float Determinant
	{
		get
		{
			if (this.IsDistinguishedIdentity)
				return 1.0f;

			if (this.IsAffine)
				return this.GetNormalizedAffineDeterminant();

			// NOTE: The beginning of this code is duplicated between
			//       the Invert method and the Determinant property.

			// compute all six 2x2 determinants of 2nd two columns
			float y01 = this.m13 * this.m24 - this.m23 * this.m14;
			float y02 = this.m13 * this.m34 - this.m33 * this.m14;
			float y03 = this.m13 * this.m44 - this.offsetZ * this.m14;
			float y12 = this.m23 * this.m34 - this.m33 * this.m24;
			float y13 = this.m23 * this.m44 - this.offsetZ * this.m24;
			float y23 = this.m33 * this.m44 - this.offsetZ * this.m34;

			// Compute 3x3 cofactors for 1st the column
			float z30 = this.m22 * y02 - this.m32 * y01 - this.m12 * y12;
			float z20 = this.m12 * y13 - this.m22 * y03 + this.offsetY * y01;
			float z10 = this.m32 * y03 - this.offsetY * y02 - this.m12 * y23;
			float z00 = this.m22 * y23 - this.m32 * y13 + this.offsetY * y12;

			return this.offsetX * z30 + this.m31 * z20 + this.m21 * z10 + this.m11 * z00;
		}
	}

	/// <summary>
	///     Computes, and substitutes in-place, the inverse of a matrix.
	///     The determinant of the matrix must be nonzero, otherwise the matrix is not invertible.
	///     In this case it will throw InvalidOperationException exception.
	/// </summary>
	/// <exception cref="InvalidOperationException">
	///     This will throw InvalidOperationException if the matrix is not invertible.
	/// </exception>
	public void Invert()
	{
		if (!this.InvertCore())
		{
			throw new InvalidOperationException("Matrix_NotInvertible");
		}
	}

	internal void SetScaleMatrix(ref Vector scale)
	{
		Debug.Assert(this.IsDistinguishedIdentity);

		this.m11 = scale.X;
		this.m22 = scale.Y;
		this.m33 = scale.Z;
		this.m44 = 1.0f;

		this.IsDistinguishedIdentity = false;
	}

	internal void SetScaleMatrix(ref Vector scale, ref Vector center)
	{
		Debug.Assert(this.IsDistinguishedIdentity);

		this.m11 = scale.X;
		this.m22 = scale.Y;
		this.m33 = scale.Z;
		this.m44 = 1.0f;

		this.offsetX = center.X - center.X * scale.X;
		this.offsetY = center.Y - center.Y * scale.Y;
		this.offsetZ = center.Z - center.Z * scale.Z;

		this.IsDistinguishedIdentity = false;
	}

	internal void SetTranslationMatrix(ref Vector offset)
	{
		Debug.Assert(this.IsDistinguishedIdentity);

		this.m11 = this.m22 = this.m33 = this.m44 = 1.0f;

		this.offsetX = offset.X;
		this.offsetY = offset.Y;
		this.offsetZ = offset.Z;

		this.IsDistinguishedIdentity = false;
	}

	//  Creates a rotation matrix given a quaternion and center.
	//
	//  Quaternion and center are passed by reference for performance
	//  only and are not modified.
	//
	internal static Matrix CreateRotationMatrix(ref Quaternion quaternion, ref Vector center)
	{
		Matrix matrix = Matrix.Identity;
		matrix.IsDistinguishedIdentity = false; // Will be using direct member access
		float wx, wy, wz, xx, yy, yz, xy, xz, zz, x2, y2, z2;

		x2 = quaternion.X + quaternion.X;
		y2 = quaternion.Y + quaternion.Y;
		z2 = quaternion.Z + quaternion.Z;
		xx = quaternion.X * x2;
		xy = quaternion.X * y2;
		xz = quaternion.X * z2;
		yy = quaternion.Y * y2;
		yz = quaternion.Y * z2;
		zz = quaternion.Z * z2;
		wx = quaternion.W * x2;
		wy = quaternion.W * y2;
		wz = quaternion.W * z2;

		matrix.m11 = 1.0f - (yy + zz);
		matrix.m12 = xy + wz;
		matrix.m13 = xz - wy;
		matrix.m21 = xy - wz;
		matrix.m22 = 1.0f - (xx + zz);
		matrix.m23 = yz + wx;
		matrix.m31 = xz + wy;
		matrix.m32 = yz - wx;
		matrix.m33 = 1.0f - (xx + yy);

		if (center.X != 0 || center.Y != 0 || center.Z != 0)
		{
			matrix.offsetX = -center.X * matrix.m11 - center.Y * matrix.m21 - center.Z * matrix.m31 + center.X;
			matrix.offsetY = -center.X * matrix.m12 - center.Y * matrix.m22 - center.Z * matrix.m32 + center.Y;
			matrix.offsetZ = -center.X * matrix.m13 - center.Y * matrix.m23 - center.Z * matrix.m33 + center.Z;
		}

		return matrix;
	}

	//  Multiplies the given Vector by this matrix, projecting the
	//  result back into the W=1 plane.
	//
	//  The point is modified in place for performance.
	//
	internal void MultiplyPoint(ref Vector point)
	{
		if (this.IsDistinguishedIdentity)
			return;

		float x = point.X;
		float y = point.Y;
		float z = point.Z;

		point.X = x * this.m11 + y * this.m21 + z * this.m31 + this.offsetX;
		point.Y = x * this.m12 + y * this.m22 + z * this.m32 + this.offsetY;
		point.Z = x * this.m13 + y * this.m23 + z * this.m33 + this.offsetZ;

		if (!this.IsAffine)
		{
			float w = x * this.m14 + y * this.m24 + z * this.m34 + this.m44;

			point.X /= w;
			point.Y /= w;
			point.Z /= w;
		}
	}

	//  Multiplies the given Vector by this matrix.
	//
	//  The vector is modified in place for performance.
	//
	internal void MultiplyVector(ref Vector vector)
	{
		if (this.IsDistinguishedIdentity)
			return;

		float x = vector.X;
		float y = vector.Y;
		float z = vector.Z;

		// Do not apply _offset to vectors.
		vector.X = x * this.m11 + y * this.m21 + z * this.m31;
		vector.Y = x * this.m12 + y * this.m22 + z * this.m32;
		vector.Z = x * this.m13 + y * this.m23 + z * this.m33;
	}

	//  Computes the determinant of the matrix assuming that it's
	//  fourth column is 0,0,0,1 and it isn't identity
	internal float GetNormalizedAffineDeterminant()
	{
		Debug.Assert(!this.IsDistinguishedIdentity);
		Debug.Assert(this.IsAffine);

		// NOTE: The beginning of this code is duplicated between
		//       GetNormalizedAffineDeterminant() and NormalizedAffineInvert()

		float z20 = this.m12 * this.m23 - this.m22 * this.m13;
		float z10 = this.m32 * this.m13 - this.m12 * this.m33;
		float z00 = this.m22 * this.m33 - this.m32 * this.m23;

		return this.m31 * z20 + this.m21 * z10 + this.m11 * z00;
	}

	// Assuming this matrix has fourth column of 0,0,0,1 and isn't identity this function:
	// Returns false if HasInverse is false, otherwise inverts the matrix.
	internal bool NormalizedAffineInvert()
	{
		Debug.Assert(!this.IsDistinguishedIdentity);
		Debug.Assert(this.IsAffine);

		// NOTE: The beginning of this code is duplicated between
		//       GetNormalizedAffineDeterminant() and NormalizedAffineInvert()

		float z20 = this.m12 * this.m23 - this.m22 * this.m13;
		float z10 = this.m32 * this.m13 - this.m12 * this.m33;
		float z00 = this.m22 * this.m33 - this.m32 * this.m23;
		float det = this.m31 * z20 + this.m21 * z10 + this.m11 * z00;

		// Fancy logic here avoids using equality with possible nan values.
		Debug.Assert(!(det < this.Determinant || det > this.Determinant), "Matrix.Inverse: Determinant property does not match value computed in Inverse.");

		if (det == 0)
			return false;

		// Compute 3x3 non-zero cofactors for the 2nd column
		float z21 = this.m21 * this.m13 - this.m11 * this.m23;
		float z11 = this.m11 * this.m33 - this.m31 * this.m13;
		float z01 = this.m31 * this.m23 - this.m21 * this.m33;

		// Compute all six 2x2 determinants of 1st two columns
		float y01 = this.m11 * this.m22 - this.m21 * this.m12;
		float y02 = this.m11 * this.m32 - this.m31 * this.m12;
		float y03 = this.m11 * this.offsetY - this.offsetX * this.m12;
		float y12 = this.m21 * this.m32 - this.m31 * this.m22;
		float y13 = this.m21 * this.offsetY - this.offsetX * this.m22;
		float y23 = this.m31 * this.offsetY - this.offsetX * this.m32;

		// Compute all non-zero and non-one 3x3 cofactors for 2nd
		// two columns
		float z23 = this.m23 * y03 - this.offsetZ * y01 - this.m13 * y13;
		float z13 = this.m13 * y23 - this.m33 * y03 + this.offsetZ * y02;
		float z03 = this.m33 * y13 - this.offsetZ * y12 - this.m23 * y23;
		float z22 = y01;
		float z12 = -y02;
		float z02 = y12;

		float rcp = 1.0f / det;

		// Multiply all 3x3 cofactors by reciprocal & transpose
		this.m11 = z00 * rcp;
		this.m12 = z10 * rcp;
		this.m13 = z20 * rcp;

		this.m21 = z01 * rcp;
		this.m22 = z11 * rcp;
		this.m23 = z21 * rcp;

		this.m31 = z02 * rcp;
		this.m32 = z12 * rcp;
		this.m33 = z22 * rcp;

		this.offsetX = z03 * rcp;
		this.offsetY = z13 * rcp;
		this.offsetZ = z23 * rcp;

		return true;
	}

	// RETURNS true if has inverse & invert was done.  Otherwise returns false & leaves matrix unchanged.
	internal bool InvertCore()
	{
		if (this.IsDistinguishedIdentity)
			return true;

		if (this.IsAffine)
			return this.NormalizedAffineInvert();

		// NOTE: The beginning of this code is duplicated between
		//       the Invert method and the Determinant property.

		// compute all six 2x2 determinants of 2nd two columns
		float y01 = this.m13 * this.m24 - this.m23 * this.m14;
		float y02 = this.m13 * this.m34 - this.m33 * this.m14;
		float y03 = this.m13 * this.m44 - this.offsetZ * this.m14;
		float y12 = this.m23 * this.m34 - this.m33 * this.m24;
		float y13 = this.m23 * this.m44 - this.offsetZ * this.m24;
		float y23 = this.m33 * this.m44 - this.offsetZ * this.m34;

		// Compute 3x3 cofactors for 1st the column
		float z30 = this.m22 * y02 - this.m32 * y01 - this.m12 * y12;
		float z20 = this.m12 * y13 - this.m22 * y03 + this.offsetY * y01;
		float z10 = this.m32 * y03 - this.offsetY * y02 - this.m12 * y23;
		float z00 = this.m22 * y23 - this.m32 * y13 + this.offsetY * y12;

		// Compute 4x4 determinant
		float det = this.offsetX * z30 + this.m31 * z20 + this.m21 * z10 + this.m11 * z00;

		// If Determinant is computed using a different method then Inverse can throw
		// NotInvertable when HasInverse is true.  (Windows OS #901174)
		//
		// The strange logic below is equivalent to "det == Determinant", but NaN safe.
		Debug.Assert(!(det < this.Determinant || det > this.Determinant),
			"Matrix.Inverse: Determinant property does not match value computed in Inverse.");

		if (det == 0)
			return false;

		// Compute 3x3 cofactors for the 2nd column
		float z31 = this.m11 * y12 - this.m21 * y02 + this.m31 * y01;
		float z21 = this.m21 * y03 - this.offsetX * y01 - this.m11 * y13;
		float z11 = this.m11 * y23 - this.m31 * y03 + this.offsetX * y02;
		float z01 = this.m31 * y13 - this.offsetX * y12 - this.m21 * y23;

		// Compute all six 2x2 determinants of 1st two columns
		y01 = this.m11 * this.m22 - this.m21 * this.m12;
		y02 = this.m11 * this.m32 - this.m31 * this.m12;
		y03 = this.m11 * this.offsetY - this.offsetX * this.m12;
		y12 = this.m21 * this.m32 - this.m31 * this.m22;
		y13 = this.m21 * this.offsetY - this.offsetX * this.m22;
		y23 = this.m31 * this.offsetY - this.offsetX * this.m32;

		// Compute all 3x3 cofactors for 2nd two columns
		float z33 = this.m13 * y12 - this.m23 * y02 + this.m33 * y01;
		float z23 = this.m23 * y03 - this.offsetZ * y01 - this.m13 * y13;
		float z13 = this.m13 * y23 - this.m33 * y03 + this.offsetZ * y02;
		float z03 = this.m33 * y13 - this.offsetZ * y12 - this.m23 * y23;
		float z32 = this.m24 * y02 - this.m34 * y01 - this.m14 * y12;
		float z22 = this.m14 * y13 - this.m24 * y03 + this.m44 * y01;
		float z12 = this.m34 * y03 - this.m44 * y02 - this.m14 * y23;
		float z02 = this.m24 * y23 - this.m34 * y13 + this.m44 * y12;

		float rcp = 1.0f / det;

		// Multiply all 3x3 cofactors by reciprocal & transpose
		this.m11 = z00 * rcp;
		this.m12 = z10 * rcp;
		this.m13 = z20 * rcp;
		this.m14 = z30 * rcp;

		this.m21 = z01 * rcp;
		this.m22 = z11 * rcp;
		this.m23 = z21 * rcp;
		this.m24 = z31 * rcp;

		this.m31 = z02 * rcp;
		this.m32 = z12 * rcp;
		this.m33 = z22 * rcp;
		this.m34 = z32 * rcp;

		this.offsetX = z03 * rcp;
		this.offsetY = z13 * rcp;
		this.offsetZ = z23 * rcp;
		this.m44 = z33 * rcp;

		return true;
	}

	// Returns true if this matrix is guaranteed to be the identity matrix.
	// This is true when a new matrix has been created or after the Identity
	// has already been computed.
	//
	// NOTE: In the case of a new matrix, the _m* fields on the diagonal
	// will be uninitialized.  You should either use the properties which interpret
	// this state and return 1.0 or you can frequently early exit with a known
	// value for the identity matrix.
	//
	// NOTE: This property being false does not mean that the matrix is
	// not the identity matrix, it means that we do not know for certain if
	// it is the identity matrix.  Use the Identity property if you need to
	// know if the matrix is the identity matrix.  (The result will be cached
	// and this property will start returning true.)
	//
	private bool IsDistinguishedIdentity
	{
		get
		{
			Debug.Assert(
				this.isNotKnownToBeIdentity
					|| (
						(this.m11 == 0.0 || this.m11 == 1.0) && (this.m12 == 0.0) && (this.m13 == 0.0) && (this.m14 == 0.0) &&
						(this.m21 == 0.0) && (this.m22 == 0.0 || this.m22 == 1.0) && (this.m23 == 0.0) && (this.m24 == 0.0) &&
						(this.m31 == 0.0) && (this.m32 == 0.0) && (this.m33 == 0.0 || this.m33 == 1.0) && (this.m34 == 0.0) &&
						(this.offsetX == 0.0) && (this.offsetY == 0.0) && (this.offsetZ == 0.0) && (this.m44 == 0.0 || this.m44 == 1.0)),
				"Matrix.IsDistinguishedIdentity - _isNotKnownToBeIdentity flag is inconsistent with matrix state.");

			return !this.isNotKnownToBeIdentity;
		}

		set
		{
			this.isNotKnownToBeIdentity = !value;

			// This not only verifies we got the inversion right, but we also hit the
			// the assert above which verifies the value matches the state of the matrix.
			Debug.Assert(this.IsDistinguishedIdentity == value,
				"Matrix.IsDistinguishedIdentity - Error detected setting IsDistinguishedIdentity.");
		}
	}
}

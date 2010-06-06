// <copyright file="Vector.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
// Copyright (c) 2009-2010 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics.LinearAlgebra.Double
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using Distributions;
    using Properties;
    using Threading;

    /// <summary>
    /// Defines the base class for <c>Vector</c> classes.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public abstract class Vector :
#if SILVERLIGHT
    IFormattable, IEnumerable<double>, IEquatable<Vector>
#else
        IFormattable, IEnumerable<double>, IEquatable<Vector>, ICloneable
#endif
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Vector"/> class. 
        /// Constructs a <strong>Vector</strong> with the given size.
        /// </summary>
        /// <param name="size">
        /// The size of the <strong>Vector</strong> to construct.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="size"/> is less than one.
        /// </exception>
        protected Vector(int size)
        {
            if (size < 1)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "size");
            }

            this.Count = size;
        }

        /// <summary>
        /// Gets he number of elements in the vector.
        /// </summary>
        public int Count
        {
            get;
            private set;
        }

        /// <summary>Gets or sets the value at the given <paramref name="index"/>.</summary>
        /// <param name="index">The index of the value to get or set.</param>
        /// <returns>The value of the vector at the given <paramref name="index"/>.</returns> 
        /// <exception cref="IndexOutOfRangeException">If <paramref name="index"/> is negative or 
        /// greater than the size of the vector.</exception>
        public abstract double this[int index]
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a matrix with the given dimensions using the same storage type
        /// as this vector.
        /// </summary>
        /// <param name="rows">
        /// The number of rows.
        /// </param>
        /// <param name="columns">
        /// The number of columns.
        /// </param>
        /// <returns>
        /// A matrix with the given dimensions.
        /// </returns>
        public abstract Matrix CreateMatrix(int rows, int columns);

        /// <summary>
        /// Creates a <strong>Vector</strong> of the given size using the same storage type
        /// as this vector.
        /// </summary>
        /// <param name="size">
        /// The size of the <strong>Vector</strong> to create.
        /// </param>
        /// <returns>
        /// The new <c>Vector</c>.
        /// </returns>
        public abstract Vector CreateVector(int size);

        #region Elementary operations

        /// <summary>
        /// Adds a scalar to each element of the vector.
        /// </summary>
        /// <param name="scalar">
        /// The scalar to add.
        /// </param>
        public virtual void Add(double scalar)
        {
            if (scalar == 0.0)
            {
                return;
            }

            CommonParallel.For(
                0, 
                this.Count, 
                index => this[index] += scalar);
        }

        /// <summary>
        /// Adds a scalar to each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">
        /// The scalar to add.
        /// </param>
        /// <param name="result">
        /// The vector to store the result of the addition.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If the result vector is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this vector and <paramref name="result"/> are not the same size.
        /// </exception>
        public virtual void Add(double scalar, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (this.Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            if (!ReferenceEquals(this, result))
            {
                this.CopyTo(result);
            }

            result.Add(scalar);
        }

        /// <summary>
        /// Returns this vector.
        /// </summary>
        /// <returns>
        /// This vector.
        /// </returns>
        /// <remarks>
        /// Added as an alternative to the unary addition operator.
        /// </remarks>
        public virtual Vector Plus()
        {
            return this;
        }

        /// <summary>
        /// Adds another vector to this vector.
        /// </summary>
        /// <param name="other">
        /// The vector to add to this one.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If the other vector is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this vector and <paramref name="other"/> are not the same size.
        /// </exception>
        public virtual void Add(Vector other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (this.Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            CommonParallel.For(
                0, 
                this.Count, 
                index => this[index] += other[index]);
        }

        /// <summary>
        /// Adds another vector to this vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">
        /// The vector to add to this one.
        /// </param>
        /// <param name="result">
        /// The vector to store the result of the addition.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If the other vector is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If the result vector is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this vector and <paramref name="other"/> are not the same size.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this vector and <paramref name="result"/> are not the same size.
        /// </exception>
        public virtual void Add(Vector other, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (this.Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            if (ReferenceEquals(this, result) || ReferenceEquals(other, result))
            {
                var tmp = result.CreateVector(result.Count);
                Add(other, tmp);
                tmp.CopyTo(result);
            }
            else
            {
                this.CopyTo(result);
                result.Add(other);
            }
        }

        /// <summary>
        /// Subtracts a scalar from each element of the vector.
        /// </summary>
        /// <param name="scalar">
        /// The scalar to subtract.
        /// </param>
        public virtual void Subtract(double scalar)
        {
            if (scalar == 0.0)
            {
                return;
            }

            CommonParallel.For(
                0, 
                this.Count, 
                index => this[index] -= scalar);
        }

        /// <summary>
        /// Subtracts a scalar from each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">
        /// The scalar to subtract.
        /// </param>
        /// <param name="result">
        /// The vector to store the result of the subtraction.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If the result vector is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this vector and <paramref name="result"/> are not the same size.
        /// </exception>
        public virtual void Subtract(double scalar, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (this.Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            if (!ReferenceEquals(this, result))
            {
                this.CopyTo(result);
            }

            result.Subtract(scalar);
        }

        /// <summary>
        /// Returns a negated vector.
        /// </summary>
        /// <returns>
        /// The negated vector.
        /// </returns>
        /// <remarks>
        /// Added as an alternative to the unary negation operator.
        /// </remarks>
        public virtual Vector Negate()
        {
            return this * -1;
        }

        /// <summary>
        /// Subtracts another vector from this vector.
        /// </summary>
        /// <param name="other">
        /// The vector to subtract from this one.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If the other vector is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this vector and <paramref name="other"/> are not the same size.
        /// </exception>
        public virtual void Subtract(Vector other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (this.Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            CommonParallel.For(
                0, 
                this.Count, 
                index => this[index] -= other[index]);
        }

        /// <summary>
        /// Subtracts another vector to this vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">
        /// The vector to subtract from this one.
        /// </param>
        /// <param name="result">
        /// The vector to store the result of the subtraction.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If the other vector is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If the result vector is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this vector and <paramref name="other"/> are not the same size.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this vector and <paramref name="result"/> are not the same size.
        /// </exception>
        public virtual void Subtract(Vector other, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (this.Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            if (ReferenceEquals(this, result) || ReferenceEquals(other, result))
            {
                var tmp = result.CreateVector(result.Count);
                Subtract(other, tmp);
                tmp.CopyTo(result);
            }
            else
            {
                this.CopyTo(result);
                result.Subtract(other);
            }
        }

        /// <summary>
        /// Multiplies a scalar to each element of the vector.
        /// </summary>
        /// <param name="scalar">
        /// The scalar to multiply.
        /// </param>
        public virtual void Multiply(double scalar)
        {
            if (scalar == 1.0)
            {
                return;
            }

            CommonParallel.For(
                0, 
                this.Count, 
                index => this[index] *= scalar);
        }

        /// <summary>
        /// Multiplies a scalar to each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">
        /// The scalar to multiply.
        /// </param>
        /// <param name="result">
        /// The vector to store the result of the multiplication.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If the result vector is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this vector and <paramref name="result"/> are not the same size.
        /// </exception>
        public virtual void Multiply(double scalar, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (this.Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            if (!ReferenceEquals(this, result))
            {
                this.CopyTo(result);
            }

            result.Multiply(scalar);
        }

        /// <summary>
        /// Computes the dot product between this vector and another vector.
        /// </summary>
        /// <param name="other">
        /// The other vector to add.
        /// </param>
        /// <returns>
        /// The result of the addition.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// If <paramref name="other"/> is not of the same size.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="other"/> is <see langword="null"/>.
        /// </exception>
        public virtual double DotProduct(Vector other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (this.Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            var dot = 0.0;
            for (var i = 0; i < this.Count; i++)
            {
                dot += this[i] * other[i];
            }

            return dot;
        }

        /// <summary>
        /// Divides each element of the vector by a scalar.
        /// </summary>
        /// <param name="scalar">
        /// The scalar to divide with.
        /// </param>
        public virtual void Divide(double scalar)
        {
            if (scalar == 1.0)
            {
                return;
            }

            this.Multiply(1.0 / scalar);
        }

        /// <summary>
        /// Divides each element of the vector by a scalar and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">
        /// The scalar to divide with.
        /// </param>
        /// <param name="result">
        /// The vector to store the result of the division.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If the result vector is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If this vector and <paramref name="result"/> are not the same size.
        /// </exception>
        public virtual void Divide(double scalar, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (this.Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            if (!ReferenceEquals(this, result))
            {
                this.CopyTo(result);
            }

            result.Multiply(1.0 / scalar);
        }

        /// <summary>
        /// Pointwise multiplies this vector with another vector.
        /// </summary>
        /// <param name="other">The vector to pointwise multiply with this one.</param>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        /// <returns>A new vector that is the pointwise multiplication of this vector and <paramref name="other"/>.</returns>
        public virtual Vector PointWiseMultiply(Vector other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (this.Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            var result = this.CreateVector(this.Count);
            this.PointWiseMultiply(other, result);
            return result;
        }

        /// <summary>
        /// Pointwise multiplies this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to pointwise multiply with this one.</param>
        /// <param name="result">The vector to store the result of the pointwise multiplication.</param>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentNullException">If the result vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public virtual void PointWiseMultiply(Vector other, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (this.Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            if (this.Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            CommonParallel.For(
                0, 
                this.Count, 
                index => result[index] = this[index] * other[index]);
        }

        /// <summary>
        /// Pointwise add this vector with another vector.
        /// </summary>
        /// <param name="other">The vector to pointwise add with this one.</param>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        /// <returns>A new vector that is the pointwise addition of this vector and <paramref name="other"/>.</returns>
        public virtual Vector PointWiseAdd(Vector other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (this.Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            var result = this.CreateVector(this.Count);
            this.PointWiseAdd(other, result);
            return result;
        }

        /// <summary>
        /// Pointwise add this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to pointwise add with this one.</param>
        /// <param name="result">The vector to store the result of the pointwise addition.</param>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentNullException">If the result vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public virtual void PointWiseAdd(Vector other, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (this.Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            if (this.Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            CommonParallel.For(
                0, 
                this.Count, 
                index => result[index] = this[index] + other[index]);
        }

        /// <summary>
        /// Pointwise subtarct this vector with another vector.
        /// </summary>
        /// <param name="other">The vector to pointwise subtract from this one.</param>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        /// <returns>A new vector that is the pointwise subtraction of this vector and <paramref name="other"/>.</returns>
        public virtual Vector PointWiseSubtract(Vector other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (this.Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            var result = this.CreateVector(this.Count);
            this.PointWiseSubtract(other, result);
            return result;
        }

        /// <summary>
        /// Pointwise subtract this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to pointwise subtract from this one.</param>
        /// <param name="result">The vector to store the result of the pointwise subtraction.</param>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentNullException">If the result vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public virtual void PointWiseSubtract(Vector other, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (this.Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            if (this.Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            CommonParallel.For(
                0, 
                this.Count, 
                index => result[index] = this[index] - other[index]);
        }

        /// <summary>
        /// Pointwise divide this vector with another vector.
        /// </summary>
        /// <param name="other">The vector to pointwise divide this one by.</param>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        /// <returns>A new vector that is the pointwise division of this vector and <paramref name="other"/>.</returns>
        public virtual Vector PointWiseDivide(Vector other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (this.Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            var result = this.CreateVector(this.Count);
            this.PointWiseDivide(other, result);
            return result;
        }

        /// <summary>
        /// Pointwise divide this vector with another vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to pointwise divide this one by.</param>
        /// <param name="result">The vector to store the result of the pointwise division.</param>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentNullException">If the result vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public virtual void PointWiseDivide(Vector other, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (this.Count != other.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "other");
            }

            if (this.Count != result.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "result");
            }

            CommonParallel.For(
                0, 
                this.Count, 
                index => result[index] = this[index] / other[index]);
        }

        /// <summary>
        /// Dyadic product of two vectors
        /// </summary>
        /// <param name="u">First vector</param>
        /// <param name="v">Second vector</param>
        /// <returns>Matrix M[i,j] = u[i]*v[j] </returns>
        /// <exception cref="ArgumentNullException">If the u vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentNullException">If the v vector is <see langword="null" />.</exception> 
        public static DenseMatrix DyadicProduct(Vector u, Vector v)
        {
            if (u == null)
            {
                throw new ArgumentNullException("u");
            }

            if (v == null)
            {
                throw new ArgumentNullException("v");
            }

            var matrix = new DenseMatrix(u.Count, v.Count);
            CommonParallel.For(
                0, 
                u.Count, 
                i => CommonParallel.For(0, v.Count, j => matrix.At(i, j, u[i] * v[j])));
            return matrix;
        }

        /// <summary>
        /// Generates a vector with random elements
        /// </summary>
        /// <param name="length">Number of elements in the vector.</param>
        /// <param name="randomDistribution">Continuous Random Distribution or Source</param>
        /// <returns>
        /// A vector with n-random elements distributed according
        /// to the specified random distribution.
        /// </returns>
        /// <exception cref="ArgumentNullException">If the n vector is non poisitive<see langword="null" />.</exception> 
        public virtual Vector Random(int length, IContinuousDistribution randomDistribution)
        {
            if (length < 0)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "length");
            }

            var v = this.CreateVector(length);
            for (var index = 0; index < v.Count; index++)
            {
                v[index] = randomDistribution.Sample();
            }

            return v;
        }

        /// <summary>
        /// Generates a vector with random elements
        /// </summary>
        /// <param name="length">Number of elements in the vector.</param>
        /// <param name="randomDistribution">Continuous Random Distribution or Source</param>
        /// <returns>
        /// A vector with n-random elements distributed according
        /// to the specified random distribution.
        /// </returns>
        /// <exception cref="ArgumentNullException">If the n vector is non poisitive<see langword="null" />.</exception> 
        public virtual Vector Random(int length, IDiscreteDistribution randomDistribution)
        {
            if (length < 0)
            {
                throw new ArgumentException(Resources.ArgumentMustBePositive, "length");
            }

            var v = this.CreateVector(length);
            for (var index = 0; index < v.Count; index++)
            {
                v[index] = randomDistribution.Sample();
            }

            return v;
        }

        /// <summary>
        /// Tensor Product (Dyadic) of this and another vector.
        /// </summary>
        /// <param name="v">The vector to operate on.</param>
        /// <returns>
        /// Matrix M[i,j] = this[i] * v[j].
        /// </returns>
        /// <seealso cref="DyadicProduct"/>
        public Matrix TensorMultiply(Vector v)
        {
            return DyadicProduct(this, v);
        }

        /// <summary>
        /// Returns the value of the absolute minimum element.
        /// </summary>
        /// <returns>The value of the absolute minimum element.</returns>
        public virtual double AbsoluteMinimum()
        {
            return Math.Abs(this[this.AbsoluteMinimumIndex()]);
        }

        /// <summary>
        /// Returns the index of the absolute minimum element.
        /// </summary>
        /// <returns>The index of absolute minimum element.</returns>   
        public virtual int AbsoluteMinimumIndex()
        {
            var index = 0;
            var min = Math.Abs(this[index]);
            for (var i = 1; i < this.Count; i++)
            {
                var test = Math.Abs(this[i]);
                if (test < min)
                {
                    index = i;
                    min = test;
                }
            }

            return index;
        }

        /// <summary>
        /// Returns the value of maximum element.
        /// </summary>
        /// <returns>The value of maximum element.</returns>        
        public virtual double Maximum()
        {
            return this[this.MaximumIndex()];
        }

        /// <summary>
        /// Returns the index of the absolute maximum element.
        /// </summary>
        /// <returns>The index of absolute maximum element.</returns>          
        public virtual int MaximumIndex()
        {
            var index = 0;
            var max = this[0];
            for (var i = 1; i < this.Count; i++)
            {
                if (max < this[i])
                {
                    index = i;
                    max = this[i];
                }
            }

            return index;
        }

        /// <summary>
        /// Returns the value of the minimum element.
        /// </summary>
        /// <returns>The value of the minimum element.</returns>
        public virtual double Minimum()
        {
            return this[this.MinimumIndex()];
        }

        /// <summary>
        /// Returns the index of the minimum element.
        /// </summary>
        /// <returns>The index of minimum element.</returns>  
        public virtual int MinimumIndex()
        {
            var index = 0;
            var min = this[0];
            for (var i = 1; i < this.Count; i++)
            {
                if (min > this[i])
                {
                    index = i;
                    min = this[i];
                }
            }

            return index;
        }

        /// <summary>
        /// Computes the sum of the vector's elements.
        /// </summary>
        /// <returns>The sum of the vector's elements.</returns>
        public virtual double Sum()
        {
            double result = 0;
            for (var i = 0; i < this.Count; i++)
            {
                result += this[i];
            }

            return result;
        }

        /// <summary>
        /// Computes the sum of the absolute value of the vector's elements.
        /// </summary>
        /// <returns>The sum of the absolute value of the vector's elements.</returns>
        public virtual double SumMagnitudes()
        {
            double result = 0;
            for (var i = 0; i < this.Count; i++)
            {
                result += Math.Abs(this[i]);
            }

            return result;
        }

        #endregion

        #region Arithmetic Operator Overloading

        /// <summary>
        /// Returns a <strong>Vector</strong> containing the same values of rightSide. 
        /// </summary>
        /// <remarks>This method is included for completeness.</remarks>
        /// <param name="rightSide">The vector to get the values from.</param>
        /// <returns>A vector containing the same values as <paramref name="rightSide"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Vector operator +(Vector rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return rightSide.Plus();
        }

        /// <summary>
        /// Adds two <strong>Vectors</strong> together and returns the results.
        /// </summary>
        /// <param name="leftSide">One of the vectors to add.</param>
        /// <param name="rightSide">The other vector to add.</param>
        /// <returns>The result of the addition.</returns>
        /// <exception cref="ArgumentException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> are not the same size.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Vector operator +(Vector leftSide, Vector rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            if (leftSide.Count != rightSide.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "rightSide");
            }

            var ret = leftSide.Clone();
            ret.Add(rightSide);
            return ret;
        }

        /// <summary>
        /// Returns a <strong>Vector</strong> containing the negated values of rightSide. 
        /// </summary>
        /// <param name="rightSide">The vector to get the values from.</param>
        /// <returns>A vector containing the negated values as <paramref name="rightSide"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Vector operator -(Vector rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            return rightSide.Negate();
        }

        /// <summary>
        /// Subtracts two <strong>Vectors</strong> and returns the results.
        /// </summary>
        /// <param name="leftSide">The vector to subtract from.</param>
        /// <param name="rightSide">The vector to subtract.</param>
        /// <returns>The result of the subtraction.</returns>
        /// <exception cref="ArgumentException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> are not the same size.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Vector operator -(Vector leftSide, Vector rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            if (leftSide.Count != rightSide.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "rightSide");
            }

            var ret = leftSide.Clone();
            ret.Subtract(rightSide);
            return ret;
        }

        /// <summary>
        /// Multiplies a vector with a scalar.
        /// </summary>
        /// <param name="leftSide">The vector to scale.</param>
        /// <param name="rightSide">The scalar value.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static Vector operator *(Vector leftSide, double rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            var ret = leftSide.Clone();
            ret.Multiply(rightSide);
            return ret;
        }

        /// <summary>
        /// Multiplies a vector with a scalar.
        /// </summary>
        /// <param name="leftSide">The scalar value.</param>
        /// <param name="rightSide">The vector to scale.</param>
        /// <returns>The result of the multiplication.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Vector operator *(double leftSide, Vector rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            var ret = rightSide.Clone();
            ret.Multiply(leftSide);
            return ret;
        }

        /// <summary>
        /// Computes the dot product between two <strong>Vectors</strong>.
        /// </summary>
        /// <param name="leftSide">The left row vector.</param>
        /// <param name="rightSide">The right column vector.</param>
        /// <returns>The dot product between the two vectors.</returns>
        /// <exception cref="ArgumentException">If <paramref name="leftSide"/> and <paramref name="rightSide"/> are not the same size.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> or <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static double operator *(Vector leftSide, Vector rightSide)
        {
            if (rightSide == null)
            {
                throw new ArgumentNullException("rightSide");
            }

            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            if (leftSide.Count != rightSide.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "rightSide");
            }

            return leftSide.DotProduct(rightSide);
        }

        /// <summary>
        /// Divides a vector with a scalar.
        /// </summary>
        /// <param name="leftSide">The vector to divide.</param>
        /// <param name="rightSide">The scalar value.</param>
        /// <returns>The result of the division.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="leftSide"/> is <see langword="null" />.</exception>
        public static Vector operator /(Vector leftSide, double rightSide)
        {
            if (leftSide == null)
            {
                throw new ArgumentNullException("leftSide");
            }

            var ret = leftSide.Clone();
            ret.Multiply(1.0 / rightSide);
            return ret;
        }

        #endregion

        #region Vector Norms

        /// <summary>
        /// Euclidean Norm also known as 2-Norm.
        /// </summary>
        /// <returns>
        /// Scalar ret = sqrt(sum(this[i]^2))
        /// </returns>
        public virtual double Norm()
        {
            return this.NormP(2);
        }

        /// <summary>
        /// Squared Euclidean 2-Norm.
        /// </summary>
        /// <returns>
        /// Scalar ret = sum(this[i]^2)
        /// </returns>
        public virtual double SquaredNorm()
        {
            var norm = this.Norm();
            return norm * norm;
        }

        /// <summary>
        /// 1-Norm also known as Manhattan Norm or Taxicab Norm.
        /// </summary>
        /// <returns>
        /// Scalar ret = sum(abs(this[i]))
        /// </returns>
        public virtual double Norm1()
        {
            return this.NormP(1);
        }

        /// <summary>
        /// Computes the p-Norm.
        /// </summary>
        /// <param name="p">
        /// The p value.
        /// </param>
        /// <returns>
        /// Scalar ret = (sum(abs(this[i])^p))^(1/p)
        /// </returns>
        public virtual double NormP(int p)
        {
            if (1 > p)
            {
                throw new ArgumentOutOfRangeException("p");
            }

            var sum = CommonParallel.Aggregate(
                0, 
                this.Count, 
                index => Math.Pow(Math.Abs(this[index]), p));

            return Math.Pow(sum, 1.0 / p);
        }

        /// <summary>
        /// Infinity Norm.
        /// </summary>
        /// <returns>
        /// Scalar ret = max(abs(this[i]))
        /// </returns>
        public virtual double NormInfinity()
        {
            return CommonParallel.Select(
                0, 
                this.Count, 
                (index, localData) => localData = Math.Max(localData, Math.Abs(this[index])), 
                Math.Max);
        }

        /// <summary>
        /// Normalizes this vector to a unit vector with respect to the Eucliden 2-Norm.
        /// </summary>
        /// <returns>
        /// This vector normalized to a unit vector with respect to the Eucliden 2-Norm.
        /// </returns>
        public virtual Vector Normalize()
        {
            var norm = this.Norm();
            var clone = this.Clone();
            if (norm == 0.0)
            {
                return clone;
            }

            clone.Multiply(1.0 / norm);
            return clone;
        }

        #endregion

        #region Copying and Conversion

        /// <summary>
        /// Returns a deep-copy clone of the vector.
        /// </summary>
        /// <returns>
        /// A deep-copy clone of the vector.
        /// </returns>
        public Vector Clone()
        {
            var retrunVector = this.CreateVector(this.Count);
            this.CopyTo(retrunVector);
            return retrunVector;
        }

        /// <summary>
        /// Copies the values of this vector into the target vector.
        /// </summary>
        /// <param name="target">
        /// The vector to copy elements into.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="target"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="target"/> is not the same size as this vector.
        /// </exception>
        public virtual void CopyTo(Vector target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (this.Count != target.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "target");
            }

            if (ReferenceEquals(this, target))
            {
                return;
            }

            CommonParallel.For(
                0, 
                this.Count, 
                index => target[index] = this[index]);
        }

        /// <summary>
        /// Copies the requested elements from this vector to another.
        /// </summary>
        /// <param name="destination">
        /// The vector to copy the elements to.
        /// </param>
        /// <param name="offset">
        /// The element to start copying from.
        /// </param>
        /// <param name="destinationOffset">
        /// The element to start copying to.
        /// </param>
        /// <param name="count">
        /// The number of elements to copy.
        /// </param>
        public virtual void CopyTo(Vector destination, int offset, int destinationOffset, int count)
        {
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }

            if (offset >= this.Count)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            if (offset + count > this.Count)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if (destinationOffset >= destination.Count)
            {
                throw new ArgumentOutOfRangeException("destinationOffset");
            }

            if (destinationOffset + count > destination.Count)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if (ReferenceEquals(this, destination))
            {
                var tmpVector = destination.CreateVector(destination.Count);
                this.CopyTo(tmpVector);

                CommonParallel.For(
                    0, 
                    count, 
                    index => destination[destinationOffset + index] = tmpVector[offset + index]);
            }
            else
            {
                CommonParallel.For(
                    0, 
                    count, 
                    index => destination[destinationOffset + index] = this[offset + index]);
            }
        }

        /// <summary>
        /// Returns the data contained in the vector as an array.
        /// </summary>
        /// <returns>
        /// The vector's data as an array.
        /// </returns>
        public virtual double[] ToArray()
        {
            var ret = new double[this.Count];
            for (var i = 0; i < ret.Length; i++)
            {
                ret[i] = this[i];
            }

            return ret;
        }

        /// <summary>
        /// Create a matrix based on this vector in column form (one single column).
        /// </summary>
        /// <returns>
        /// This vector as a column matrix.
        /// </returns>
        public virtual Matrix ToColumnMatrix()
        {
            var matrix = this.CreateMatrix(this.Count, 1);
            for (var i = 0; i < this.Count; i++)
            {
                matrix[i, 0] = this[i];
            }

            return matrix;
        }

        /// <summary>
        /// Create a matrix based on this vector in row form (one single row).
        /// </summary>
        /// <returns>
        /// This vector as a row matrix.
        /// </returns>
        public virtual Matrix ToRowMatrix()
        {
            var matrix = this.CreateMatrix(1, this.Count);
            for (var i = 0; i < this.Count; i++)
            {
                matrix[0, i] = this[i];
            }

            return matrix;
        }

        /// <summary>
        /// Creates a vector containing specified elements.
        /// </summary>
        /// <param name="index">The first element to begin copying from.</param>
        /// <param name="length">The number of elements to copy.</param>
        /// <returns>A vector containing a copy of the specified elements.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><list><item>If <paramref name="index"/> is not positive or
        /// greater than or equal to the size of the vector.</item>
        /// <item>If <paramref name="index"/> + <paramref name="length"/> is greater than or equal to the size of the vector.</item>
        /// </list></exception>
        /// <exception cref="ArgumentException">If <paramref name="length"/> is not positive.</exception>
        public virtual Vector SubVector(int index, int length)
        {
            if (index < 0 || index >= this.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (length <= 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            if (index + length > this.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var result = this.CreateVector(length);

            CommonParallel.For(
                index, 
                index + length, 
                i => result[i - index] = this[i]);
            return result;
        }

        /// <summary>
        /// Set the values of this vector to the given values.
        /// </summary>
        /// <param name="values">The array containing the values to use.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="values"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If <paramref name="values"/> is not the same size as this vector.</exception>
        public virtual void SetValues(double[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            if (values.Length != this.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength, "values");
            }

            CommonParallel.For(
                0, 
                values.Length, 
                i => this[i] = values[i]);
        }

        #endregion

        #region Implemented Interfaces

#if !SILVERLIGHT

        #region ICloneable

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        object ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion

#endif

        #region IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IEnumerable<double>

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public virtual IEnumerator<double> GetEnumerator()
        {
            for (var index = 0; index < this.Count; index++)
            {
                yield return this[index];
            }
        }

        #endregion

        /// <summary>
        /// Returns an <see cref="IEnumerator{T}"/> that contains the position and value of the element.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator{T}"/> over this vector that contains the position and value of each
        /// non-zero element.
        /// </returns>
        /// <remarks>
        /// The enumerator returns a 
        /// <seealso cref="KeyValuePair{T,K}"/>
        /// with the key being the element index and the value 
        /// being the value of the element at that index. For sparse vectors, the enumerator will exclude all elements
        /// with a zero value.
        /// </remarks>
        public virtual IEnumerable<KeyValuePair<int, double>> GetIndexedEnumerator()
        {
            for (var i = 0; i < this.Count; i++)
            {
                yield return new KeyValuePair<int, double>(i, this[i]);
            }
        }

        #region IEquatable<Vector>

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">
        /// An object to compare with this object.
        /// </param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(Vector other)
        {
            // Reject equality when the argument is null or has a different length.
            if (other == null)
            {
                return false;
            }

            if (this.Count != other.Count)
            {
                return false;
            }

            // Accept if the argument is the same object as this.
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            // If all else fails, perform element wise comparison.
            for (var index = 0; index < this.Count; index++)
            {
                if (!this[index].AlmostEqual(other[index]))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region IFormattable

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="formatProvider">
        /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information.
        /// </param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return this.ToString(null, formatProvider);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="format">
        /// The format to use.
        /// </param>
        /// <param name="formatProvider">
        /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information.
        /// </param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            var stringBuilder = new StringBuilder();
            for (var index = 0; index < this.Count; index++)
            {
                stringBuilder.Append(this[index].ToString(format, formatProvider));
                if (index != this.Count - 1)
                {
                    stringBuilder.Append(formatProvider.GetTextInfo().ListSeparator);
                }
            }

            return stringBuilder.ToString();
        }

        #endregion

        #endregion

        #region System.Object overrides

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">
        /// The <see cref="System.Object"/> to compare with this instance.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as Vector);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            var hashNum = Math.Min(this.Count, 20);
            long hash = 0;
            for (var i = 0; i < hashNum; i++)
            {
#if SILVERLIGHT
                hash ^= Precision.DoubleToInt64Bits(this[i]);
#else
                hash ^= BitConverter.DoubleToInt64Bits(this[i]);
#endif
            }

            return BitConverter.ToInt32(BitConverter.GetBytes(hash), 4);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.ToString(null, null);
        }

        #endregion
    }
}
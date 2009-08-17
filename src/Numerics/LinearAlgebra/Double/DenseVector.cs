// <copyright file="DenseVector.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
// Copyright (c) 2009 Math.NET
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
using MathNet.Numerics.Algorithms;
using MathNet.Numerics.Algorithms.LinearAlgebra;
using MathNet.Numerics.Threading;

namespace MathNet.Numerics.LinearAlgebra.Double
{
    using System;

    using Properties;

    /// <summary>
    /// A vector using dense storage.
    /// </summary>
    public class DenseVector : Vector
    {
        /// <summary>
        /// The linear algebra provider.
        /// </summary>
        private readonly ILinearAlgebra _linearAlgebra = AlgorithmFactory.LinearAlgebra;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DenseVector"/> class with a given size.
        /// </summary>
        /// <param name="size">
        /// the size of the vector.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="size"/> is less than one.
        /// </exception>
        public DenseVector(int size)
            : base(size)
        {
            Data = new double[size];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseVector"/> class with a given size
        /// and each element set to the given value;
        /// </summary>
        /// <param name="size">
        /// the size of the vector.
        /// </param>
        /// <param name="value">
        /// the value to set each element to.
        /// </param>
        /// <exception cref="ArgumentException">
        /// If <paramref name="size"/> is less than one.
        /// </exception>
        public DenseVector(int size, double value)
            : this(size)
        {
            for (var index = 0; index < Data.Length; index++)
            {
                Data[index] = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DenseVector"/> class by
        /// copying the values from another.
        /// </summary>
        /// <param name="other">
        /// The matrix to create the new matrix from.
        /// </param>
        public DenseVector(Vector other)
            : this(other.Count)
        {
            var vector = other as DenseVector;
            if (vector == null)
            {
                // using enumerators since they will be more efficient for copying sparse matrices
                foreach (var item in other.GetIndexedEnumerator())
                {
                    Data[item.Key] = item.Value;
                }
            }
            else
            {
                Buffer.BlockCopy(vector.Data, 0, Data, 0, Data.Length * Constants.SizeOfDouble);
            }
        }

        /// <summary>
        ///  Gets the vector's data.
        /// </summary>
        /// <value>The vector's data.</value>
        internal double[] Data
        {
            get;
            private set;
        }

        /// <summary>Gets or sets the value at the given <paramref name="index"/>.</summary>
        /// <param name="index">The index of the value to get or set.</param>
        /// <returns>The value of the vector at the given <paramref name="index"/>.</returns> 
        /// <exception cref="IndexOutOfRangeException">If <paramref name="index"/> is negative or 
        /// greater than the size of the vector.</exception>
        public override double this[int index]
        {
            get
            {
                return Data[index];
            }

            set
            {
                Data[index] = value;
            }
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
        public override Matrix CreateMatrix(int rows, int columns)
        {
            throw new NotImplementedException();
        }

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
        public override Vector CreateVector(int size)
        {
            return new DenseVector(size);
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
        public override void CopyTo(Vector target)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            if (Count != target.Count)
            {
                throw new ArgumentException("target", Resources.ArgumentVectorsSameLength);
            }

            var otherVector = target as DenseVector;
            if (otherVector == null)
            {
                for (var index = 0; index < Data.Length; index++)
                {
                    target[index] = Data[index];
                }
            }
            else
            {
                Buffer.BlockCopy(Data, 0, otherVector.Data, 0, Data.Length * Constants.SizeOfDouble);
            }
        }

        /// <summary>
        /// Adds a scalar to each element of the vector.
        /// </summary>
        /// <param name="scalar">The scalar to add.</param>
        public override void Add(double scalar)
        {
            if (scalar.AlmostZero())
            {
                return;
            }

            Parallel.For(0, Count, i => Data[i] += scalar);
        }

        /// <summary>
        /// Adds a scalar to each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to add.</param>
        /// <param name="result">The vector to store the result of the addition.</param>
        /// <exception cref="ArgumentNullException">If the result vector is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public override void Add(double scalar, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (Count != result.Count)
            {
                throw new ArgumentException("result", Resources.ArgumentVectorsSameLength);
            }

            CopyTo(result);
            result.Add(scalar);
        }

        /// <summary>
        /// Adds another vector to this vector.
        /// </summary>
        /// <param name="other">The vector to add to this one.</param>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        public override void Add(Vector other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (Count != other.Count)
            {
                throw new ArgumentException("other", Resources.ArgumentVectorsSameLength);
            }

            var denseVector = other as DenseVector;

            if (denseVector == null)
            {
                base.Add(other);
            }
            else
            {
                _linearAlgebra.AddVectorToScaledVector(Data, 1.0, denseVector.Data);
            }
        }

        /// <summary>
        /// Adds another vector to this vector and stores the result into the result vector.
        /// </summary>
        /// <param name="other">The vector to add to this one.</param>
        /// <param name="result">The vector to store the result of the addition.</param>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentNullException">If the result vector is <see langword="null" />.</exception> 
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public override void Add(Vector other, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (Count != other.Count)
            {
                throw new ArgumentException("other", Resources.ArgumentVectorsSameLength);
            }

            if (Count != result.Count)
            {
                throw new ArgumentException("result", Resources.ArgumentVectorsSameLength);
            }

            if (ReferenceEquals(this, result) || ReferenceEquals(other, result))
            {
                var tmp = result.CreateVector(result.Count);
                Add(other, tmp);
                tmp.CopyTo(result);
            }
            else
            {
                CopyTo(result);
                result.Add(other);
            }
        }

        /// <summary>
        /// Returns a <strong>Vector</strong> containing the same values of rightSide. 
        /// </summary>
        /// <remarks>This method is included for completeness.</remarks>
        /// <param name="rightSide">The vector to get the values from.</param>
        /// <returns>A vector containing a the same values as <paramref name="rightSide"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Vector operator +(DenseVector rightSide)
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
        public static Vector operator +(DenseVector leftSide, DenseVector rightSide)
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
                throw new ArgumentException("rightSide", Resources.ArgumentVectorsSameLength);
            }

            var ret = leftSide.Clone();
            ret.Add(rightSide);
            return ret;
        }

        /// <summary>
        /// Subtracts a scalar from each element of the vector.
        /// </summary>
        /// <param name="scalar">The scalar to subtract.</param>
        public override void Subtract(double scalar)
        {
            if (scalar.AlmostZero())
            {
                return;
            }

            Parallel.For(0, Count, i => Data[i] -= scalar);
        }

        /// <summary>
        /// Subtracts a scalar from each element of the vector and stores the result in the result vector.
        /// </summary>
        /// <param name="scalar">The scalar to subtract.</param>
        /// <param name="result">The vector to store the result of the subtraction.</param>
        /// <exception cref="ArgumentNullException">If the result vector is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public override void Subtract(double scalar, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (Count != result.Count)
            {
                throw new ArgumentException("result", Resources.ArgumentVectorsSameLength);
            }

            CopyTo(result);
            result.Subtract(scalar);
        }

        /// <summary>
        /// Subtracts another vector from this vector.
        /// </summary>
        /// <param name="other">The vector to subtract from this one.</param>
        /// <exception cref="ArgumentNullException">If the other vector is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
         public override void Subtract(Vector other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

            if (Count != other.Count)
            {
                throw new ArgumentException("other", Resources.ArgumentVectorsSameLength);
            }

            var denseVector = other as DenseVector;

            if (denseVector == null)
            {
                base.Subtract(other);
            }
            else
            {
                _linearAlgebra.AddVectorToScaledVector(Data, -1.0, denseVector.Data);
            }
        }

         /// <summary>
         /// Subtracts another vector to this vector and stores the result into the result vector.
         /// </summary>
         /// <param name="other">The vector to subtract from this one.</param>
         /// <param name="result">The vector to store the result of the subtraction.</param>
         /// <exception cref="ArgumentNullException">If the other vector is <see langword="null"/>.</exception>
         /// <exception cref="ArgumentNullException">If the result vector is <see langword="null"/>.</exception>
         /// <exception cref="ArgumentException">If this vector and <paramref name="other"/> are not the same size.</exception>
         /// <exception cref="ArgumentException">If this vector and <paramref name="result"/> are not the same size.</exception>
        public override void Subtract(Vector other, Vector result)
        {
            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (Count != other.Count)
            {
                throw new ArgumentException("other", Resources.ArgumentVectorsSameLength);
            }

            if (Count != result.Count)
            {
                throw new ArgumentException("result", Resources.ArgumentVectorsSameLength);
            }

            if (ReferenceEquals(this, result) || ReferenceEquals(other, result))
            {
                var tmp = result.CreateVector(result.Count);
                Subtract(other, tmp);
                tmp.CopyTo(result);
            }
            else
            {
                CopyTo(result);
                result.Subtract(other);
            }
        }

        /// <summary>
        /// Returns a <strong>Vector</strong> containing the negated values of rightSide. 
        /// </summary>
        /// <param name="rightSide">The vector to get the values from.</param>
        /// <returns>A vector containing the negated values as <paramref name="rightSide"/>.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="rightSide"/> is <see langword="null" />.</exception>
        public static Vector operator -(DenseVector rightSide)
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
        public static Vector operator -(DenseVector leftSide, DenseVector rightSide)
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
                throw new ArgumentException("rightSide", Resources.ArgumentVectorsSameLength);
            }

            var ret = leftSide.Clone();
            ret.Subtract(rightSide);
            return ret;
        }

        /// <summary>
        /// Returns a negated vector.
        /// </summary>
        /// <returns>The negated vector.</returns>
        /// <remarks>Added as an alternative to the unary negation operator.</remarks>
        public virtual Vector Negate()
        {
            var result = new DenseVector(Count);
            Parallel.For(0, Count, i => result[i] = -Data[i]);
            return result;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathNet.Numerics.LinearAlgebra.Storage
{
    /// <summary>
    /// Node to store elements in Knuth sparse matrix
    /// </summary>
     public partial class KnuthNode<T>
         where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Define zero element for current type
        /// </summary>
        readonly T zero;

        /// <summary>
        /// Row position of element
        /// </summary>
        public int Row
        {
            set;
            get;
        }

        /// <summary>
        /// Column possition of element
        /// </summary>
        public int Col
        {
            set;
            get;
        }

        /// <summary>
        /// Stored value
        /// </summary>
        public T Val
        {
            set;
            get;
        }

        /// <summary>
        /// Pointer to lefter element in row 
        /// </summary>
        public KnuthNode<T> Left
        {
            set;
            get;
        }

        /// <summary>
        /// Pointer to upper element in column
        /// </summary>
        public KnuthNode<T> Up
        {
            set;
            get;
        }

        /// <summary>
        /// Initialize new default instanse of KnuthNode class
        /// </summary>
        public KnuthNode(T _zero)
        {
            zero = _zero;
            Row = -1;
            Col = -1;
            Val = _zero;
            Left = this;
            Up = this;
        }
        /// <summary>
        /// Initialize new instanse of KnuthNode class
        /// </summary>
        /// <param name="x">row number </param>
        /// <param name="y">column number</param>
        /// <param name="val">value</param>
        public KnuthNode(int x, int y, T val, T _zero)
        {
            zero = _zero;
            Row = x;
            Col = y;
            Val = val;
            Left = this;
            Up = this;
        }

        /// <summary>
        /// Initialize new instanse of KnuthNode class
        /// </summary>
        /// <param name="x">row number </param>
        /// <param name="y">column number</param>
        /// <param name="val">value</param>
        /// <param name="left">pointer to lefter element in row</param>
        /// <param name="up">pointer to upper element in column</param>
        public KnuthNode(int x, int y, T val, KnuthNode<T> left, KnuthNode<T> up, T _zero)
        {
            zero = _zero;
            Row = x;
            Col = y;
            Val = val;
            Left = left;
            Up = up;
            Left = this;
            Up = this;
        }

        /// <summary>
        /// Check if elements are equal
        /// </summary>
        /// <param name="nd2">second element</param>
        /// <returns>if elements are equal</returns>
        public bool Equals(KnuthNode<T> nd2)
        {
            if (nd2 == null)
                return false;
            return Val.Equals(nd2.Val) && Row == nd2.Row && Col == nd2.Col;
        }

        /// <summary>
        /// Oparetor of equality
        /// </summary>
        /// <param name="nd1">first element</param>
        /// <param name="nd2">second element</param>
        /// <returns>if elements are equals</returns>
        public static bool operator ==(KnuthNode<T> nd1, KnuthNode<T> nd2)
        {
            return nd1.Equals(nd2);
        }

        /// <summary>
        /// Oparetor of equality
        /// </summary>
        /// <param name="nd1">first element</param>
        /// <param name="nd2">second element</param>
        /// <returns>if elements aren`t equals</returns>
        public static bool operator !=(KnuthNode<T> nd1, KnuthNode<T> nd2)
        {
            return !nd1.Equals(nd2);
        }

        /// <summary>
        /// Check if elements are equal
        /// </summary>
        /// <param name="nd2">second element</param>
        /// <returns>if elements are equal</returns>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

         /// <summary>
         /// Gets hash code of node
         /// </summary>
         /// <returns></returns>
        public override int GetHashCode()
        {
            var hashNum = Math.Min(Row * Col, 25);
            int hash = 17;
            unchecked
            {
                for (var i = 0; i < hashNum; i++)
                {
                    var col = i % Row;
                    var row = (i - col) / Col;
                    hash = hash * 31 + Val.GetHashCode();
                }
            }
            return hash;
        }
    }
}

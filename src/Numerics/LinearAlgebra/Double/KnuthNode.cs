using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MathNet.Numerics.LinearAlgebra.Double
{
    /// <summary>
    /// Node to store elements in Knuth sparse matrix
    /// </summary>
    public class KnuthNode
    {
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
        public double Val
        {
            set;
            get;
        }
        /// <summary>
        /// Pointer to lefter element in row 
        /// </summary>
        public KnuthNode Left
        {
            set;
            get;
        }
        /// <summary>
        /// pointer to upper element in column
        /// </summary>
        public KnuthNode Up
        {
            set;
            get;
        }

        /// <summary>
        /// Initialize new default instanse of KnuthNode class
        /// </summary>
        public KnuthNode()
        {
            Row = -1;
            Col = -1;
            Val = 0;
            Left = this;
            Up = this;
        }
        /// <summary>
        /// Initialize new instanse of KnuthNode class
        /// </summary>
        /// <param name="x">row number </param>
        /// <param name="y">column number</param>
        /// <param name="val">value</param>
        public KnuthNode(int x, int y, double val)
        {
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
        public KnuthNode(int x, int y, double val, KnuthNode left, KnuthNode up)
        {
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
        public bool Equals(KnuthNode nd2)
        {
            if (nd2 == null)
                return false;
            return Val == nd2.Val && Row == nd2.Row && Col == nd2.Col;
        }

        /// <summary>
        /// Oparetor of equality
        /// </summary>
        /// <param name="nd1">first element</param>
        /// <param name="nd2">second element</param>
        /// <returns>if elements are equals</returns>
        public static bool operator ==(KnuthNode nd1, KnuthNode nd2)
        {
            return nd1.Equals(nd2);
        }

        /// <summary>
        /// Oparetor of equality
        /// </summary>
        /// <param name="nd1">first element</param>
        /// <param name="nd2">second element</param>
        /// <returns>if elements aren`t equals</returns>
        public static bool operator !=(KnuthNode nd1, KnuthNode nd2)
        {
            return !(nd1 == nd2);
        }
    }
}

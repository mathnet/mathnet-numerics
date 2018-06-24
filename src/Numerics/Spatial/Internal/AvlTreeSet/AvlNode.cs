using System.Diagnostics.CodeAnalysis;

namespace MathNet.Numerics.Spatial.Internal.AvlTreeSet
{
    /// <summary>
    /// A node of the Avl Tree
    /// </summary>
    /// <typeparam name="T">Any type</typeparam>
    [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "By design")]
    internal sealed class AvlNode<T>
    {
        /// <summary>
        /// Gets or sets the parent node
        /// </summary>
        public AvlNode<T> Parent;

        /// <summary>
        /// Gets or sets the left node
        /// </summary>
        public AvlNode<T> Left;

        /// <summary>
        /// Gets or sets the right node
        /// </summary>
        public AvlNode<T> Right;

        /// <summary>
        /// Gets or sets the item
        /// </summary>
        public T Item;

        /// <summary>
        /// Gets or sets the Avl balance
        /// </summary>
        public int Balance;

        /// <summary>
        /// Non recursive function that return the next ordered node
        /// </summary>
        /// <returns>The next node</returns>
        public AvlNode<T> GetNextNode()
        {
            AvlNode<T> current;

            if (this.Right != null)
            {
                current = this.Right;
                while (current.Left != null)
                {
                    current = current.Left;
                }

                return current;
            }

            current = this;
            while (current.Parent != null)
            {
                if (current.Parent.Left == current)
                {
                    return current.Parent;
                }

                current = current.Parent;
            }

            return null;
        }

        /// <summary>
        /// Non recursive function that return the previous ordered node
        /// </summary>
        /// <returns>The previous node</returns>
        public AvlNode<T> GetPreviousNode()
        {
            AvlNode<T> current;

            if (this.Left != null)
            {
                current = this.Left;
                while (current.Right != null)
                {
                    current = current.Right;
                }

                return current;
            }

            current = this;
            while (current.Parent != null)
            {
                if (current.Parent.Right == current)
                {
                    return current.Parent;
                }

                current = current.Parent;
            }

            return null;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"AvlNode [{this.Item}], balance: {this.Balance}, Parent: {this.Parent?.Item.ToString()}, Left: {this.Left?.Item.ToString()}, Right: {this.Right?.Item.ToString()},";
        }
    }
}

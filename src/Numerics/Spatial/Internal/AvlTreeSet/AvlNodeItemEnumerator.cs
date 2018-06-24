using System;
using System.Collections;
using System.Collections.Generic;

namespace MathNet.Numerics.Spatial.Internal.AvlTreeSet
{
    /// <summary>
    /// An enumerator for AvlNodeItems
    /// </summary>
    /// <typeparam name="T">Any type which is also a node type</typeparam>
    internal class AvlNodeItemEnumerator<T> : IEnumerator<T>
    {
        /// <summary>
        /// A reference to the tree
        /// </summary>
        private readonly AvlTreeSet<T> avlTree;

        /// <summary>
        /// The current node
        /// </summary>
        private AvlNode<T> current = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvlNodeItemEnumerator{T}"/> class.
        /// </summary>
        /// <param name="avlTree">The tree to enumerate</param>
        public AvlNodeItemEnumerator(AvlTreeSet<T> avlTree)
        {
            this.avlTree = avlTree ?? throw new ArgumentNullException("avlTree can't be null");
        }

        /// <summary>
        /// Gets the current node
        /// </summary>
        public T Current
        {
            get
            {
                if (this.current == null)
                {
                    throw new InvalidOperationException("Current is invalid");
                }

                return this.current.Item;
            }
        }

        /// <summary>
        /// Gets the current node
        /// </summary>
        object IEnumerator.Current => this.Current;

        /// <summary>
        /// Dispose of the enumerator
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Moves to the next node
        /// </summary>
        /// <returns>True if the move to the next node was successful; otherwise false</returns>
        public bool MoveNext()
        {
            if (this.current == null)
            {
                this.current = this.avlTree.GetFirstNode();
            }
            else
            {
                this.current = this.current.GetNextNode();
            }

            // Should check for an empty tree too :-)
            if (this.current == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Resets the enumerator
        /// </summary>
        public void Reset()
        {
            this.current = null;
        }
    }
}

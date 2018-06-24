using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace MathNet.Numerics.Spatial.Internal.AvlTreeSet
{
    /// <summary>
    /// 2016-12-08, Eric Ouellet
    /// The code is an adapted version of BitLush AvlTree: https://bitlush.com/blog/efficient-avl-tree-in-c-sharp
    /// </summary>
    /// <typeparam name="T">Any type</typeparam>
    internal class AvlTreeSet<T> : IEnumerable<T>, IEnumerable, ICollection<T>, ICollection
    {
        /// <summary>
        /// A comparer
        /// </summary>
        private readonly IComparer<T> comparer;

        /// <summary>
        /// The root node of the tree
        /// </summary>
        private AvlNode<T> root;

        /// <summary>
        /// a sync object
        /// </summary>
        private object syncRoot;

        /// <summary>
        /// A count of nodes
        /// </summary>
        private int count = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="AvlTreeSet{T}"/> class.
        /// </summary>
        /// <param name="comparer">a comparer for nodes</param>
        public AvlTreeSet(IComparer<T> comparer)
        {
            this.comparer = comparer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AvlTreeSet{T}"/> class.
        /// </summary>
        public AvlTreeSet()
            : this(Comparer<T>.Default)
        {
        }

        /// <summary>
        /// Gets the root node
        /// </summary>
        public AvlNode<T> Root => this.root;

        /// <summary>
        /// Gets the count of nodes
        /// </summary>
        public int Count => this.count;

        /// <summary>
        /// Gets the sync object
        /// </summary>
        public object SyncRoot
        {
            get
            {
                if (this.syncRoot == null)
                {
                    Interlocked.CompareExchange(ref this.syncRoot, new object(), (object)null);
                }

                return this.syncRoot;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the tree is synchronized;
        /// </summary>
        public bool IsSynchronized => true;

        /// <summary>
        /// Gets a value indicating whether the tree is read only;
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets an enumerator for the tree
        /// </summary>
        /// <returns>An enumerator</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new AvlNodeItemEnumerator<T>(this);
        }

        /// <summary>
        /// Gets a value indicating whether the tree contains an item
        /// </summary>
        /// <param name="item">The item to find</param>
        /// <returns>True if the item is found; otherwise false;</returns>
        public bool Contains(T item)
        {
            AvlNode<T> node = this.root;

            while (node != null)
            {
                int compareResult = this.comparer.Compare(item, node.Item);
                if (compareResult < 0)
                {
                    node = node.Left;
                }
                else if (compareResult > 0)
                {
                    node = node.Right;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Adds an item to the tree
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <returns>True if adding was successful; otherwise false</returns>
        public virtual bool Add(T item)
        {
            AvlNode<T> node = this.root;

            while (node != null)
            {
                int compare = this.comparer.Compare(item, node.Item);

                if (compare < 0)
                {
                    AvlNode<T> left = node.Left;

                    if (left == null)
                    {
                        node.Left = new AvlNode<T> { Item = item, Parent = node };
                        this.AddBalance(node, 1);
                        return true;
                    }
                    else
                    {
                        node = left;
                    }
                }
                else if (compare > 0)
                {
                    AvlNode<T> right = node.Right;

                    if (right == null)
                    {
                        node.Right = new AvlNode<T> { Item = item, Parent = node };
                        this.AddBalance(node, -1);
                        return true;
                    }
                    else
                    {
                        node = right;
                    }
                }
                else
                {
                    return false;
                }
            }

            this.root = new AvlNode<T> { Item = item };
            this.count++;

            return true;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            this.root = null;
            this.count = 0;
        }

        /// <summary>
        /// Gets an enumerator
        /// </summary>
        /// <returns>an enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Copies to an array
        /// </summary>
        /// <param name="array">The array to copy to</param>
        /// <param name="index">start point</param>
        /// <param name="count">number of items to copy</param>
        public void CopyTo(T[] array, int index, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException("'array' can't be null");
            }

            if (index < 0)
            {
                throw new ArgumentException("'index' can't be null");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("'count' should be greater or equal to 0");
            }

            if (index > array.Length || count > array.Length - index)
            {
                throw new ArgumentException("The array size is not big enough to get all items");
            }

            if (count == 0)
            {
                return;
            }

            int indexIter = 0;
            int indexArray = 0;

            AvlNode<T> current = this.GetFirstNode();
            while (current.GetNextNode() != null)
            {
                if (indexIter >= index)
                {
                    array[indexArray] = current.Item;
                    indexArray++;
                    count--;
                    if (count == 0)
                    {
                        return;
                    }
                }

                indexIter++;
            }

            /*
            foreach (AvlNode<T> node in this.Nodes())
            {
                if (indexIter >= index)
                {
                    array[indexArray] = node.Item;
                    indexArray++;
                    count--;
                    if (count == 0)
                    {
                        return;
                    }
                }

                indexIter++;
            }*/
        }

        /// <inheritdoc/>
        public void CopyTo(T[] array, int arrayIndex)
        {
            this.CopyTo(array, arrayIndex, this.Count);
        }

        /// <inheritdoc/>
        public void CopyTo(Array array, int index)
        {
            this.CopyTo(array as T[], index, this.Count);
        }

        /// <inheritdoc/>
        void ICollection<T>.Add(T item)
        {
            this.Add(item);
        }

        /// <summary>
        /// Gets the first item
        /// </summary>
        /// <returns>The first item</returns>
        public T GetFirstItem()
        {
            AvlNode<T> node = this.GetFirstNode();
            if (node != null)
            {
                return node.Item;
            }

            return default(T);
        }

        /// <summary>
        /// Gets the first node
        /// </summary>
        /// <returns>The first node</returns>
        public AvlNode<T> GetFirstNode()
        {
            if (this.Root != null)
            {
                AvlNode<T> current = this.Root;
                while (current.Left != null)
                {
                    current = current.Left;
                }

                return current;
            }

            return null;
        }

        /// <summary>
        /// gets the last item
        /// </summary>
        /// <returns>The last item</returns>
        public T GetLastItem()
        {
            AvlNode<T> node = this.GetLastNode();
            if (node != null)
            {
                return node.Item;
            }

            return default(T);
        }

        /// <summary>
        /// Gets the last node
        /// </summary>
        /// <returns>returns the last node</returns>
        public AvlNode<T> GetLastNode()
        {
            if (this.Root != null)
            {
                AvlNode<T> current = this.Root;
                while (current.Right != null)
                {
                    current = current.Right;
                }

                return current;
            }

            return null;
        }

        /// <summary>
        /// Removes a node
        /// </summary>
        /// <param name="item">item to remove</param>
        /// <returns>True if successful; otherwise false</returns>
        public virtual bool Remove(T item)
        {
            AvlNode<T> node = this.root;

            while (node != null)
            {
                if (this.comparer.Compare(item, node.Item) < 0)
                {
                    node = node.Left;
                }
                else if (this.comparer.Compare(item, node.Item) > 0)
                {
                    node = node.Right;
                }
                else
                {
                    this.RemoveNode(node);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets a node with the provided item
        /// </summary>
        /// <param name="item">An item to find</param>
        /// <returns>The node with that item</returns>
        protected AvlNode<T> GetNode(T item)
        {
            AvlNode<T> node = this.root;

            while (node != null)
            {
                int compareResult = this.comparer.Compare(item, node.Item);
                if (compareResult < 0)
                {
                    node = node.Left;
                }
                else if (compareResult > 0)
                {
                    node = node.Right;
                }
                else
                {
                    return node;
                }
            }

            return null;
        }

        /// <summary>
        /// Should always be called for any inserted node
        /// </summary>
        /// <param name="node">A node</param>
        /// <param name="balance">The balance measure</param>
        //// [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void AddBalance(AvlNode<T> node, int balance)
        {
            this.count++;

            while (node != null)
            {
                balance = node.Balance += balance;

                if (balance == 0)
                {
                    break;
                }

                if (balance == 2)
                {
                    if (node.Left.Balance == 1)
                    {
                        this.RotateRight(node);
                    }
                    else
                    {
                        this.RotateLeftRight(node);
                    }

                    break;
                }

                if (balance == -2)
                {
                    if (node.Right.Balance == -1)
                    {
                        this.RotateLeft(node);
                    }
                    else
                    {
                        this.RotateRightLeft(node);
                    }

                    break;
                }

                AvlNode<T> parent = node.Parent;

                if (parent != null)
                {
                    balance = parent.Left == node ? 1 : -1;
                }

                node = parent;
            }
        }

        /// <summary>
        /// Rotate the tree node left
        /// </summary>
        /// <param name="node">A node</param>
        /// <returns>The rotated node</returns>
        protected AvlNode<T> RotateLeft(AvlNode<T> node)
        {
            AvlNode<T> right = node.Right;
            AvlNode<T> rightLeft = right.Left;
            AvlNode<T> parent = node.Parent;

            right.Parent = parent;
            right.Left = node;
            node.Right = rightLeft;
            node.Parent = right;

            if (rightLeft != null)
            {
                rightLeft.Parent = node;
            }

            if (node == this.root)
            {
                this.root = right;
            }
            else if (parent.Right == node)
            {
                parent.Right = right;
            }
            else
            {
                parent.Left = right;
            }

            right.Balance++;
            node.Balance = -right.Balance;

            return right;
        }

        /// <summary>
        /// Rotate a tree node right
        /// </summary>
        /// <param name="node">a node</param>
        /// <returns>The rotated tree node</returns>
        protected AvlNode<T> RotateRight(AvlNode<T> node)
        {
            AvlNode<T> left = node.Left;
            AvlNode<T> leftRight = left.Right;
            AvlNode<T> parent = node.Parent;

            left.Parent = parent;
            left.Right = node;
            node.Left = leftRight;
            node.Parent = left;

            if (leftRight != null)
            {
                leftRight.Parent = node;
            }

            if (node == this.root)
            {
                this.root = left;
            }
            else if (parent.Left == node)
            {
                parent.Left = left;
            }
            else
            {
                parent.Right = left;
            }

            left.Balance--;
            node.Balance = -left.Balance;

            return left;
        }

        /// <summary>
        /// Rotate a tree node leftright
        /// </summary>
        /// <param name="node">a node</param>
        /// <returns>a rotated tree node</returns>
        protected AvlNode<T> RotateLeftRight(AvlNode<T> node)
        {
            AvlNode<T> left = node.Left;
            AvlNode<T> leftRight = left.Right;
            AvlNode<T> parent = node.Parent;
            AvlNode<T> leftRightRight = leftRight.Right;
            AvlNode<T> leftRightLeft = leftRight.Left;

            leftRight.Parent = parent;
            node.Left = leftRightRight;
            left.Right = leftRightLeft;
            leftRight.Left = left;
            leftRight.Right = node;
            left.Parent = leftRight;
            node.Parent = leftRight;

            if (leftRightRight != null)
            {
                leftRightRight.Parent = node;
            }

            if (leftRightLeft != null)
            {
                leftRightLeft.Parent = left;
            }

            if (node == this.root)
            {
                this.root = leftRight;
            }
            else if (parent.Left == node)
            {
                parent.Left = leftRight;
            }
            else
            {
                parent.Right = leftRight;
            }

            if (leftRight.Balance == -1)
            {
                node.Balance = 0;
                left.Balance = 1;
            }
            else if (leftRight.Balance == 0)
            {
                node.Balance = 0;
                left.Balance = 0;
            }
            else
            {
                node.Balance = -1;
                left.Balance = 0;
            }

            leftRight.Balance = 0;

            return leftRight;
        }

        /// <summary>
        /// Rotate a tree node rightleft
        /// </summary>
        /// <param name="node">a node</param>
        /// <returns>a rotated tree node</returns>
        protected AvlNode<T> RotateRightLeft(AvlNode<T> node)
        {
            AvlNode<T> right = node.Right;
            AvlNode<T> rightLeft = right.Left;
            AvlNode<T> parent = node.Parent;
            AvlNode<T> rightLeftLeft = rightLeft.Left;
            AvlNode<T> rightLeftRight = rightLeft.Right;

            rightLeft.Parent = parent;
            node.Right = rightLeftLeft;
            right.Left = rightLeftRight;
            rightLeft.Right = right;
            rightLeft.Left = node;
            right.Parent = rightLeft;
            node.Parent = rightLeft;

            if (rightLeftLeft != null)
            {
                rightLeftLeft.Parent = node;
            }

            if (rightLeftRight != null)
            {
                rightLeftRight.Parent = right;
            }

            if (node == this.root)
            {
                this.root = rightLeft;
            }
            else if (parent.Right == node)
            {
                parent.Right = rightLeft;
            }
            else
            {
                parent.Left = rightLeft;
            }

            if (rightLeft.Balance == 1)
            {
                node.Balance = 0;
                right.Balance = -1;
            }
            else if (rightLeft.Balance == 0)
            {
                node.Balance = 0;
                right.Balance = 0;
            }
            else
            {
                node.Balance = 1;
                right.Balance = 0;
            }

            rightLeft.Balance = 0;

            return rightLeft;
        }

        /// <summary>
        /// Removes a node
        /// </summary>
        /// <param name="node">node to remove</param>
        protected void RemoveNode(AvlNode<T> node)
        {
            this.count--;

            AvlNode<T> left = node.Left;
            AvlNode<T> right = node.Right;

            if (left == null)
            {
                if (right == null)
                {
                    if (node == this.root)
                    {
                        this.root = null;
                    }
                    else
                    {
                        if (node.Parent.Left == node)
                        {
                            node.Parent.Left = null;

                            this.RemoveBalance(node.Parent, -1);
                        }
                        else if (node.Parent.Right == node)
                        {
                            node.Parent.Right = null;

                            this.RemoveBalance(node.Parent, 1);
                        }
                    }
                }
                else
                {
                    Replace(node, right);

                    this.RemoveBalance(node, 0);
                }
            }
            else if (right == null)
            {
                Replace(node, left);

                this.RemoveBalance(node, 0);
            }
            else
            {
                AvlNode<T> successor = right;

                if (successor.Left == null)
                {
                    AvlNode<T> parent = node.Parent;

                    successor.Parent = parent;
                    successor.Left = left;
                    successor.Balance = node.Balance;

                    left.Parent = successor;

                    if (node == this.root)
                    {
                        this.root = successor;
                    }
                    else
                    {
                        if (parent.Left == node)
                        {
                            parent.Left = successor;
                        }
                        else
                        {
                            parent.Right = successor;
                        }
                    }

                    this.RemoveBalance(successor, 1);
                }
                else
                {
                    while (successor.Left != null)
                    {
                        successor = successor.Left;
                    }

                    AvlNode<T> parent = node.Parent;
                    AvlNode<T> successorParent = successor.Parent;
                    AvlNode<T> successorRight = successor.Right;

                    if (successorParent.Left == successor)
                    {
                        successorParent.Left = successorRight;
                    }
                    else
                    {
                        successorParent.Right = successorRight;
                    }

                    if (successorRight != null)
                    {
                        successorRight.Parent = successorParent;
                    }

                    successor.Parent = parent;
                    successor.Left = left;
                    successor.Balance = node.Balance;
                    successor.Right = right;
                    right.Parent = successor;

                    left.Parent = successor;

                    if (node == this.root)
                    {
                        this.root = successor;
                    }
                    else
                    {
                        if (parent.Left == node)
                        {
                            parent.Left = successor;
                        }
                        else
                        {
                            parent.Right = successor;
                        }
                    }

                    this.RemoveBalance(successorParent, -1);
                }
            }
        }

        /// <summary>
        /// Should always be called for any removed node
        /// </summary>
        /// <param name="node">a node</param>
        /// <param name="balance">the node balance</param>
        protected void RemoveBalance(AvlNode<T> node, int balance)
        {
            while (node != null)
            {
                balance = node.Balance += balance;

                if (balance == 2)
                {
                    if (node.Left.Balance >= 0)
                    {
                        node = this.RotateRight(node);

                        if (node.Balance == -1)
                        {
                            return;
                        }
                    }
                    else
                    {
                        node = this.RotateLeftRight(node);
                    }
                }
                else if (balance == -2)
                {
                    if (node.Right.Balance <= 0)
                    {
                        node = this.RotateLeft(node);

                        if (node.Balance == 1)
                        {
                            return;
                        }
                    }
                    else
                    {
                        node = this.RotateRightLeft(node);
                    }
                }
                else if (balance != 0)
                {
                    return;
                }

                AvlNode<T> parent = node.Parent;

                if (parent != null)
                {
                    balance = parent.Left == node ? -1 : 1;
                }

                node = parent;
            }
        }

        /// <summary>
        /// Replace a node
        /// </summary>
        /// <param name="target">The node to replace</param>
        /// <param name="source">The replacing node</param>
        private static void Replace(AvlNode<T> target, AvlNode<T> source)
        {
            AvlNode<T> left = source.Left;
            AvlNode<T> right = source.Right;

            target.Balance = source.Balance;
            target.Item = source.Item;
            target.Left = left;
            target.Right = right;

            if (left != null)
            {
                left.Parent = target;
            }

            if (right != null)
            {
                right.Parent = target;
            }
        }

        /// <summary>
        /// Counts the nodes recursively
        /// </summary>
        /// <param name="node">A node in the tree</param>
        /// <returns>A count of nodes</returns>
        private int RecursiveCount(AvlNode<T> node)
        {
            if (node == null)
            {
                return 0;
            }

            return 1 + this.RecursiveCount(node.Left) + this.RecursiveCount(node.Right);
        }

        /// <summary>
        /// Find child max height
        /// </summary>
        /// <param name="node">A node in the tree</param>
        /// <returns>The height</returns>
        private int RecursiveGetChildMaxHeight(AvlNode<T> node)
        {
            if (node == null)
            {
                return 0;
            }

            int leftHeight = 0;
            if (node.Left != null)
            {
                leftHeight = this.RecursiveGetChildMaxHeight(node.Left);
            }

            int rightHeight = 0;
            if (node.Right != null)
            {
                rightHeight = this.RecursiveGetChildMaxHeight(node.Right);
            }

            return 1 + Math.Max(leftHeight, rightHeight);
        }
    }
}

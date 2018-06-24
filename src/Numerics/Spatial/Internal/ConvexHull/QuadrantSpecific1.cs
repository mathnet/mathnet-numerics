using System;
using System.Linq;
using MathNet.Numerics.Spatial.Internal.AvlTreeSet;

namespace MathNet.Numerics.Spatial.Internal.ConvexHull
{
    /// <summary>
    /// Class to process quadrant 1
    /// </summary>
    internal class QuadrantSpecific1 : Quadrant
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QuadrantSpecific1"/> class.
        /// </summary>
        /// <param name="listOfPoint">a list of points</param>
        /// <param name="comparer">a comparer</param>
        internal QuadrantSpecific1(MutablePoint[] listOfPoint, Func<MutablePoint, MutablePoint, int> comparer)
            : base(listOfPoint, new QComparer(comparer))
        {
        }

        /// <summary>
        /// Check if we can quickly reject a point
        /// </summary>
        /// <param name="point">a point</param>
        /// <param name="pointHull">a point on the hull</param>
        /// <returns>True if can quickly reject; otherwise false</returns>
        //// [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool CanQuickReject(ref MutablePoint point, ref MutablePoint pointHull)
        {
            return point.X <= pointHull.X && point.Y <= pointHull.Y;
        }

        /// <summary>
        /// Iterate over each points to see if we can add it has a ConvexHull point.
        /// It is specific by Quadrant to improve efficiency.
        /// </summary>
        /// <param name="point">a point</param>
        //// [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void ProcessPoint(ref MutablePoint point)
        {
            this.CurrentNode = this.Root;
            AvlNode<MutablePoint> currentPrevious = null;
            AvlNode<MutablePoint> currentNext = null;

            while (this.CurrentNode != null)
            {
                var insertionSide = Side.Unknown;
                if (point.X > this.CurrentNode.Item.X)
                {
                    if (this.CurrentNode.Left != null)
                    {
                        this.CurrentNode = this.CurrentNode.Left;
                        continue;
                    }

                    currentPrevious = this.CurrentNode.GetPreviousNode();
                    if (CanQuickReject(ref point, ref currentPrevious.Item))
                    {
                        return;
                    }

                    if (!this.IsPointToTheRightOfOthers(currentPrevious.Item, this.CurrentNode.Item, point))
                    {
                        return;
                    }

                    // Ensure to have no duplicate
                    if (this.CurrentNode.Item == point)
                    {
                        return;
                    }

                    insertionSide = Side.Left;
                }
                else if (point.X < this.CurrentNode.Item.X)
                {
                    if (this.CurrentNode.Right != null)
                    {
                        this.CurrentNode = this.CurrentNode.Right;
                        continue;
                    }

                    currentNext = this.CurrentNode.GetNextNode();
                    if (CanQuickReject(ref point, ref currentNext.Item))
                    {
                        return;
                    }

                    if (!this.IsPointToTheRightOfOthers(this.CurrentNode.Item, currentNext.Item, point))
                    {
                        return;
                    }

                    // Ensure to have no duplicate
                    if (this.CurrentNode.Item == point)
                    {
                        return;
                    }

                    insertionSide = Side.Right;
                }
                else
                {
                    if (point.Y <= this.CurrentNode.Item.Y)
                    {
                        return; // invalid point
                    }

                    // Replace CurrentNode point with point
                    this.CurrentNode.Item = point;

                    this.InvalidateNeighbors(this.CurrentNode.GetPreviousNode(), this.CurrentNode, this.CurrentNode.GetNextNode());
                    return;
                }

                // We should insert the point
                // Try to optimize and verify if can replace a node instead insertion to minimize tree balancing
                if (insertionSide == Side.Right)
                {
                    currentPrevious = this.CurrentNode.GetPreviousNode();
                    if (currentPrevious != null && !this.IsPointToTheRightOfOthers(currentPrevious.Item, point, this.CurrentNode.Item))
                    {
                        this.CurrentNode.Item = point;
                        this.InvalidateNeighbors(currentPrevious, this.CurrentNode, currentNext);
                        return;
                    }

                    var nextNext = currentNext.GetNextNode();
                    if (nextNext != null && !this.IsPointToTheRightOfOthers(point, nextNext.Item, currentNext.Item))
                    {
                        currentNext.Item = point;
                        this.InvalidateNeighbors(null, currentNext, nextNext);
                        return;
                    }
                }
                else
                {
                    // Left
                    currentNext = this.CurrentNode.GetNextNode();
                    if (currentNext != null && !this.IsPointToTheRightOfOthers(point, currentNext.Item, this.CurrentNode.Item))
                    {
                        this.CurrentNode.Item = point;
                        this.InvalidateNeighbors(currentPrevious, this.CurrentNode, currentNext);
                        return;
                    }

                    var previousPrevious = currentPrevious.GetPreviousNode();
                    if (previousPrevious != null && !this.IsPointToTheRightOfOthers(previousPrevious.Item, point, currentPrevious.Item))
                    {
                        currentPrevious.Item = point;
                        this.InvalidateNeighbors(previousPrevious, currentPrevious, null);
                        return;
                    }
                }

                // Should insert but no invalidation is required. (That's why we need to insert... can't replace an adjacent neighbor)
                AvlNode<MutablePoint> newNode = new AvlNode<MutablePoint>();
                if (insertionSide == Side.Right)
                {
                    newNode.Parent = this.CurrentNode;
                    newNode.Item = point;
                    this.CurrentNode.Right = newNode;
                    this.AddBalance(newNode.Parent, -1);
                }
                else
                {
                    // Left
                    newNode.Parent = this.CurrentNode;
                    newNode.Item = point;
                    this.CurrentNode.Left = newNode;
                    this.AddBalance(newNode.Parent, 1);
                }
            }
        }

        /// <inheritdoc />
        protected override void SetQuadrantLimits()
        {
            MutablePoint firstPoint = this.ListOfPoint.First();

            double rightX = firstPoint.X;
            double rightY = firstPoint.Y;

            double topX = rightX;
            double topY = rightY;

            foreach (var point in this.ListOfPoint)
            {
                if (point.X >= rightX)
                {
                    if (point.X == rightX)
                    {
                        if (point.Y > rightY)
                        {
                            rightY = point.Y;
                        }
                    }
                    else
                    {
                        rightX = point.X;
                        rightY = point.Y;
                    }
                }

                if (point.Y >= topY)
                {
                    if (point.Y == topY)
                    {
                        if (point.X > topX)
                        {
                            topX = point.X;
                        }
                    }
                    else
                    {
                        topX = point.X;
                        topY = point.Y;
                    }
                }
            }

            this.FirstPoint = new MutablePoint(rightX, rightY);
            this.LastPoint = new MutablePoint(topX, topY);
            this.RootPoint = new MutablePoint(topX, rightY);
        }

        /// <inheritdoc />
        //// [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool IsGoodQuadrantForPoint(MutablePoint pt)
        {
            if (pt.X > this.RootPoint.X && pt.Y > this.RootPoint.Y)
            {
                return true;
            }

            return false;
        }
    }
}

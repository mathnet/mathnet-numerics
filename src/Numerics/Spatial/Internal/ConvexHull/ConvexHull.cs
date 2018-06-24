using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MathNet.Numerics.Spatial.Euclidean2D;

namespace MathNet.Numerics.Spatial.Internal.ConvexHull
{
    /// <summary>
    /// An implementation of the work of Lui, Chen and Ouellet for solving the convex hull problem
    /// https://www.codeproject.com/Articles/1210225/Fast-and-improved-D-Convex-Hull-algorithm-and-its
    /// <para>
    ///  Quadrant: Q2 | Q1
    ///            -------
    ///            Q3 | Q4
    /// </para>
    /// </summary>
    internal class ConvexHull
    {
        /// <summary>
        /// First quadrant
        /// </summary>
        private Quadrant q1;

        /// <summary>
        /// Second quadrant
        /// </summary>
        private Quadrant q2;

        /// <summary>
        /// Third quadrant
        /// </summary>
        private Quadrant q3;

        /// <summary>
        /// Fourth quadrant
        /// </summary>
        private Quadrant q4;

        /// <summary>
        /// Initial list of points
        /// </summary>
        private MutablePoint[] listOfPoint;

        /// <summary>
        /// A value indicating if the graph needs closing
        /// </summary>
        private bool shouldCloseTheGraph;

        /// <summary>
        /// A lock object
        /// </summary>
        private object findLimitFinalLock = new object();

        /// <summary>
        /// A limit
        /// </summary>
        private Limit limit = default(Limit);

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvexHull"/> class.
        /// </summary>
        /// <param name="listOfPoint">a list of points</param>
        /// <param name="shouldCloseTheGraph">True if the graph should be closed; otherwise false</param>
        /// <param name="initialResultGuessSize">An estimate for the initial size of the result set</param>
        public ConvexHull(IEnumerable<Point2D> listOfPoint, bool shouldCloseTheGraph = true, int initialResultGuessSize = 0)
        {
            List<MutablePoint> l = new List<MutablePoint>();
            foreach (var point in listOfPoint)
            {
                MutablePoint p = new MutablePoint(point.X, point.Y);
                l.Add(p);
            }

            this.Init(l.ToArray(), shouldCloseTheGraph);
        }

        /// <summary>
        /// Returns the results as an array of points
        /// </summary>
        /// <returns>The results</returns>
        public Point2D[] GetResultsAsArrayOfPoint()
        {
            if (this.listOfPoint == null || !this.listOfPoint.Any())
            {
                return new Point2D[0];
            }

            int countOfPoints = this.q1.Count + this.q2.Count + this.q3.Count + this.q4.Count;

            if (this.q1.LastPoint == this.q2.FirstPoint)
            {
                countOfPoints--;
            }

            if (this.q2.LastPoint == this.q3.FirstPoint)
            {
                countOfPoints--;
            }

            if (this.q3.LastPoint == this.q4.FirstPoint)
            {
                countOfPoints--;
            }

            if (this.q4.LastPoint == this.q1.FirstPoint)
            {
                countOfPoints--;
            }

            // Case where there is only one point
            if (countOfPoints == 0)
            {
                return new Point2D[] { new Point2D(this.q1.FirstPoint.X, this.q1.FirstPoint.Y) };
            }

            if (this.shouldCloseTheGraph)
            {
                countOfPoints++;
            }

            Point2D[] results = new Point2D[countOfPoints];

            int resultIndex = -1;

            if (this.q1.FirstPoint != this.q4.LastPoint)
            {
                foreach (MutablePoint pt in this.q1)
                {
                    results[++resultIndex] = new Point2D(pt.X, pt.Y);
                }
            }
            else
            {
                var enumerator = this.q1.GetEnumerator();
                enumerator.Reset();
                if (enumerator.MoveNext())
                {
                    // Skip first (same as the last one as quadrant 4
                    while (enumerator.MoveNext())
                    {
                        results[++resultIndex] = new Point2D(enumerator.Current.X, enumerator.Current.Y);
                    }
                }
            }

            if (this.q2.Count == 1)
            {
                if (this.q2.FirstPoint != this.q1.LastPoint)
                {
                    results[++resultIndex] = new Point2D(this.q2.FirstPoint.X, this.q2.FirstPoint.Y);
                }
            }
            else
            {
                var enumerator = this.q2.GetEnumerator();
                enumerator.Reset();
                if (enumerator.MoveNext())
                {
                    if (enumerator.Current != this.q1.LastPoint)
                    {
                        results[++resultIndex] = new Point2D(enumerator.Current.X, enumerator.Current.Y);
                    }

                    while (enumerator.MoveNext())
                    {
                        results[++resultIndex] = new Point2D(enumerator.Current.X, enumerator.Current.Y);
                    }
                }
            }

            if (this.q3.Count == 1)
            {
                if (this.q3.FirstPoint != this.q2.LastPoint)
                {
                    results[++resultIndex] = new Point2D(this.q3.FirstPoint.X, this.q3.FirstPoint.Y);
                }
            }
            else
            {
                var enumerator = this.q3.GetEnumerator();
                enumerator.Reset();
                if (enumerator.MoveNext())
                {
                    if (enumerator.Current != this.q2.LastPoint)
                    {
                        results[++resultIndex] = new Point2D(enumerator.Current.X, enumerator.Current.Y);
                    }

                    while (enumerator.MoveNext())
                    {
                        results[++resultIndex] = new Point2D(enumerator.Current.X, enumerator.Current.Y);
                    }
                }
            }

            if (this.q4.Count == 1)
            {
                if (this.q4.FirstPoint != this.q3.LastPoint)
                {
                    results[++resultIndex] = new Point2D(this.q4.FirstPoint.X, this.q4.FirstPoint.Y);
                }
            }
            else
            {
                var enumerator = this.q4.GetEnumerator();
                enumerator.Reset();
                if (enumerator.MoveNext())
                {
                    if (enumerator.Current != this.q3.LastPoint)
                    {
                        results[++resultIndex] = new Point2D(enumerator.Current.X, enumerator.Current.Y);
                    }

                    while (enumerator.MoveNext())
                    {
                        results[++resultIndex] = new Point2D(enumerator.Current.X, enumerator.Current.Y);
                    }
                }
            }

            if (this.shouldCloseTheGraph && results[resultIndex] != results[0])
            {
                results[++resultIndex] = results[0];
            }

            return results;
        }

        /// <summary>
        /// Calculate the Convex Hull
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Simple struct clearly named")]
        public void CalcConvexHull()
        {
            if (this.IsZeroData())
            {
                return;
            }

            this.SetQuadrantLimitsOneThread();

            this.q1.Prepare();
            this.q2.Prepare();
            this.q3.Prepare();
            this.q4.Prepare();

            MutablePoint q1Root = this.q1.RootPoint;
            MutablePoint q2Root = this.q2.RootPoint;
            MutablePoint q3Root = this.q3.RootPoint;
            MutablePoint q4Root = this.q4.RootPoint;

            // Main Loop to extract ConvexHullPoints
            MutablePoint[] points = this.listOfPoint;
            int index = 0;
            int pointCount = points.Length;

            if (points != null)
            {
                MutablePoint point;

                if (this.IsQuadrantAreDisjoint())
                {
                    Q1First:
                    if (index < pointCount)
                    {
                        point = points[index++];

                        if (point.X > q1Root.X && point.Y > q1Root.Y)
                        {
                            this.q1.ProcessPoint(ref point);
                            goto Q1First;
                        }

                        if (point.X < q2Root.X && point.Y > q2Root.Y)
                        {
                            this.q2.ProcessPoint(ref point);
                            goto Q2First;
                        }

                        if (point.X < q3Root.X && point.Y < q3Root.Y)
                        {
                            this.q3.ProcessPoint(ref point);
                            goto Q3First;
                        }

                        if (point.X > q4Root.X && point.Y < q4Root.Y)
                        {
                            this.q4.ProcessPoint(ref point);
                            goto Q4First;
                        }

                        goto Q1First;
                    }
                    else
                    {
                        goto End;
                    }

                    Q2First:
                    if (index < pointCount)
                    {
                        point = points[index++];

                        if (point.X < q2Root.X && point.Y > q2Root.Y)
                        {
                            this.q2.ProcessPoint(ref point);
                            goto Q2First;
                        }

                        if (point.X < q3Root.X && point.Y < q3Root.Y)
                        {
                            this.q3.ProcessPoint(ref point);
                            goto Q3First;
                        }

                        if (point.X > q4Root.X && point.Y < q4Root.Y)
                        {
                            this.q4.ProcessPoint(ref point);
                            goto Q4First;
                        }

                        if (point.X > q1Root.X && point.Y > q1Root.Y)
                        {
                            this.q1.ProcessPoint(ref point);
                            goto Q1First;
                        }

                        goto Q2First;
                    }
                    else
                    {
                        goto End;
                    }

                    Q3First:
                    if (index < pointCount)
                    {
                        point = points[index++];

                        if (point.X < q3Root.X && point.Y < q3Root.Y)
                        {
                            this.q3.ProcessPoint(ref point);
                            goto Q3First;
                        }

                        if (point.X > q4Root.X && point.Y < q4Root.Y)
                        {
                            this.q4.ProcessPoint(ref point);
                            goto Q4First;
                        }

                        if (point.X > q1Root.X && point.Y > q1Root.Y)
                        {
                            this.q1.ProcessPoint(ref point);
                            goto Q1First;
                        }

                        if (point.X < q2Root.X && point.Y > q2Root.Y)
                        {
                            this.q2.ProcessPoint(ref point);
                            goto Q2First;
                        }

                        goto Q3First;
                    }
                    else
                    {
                        goto End;
                    }

                    Q4First:
                    if (index < pointCount)
                    {
                        point = points[index++];

                        if (point.X > q4Root.X && point.Y < q4Root.Y)
                        {
                            this.q4.ProcessPoint(ref point);
                            goto Q4First;
                        }

                        if (point.X > q1Root.X && point.Y > q1Root.Y)
                        {
                            this.q1.ProcessPoint(ref point);
                            goto Q1First;
                        }

                        if (point.X < q2Root.X && point.Y > q2Root.Y)
                        {
                            this.q2.ProcessPoint(ref point);
                            goto Q2First;
                        }

                        if (point.X < q3Root.X && point.Y < q3Root.Y)
                        {
                            this.q3.ProcessPoint(ref point);
                            goto Q3First;
                        }

                        goto Q4First;
                    }
                    else
                    {
                        goto End;
                    }
                }
                else
                {
                    // Not disjoint
                    Q1First:
                    if (index < pointCount)
                    {
                        point = points[index++];

                        if (point.X > q1Root.X && point.Y > q1Root.Y)
                        {
                            if (IsPointToTheRightOfOthers(this.q1.FirstPoint, this.q1.LastPoint, point))
                            {
                                this.q1.ProcessPoint(ref point);
                                goto Q1First;
                            }

                            if (point.X < q3Root.X && point.Y < q3Root.Y)
                            {
                                if (IsPointToTheRightOfOthers(this.q3.FirstPoint, this.q3.LastPoint, point))
                                {
                                    this.q3.ProcessPoint(ref point);
                                }

                                goto Q3First;
                            }

                            goto Q1First;
                        }

                        if (point.X < q2Root.X && point.Y > q2Root.Y)
                        {
                            if (IsPointToTheRightOfOthers(this.q2.FirstPoint, this.q2.LastPoint, point))
                            {
                                this.q2.ProcessPoint(ref point);
                                goto Q2First;
                            }

                            if (point.X > q4Root.X && point.Y < q4Root.Y)
                            {
                                if (IsPointToTheRightOfOthers(this.q4.FirstPoint, this.q4.LastPoint, point))
                                {
                                    this.q4.ProcessPoint(ref point);
                                }

                                goto Q4First;
                            }

                            goto Q2First;
                        }

                        if (point.X < q3Root.X && point.Y < q3Root.Y)
                        {
                            if (IsPointToTheRightOfOthers(this.q3.FirstPoint, this.q3.LastPoint, point))
                            {
                                this.q3.ProcessPoint(ref point);
                            }

                            goto Q3First;
                        }
                        else if (point.X > q4Root.X && point.Y < q4Root.Y)
                        {
                            if (IsPointToTheRightOfOthers(this.q4.FirstPoint, this.q4.LastPoint, point))
                            {
                                this.q4.ProcessPoint(ref point);
                            }

                            goto Q4First;
                        }

                        goto Q1First;
                    }
                    else
                    {
                        goto End;
                    }

                    Q2First:
                    if (index < pointCount)
                    {
                        point = points[index++];

                        if (point.X < q2Root.X && point.Y > q2Root.Y)
                        {
                            if (IsPointToTheRightOfOthers(this.q2.FirstPoint, this.q2.LastPoint, point))
                            {
                                this.q2.ProcessPoint(ref point);
                                goto Q2First;
                            }

                            if (point.X > q4Root.X && point.Y < q4Root.Y)
                            {
                                if (IsPointToTheRightOfOthers(this.q4.FirstPoint, this.q4.LastPoint, point))
                                {
                                    this.q4.ProcessPoint(ref point);
                                }

                                goto Q4First;
                            }

                            goto Q2First;
                        }

                        if (point.X < q3Root.X && point.Y < q3Root.Y)
                        {
                            if (IsPointToTheRightOfOthers(this.q3.FirstPoint, this.q3.LastPoint, point))
                            {
                                this.q3.ProcessPoint(ref point);
                                goto Q3First;
                            }

                            if (point.X > q1Root.X && point.Y > q1Root.Y)
                            {
                                if (IsPointToTheRightOfOthers(this.q1.FirstPoint, this.q1.LastPoint, point))
                                {
                                    this.q1.ProcessPoint(ref point);
                                }

                                goto Q1First;
                            }

                            goto Q3First;
                        }

                        if (point.X > q4Root.X && point.Y < q4Root.Y)
                        {
                            if (IsPointToTheRightOfOthers(this.q4.FirstPoint, this.q4.LastPoint, point))
                            {
                                this.q4.ProcessPoint(ref point);
                            }

                            goto Q4First;
                        }
                        else if (point.X > q1Root.X && point.Y > q1Root.Y)
                        {
                            if (IsPointToTheRightOfOthers(this.q1.FirstPoint, this.q1.LastPoint, point))
                            {
                                this.q1.ProcessPoint(ref point);
                            }

                            goto Q1First;
                        }

                        goto Q2First;
                    }
                    else
                    {
                        goto End;
                    }

                    Q3First:
                    if (index < pointCount)
                    {
                        point = points[index++];

                        if (point.X < q3Root.X && point.Y < q3Root.Y)
                        {
                            if (IsPointToTheRightOfOthers(this.q3.FirstPoint, this.q3.LastPoint, point))
                            {
                                this.q3.ProcessPoint(ref point);
                                goto Q3First;
                            }

                            if (point.X > q1Root.X && point.Y > q1Root.Y)
                            {
                                if (IsPointToTheRightOfOthers(this.q1.FirstPoint, this.q1.LastPoint, point))
                                {
                                    this.q1.ProcessPoint(ref point);
                                }

                                goto Q1First;
                            }

                            goto Q3First;
                        }

                        if (point.X > q4Root.X && point.Y < q4Root.Y)
                        {
                            if (IsPointToTheRightOfOthers(this.q4.FirstPoint, this.q4.LastPoint, point))
                            {
                                this.q4.ProcessPoint(ref point);
                                goto Q4First;
                            }

                            if (point.X < q2Root.X && point.Y > q2Root.Y)
                            {
                                if (IsPointToTheRightOfOthers(this.q2.FirstPoint, this.q2.LastPoint, point))
                                {
                                    this.q2.ProcessPoint(ref point);
                                }

                                goto Q2First;
                            }

                            goto Q4First;
                        }

                        if (point.X > q1Root.X && point.Y > q1Root.Y)
                        {
                            if (IsPointToTheRightOfOthers(this.q1.FirstPoint, this.q1.LastPoint, point))
                            {
                                this.q1.ProcessPoint(ref point);
                                goto Q1First;
                            }
                        }
                        else if (point.X < q2Root.X && point.Y > q2Root.Y)
                        {
                            if (IsPointToTheRightOfOthers(this.q2.FirstPoint, this.q2.LastPoint, point))
                            {
                                this.q2.ProcessPoint(ref point);
                                goto Q2First;
                            }
                        }

                        goto Q3First;
                    }
                    else
                    {
                        goto End;
                    }

                    Q4First:
                    if (index < pointCount)
                    {
                        point = points[index++];

                        if (point.X > q4Root.X && point.Y < q4Root.Y)
                        {
                            if (IsPointToTheRightOfOthers(this.q4.FirstPoint, this.q4.LastPoint, point))
                            {
                                this.q4.ProcessPoint(ref point);
                                goto Q4First;
                            }

                            if (point.X < q2Root.X && point.Y > q2Root.Y)
                            {
                                if (IsPointToTheRightOfOthers(this.q2.FirstPoint, this.q2.LastPoint, point))
                                {
                                    this.q2.ProcessPoint(ref point);
                                }

                                goto Q2First;
                            }

                            goto Q4First;
                        }

                        if (point.X > q1Root.X && point.Y > q1Root.Y)
                        {
                            if (IsPointToTheRightOfOthers(this.q1.FirstPoint, this.q1.LastPoint, point))
                            {
                                this.q1.ProcessPoint(ref point);
                                goto Q1First;
                            }

                            if (point.X < q3Root.X && point.Y < q3Root.Y)
                            {
                                if (IsPointToTheRightOfOthers(this.q3.FirstPoint, this.q3.LastPoint, point))
                                {
                                    this.q3.ProcessPoint(ref point);
                                }

                                goto Q3First;
                            }

                            goto Q1First;
                        }

                        if (point.X < q3Root.X && point.Y < q3Root.Y)
                        {
                            if (IsPointToTheRightOfOthers(this.q3.FirstPoint, this.q3.LastPoint, point))
                            {
                                this.q3.ProcessPoint(ref point);
                                goto Q3First;
                            }
                        }

                        if (point.X < q2Root.X && point.Y > q2Root.Y)
                        {
                            if (IsPointToTheRightOfOthers(this.q2.FirstPoint, this.q2.LastPoint, point))
                            {
                                this.q2.ProcessPoint(ref point);
                                goto Q2First;
                            }
                        }

                        goto Q4First;
                    }
                    else
                    {
                        goto End;
                    }
                }

                End:
                {
                }
            }
        }

        /// <summary>
        /// True if the point to check is to the right of the other provided points
        /// </summary>
        /// <param name="p1">The first point</param>
        /// <param name="p2">The second point</param>
        /// <param name="pointToCheck">The point to check</param>
        /// <returns>True if the point is to rhe right of the other; Otherwise false</returns>
        //// [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool IsPointToTheRightOfOthers(MutablePoint p1, MutablePoint p2, MutablePoint pointToCheck)
        {
            return ((p2.X - p1.X) * (pointToCheck.Y - p1.Y)) - ((p2.Y - p1.Y) * (pointToCheck.X - p1.X)) < 0;
        }

        /// <summary>
        /// Set quadrant limits
        /// </summary>
        private void SetQuadrantLimitsOneThread()
        {
            MutablePoint pointFirst = this.listOfPoint.First();

            // Find the quadrant limits (maximum x and y)
            double right, topLeft, topRight, left, bottomLeft, bottomRight;
            right = topLeft = topRight = left = bottomLeft = bottomRight = pointFirst.X;

            double top, rightTop, rightBottom, bottom, leftTop, leftBottom;
            top = rightTop = rightBottom = bottom = leftTop = leftBottom = pointFirst.Y;

            foreach (MutablePoint pt in this.listOfPoint)
            {
                if (pt.X >= right)
                {
                    if (pt.X == right)
                    {
                        if (pt.Y > rightTop)
                        {
                            rightTop = pt.Y;
                        }
                        else
                        {
                            if (pt.Y < rightBottom)
                            {
                                rightBottom = pt.Y;
                            }
                        }
                    }
                    else
                    {
                        right = pt.X;
                        rightTop = rightBottom = pt.Y;
                    }
                }

                if (pt.X <= left)
                {
                    if (pt.X == left)
                    {
                        if (pt.Y > leftTop)
                        {
                            leftTop = pt.Y;
                        }
                        else
                        {
                            if (pt.Y < leftBottom)
                            {
                                leftBottom = pt.Y;
                            }
                        }
                    }
                    else
                    {
                        left = pt.X;
                        leftBottom = leftTop = pt.Y;
                    }
                }

                if (pt.Y >= top)
                {
                    if (pt.Y == top)
                    {
                        if (pt.X < topLeft)
                        {
                            topLeft = pt.X;
                        }
                        else
                        {
                            if (pt.X > topRight)
                            {
                                topRight = pt.X;
                            }
                        }
                    }
                    else
                    {
                        top = pt.Y;
                        topLeft = topRight = pt.X;
                    }
                }

                if (pt.Y <= bottom)
                {
                    if (pt.Y == bottom)
                    {
                        if (pt.X < bottomLeft)
                        {
                            bottomLeft = pt.X;
                        }
                        else
                        {
                            if (pt.X > bottomRight)
                            {
                                bottomRight = pt.X;
                            }
                        }
                    }
                    else
                    {
                        bottom = pt.Y;
                        bottomRight = bottomLeft = pt.X;
                    }
                }

                this.q1.FirstPoint = new MutablePoint(right, rightTop);
                this.q1.LastPoint = new MutablePoint(topRight, top);
                this.q1.RootPoint = new MutablePoint(topRight, rightTop);

                this.q2.FirstPoint = new MutablePoint(topLeft, top);
                this.q2.LastPoint = new MutablePoint(left, leftTop);
                this.q2.RootPoint = new MutablePoint(topLeft, leftTop);

                this.q3.FirstPoint = new MutablePoint(left, leftBottom);
                this.q3.LastPoint = new MutablePoint(bottomLeft, bottom);
                this.q3.RootPoint = new MutablePoint(bottomLeft, leftBottom);

                this.q4.FirstPoint = new MutablePoint(bottomRight, bottom);
                this.q4.LastPoint = new MutablePoint(right, rightBottom);
                this.q4.RootPoint = new MutablePoint(bottomRight, rightBottom);
            }
        }

        /*
        // ******************************************************************
        // For usage of Parallel func, I highly suggest: Stephen Toub: Patterns of parallel programming ==> Just Awsome !!!
        // But its only my own fault if I'm not using it at its full potential...
        private void SetQuadrantLimitsUsingAllThreads()
        {
            MutablePoint pt = this._listOfPoint.First();
            _limit = new Limit(pt);

            int coreCount = Environment.ProcessorCount;

            Task[] tasks = new Task[coreCount];
            for (int n = 0; n < tasks.Length; n++)
            {
                int nLocal = n; // Prevent Lambda internal closure error.
                tasks[n] = Task.Factory.StartNew(() =>
                {
                    Limit limit = _limit.Copy();
                    FindLimits(_listOfPoint, nLocal, coreCount, limit);
                    AggregateLimits(limit);
                });
            }
            Task.WaitAll(tasks);

            _q1.FirstPoint = _limit.Q1Right;
            _q1.LastPoint = _limit.Q1Top;
            _q2.FirstPoint = _limit.Q2Top;
            _q2.LastPoint = _limit.Q2Left;
            _q3.FirstPoint = _limit.Q3Left;
            _q3.LastPoint = _limit.Q3Bottom;
            _q4.FirstPoint = _limit.Q4Bottom;
            _q4.LastPoint = _limit.Q4Right;

            _q1.RootPoint = new MutablePoint(_q1.LastPoint.X, _q1.FirstPoint.Y);
            _q2.RootPoint = new MutablePoint(_q2.FirstPoint.X, _q2.LastPoint.Y);
            _q3.RootPoint = new MutablePoint(_q3.LastPoint.X, _q3.FirstPoint.Y);
            _q4.RootPoint = new MutablePoint(_q4.FirstPoint.X, _q4.LastPoint.Y);
        }
        */

        /// <summary>
        /// Find the limits
        /// </summary>
        /// <param name="listOfPoint">a list of points</param>
        /// <param name="start">the start point</param>
        /// <param name="offset">the offset</param>
        /// <param name="limit">the limit</param>
        /// <returns>A limit</returns>
        private Limit FindLimits(MutablePoint[] listOfPoint, int start, int offset, Limit limit)
        {
            for (int index = start; index < listOfPoint.Length; index += offset)
            {
                MutablePoint pt = listOfPoint[index];

                double x = pt.X;
                double y = pt.Y;

                // Top
                if (y >= limit.Q2Top.Y)
                {
                    if (y == limit.Q2Top.Y)
                    {
                        // Special
                        if (y == limit.Q1Top.Y)
                        {
                            if (x < limit.Q2Top.X)
                            {
                                limit.Q2Top.X = x;
                            }
                            else if (x > limit.Q1Top.X)
                            {
                                limit.Q1Top.X = x;
                            }
                        }
                        else
                        {
                            if (x < limit.Q2Top.X)
                            {
                                limit.Q1Top.X = limit.Q2Top.X;
                                limit.Q1Top.Y = limit.Q2Top.Y;

                                limit.Q2Top.X = x;
                            }
                            else if (x > limit.Q1Top.X)
                            {
                                limit.Q1Top.X = x;
                                limit.Q1Top.Y = y;
                            }
                        }
                    }
                    else
                    {
                        limit.Q2Top.X = x;
                        limit.Q2Top.Y = y;
                    }
                }

                // Bottom
                if (y <= limit.Q3Bottom.Y)
                {
                    if (y == limit.Q3Bottom.Y)
                    {
                        // Special
                        if (y == limit.Q4Bottom.Y)
                        {
                            if (x < limit.Q3Bottom.X)
                            {
                                limit.Q3Bottom.X = x;
                            }
                            else if (x > limit.Q4Bottom.X)
                            {
                                limit.Q4Bottom.X = x;
                            }
                        }
                        else
                        {
                            if (x < limit.Q3Bottom.X)
                            {
                                limit.Q4Bottom.X = limit.Q3Bottom.X;
                                limit.Q4Bottom.Y = limit.Q3Bottom.Y;

                                limit.Q3Bottom.X = x;
                            }
                            else if (x > limit.Q3Bottom.X)
                            {
                                limit.Q4Bottom.X = x;
                                limit.Q4Bottom.Y = y;
                            }
                        }
                    }
                    else
                    {
                        limit.Q3Bottom.X = x;
                        limit.Q3Bottom.Y = y;
                    }
                }

                // Right
                if (x >= limit.Q4Right.X)
                {
                    if (x == limit.Q4Right.X)
                    {
                        // Special
                        if (x == limit.Q1Right.X)
                        {
                            if (y < limit.Q4Right.Y)
                            {
                                limit.Q4Right.Y = y;
                            }
                            else if (y > limit.Q1Right.Y)
                            {
                                limit.Q1Right.Y = y;
                            }
                        }
                        else
                        {
                            if (y < limit.Q4Right.Y)
                            {
                                limit.Q1Right.X = limit.Q4Right.X;
                                limit.Q1Right.Y = limit.Q4Right.Y;

                                limit.Q4Right.Y = y;
                            }
                            else if (y > limit.Q1Right.Y)
                            {
                                limit.Q1Right.X = x;
                                limit.Q1Right.Y = y;
                            }
                        }
                    }
                    else
                    {
                        limit.Q4Right.X = x;
                        limit.Q4Right.Y = y;
                    }
                }

                // Left
                if (x <= limit.Q3Left.X)
                {
                    if (x == limit.Q3Left.X)
                    {
                        // Special
                        if (x == limit.Q2Left.X)
                        {
                            if (y < limit.Q3Left.Y)
                            {
                                limit.Q3Left.Y = y;
                            }
                            else if (y > limit.Q2Left.Y)
                            {
                                limit.Q2Left.Y = y;
                            }
                        }
                        else
                        {
                            if (y < limit.Q3Left.Y)
                            {
                                limit.Q2Left.X = limit.Q3Left.X;
                                limit.Q2Left.Y = limit.Q3Left.Y;

                                limit.Q3Left.Y = y;
                            }
                            else if (y > limit.Q2Left.Y)
                            {
                                limit.Q2Left.X = x;
                                limit.Q2Left.Y = y;
                            }
                        }
                    }
                    else
                    {
                        limit.Q3Left.X = x;
                        limit.Q3Left.Y = y;
                    }
                }

                if (limit.Q2Left.X != limit.Q3Left.X)
                {
                    limit.Q2Left.X = limit.Q3Left.X;
                    limit.Q2Left.Y = limit.Q3Left.Y;
                }

                if (limit.Q1Right.X != limit.Q4Right.X)
                {
                    limit.Q1Right.X = limit.Q4Right.X;
                    limit.Q1Right.Y = limit.Q4Right.Y;
                }

                if (limit.Q1Top.Y != limit.Q2Top.Y)
                {
                    limit.Q1Top.X = limit.Q2Top.X;
                    limit.Q1Top.Y = limit.Q2Top.Y;
                }

                if (limit.Q4Bottom.Y != limit.Q3Bottom.Y)
                {
                    limit.Q4Bottom.X = limit.Q3Bottom.X;
                    limit.Q4Bottom.Y = limit.Q3Bottom.Y;
                }
            }

            return limit;
        }

        /// <summary>
        /// Find limits
        /// </summary>
        /// <param name="point">a point</param>
        /// <param name="limit">a limit</param>
        /// <returns>The found limit</returns>
        private Limit FindLimits(MutablePoint point, Limit limit)
        {
            double x = point.X;
            double y = point.Y;

            // Top
            if (y >= limit.Q2Top.Y)
            {
                if (y == limit.Q2Top.Y)
                {
                    // Special
                    if (y == limit.Q1Top.Y)
                    {
                        if (x < limit.Q2Top.X)
                        {
                            limit.Q2Top.X = x;
                        }
                        else if (x > limit.Q1Top.X)
                        {
                            limit.Q1Top.X = x;
                        }
                    }
                    else
                    {
                        if (x < limit.Q2Top.X)
                        {
                            limit.Q1Top.X = limit.Q2Top.X;
                            limit.Q1Top.Y = limit.Q2Top.Y;

                            limit.Q2Top.X = x;
                        }
                        else if (x > limit.Q1Top.X)
                        {
                            limit.Q1Top.X = x;
                            limit.Q1Top.Y = y;
                        }
                    }
                }
                else
                {
                    limit.Q2Top.X = x;
                    limit.Q2Top.Y = y;
                }
            }

            // Bottom
            if (y <= limit.Q3Bottom.Y)
            {
                if (y == limit.Q3Bottom.Y)
                {
                    // Special
                    if (y == limit.Q4Bottom.Y)
                    {
                        if (x < limit.Q3Bottom.X)
                        {
                            limit.Q3Bottom.X = x;
                        }
                        else if (x > limit.Q4Bottom.X)
                        {
                            limit.Q4Bottom.X = x;
                        }
                    }
                    else
                    {
                        if (x < limit.Q3Bottom.X)
                        {
                            limit.Q4Bottom.X = limit.Q3Bottom.X;
                            limit.Q4Bottom.Y = limit.Q3Bottom.Y;

                            limit.Q3Bottom.X = x;
                        }
                        else if (x > limit.Q3Bottom.X)
                        {
                            limit.Q4Bottom.X = x;
                            limit.Q4Bottom.Y = y;
                        }
                    }
                }
                else
                {
                    limit.Q3Bottom.X = x;
                    limit.Q3Bottom.Y = y;
                }
            }

            // Right
            if (x >= limit.Q4Right.X)
            {
                if (x == limit.Q4Right.X)
                {
                    // Special
                    if (x == limit.Q1Right.X)
                    {
                        if (y < limit.Q4Right.Y)
                        {
                            limit.Q4Right.Y = y;
                        }
                        else if (y > limit.Q1Right.Y)
                        {
                            limit.Q1Right.Y = y;
                        }
                    }
                    else
                    {
                        if (y < limit.Q4Right.Y)
                        {
                            limit.Q1Right.X = limit.Q4Right.X;
                            limit.Q1Right.Y = limit.Q4Right.Y;

                            limit.Q4Right.Y = y;
                        }
                        else if (y > limit.Q1Right.Y)
                        {
                            limit.Q1Right.X = x;
                            limit.Q1Right.Y = y;
                        }
                    }
                }
                else
                {
                    limit.Q4Right.X = x;
                    limit.Q4Right.Y = y;
                }
            }

            // Left
            if (x <= limit.Q3Left.X)
            {
                if (x == limit.Q3Left.X)
                {
                    // Special
                    if (x == limit.Q2Left.X)
                    {
                        if (y < limit.Q3Left.Y)
                        {
                            limit.Q3Left.Y = y;
                        }
                        else if (y > limit.Q2Left.Y)
                        {
                            limit.Q2Left.Y = y;
                        }
                    }
                    else
                    {
                        if (y < limit.Q3Left.Y)
                        {
                            limit.Q2Left.X = limit.Q3Left.X;
                            limit.Q2Left.Y = limit.Q3Left.Y;

                            limit.Q3Left.Y = y;
                        }
                        else if (y > limit.Q2Left.Y)
                        {
                            limit.Q2Left.X = x;
                            limit.Q2Left.Y = y;
                        }
                    }
                }
                else
                {
                    limit.Q3Left.X = x;
                    limit.Q3Left.Y = y;
                }
            }

            if (limit.Q2Left.X != limit.Q3Left.X)
            {
                limit.Q2Left.X = limit.Q3Left.X;
                limit.Q2Left.Y = limit.Q3Left.Y;
            }

            if (limit.Q1Right.X != limit.Q4Right.X)
            {
                limit.Q1Right.X = limit.Q4Right.X;
                limit.Q1Right.Y = limit.Q4Right.Y;
            }

            if (limit.Q1Top.Y != limit.Q2Top.Y)
            {
                limit.Q1Top.X = limit.Q2Top.X;
                limit.Q1Top.Y = limit.Q2Top.Y;
            }

            if (limit.Q4Bottom.Y != limit.Q3Bottom.Y)
            {
                limit.Q4Bottom.X = limit.Q3Bottom.X;
                limit.Q4Bottom.Y = limit.Q3Bottom.Y;
            }

            return limit;
        }

        /// <summary>
        /// Set aggregate limits
        /// </summary>
        /// <param name="limit">A limit</param>
        private void AggregateLimits(Limit limit)
        {
            lock (this.findLimitFinalLock)
            {
                if (limit.Q1Right.X >= this.limit.Q1Right.X)
                {
                    if (limit.Q1Right.X == this.limit.Q1Right.X)
                    {
                        if (limit.Q1Right.Y > this.limit.Q1Right.Y)
                        {
                            this.limit.Q1Right = limit.Q1Right;
                        }
                    }
                    else
                    {
                        this.limit.Q1Right = limit.Q1Right;
                    }
                }

                if (limit.Q4Right.X > this.limit.Q4Right.X)
                {
                    if (limit.Q4Right.X == this.limit.Q4Right.X)
                    {
                        if (limit.Q4Right.Y < this.limit.Q4Right.Y)
                        {
                            this.limit.Q4Right = limit.Q4Right;
                        }
                    }
                    else
                    {
                        this.limit.Q4Right = limit.Q4Right;
                    }
                }

                if (limit.Q2Left.X < this.limit.Q2Left.X)
                {
                    if (limit.Q2Left.X == this.limit.Q2Left.X)
                    {
                        if (limit.Q2Left.Y > this.limit.Q2Left.Y)
                        {
                            this.limit.Q2Left = limit.Q2Left;
                        }
                    }
                    else
                    {
                        this.limit.Q2Left = limit.Q2Left;
                    }
                }

                if (limit.Q3Left.X < this.limit.Q3Left.X)
                {
                    if (limit.Q3Left.X == this.limit.Q3Left.X)
                    {
                        if (limit.Q3Left.Y > this.limit.Q3Left.Y)
                        {
                            this.limit.Q3Left = limit.Q3Left;
                        }
                    }
                    else
                    {
                        this.limit.Q3Left = limit.Q3Left;
                    }
                }

                if (limit.Q1Top.Y > this.limit.Q1Top.Y)
                {
                    if (limit.Q1Top.Y == this.limit.Q1Top.Y)
                    {
                        if (limit.Q1Top.X > this.limit.Q1Top.X)
                        {
                            this.limit.Q1Top = limit.Q1Top;
                        }
                    }
                    else
                    {
                        this.limit.Q1Top = limit.Q1Top;
                    }
                }

                if (limit.Q2Top.Y > this.limit.Q2Top.Y)
                {
                    if (limit.Q2Top.Y == this.limit.Q2Top.Y)
                    {
                        if (limit.Q2Top.X < this.limit.Q2Top.X)
                        {
                            this.limit.Q2Top = limit.Q2Top;
                        }
                    }
                    else
                    {
                        this.limit.Q2Top = limit.Q2Top;
                    }
                }

                if (limit.Q3Bottom.Y < this.limit.Q3Bottom.Y)
                {
                    if (limit.Q3Bottom.Y == this.limit.Q3Bottom.Y)
                    {
                        if (limit.Q3Bottom.X < this.limit.Q3Bottom.X)
                        {
                            this.limit.Q3Bottom = limit.Q3Bottom;
                        }
                    }
                    else
                    {
                        this.limit.Q3Bottom = limit.Q3Bottom;
                    }
                }

                if (limit.Q4Bottom.Y < this.limit.Q4Bottom.Y)
                {
                    if (limit.Q4Bottom.Y == this.limit.Q4Bottom.Y)
                    {
                        if (limit.Q4Bottom.X > this.limit.Q4Bottom.X)
                        {
                            this.limit.Q4Bottom = limit.Q4Bottom;
                        }
                    }
                    else
                    {
                        this.limit.Q4Bottom = limit.Q4Bottom;
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the data is empty
        /// </summary>
        /// <returns>True if no data</returns>
        private bool IsZeroData()
        {
            return this.listOfPoint == null || !this.listOfPoint.Any();
        }

        /// <summary>
        /// Returns true if the quadrants are disjointed
        /// </summary>
        /// <returns>True if Disjoint; otherwise false</returns>
        private bool IsQuadrantAreDisjoint()
        {
            if (IsPointToTheRightOfOthers(this.q1.FirstPoint, this.q1.LastPoint, this.q3.RootPoint))
            {
                return false;
            }

            if (IsPointToTheRightOfOthers(this.q2.FirstPoint, this.q2.LastPoint, this.q4.RootPoint))
            {
                return false;
            }

            if (IsPointToTheRightOfOthers(this.q3.FirstPoint, this.q3.LastPoint, this.q1.RootPoint))
            {
                return false;
            }

            if (IsPointToTheRightOfOthers(this.q4.FirstPoint, this.q4.LastPoint, this.q2.RootPoint))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Initializes the class
        /// </summary>
        /// <param name="listOfPoint">a list of points</param>
        /// <param name="shouldCloseTheGraph">a bool indicating if the graph should be closed</param>
        private void Init(MutablePoint[] listOfPoint, bool shouldCloseTheGraph)
        {
            this.listOfPoint = listOfPoint;
            this.shouldCloseTheGraph = shouldCloseTheGraph;

            this.q1 = new QuadrantSpecific1(this.listOfPoint, (a, b) => (a.X > b.X) ? -1 : (a.X < b.X) ? 1 : 0);
            this.q2 = new QuadrantSpecific2(this.listOfPoint, (a, b) => (a.X > b.X) ? -1 : (a.X < b.X) ? 1 : 0);
            this.q3 = new QuadrantSpecific3(this.listOfPoint, (a, b) => (a.X < b.X) ? -1 : (a.X > b.X) ? 1 : 0);
            this.q4 = new QuadrantSpecific4(this.listOfPoint, (a, b) => (a.X < b.X) ? -1 : (a.X > b.X) ? 1 : 0);
        }

        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Simple struct clearly named")]
        private struct Limit
        {
            internal MutablePoint Q1Top;
            internal MutablePoint Q2Top;
            internal MutablePoint Q2Left;
            internal MutablePoint Q3Left;
            internal MutablePoint Q3Bottom;
            internal MutablePoint Q4Bottom;
            internal MutablePoint Q4Right;
            internal MutablePoint Q1Right;

            /// <summary>
            /// Initializes a new instance of the <see cref="Limit"/> struct.
            /// </summary>
            /// <param name="point">a point</param>
            internal Limit(MutablePoint point)
            {
                this.Q1Top = point;
                this.Q2Top = point;
                this.Q2Left = point;
                this.Q3Left = point;
                this.Q3Bottom = point;
                this.Q4Bottom = point;
                this.Q4Right = point;
                this.Q1Right = point;
            }

            /// <summary>
            /// Copies a limit
            /// </summary>
            /// <returns>a new limit</returns>
            internal Limit Copy()
            {
                Limit limit = new Limit
                {
                    Q1Top = this.Q1Top,
                    Q2Top = this.Q2Top,
                    Q2Left = this.Q2Left,
                    Q3Left = this.Q3Left,
                    Q3Bottom = this.Q3Bottom,
                    Q4Bottom = this.Q4Bottom,
                    Q4Right = this.Q4Right,
                    Q1Right = this.Q1Right
                };

                return limit;
            }
        }
    }
}

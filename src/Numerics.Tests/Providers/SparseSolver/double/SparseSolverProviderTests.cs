using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Storage;
using MathNet.Numerics.Providers.SparseSolver;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace MathNet.Numerics.Tests.Providers.SparseSolver.Double
{

#if MKL

    /// <summary>
    /// Base class for sparse solver provider tests.
    /// </summary>
    [TestFixture, Category("SparseSolverProvider")]
    public class SparseSolverProviderTests
    {
        readonly double[] _b4 = { 1.0, 2.0, 3.0, 4.0};
        readonly double[] _b5 = { 1.0, 2.0, 3.0, 4.0, 5.0 };

        /// <summary>
        /// Test matrix to use.
        /// </summary>
        readonly IDictionary<string, SparseMatrix> _matrices = new Dictionary<string, SparseMatrix>
        {
            {"SymmetricPositiveDefinite5x5", (SparseMatrix)Matrix<double>.Build.SparseOfColumnArrays(new [] {9.0, 1.5, 6.0, 0.75, 3.0}, new [] {1.5, 0.5, 0.0, 0.0, 0.0}, new [] {6.0, 0.0, 12.0, 0.0, 0.0 }, new [] { 0.75, 0.0, 0.0, 0.625, 0.0}, new [] {3.0, 0.0, 0.0, 0.0, 16.0})},
            {"Triangle5x5", (SparseMatrix)Matrix<double>.Build.SparseOfColumnArrays(new [] {1.0, 0.0, 0.0, 0.0, 0.0}, new [] {5.0, 2.0, 0.0, 0.0, 0.0}, new [] {0.0, 8.0, 3.0, 0.0, 0.0 }, new [] { 0.0, 0.0, 9.0, 4.0, 0.0}, new [] {0.0, 0.0, 0.0, 10.0, 5.0})},
            {"Square4x4", (SparseMatrix)Matrix<double>.Build.SparseOfColumnArrays(new [] {1.0, 1.0, 1.0, 2.0 },new [] {2.0, 0.0, 0.0, 2.0 },new [] {0.0, 0.0, 2.0, 1.0 },new [] {4.0, 1.0, 1.0, 0.0 })},
        };

        /// <summary>
        /// Can solve Ax=b using direct sparse solver.
        /// </summary>
        [Test]
        public void CanSolveSymmetricPositiveDefiniteMatrix()
        {
            var A = _matrices["SymmetricPositiveDefinite5x5"].UpperTriangle();

            var csr = A.Storage as SparseCompressedRowMatrixStorage<double>;
            csr.PopulateExplicitZerosOnDiagonal();

            var rowCount = csr.RowCount;
            var columnCount = csr.ColumnCount;
            var valueCount = csr.ValueCount;
            var values = csr.Values;
            var rowPointers = csr.RowPointers;
            var columnIndices = csr.ColumnIndices;

            var xactual = new double[rowCount];

            var error = SparseSolverControl.Provider.Solve(DssMatrixStructure.Symmetric, DssMatrixType.PositiveDefinite, DssSystemType.DontTranspose,
                rowCount, columnCount, valueCount, rowPointers, columnIndices, values,
                1, _b5, xactual);

            Assert.That(error, Is.EqualTo(DssStatus.MKL_DSS_SUCCESS));

            var xtrue = new double[] { -979.0 / 3.0, 983.0, 1961.0 / 12.0, 398.0, 123.0 / 2.0 };

            for (int i = 0; i < xtrue.Length; i++)
                AssertHelpers.AlmostEqualRelative(xtrue[i], xactual[i], 12);
        }

        /// <summary>
        /// Can solve Ax=b using direct sparse solver.
        /// </summary>
        [Test]
        public void CanSolveUpperTriangularMatrix()
        {
            var A = _matrices["Triangle5x5"].UpperTriangle();

            var csr = A.Storage as SparseCompressedRowMatrixStorage<double>;
            csr.PopulateExplicitZerosOnDiagonal();

            var rowCount = csr.RowCount;
            var columnCount = csr.ColumnCount;
            var valueCount = csr.ValueCount;
            var values = csr.Values;
            var rowPointers = csr.RowPointers;
            var columnIndices = csr.ColumnIndices;

            var xactual = new double[rowCount];

            var error = SparseSolverControl.Provider.Solve(DssMatrixStructure.Nonsymmetric, DssMatrixType.Indefinite, DssSystemType.DontTranspose,
                rowCount, columnCount, valueCount, rowPointers, columnIndices, values,
                1, _b5, xactual);

            Assert.That(error, Is.EqualTo(DssStatus.MKL_DSS_SUCCESS));

            var xtrue = new double[] { 106.0, -21.0, 5.5, -1.5, 1.0 };

            for (int i = 0; i < xtrue.Length; i++)
                AssertHelpers.AlmostEqualRelative(xtrue[i], xactual[i], 13);
        }

        /// <summary>
        /// Can solve Ax=b using direct sparse solver.
        /// </summary>
        [Test]
        public void CanSolveSquareMatrix()
        {
            var A = _matrices["Square4x4"];

            var csr = A.Storage as SparseCompressedRowMatrixStorage<double>;
            csr.PopulateExplicitZerosOnDiagonal();

            var rowCount = csr.RowCount;
            var columnCount = csr.ColumnCount;
            var valueCount = csr.ValueCount;
            var values = csr.Values;
            var rowPointers = csr.RowPointers;
            var columnIndices = csr.ColumnIndices;

            var xactual = new double[rowCount];

            var error = SparseSolverControl.Provider.Solve(DssMatrixStructure.Nonsymmetric, DssMatrixType.Indefinite, DssSystemType.DontTranspose,
                rowCount, columnCount, valueCount, rowPointers, columnIndices, values,
                1, _b4, xactual);

            Assert.That(error, Is.EqualTo(DssStatus.MKL_DSS_SUCCESS));

            var xtrue = new double[] { 2.1, -0.35, 0.5, -0.1 };

            for (int i = 0; i < xtrue.Length; i++)
                AssertHelpers.AlmostEqualRelative(xtrue[i], xactual[i], 10);
        }

        /// <summary>
        /// Can inverse A by using AX = I.
        /// </summary>
        [Test]
        public void CanInverseSquareMatrix()
        {
            var A = _matrices["SymmetricPositiveDefinite5x5"];
            var Atr = A.UpperTriangle();

            var csr = Atr.Storage as SparseCompressedRowMatrixStorage<double>;
            csr.PopulateExplicitZerosOnDiagonal();

            var rowCount = csr.RowCount;
            var columnCount = csr.ColumnCount;
            var valueCount = csr.ValueCount;
            var values = csr.Values;
            var rowPointers = csr.RowPointers;
            var columnIndices = csr.ColumnIndices;

            var Identity = Matrix<double>.Build.DenseIdentity(columnCount);
            var b = Identity.ToColumnMajorArray();
            var Xactual = new double[rowCount * columnCount];

            var error = SparseSolverControl.Provider.Solve(DssMatrixStructure.Symmetric, DssMatrixType.PositiveDefinite, DssSystemType.DontTranspose,
                rowCount, columnCount, valueCount, rowPointers, columnIndices, values,
                Identity.ColumnCount, b, Xactual);

            Assert.That(error, Is.EqualTo(DssStatus.MKL_DSS_SUCCESS));

            var Ainverse_actual = Matrix<double>.Build.SparseOfColumnMajor(rowCount, columnCount, Xactual);

            var Ainverse_expected = Matrix<double>.Build.DenseOfColumnArrays(
                new[] { 80.0/3.0, -80.0, -40.0/3.0, -32.0, -5.0 },
                new[] { -80.0,    242.0,      40.0,  96.0, 15.0 },
                new[] { -40.0/3.0, 40.0,      6.75,  16.0,  2.5 },
                new[] { -32.0,     96.0,      16.0,  40.0,  6.0 },
                new[] { -5.0,      15.0,       2.5,   6.0,  1.0});

            for (int i = 0; i < Ainverse_actual.RowCount; i++)
                for (int j = 0; j < Ainverse_actual.ColumnCount; j++)
                    AssertHelpers.AlmostEqualRelative(Ainverse_actual[i, j], Ainverse_expected[i, j], 10);
        }

        /// <summary>
        /// Can solve 1D boundary problem.
        /// </summary>
        [TestCase(4, 8001, 10, 1000)]
        [TestCase(40, 8001, 0.1, 100)]
        [TestCase(400, 8001, 0.001, 10)]
        [TestCase(4000, 8001, 0.00001, 1)]
        public void CanSolvePoissonEquation(int elementCount, int interpolationCount, double errV, double errE)
        {
            var domain = new Domain(elementCount, 0.08);
            domain.DefineBoundaries(1.0, 0.0);
            domain.DefineProblem();
            domain.SolveProblem();

            var xGrid = Generate.LinearSpaced(interpolationCount, 0.0, domain.Length);

            var interpolated = domain.InterpolateAt(xGrid);
            var Vactual = interpolated.Item1; // interpolated electric potential
            var Eactual = interpolated.Item2; // interpolated electric field

            var exact = domain.GetExactSolution(xGrid);
            var Vexpected = exact.Item1; // expected electric potential
            var Eexpected = exact.Item2; // expected electric field

            var Vdiff = Vector<double>.Build.Dense(Vactual.Length, (i) => Vexpected[i] - Vactual[i]);
            var Vnorm2 = Vdiff.L2Norm();
            Assert.LessOrEqual(Vnorm2, errV);

            var Ediff = Vector<double>.Build.Dense(Eactual.Length, (i) => Eexpected[i] - Eactual[i]);
            var Enorm2 = Ediff.L2Norm();
            Assert.LessOrEqual(Enorm2, errE);
        }

        #region Finite element method to solve Poisson's equation

        class Node
        {
            public int ID;
            public double X;
            public double PrimaryValue; // electric potential at the node
            public double SecondaryValue; // electric field at the node

            public Node(double x)
            {
                X = x;
            }
        }

        class Element // Linear element
        {
            public int ID;
            public Node[] Nodes;
            public double Alpha;
            public double Gamma;
            public Matrix<double> Kmatrix;
            public Vector<double> Rhs;

            public Element(Node node1, Node node2, double alpha, double gamma)
            {
                Nodes = new[] { node1, node2 };
                Alpha = alpha;
                Gamma = gamma;
            }

            public void ComputeMatrices()
            {
                // The master equation:
                //   ∇(α∇V) + γ = 0
                //
                // Let's define a transformation from u to x
                //    x = a * u + b
                //
                // for Node1, u = -1 gives -a + b = x1
                // for Node2, u = +1 gives a + b = x2
                //
                // So,
                //    x = (x2 - x1)/2*u + (x2 + x1)/2
                //    u = 2*(x - x1)/(x2 - x1) - 1
                //
                // This gives
                //    dx = l/2*du where l = x2 - x1 = x21 is length of the line
                //
                // Interpolation function, Ni(u) = ai + bi*u for i = 1, 2
                //    N1(-1) = a1 - b1 = 1
                //    N1(+1) = a1 + b1 = 0 -> a1 = 1/2, b1 = -1/2
                //    N2(-1) = a2 - b2 = 0
                //    N2(+1) = a2 + b2 = 1 -> a2 = 1/2, b2 = 1/2
                // so,
                //    N1 = (1 - u) / 2
                //    N2 = (1 + u) / 2
                //
                // Using the chain rule of differentiation,
                //    ∂Ni(x)/∂u = ∂Ni/∂x ∂x/∂u
                //
                // Here,
                //    J = ∂x/∂u = x21/2 = l/2
                //
                //    |J| = l/2  where l is the length of the line
                //
                // Therefore, we can get
                //    ∂N1/∂x = 2/l ∂N1/∂u = - 1/l
                //    ∂N2/∂x = 2/l ∂N2/∂u = 1/l
                //-----------------------------------------------------------------------------
                // By using V(u) = Σ Vi*Ni(u) and ω = Ni for i = 1, 2
                // the weak form of the master equation is given as
                //
                //    ∫ ω [d/dx(α dV/dx) + γ] dx = 0
                //
                //    K v = f + p
                //
                // where
                //    Kij = ∫ (dNi/dx) α (dNj/dx) dx
                //    fi  = ∫ Ni γ dx
                //    pi  = -Ni(x2)D(x2) + Ni(x1)D(x1)  where D(x) = -α dV(x)/dx
                //-----------------------------------------------------------------------------
                //    Kij = (l/2) ∫ α (∂Ni/∂x) (∂Nj/∂x) du, where α = const.
                //        = α*(∂Ni/∂x)*(∂Nj/∂x)*l
                //    K11 = α/l
                //    K12 = M21 = - α/l
                //    K22 = α/l
                //-----------------------------------------------------------------------------
                //    fi = γ*(l/2) ∫ Ni du, where γ = const.
                //       = γ*(l/2)
                //    f1 = γ*l/2
                //    f2 = γ*l/2
                //-----------------------------------------------------------------------------
                //    p1 = -N1(x2)D(x2) + N1(x1)D(x1) = D(x1) = D1
                //    p2 = -N2(x2)D(x2) + N2(x1)D(x1) = -D(x2) = -D2
                // For a sufﬁciently large number of elements in the domain, we can ignore p.

                var x21 = Nodes[1].X - Nodes[0].X; // element length

                Kmatrix = Matrix<double>.Build.Dense(2, 2);
                Kmatrix[0, 0] = Alpha / x21;
                Kmatrix[0, 1] = -Alpha / x21;
                Kmatrix[1, 0] = -Alpha / x21;
                Kmatrix[1, 1] = Alpha / x21;

                Rhs = Vector<double>.Build.Dense(2);
                Rhs[0] = Gamma * x21 / 2.0;
                Rhs[1] = Gamma * x21 / 2.0;
            }

            public bool Contains(Node point)
            {
                // A point P can be described with a line A-B
                //    P = A + s*(B - A)
                //    s = PA/BA
                // if (s >= 0 && s <= 1), then P is inside the line A-B.

                var s = (point.X - Nodes[0].X) / (Nodes[1].X - Nodes[0].X);
                return s >= 0.0 && (1.0 - s) >= 0.0;
            }

            public void InterpolateAt(Node point)
            {
                // V(u) can be described with interpolation functions
                //    V(u) = V1*N1(u) + V2*N2(u)
                // where
                //    N1 = (1 - u) / 2
                //    N2 = (1 + u) / 2
                // The transformation from x to u,
                //    u = 2*(x - x1)/(x2 - x1) - 1
                // gives
                //    V(x) = V1*(x2 - x)/(x2 - x1) + V2*(x - x1)/(x2 - x1)

                var V1 = Nodes[0].PrimaryValue;
                var V2 = Nodes[1].PrimaryValue;

                var x21 = Nodes[1].X - Nodes[0].X;
                var xp1 = point.X - Nodes[0].X;
                var u = 2.0 * xp1 / x21 - 1.0;

                var Vx = V1 * (1 - u) * 0.5 + V2 * (1 + u) * 0.5;

                // Electric field,
                //    E = -∇V
                // where
                //    ∇V = [ (∂/∂x) ∑ViNi ]
                //       = [ (∂/∂x)(V1*N1 + V2*N2) ]
                //       = [ V1*(∂N1/∂x) + V2*(∂N2/∂x) ]
                //
                // The derivatives of Ni are
                //    ∂N1/∂x = 2/l ∂N1/∂u = - 1/l where l = x21
                //    ∂N2/∂x = 2/l ∂N2/∂u = 1/l
                //
                // Therefore,
                //    E = (V1 - V2) / x21

                var Ex = (V1 - V2) / x21;

                point.PrimaryValue = Vx;
                point.SecondaryValue = Ex;
            }
        }

        class Domain
        {
            // Length of the domain in [m]
            public double Length;

            // Dielectric constant of the domain
            public double Permittivity;

            // Charge density in [C/m^3]
            public double ChargeDensity;

            // Boundary conditions
            public double VoltageAtLeft;
            public double VoltageAtRight;
            public List<Tuple<Node, double>> Boundaries;

            public Node[] Nodes;
            public Element[] Elements;

            public Matrix<double> Kmatrix;
            public Vector<double> Rhs;

            public Domain(int elementCount = 4, double length = 0.08, double relativePermittivity = 1, double chargeDensity = 1E-8)
            {
                Length = length;

                Permittivity = Constants.ElectricPermittivity * relativePermittivity;
                ChargeDensity = chargeDensity;

                // Create nodes and elements
                Nodes = new Node[elementCount + 1];
                Elements = new Element[elementCount];

                var dx = length / (double)elementCount;

                // nodes:    0   1   2   3 ...    n+1
                // elements:   0   1   2 ...    n
                //           +---+---+---+ ... ---+
                // x axis:   0                    length

                for (int i = 0; i < Nodes.Length; i++)
                {
                    Nodes[i] = new Node(dx * i) { ID = i };
                }
                for (int i = 0; i < Elements.Length; i++)
                {
                    Elements[i] = new Element(Nodes[i], Nodes[i + 1], Permittivity, -ChargeDensity) { ID = i };
                }

                // Initialization of the global K matrix and right-hand side vector
                // We know the K matrix is a symmetric matrix, so we will only handle the upper triangular parts.
                Kmatrix = Matrix<double>.Build.Sparse(Nodes.Length, Nodes.Length);
                Rhs = Vector<double>.Build.Dense(Nodes.Length);
            }

            public void DefineBoundaries(double Vleft, double Vright)
            {
                VoltageAtLeft = Vleft;
                VoltageAtRight = Vright;

                // Dirichlet boundary conditions
                Boundaries = new List<Tuple<Node, double>>();
                foreach (var node in Nodes)
                {
                    if (node.X == 0d)
                    {
                        Boundaries.Add(new Tuple<Node, double>(node, Vleft));
                    }
                    else if (node.X == Length)
                    {
                        Boundaries.Add(new Tuple<Node, double>(node, Vright));
                    }
                }
            }

            public void DefineProblem()
            {
                // Form the element matrices and assemble to the global matrix
                foreach (var element in Elements)
                {
                    element.ComputeMatrices();

                    // Assemble element matrix into the global K matrix
                    for (int i = 0; i < element.Nodes.Length; i++)
                    {
                        var row = element.Nodes[i].ID;
                        for (int j = 0; j < element.Nodes.Length; j++)
                        {
                            var col = element.Nodes[j].ID;
                            if (row <= col) // only upper triangular parts are handled
                            {
                                Kmatrix[row, col] += element.Kmatrix[i, j];
                            }
                        }
                        Rhs[row] += element.Rhs[i];
                    }
                }

                // Imposition of Dirichlet boundary conditions
                //
                // If xn is given, i.e. x2 = b0, then the linear equations,
                //    [ K11 K12 K13 K14 ][ x1 ] = [ b1 ]
                //    [ K21 K22 K23 K24 ][ x2 ]   [ b2 ]
                //    [ K31 K32 K33 K34 ][ x3 ]   [ b3 ]
                //    [ K41 K42 K43 K44 ][ x4 ]   [ b4 ]
                // can be changed to
                //    [ K11  0  K13 K14 ][ x1 ] = [ b1 - K12*b0 ]
                //    [  0   1   0   0  ][ x2 ]   [ b0          ]
                //    [ K31  0  K33 K34 ][ x3 ]   [ b3 - K32*b0 ]
                //    [ K41  0  K43 K44 ][ x4 ]   [ b4 - K42*b0 ]

                foreach (var boundary in Boundaries)
                {
                    var node = boundary.Item1;
                    var i = node.ID;
                    var val = boundary.Item2;

                    for (int j = 0; j < Nodes.Length; j++)
                    {
                        if (Nodes[j].ID != i)
                        {
                            Rhs[j] -= (j <= i)
                                ? Kmatrix[j, i] * val
                                : Kmatrix[i, j] * val; // Kmatrix has only upper triangular parts
                        }
                    }

                    var storage = Kmatrix.Storage as SparseCompressedRowMatrixStorage<double>;
                    storage.MapIndexedInplace(
                        (row, col, x) => (row == i && col == i)
                                            ? 1d
                                            : (row == i) || (col == i)
                                                ? 0d
                                                : x,
                        Zeros.AllowSkip);

                    Rhs[i] = val;
                }
            }

            public void SolveProblem()
            {
                // Note that Kmatrix is actually an upper triangular, but considered as a symmetric.
                var storage = Kmatrix.Storage as SparseCompressedRowMatrixStorage<double>;
                var rowCount = storage.RowCount;
                var columnCount = storage.ColumnCount;
                var valueCount = storage.ValueCount;
                var values = storage.Values;
                var rowPointers = storage.RowPointers;
                var columnIndices = storage.ColumnIndices;

                var rhs = Rhs.ToArray();
                var solution = new double[rowCount];

                SparseSolverControl.Provider.Solve(DssMatrixStructure.Symmetric, DssMatrixType.Indefinite, DssSystemType.DontTranspose,
                    rowCount, columnCount, valueCount, rowPointers, columnIndices, values,
                    1, rhs, solution);

                for (int i = 0; i < solution.Length; i++)
                {
                    Nodes[i].PrimaryValue = solution[i];
                }
            }

            public Tuple<double[], double[]> InterpolateAt(double[] xGrid)
            {
                var Vactual = new double[xGrid.Length];
                var Eactual = new double[xGrid.Length];

                for (int i = 0; i < xGrid.Length; i++)
                {
                    var point = new Node(xGrid[i]);
                    for (int j = 0; j < Elements.Length; j++)
                    {
                        if (Elements[j].Contains(point))
                        {
                            Elements[j].InterpolateAt(point);
                            Vactual[i] = point.PrimaryValue;
                            Eactual[i] = point.SecondaryValue;
                            break;
                        }
                    }
                }

                return new Tuple<double[], double[]>(Vactual, Eactual);
            }

            public Tuple<double[], double[]> GetExactSolution(double[] xGrid)
            {
                // Poisson's equation: ∇(ε∇V) = ρ
                // Solution:
                //    V(x) = ρ/ε/2*x^2 - (ρ/ε/2*d + (Va - Vb)/d)*x + Va, where d = length
                //    E(x) = -∇V = ρ/ε*x - (ρ/ε/2*d + (Va - Vb)/d)

                double[] Vexact = new double[xGrid.Length]; // electric potentials
                double[] Eexact = new double[xGrid.Length]; // electric fields
                var factor = ChargeDensity / Permittivity * 0.5; // ρ/ε/2

                for (int i = 0; i < xGrid.Length; i++)
                {
                    var x = xGrid[i];
                    Vexact[i] = factor * x * x - factor * Length * x - (VoltageAtLeft - VoltageAtRight) / Length * x + VoltageAtLeft;
                    Eexact[i] = -2d * factor * x + factor * Length + (VoltageAtLeft - VoltageAtRight) / Length;
                }

                return new Tuple<double[], double[]>(Vexact, Eexact);
            }
        }

        #endregion
    }

#endif

}


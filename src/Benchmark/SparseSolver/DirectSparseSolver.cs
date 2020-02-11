using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Storage;
using MathNet.Numerics.Providers.Common.Mkl;
using MathNet.Numerics.Providers.SparseSolver;
using System;
using System.Collections.Generic;

namespace Benchmark.SparseSolver
{
    [Config(typeof(Config))]
    public class DirectSparseSolver
    {
        class Config : ManualConfig
        {
            public Config()
            {
                Add(Job.Clr.With(Platform.X64).With(Jit.RyuJit));
                Add(Job.Clr.With(Platform.X86).With(Jit.LegacyJit));
#if !NET461
                Add(Job.Core.With(Platform.X64).With(Jit.RyuJit));
#endif
            }
        }

        public enum ProviderId
        {
            NativeMKL,
        }

        [Params(32, 128, 1024)]
        public int N { get; set; }

        [Params(ProviderId.NativeMKL)]
        public ProviderId Provider { get; set; }

        int rowCount;
        int columnCount;
        int valueCount;
        double[] values;
        int[] rowPointers;
        int[] columnIndices;
        double[] rhs;

        [GlobalSetup]
        public void GlobalSetup()
        {
            switch (Provider)
            {
                case ProviderId.NativeMKL:
                    Control.UseNativeMKL(MklConsistency.Auto, MklPrecision.Double, MklAccuracy.High);
                    break;
            }

            var domain = new Domain(N);
            domain.DefineProblem();

            // Kmatrix is symmetric so, we need only upper triangle entries.         
            var storage = domain.Kmatrix.UpperTriangle().Storage as SparseCompressedRowMatrixStorage<double>;
            rowCount = storage.RowCount;
            columnCount = storage.ColumnCount;
            valueCount = storage.ValueCount;
            values = storage.Values;
            rowPointers = storage.RowPointers;
            columnIndices = storage.ColumnIndices;

            rhs = domain.Rhs.ToArray();
        }

        [Benchmark(OperationsPerInvoke = 1)]
        public double[] SolveProblem()
        {
            double[] solution = new double[rowCount];
            SparseSolverControl.Provider.Solve(DssMatrixStructure.Symmetric, DssMatrixType.Indefinite, DssSystemType.NonTransposed,
                rowCount, columnCount, valueCount, rowPointers, columnIndices, values,
                1, rhs, solution);
            return solution;
        }

        #region Finite element method to solve Poisson's equation

        class Node
        {
            public int ID;
            public double X;

            public Node(int id, double x)
            {
                ID = id;
                X = x;
            }
        }

        class Element
        {
            public int ID;
            public Node[] Nodes;
            public double Alpha;
            public double Gamma;
            public Matrix<double> Kmatrix;
            public Vector<double> Rhs;

            public Element(int id, Node node1, Node node2, double alpha, double gamma)
            {
                ID = id;
                Nodes = new[] { node1, node2 };
                Alpha = alpha;
                Gamma = gamma;
            }

            public void ComputeMatrices()
            {
                var length = Nodes[1].X - Nodes[0].X; // length

                Kmatrix = Matrix<double>.Build.Dense(2, 2);
                Kmatrix[0, 0] = Alpha / length;
                Kmatrix[0, 1] = -Alpha / length;
                Kmatrix[1, 0] = -Alpha / length;
                Kmatrix[1, 1] = Alpha / length;

                Rhs = Vector<double>.Build.Dense(2);
                Rhs[0] = -Gamma * length / 2.0;
                Rhs[1] = -Gamma * length / 2.0;
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

            public double VoltageAtLeft;
            public double VoltageAtRight;

            Node[] nodes;
            Element[] elements;
            List<Tuple<Node, double>> boundaries;

            public Matrix<double> Kmatrix;
            public Vector<double> Rhs;

            public Domain(int elementCount = 4, double length = 0.08, double relativePermittivity = 1, double chargeDensity = 1E-8)
            {
                // Boundary value problem: 
                //
                // [0] ------ [1] ------ ... ------ [N]
                //
                // each element is characterized by an electron charge density and a dielectric constant
                //
                // V at node1 = 1V 
                // V at node5 = 0V (ground)

                Length = length;

                Permittivity = Constants.ElectricPermittivity * relativePermittivity;
                ChargeDensity = chargeDensity;

                // Create nodes and elements 
                nodes = new Node[elementCount + 1];
                elements = new Element[elementCount];

                var dx = Length / elements.Length;
                for (int i = 0; i < nodes.Length; i++)
                {
                    nodes[i] = new Node(i, dx * i);
                }
                for (int i = 0; i < elements.Length; i++)
                {
                    elements[i] = new Element(i, nodes[i], nodes[i + 1], Permittivity, ChargeDensity);
                }

                // Initialization of the global K matrix and right-hand side vector
                Kmatrix = Matrix<double>.Build.Sparse(nodes.Length, nodes.Length);
                Rhs = Vector<double>.Build.Dense(nodes.Length);
            }

            public void DefineProblem(double Va = 1.0, double Vb = 0.0)
            {
                VoltageAtLeft = Va; // Boundary condition at the leftmost node
                VoltageAtRight = Vb; // Boundary condition at the rightmost node

                // Apply Dirichlet boundary conditions
                boundaries = new List<Tuple<Node, double>>();
                foreach (var node in nodes)
                {
                    if (node.X == 0)
                    {
                        boundaries.Add(new Tuple<Node, double>(node, VoltageAtLeft));
                    }
                    else if (node.X == Length)
                    {
                        boundaries.Add(new Tuple<Node, double>(node, VoltageAtRight));
                    }
                }

                // Form the element matrices and assemble to the global matrix
                foreach (var element in elements)
                {
                    element.ComputeMatrices();

                    // Assemble element matrix into the global K matrix
                    for (int i = 0; i < element.Nodes.Length; i++)
                    {
                        var row = element.Nodes[i].ID;
                        for (int j = 0; j < element.Nodes.Length; j++)
                        {
                            var col = element.Nodes[j].ID;
                            Kmatrix[row, col] += element.Kmatrix[i, j];
                        }
                        Rhs[row] += element.Rhs[i];
                    }
                }

                // Imposition of Dirichlet boundary conditions
                foreach (var boundary in boundaries)
                {
                    var node = boundary.Item1;
                    var i = node.ID;
                    var val = boundary.Item2;

                    for (int j = 0; j < nodes.Length; j++)
                    {
                        if (nodes[j].ID != i)
                            Rhs[j] = Rhs[j] - Kmatrix[j, i] * val;
                    }

                    Kmatrix.SetColumn(i, new double[Kmatrix.RowCount]);
                    Kmatrix.SetRow(i, new double[Kmatrix.ColumnCount]);
                    Kmatrix[i, i] = 1.0;
                    Rhs[i] = val;
                }
            }

            public double[] SolveProblem()
            {
                // Kmatrix is symmetric so, we need only upper triangle entries.
                var storage = Kmatrix.UpperTriangle().Storage as SparseCompressedRowMatrixStorage<double>;
                var rowCount = storage.RowCount;
                var columnCount = storage.ColumnCount;
                var valueCount = storage.ValueCount;
                var values = storage.Values;
                var rowPointers = storage.RowPointers;
                var columnIndices = storage.ColumnIndices;

                var rhs = Rhs.ToArray();
                var solution = new double[rowCount];

                SparseSolverControl.Provider.Solve(DssMatrixStructure.Symmetric, DssMatrixType.Indefinite, DssSystemType.NonTransposed,
                    rowCount, columnCount, valueCount, rowPointers, columnIndices, values,
                    1, rhs, solution);

                return solution;
            }

            public double[] GetExactSolution()
            {
                // Poisson's equation : ∇(ε∇V) = ρ
                // solution : V(x) = ρ/ε/2*x^2 - (ρ/ε/2*d + (Va - Vb)/d)*x + Va, where d = length

                double[] Vexact = new double[nodes.Length];
                var factor = ChargeDensity / Permittivity * 0.5;
                for (int i = 0; i < nodes.Length; i++)
                {
                    var x = nodes[i].X;
                    Vexact[i] = factor * x * x - factor * Length * x - (VoltageAtLeft - VoltageAtRight) / Length * x + VoltageAtLeft;
                }

                return Vexact;
            }
        }

        #endregion
    }
}

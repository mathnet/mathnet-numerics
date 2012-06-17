// <copyright file="CompositeSolver.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
// http://mathnetnumerics.codeplex.com
//
// Copyright (c) 2009-2010 Math.NET
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics.LinearAlgebra.Complex32.Solvers.Iterative
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Generic.Solvers.Status;
    using Properties;

    /// <summary>
    /// A composite matrix solver. The actual solver is made by a sequence of
    /// matrix solvers. 
    /// </summary>
    /// <remarks>
    /// <para>
    /// Solver based on:<br />
    /// Faster PDE-based simulations using robust composite linear solvers<br />
    /// S. Bhowmicka, P. Raghavan a,*, L. McInnes b, B. Norris<br />
    /// Future Generation Computer Systems, Vol 20, 2004, pp 373–387<br />
    /// </para>
    /// <para>
    /// Note that if an iterator is passed to this solver it will be used for all the sub-solvers.
    /// </para>
    /// </remarks>
    public sealed class CompositeSolver : IIterativeSolver
    {
        #region Internal class - DoubleComparer
        /// <summary>
        /// An <c>IComparer</c> used to compare double precision floating points.
        /// </summary>
        /// NOTE: The instance of this class is used only in <see cref="SolverSetups"/>. If C# suppports interface inheritence
        /// NOTE: and methods in anonymous types, then this class should be deleted and anonymous type implemented with IComaprer support
        /// NOTE: in <see cref="SolverSetups"/> constructor
        public sealed class DoubleComparer : IComparer<double>
        {
            /// <summary>
            /// Compares two double values based on the selected comparison method.
            /// </summary>
            /// <param name="x">The first double to compare.</param>
            /// <param name="y">The second double to compare.</param>
            /// <returns>
            /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return 
            /// value has the following meanings: 
            /// Value Meaning Less than zero This object is less than the other parameter.
            /// Zero This object is equal to other. 
            /// Greater than zero This object is greater than other. 
            /// </returns>
            public int Compare(double x, double y)
            {
                return x.CompareTo(y, 1);
            }
        } 
        #endregion

        /// <summary>
        /// The default status used if the solver is not running.
        /// </summary>
        private static readonly ICalculationStatus NonRunningStatus = new CalculationIndetermined();

        /// <summary>
        /// The default status used if the solver is running.
        /// </summary>
        private static readonly ICalculationStatus RunningStatus = new CalculationRunning();
        
#if PORTABLE
        private static readonly Dictionary<double, List<IIterativeSolverSetup>> SolverSetups = new Dictionary<double, List<IIterativeSolverSetup>>();        
#else
        /// <summary>
        /// The collection of iterative solver setups. Stored based on the
        /// ratio between the relative speed and relative accuracy.
        /// </summary>
        private static readonly SortedList<double, List<IIterativeSolverSetup>> SolverSetups = new SortedList<double, List<IIterativeSolverSetup>>(new DoubleComparer());
#endif

        #region Solver information loading methods

        /// <summary>
        /// Loads all the available <see cref="IIterativeSolverSetup"/> objects from the MathNet.Numerics assembly.
        /// </summary>
        public static void LoadSolverInformation()
        {
            LoadSolverInformation(new Type[0]);
        }

        /// <summary>
        /// Loads the available <see cref="IIterativeSolverSetup"/> objects from the MathNet.Numerics assembly.
        /// </summary>
        /// <param name="typesToExclude">The <see cref="IIterativeSolver"/> types that should not be loaded.</param>
        public static void LoadSolverInformation(Type[] typesToExclude)
        {
            LoadSolverInformationFromAssembly(Assembly.GetExecutingAssembly(), typesToExclude);
        }

#if !PORTABLE
        /// <summary>
        /// Loads the available <see cref="IIterativeSolverSetup"/> objects from the assembly specified by the file location.
        /// </summary>
        /// <param name="assemblyLocation">The fully qualified path to the assembly.</param>
        public static void LoadSolverInformationFromAssembly(string assemblyLocation)
        {
            LoadSolverInformationFromAssembly(assemblyLocation, new Type[0]);
        }

        /// <summary>
        /// Loads the available <see cref="IIterativeSolverSetup"/> objects from the assembly specified by the file location.
        /// </summary>
        /// <param name="assemblyLocation">The fully qualified path to the assembly.</param>
        /// <param name="typesToExclude">The <see cref="IIterativeSolver"/> types that should not be loaded. </param>
        public static void LoadSolverInformationFromAssembly(string assemblyLocation, params Type[] typesToExclude)
        {
            if (assemblyLocation == null)
            {
                throw new ArgumentNullException("assemblyLocation");
            }

            if (assemblyLocation.Length == 0)
            {
                throw new ArgumentException();
            }

            if (!File.Exists(assemblyLocation))
            {
                throw new FileNotFoundException();
            }

            // Get the assembly name
            var assemblyFileName = Path.GetFileNameWithoutExtension(assemblyLocation);

            // Now load the assembly with an AssemblyName
            var assemblyName = new AssemblyName(assemblyFileName);
            var assembly = Assembly.Load(assemblyName.FullName);
            
            // <ay throws:
            // ArgumentNullException --> Can't get this because we checked that the file exists.
            // FileNotFoundException --> Can't get this because we checked that the file exists.
            // FileLoadException
            // BadImageFormatException
            // Now we can load the solver information.
            LoadSolverInformationFromAssembly(assembly, typesToExclude);
        }
#endif

        /// <summary>
        /// Loads the available <see cref="IIterativeSolverSetup"/> objects from the assembly specified by the assembly name.
        /// </summary>
        /// <param name="assemblyName">The <see cref="AssemblyName"/> of the assembly that should be searched for setup objects. </param>
        public static void LoadSolverInformationFromAssembly(AssemblyName assemblyName)
        {
            LoadSolverInformationFromAssembly(assemblyName, new Type[0]);
        }

        /// <summary>
        /// Loads the available <see cref="IIterativeSolverSetup"/> objects from the assembly specified by the assembly name.
        /// </summary>
        /// <param name="assemblyName">The <see cref="AssemblyName"/> of the assembly that should be searched for setup objects.</param>
        /// <param name="typesToExclude">The <see cref="IIterativeSolver"/> types that should not be loaded.</param>
        public static void LoadSolverInformationFromAssembly(AssemblyName assemblyName, params Type[] typesToExclude)
        {
            if (assemblyName == null)
            {
                throw new ArgumentNullException("assemblyName");
            }

            var assembly = Assembly.Load(assemblyName.FullName);

            // May throw:
            // ArgumentNullException --> Can't get this because we checked it.
            // FileNotFoundException
            // FileLoadException
            // BadImageFormatException
            // Now we can load the solver information.
            LoadSolverInformationFromAssembly(assembly, typesToExclude);
        }

        /// <summary>
        /// Loads the available <see cref="IIterativeSolverSetup"/> objects from the assembly specified by the type.
        /// </summary>
        /// <param name="typeInAssembly">The type in the assembly which should be searched for setup objects.</param>
        public static void LoadSolverInformationFromAssembly(Type typeInAssembly)
        {
            LoadSolverInformationFromAssembly(typeInAssembly, new Type[0]);
        }

        /// <summary>
        /// Loads the available <see cref="IIterativeSolverSetup"/> objects from the assembly specified by the type.
        /// </summary>
        /// <param name="typeInAssembly">The type in the assembly which should be searched for setup objects.</param>
        /// <param name="typesToExclude">The <see cref="IIterativeSolver"/> types that should not be loaded.</param>
        public static void LoadSolverInformationFromAssembly(Type typeInAssembly, params Type[] typesToExclude)
        {
            if (typeInAssembly == null)
            {
                throw new ArgumentNullException("typeInAssembly");
            }

            LoadSolverInformationFromAssembly(typeInAssembly.Assembly, typesToExclude);
        }

        /// <summary>
        /// Loads the available <see cref="IIterativeSolverSetup"/> objects from the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly which will be searched for setup objects.</param>
        public static void LoadSolverInformationFromAssembly(Assembly assembly)
        {
            LoadSolverInformationFromAssembly(assembly, new Type[0]);
        }

        /// <summary>
        /// Loads the available <see cref="IIterativeSolverSetup"/> objects from the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly which will be searched for setup objects.</param>
        /// <param name="typesToExclude">The <see cref="IIterativeSolver"/> types that should not be loaded.</param>
        public static void LoadSolverInformationFromAssembly(Assembly assembly, params Type[] typesToExclude)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            if (typesToExclude == null)
            {
                throw new ArgumentNullException("typesToExclude");
            }

            var excludedTypes = new List<Type>(typesToExclude);

            // Load all the types in the assembly
            // Find all the types that implement IIterativeSolverSetup
            // Create an object of each of these types
            // Get the type of the iterative solver that will be instantiated by the setup object
            // Check if it's on the excluding list, if so throw the setup object away otherwise keep it.
            var interfaceTypes = new List<Type>();
            foreach (var type in assembly.GetTypes().Where(type => (!type.IsAbstract && !type.IsEnum && !type.IsInterface && type.IsVisible)))
            {
                interfaceTypes.Clear();
                interfaceTypes.AddRange(type.GetInterfaces());
                if (!interfaceTypes.Any(match => typeof(IIterativeSolverSetup).IsAssignableFrom(match)))
                {
                    continue;
                }

                // See if we actually want this type of iterative solver
                IIterativeSolverSetup setup;
                try
                {
                    // If something goes wrong we just ignore it and move on with the next type.
                    // There should probably be a log somewhere indicating that something went wrong?
                    setup = (IIterativeSolverSetup)Activator.CreateInstance(type);
                }
                catch (ArgumentException)
                {
                    continue;
                }
                catch (NotSupportedException)
                {
                    continue;
                }
                catch (TargetInvocationException)
                {
                    continue;
                }
#if !PORTABLE
                catch (MethodAccessException)
                {
                    continue;
                }
                catch (MissingMethodException)
                {
                    continue;
                }
#endif
                catch (MemberAccessException)
                {
                    continue;
                }
                catch (TypeLoadException)
                {
                    continue;
                }

                if (excludedTypes.Any(match => match.IsAssignableFrom(setup.SolverType) ||
                                               match.IsAssignableFrom(setup.PreconditionerType)))
                {
                    continue;
                }

                // Ok we want the solver, so store the object
                var ratio = setup.SolutionSpeed / setup.Reliability;
                if (!SolverSetups.ContainsKey(ratio))
                {
                    SolverSetups.Add(ratio, new List<IIterativeSolverSetup>());
                }

                var list = SolverSetups[ratio];
                list.Add(setup);
            }
        } 

        #endregion

        /// <summary>
        /// The collection of solvers that will be used to 
        /// </summary>
        private readonly List<IIterativeSolver> _solvers = new List<IIterativeSolver>();

        /// <summary>
        /// The status of the calculation.
        /// </summary>
        private ICalculationStatus _status = NonRunningStatus;

        /// <summary>
        /// The iterator that is used to control the iteration process.
        /// </summary>
        private IIterator _iterator;

        /// <summary>
        /// A flag indicating if the solver has been stopped or not.
        /// </summary>
        private bool _hasBeenStopped;

        /// <summary>
        /// The solver that is currently running. Reference is used to be able to stop the
        /// solver if the user cancels the solve process.
        /// </summary>
        private IIterativeSolver _currentSolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeSolver"/> class with the default iterator.
        /// </summary>
        public CompositeSolver() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeSolver"/> class with the specified iterator.
        /// </summary>
        /// <param name="iterator">The iterator that will be used to control the iteration process. </param>
        public CompositeSolver(IIterator iterator)
        {
            _iterator = iterator;
        }

        /// <summary>
        /// Sets the <c>IIterator</c> that will be used to track the iterative process.
        /// </summary>
        /// <param name="iterator">The iterator.</param>
        public void SetIterator(IIterator iterator)
        {
            _iterator = iterator;
        }

        /// <summary>
        /// Gets the status of the iteration once the calculation is finished.
        /// </summary>
        public ICalculationStatus IterationResult
        {
            get 
            { 
                return _status; 
            }
        }

        /// <summary>
        /// Stops the solve process. 
        /// </summary>
        /// <remarks>
        /// Note that it may take an indetermined amount of time for the solver to actually stop the process.
        /// </remarks>
        public void StopSolve()
        {
            _hasBeenStopped = true;
            if (_currentSolver != null)
            { 
                _currentSolver.StopSolve();
            }
        }

        /// <summary>
        /// Solves the matrix equation Ax = b, where A is the coefficient matrix, b is the
        /// solution vector and x is the unknown vector.
        /// </summary>
        /// <param name="matrix">The coefficient matrix, <c>A</c>.</param>
        /// <param name="vector">The solution vector, <c>b</c>.</param>
        /// <returns>The result vector, <c>x</c>.</returns>
        public Vector Solve(Matrix matrix, Vector vector)
        {
            if (vector == null)
            {
                throw new ArgumentNullException();
            }

            Vector result = new DenseVector(matrix.RowCount);
            Solve(matrix, vector, result);
            return result;
        }

        /// <summary>
        /// Solves the matrix equation Ax = b, where A is the coefficient matrix, b is the
        /// solution vector and x is the unknown vector.
        /// </summary>
        /// <param name="matrix">The coefficient matrix, <c>A</c>.</param>
        /// <param name="input">The solution vector, <c>b</c></param>
        /// <param name="result">The result vector, <c>x</c></param>
        public void Solve(Matrix matrix, Vector input, Vector result)
        {
            // If we were stopped before, we are no longer
            // We're doing this at the start of the method to ensure
            // that we can use these fields immediately.
            _hasBeenStopped = false;
            _currentSolver = null;

            // Error checks
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (matrix.RowCount != matrix.ColumnCount)
            {
                throw new ArgumentException(Resources.ArgumentMatrixSquare, "matrix");
            }

            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (result.Count != input.Count)
            {
                throw new ArgumentException(Resources.ArgumentVectorsSameLength);
            }

            // Initialize the solver fields
            // Set the convergence monitor
            if (_iterator == null)
            {
                _iterator = Iterator.CreateDefault();
            }

            // Load the solvers into our own internal data structure
            // Once we have solvers we can always reuse them.
            if (_solvers.Count == 0)
            {
                LoadSolvers();
            }

            // Create a copy of the solution and result vectors so we can use them
            // later on
            var internalInput = (Vector)input.Clone();
            var internalResult = (Vector)result.Clone();

            foreach (var solver in _solvers.TakeWhile(solver => !_hasBeenStopped))
            {
                // Store a reference to the solver so we can stop it.
                _currentSolver = solver;

                try
                {
                    // Reset the iterator and pass it to the solver
                    _iterator.ResetToPrecalculationState();
                    solver.SetIterator(_iterator);

                    // Start the solver
                    solver.Solve(matrix, internalInput, internalResult);
                }
                catch (Exception)
                {
                    // The solver broke down. 
                    // Log a message about this
                    // Switch to the next preconditioner. 
                    // Reset the solution vector to the previous solution
                    input.CopyTo(internalInput);
                    _status = RunningStatus;
                    continue;
                }

                // There was no fatal breakdown so check the status
                if (_iterator.Status is CalculationConverged)
                {
                    // We're done
                    internalResult.CopyTo(result);
                    break;
                }

                // We're not done
                // Either:
                // - calculation finished without convergence
                if (_iterator.Status is CalculationStoppedWithoutConvergence)
                {
                    // Copy the internal result to the result vector and
                    // continue with the calculation.
                    internalResult.CopyTo(result);
                }
                else
                {
                    // - calculation failed --> restart with the original vector
                    // - calculation diverged --> restart with the original vector
                    // - Some unknown status occurred --> To be safe restart.
                    input.CopyTo(internalInput);
                }
            }

            // Inside the loop we already copied the final results (if there are any)
            // So no need to do that again.

            // Clean up
            // No longer need the current solver
            _currentSolver = null;

            // Set the final status
            _status = _iterator.Status;
        }

        /// <summary>
        /// Load solvers
        /// </summary>
        private void LoadSolvers()
        {
            if (SolverSetups.Count == 0)
            {
                throw new Exception("IIterativeSolverSetup objects not found");
            }

#if PORTABLE
            foreach (var setup in SolverSetups.OrderBy(solver => solver.Key, new DoubleComparer()).Select(pair => pair.Value).SelectMany(setups => setups))          
#else
            foreach (var setup in SolverSetups.Select(pair => pair.Value).SelectMany(setups => setups))
#endif
            {
                _solvers.Add(setup.CreateNew());
            }
        }

        /// <summary>
        /// Solves the matrix equation AX = B, where A is the coefficient matrix, B is the
        /// solution matrix and X is the unknown matrix.
        /// </summary>
        /// <param name="matrix">The coefficient matrix, <c>A</c>.</param>
        /// <param name="input">The solution matrix, <c>B</c>.</param>
        /// <returns>The result matrix, <c>X</c>.</returns>
        public Matrix Solve(Matrix matrix, Matrix input)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            var result = (Matrix)matrix.CreateMatrix(input.RowCount, input.ColumnCount);
            Solve(matrix, input, result);
            return result;
        }

        /// <summary>
        /// Solves the matrix equation AX = B, where A is the coefficient matrix, B is the
        /// solution matrix and X is the unknown matrix.
        /// </summary>
        /// <param name="matrix">The coefficient matrix, <c>A</c>.</param>
        /// <param name="input">The solution matrix, <c>B</c>.</param>
        /// <param name="result">The result matrix, <c>X</c></param>
        public void Solve(Matrix matrix, Matrix input, Matrix result)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException("matrix");
            }

            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            if (result == null)
            {
                throw new ArgumentNullException("result");
            }

            if (matrix.RowCount != input.RowCount || input.RowCount != result.RowCount || input.ColumnCount != result.ColumnCount)
            {
                throw Matrix.DimensionsDontMatch<ArgumentException>(matrix, input, result);
            }

            for (var column = 0; column < input.ColumnCount; column++)
            {
                var solution = Solve(matrix, (Vector)input.Column(column));
                foreach (var element in solution.GetIndexedEnumerator())
                {
                    result.At(element.Item1, column, element.Item2);
                }
            }
        }
    }
}

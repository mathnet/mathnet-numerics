// <copyright file="SolverSetup.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://numerics.mathdotnet.com
// http://github.com/mathnet/mathnet-numerics
//
// Copyright (c) 2009-2014 Math.NET
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MathNet.Numerics.LinearAlgebra.Solvers
{
    public static class SolverSetup<T> where T : struct, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Loads the available <see cref="IIterativeSolverSetup{T}"/> objects from the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly which will be searched for setup objects.</param>
        /// <param name="ignoreFailed">If true, types that fail to load are simply ignored. Otherwise the exception is rethrown.</param>
        /// <param name="typesToExclude">The <see cref="IIterativeSolver{T}"/> types that should not be loaded.</param>
        public static IEnumerable<IIterativeSolverSetup<T>> LoadFromAssembly(Assembly assembly, bool ignoreFailed = true, params Type[] typesToExclude)
        {
            Type setupInterfaceType = typeof (IIterativeSolverSetup<T>);
            IEnumerable<Type> candidates = assembly.GetTypes()
                .Where(type => !type.IsAbstract && !type.IsEnum && !type.IsInterface && type.IsVisible)
                .Where(type => type.GetInterfaces().Any(setupInterfaceType.IsAssignableFrom));

            var setups = new List<IIterativeSolverSetup<T>>();
            foreach (var type in candidates)
            {
                try
                {
                    setups.Add((IIterativeSolverSetup<T>) Activator.CreateInstance(type));
                }
                catch
                {
                    if (!ignoreFailed)
                    {
                        throw;
                    }
                }
            }

            var excludedTypes = new List<Type>(typesToExclude);
            return setups
                .Where(s => !excludedTypes.Any(t => t.IsAssignableFrom(s.SolverType) || t.IsAssignableFrom(s.PreconditionerType)))
                .OrderBy(s => s.SolutionSpeed/s.Reliability);
        }

        /// <summary>
        /// Loads the available <see cref="IIterativeSolverSetup{T}"/> objects from the specified assembly.
        /// </summary>
        /// <param name="typeInAssembly">The type in the assembly which should be searched for setup objects.</param>
        /// <param name="ignoreFailed">If true, types that fail to load are simply ignored. Otherwise the exception is rethrown.</param>
        /// <param name="typesToExclude">The <see cref="IIterativeSolver{T}"/> types that should not be loaded.</param>
        public static IEnumerable<IIterativeSolverSetup<T>> LoadFromAssembly(Type typeInAssembly, bool ignoreFailed = true, params Type[] typesToExclude)
        {
            return LoadFromAssembly(typeInAssembly.Assembly, ignoreFailed, typesToExclude);
        }

        /// <summary>
        /// Loads the available <see cref="IIterativeSolverSetup{T}"/> objects from the specified assembly.
        /// </summary>
        /// <param name="assemblyName">The <see cref="AssemblyName"/> of the assembly that should be searched for setup objects.</param>
        /// <param name="ignoreFailed">If true, types that fail to load are simply ignored. Otherwise the exception is rethrown.</param>
        /// <param name="typesToExclude">The <see cref="IIterativeSolver{T}"/> types that should not be loaded.</param>
        public static IEnumerable<IIterativeSolverSetup<T>> LoadFromAssembly(AssemblyName assemblyName, bool ignoreFailed = true, params Type[] typesToExclude)
        {
            return LoadFromAssembly(Assembly.Load(assemblyName.FullName), ignoreFailed, typesToExclude);
        }

        /// <summary>
        /// Loads the available <see cref="IIterativeSolverSetup{T}"/> objects from the Math.NET Numerics assembly.
        /// </summary>
        /// <param name="typesToExclude">The <see cref="IIterativeSolver{T}"/> types that should not be loaded.</param>
        public static IEnumerable<IIterativeSolverSetup<T>> Load(Type[] typesToExclude)
        {
            return LoadFromAssembly(typeof(SolverSetup<T>), false, typesToExclude);
        }

        /// <summary>
        /// Loads the available <see cref="IIterativeSolverSetup{T}"/> objects from the  Math.NET Numerics assembly.
        /// </summary>
        public static IEnumerable<IIterativeSolverSetup<T>> Load()
        {
            return LoadFromAssembly(typeof(SolverSetup<T>), false);
        }
    }
}

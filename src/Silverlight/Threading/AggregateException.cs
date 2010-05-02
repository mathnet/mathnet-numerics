// <copyright file="AggregateException.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
// Copyright (c) 2009 Math.NET
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// </copyright>

namespace MathNet.Numerics.Threading
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Represents multiple errors that occur during application execution.
    /// </summary>
    public class AggregateException : Exception
    {
        /// <summary>
        /// List of the aggregated exceptions.
        /// </summary>
        private readonly IList<Exception> _exceptions = new List<Exception>();

        /// <summary>
        /// Initializes a new instance of the AggregateException class with a specified error message and references to the inner exceptions that are the cause of this exception.
        /// </summary>
        /// <param name="exceptions">The exceptions that are the cause of the current exception.</param>
        public AggregateException(IEnumerable<Exception> exceptions)
        {
            foreach (var exception in exceptions)
            {
                this._exceptions.Add(exception);
            }
        }

        /// <summary>
        /// Gets a read-only collection of the Exception instances that caused the current exception. 
        /// </summary>
        /// <value>A read-only collection of the Exception instances that caused the current exception</value>
        public ReadOnlyCollection<Exception> InnerExceptions
        {
            get
            {
                return new ReadOnlyCollection<Exception>(this._exceptions);
            }
        }
    }
}
// <copyright file="Task.cs" company="Math.NET">
// Math.NET Numerics, part of the Math.NET Project
// http://mathnet.opensourcedotnet.info
//
// Copyright (c) 2009 Math.NET
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

namespace MathNet.Numerics.Threading
{
    using System;
    using System.Threading;

    /// <summary>
    /// Internal Parallel Task Handle.
    /// </summary>
    internal class Task : EventWaitHandle
    {
        /// <summary>
        /// Delegate to the task's action.
        /// </summary>
        private readonly Action _body;

        /// <summary>
        /// Initializes a new instance of the Task class.
        /// </summary>
        /// <param name="body">Delegate to the task's action.</param>
        internal Task(Action body)
            : base(false, EventResetMode.ManualReset)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }

            _body = body;
        }

        /// <summary>
        /// Gets a value indicating whether the task has thrown one or more exceptions while executing.
        /// </summary>
        internal bool ThrewException
        {
            get { return Exception != null; }
        }

        /// <summary>
        /// Gets the exception thrown by the task, if any.
        /// </summary>
        internal Exception Exception { get; private set; }

        /// <summary>
        /// Run the task.
        /// </summary>
        internal void Compute()
        {
            try
            {
                _body();
            }
            catch (Exception e)
            {
                Exception = e;
            }
        }
    }
}
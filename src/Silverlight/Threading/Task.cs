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
    internal class Task
    {
        /// <summary>
        /// Delegate to the task's action.
        /// </summary>
        private readonly System.Action _body;

        /// <summary>
        /// Initializes a new instance of the Task class.
        /// </summary>
        /// <param name="body">Delegate to the task's action.</param>
        public Task(Action body)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }

            _body = body;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Task"/> class.
        /// </summary>
        protected Task()
        {
        }

        /// <summary>
        /// Gets a value indicating whether the task completed due to an unhandled exception.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this task completed due to an unhandled exception; otherwise, <c>false</c>.
        /// </value>
        public bool IsFaulted
        {
            get { return Exception != null; }
        }

        /// <summary>
        /// Gets a value indicating whether this task has completed.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this task has completed; otherwise, <c>false</c>.
        /// </value>
        public bool IsCompleted
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the exception thrown by the task, if any.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Run the task.
        /// </summary>
        public void Compute()
        {
            try
            {
                DoCompute();
                IsCompleted = true;
            }
            catch (Exception e)
            {
                Exception = e;
            }
        }

        /// <summary>
        /// Runs the actual task.
        /// </summary>
        protected virtual void DoCompute()
        {
            _body();
        }

        /// <summary>
        /// Waits for the task to complete execution.
        /// </summary>
        public void Wait()
        {
            while (!IsCompleted && !IsFaulted)
            {
                Thread.Sleep(0);
            }
        }
    }
}
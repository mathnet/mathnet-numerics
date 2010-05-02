// <copyright file="TaskOfT.cs" company="Math.NET">
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

    /// <summary>
    /// Internal Generic Parallel Task Handle.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    internal class Task<TResult> : Task
    {
        /// <summary>
        /// Delegate to the task's action.
        /// </summary>
        private readonly Func<object, TResult> _body;

        /// <summary>
        /// Variable used to hold state information between iterations.
        /// </summary>
        private readonly object _state;

        /// <summary>
        /// Gets the result of the task.
        /// </summary>
        /// <value>The result of the task.</value>
        public TResult Result { get; private set; }

        /// <summary>
        /// Initializes a new instance of the Task class.
        /// </summary>
        /// <param name="body">Delegate to the task's action.</param>
        /// <param name="state">An object representing data to be used by the action.</param>
        public Task(Func<object, TResult> body, object state)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body");
            }

            _state = state;
            _body = body;
        }

        /// <summary>
        /// Runs the actual task.
        /// </summary>
        protected override void DoCompute()
        {
            Result = _body(_state);
        }
    }
}
﻿// Python Tools for Visual Studio
// Copyright(c) Microsoft Corporation
// All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the License); you may not use
// this file except in compliance with the License. You may obtain a copy of the
// License at http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED ON AN  *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS
// OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY
// IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE,
// MERCHANTABLITY OR NON-INFRINGEMENT.
//
// See the Apache Version 2.0 License for specific language governing
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.PythonTools.Interpreter {
    public interface IInterpreterRegistryService {
        /// <summary>
        /// Returns a sequence of available interpreters. The sequence is sorted
        /// and should not be re-sorted if it will be displayed to users.
        /// </summary>
        IEnumerable<IPythonInterpreterFactory> Interpreters { get; }

        IEnumerable<InterpreterConfiguration> Configurations { get; }

        /// <summary>
        /// Returns a sequence of available interpreters. If no interpreters are
        /// available, the sequence contains only
        /// <see cref="NoInterpretersValue"/>.
        /// </summary>
        IEnumerable<IPythonInterpreterFactory> InterpretersOrDefault { get; }

        /// <summary>
        /// Gets the factory that represents the state when no factories are
        /// available.
        /// </summary>
        IPythonInterpreterFactory NoInterpretersValue { get; }

        IPythonInterpreterFactory FindInterpreter(string id);

        InterpreterConfiguration FindConfiguration(string id);

        /// <summary>
        /// Gets a property value relating to a specific interpreter.
        /// 
        /// If the property is not set, returns <c>null</c>.
        /// </summary>
        /// <param name="id">The interpreter identifier.</param>
        /// <param name="propName">A case-sensitive string identifying the
        /// property. Values will be compared by ordinal.</param>
        /// <returns>The property value, or <c>null</c> if not set.</returns>
        object GetProperty(string id, string propName);

        /// <summary>
        /// Raised when the set of interpreters changes. This is not raised when
        /// the set is first initialized.
        /// </summary>
        event EventHandler InterpretersChanged;

        /// <summary>
        /// Called to suppress the <see cref="InterpretersChanged"/> event while
        /// making changes to the registry. If the event is triggered while
        /// suppressed, it will not be raised until suppression is lifted.
        /// 
        /// <see cref="EndSuppressInterpretersChangedEvent"/> must be called
        /// once for every call to this function.
        /// </summary>
        void BeginSuppressInterpretersChangedEvent();

        /// <summary>
        /// Lifts the suppression of the <see cref="InterpretersChanged"/> event
        /// initiated by <see cref="BeginSuppressInterpretersChangedEvent"/>.
        /// 
        /// This must be called once for every call to
        /// <see cref="BeginSuppressInterpretersChangedEvent"/>.
        /// </summary>
        void EndSuppressInterpretersChangedEvent();

        /// <summary>
        /// Marks the factory as locked. Future calls to LockInterpreterAsync
        /// with the same moniker will block until
        /// <see cref="UnlockInterpreter"/> is called. Each call
        /// to LockInterpreterAsync must have a matching call to
        /// <see cref="UnlockInterpreter"/>.
        /// </summary>
        /// <returns>
        /// A cookie representing the current lock. This must be passed to
        /// <see cref="UnlockInterpreter"/>.
        /// </returns>
        /// <remarks>New in 2.1</remarks>
        Task<object> LockInterpreterAsync(IPythonInterpreterFactory factory, object moniker, TimeSpan timeout);

        /// <summary>
        /// Returns true if the factory is locked.
        /// </summary>
        /// <remarks>New in 2.1</remarks>
        bool IsInterpreterLocked(IPythonInterpreterFactory factory, object moniker);

        /// <summary>
        /// Unlocks the factory.
        /// </summary>
        /// <returns>
        /// <c>True</c> if there is nobody waiting on the same moniker and
        /// factory.
        /// </returns>
        /// <remarks>New in 2.1</remarks>
        bool UnlockInterpreter(object cookie);
    }

}

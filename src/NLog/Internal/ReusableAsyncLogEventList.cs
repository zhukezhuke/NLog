﻿// 
// Copyright (c) 2004-2016 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.Collections.Generic;
using NLog.Common;

namespace NLog.Internal
{
    /// <summary>
    /// Controls a single allocated AsyncLogEventInfo-List for reuse (only one active user)
    /// </summary>
    internal class ReusableAsyncLogEventList
    {
        private IList<AsyncLogEventInfo> _list;

        /// <summary>Empty handle when <see cref="Targets.Target.OptimizeBufferReuse"/> is disabled</summary>
        public readonly LockList None = default(LockList);

        public ReusableAsyncLogEventList(int capacity)
        {
            _list = new List<AsyncLogEventInfo>(capacity);
        }

        /// <summary>
        /// Creates handle to the reusable AsyncLogEventInfo-List for active usage
        /// </summary>
        /// <returns>Handle to the reusable item, that can release it again</returns>
        public LockList Allocate()
        {
            return new LockList(this);
        }

        public struct LockList : IDisposable
        {
            /// <summary>
            /// Access the AsyncLogEventInfo[]-buffer acquired
            /// </summary>
            public readonly IList<AsyncLogEventInfo> Result;
            private readonly ReusableAsyncLogEventList _owner;

            public LockList(ReusableAsyncLogEventList owner)
            {
                Result = owner._list;
                owner._list = null;
                _owner = owner;
            }

            public void Dispose()
            {
                if (Result != null)
                {
                    Result.Clear();
                    _owner._list = Result;
                }
            }
        }
    }
}

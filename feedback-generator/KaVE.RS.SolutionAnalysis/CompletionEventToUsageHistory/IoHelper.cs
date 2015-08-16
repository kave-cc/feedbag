/*
 * Copyright 2014 Technische Universitšt Darmstadt
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *    http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KaVE.Commons.Model.Events;
using KaVE.Commons.Model.Events.CompletionEvents;
using KaVE.Commons.Model.ObjectUsage;
using KaVE.Commons.Utils.IO.Archives;

namespace KaVE.RS.SolutionAnalysis.CompletionEventToUsageHistory
{
    public interface IIoHelper
    {
        IList<string> FindExports();
        IList<CompletionEvent> ReadCompletionEvents(string exportFile);

        void OpenCache();
        void AddTuple(Tuple<Query, Query> qt);
        void CloseCache();
    }

    public class IoHelper : IIoHelper
    {
        private readonly string _dirEvents;
        private readonly string _dirHistories;

        private ZipFolderLRUCache<CoReTypeName> _cache;

        public IoHelper(string dirEvents, string dirHistories)
        {
            _dirEvents = dirEvents;
            _dirHistories = dirHistories;
        }

        public IList<string> FindExports()
        {
            return Directory.EnumerateFiles(_dirEvents, "*.zip", SearchOption.AllDirectories).ToList();
        }

        public IList<CompletionEvent> ReadCompletionEvents(string exportFile)
        {
            var events = new List<CompletionEvent>();
            var ra = new ReadingArchive(exportFile);
            while (ra.HasNext())
            {
                var e = ra.GetNext<IDEEvent>();
                var cce = e as CompletionEvent;
                if (cce == null)
                {
                    continue;
                }
                if (!cce.TriggeredAt.HasValue)
                {
                    continue;
                }
                if (!IsCSharpFile(cce))
                {
                    continue;
                }
                events.Add(cce);
            }
            return events;
        }

        private static bool IsCSharpFile(IDEEvent cce)
        {
            var extension = Path.GetExtension(cce.ActiveDocument.FileName);
            var isCSharpFile = extension != null && extension.EndsWith("cs");
            return isCSharpFile;
        }

        public void OpenCache()
        {
            _cache = new ZipFolderLRUCache<CoReTypeName>(_dirHistories, 1000);
        }

        public void AddTuple(Tuple<Query, Query> qt)
        {
            var type = qt.Item1.type;
            _cache.GetArchive(type).Add(qt);
        }

        public void CloseCache()
        {
            _cache.Dispose();
        }
    }
}
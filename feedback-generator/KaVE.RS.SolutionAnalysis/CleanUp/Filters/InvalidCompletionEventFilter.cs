﻿/*
 * Copyright 2014 Technische Universität Darmstadt
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
using System.IO;
using KaVE.Commons.Model.Events;
using KaVE.Commons.Model.Events.CompletionEvents;

namespace KaVE.RS.SolutionAnalysis.CleanUp.Filters
{
    public class InvalidCompletionEventFilter : BaseFilter
    {
        private static readonly Context DefaultContext = new Context();

        public override Func<IDEEvent, bool> Func
        {
            get
            {
                return e =>
                {
                    var ce = e as CompletionEvent;
                    return ce == null || IsValid(ce);
                };
            }
        }

        private static bool IsValid(CompletionEvent ce)
        {
            return IsCSharpFile(ce) && HasNonDefaultContext(ce);
        }

        private static bool IsCSharpFile(IDEEvent e)
        {
            var extension = Path.GetExtension(e.ActiveDocument.FileName);
            var isCSharpFile = extension != null && extension.EndsWith("cs");
            return isCSharpFile;
        }

        private static bool HasNonDefaultContext(CompletionEvent ce)
        {
            return !DefaultContext.Equals(ce.Context2);
        }
    }
}
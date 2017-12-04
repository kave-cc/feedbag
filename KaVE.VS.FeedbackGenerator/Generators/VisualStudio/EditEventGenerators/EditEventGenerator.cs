/*
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
using EnvDTE;
using JetBrains.Application.Threading;
using JetBrains.ProjectModel;
using JetBrains.Threading;
using KaVE.Commons.Model.Events.CompletionEvents;
using KaVE.Commons.Model.Events.VisualStudio;
using KaVE.Commons.Utils;
using KaVE.VS.Commons;
using KaVE.VS.Commons.Generators;
using KaVE.VS.FeedbackGenerator.Generators.VisualStudio.EditEventGenerators.EventContext;

namespace KaVE.VS.FeedbackGenerator.Generators.VisualStudio.EditEventGenerators
{
    [SolutionComponent]
    internal class EditEventGenerator : EventGeneratorBase
    {
        // TODO evaluate good threshold value
        private static readonly TimeSpan InactivityPeriodToCompleteEditAction = TimeSpan.FromSeconds(3);

        private readonly IDateUtils _dateUtils;
        private readonly ContextGenerator _contextGenerator;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly TextEditorEvents _textEditorEvents;

        private DateTimeOffset _lastFired = DateTimeOffset.MinValue;
        private Context _lastCtx = Context.Default;
        private int _numChanges;


        public EditEventGenerator(IRSEnv env,
            IMessageBus messageBus,
            IDateUtils dateUtils,
            ContextGenerator contextGenerator,
            IThreading threading)
            : base(env, messageBus, dateUtils, threading)
        {
            _dateUtils = dateUtils;
            _contextGenerator = contextGenerator;
            _textEditorEvents = DTE.Events.TextEditorEvents;
            _textEditorEvents.LineChanged += TextEditorEvents_LineChanged;
            // TODO event happens before change is reflected... think about how to capture the
            // state *after* the change, e.g., by scheduling an artificial event or by attaching
            // this generator also to file save/close events
        }

        private void TextEditorEvents_LineChanged(TextPoint startPoint, TextPoint endPoint, int hint)
        {
            ReentrancyGuard.Current.ExecuteOrQueue(
                "KaVE.EditEventGenerator",
                () =>
                {
                    _numChanges++;

                    // prevent storage of tiny deltas
                    var timeSinceLastFire = _dateUtils.Now - _lastFired;
                    if (timeSinceLastFire < InactivityPeriodToCompleteEditAction)
                    {
                        return;
                    }

                    var ctx = Context.Default;

                    var activeDocument = startPoint.DTE.ActiveDocument;
                    if (activeDocument != null)
                    {
                        ctx = _contextGenerator.RunContextAnalysis() ?? Context.Default;
                    }

                    // prevent edits that are unreflected in SSTs (e.g., whitespace)
                    if (ctx.Equals(_lastCtx))
                    {
                        return;
                    }

                    var e = Create<EditEvent>();
                    e.NumberOfChanges = _numChanges;
                    e.Context2 = _lastCtx = ctx;

                    FireNow(e);

                    if (e.TriggeredAt.HasValue)
                    {
                        _lastFired = e.TriggeredAt.Value;
                    }
                    _lastCtx = e.Context2;
                    _numChanges = 0;
                });
        }
    }
}
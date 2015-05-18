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
 * 
 * Contributors:
 *    - Mattis Manfred Kämmerer
 *    - Markus Zimmermann
 */

using System.Collections.Generic;
using System.Linq;
using KaVE.Commons.Model.Events;
using KaVE.FeedbackProcessor.Cleanup.Heuristics;
using KaVE.FeedbackProcessor.Model;

namespace KaVE.FeedbackProcessor.Cleanup.Processors
{
    internal class ToConcurrentEventMapper : BaseEventMapper
    {
        private readonly IList<IDEEvent> _eventCache = new List<IDEEvent>();

        public ToConcurrentEventMapper()
        {
            RegisterFor<IDEEvent>(ProcessIDEEvent);
        }

        private void ProcessIDEEvent(IDEEvent @event)
        {
            var lastEvent = _eventCache.LastOrDefault();
            if (lastEvent != null && !ConcurrentEventHeuristic.AreConcurrent(lastEvent, @event))
            {
                if (_eventCache.Count > 1)
                {
                    ReplaceCurrentEventWith(GenerateConcurrentEvent());
                }
                _eventCache.Clear();
            }
            _eventCache.Add(@event);
        }

        private ConcurrentEvent GenerateConcurrentEvent()
        {
            var firstConcurrentEvent = _eventCache.First();
            var lastConcurrentEvent = _eventCache.Last();
            return new ConcurrentEvent
            {
                ConcurrentEventList = new List<IDEEvent>(_eventCache),
                IDESessionUUID = firstConcurrentEvent.IDESessionUUID,
                Id = firstConcurrentEvent.Id,
                TriggeredAt = firstConcurrentEvent.TriggeredAt,
                TriggeredBy = firstConcurrentEvent.TriggeredBy,
                Duration = lastConcurrentEvent.TerminatedAt - firstConcurrentEvent.TriggeredAt,
                KaVEVersion = firstConcurrentEvent.KaVEVersion
            };
        }
    }
}
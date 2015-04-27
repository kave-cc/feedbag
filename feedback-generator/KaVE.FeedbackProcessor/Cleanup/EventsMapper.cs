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
 *    - Sven Amann
 *    - Mattis Manfred Kämmerer
 *    - Markus Zimmermann
 */

using System;
using System.Collections.Generic;
using System.Linq;
using KaVE.Commons.Model.Events;
using KaVE.Commons.Utils.Collections;
using KaVE.FeedbackProcessor.Database;
using KaVE.FeedbackProcessor.Model;

namespace KaVE.FeedbackProcessor.Cleanup
{
    internal class EventsMapper
    {
        private readonly IFeedbackDatabase _sourceDatabase;
        private readonly IFeedbackDatabase _targetDatabase;
        private readonly ICollection<Type> _processors;

        public EventsMapper(IFeedbackDatabase sourceDatabase, IFeedbackDatabase targetDatabase)
        {
            _sourceDatabase = sourceDatabase;
            _targetDatabase = targetDatabase;
            _processors = new List<Type>();
        }

        public void RegisterProcessor<TP>() where TP : IIDEEventProcessor, new()
        {
            _processors.Add(typeof (TP));
        }

        public void ProcessFeedback()
        {
            _targetDatabase.GetDeveloperCollection().Clear();
            _targetDatabase.GetEventsCollection().Clear();

            var developers = _sourceDatabase.GetDeveloperCollection().FindAll();
            foreach (var developer in developers)
            {
                _targetDatabase.GetDeveloperCollection().Insert(developer);
                var processors = CreateProcessors();
                SetDeveloper(developer, processors);
                ProcessEventStreamOf(developer, processors);
            }
        }

        private static void SetDeveloper(Developer developer, IEnumerable<IIDEEventProcessor> processors)
        {
            foreach (var processor in processors)
            {
                processor.Developer = developer;
            }
        }

        private void ProcessEventStreamOf(Developer developer, List<IIDEEventProcessor> processors)
        {
            foreach (var ideEvent in GetAllEventsOf(developer))
            {
                ProcessEvent(ideEvent, processors);
            }
        }

        private List<IIDEEventProcessor> CreateProcessors()
        {
            return _processors.Select(Activator.CreateInstance).Cast<IIDEEventProcessor>().ToList();
        }

        private IEnumerable<IDEEvent> GetAllEventsOf(Developer developer)
        {
            var events = _sourceDatabase.GetEventsCollection();
            return events.GetEventStream(developer);
        }

        private void ProcessEvent(IDEEvent originalEvent, IEnumerable<IIDEEventProcessor> processors)
        {
            var resultingEventSet = new KaVEHashSet<IDEEvent>();
            var dropOriginalEvent = false;

            foreach (var intermediateEventSet in processors.Select(processor => processor.Process(originalEvent)))
            {
                if (IsDropOriginalEventSignal(intermediateEventSet, originalEvent))
                {
                    dropOriginalEvent = true;
                }
                resultingEventSet.UnionWith(intermediateEventSet);
            }

            if (dropOriginalEvent)
            {
                resultingEventSet.Remove(originalEvent);
            }

            InsertEventsToTargetEventCollection(resultingEventSet);
        }

        private void InsertEventsToTargetEventCollection(IEnumerable<IDEEvent> resultingEventSet)
        {
            foreach (var ideEvent in resultingEventSet)
            {
                _targetDatabase.GetEventsCollection().Insert(ideEvent);
            }
        }

        private static bool IsDropOriginalEventSignal(ICollection<IDEEvent> eventSet, IDEEvent originalEvent)
        {
            return !eventSet.Contains(originalEvent);
        }
    }
}
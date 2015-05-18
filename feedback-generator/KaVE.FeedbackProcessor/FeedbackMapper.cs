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

namespace KaVE.FeedbackProcessor
{
    internal class FeedbackMapper
    {
        private readonly IFeedbackDatabase _sourceDatabase;
        private readonly IFeedbackDatabase _targetDatabase;
        private readonly ICollection<Type> _mappers;

        public FeedbackMapper(IFeedbackDatabase sourceDatabase, IFeedbackDatabase targetDatabase)
        {
            _sourceDatabase = sourceDatabase;
            _targetDatabase = targetDatabase;
            _mappers = new List<Type>();
        }

        public void RegisterMapper<TP>() where TP : IEventMapper<IKaVESet<IDEEvent>>, new()
        {
            _mappers.Add(typeof (TP));
        }

        public void MapFeedback()
        {
            _targetDatabase.GetDeveloperCollection().Clear();
            _targetDatabase.GetEventsCollection().Clear();

            var developers = _sourceDatabase.GetDeveloperCollection().FindAll();
            foreach (var developer in developers)
            {
                MapFeedbackOf(developer);
            }
        }

        private void MapFeedbackOf(Developer developer)
        {
            _targetDatabase.GetDeveloperCollection().Insert(developer);

            var processors = CreateMappers();
            NotifyEventStreamStarts(processors, developer);
            foreach (var ideEvent in GetEventStream(developer))
            {
                MapEvent(processors, ideEvent);
            }
        }

        private List<IEventMapper<IKaVESet<IDEEvent>>> CreateMappers()
        {
            return _mappers.Select(Activator.CreateInstance).Cast<IEventMapper<IKaVESet<IDEEvent>>>().ToList();
        }

        private static void NotifyEventStreamStarts(IEnumerable<IEventMapper<IKaVESet<IDEEvent>>> mappers, Developer developer)
        {
            foreach (var mapper in mappers)
            {
                mapper.OnStreamStarts(developer);
            }
        }

        private IEnumerable<IDEEvent> GetEventStream(Developer developer)
        {
            var events = _sourceDatabase.GetEventsCollection();
            return events.GetEventStream(developer);
        }

        private void MapEvent(IEnumerable<IEventMapper<IKaVESet<IDEEvent>>> mappers, IDEEvent originalEvent)
        {
            var resultingEventSet = new KaVEHashSet<IDEEvent>();
            var dropOriginalEvent = false;

            foreach (var intermediateEventSet in mappers.Select(mapper => mapper.Map(originalEvent)))
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
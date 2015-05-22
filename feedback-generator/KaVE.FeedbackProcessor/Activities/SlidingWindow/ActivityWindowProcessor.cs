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
 */

using System;
using System.Collections.Generic;
using KaVE.FeedbackProcessor.Activities.Model;
using KaVE.FeedbackProcessor.Model;
using KaVE.FeedbackProcessor.Utils;

namespace KaVE.FeedbackProcessor.Activities.SlidingWindow
{
    internal class ActivityWindowProcessor : BaseEventProcessor
    {
        private readonly IActivityMergeStrategy _strategy;
        private readonly TimeSpan _windowSpan;

        private Window _currentWindow;
        private Developer _currentDeveloper;

        public ActivityWindowProcessor(IActivityMergeStrategy strategy, TimeSpan windowSpan)
        {
            _strategy = strategy;
            _windowSpan = windowSpan;
            ActivityStreams = new Dictionary<Developer, IDictionary<DateTime, ActivityStream>>();
            RegisterFor<ActivityEvent>(ProcessActivities);
        }

        public IDictionary<Developer, IDictionary<DateTime, ActivityStream>> ActivityStreams { get; private set; }

        public override void OnStreamStarts(Developer developer)
        {
            _currentDeveloper = developer;
            ActivityStreams[developer] = new Dictionary<DateTime, ActivityStream>();
            _currentWindow = null;
        }

        private void ProcessActivities(ActivityEvent @event)
        {
            if (_currentWindow == null)
            {
                _currentWindow = CreateWindowStartingAt(@event);
            }

            while (_currentWindow.EndsBeforeStartOf(@event))
            {
                ProceedToNextWindow(@event);
            }

            while (_currentWindow.EndsBeforeEndOf(@event))
            {
                var headAndTail = SplitAt(@event, _currentWindow.End);
                _currentWindow.Add(headAndTail.Item1);

                ProceedToNextWindow(@event);

                @event = headAndTail.Item2;
            }

            _currentWindow.Add(@event);
        }

        private void ProceedToNextWindow(ActivityEvent @event)
        {
            if (_currentWindow.IsNotEmpty || EmptyWindowRequired(@event))
            {
                AppendMergedWindowToStream();
            }

            if (_currentWindow.IsOnSameDayAs(@event))
            {
                _currentWindow = CreateFollowingWindow();
            }
            else
            {
                _currentWindow = CreateWindowStartingAt(@event);
            }
        }

        private bool EmptyWindowRequired(ActivityEvent @event)
        {
            return _currentWindow.IsOnSameDayAs(@event);
        }

        private static Pair<ActivityEvent> SplitAt(ActivityEvent activityEvent, DateTime at)
        {
            var head = Clone(activityEvent);
            head.TerminatedAt = at;
            var tail = Clone(activityEvent);
            tail.TriggeredAt = head.TerminatedAt;
            tail.Duration = activityEvent.Duration - head.Duration;
            return new Pair<ActivityEvent>(head, tail);
        }

        private static ActivityEvent Clone(ActivityEvent originalEvent)
        {
            var clone = new ActivityEvent
            {
                Activity = originalEvent.Activity
            };
            clone.CopyIDEEventPropertiesFrom(originalEvent);
            return clone;
        }

        private void AppendMergedWindowToStream()
        {
            if (!ActivityStreams[_currentDeveloper].ContainsKey(_currentWindow.Start.Date))
            {
                ActivityStreams[_currentDeveloper][_currentWindow.Start.Date] = new ActivityStream(_windowSpan);
            }
            ActivityStreams[_currentDeveloper][_currentWindow.Start.Date].Add(
                _strategy.Merge(_currentWindow));
            _strategy.Reset();
        }

        private Window CreateWindowStartingAt(ActivityEvent @event)
        {
            return CreateWindowStartingAt(@event.GetTriggeredAt());
        }

        private Window CreateFollowingWindow()
        {
            return CreateWindowStartingAt(_currentWindow.End);
        }

        private Window CreateWindowStartingAt(DateTime windowStart)
        {
            return new Window(windowStart, _windowSpan);
        }

        public override void OnStreamEnds()
        {
            if (_currentWindow != null)
            {
                AppendMergedWindowToStream();
            }
        }
    }
}
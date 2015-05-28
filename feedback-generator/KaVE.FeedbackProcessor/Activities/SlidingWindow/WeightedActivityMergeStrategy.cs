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

using System.Collections.Generic;
using System.Linq;
using KaVE.FeedbackProcessor.Activities.Model;

namespace KaVE.FeedbackProcessor.Activities.SlidingWindow
{
    internal abstract class WeightedActivityMergeStrategy : IActivityMergeStrategy
    {
        private Activity? _lastActivity;

        public Activity Merge(Window window)
        {
            _lastActivity = window.IsNotEmpty ? MergeNonEmptyWindow(window) : Activity.Waiting;
            return _lastActivity.Value;
        }

        private static bool IsEmptyWindow(ICollection<ActivityEvent> window)
        {
            return window.Count == 0;
        }

        private Activity MergeNonEmptyWindow(Window window)
        {
            var windowWithoutAny = WithoutAnyActivity(window);
            if (!IsEmptyWindow(windowWithoutAny))
            {
                return SelectsRepresentativeActivity(windowWithoutAny);
            }
            if (_lastActivity == null || IsInactivity(_lastActivity))
            {
                return Activity.Other;
            }
            return _lastActivity.Value;
        }

        private Activity SelectsRepresentativeActivity(IList<ActivityEvent> window)
        {
            var representativeActivity = Activity.Any;
            var maxActivityWeight = 0;
            foreach (var activity in window.Select(e => e.Activity))
            {
                var weightOfActivity = GetWeightOfActivity(window, activity);
                if (weightOfActivity >= maxActivityWeight)
                {
                    representativeActivity = activity;
                    maxActivityWeight = weightOfActivity;
                }
            }
            return representativeActivity;
        }

        protected abstract int GetWeightOfActivity(IList<ActivityEvent> window, Activity activity);

        private static IList<ActivityEvent> WithoutAnyActivity(Window window)
        {
            return window.Events.Where(e => e.Activity != Activity.Any).ToList();
        }

        private static bool IsInactivity(Activity? lastActivity)
        {
            return lastActivity == Activity.Away || lastActivity == Activity.Waiting;
        }

        public void Reset()
        {
            _lastActivity = null;
        }
    }
}
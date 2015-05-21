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
using KaVE.FeedbackProcessor.Activities.SlidingWindow;
using NUnit.Framework;

namespace KaVE.FeedbackProcessor.Tests.Activities.SlidingWindow
{
    [TestFixture]
    internal class FrequencyActivityMergeStrategyTest
    {
        private FrequencyActivityMergeStrategy _uut;

        [SetUp]
        public void SetUp()
        {
            _uut = new FrequencyActivityMergeStrategy();
        }

        [Test]
        public void ReturnsWaitingForEmptyWindow()
        {
            var actual = _uut.Merge(EmptyWindow());

            Assert.AreEqual(Activity.Waiting, actual);
        }

        [Test]
        public void PicksMostFrequentActivity()
        {
            var actual = _uut.Merge(
                Window(Activity.Development, Activity.Any, Activity.Development, Activity.Navigation));

            Assert.AreEqual(Activity.Development, actual);
        }

        [Test]
        public void PicksLaterActivityOnEqualsFrequence()
        {
            var actual = _uut.Merge(Window(Activity.Development, Activity.Navigation));

            Assert.AreEqual(Activity.Navigation, actual);
        }

        [Test]
        public void IgnoresAnyActivity()
        {
            var actual = _uut.Merge(Window(Activity.Any, Activity.Any, Activity.Navigation));

            Assert.AreEqual(Activity.Navigation, actual);
        }

        [Test]
        public void MapsOnlyAnyToOtherByDefault()
        {
            var actual = _uut.Merge(Window(Activity.Any));

            Assert.AreEqual(Activity.Other, actual);
        }

        [Test]
        public void MapsOnlyAnyToPrevious()
        {
            var previous = _uut.Merge(Window(Activity.Development));
            var actual = _uut.Merge(Window(Activity.Any));

            Assert.AreEqual(previous, actual);
        }

        [Test]
        public void ClearsPreviousOnReset()
        {
            _uut.Merge(Window(Activity.Development));
            _uut.Reset();
            var actual = _uut.Merge(Window(Activity.Any));

            Assert.AreEqual(Activity.Other, actual);
        }

        [TestCase(Activity.Waiting), TestCase(Activity.Away)]
        public void MapsOnlyAnyToOtherIfPreviousIsWaitingOrAway(Activity waitingOrAway)
        {
            _uut.Merge(Window(waitingOrAway));
            var actual = _uut.Merge(Window(Activity.Any));

            Assert.AreEqual(Activity.Other, actual);
        }

        private IList<Activity> EmptyWindow()
        {
            return Window( /* no activities */);
        }

        private IList<Activity> Window(params Activity[] activities)
        {
            return activities.ToList();
        }
    }
}
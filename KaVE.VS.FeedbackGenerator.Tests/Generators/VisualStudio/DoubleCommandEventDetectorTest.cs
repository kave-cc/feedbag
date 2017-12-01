/*
 * Copyright 2017 University of Zurich
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
using KaVE.Commons.TestUtils.Utils;
using KaVE.VS.FeedbackGenerator.Generators.VisualStudio;
using NUnit.Framework;

namespace KaVE.VS.FeedbackGenerator.Tests.Generators.VisualStudio
{
    internal class DoubleCommandEventDetectorTest
    {
        private const int Timeout = DoubleCommandEventDetector.TimeForDoubledEventInMs + 1;

        #region setup and utils

        private DateTimeOffset _now;
        private TestDateUtils _dateUtils;
        private DoubleCommandEventDetector _sut;

        [SetUp]
        public void SetUp()
        {
            _now = DateTimeOffset.Now;

            _dateUtils = new TestDateUtils();
            _dateUtils.Now = TimeInMs(0);

            _sut = new DoubleCommandEventDetector(_dateUtils);
        }

        private DateTimeOffset TimeInMs(int delta)
        {
            return _now.AddMilliseconds(delta);
        }

        private bool Event(string id, int time)
        {
            _dateUtils.Now = TimeInMs(time);
            return _sut.ShouldProcess(id);
        }

        private void AssertTrue(string id, int time)
        {
            Assert.IsTrue(Event(id, time));
        }

        private void AssertFalse(string id, int time)
        {
            Assert.IsFalse(Event(id, time));
        }

        #endregion

        [Test]
        public void AfterInit()
        {
            AssertTrue("x", 2);
        }

        [Test]
        public void RegularEvent()
        {
            Event("y", 0);
            AssertTrue("x", 1000);
        }

        [Test]
        public void SameEventLongDistance()
        {
            Event("x", 0);
            AssertTrue("x", 1000);
        }

        [Test]
        public void DifferentEventShortDistance()
        {
            Event("x", 0);
            AssertTrue("y", 10);
        }

        [Test]
        public void SameEventShortDistance()
        {
            Event("x", 0);
            AssertFalse("x", DoubleCommandEventDetector.TimeForDoubledEventInMs);
        }

        [Test]
        public void SameEventSameTime()
        {
            Event("x", 0);
            AssertFalse("x", 0);
        }

        [Test]
        public void SameEventPast()
        {
            Event("x", 1000);
            AssertFalse("x", 0);
        }

        [Test]
        public void IntermediateEventsResetCounter()
        {
            Event("x", 0);
            AssertTrue("y", 10);
            AssertTrue("x", 20);
        }

        [Test]
        public void SameEventRepeatedShortDistance()
        {
            Event("x", 0);
            for (var i = 0; i < 10; i++)
            {
                var delta = i * (Timeout - 1);
                AssertFalse("x", delta);
            }
            AssertTrue("x", 10 * (Timeout - 1) + 1);
        }

        [Test]
        public void SameEventJustAboveThreshold()
        {
            Event("x", 0);
            AssertTrue("x", Timeout);
        }

        [Test]
        public void ComplexExample()
        {
            Event("x", 0);
            AssertTrue("y", Timeout);
            AssertTrue("y", Timeout + Timeout);
            AssertFalse("y", Timeout + Timeout + 10);
            AssertTrue("x", Timeout + Timeout + 10 + 20);
            AssertFalse("x", Timeout + Timeout + 10 + 20 + 20);
        }
    }
}
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
using KaVE.FeedbackProcessor.Intervals;
using KaVE.FeedbackProcessor.Tests.Intervals.Transformers;
using NUnit.Framework;

namespace KaVE.FeedbackProcessor.Tests.Intervals
{
    internal class TransformerUtilsTest : TransformerTestBase<TestInterval>
    {
        [Test]
        public void CreatesIntervalCorrectly()
        {
            var i = TransformerUtils.CreateIntervalFromEvent<TestInterval>(TestIDEEvent(0, 1, "a"));
            Assert.AreEqual(TestTime(0), i.StartTime);
            Assert.AreEqual(TimeSpan.FromMinutes(1), i.Duration);
            Assert.AreEqual("a", i.IDESessionId);
        }

        [Test]
        public void AdaptsIntervalCorrectly()
        {
            var i = new TestInterval {StartTime = TestTime(0), Duration = TimeSpan.FromMinutes(1)};
            TransformerUtils.AdaptIntervalTimeData(i, TestIDEEvent(1, 2));

            Assert.AreEqual(TestTime(0), i.StartTime);
            Assert.AreEqual(TimeSpan.FromMinutes(2), i.Duration);
        }

        [Test]
        public void DoesNotMakeIntervalShorter()
        {
            var i = new TestInterval { StartTime = TestTime(0), Duration = TimeSpan.FromMinutes(5) };
            TransformerUtils.AdaptIntervalTimeData(i, TestIDEEvent(1, 2));

            Assert.AreEqual(TestTime(0), i.StartTime);
            Assert.AreEqual(TimeSpan.FromMinutes(5), i.Duration);
        }
    }
}
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
 *    - Markus Zimmermann
 */

using System;
using KaVE.Commons.Utils.Assertion;
using KaVE.FeedbackProcessor.Cleanup.Heuristics;
using NUnit.Framework;

namespace KaVE.FeedbackProcessor.Tests.Cleanup.Heuristics
{
    [TestFixture]
    class ConcurrentEventHeuristicTest
    {
        [Test]
        public void GetValidEventTimeTest()
        {
            DateTime? value = new DateTime(1984, 1, 1);

            Assert.AreEqual(new DateTime(1984, 1, 1),ConcurrentEventHeuristic.GetValidEventTime(value));
        }

        [Test,
         ExpectedException(typeof (AssertException),
             ExpectedMessage = "Events should have a DateTime value in TriggeredAt")]
        public void ThrowsOnNonValidDateTime()
        {
            DateTime? value = null;

            ConcurrentEventHeuristic.GetValidEventTime(value);
        }

        [Test]
        public void HaveSimiliarEventTimeTest()
        {
            TimeSpan eventTimeDifference = new TimeSpan(0, 0, 0, 0, 20);
            DateTime eventTime1 = new DateTime(1984, 1, 1, 1, 1, 1, 1);
            DateTime eventTime2 = new DateTime(1984, 1, 1, 1, 1, 1, 10);

            Assert.IsTrue(ConcurrentEventHeuristic.HaveSimiliarEventTime(eventTime1,eventTime2,eventTimeDifference));
        }

    }
}
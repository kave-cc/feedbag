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
 */

using System;
using KaVE.Commons.Model.Events;
using KaVE.Commons.Utils.Collections;
using KaVE.FeedbackProcessor.Cleanup.Processors;
using NUnit.Framework;

namespace KaVE.FeedbackProcessor.Tests.Cleanup.Processors
{
    [TestFixture]
    internal class DuplicateCommandFilterProcessorTest
    {
        private DuplicateCommandFilterProcessor _uut;

        [SetUp]
        public void Setup()
        {
            _uut = new DuplicateCommandFilterProcessor();
        }

        [Test]
        public void FiltersDuplicatedCommandEvents()
        {
            var firstEventTime = new DateTime(1984,2,2,1,1,1,0);
            var commandEvent = new CommandEvent
            {
                CommandId = "Test",
                TriggeredAt = firstEventTime
            };            
            var commandEvent2 = new CommandEvent
            {
                CommandId = "Test",
                TriggeredAt = firstEventTime + DuplicateCommandFilterProcessor.CommandEventTimeDifference
            };
            var commandEvent3 = new CommandEvent
            {
                CommandId = "Test",
                TriggeredAt = firstEventTime + DuplicateCommandFilterProcessor.CommandEventTimeDifference.Add(new TimeSpan(TimeSpan.TicksPerSecond))
            };

            Assert.AreEqual(Sets.NewHashSet<IDEEvent>(commandEvent),_uut.Process(commandEvent));
            Assert.AreEqual(Sets.NewHashSet<IDEEvent>(), _uut.Process(commandEvent2));
            Assert.AreEqual(Sets.NewHashSet<IDEEvent>(commandEvent3), _uut.Process(commandEvent3));
        }



        [Test]
        public void ShouldNotFilterAnyOtherCommandEvents()
        {
            var commandEvent = new CommandEvent { CommandId = "Test" };
            Assert.AreEqual(Sets.NewHashSet<IDEEvent>(commandEvent), _uut.Process(commandEvent));
        }
    }
}
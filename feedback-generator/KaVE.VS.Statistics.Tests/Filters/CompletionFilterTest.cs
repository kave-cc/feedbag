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
using KaVE.Commons.Model.Events;
using KaVE.Commons.Model.Events.CompletionEvents;
using KaVE.Commons.Model.Events.VisualStudio;
using KaVE.VS.Statistics.Filters;
using NUnit.Framework;

namespace KaVE.VS.Statistics.Tests.Filters
{
    [TestFixture]
    internal class CompletionFilterTest
    {
        [SetUp]
        public void Init()
        {
            _completionPreprocessor = new CompletionPreprocessor();
        }

        private CompletionPreprocessor _completionPreprocessor;

        private static readonly CompletionEvent AppliedCompletionEvent = new CompletionEvent
        {
            TerminatedState = TerminationState.Applied
        };

        private static readonly CompletionEvent FilteredCompletionEvent = new CompletionEvent
        {
            TerminatedState = TerminationState.Filtered
        };

        private static readonly CompletionEvent CancelledCompletionEvent = new CompletionEvent
        {
            TerminatedState = TerminationState.Cancelled
        };

        private static readonly object[] FiltersFilteredCompletionEventsTestCases =
        {
            new object[]
            {
                AppliedCompletionEvent,
                false
            },
            new object[]
            {
                CancelledCompletionEvent,
                false
            },
            new object[]
            {
                FilteredCompletionEvent,
                true
            }
        };

        [Test]
        public void AccumulatesTimesForCompletionEventsTest()
        {
            var filteredCompletionEvent = new CompletionEvent
            {
                TerminatedState = TerminationState.Filtered,
                Duration = new TimeSpan(0, 0, 10)
            };
            var cancelledCompletionEvent = new CompletionEvent
            {
                TerminatedState = TerminationState.Cancelled,
                Duration = new TimeSpan(0, 0, 10)
            };

            _completionPreprocessor.Preprocess(filteredCompletionEvent);
            var actualEvent = _completionPreprocessor.Preprocess(cancelledCompletionEvent);

            var expectedDuration = new TimeSpan(0, 0, 20);
            Assert.AreEqual(expectedDuration, actualEvent.Duration);
        }

        [Test, TestCaseSource("FiltersFilteredCompletionEventsTestCases")]
        public void FiltersFilteredCompletionEventsTest(CompletionEvent completionEvent, bool isFilteredEvent)
        {
            var actualEvent = (CompletionEvent) _completionPreprocessor.Preprocess(completionEvent);

            if (isFilteredEvent)
            {
                Assert.IsNull(actualEvent);
            }
            else
            {
                Assert.AreEqual(completionEvent, actualEvent);
            }
        }

        [Test]
        public void FiltersOtherEvents()
        {
            Assert.Null(_completionPreprocessor.Preprocess(new CommandEvent()));
            Assert.Null(_completionPreprocessor.Preprocess(new SolutionEvent()));
            Assert.Null(_completionPreprocessor.Preprocess(new EditEvent()));
            Assert.Null(_completionPreprocessor.Preprocess(new DebuggerEvent()));
            Assert.Null(_completionPreprocessor.Preprocess(new BuildEvent()));
            Assert.Null(_completionPreprocessor.Preprocess(new WindowEvent()));
            Assert.Null(_completionPreprocessor.Preprocess(new DocumentEvent()));
        }
    }
}
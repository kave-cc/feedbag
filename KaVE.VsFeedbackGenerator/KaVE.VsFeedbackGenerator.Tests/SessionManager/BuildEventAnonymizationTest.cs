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
using JetBrains.Util;
using KaVE.Model.Events.VisualStudio;
using NUnit.Framework;

namespace KaVE.VsFeedbackGenerator.Tests.SessionManager
{
    [TestFixture]
    class BuildEventAnonymizationTest : IDEEventAnonymizerTestBase<BuildEvent>
    {
        private const string TestTargetProjectName = "ProjectName";
        private const string TestTargetProjectNameHash = "0Wc0SWJ1Vy6bpzAFL2QHMg==";

        protected override BuildEvent CreateEventWithAllAnonymizablePropertiesSet()
        {
            return new BuildEvent
            {
                Scope = "Build All",
                Action = "Rebuild",
                Targets = new[]
                {
                    CreateBuildTarget(),
                    CreateBuildTarget(),
                    CreateBuildTarget()
                }
            };
        }

        protected BuildTarget CreateBuildTarget()
        {
            return new BuildTarget
            {
                StartedAt = DateTime.Now,
                Duration = TimeSpan.FromMinutes(42),
                Platform = "x86",
                Project = TestTargetProjectName,
                ProjectConfiguration = "Debug",
                SolutionConfiguration = "Release",
                Successful = true
            };
        }

        [Test]
        public void ShouldRemoveBuildTargetStartTimesWhenRemovingStartTimes()
        {
            ExportSettings.RemoveStartTimes = true;

            var actual = WhenEventIsAnonymized();

            AssertForEachTargetThat(actual, target => Assert.IsNull(target.StartedAt));
        }

        [Test]
        public void ShouldRemoveBuildTargetDurationsWhenRemovingDurations()
        {
            ExportSettings.RemoveDurations = true;

            var actual = WhenEventIsAnonymized();

            AssertForEachTargetThat(actual, target => Assert.IsNull(target.Duration));
        }

        [Test]
        public void ShouldAnonymizeProjectNameFromBuildTargetsWhenRemovingNames()
        {
            ExportSettings.RemoveCodeNames = true;

            var actual = WhenEventIsAnonymized();

            AssertForEachTargetThat(actual, target => Assert.AreEqual(TestTargetProjectNameHash, target.Project));
        }

        protected override void AssertThatPropertiesThatAreNotTouchedByAnonymizationAreUnchanged(BuildEvent target, BuildEvent actual)
        {
            Assert.AreEqual(target.Scope, actual.Scope);
            Assert.AreEqual(target.Action, actual.Action);
            Assert.AreEqual(target.Targets.Count, actual.Targets.Count);
            // TODO @Sven: assert properties of the targets
        }

        private static void AssertForEachTargetThat(BuildEvent buildEvent, Action<BuildTarget> assertion)
        {
            buildEvent.Targets.ForEach(assertion);
        }
    }
}

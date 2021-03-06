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

using KaVE.Commons.Model.Events.VisualStudio;
using KaVE.Commons.Model.Naming;
using NUnit.Framework;

namespace KaVE.VS.FeedbackGenerator.Tests.SessionManager.Anonymize
{
    internal class IDEStateEventAnonymizerTest : IDEEventAnonymizerTestBase<IDEStateEvent>
    {
        protected override IDEStateEvent CreateEventWithAllAnonymizablePropertiesSet()
        {
            return new IDEStateEvent
            {
                IDELifecyclePhase = IDELifecyclePhase.Shutdown,
                OpenDocuments =
                    new[]
                    {
                        Names.Document("CSharp C:\\File.cs"),
                        Names.Document("CSharp C:\\AnotherFile.cs")
                    },
                OpenWindows =
                    new[]
                    {
                        Names.Window("vsWinType Solution Explorer"),
                        Names.Window("vsToolWindow Unit Test Sessions"),
                        Names.Window("vsEditor File.cs")
                    }
            };
        }

        [Test]
        public void ShouldAnonymizeOpenDocumentWhenRemoveNamesIsSet()
        {
            AnonymizationSettings.RemoveCodeNames = true;

            var actual = WhenEventIsAnonymized();

            Assert.AreEqual("CSharp ixlmuLAuUg0yq59EtLWB7w==", actual.OpenDocuments[0].Identifier);
            Assert.AreEqual("CSharp e1wt1Tr04XQ1wYxVobjmBw==", actual.OpenDocuments[1].Identifier);
        }

        [Test]
        public void ShouldAnonymizeOpenWindowsWhenRemoveNamesIsSetAndWindowCaptionContainsFileName()
        {
            AnonymizationSettings.RemoveCodeNames = true;

            var actual = WhenEventIsAnonymized();

            Assert.AreEqual("vsWinType Solution Explorer", actual.OpenWindows[0].Identifier);
            Assert.AreEqual("vsToolWindow Unit Test Sessions", actual.OpenWindows[1].Identifier);
            Assert.AreEqual("vsEditor f2l2zVnq9lX05A_CO1ntrQ==", actual.OpenWindows[2].Identifier);
        }

        protected override void AssertThatPropertiesThatAreNotTouchedByAnonymizationAreUnchanged(IDEStateEvent original,
            IDEStateEvent anonymized)
        {
            Assert.AreEqual(original.IDELifecyclePhase, anonymized.IDELifecyclePhase);
        }
    }
}
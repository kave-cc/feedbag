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
using KaVE.Commons.Model.Events;
using KaVE.VS.FeedbackGenerator.Generators;
using NUnit.Framework;

namespace KaVE.VS.FeedbackGenerator.Tests.Generators
{
    internal class KaVECommandGeneratorTest : EventGeneratorTestBase
    {
        private DateTimeOffset _now;
        private KaVECommandGenerator _sut;

        [SetUp]
        public void SetUp()
        {
            TestDateUtils.Now = _now = DateTimeOffset.Now;
            _sut = new KaVECommandGenerator(TestRSEnv, TestMessageBus, TestDateUtils, TestThreading);
        }

        [Test]
        public void FireDeleteDays()
        {
            _sut.FireDeleteDays();
            AssertEvents(Cmd("KaVE.FeedBaG.DeleteDays"));
        }

        [Test]
        public void FireDeleteEvents()
        {
            _sut.FireDeleteEvents();
            AssertEvents(Cmd("KaVE.FeedBaG.DeleteEvents"));
        }

        [Test]
        public void FireExportToServer()
        {
            _sut.FireExportToServer();
            AssertEvents(Cmd("KaVE.FeedBaG.ExportToServer"));
        }

        [Test]
        public void FireExportIntoZip()
        {
            _sut.FireExportIntoZip();
            AssertEvents(Cmd("KaVE.FeedBaG.ExportIntoZip"));
        }

        [Test]
        public void FireGotoHomepage()
        {
            _sut.FireGotoHomepage();
            AssertEvents(Cmd("KaVE.FeedBaG.GotoHomepage"));
        }

        [Test]
        public void FireGotoUploadPage()
        {
            _sut.FireGotoUploadPage();
            AssertEvents(Cmd("KaVE.FeedBaG.GotoUploadPage"));
        }

        [Test]
        public void FireOpenAboutDialog()
        {
            _sut.FireOpenAboutDialog();
            AssertEvents(Cmd("KaVE.FeedBaG.OpenAboutDialog"));
        }

        [Test]
        public void FireOpenExportDialog()
        {
            _sut.FireOpenExportDialog();
            AssertEvents(Cmd("KaVE.FeedBaG.OpenExportDialog"));
        }

        [Test]
        public void FireOpenOptions()
        {
            _sut.FireOpenOptions();
            AssertEvents(Cmd("KaVE.FeedBaG.OpenOptions"));
        }

        [Test]
        public void FireReloadEvents()
        {
            _sut.FireReloadEvents();
            AssertEvents(Cmd("KaVE.FeedBaG.ReloadEvents"));
        }

        [Test]
        public void Integration()
        {
            Fire(1, () => _sut.FireDeleteEvents());
            Fire(3, () => _sut.FireOpenExportDialog());
            Fire(24, () => _sut.FireExportToServer());

            AssertEvents(
                Cmd("KaVE.FeedBaG.DeleteEvents", 1),
                Cmd("KaVE.FeedBaG.OpenExportDialog", 3),
                Cmd("KaVE.FeedBaG.ExportToServer", 24));
        }

        private CommandEvent Cmd(string cmdId, int sec = 0)
        {
            return new CommandEvent
            {
                CommandId = cmdId,
                IDESessionUUID = TestIDESession.UUID,
                KaVEVersion = TestRSEnv.KaVEVersion,
                TriggeredAt = _now.AddSeconds(sec),
                TriggeredBy = EventTrigger.Unknown
            };
        }

        private void Fire(int afterSeconds, Action a)
        {
            TestDateUtils.Now = _now.AddSeconds(afterSeconds);
            a();
        }
    }
}
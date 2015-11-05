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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KaVE.Commons.Model.Events;
using KaVE.Commons.Model.Events.UserProfiles;
using KaVE.Commons.Model.Names.VisualStudio;
using KaVE.Commons.Utils.Collections;
using KaVE.Commons.Utils.IO.Archives;
using KaVE.RS.SolutionAnalysis.SortByUser;
using Moq;
using NUnit.Framework;

namespace KaVE.RS.SolutionAnalysis.Tests.SortByUser
{
    internal class SortByUserIoTest
    {
        #region setup and helpers

        private SortByUserIo _sut;
        private string _dirIn;
        private string _dirOut;
        private ISortByUserLogger _log;

        private List<string> _actualFilesRead;
        private List<string> _actualFilesWritten;

        [SetUp]
        public void Setup()
        {
            _log = Mock.Of<ISortByUserLogger>();
            _dirIn = CreateTempDir();
            _dirOut = CreateTempDir();
            _sut = new SortByUserIo(_dirIn, _dirOut, _log);

            _actualFilesRead = new List<string>();
            _actualFilesWritten = new List<string>();

            Mock.Get(_log)
                .Setup(l => l.ReadingArchive(It.IsAny<string>()))
                .Callback<string>(f => _actualFilesRead.Add(f));

            Mock.Get(_log)
                .Setup(l => l.WritingArchive(It.IsAny<string>()))
                .Callback<string>(f => _actualFilesWritten.Add(f));
        }

        [TearDown]
        public void Teardown()
        {
            if (Directory.Exists(_dirIn))
            {
                Directory.Delete(_dirIn, true);
            }
            if (Directory.Exists(_dirOut))
            {
                Directory.Delete(_dirOut, true);
            }
        }

        private static string CreateTempDir()
        {
            var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(path);
            return path;
        }

        private static IDEEvent Profile(int pid)
        {
            return new UserProfileEvent
            {
                // this is invalid but Ok for this test!
                ActiveDocument = DocumentName.Get(Guid.NewGuid().ToString()),
                ProfileId = string.Format("{0}", pid)
            };
        }

        private static IDEEvent Event(int sid)
        {
            return new ActivityEvent
            {
                // this is invalid but Ok for this test!
                ActiveDocument = DocumentName.Get(Guid.NewGuid().ToString()),
                IDESessionUUID = string.Format("{0}", sid)
            };
        }

        private static IDEEvent Event()
        {
            return new ActivityEvent
            {
                // this is invalid but Ok for this test!
                ActiveDocument = DocumentName.Get(Guid.NewGuid().ToString())
            };
        }

        private void AddFile(string fileName, params IDEEvent[] events)
        {
            var fullName = Path.Combine(_dirIn, fileName);
            var dir = Path.GetDirectoryName(fullName);
            if (dir != null)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }

            using (var wa = new WritingArchive(fullName))
            {
                foreach (var e in events)
                {
                    wa.Add(e);
                }
            }
        }

        private IDictionary<string, IKaVESet<string>> AssertArchivesFound(int expectedCount)
        {
            var actuals = _sut.ScanArchivesForIdentifiers();

            Assert.AreEqual(expectedCount, actuals.Keys.Count);
            return actuals;
        }

        private static void AssertIdentifiers(IDictionary<string, IKaVESet<string>> actuals,
            string fileName,
            params string[] ids)
        {
            Assert.True(actuals.ContainsKey(fileName));
            var actual = actuals[fileName];
            var expected = Sets.NewHashSetFrom(ids);
            Assert.AreEqual(expected, actual);
        }

        private void AssertEvents(string fileName, params IDEEvent[] events)
        {
            var fullName = Path.Combine(_dirOut, fileName);
            var ra = new ReadingArchive(fullName);
            var actuals = ra.GetAll<IDEEvent>();
            var expecteds = events.ToList();
            CollectionAssert.AreEqual(expecteds, actuals);
        }

        #endregion

        [Test]
        public void FindsArchivesAndReadsEvents_HappyPath()
        {
            AddFile(@"1.zip", Profile(1), Event(1));

            var actuals = AssertArchivesFound(1);

            AssertIdentifiers(actuals, @"1.zip", "pid:" + 1, "sid:" + 1);
        }

        [Test]
        public void FindsArchivesAndReadsEvents_AlsoWorksWithTrailingSlashInPathes()
        {
            _sut = new SortByUserIo(_dirIn + @"\", _dirOut + @"\", _log);
            FindsArchivesAndReadsEvents_HappyPath();
        }

        [Test]
        public void FindsArchivesAndReadsEvents_NoProfile()
        {
            AddFile(@"2.zip", Event(2));

            var actuals = AssertArchivesFound(1);

            AssertIdentifiers(actuals, @"2.zip", "sid:" + 2);
        }

        [Test]
        public void FindsArchivesAndReadsEvents_EmptyProfileId()
        {
            AddFile(@"2b.zip", new UserProfileEvent(), Event(2));

            var actuals = AssertArchivesFound(1);

            AssertIdentifiers(actuals, @"2b.zip", "sid:" + 2);
        }

        [Test]
        public void FindsArchivesAndReadsEvents_NoSessionIds()
        {
            AddFile(@"3.zip", Profile(3), Event());

            var actuals = AssertArchivesFound(1);

            AssertIdentifiers(actuals, @"3.zip", "pid:" + 3);
        }

        [Test]
        public void FindsArchivesAndReadsEvents_EmptySessionId()
        {
            AddFile(@"3b.zip", Profile(3), new ActivityEvent {IDESessionUUID = ""});

            var actuals = AssertArchivesFound(1);

            AssertIdentifiers(actuals, @"3b.zip", "pid:" + 3);
        }

        [Test]
        public void FindsArchivesAndReadsEvents_Subfolder()
        {
            AddFile(@"sub\4.zip", Profile(4), Event(5));

            var actuals = AssertArchivesFound(1);

            AssertIdentifiers(actuals, @"sub\4.zip", "pid:" + 4, "sid:" + 5);
        }

        [Test]
        public void Merging()
        {
            var e1 = Event(1);
            var e2 = Event(2);
            var e3 = Event(3);

            AddFile(@"sub\1.zip", e1);
            AddFile(@"2.zip", e2);
            AddFile(@"3.zip", e3);

            _sut.MergeArchives(Sets.NewHashSet(@"sub\1.zip", "2.zip"));
            _sut.MergeArchives(Sets.NewHashSet("3.zip"));

            AssertEvents(@"sub\1.zip", e1, e2);
            AssertEvents(@"3.zip", e3);
        }

        [Test]
        public void MergingDoesNotRemoveDuplicatesToMakeNumbersTraceable()
        {
            var e1 = Event(1);

            AddFile("1.zip", e1);
            AddFile("2.zip", e1);

            _sut.MergeArchives(Sets.NewHashSet("1.zip", "2.zip"));

            AssertEvents("1.zip", e1, e1);
        }

        [Test]
        public void Merging_NoFileSupplied()
        {
            _sut.MergeArchives(Sets.NewHashSet<string>());
        }

        [Test]
        public void LoggerIsCalled_ScanArchives()
        {
            AddFile(@"sub\1.zip", Event(1), Event(2), Event(3));
            AddFile(@"2.zip", Event(4), Event(5));
            AddFile(@"3.zip", Event(6));

            _sut.ScanArchivesForIdentifiers();

            Mock.Get(_log).Verify(l => l.WorkingIn(_dirIn + @"\", _dirOut + @"\"));
            Mock.Get(_log).Verify(l => l.FoundNumArchives(3));
            AssertReadFiles(@"sub\1.zip", @"2.zip", @"3.zip");

            var numEvents = Times.Exactly(6);
            Mock.Get(_log).Verify(l => l.Progress(), numEvents);
            Mock.Get(_log).Verify(l => l.CountInputEvent(), numEvents);
        }

        [Test]
        public void LoggerIsCalled_Merge()
        {
            AddFile(@"sub\1.zip", Event(1), Event(2), Event(3));
            AddFile(@"2.zip", Event(4), Event(5));

            _sut.MergeArchives(Sets.NewHashSet(@"sub\1.zip", "2.zip"));


            Mock.Get(_log).Verify(l => l.WorkingIn(_dirIn + @"\", _dirOut + @"\"));
            Mock.Get(_log).Verify(l => l.Merging(Sets.NewHashSet(@"sub\1.zip", "2.zip")));
            AssertReadFiles(@"sub\1.zip", @"2.zip");
            Mock.Get(_log).Verify(l => l.Progress(), Times.Exactly(5));
            Mock.Get(_log).Verify(l => l.StoreOutputEvents(5));
            AssertWrittenFiles(@"sub\1.zip");
        }

        private void AssertReadFiles(params string[] expectedFiles)
        {
            Assert.AreEqual(expectedFiles.Length, _actualFilesRead.Count);
            Mock.Get(_log).Verify(l => l.ReadingArchive(It.IsAny<string>()), Times.Exactly(expectedFiles.Length));

            foreach (var expectedFile in expectedFiles)
            {
                var fullExpectedFile = Path.Combine(_dirIn, expectedFile);
                Assert.That(_actualFilesRead.Contains(fullExpectedFile));
            }
        }

        private void AssertWrittenFiles(params string[] expectedFiles)
        {
            Assert.AreEqual(expectedFiles.Length, _actualFilesWritten.Count);
            Mock.Get(_log).Verify(l => l.WritingArchive(It.IsAny<string>()), Times.Exactly(expectedFiles.Length));

            foreach (var expectedFile in expectedFiles)
            {
                var fullExpectedFile = Path.Combine(_dirOut, expectedFile);
                Assert.That(_actualFilesWritten.Contains(fullExpectedFile));
            }
        }
    }
}
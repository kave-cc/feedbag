﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using KaVE.VsFeedbackGenerator.Utils;
using KaVE.VsFeedbackGenerator.Utils.Json;
using NUnit.Framework;

namespace KaVE.VsFeedbackGenerator.Tests.Utils
{
    [TestFixture]
    internal class SessionExportTest
    {
        [Test]
        public void TemporaryExportContainsGivenList()
        {
            var uut = new TempExport<string>();
            var expected = RandomlyFilledList(25);
            var tempLocation = uut.ExportToTemporaryFile(expected, FileManager.NewLogWriter);
            AssertFileIsComplete(expected, tempLocation);
        }

        [Test]
        public void FileExportContainsGivenList()
        {
            var targetLocation = Path.GetTempFileName();
            var uut = new FileExport<string>(() => targetLocation);
            var expected = RandomlyFilledList(25);
            var response = uut.Export(expected, FileManager.NewLogWriter);
            Assert.AreEqual(State.Ok, response.Status);
            AssertFileIsComplete(expected, targetLocation);
        }

        [Test]
        public void FileExportInterruptsWithInvalidFileName()
        {
            var uut = new FileExport<string>(() => null);
            var list = RandomlyFilledList(25);
            var response = uut.Export(list, FileManager.NewLogWriter);
            Assert.AreEqual(State.Fail, response.Status);
        }

        private static void AssertFileIsComplete(IList<string> expected, string location)
        {
            var actual = FileManager.NewLogReader(location).GetEnumeration<string>();
            Assert.AreEqual(expected, actual);
        }

        private static JsonLogFileManager FileManager
        {
            get { return new JsonLogFileManager(); } // TODO: replace with Interface
        }

        private static IList<string> RandomlyFilledList(int count)
        {
            var list = new List<string>();
            for (var i = 0; i < count; i++)
            {
                list.Add(RandomString());
            }
            return list;
        }

        private static string RandomString()
        {
            return new Random().Next().ToString(CultureInfo.InvariantCulture);
        }
    }
}
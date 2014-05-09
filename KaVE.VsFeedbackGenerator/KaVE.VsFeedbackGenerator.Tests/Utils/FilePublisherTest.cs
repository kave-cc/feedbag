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
using System.IO;
using KaVE.Utils.Assertion;
using KaVE.VsFeedbackGenerator.Utils;
using Moq;
using NUnit.Framework;

namespace KaVE.VsFeedbackGenerator.Tests.Utils
{
    [TestFixture]
    internal class FilePublisherTest
    {
        private Mock<IIoUtils> _ioMock;
        private const string SrcLoc = "existing source file";
        private const string TrgLoc = "existing target file";

        [SetUp]
        public void SetUp()
        {
            _ioMock = new Mock<IIoUtils>();
            _ioMock.Setup(io => io.FileExists(SrcLoc)).Returns(true);
            Registry.RegisterComponent(_ioMock.Object);
        }

        [Test]
        public void ShouldInvokeCopy()
        {
            var uut = new FilePublisher(() => TrgLoc);
            uut.Publish(SrcLoc);

            _ioMock.Verify(m => m.CopyFile(SrcLoc, TrgLoc));
        }

        [Test, ExpectedException(typeof (AssertException))]
        public void ShouldThrowExceptionOnNonexistingSourceFile()
        {
            var uut = new FilePublisher(() => TrgLoc);
            uut.Publish("some illegal location");
        }

        [Test, ExpectedException(typeof(AssertException))]
        public void ShouldThrowExceptionOnNullArgument()
        {
            var uut = new FilePublisher(() => null);
            uut.Publish(SrcLoc);
        }

        [Test, ExpectedException(typeof (AssertException))]
        public void ShouldThrowExceptionOnEmptyArgument()
        {
            var uut = new FilePublisher(() => "");
            uut.Publish(SrcLoc);
        }

        private const string CopyFailureMessage = "Datei-Zugriff verweigert";

        [Test,
         ExpectedException(typeof (AssertException),
             ExpectedMessage = "Datei-Export fehlgeschlagen: " + CopyFailureMessage)]
        public void ShouldThrowExceptionIfCopyFails()
        {
            _ioMock.Setup(io => io.CopyFile(SrcLoc, TrgLoc)).Throws(new Exception(CopyFailureMessage));
            var uut = new FilePublisher(() => TrgLoc);
            uut.Publish(SrcLoc);
        }

        [TearDown]
        public void CleanUpRegistry()
        {
            Registry.Clear();
        }
    }
}
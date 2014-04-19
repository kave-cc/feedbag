﻿using System;
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

        // TODO jetzt überflüssig, da die Registry genutzt wird?
        [Test]
        public void ShouldPublishFile()
        {
            const string content = "some text to test the file-copy";

            var sourceLocation = Path.GetTempFileName();
            var targetLocation = Path.GetTempFileName();
            WriteSourceFile(content, sourceLocation);
            _ioMock.Setup(io => io.FileExists(It.IsAny<string>())).Returns<string>(new IoUtils().FileExists);
            _ioMock.Setup(m => m.CopyFile(It.IsAny<string>(), It.IsAny<string>()))
                   .Callback<string, string>(new IoUtils().CopyFile);

            var uut = new FilePublisher(() => targetLocation);
            uut.Publish(sourceLocation);

            var actual = ReadPublishedFile(targetLocation);

            Assert.AreEqual(content, actual);
        }

        private static void WriteSourceFile(string content, string location)
        {
            using (var writer = new StreamWriter(new FileStream(location, FileMode.Create, FileAccess.Write)))
            {
                writer.Write(content);
            }
        }

        private static string ReadPublishedFile(string location)
        {
            using (var reader = new StreamReader(new FileStream(location, FileMode.Open)))
            {
                return reader.ReadToEnd();
            }
        }

        [Test]
        public void ShouldInvokeCopy()
        {
            var uut = new FilePublisher(() => TrgLoc);
            uut.Publish(SrcLoc);

            _ioMock.Verify(m => m.CopyFile(SrcLoc, TrgLoc));
        }

        [Test, ExpectedException(typeof (AssertException), ExpectedMessage = "Quelldatei existiert nicht")]
        public void ShouldThrowExceptionOnNonexistingSourceFile()
        {
            var uut = new FilePublisher(() => TrgLoc);
            uut.Publish("some illegal location");
        }

        [Test, ExpectedException(typeof (AssertException), ExpectedMessage = "Kein Ziel angegeben")]
        public void ShouldThrowExceptionOnNullArgument()
        {
            var uut = new FilePublisher(() => null);
            uut.Publish(SrcLoc);
        }

        [Test, ExpectedException(typeof (AssertException), ExpectedMessage = "Invalides Ziel angegeben")]
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
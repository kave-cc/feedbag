/*
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

using KaVE.Commons.Model;
using NUnit.Framework;

namespace KaVE.RS.Commons.Tests_Unit.FeedBaGVersionUtilTestSuite
{
    internal class ReleaseTest
    {
        private FeedBaGVersionUtil _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new FeedBaGVersionUtil();
        }

        [Test]
        public void GetInformalVersion()
        {
            var actual = _sut.GetInformalVersion();
            Assert.That(actual.StartsWith("0."));
            Assert.That(actual.EndsWith("-Release"));
        }

        [Test]
        public void GetVariant()
        {
            var actual = _sut.GetVariant();
            Assert.AreEqual(Variant.Release, actual);
        }
    }
}
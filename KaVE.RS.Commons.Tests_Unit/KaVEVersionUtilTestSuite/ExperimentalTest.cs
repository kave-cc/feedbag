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

namespace KaVE.RS.Commons.Tests_Unit.KaVEVersionUtilTestSuite
{
    internal class ExperimentalTest
    {
        [Test]
        public void CurrentVersion()
        {
            var actual = new KaVEVersionUtil().GetCurrentVersion().ToString();
            Assert.AreNotEqual("0.0.0.0", actual);
            Assert.True(actual.StartsWith("0."));
            Assert.True(actual.EndsWith(".0.0"));
        }

        [Test]
        public void CurrentInformalVersion()
        {
            var actual = new KaVEVersionUtil().GetCurrentInformalVersion();
            Assert.AreNotEqual("0.0.0-Development", actual);
            Assert.True(actual.StartsWith("0."));
            Assert.True(actual.EndsWith("-Experimental"));
        }

        [Test]
        public void CurrentVariant()
        {
            var actual = new KaVEVersionUtil().GetCurrentVariant();
            var expected = Variant.Experimental;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AlsoWorksForOverwrittenClasses()
        {
            var actual = new ExtendedVersionUtilInAssemblyWithoutVersion().GetCurrentInformalVersion();
            Assert.AreNotEqual("0.0-Development", actual);
            Assert.True(actual.StartsWith("0."));
            Assert.True(actual.EndsWith("-Experimental"));
        }

        private class ExtendedVersionUtilInAssemblyWithoutVersion : KaVEVersionUtil { }
    }
}
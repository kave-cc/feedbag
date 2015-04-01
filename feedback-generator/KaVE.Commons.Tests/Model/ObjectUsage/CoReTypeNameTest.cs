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
 *    - Dennis Albrecht
 */

using KaVE.Commons.Model.ObjectUsage;
using KaVE.Commons.Utils.Assertion;
using NUnit.Framework;

namespace KaVE.Commons.Tests.Model.ObjectUsage
{
    internal class CoReTypeNameTest
    {
        [Test]
        public void ShouldRecognizeEqualTypeNames()
        {
            Assert.AreEqual(
                new CoReTypeName("LClass"),
                new CoReTypeName("LClass"));
        }

        [TestCase("System.Int32, mscore, 4.0.0.0"), TestCase("KaVE.Model.ObjectUsage.Query"), TestCase("Type"),
         TestCase("LType;"), TestCase("L"), TestCase("LN.T"),
         ExpectedException(typeof (AssertException))]
        public void ShouldRejectInvalidTypeNames(string typeName)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new CoReTypeName(typeName);
        }

        [TestCase("LType"), TestCase("LKaVE/Model/ObjectUsage/Query"), TestCase("LT1"), TestCase("LT$"),
         TestCase("LN1/T"), TestCase("[LType"), TestCase("[[[LType")]
        public void ShouldAcceptValidTypeNames(string typeName)
        {
            // ReSharper disable once ObjectCreationAsStatement
            new CoReTypeName(typeName);
        }
    }
}
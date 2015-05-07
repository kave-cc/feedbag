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

using KaVE.Commons.Model.Names.CSharp;
using NUnit.Framework;

namespace KaVE.Commons.Tests.Model.Names.CSharp
{
    [TestFixture]
    class NameUtilsTest
    {
        [Test]
        public void HasParameters()
        {
            Assert.IsTrue("M([C,P] p)".HasParameters());
            Assert.IsTrue("M([[DR, P] [D, P].()] p)".HasParameters());
        }

        [Test]
        public void HasNoParameters()
        {
            Assert.IsFalse("M()".HasParameters());
        }

        [Test]
        public void ParsesParametersWithParameterizedType()
        {
            var parameterNames = "M([A`1[[B, P]], P] p)".GetParameterNames();
            Assert.AreEqual(ParameterName.Get("[A`1[[B, P]], P] p"), parameterNames[0]);
        }

        [Test]
        public void ParsesParametersWithDelegateType()
        {
            var parameterNames = "M([[DR, P] [D, P].()] p)".GetParameterNames();
            Assert.AreEqual(ParameterName.Get("[[DR, P] [D, P].()] p"), parameterNames[0]);
        }
    }
}

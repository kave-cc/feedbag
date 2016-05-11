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
using System.Linq;
using System.Text;
using KaVE.Commons.Model.Names.CSharp.Parser;
using KaVE.Commons.Utils.Collections;
using NUnit.Framework;

namespace KaVE.Commons.Tests.Model.Names.CSharp.Parser
{
    [TestFixture]
    class TypeNameTestSuite
    {
        private IKaVEList<TypeNameTestCase> testcases;
        private IKaVEList<string> invalidTypes;

        [SetUp]
        public void Init()
        {
            testcases = TestCaseProvider.ValidTypeNames();
            invalidTypes = TestCaseProvider.InvalidTypeNames();
        }

        [TestCaseSource("testcases")]
        public void ValidTypeName(TypeNameTestCase typeNameTestCase)
        {
            Assert.DoesNotThrow(delegate { new CsTypeName(typeNameTestCase.GetIdentifier()); });
            var type = new CsTypeName(typeNameTestCase.GetIdentifier());
            Assert.AreEqual(typeNameTestCase.GetIdentifier(), type.Identifier);
            Assert.AreEqual(typeNameTestCase.GetAssembly(), type.Assembly.Identifier);
        }

        [TestCaseSource("invalidTypes")]
        public void InvalidTypeName(string invalidType)
        {
            Assert.Catch(delegate { new CsTypeName(invalidType); });
        }
    }
}
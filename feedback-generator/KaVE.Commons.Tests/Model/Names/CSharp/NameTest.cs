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

using KaVE.Commons.Model.Names.CSharp;
using NUnit.Framework;

namespace KaVE.Commons.Tests.Model.Names.CSharp
{
    internal class NameTest
    {
        [Test]
        public void ShouldImplementIsUnknown()
        {
            Assert.That(Name.UnknownName.IsUnknown);
        }

        [Test]
        public void ShouldImplementIsHashed()
        {
            Assert.IsFalse(TypeName.Get("System.Int32, mscore, 4.0.0.0").IsHashed);
            Assert.IsTrue(TypeName.Get("pNX6Dym0JEkqjiMC-A8XnQ==.J5Ork5yYgIPYJrg19qjg5A==, 72launbJW34oSO9wR5XBdw==").IsHashed);
        }
    }
}
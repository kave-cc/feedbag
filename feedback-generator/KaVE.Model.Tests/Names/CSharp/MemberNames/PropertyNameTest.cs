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

using KaVE.Model.Names.CSharp;
using NUnit.Framework;

namespace KaVE.Model.Tests.Names.CSharp.MemberNames
{
    internal class PropertyNameTest
    {
        [Test]
        public void ShouldImplementIsUnknown()
        {
            Assert.That(PropertyName.UnknownName.IsUnknown);
        }

        [Test]
        public void ShouldBeSimpleProperty()
        {
            const string valueTypeIdentifier = "A, B, 1.0.0.0";
            const string declaringTypeIdentifier = "C, D, 0.9.8.7";
            var propertyName =
                PropertyName.Get("[" + valueTypeIdentifier + "] [" + declaringTypeIdentifier + "].Property");

            Assert.AreEqual("Property", propertyName.Name);
            Assert.AreEqual(valueTypeIdentifier, propertyName.ValueType.Identifier);
            Assert.AreEqual(declaringTypeIdentifier, propertyName.DeclaringType.Identifier);
            Assert.IsFalse(propertyName.IsStatic);
        }

        [Test]
        public void ShoudBePropertyWithGetter()
        {
            var propertyName = PropertyName.Get("get [Z, Y, 0.5.6.1] [X, W, 0.3.4.2].Prop");

            Assert.IsTrue(propertyName.HasGetter);
        }

        [Test]
        public void ShoudBePropertyWithSetter()
        {
            var propertyName = PropertyName.Get("set [Z, Y, 0.5.6.1] [X, W, 0.3.4.2].Prop");

            Assert.IsTrue(propertyName.HasSetter);
        }

        [Test]
        public void ShouldBeStaticProperty()
        {
            var propertyName = PropertyName.Get("static [A, B, 1.2.3.4] [C, D, 5.6.7.8].E");

            Assert.IsTrue(propertyName.IsStatic);
        }
    }
}
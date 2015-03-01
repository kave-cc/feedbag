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
 *    - Sebastian Proksch
 */

using KaVE.Model.Names.CSharp;
using KaVE.Model.SSTs.Impl.References;
using KaVE.Model.SSTs.Impl.Visitor;
using KaVE.Model.SSTs.References;
using NUnit.Framework;

namespace KaVE.Model.Tests.SSTs.Impl.References
{
    public class PropertyReferenceTest
    {
        private PropertyVisitor _visitor;

        [SetUp]
        public void SetUp()
        {
            _visitor = new PropertyVisitor();
        }

        [Test]
        public void DefaultValues()
        {
            var sut = new PropertyReference();
            Assert.Null(sut.PropertyName);
            Assert.AreNotEqual(0, sut.GetHashCode());
            Assert.AreNotEqual(1, sut.GetHashCode());
        }

        [Test]
        public void SettingValues()
        {
            var sut = new PropertyReference {PropertyName = PropertyName.UnknownName};
            Assert.AreEqual(PropertyName.UnknownName, sut.PropertyName);
        }

        [Test]
        public void Equality_Default()
        {
            var a = new PropertyReference();
            var b = new PropertyReference();
            Assert.AreEqual(a, b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Equality_ReallyTheSame()
        {
            var a = new PropertyReference {PropertyName = PropertyName.UnknownName};
            var b = new PropertyReference {PropertyName = PropertyName.UnknownName};
            Assert.AreEqual(a, b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Equality_DifferentName()
        {
            var a = new PropertyReference {PropertyName = PropertyName.UnknownName};
            var b = new PropertyReference();
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void VisitorIsImplemented()
        {
            var sut = new PropertyReference();
            Assert.Null(_visitor.Argument);
            sut.Accept(_visitor, 0);
            Assert.AreEqual(sut, _visitor.Argument);
        }


        internal class PropertyVisitor : AbstractNodeVisitor<int>
        {
            public IPropertyReference Argument { get; set; }

            public override void Visit(IPropertyReference eventRef, int context)
            {
                Argument = eventRef;
            }
        }
    }
}
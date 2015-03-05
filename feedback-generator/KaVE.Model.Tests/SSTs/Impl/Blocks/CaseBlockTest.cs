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

using KaVE.Model.Collections;
using KaVE.Model.SSTs.Blocks;
using KaVE.Model.SSTs.Impl.Blocks;
using KaVE.Model.SSTs.Impl.Statements;
using KaVE.Model.SSTs.Impl.Visitor;
using NUnit.Framework;

namespace KaVE.Model.Tests.SSTs.Impl.Blocks
{
    internal class CaseBlockTest
    {
        [Test]
        public void DefaultValues()
        {
            var sut = new CaseBlock();
            Assert.Null(sut.Label);
            Assert.NotNull(sut.Body);
            Assert.AreEqual(0, sut.Body.Count);
            Assert.AreNotEqual(0, sut.GetHashCode());
            Assert.AreNotEqual(1, sut.GetHashCode());
        }

        [Test]
        public void SettingValues()
        {
            var sut = new CaseBlock {Label = "a"};
            sut.Body.Add(new ReturnStatement());

            Assert.AreEqual("a", sut.Label);
            Assert.AreEqual(Lists.NewList(new ReturnStatement()), sut.Body);
        }

        [Test]
        public void Equality_Default()
        {
            var a = new CaseBlock();
            var b = new CaseBlock();
            Assert.AreEqual(a, b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Equality_ReallyTheSame()
        {
            var a = new CaseBlock {Label = "a"};
            a.Body.Add(new ReturnStatement());
            var b = new CaseBlock {Label = "a"};
            b.Body.Add(new ReturnStatement());
            Assert.AreEqual(a, b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Equality_DifferentLabel()
        {
            var a = new CaseBlock {Label = "a"};
            var b = new CaseBlock();
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Equality_DifferentBody()
        {
            var a = new CaseBlock();
            a.Body.Add(new ReturnStatement());
            var b = new CaseBlock();
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void VisitorIsImplemented()
        {
            var sut = new CaseBlock();
            var visitor = new TestVisitor();
            sut.Accept(visitor, 1);

            Assert.AreEqual(sut, visitor.Statement);
            Assert.AreEqual(1, visitor.Context);
        }

        internal class TestVisitor : AbstractNodeVisitor<int>
        {
            public ICaseBlock Statement { get; private set; }
            public int Context { get; private set; }

            public override void Visit(ICaseBlock stmt, int context)
            {
                Statement = stmt;
                Context = context;
            }
        }
    }
}
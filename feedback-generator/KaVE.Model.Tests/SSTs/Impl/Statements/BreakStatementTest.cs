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

using KaVE.Model.SSTs.Impl.Statements;
using KaVE.Model.SSTs.Impl.Visitor;
using KaVE.Model.SSTs.Statements;
using NUnit.Framework;

namespace KaVE.Model.Tests.SSTs.Impl.Statements
{
    public class BreakStatementTest
    {
        [Test]
        public void Equality()
        {
            var a = new BreakStatement();
            var b = new BreakStatement();
            Assert.AreEqual(a, b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
            Assert.AreNotEqual(0, a.GetHashCode());
            Assert.AreNotEqual(1, a.GetHashCode());
        }
        [Test]
        public void VisitorIsImplemented()
        {
            var sut = new BreakStatement();
            var visitor = new TestVisitor();
            sut.Accept(visitor, 1);

            Assert.AreEqual(sut, visitor.Statement);
            Assert.AreEqual(1, visitor.Context);
        }

        internal class TestVisitor : AbstractNodeVisitor<int>
        {
            public IBreakStatement Statement { get; private set; }
            public int Context { get; private set; }

            public override void Visit(IBreakStatement stmt, int context)
            {
                Statement = stmt;
                Context = context;
            }
        }
    }
}
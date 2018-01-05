/*
 * Copyright 2017 University of Zurich
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
using JetBrains.ReSharper.Psi.CSharp.Tree;
using KaVE.Commons.Model.SSTs.Impl.Expressions.Assignable;
using KaVE.RS.Commons.Analysis.CompletionTarget;
using NUnit.Framework;
using Fix = KaVE.RS.Commons.Tests_Integration.Analysis.SSTAnalysisTestSuite.SSTAnalysisFixture;

namespace KaVE.RS.Commons.Tests_Integration.Analysis.CompletionTargetTestSuite.Mixed
{
    internal class EmptyAndAssignments : BaseTest
    {
        private object[] _cases =
        {
            // empty
            new object[] {"break;\n $", typeof(IBreakStatement), CompletionCase.EmptyCompletionAfter},
            new object[] {"break; $", typeof(IBreakStatement), CompletionCase.EmptyCompletionAfter},
            new object[] {"break;$", typeof(IBreakStatement), CompletionCase.EmptyCompletionAfter},

            // var decl (left)
            new object[] {"va$r i = 1;", typeof(ILocalVariableDeclaration), CompletionCase.InSignature},
            new object[] {"var$ i = 1;", typeof(ILocalVariableDeclaration), CompletionCase.InSignature},
            new object[] {"var $ i = 1;", typeof(ILocalVariableDeclaration), CompletionCase.InSignature},
            new object[] {"var $i = 1;", typeof(ILocalVariableDeclaration), CompletionCase.InSignature},
            new object[] {"var i$jk = 1;", typeof(ILocalVariableDeclaration), CompletionCase.InSignature},
            new object[] {"var i$ = 1;", typeof(ILocalVariableDeclaration), CompletionCase.InSignature},
            new object[] {"var i $ = 1;", typeof(ILocalVariableDeclaration), CompletionCase.InSignature},
            new object[] {"var i $= 1;", typeof(ILocalVariableDeclaration), CompletionCase.InSignature},
            new object[] {"var $ = 1;", typeof(IAssignmentExpression), CompletionCase.InSignature},
            new object[] {"var i = $ = 1;", typeof(ILocalVariableDeclaration), CompletionCase.InSignature},

            // assignment (left)
            new object[] {"int i; $ i = 1;", typeof(ILocalVariableDeclaration), CompletionCase.EmptyCompletionAfter},
            new object[] {"int i; $i = 1;", typeof(ILocalVariableDeclaration), CompletionCase.EmptyCompletionAfter},
            new object[] {"int i; i$jk = 1;", typeof(IAssignmentExpression), CompletionCase.InSignature},
            new object[] {"int i; ijk$ = 1;", typeof(IAssignmentExpression), CompletionCase.InSignature},
            new object[] {"int i; ijk $ = 1;", typeof(IAssignmentExpression), CompletionCase.InSignature},
            new object[] {"int i; ijk $= 1;", typeof(IAssignmentExpression), CompletionCase.InSignature},

            // var decl (right)
            new object[] {"var o =$", typeof(ILocalVariableDeclaration), CompletionCase.InBody},
            new object[] {"var o = $", typeof(ILocalVariableDeclaration), CompletionCase.InBody},
            new object[] {"var o =\n $", typeof(ILocalVariableDeclaration), CompletionCase.InBody},
            new object[] {"int i, j =$", typeof(ILocalVariableDeclaration), CompletionCase.InBody},
            new object[] {"int i, j = $", typeof(ILocalVariableDeclaration), CompletionCase.InBody},

            // assignment (right)
            new object[] {"int i; i =$", typeof(IAssignmentExpression), CompletionCase.InBody},
            new object[] {"int i; i = $", typeof(IAssignmentExpression), CompletionCase.InBody},
            new object[] {"int i; i =\n $", typeof(IAssignmentExpression), CompletionCase.InBody},
            new object[] {"int i,j; i = j =$", typeof(IAssignmentExpression), CompletionCase.InBody},
            new object[] {"int i,j; i = j = $", typeof(IAssignmentExpression), CompletionCase.InBody},

            // multi assignment
            new object[] {"int i, j; i$ = j = 1;", typeof(IAssignmentExpression), CompletionCase.InSignature},
            new object[] {"int i, j; i = j$ = 1;", typeof(IAssignmentExpression), CompletionCase.InSignature},
            new object[] {"int i, j; i = j = $", typeof(IAssignmentExpression), CompletionCase.InBody},
            new object[] {"int i,j; i = $ = 1;", typeof(IAssignmentExpression), CompletionCase.InSignature},

            // assignment of ref
            new object[] {"var o = thi$", typeof(IReferenceExpression), CompletionCase.Undefined},
            new object[] {"var o = this.$", typeof(IReferenceExpression), CompletionCase.Undefined},
            new object[] {"var o = this.G$", typeof(IReferenceExpression), CompletionCase.Undefined},

            // ??
            new object[] {"$\nobject o;", typeof(ILocalVariableDeclaration), CompletionCase.EmptyCompletionBefore},
            new object[] {"object o;\n$", typeof(ILocalVariableDeclaration), CompletionCase.EmptyCompletionAfter}
        };

        [TestCaseSource(nameof(_cases))]
        public void Test(string body, Type expectedHandler, CompletionCase expectedCase)
        {
            Exec(body);
            AssertCompletionMarker(expectedHandler, expectedCase);
        }

        [TestCaseSource(nameof(_cases))]
        public void Test_WithStatement(string body, Type expectedHandler, CompletionCase expectedCase)
        {
            Exec(body + "\ncontinue;");
            AssertCompletionMarker(expectedHandler, expectedCase);
        }

        private void Exec(string body)
        {
            Console.WriteLine("--- body: ---\n\n{0}\n\n", body);
            CompleteInMethod(body);
            Console.WriteLine("--- resulting SST: ---\n\n{0}\n\n", ResultContext.SST);
        }

        [Test, Ignore]
        public void MultiAssignment()
        {
            CompleteInMethod(
                @"
                int i, j;
                i = j = 1;
                $
            ");

            AssertBody(
                VarDecl("i", Fix.Int),
                VarDecl("j", Fix.Int),
                Assign("j", Const("1")),
                Assign("i", RefExpr("j")),
                Fix.EmptyCompletion);
        }

        [Test]
        public void SimpleDecl()
        {
            CompleteInMethod(
                @"
                int i = $
            ");

            AssertBody(
                VarDecl("i", Fix.Int),
                Assign("i", new CompletionExpression()));
        }

        [Test]
        public void SimpleDecl_No()
        {
            CompleteInMethod(
                @"
                int i$ = 1;
            ");

            AssertBody(
                VarDecl("i", Fix.Int),
                Assign("i", Const("1")));
        }

        [Test]
        public void SimpleAssign()
        {
            CompleteInMethod(
                @"
                int i;
                i = $
            ");

            AssertBody(
                VarDecl("i", Fix.Int),
                Assign("i", new CompletionExpression()));
        }

        [Test]
        public void SimpleAssign_No()
        {
            CompleteInMethod(
                @"
                int i;
                i$ = 1;
            ");

            AssertBody(
                VarDecl("i", Fix.Int),
                Assign("i", Const("1")));
        }
    }
}
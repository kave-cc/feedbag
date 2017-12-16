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

using JetBrains.ReSharper.Psi.CSharp.Tree;
using KaVE.Commons.Model.SSTs.Impl.Blocks;
using KaVE.Commons.Model.SSTs.Impl.Statements;
using KaVE.RS.Commons.Analysis.CompletionTarget;
using NUnit.Framework;
using Fix = KaVE.RS.Commons.Tests_Integration.Analysis.SSTAnalysisTestSuite.SSTAnalysisFixture;

namespace KaVE.RS.Commons.Tests_Integration.Analysis.CompletionTargetTestSuite
{
    internal class SyntaxElements : BaseTest
    {
        [Test]
        public void CompletionOnOpeningBracket()
        {
            CompleteInClass(
                @" 
                public void M()
                {$
                   
                }");

            AssertCompletionMarker<IMethodDeclaration>(CompletionCase.InBody);

            AssertBody(
                "M",
                Fix.EmptyCompletion);
        }

        [Test]
        public void CompletionBeforeClosingBracket()
        {
            CompleteInClass(
                @" 
                public void M()
                {
                   
                $}");

            AssertCompletionMarker<IMethodDeclaration>(CompletionCase.InBody);

            AssertBody(
                "M",
                Fix.EmptyCompletion);
        }

        [Test]
        public void CompletionBeforeClosingBracket_WithStatement()
        {
            CompleteInClass(
                @" 
                public void M()
                {
                   continue;
                $}");

            AssertCompletionMarker<IContinueStatement>(CompletionCase.EmptyCompletionAfter);

            AssertBody(
                "M",
                new ContinueStatement(),
                Fix.EmptyCompletion);
        }

        [Test]
        public void CompletionOnClosingBracket_Method()
        {
            CompleteInClass(
                @" 
                public void M()
                {
                  
                } $"); // on bracket does not trigger completion

            AssertCompletionMarker<IMethodDeclaration>(CompletionCase.EmptyCompletionAfter);

            AssertBody("M");
        }

        [Test]
        public void CompletionOnClosingBracket_If()
        {
            CompleteInClass(
                @" 
                public void M()
                {
                  if(true) { }$
                }");

            AssertCompletionMarker<IIfStatement>(CompletionCase.EmptyCompletionAfter);

            AssertBody(
                "M",
                new IfElseBlock
                {
                    Condition = Const("true")
                },
                Fix.EmptyCompletion);
        }

        [Test]
        public void CompletionOnSemicolon()
        {
            CompleteInClass(
                @" 
                public void M()
                {
                    ;$
                }");

            AssertCompletionMarker<IEmptyStatement>(CompletionCase.EmptyCompletionAfter);

            AssertBody(
                "M",
                Fix.EmptyCompletion);
        }

        [Test]
        public void CompletionOnSemicolonAfterStatement()
        {
            CompleteInClass(
                @" 
                public void M()
                {
                    return;$
                }");

            AssertCompletionMarker<IReturnStatement>(CompletionCase.EmptyCompletionAfter);

            AssertBody(
                "M",
                new ReturnStatement {IsVoid = true},
                Fix.EmptyCompletion);
        }

        [Test]
        public void AfterIfParenthesis()
        {
            CompleteInMethod(
                @" 
                if(true)$
            ");

            AssertCompletionMarker<IIfStatement>(CompletionCase.InBody);

            AssertBody(
                "M",
                new IfElseBlock
                {
                    Condition = Const("true"),
                    Then = {Fix.EmptyCompletion}
                });
        }

        [Test]
        public void AfterIfParenthesis_WithBody()
        {
            CompleteInMethod(
                @" 
                if(true)$
                    continue;
            ");

            AssertCompletionMarker<IIfStatement>(CompletionCase.InBody);

            AssertBody(
                "M",
                new IfElseBlock
                {
                    Condition = Const("true"),
                    Then =
                    {
                        Fix.EmptyCompletion,
                        new ContinueStatement()
                    }
                });
        }
    }
}
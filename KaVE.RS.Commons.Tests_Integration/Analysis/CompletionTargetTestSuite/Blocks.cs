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

using JetBrains.ReSharper.Psi.CSharp.Tree;
using KaVE.Commons.Model.Naming;
using KaVE.Commons.Model.SSTs.Blocks;
using KaVE.Commons.Model.SSTs.Impl.Blocks;
using KaVE.Commons.Model.SSTs.Impl.Expressions.Assignable;
using KaVE.RS.Commons.Analysis.CompletionTarget;
using NUnit.Framework;
using Fix = KaVE.RS.Commons.Tests_Integration.Analysis.SSTAnalysisTestSuite.SSTAnalysisFixture;

namespace KaVE.RS.Commons.Tests_Integration.Analysis.CompletionTargetTestSuite
{
    internal class Blocks : BaseTest
    {
        [Test]
        public void Block_Before()
        {
            CompleteInMethod(
                @"
                $
                { }
            ");

            AssertCompletionMarker<IBlock>(CompletionCase.EmptyCompletionBefore);

            AssertBody(
                "M",
                Fix.EmptyCompletion);
        }

        [Test]
        public void Block_In()
        {
            CompleteInMethod(
                @" 
                { $ }
            ");

            AssertCompletionMarker<IBlock>(CompletionCase.InBody);

            AssertBody(
                "M",
                Fix.EmptyCompletion);
        }

        [Test]
        public void Block_After()
        {
            CompleteInMethod(
                @"
                { }
                $
            ");

            AssertCompletionMarker<IBlock>(CompletionCase.EmptyCompletionAfter);

            AssertBody(
                "M",
                Fix.EmptyCompletion);
        }

        // switch moved

        [Test]
        public void InTry_Body()
        {
            CompleteInMethod(
                @"
                try
                {
                    $
                }
                finally { }
            ");

            AssertCompletionMarker<ITryStatement>(CompletionCase.InBody);

            AssertBody(
                "M",
                new TryBlock
                {
                    Body = {Fix.EmptyCompletion}
                }
            );
        }

        [Test]
        public void InTry_GeneralCatch()
        {
            CompleteInMethod(
                @"
                try {}
                catch
                {
                    $
                }
            ");

            AssertCompletionMarker<IGeneralCatchClause>(CompletionCase.InBody);

            AssertBody(
                "M",
                new TryBlock
                {
                    CatchBlocks =
                    {
                        new CatchBlock
                        {
                            Kind = CatchBlockKind.General,
                            Body = {Fix.EmptyCompletion}
                        }
                    }
                }
            );
        }

        [Test]
        public void InTry_SpecificCatch()
        {
            CompleteInMethod(
                @"
                try {}
                catch (Exception e)
                {
                    $
                }
            ");

            AssertCompletionMarker<ISpecificCatchClause>(CompletionCase.InBody);

            AssertBody(
                "M",
                new TryBlock
                {
                    CatchBlocks =
                    {
                        new CatchBlock
                        {
                            Kind = CatchBlockKind.Default,
                            Parameter = Names.Parameter("[{0}] e", Fix.Exception),
                            Body = {Fix.EmptyCompletion}
                        }
                    }
                }
            );
        }

        [Test]
        public void InTry_Finally()
        {
            CompleteInMethod(
                @"
                try {}
                finally
                {
                    $
                }
            ");

            AssertCompletionMarker<ITryStatement>(CompletionCase.InFinally);

            AssertBody(
                "M",
                new TryBlock
                {
                    Finally = {Fix.EmptyCompletion}
                }
            );
        }

        [Test]
        public void InDo()
        {
            CompleteInMethod(
                @"
                do
                {
                    $
                } while (true);
            ");

            AssertCompletionMarker<IDoStatement>(CompletionCase.InBody);

            AssertBody(
                "M",
                new DoLoop
                {
                    Body = {Fix.EmptyCompletion},
                    Condition = Const("true")
                }
            );
        }

        [Test]
        public void InForeach()
        {
            CompleteInMethod(
                @"
                foreach (var x in null)
                {
                    $
                }
            ");

            AssertCompletionMarker<IForeachStatement>(CompletionCase.InBody);

            AssertBody(
                "M",
                VarDecl("$0", Fix.Unknown),
                Assign("$0", Const("null")),
                new ForEachLoop
                {
                    LoopedReference = VarRef("$0"),
                    Declaration = VarDecl("x", Fix.Object),
                    Body = {Fix.EmptyCompletion}
                }
            );
        }

        [Test]
        public void InFor()
        {
            CompleteInMethod(
                @"
                for (;;)
                {
                    $
                }
            ");

            AssertCompletionMarker<IForStatement>(CompletionCase.InBody);

            AssertBody(
                "M",
                new ForLoop
                {
                    Body = {Fix.EmptyCompletion}
                }
            );
        }

        [Test]
        public void InIf_Then()
        {
            CompleteInMethod(
                @"
                if (true)
                { $ }
                else {}
            ");

            AssertCompletionMarker<IIfStatement>(CompletionCase.InBody);

            AssertBody(
                "M",
                new IfElseBlock
                {
                    Condition = Const("true"),
                    Then = {Fix.EmptyCompletion}
                }
            );
        }

        [Test]
        public void InIf_Else()
        {
            CompleteInMethod(
                @"
                if (true) {}
                else { $ }
            ");

            AssertCompletionMarker<IIfStatement>(CompletionCase.InElse);

            AssertBody(
                "M",
                new IfElseBlock
                {
                    Condition = Const("true"),
                    Else = {Fix.EmptyCompletion}
                }
            );
        }

        [Test]
        public void InLock()
        {
            CompleteInMethod(
                @"
                lock(this) { $ }
            ");

            AssertCompletionMarker<ILockStatement>(CompletionCase.InBody);

            AssertBody(
                "M",
                new LockBlock
                {
                    Reference = VarRef("this"),
                    Body = {Fix.EmptyCompletion}
                }
            );
        }

        [Test]
        public void InUnchecked()
        {
            CompleteInMethod(
                @"
                unchecked { $ }
            ");

            AssertCompletionMarker<IUncheckedStatement>(CompletionCase.InBody);

            AssertBody(
                "M",
                new UncheckedBlock
                {
                    Body = {Fix.EmptyCompletion}
                }
            );
        }

        [Test]
        public void InUnsafe()
        {
            CompleteInMethod(
                @"
                unsafe { $ }
            ");

            AssertCompletionMarker<IUnsafeCodeUnsafeStatement>(CompletionCase.InBody);

            AssertBody(
                "M",
                new UnsafeBlock()
            );
        }

        [Test]
        public void InUsing()
        {
            CompleteInMethod(
                @"
                using(this) { $ }
            ");

            AssertCompletionMarker<IUsingStatement>(CompletionCase.InBody);

            AssertBody(
                "M",
                new UsingBlock
                {
                    Reference = VarRef("this"),
                    Body = {Fix.EmptyCompletion}
                }
            );
        }

        [Test]
        public void InWhile()
        {
            CompleteInMethod(
                @"
                while (true) { $ }
            ");

            AssertCompletionMarker<IWhileStatement>(CompletionCase.InBody);

            AssertBody(
                "M",
                new WhileLoop
                {
                    Condition = Const("true"),
                    Body = {Fix.EmptyCompletion}
                }
            );
        }

        [Test]
        public void InLambdaExpr()
        {
            CompleteInMethod(
                @"
                () => { $ };
            ");

            AssertCompletionMarker<ILambdaExpression>(CompletionCase.InBody);

            AssertBody(
                "M",
                ExprStmt(
                    new LambdaExpression
                    {
                        Name = Names.Lambda("[?] ()"),
                        Body = {Fix.EmptyCompletion}
                    })
            );
        }
    }
}
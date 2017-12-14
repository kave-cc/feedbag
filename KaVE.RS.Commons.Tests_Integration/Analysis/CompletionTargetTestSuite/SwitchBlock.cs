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
using KaVE.RS.Commons.Analysis.CompletionTarget;
using NUnit.Framework;
using Fix = KaVE.RS.Commons.Tests_Integration.Analysis.SSTAnalysisTestSuite.SSTAnalysisFixture;

namespace KaVE.RS.Commons.Tests_Integration.Analysis.CompletionTargetTestSuite
{
    internal class SwitchBlock : BaseTest
    {
        [Test]
        public void Before()
        {
            CompleteInMethod(
                @"
                $
                switch (this)
                {
                    default:
                        break;
                }");

            AssertCompletionMarker<ISwitchStatement>(CompletionCase.EmptyCompletionBefore);
        }

        [Test]
        public void After()
        {
            CompleteInMethod(
                @"
                switch (this)
                {
                    default:
                        continue;
                }
                $
            ");

            AssertCompletionMarker<ISwitchStatement>(CompletionCase.EmptyCompletionAfter);
        }

        [Test]
        public void InParameter()
        {
            CompleteInMethod(
                @"
                switch (thi$)
                {
                    default:
                        continue;
                }
                $
            ");

            AssertCompletionMarker<IReferenceExpression>(CompletionCase.Undefined);
        }

        [Test]
        public void InLabel()
        {
            CompleteInMethod(
                @"
                switch (this)
                {
                    defa$
                }");

            AssertCompletionMarker<ISwitchSection>(CompletionCase.InBody);
        }

        [Test]
        public void InLabel_WithColons()
        {
            CompleteInMethod(
                @"
                switch (this)
                {
                    defa$:
                }");

            AssertCompletionMarker<ISwitchSection>(CompletionCase.InBody);
        }

        [Test]
        public void InLabel_Case()
        {
            CompleteInMethod(
                @"
                switch (this)
                {
                    case$
                }");

            AssertCompletionMarker<ISwitchSection>(CompletionCase.InSignature);
        }

        [Test]
        public void InLabel_Case2()
        {
            CompleteInMethod(
                @"
                switch (this)
                {
                    case $
                }");

            AssertCompletionMarker<ISwitchSection>(CompletionCase.InSignature);
        }

        [Test]
        public void InLabel_Case3()
        {
            CompleteInMethod(
                @"
                switch (this)
                {
                    case 3$
                }");

            AssertCompletionMarker<ISwitchSection>(CompletionCase.InSignature);
        }

        [Test]
        public void InLabel_Case4()
        {
            CompleteInMethod(
                @"
                switch (this)
                {
                    case 3:$
                }");

            AssertCompletionMarker<ISwitchSection>(CompletionCase.InBody);
        }

        [Test]
        public void AfterLabel()
        {
            CompleteInMethod(
                @"
                switch (this)
                {
                    default:
                        $
                }");

            AssertCompletionMarker<ISwitchSection>(CompletionCase.InBody);
        }

        [Test]
        public void AfterLabelMulti()
        {
            CompleteInMethod(
                @"
                switch (this)
                {
                    case 0:
                        $
                    case 1:
                        continue;
                }");

            AssertCompletionMarker<ISwitchSection>(CompletionCase.InBody);
        }

        [Test]
        public void AfterLabelNonEmpty()
        {
            CompleteInMethod(
                @"
                switch (this)
                {
                    default:
                        $
                        continue;
                }");

            AssertCompletionMarker<IContinueStatement>(CompletionCase.EmptyCompletionBefore);
        }

        [Test]
        public void Nested()
        {
            CompleteInMethod(
                @"
                switch (this)
                {
                    default:
                        continue;
                        $
                }");

            AssertCompletionMarker<IContinueStatement>(CompletionCase.EmptyCompletionAfter);
        }

        [Test]
        public void Nested2()
        {
            CompleteInMethod(
                @"
                switch (this)
                {
                    default:
                        int i;
                        continue;
                        $
                }");

            AssertCompletionMarker<IContinueStatement>(CompletionCase.EmptyCompletionAfter);
        }

        [Test]
        public void Nested3()
        {
            CompleteInMethod(
                @"
                switch (this)
                {
                    default:
                        continue;
                        $
                        int i;
                }");

            AssertCompletionMarker<IContinueStatement>(CompletionCase.EmptyCompletionAfter);
        }
    }
}
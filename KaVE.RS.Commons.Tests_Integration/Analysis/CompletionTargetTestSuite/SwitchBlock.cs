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
            ");

            AssertCompletionMarker<IReferenceExpression>(CompletionCase.Undefined);
        }

        [Test]
        public void InSwitchCase()
        {
            CompleteInMethod(
                @"
                switch (this)
                {
                    default:
                        $
                }
            ");

            AssertCompletionMarker<ISwitchCaseLabel>(CompletionCase.InBody);

            AssertBody(
                "M",
                new KaVE.Commons.Model.SSTs.Impl.Blocks.SwitchBlock
                {
                    Reference = VarRef("this"),
                    DefaultSection = {Fix.EmptyCompletion}
                }
            );
        }

        [Test]
        public void InEmptySwitch()
        {
            CompleteInMethod(
                @"
                switch (this)
                {
                   $
                }
            ");

            AssertCompletionMarker<ISwitchStatement>(CompletionCase.InBody);

            AssertBody(
                "M",
                new KaVE.Commons.Model.SSTs.Impl.Blocks.SwitchBlock {Reference = VarRef("this")}
            );
        }

        public string[] SignatureCases()
        {
            return new[]
            {
                "swi$tch (this)",
                "switch$ (this)",
                "switch $ (this)",
                "switch $(this)",
                "switch (this)$",
                "switch (this) $"
            };
        }

        [TestCaseSource(nameof(SignatureCases))]
        public void InSignature(string sig)
        {
            CompleteInMethod(
                @"
                " + sig + @"
                {
                    default:
                        continue;
                }
            ");

            AssertCompletionMarker<ISwitchStatement>(CompletionCase.InSignature);
        }

        public string[] LabelCases()
        {
            return new[]
            {
                // indistinguishable from reference: "defa$",
                // indistinguishable from labeled statement: "defa$:",
                "case$",
                "case $",
                "case 3 $",
                "case$:",
                "case $:",
                "case 3 $:"
            };
        }

        [TestCaseSource(nameof(LabelCases))]
        public void InLabel(string sig)
        {
            CompleteInMethod(
                @"
                switch (this)
                {
                    " + sig + @"
                }");

            AssertCompletionMarker<ISwitchCaseLabel>(CompletionCase.InSignature);
        }

        public void InLabelParam_Lit()
        {
            CompleteInMethod(
                @"
                switch (this)
                {
                    case 3$:
                }");

            AssertCompletionMarker<ICSharpLiteralExpression>(CompletionCase.Undefined);
        }

        public void InLabelParam_Ref()
        {
            CompleteInMethod(
                @"
                switch (this)
                {
                    case 1.G$:
                }");

            AssertCompletionMarker<IReferenceExpression>(CompletionCase.Undefined);
        }

        [Test]
        public void AfterLabel_Case()
        {
            CompleteInMethod(
                @"
                switch (this)
                {
                    case 3:$
                }");

            AssertCompletionMarker<ISwitchCaseLabel>(CompletionCase.InBody);
        }

        [Test]
        public void AfterLabel_Default()
        {
            CompleteInMethod(
                @"
                switch (this)
                {
                    default:
                        $
                }");

            AssertCompletionMarker<ISwitchCaseLabel>(CompletionCase.InBody);
        }

        [Test]
        public void AfterLabel_Multi()
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

            AssertCompletionMarker<ISwitchCaseLabel>(CompletionCase.InBody);
        }

        [Test]
        public void AfterLabel_NonEmpty()
        {
            CompleteInMethod(
                @"
                switch (this)
                {
                    default:
                        $
                        continue;
                }");

            AssertCompletionMarker<ISwitchCaseLabel>(CompletionCase.InBody);
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
                        break;
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
                        break;
                }");

            AssertCompletionMarker<IContinueStatement>(CompletionCase.EmptyCompletionAfter);
        }
    }
}
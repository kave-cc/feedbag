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

namespace KaVE.RS.Commons.Tests_Integration.Analysis.CompletionTargetTestSuite
{
    internal class References : BaseTest
    {
        [Test]
        public void ShouldBeReference()
        {
            CompleteInMethod(
                @"
                object o;
                o.$
            ");
            AssertCompletionMarker<IReferenceExpression>(CompletionCase.Undefined);
        }

        [Test]
        public void ShouldBeReference_WithPrefix()
        {
            CompleteInMethod(
                @"
                object o;
                o.G$
            ");
            AssertCompletionMarker<IReferenceExpression>(CompletionCase.Undefined);
        }

        [Test]
        public void ShouldBeReference_OnLiteral()
        {
            CompleteInMethod(
                @"
                1.$
            ");
            AssertCompletionMarker<IReferenceExpression>(CompletionCase.Undefined);
        }

        [Test]
        public void ShouldBeReference_OnLiteralWithPrefix()
        {
            CompleteInMethod(
                @"
                1.G$
            ");
            AssertCompletionMarker<IReferenceExpression>(CompletionCase.Undefined);
        }

        [Test]
        public void ShouldBeReference_OnThis()
        {
            CompleteInMethod(
                @"
                this.$
            ");
            AssertCompletionMarker<IReferenceExpression>(CompletionCase.Undefined);
        }

        [Test]
        public void ShouldBeReference_OnBase()
        {
            CompleteInCSharpFile(
                @"
                namespace N
                {
                    class S {}

                    class C : S
                    {
                        public void M()
                        {
                            base.$
                        }
                    }
                }
            ");
            AssertCompletionMarker<IReferenceExpression>(CompletionCase.Undefined);
        }

        [Test]
        public void ShouldBeSuperTypeIfExplicitBaseIsSpecified()
        {
            CompleteInCSharpFile(
                @"
                namespace N
                {
                    class S {}

                    class C : S
                    {
                        public void M()
                        {
                            base.$
                        }
                    }
                }
            ");
            AssertCompletionMarker<IReferenceExpression>(CompletionCase.Undefined);
        }

        [Test]
        public void ShouldBeCastType()
        {
            CompleteInMethod(
                @"
                object o;
                ((string) o).$
            ");
            AssertCompletionMarker<IReferenceExpression>(CompletionCase.Undefined);
        }

        [Test]
        public void ShouldBeSafeCastType()
        {
            CompleteInMethod(
                @"
                object o;
                (o as System.Collections.IList).$
            ");
            AssertCompletionMarker<IReferenceExpression>(CompletionCase.Undefined);
        }

        [Test]
        public void ShouldBeReturnType()
        {
            CompleteInClass(
                @"
                public System.Collections.IList GetList() {}
                
                public void M()
                {
                    GetList().$
                }");
            AssertCompletionMarker<IReferenceExpression>(CompletionCase.Undefined);
        }

        [Test]
        public void ShouldBeInstantiatedType()
        {
            CompleteInMethod(
                @"
                (new object()).$
            ");
            AssertCompletionMarker<IReferenceExpression>(CompletionCase.Undefined);
        }

        [Test]
        public void ShouldBeField()
        {
            CompleteInCSharpFile(
                @"
                class C
                {
                    private string _f;
                
                    public void M()
                    {
                        _f.$
                    }
                }");
            AssertCompletionMarker<IReferenceExpression>(CompletionCase.Undefined);
        }

        [Test]
        public void ShouldBeConstLocalVariable()
        {
            CompleteInMethod(
                @"
                const string Const;
                Const.$
            ");
            AssertCompletionMarker<IReferenceExpression>(CompletionCase.Undefined);
        }

        [Test]
        public void ShouldBeNamespace()
        {
            CompleteInMethod(
                @"
                System.$
            ");
            AssertCompletionMarker<IReferenceExpression>(CompletionCase.Undefined);
        }

        [Test]
        public void ShouldIgnoreWhitespaces()
        {
            CompleteInMethod(
                @"
                object o;
                o.
                    $
            ");
            AssertCompletionMarker<IReferenceExpression>(CompletionCase.Undefined);
        }

        [Test]
        public void ShouldIgnoreWhitespacesBeforePrefix()
        {
            CompleteInMethod(
                @"
                object o;
                o.
                    Equ$
            ");
            AssertCompletionMarker<IReferenceExpression>(CompletionCase.Undefined);
        }

        [Test]
        public void ShouldIgnorePreceedingCompleteExpression()
        {
            CompleteInMethod(
                @"
                object o;
                o.GetHashCode();
                $
            ");
            AssertCompletionMarker<IInvocationExpression>(CompletionCase.EmptyCompletionAfter);
        }

        [Test]
        public void ShouldBeReferencedType()
        {
            CompleteInMethod(
                @"
                object.$
            ");
            AssertCompletionMarker<IReferenceExpression>(CompletionCase.Undefined);
        }

        [Test]
        public void ShouldBeParameter()
        {
            CompleteInClass(
                @"
                void M(object param)
                {
                    param.$
                }");
            AssertCompletionMarker<IReferenceExpression>(CompletionCase.Undefined);
        }
    }
}
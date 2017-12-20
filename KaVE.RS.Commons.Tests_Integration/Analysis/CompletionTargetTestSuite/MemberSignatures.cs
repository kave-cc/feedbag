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
    internal class MemberSignatures : BaseTest
    {
        public string[] FieldCases()
        {
            return new[]
            {
                "pub$lic int _f;",
                "public$ int _f;",
                "public $ int _f;",
                "public $int _f;",
                "public in$t _f;",
                "public int$ _f;",
                "public int $ _f;",
                "public int $_f;",
                "public int _$f;",
                "public int _f$;"
            };
        }

        [TestCaseSource(nameof(FieldCases))]
        public void FieldTests(string sig)
        {
            CompleteInClass(sig);
            AssertCompletionMarker<IFieldDeclaration>(CompletionCase.InSignature);
        }

        [Test]
        public void FieldBefore()
        {
            CompleteInClass(
                @"
                $
                public int _f;");
            AssertCompletionMarker<IFieldDeclaration>(CompletionCase.EmptyCompletionBefore);
        }

        [Test]
        public void FieldAfter()
        {
            CompleteInClass(
                @"
                public int _f;
                $");
            AssertCompletionMarker<IFieldDeclaration>(CompletionCase.EmptyCompletionAfter);
        }

        public string[] EventCases()
        {
            return new[]
            {
                "pub$lic event Action A;",
                "public$ event Action A;",
                "public $event Action A;",
                "public ev$ent Action A;",
                "public event$ Action A;",
                "public event $Action A;",
                "public event Act$ion A;",
                "public event Action$ A;",
                "public event Action $A;",
                "public event Action Aa$aa;",
                "public event Action A$;"
            };
        }

        [TestCaseSource(nameof(EventCases))]
        public void EventTests(string sig)
        {
            CompleteInClass(sig);
            AssertCompletionMarker<IEventDeclaration>(CompletionCase.InSignature);
        }

        [Test]
        public void EventBefore()
        {
            CompleteInClass(
                @"
                $
                public event Action A;");
            AssertCompletionMarker<IEventDeclaration>(CompletionCase.EmptyCompletionBefore);
        }

        [Test]
        public void EventAfter()
        {
            CompleteInClass(
                @"
                public event Action A;
                $");
            AssertCompletionMarker<IEventDeclaration>(CompletionCase.EmptyCompletionAfter);
        }

        [TestCaseSource(nameof(DelegateCases))]
        public void DelegateTests(string sig)
        {
            CompleteInClass(sig);
            AssertCompletionMarker<IDelegateDeclaration>(CompletionCase.InSignature);
        }

        public string[] DelegateCases()
        {
            return new[]
            {
                "pu$blic delegate int M(int i);",
                "public$ delegate int M(int i);",
                "public $delegate int M(int i);",
                "public dele$gate int M(int i);",
                "public delegate$ int M(int i);",
                "public delegate $int M(int i);",
                "public delegate i$nt M(int i);",
                "public delegate int$ M(int i);",
                "public delegate int $M(int i);",
                "public delegate int M$e(int i);",
                "public delegate int Me$(int i);",
                "public delegate int M($int i);",
                "public delegate int M(in$t i);",
                "public delegate int M(int$ i);",
                "public delegate int M(int $i);",
                "public delegate int M(int ii$i);",
                "public delegate int M(int i$);"
            };
        }

        [Test]
        public void DelegateBefore()
        {
            CompleteInClass(
                @"
                $
                public delegate int M(int i);");
            AssertCompletionMarker<IDelegateDeclaration>(CompletionCase.EmptyCompletionBefore);
        }

        [Test]
        public void DelegateAfter()
        {
            CompleteInClass(
                @"
                public delegate int M(int i);
                $");
            AssertCompletionMarker<IDelegateDeclaration>(CompletionCase.EmptyCompletionAfter);
        }

        public string[] PropertyCases()
        {
            return new[]
            {
                "pub$lic int P { get;set;}",
                "public$ int P { get;set;}",
                "public $ int P { get;set;}",
                "public $int P { get;set;}",
                "public i$nt P { get;set;}",
                "public int$ P { get;set;}",
                "public int $ P { get;set;}",
                "public int $P { get;set;}",
                "public int Pr$op { get;set;}",
                "public int P$ { get;set;}",
                "public int P {$ get;set;}",
                "public int P { $ get;set;}",
                "public int P { $get;set;}",
                "public int P { $ get;set;}",
                "public int P { ge$t;set;}",
                "public int P { get$;set;}",
                "public int P { get;$set;}",
                "public int P { get;se$t;}",
                "public int P { get;set$;}",
                "public int P { get;set;$}",
                "public int P { get ${ return 1; } set {}}",
                "public int P { get { return 1; } $ set {}}",
                "public int P { get { return 1; } $set {}}",
                "public int P { get { return 1; } set $ {}}",
                "public int P { get { return 1; } set {}$}",
                "public int P { get { return 1; } set {} $ }"
            };
        }

        [TestCaseSource(nameof(PropertyCases))]
        public void PropertyTests(string sig)
        {
            CompleteInClass(sig);
            AssertCompletionMarker<IPropertyDeclaration>(CompletionCase.InSignature);
        }

        [Test]
        public void PropertyTests_GetBody()
        {
            CompleteInClass("public int P { get { $ } set { }}");
            AssertCompletionMarker<IPropertyDeclaration>(CompletionCase.InGetBody);
        }

        [Test]
        public void PropertyTests_SetBody()
        {
            CompleteInClass("public int P { get { return 1; } set { $ }}");
            AssertCompletionMarker<IPropertyDeclaration>(CompletionCase.InSetBody);
        }

        [Test]
        public void PropertyBefore()
        {
            CompleteInClass(
                @"
                $
                public int P { get;set;}");
            AssertCompletionMarker<IPropertyDeclaration>(CompletionCase.EmptyCompletionBefore);
        }

        [Test]
        public void PropertyAfter()
        {
            CompleteInClass(
                @"
                public int P { get;set;}
                $");
            AssertCompletionMarker<IPropertyDeclaration>(CompletionCase.EmptyCompletionAfter);
        }
    }
}
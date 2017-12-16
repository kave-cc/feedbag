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
using KaVE.RS.Commons.Analysis.CompletionTarget;
using NUnit.Framework;

namespace KaVE.RS.Commons.Tests_Integration.Analysis.CompletionTargetTestSuite
{
    internal class MethodSignatures : BaseTest
    {
        public string[] Cases()
        {
            return new[]
            {
                "public void MyMe$thod() {}",
                "public voi$ MyMethod() {}",
                "publi$ void MyMethod() {}",
                "public void MyMethod(in$) {}",
                "public void MyMethod(int i$) {}",
                // no trigger: "public void MyMethod(int i)$ {}",
                //
                "public void M$<T>() {}",
                "public void M<$T>() {}",
                "public void M<T$>() {}",
                // no trigger: "public void M<T>$() {}",
                //
                // no trigger: "public void M<T>()$ where T:new() {}",
                // no trigger: "public void M<T>() $where T:new() {}",
                // no trigger: "public void M<T>() wh$ere T:new() {}",
                // no trigger: "public void M<T>() where$ T:new() {}",
                "public void M<T>() where $T:new() {}",
                "public void M<T>() where T$:new() {}",
                "public void M<T>() where T:$new() {}",
                "public void M<T>() where T:n$ew() {}"
                // no trigger: "public void M<T>() where T:new()$ {}",
                // no trigger: "public void M<T>() where T:new() ${}"
            };
        }

        [TestCaseSource(nameof(Cases))]
        public void Test(string sig)
        {
            CompleteInClass(sig);
            AssertCompletionMarker<IMethodDeclaration>(CompletionCase.InSignature);
        }

        [Test]
        public void InMethodSignature_Regression()
        {
            CompleteInClass(
                @"
                public void Foo$() {
                    int i;
                }
            ");
            AssertCompletionMarker<IMethodDeclaration>(CompletionCase.InSignature);
        }

        [Test]
        public void NonEmptyBeforeMethodDecl()
        {
            CompleteInClass(
                @"
                G$
                public void M() {}
            ");
            AssertCompletionMarker<IMethodDeclaration>(CompletionCase.InSignature);
        }

        [Test]
        public void NonEmptyAfterMethodDecl()
        {
            CompleteInClass(
                @"
                public void M() {}
                G$
            ");
            AssertCompletionMarker<IMethodDeclaration>(CompletionCase.InSignature);
        }

        [Test]
        public void InLambdaSignature()
        {
            CompleteInMethod(
                @"
                ( $ ) => {};
            ");
            AssertCompletionMarker<ILambdaExpression>(CompletionCase.InSignature);
        }
    }
}
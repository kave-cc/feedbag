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
using JetBrains.ReSharper.Psi.Tree;
using KaVE.RS.Commons.Analysis.CompletionTarget;
using NUnit.Framework;

namespace KaVE.RS.Commons.Tests_Integration.Analysis.CompletionTargetTestSuite
{
    internal class EmptyCompletions : BaseTest
    {
        [Test]
        public void InEmptyFile()
        {
            CompleteInCSharpFile(
                @"
                $
            ");
            AssertCompletionMarker<ICSharpFile>(CompletionCase.InBody);
        }

        [Test]
        public void EmptyBeforeNamespace()
        {
            CompleteInCSharpFile(
                @"
                $
                namespace N {}
            ");
            AssertCompletionMarker<INamespaceDeclaration>(CompletionCase.EmptyCompletionBefore);
        }

        [Test]
        public void EmptyAfterNamespace()
        {
            CompleteInCSharpFile(
                @"
                namespace N {}
                $
            ");
            AssertCompletionMarker<INamespaceDeclaration>(CompletionCase.EmptyCompletionAfter);
        }

        [Test]
        public void InEmptyNamespace()
        {
            CompleteInNamespace(
                @"
                $
            ");
            AssertCompletionMarker<INamespaceDeclaration>(CompletionCase.InBody);
        }

        [Test]
        public void EmptyBeforeClass()
        {
            CompleteInNamespace(
                @"
                $
                public class C {}
            ");
            AssertCompletionMarker<IClassDeclaration>(CompletionCase.EmptyCompletionBefore);
        }

        [Test]
        public void EmptyAfterClass()
        {
            CompleteInNamespace(
                @"
                public class C {}
                $
            ");
            AssertCompletionMarker<IClassDeclaration>(CompletionCase.EmptyCompletionAfter);
        }

        [Test]
        public void InEmptyClass()
        {
            CompleteInClass(
                @"
                $
            ");
            AssertCompletionMarker<IClassDeclaration>(CompletionCase.InBody);
        }

        [Test]
        public void EmptyBeforeMethodDecl()
        {
            CompleteInClass(
                @"
                $
                public void M() {}
            ");
            AssertCompletionMarker<IMethodDeclaration>(CompletionCase.EmptyCompletionBefore);
        }

        [Test]
        public void EmptyAfterMethodDecl()
        {
            CompleteInClass(
                @"
                public void M() {}
                $
            ");
            AssertCompletionMarker<IMethodDeclaration>(CompletionCase.EmptyCompletionAfter);
        }

        [Test]
        public void InEmptyMethod()
        {
            CompleteInMethod(
                @"
                $
            ");
            AssertCompletionMarker<IMethodDeclaration>(CompletionCase.InBody);
        }

        [Test]
        public void EmptyBeforeStatement()
        {
            CompleteInMethod(
                @"
                $
                object o;
            ");
            AssertCompletionMarker<ILocalVariableDeclaration>(CompletionCase.Undefined);
        }

        [Test]
        public void EmptyAfterStatement()
        {
            CompleteInMethod(
                @"
                object o;
                $
            ");
            AssertCompletionMarker<ILocalVariableDeclaration>(CompletionCase.Undefined);
        }
    }
}
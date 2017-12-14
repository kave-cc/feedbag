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
    internal class Comments : BaseTest
    {
        [Test]
        public void InComment_Line()
        {
            CompleteInMethod(
                @" 
                // comment $
                object o;
            ");

            AssertInvalidCompletionTarget();
        }

        [Test]
        public void InEmptyMethodWithComment()
        {
            CompleteInMethod(
                @" 
                // comment
                $
            ");

            AssertCompletionMarker<IMethodDeclaration>(CompletionCase.InBody);
        }

        [Test]
        public void InComment_Block()
        {
            CompleteInMethod(
                @" 
                /*
                 * $
                 */
                object o;
            ");

            AssertInvalidCompletionTarget();
        }

        [Test]
        public void InComment_Block_RightAtTheEnd()
        {
            CompleteInMethod(
                @" 
                /*
                 */$
                object o;
            ");

            AssertCompletionMarker<ILocalVariableDeclaration>(CompletionCase.EmptyCompletionBefore);
        }
    }
}
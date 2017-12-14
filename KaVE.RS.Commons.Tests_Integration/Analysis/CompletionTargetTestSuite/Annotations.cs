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
    internal class Annotations : BaseTest
    {
        [Test]
        public void InAnnotation_Class()
        {
            CompleteInNamespace(
                @" 
                [Asd$]
                class C {}
            ");
            AssertCompletionMarker<IAttribute>(CompletionCase.Undefined);
        }


        [Test]
        public void InAnnotation_Method()
        {
            CompleteInClass(
                @" 
                [Asd$]
                void M() {}
            ");
            AssertCompletionMarker<IAttribute>(CompletionCase.Undefined);
        }

        [Test]
        public void InAnnotation_Parameter()
        {
            CompleteInClass(
                @" 
                void M([Asd$] int i) {}
            ");
            AssertCompletionMarker<IAttribute>(CompletionCase.Undefined);
        }

        [Test]
        public void Annotation_InEmptyClass()
        {
            CompleteInClass(
                @" 
                [TestCa$]
            ");
            AssertCompletionMarker<IAttribute>(CompletionCase.Undefined);
        }
    }
}
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

using KaVE.RS.Commons.Analysis.CompletionTarget;
using KaVE.RS.Commons.Tests_Integration.Analysis.SSTAnalysisTestSuite;
using NUnit.Framework;

namespace KaVE.RS.Commons.Tests_Integration.Analysis.CompletionTargetTestSuite
{
    internal abstract class BaseTest : BaseSSTAnalysisTest
    {
        protected void AssertInvalidCompletionTarget()
        {
            var actualNode = LastCompletionMarker.HandlingNode;
            var actualCase = LastCompletionMarker.Case;

            if (actualNode == null)
            {
                Assert.Fail(
                    "Invalid completion marker expected. Node is 'null' as expected, but case is {0}.",
                    actualCase);
            }

            if (actualCase != CompletionCase.Invalid)
            {
                Assert.Fail(
                    "Invalid completion marker expected ('null', '{3}'). However, node type is {0}, case is {1}.\n\nNode details:\n{2}\n\n",
                    actualNode.GetType(),
                    actualCase,
                    actualNode,
                    CompletionCase.Invalid);
            }
        }
    }
}
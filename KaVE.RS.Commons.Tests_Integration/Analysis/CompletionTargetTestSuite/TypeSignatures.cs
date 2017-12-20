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

using JetBrains.ReSharper.Psi.Tree;
using KaVE.RS.Commons.Analysis.CompletionTarget;
using NUnit.Framework;

namespace KaVE.RS.Commons.Tests_Integration.Analysis.CompletionTargetTestSuite
{
    internal class TypeSignatures : BaseTest
    {
        public string[] Cases()
        {
            return new[]
            {
                // no trigger: "public class C ${}",
                "public class C$ {}",
                "public class$ C {}",
                "public cla$ss C {}",
                "public$ class C {}",
                "pu$blic class C {}",
                //
                // no trigger: "public class C $: D {}",
                "public class C : $ {}",
                "public class C : object$ {}",
                "public class C : obj$ect {}",
                // no trigger: "public class C : object ${}",
                //
                "public class C$<Test> {}",
                "public class C<$Test> {}",
                "public class C<T$est> {}",
                "public class C<Test$> {}",
                // no trigger: "public class C<Test>$ {}",
                // no trigger: "public class C<Test> ${}",
                //
                "public class C : S<$T> {}",
                "public class C : S<T$> {}",
                //
                // no trigger: "public class C<T> $where T:new() {}",
                "public class C<T> where $T:new() {}",
                "public class C<T> where T:$new() {}"
                // no trigger: "public class C<T> where T:new()$ {}"
            };
        }

        [TestCaseSource(nameof(Cases))]
        public void Test(string sig)
        {
            CompleteInNamespace(sig);
            AssertCompletionMarker<ITypeDeclaration>(CompletionCase.InSignature);
        }

        [Test]
        public void Smoketest_OutOfNamespace()
        {
            CompleteInCSharpFile("class C$ { }");
            AssertCompletionMarker<ITypeDeclaration>(CompletionCase.InSignature);
        }
    }
}
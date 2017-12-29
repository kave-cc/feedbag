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

using System;
using KaVE.RS.Commons.Analysis.CompletionTarget;
using NUnit.Framework;

namespace KaVE.RS.Commons.Tests_Integration.Analysis.CompletionTargetTestSuite.Mixed
{
    internal class OnClosingBrace : BaseTest
    {
        private object[] _cases =
        {
            // if
            new object[] {"if(true) {}$", CompletionCase.EmptyCompletionAfter},
            // if-else
            new object[] {"if(true) {} else {}$", CompletionCase.EmptyCompletionAfter},
            new object[] {"if(true) {}$ else {}", CompletionCase.Undefined},
            // if-else
            new object[] {"if(true) {}$ else {}", CompletionCase.Undefined},
            new object[] {"if(true) {} $ else {}", CompletionCase.Undefined},
            new object[] {"if(true) {} $else {}", CompletionCase.Undefined},
            new object[] {"if(true) {} else {}$", CompletionCase.EmptyCompletionAfter},
            new object[] {"if(true) {} else {} $", CompletionCase.EmptyCompletionAfter},
            // while
            new object[] {"while(true) {}$", CompletionCase.EmptyCompletionAfter},
            // foreach
            new object[] {"foreach(var n in new int[0]) {}$", CompletionCase.EmptyCompletionAfter},
            // for
            new object[] {"for(;;) {}$", CompletionCase.EmptyCompletionAfter},
            // using
            new object[] {"using(this) {}$", CompletionCase.EmptyCompletionAfter},
            // lock
            new object[] {"lock(this) {}$", CompletionCase.EmptyCompletionAfter},
            // try - body
            new object[] {"try {}$ finally {}", CompletionCase.Undefined},
            new object[] {"try {}$ catch {}", CompletionCase.Undefined},
            new object[] {"try {} finally {}$", CompletionCase.EmptyCompletionAfter},
            new object[] {"try {} catch {}$", CompletionCase.EmptyCompletionAfter},
            new object[] {"try {} catch (Exception) {} catch {}$", CompletionCase.EmptyCompletionAfter},
            new object[] {"try {} catch (Exception) {}$ catch {}", CompletionCase.Undefined},
            new object[] {"try {} catch {}$ finally {}", CompletionCase.Undefined},
            new object[] {"try {} catch {} finally {}$", CompletionCase.EmptyCompletionAfter},
            // var init
            new object[] {"var l = new List<int>() {1}$", CompletionCase.EmptyCompletionAfter},
            new object[] {"var d = new Dictionary<int, int> {{1,2}}$", CompletionCase.EmptyCompletionAfter},
            new object[] {"var d = new Dictionary<int, int> {{1,2}$}", CompletionCase.Undefined},
            new object[] {"var l = new List<List<int>> { new List<int> { 1 } }$", CompletionCase.EmptyCompletionAfter},
            new object[] {"var l = new List<List<int>> { new List<int> { 1 }$ }", CompletionCase.Undefined}
        };

        [TestCaseSource(nameof(_cases))]
        public void Test(string body, CompletionCase expectedCase)
        {
            Exec(body);
            AssertCompletionCase(expectedCase);
        }

        [TestCaseSource(nameof(_cases))]
        public void Test_WithStatement(string body, CompletionCase expectedCase)
        {
            Exec(body + "\ncontinue;");
            AssertCompletionCase(expectedCase);
        }

        private void Exec(string body)
        {
            Console.WriteLine(body);
            CompleteInMethod(body);
        }
    }
}
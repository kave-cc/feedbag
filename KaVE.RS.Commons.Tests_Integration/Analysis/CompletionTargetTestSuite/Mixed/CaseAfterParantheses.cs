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
    internal class CaseAfterParantheses : BaseTest
    {
        private object[] _cases =
        {
            // if
            new object[] {"if(true)$ {}", CompletionCase.Undefined},
            new object[] {"if(true) $ {}", CompletionCase.Undefined},
            new object[] {"if(true) ${}", CompletionCase.Undefined},
            new object[] {"if(true)$", CompletionCase.InBody},
            new object[] {"if(true) $", CompletionCase.InBody},
            // while
            new object[] {"while(true)$ {}", CompletionCase.Undefined},
            new object[] {"while(true) $ {}", CompletionCase.Undefined},
            new object[] {"while(true) ${}", CompletionCase.Undefined},
            new object[] {"while(true)$", CompletionCase.InBody},
            new object[] {"while(true) $", CompletionCase.InBody},
            // foreach
            new object[] {"foreach(var n in new int[0])$ {}", CompletionCase.Undefined},
            new object[] {"foreach(var n in new int[0]) $ {}", CompletionCase.Undefined},
            new object[] {"foreach(var n in new int[0]) ${}", CompletionCase.Undefined},
            new object[] {"foreach(var n in new int[0])$", CompletionCase.InBody},
            new object[] {"foreach(var n in new int[0]) $", CompletionCase.InBody},
            // for
            new object[] {"for(;;)$ {}", CompletionCase.Undefined},
            new object[] {"for(;;) $ {}", CompletionCase.Undefined},
            new object[] {"for(;;) ${}", CompletionCase.Undefined},
            new object[] {"for(;;)$", CompletionCase.InBody},
            new object[] {"for(;;) $", CompletionCase.InBody},
            // using
            new object[] {"using(this)$ {}", CompletionCase.Undefined},
            new object[] {"using(this) $ {}", CompletionCase.Undefined},
            new object[] {"using(this) ${}", CompletionCase.Undefined},
            new object[] {"using(this)$", CompletionCase.InBody},
            new object[] {"using(this) $", CompletionCase.InBody},
            // lock
            new object[] {"lock(this)$ {}", CompletionCase.Undefined},
            new object[] {"lock(this) $ {}", CompletionCase.Undefined},
            new object[] {"lock(this) ${}", CompletionCase.Undefined},
            new object[] {"lock(this)$", CompletionCase.InBody},
            new object[] {"lock(this) $", CompletionCase.InBody},
            // specific catch
            new object[] {"try {} catch (Exception)$ {}", CompletionCase.Undefined},
            new object[] {"try {} catch (Exception) $ {}", CompletionCase.Undefined},
            new object[] {"try {} catch (Exception) ${}", CompletionCase.Undefined},
            // object init
            new object[] {"var l = new List<int>()$ {1};", CompletionCase.Undefined},
            new object[] {"var l = new List<int>() $ {1};", CompletionCase.Undefined},
            new object[] {"var l = new List<int>() ${1};", CompletionCase.Undefined}
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
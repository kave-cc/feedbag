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
using System.Collections.Generic;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using KaVE.Commons.Model.Naming;
using KaVE.Commons.Model.SSTs.Blocks;
using KaVE.Commons.Model.SSTs.Impl.Blocks;
using KaVE.RS.Commons.Analysis.CompletionTarget;
using NUnit.Framework;
using Fix = KaVE.RS.Commons.Tests_Integration.Analysis.SSTAnalysisTestSuite.SSTAnalysisFixture;

namespace KaVE.RS.Commons.Tests_Integration.Analysis.CompletionTargetTestSuite.Blocks
{
    internal class TryCatchBlock : BaseTest
    {
        private static IEnumerable<object[]> GetUndefinedCases()
        {
            yield return new object[]
            {
                @"t$ry { }
                  finally { }",
                typeof(ITryStatement),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                @"try$ { }
                  finally { }",
                typeof(ITryStatement),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                @"try $ { }
                  finally { }",
                typeof(ITryStatement),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                @"try ${ }
                  finally { }",
                typeof(ITryStatement),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                @"try {$ }
                  finally { }",
                typeof(ITryStatement),
                CompletionCase.InBody
            };

            yield return new object[]
            {
                @"try { $ }
                  finally { }",
                typeof(ITryStatement),
                CompletionCase.InBody
            };

            yield return new object[]
            {
                @"try { $}
                  finally { }",
                typeof(ITryStatement),
                CompletionCase.InBody
            };

            yield return new object[]
            {
                @"try { }$
                  finally { }",
                typeof(ITryStatement),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                @"try { } $
                  finally { }",
                typeof(ITryStatement),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                @"try { }
                  $finally { }",
                typeof(ITryStatement),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                @"try { }
                  fi$nally { }",
                typeof(ITryStatement),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                @"try { }
                  finally$ { }",
                typeof(ITryStatement),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                @"try { }
                  finally $ { }",
                typeof(ITryStatement),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                @"try { }
                  finally ${ }",
                typeof(ITryStatement),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                @"try { }
                  finally {$ }",
                typeof(ITryStatement),
                CompletionCase.InFinally
            };

            yield return new object[]
            {
                @"try { }
                  finally { $ }",
                typeof(ITryStatement),
                CompletionCase.InFinally
            };

            yield return new object[]
            {
                @"try { }
                  finally { $}",
                typeof(ITryStatement),
                CompletionCase.InFinally
            };

            yield return new object[]
            {
                @"try { }
                  finally { }$",
                typeof(ITryStatement),
                CompletionCase.EmptyCompletionAfter
            };

            yield return new object[]
            {
                @"try { }
                  finally { } $",
                typeof(ITryStatement),
                CompletionCase.EmptyCompletionAfter
            };

            yield return new object[]
            {
                TryFinally("$ catch (Exception) { }"),
                typeof(ISpecificCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                TryFinally("$catch (Exception) { }"),
                typeof(ISpecificCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                TryFinally("c$atch (Exception) { }"),
                typeof(ISpecificCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                TryFinally("catch$ (Exception) { }"),
                typeof(ISpecificCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                TryFinally("catch $ (Exception) { }"),
                typeof(ISpecificCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                TryFinally("catch $(Exception) { }"),
                typeof(ISpecificCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                TryFinally("catch ($Exception) { }"),
                typeof(ISpecificCatchClause),
                CompletionCase.InSignature
            };

            yield return new object[]
            {
                TryFinally("catch (Exception$) { }"),
                typeof(ISpecificCatchClause),
                CompletionCase.InSignature
            };

            yield return new object[]
            {
                TryFinally("catch (Exception)$ { }"),
                typeof(ISpecificCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                TryFinally("catch (Exception) $ { }"),
                typeof(ISpecificCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                TryFinally("catch (Exception) ${ }"),
                typeof(ISpecificCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                TryFinally("catch (Exception) {$ }"),
                typeof(ISpecificCatchClause),
                CompletionCase.InBody
            };

            yield return new object[]
            {
                TryFinally("catch (Exception) { $ }"),
                typeof(ISpecificCatchClause),
                CompletionCase.InBody
            };

            yield return new object[]
            {
                TryFinally("catch (Exception) { $}"),
                typeof(ISpecificCatchClause),
                CompletionCase.InBody
            };

            yield return new object[]
            {
                TryFinally("catch (Exception) { }$"),
                typeof(ISpecificCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                TryFinally("catch (Exception) { } $"),
                typeof(ISpecificCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                OnlyTry("catch (Exception) { }$"),
                typeof(ITryStatement),
                CompletionCase.EmptyCompletionAfter
            };

            yield return new object[]
            {
                OnlyTry("catch (Exception) { } $"),
                typeof(ITryStatement),
                CompletionCase.EmptyCompletionAfter
            };

            yield return new object[]
            {
                TryFinally("$ catch (Exception e) { }"),
                typeof(ISpecificCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                TryFinally("$catch (Exception e) { }"),
                typeof(ISpecificCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                TryFinally("ca$tch (Exception e) { }"),
                typeof(ISpecificCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                TryFinally("catch$ (Exception e) { }"),
                typeof(ISpecificCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                TryFinally("catch $ (Exception e) { }"),
                typeof(ISpecificCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                TryFinally("catch $(Exception e) { }"),
                typeof(ISpecificCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                TryFinally("catch ($Exception e) { }"),
                typeof(ISpecificCatchClause),
                CompletionCase.InSignature
            };

            yield return new object[]
            {
                TryFinally("catch (Exception$ e) { }"),
                typeof(ISpecificCatchClause),
                CompletionCase.InSignature
            };

            yield return new object[]
            {
                TryFinally("catch (Exception $ e) { }"),
                typeof(ISpecificCatchClause),
                CompletionCase.InSignature
            };

            yield return new object[]
            {
                TryFinally("catch (Exception $e) { }"),
                typeof(ISpecificCatchClause),
                CompletionCase.InSignature
            };

            yield return new object[]
            {
                TryFinally("catch (Exception e$) { }"),
                typeof(ISpecificCatchClause),
                CompletionCase.InSignature
            };

            yield return new object[]
            {
                TryFinally("catch (Exception e)$ { }"),
                typeof(ISpecificCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                TryFinally("catch (Exception e) $ { }"),
                typeof(ISpecificCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                TryFinally("catch (Exception e) ${ }"),
                typeof(ISpecificCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                TryFinally("catch (Exception e) {$ }"),
                typeof(ISpecificCatchClause),
                CompletionCase.InBody
            };

            yield return new object[]
            {
                TryFinally("catch (Exception e) { $ }"),
                typeof(ISpecificCatchClause),
                CompletionCase.InBody
            };

            yield return new object[]
            {
                TryFinally("catch (Exception e) { $}"),
                typeof(ISpecificCatchClause),
                CompletionCase.InBody
            };

            yield return new object[]
            {
                TryFinally("catch (Exception e) { }$"),
                typeof(ISpecificCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                TryFinally("catch (Exception e) { } $"),
                typeof(ISpecificCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                OnlyTry("catch (Exception e) { }$"),
                typeof(ITryStatement),
                CompletionCase.EmptyCompletionAfter
            };

            yield return new object[]
            {
                OnlyTry("catch (Exception e) { } $"),
                typeof(ITryStatement),
                CompletionCase.EmptyCompletionAfter
            };

            yield return new object[]
            {
                TryFinally("$ catch { }"),
                typeof(IGeneralCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                TryFinally("$catch { }"),
                typeof(IGeneralCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                TryFinally("ca$tch { }"),
                typeof(IGeneralCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                TryFinally("catch$ { }"),
                typeof(IGeneralCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                TryFinally("catch $ { }"),
                typeof(IGeneralCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                TryFinally("catch ${ }"),
                typeof(IGeneralCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                TryFinally("catch {$ }"),
                typeof(IGeneralCatchClause),
                CompletionCase.InBody
            };

            yield return new object[]
            {
                TryFinally("catch { $ }"),
                typeof(IGeneralCatchClause),
                CompletionCase.InBody
            };

            yield return new object[]
            {
                TryFinally("catch { $}"),
                typeof(IGeneralCatchClause),
                CompletionCase.InBody
            };

            yield return new object[]
            {
                TryFinally("catch { }$"),
                typeof(IGeneralCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                TryFinally("catch { } $"),
                typeof(IGeneralCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                OnlyTry("catch { }$"),
                typeof(ITryStatement),
                CompletionCase.EmptyCompletionAfter
            };

            yield return new object[]
            {
                OnlyTry("catch { } $"),
                typeof(ITryStatement),
                CompletionCase.EmptyCompletionAfter
            };

            yield return new object[]
            {
                OnlyTry("catch { } $ catch (Exception e) {}"),
                typeof(IGeneralCatchClause),
                CompletionCase.Undefined
            };

            yield return new object[]
            {
                OnlyTry("catch (Exception e) {} $ catch { }"),
                typeof(ISpecificCatchClause),
                CompletionCase.Undefined
            };
        }

        private static string TryFinally(string line)
        {
            return @"try { }
                 " + line + @"
                     finally { }";
        }

        private static string OnlyTry(string line)
        {
            return @"try { }
                 " + line;
        }

        [TestCaseSource(nameof(GetUndefinedCases))]
        public void UndefinedCases(string block, Type expectedType, CompletionCase expectedCase)
        {
            Console.WriteLine(block);
            CompleteInMethod(block);
            AssertCompletionMarker(expectedType, expectedCase);
        }

        [Test]
        public void InTry_Body()
        {
            CompleteInMethod(
                @"
                try
                {
                    $
                }
                finally { }
            ");

            AssertCompletionMarker<ITryStatement>(CompletionCase.InBody);

            AssertBody(
                "M",
                new TryBlock
                {
                    Body = {Fix.EmptyCompletion}
                }
            );
        }

        [Test]
        public void InTry_GeneralCatch()
        {
            CompleteInMethod(
                @"
                try {}
                catch
                {
                    $
                }
            ");

            AssertCompletionMarker<IGeneralCatchClause>(CompletionCase.InBody);

            AssertBody(
                "M",
                new TryBlock
                {
                    CatchBlocks =
                    {
                        new CatchBlock
                        {
                            Kind = CatchBlockKind.General,
                            Body = {Fix.EmptyCompletion}
                        }
                    }
                }
            );
        }

        [Test]
        public void InTry_SpecificCatch()
        {
            CompleteInMethod(
                @"
                try {}
                catch (Exception e)
                {
                    $
                }
            ");

            AssertCompletionMarker<ISpecificCatchClause>(CompletionCase.InBody);

            AssertBody(
                "M",
                new TryBlock
                {
                    CatchBlocks =
                    {
                        new CatchBlock
                        {
                            Kind = CatchBlockKind.Default,
                            Parameter = Names.Parameter("[{0}] e", Fix.Exception),
                            Body = {Fix.EmptyCompletion}
                        }
                    }
                }
            );
        }

        [Test]
        public void InTry_Finally()
        {
            CompleteInMethod(
                @"
                try {}
                finally
                {
                    $
                }
            ");

            AssertCompletionMarker<ITryStatement>(CompletionCase.InFinally);

            AssertBody(
                "M",
                new TryBlock
                {
                    Finally = {Fix.EmptyCompletion}
                }
            );
        }
    }
}
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

using System.Collections.Generic;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using KaVE.Commons.Model.Naming;
using KaVE.Commons.Model.Naming.CodeElements;
using KaVE.Commons.Model.SSTs;
using KaVE.Commons.Model.SSTs.Blocks;
using KaVE.Commons.Model.SSTs.Impl.Blocks;
using KaVE.Commons.Model.SSTs.Impl.Expressions.Assignable;
using KaVE.Commons.Model.SSTs.Impl.Expressions.Simple;
using KaVE.Commons.Model.SSTs.Impl.References;
using KaVE.Commons.Model.SSTs.Impl.Statements;
using KaVE.Commons.Model.SSTs.References;
using KaVE.Commons.Utils.Collections;
using KaVE.Commons.Utils.Exceptions;
using KaVE.RS.Commons.Analysis.CompletionTarget;
using KaVE.RS.Commons.Utils.Naming;

namespace KaVE.RS.Commons.Analysis.Transformer.StatementVisitorParts
{
    public partial class StatementVisitor : TreeNodeVisitor<IList<IStatement>>
    {
        public override void VisitIfStatement(IIfStatement stmt, IList<IStatement> body)
        {
            if (IsTargetMatch(stmt, CompletionCase.EmptyCompletionBefore))
            {
                body.Add(EmptyCompletionExpression);
            }
            var ifElseBlock = new IfElseBlock
            {
                Condition = _exprVisitor.ToSimpleExpression(stmt.Condition, body) ?? new UnknownExpression()
            };
            if (IsTargetMatch(stmt, CompletionCase.InBody))
            {
                ifElseBlock.Then.Add(EmptyCompletionExpression);
            }
            if (IsTargetMatch(stmt, CompletionCase.InElse))
            {
                ifElseBlock.Else.Add(EmptyCompletionExpression);
            }
            if (stmt.Then != null)
            {
                stmt.Then.Accept(this, ifElseBlock.Then);
            }
            if (stmt.Else != null)
            {
                stmt.Else.Accept(this, ifElseBlock.Else);
            }

            body.Add(ifElseBlock);

            if (IsTargetMatch(stmt, CompletionCase.EmptyCompletionAfter))
            {
                body.Add(EmptyCompletionExpression);
            }
        }

        public override void VisitDoStatement(IDoStatement stmt, IList<IStatement> body)
        {
            if (IsTargetMatch(stmt, CompletionCase.EmptyCompletionBefore))
            {
                body.Add(EmptyCompletionExpression);
            }
            var loop = new DoLoop
            {
                Condition = _exprVisitor.ToLoopHeaderExpression(stmt.Condition, body)
            };
            body.Add(loop);

            if (IsTargetMatch(stmt, CompletionCase.InBody))
            {
                loop.Body.Add(EmptyCompletionExpression);
            }

            stmt.Body.Accept(this, loop.Body);


            if (IsTargetMatch(stmt, CompletionCase.EmptyCompletionAfter))
            {
                body.Add(EmptyCompletionExpression);
            }
        }

        public override void VisitLockStatement(ILockStatement stmt, IList<IStatement> body)
        {
            if (IsTargetMatch(stmt, CompletionCase.EmptyCompletionBefore))
            {
                body.Add(EmptyCompletionExpression);
            }
            var block = new LockBlock
            {
                Reference = _exprVisitor.ToVariableRef(stmt.Monitor, body)
            };
            body.Add(block);

            if (IsTargetMatch(stmt, CompletionCase.InBody))
            {
                block.Body.Add(EmptyCompletionExpression);
            }

            stmt.Body.Accept(this, block.Body);


            if (IsTargetMatch(stmt, CompletionCase.EmptyCompletionAfter))
            {
                body.Add(EmptyCompletionExpression);
            }
        }

        public override void VisitUnsafeCodeUnsafeStatement(IUnsafeCodeUnsafeStatement stmt, IList<IStatement> body)
        {
            if (IsTargetMatch(stmt, CompletionCase.EmptyCompletionBefore))
            {
                body.Add(EmptyCompletionExpression);
            }

            body.Add(new UnsafeBlock());

            if (IsTargetMatch(stmt, CompletionCase.EmptyCompletionAfter))
            {
                body.Add(EmptyCompletionExpression);
            }
        }

        public override void VisitWhileStatement(IWhileStatement rsLoop, IList<IStatement> body)
        {
            if (_marker.HandlingNode == rsLoop && _marker.Case == CompletionCase.EmptyCompletionBefore)
            {
                body.Add(EmptyCompletionExpression);
            }

            var loop = new WhileLoop
            {
                Condition = _exprVisitor.ToLoopHeaderExpression(rsLoop.Condition, body)
            };

            body.Add(loop);

            if (_marker.HandlingNode == rsLoop && _marker.Case == CompletionCase.InBody)
            {
                loop.Body.Add(EmptyCompletionExpression);
            }

            rsLoop.Body.Accept(this, loop.Body);

            if (_marker.HandlingNode == rsLoop && _marker.Case == CompletionCase.EmptyCompletionAfter)
            {
                body.Add(EmptyCompletionExpression);
            }
        }

        public override void VisitForStatement(IForStatement stmt, IList<IStatement> body)
        {
            if (IsTargetMatch(stmt, CompletionCase.EmptyCompletionBefore))
            {
                body.Add(EmptyCompletionExpression);
            }

            var forLoop = new ForLoop();
            body.Add(forLoop);

            if (IsTargetMatch(stmt, CompletionCase.InBody))
            {
                forLoop.Body.Add(EmptyCompletionExpression);
            }

            VisitForStatement_Init(stmt.Initializer, forLoop.Init, body);
            forLoop.Condition = _exprVisitor.ToLoopHeaderExpression(stmt.Condition, body);
            foreach (var expr in stmt.IteratorExpressionsEnumerable)
            {
                expr.Accept(this, forLoop.Step);
            }

            if (stmt.Body != null)
            {
                stmt.Body.Accept(this, forLoop.Body);
            }

            if (IsTargetMatch(stmt, CompletionCase.EmptyCompletionAfter))
            {
                body.Add(EmptyCompletionExpression);
            }
        }

        private void VisitForStatement_Init(IForInitializer init, IKaVEList<IStatement> forInit, IList<IStatement> body)
        {
            if (init == null)
            {
                return;
            }

            // case 1: single declaration
            var isDeclaration = init.Declaration != null;
            if (isDeclaration)
            {
                var decl = init.Declaration.Declarators[0];
                decl.Accept(this, forInit);
            }

            // case 2: multiple statements
            var hasStatements = init.Expressions.Count > 0;
            if (hasStatements)
            {
                foreach (var expr in init.ExpressionsEnumerable)
                {
                    expr.Accept(this, forInit);
                }
            }
        }

        public override void VisitForeachStatement(IForeachStatement stmt, IList<IStatement> body)
        {
            if (IsTargetMatch(stmt, CompletionCase.EmptyCompletionBefore))
            {
                body.Add(EmptyCompletionExpression);
            }

            var loop = new ForEachLoop
            {
                LoopedReference = _exprVisitor.ToVariableRef(stmt.Collection, body)
            };
            body.Add(loop);

            foreach (var itDecl in stmt.IteratorDeclarations)
            {
                var localVar = itDecl.DeclaredElement.GetName<ILocalVariableName>();
                loop.Declaration = new VariableDeclaration
                {
                    Reference = new VariableReference {Identifier = localVar.Name},
                    Type = localVar.ValueType
                };
            }

            if (IsTargetMatch(stmt, CompletionCase.InBody))
            {
                loop.Body.Add(EmptyCompletionExpression);
            }

            if (stmt.Body != null)
            {
                stmt.Body.Accept(this, loop.Body);
            }


            if (IsTargetMatch(stmt, CompletionCase.EmptyCompletionAfter))
            {
                body.Add(EmptyCompletionExpression);
            }
        }

        public override void VisitTryStatement(ITryStatement block, IList<IStatement> body)
        {
            AddIf(block, CompletionCase.EmptyCompletionBefore, body);

            var tryBlock = new TryBlock();
            body.Add(tryBlock);

            AddIf(block, CompletionCase.InBody, tryBlock.Body);
            AddIf(block, CompletionCase.InFinally, tryBlock.Finally);
            VisitBlock(block.Try, tryBlock.Body);
            VisitBlock(block.FinallyBlock, tryBlock.Finally);

            foreach (var clause in block.Catches)
            {
                var catchBlock = new CatchBlock();
                tryBlock.CatchBlocks.Add(catchBlock);

                AddIf(clause, CompletionCase.InBody, catchBlock.Body);

                VisitBlock(clause.Body, catchBlock.Body);

                var generalClause = clause as IGeneralCatchClause;
                if (generalClause != null)
                {
                    catchBlock.Kind = CatchBlockKind.General;
                    continue;
                }

                var specificClause = clause as ISpecificCatchClause;
                if (specificClause != null)
                {
                    var varDecl = specificClause.ExceptionDeclaration;
                    var isUnnamed = varDecl == null;

                    var typeName = specificClause.ExceptionType.GetName();
                    var varName = isUnnamed ? "?" : varDecl.DeclaredName;
                    catchBlock.Parameter = Names.Parameter("[{0}] {1}", typeName, varName);
                    catchBlock.Kind = isUnnamed ? CatchBlockKind.Unnamed : CatchBlockKind.Default;
                }
            }

            AddIf(block, CompletionCase.EmptyCompletionAfter, body);
        }

        private void AddIf(ICSharpTreeNode node, CompletionCase completionCase, IList<IStatement> body)
        {
            if (IsTargetMatch(node, completionCase))
            {
                body.Add(EmptyCompletionExpression);
            }
        }

        public override void VisitUsingStatement(IUsingStatement block, IList<IStatement> body)
        {
            AddIf(block, CompletionCase.EmptyCompletionBefore, body);

            var usingBlock = new UsingBlock();

            IVariableReference varRef = new VariableReference();

            // case 1: variable declarations
            if (block.VariableDeclarations.Any())
            {
                var decl = block.VariableDeclarations[0];
                decl.Accept(this, body);
                varRef = new VariableReference {Identifier = decl.DeclaredName};
            }
            // case 2: expressions (var refs, method calls ...)
            else if (block.Expressions.Any())
            {
                var expr = block.Expressions[0];
                varRef = _exprVisitor.ToVariableRef(expr, body);
            }

            usingBlock.Reference = varRef;

            var bodyAsIBlock = block.Body as IBlock;
            if (bodyAsIBlock != null && !bodyAsIBlock.Statements.Any() && IsTargetMatch(block, CompletionCase.InBody))
            {
                usingBlock.Body.Add(new ExpressionStatement {Expression = new CompletionExpression()});
            }
            else
            {
                block.Body.Accept(this, usingBlock.Body);
            }

            body.Add(usingBlock);

            AddIf(block, CompletionCase.EmptyCompletionAfter, body);
        }

        public override void VisitSwitchStatement(ISwitchStatement block, IList<IStatement> body)
        {
            AddIf(block, CompletionCase.EmptyCompletionBefore, body);

            var switchBlock = new SwitchBlock {Reference = _exprVisitor.ToVariableRef(block.Condition, body)};

            foreach (var section in block.Sections)
            {
                IKaVEList<IStatement> currentSection = null;

                foreach (var label in section.CaseLabels)
                {
                    currentSection = new KaVEList<IStatement>();
                    if (label.IsDefault)
                    {
                        switchBlock.DefaultSection = currentSection;
                    }
                    else
                    {
                        switchBlock.Sections.Add(
                            new CaseBlock
                            {
                                Label = _exprVisitor.ToSimpleExpression(label.ValueExpression, body),
                                Body = currentSection
                            });
                    }
                    AddIf(label, CompletionCase.InBody, currentSection);
                }

                AddIf(section, CompletionCase.InBody, currentSection);
                foreach (var statement in section.Statements)
                {
                    statement.Accept(this, currentSection);
                }
            }

            body.Add(switchBlock);

            AddIf(block, CompletionCase.EmptyCompletionAfter, body);
        }

        public override void VisitUncheckedStatement(IUncheckedStatement block, IList<IStatement> body)
        {
            AddIf(block, CompletionCase.EmptyCompletionBefore, body);

            var uncheckedBlock = new UncheckedBlock();
            AddIf(block, CompletionCase.InBody, uncheckedBlock.Body);
            block.Body.Accept(this, uncheckedBlock.Body);
            body.Add(uncheckedBlock);

            AddIf(block, CompletionCase.EmptyCompletionAfter, body);
        }

        public override void VisitBlock(IBlock block, IList<IStatement> body)
        {
            // TODO NameUpdate: changed another helper to overriding this method, check if Null check is really necessary now
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (block == null)
            {
                return;
            }
            // TODO NameUpdate: untested addition
            AddIf(block, CompletionCase.EmptyCompletionBefore, body);
            // TODO NameUpdate: untested addition
            AddIf(block, CompletionCase.InBody, body);

            foreach (var stmt in block.Statements)
            {
                Execute.AndSupressExceptions(
                    delegate { stmt.Accept(this, body); });
            }
            // TODO NameUpdate: untested addition
            AddIf(block, CompletionCase.EmptyCompletionAfter, body);
        }
    }
}
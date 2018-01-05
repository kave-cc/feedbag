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

using System;
using System.Collections.Generic;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;
using KaVE.Commons.Model.Naming;
using KaVE.Commons.Model.Naming.Types;
using KaVE.Commons.Model.SSTs.Expressions;
using KaVE.Commons.Model.SSTs.Impl;
using KaVE.Commons.Model.SSTs.Impl.Expressions.Assignable;
using KaVE.Commons.Model.SSTs.Impl.Expressions.Simple;
using KaVE.Commons.Model.SSTs.Impl.References;
using KaVE.Commons.Model.SSTs.Impl.Statements;
using KaVE.Commons.Model.SSTs.References;
using KaVE.Commons.Model.SSTs.Statements;
using KaVE.Commons.Utils.Assertion;
using KaVE.RS.Commons.Analysis.CompletionTarget;
using KaVE.RS.Commons.Analysis.Util;
using KaVE.RS.Commons.Utils.Naming;
using IBreakStatement = JetBrains.ReSharper.Psi.CSharp.Tree.IBreakStatement;
using IContinueStatement = JetBrains.ReSharper.Psi.CSharp.Tree.IContinueStatement;
using IExpressionStatement = JetBrains.ReSharper.Psi.CSharp.Tree.IExpressionStatement;
using IReturnStatement = JetBrains.ReSharper.Psi.CSharp.Tree.IReturnStatement;
using IStatement = KaVE.Commons.Model.SSTs.IStatement;
using IThrowStatement = JetBrains.ReSharper.Psi.CSharp.Tree.IThrowStatement;

namespace KaVE.RS.Commons.Analysis.Transformer.StatementVisitorParts
{
    public partial class StatementVisitor : TreeNodeVisitor<IList<IStatement>>
    {
        private readonly CompletionTargetMarker _marker;
        private readonly ExpressionVisitor _exprVisitor;
        private readonly UniqueVariableNameGenerator _nameGen;

        private static ExpressionStatement EmptyCompletionExpression
        {
            get { return new ExpressionStatement {Expression = new CompletionExpression()}; }
        }

        public StatementVisitor(UniqueVariableNameGenerator nameGen, CompletionTargetMarker marker)
        {
            _marker = marker;
            _nameGen = nameGen;
            _exprVisitor = new ExpressionVisitor(_nameGen, marker);
        }

        public override void VisitNode(ITreeNode node, IList<IStatement> context)
        {
            node.Children<ICSharpTreeNode>().ForEach(
                child =>
                {
                    try
                    {
                        child.Accept(this, context);
                    }
                    catch (NullReferenceException) { }
                    catch (AssertException) { }
                });
        }

        #region statements

        public override void VisitBreakStatement(IBreakStatement stmt, IList<IStatement> body)
        {
            AddIf(stmt, CompletionCase.EmptyCompletionBefore, body);

            body.Add(new BreakStatement());

            AddIf(stmt, CompletionCase.EmptyCompletionAfter, body);
        }

        public override void VisitLocalVariableDeclaration(ILocalVariableDeclaration decl, IList<IStatement> body)
        {
            if (IsTargetMatch(decl, CompletionCase.EmptyCompletionBefore))
            {
                body.Add(EmptyCompletionExpression);
            }

            var id = decl.DeclaredName;
            ITypeName type;
            try
            {
                type = decl.Type.GetName();
            }
            catch (AssertException)
            {
                // TODO this is an intermediate "fix"... the analysis sometimes fails here ("cannot create name for anonymous type")
                type = Names.UnknownType;
            }
            body.Add(SSTUtil.Declare(id, type));

            IAssignableExpression initializer = null;
            if (decl.Initial != null)
            {
                initializer = _exprVisitor.ToAssignableExpr(decl.Initial, body);
            }
            else if (_marker.HandlingNode == decl && _marker.Case == CompletionCase.InBody)
            {
                initializer = new CompletionExpression();
            }

            if (initializer != null)
            {
                if (!IsSelfAssign(id, initializer))
                {
                    body.Add(SSTUtil.AssignmentToLocal(id, initializer));
                }
            }

            if (decl == _marker.HandlingNode && _marker.Case == CompletionCase.EmptyCompletionAfter)
            {
                body.Add(EmptyCompletionExpression);
            }
        }

        public override void VisitDeclarationStatement(IDeclarationStatement decl, IList<IStatement> body)
        {
            if (IsTargetMatch(decl, CompletionCase.EmptyCompletionBefore))
            {
                body.Add(EmptyCompletionExpression);
            }

            VisitNode(decl.Declaration, body);

            if (decl == _marker.HandlingNode && _marker.Case == CompletionCase.EmptyCompletionAfter)
            {
                body.Add(EmptyCompletionExpression);
            }
        }

        public override void VisitLocalConstantDeclaration(ILocalConstantDeclaration decl, IList<IStatement> body)
        {
            if (IsTargetMatch(decl, CompletionCase.EmptyCompletionBefore))
            {
                body.Add(EmptyCompletionExpression);
            }

            var id = decl.DeclaredName;
            ITypeName type;
            try
            {
                type = decl.Type.GetName();
            }
            catch (AssertException)
            {
                // TODO this is an intermediate "fix"... the analysis sometimes fails here ("cannot create name for anonymous type")
                type = Names.UnknownType;
            }
            body.Add(SSTUtil.Declare(id, type));

            IAssignableExpression initializer = null;
            if (decl.ValueExpression != null)
            {
                initializer = _exprVisitor.ToAssignableExpr(decl.ValueExpression, body);
            }
            else if (_marker.HandlingNode == decl && _marker.Case == CompletionCase.Undefined)
            {
                initializer = new CompletionExpression();
            }

            if (initializer != null)
            {
                if (!IsSelfAssign(id, initializer))
                {
                    body.Add(SSTUtil.AssignmentToLocal(id, initializer));
                }
            }

            if (decl == _marker.HandlingNode && _marker.Case == CompletionCase.EmptyCompletionAfter)
            {
                body.Add(EmptyCompletionExpression);
            }
        }

        private bool IsTargetMatch(ICSharpTreeNode o, CompletionCase completionCase)
        {
            var isValid = _marker.HandlingNode != null;
            var isMatch = o == _marker.HandlingNode;
            var isRightCase = _marker.Case == completionCase;
            return isValid && isMatch && isRightCase;
        }

        public override void VisitAssignmentExpression(IAssignmentExpression expr, IList<IStatement> body)
        {
            if (IsTargetMatch(expr, CompletionCase.EmptyCompletionBefore))
            {
                body.Add(EmptyCompletionExpression);
            }

            var isTarget = IsTargetMatch(expr, CompletionCase.InBody);

            var sstRef = _exprVisitor.ToAssignableRef(expr.Dest, body) ?? new UnknownReference();

            var sstExpr =
                isTarget
                    ? new CompletionExpression()
                    : _exprVisitor.ToAssignableExpr(expr.Source, body);

            var operation = TryGetEventSubscriptionOperation(expr);
            if (operation.HasValue)
            {
                body.Add(
                    new EventSubscriptionStatement
                    {
                        Reference = sstRef,
                        Operation = operation.Value,
                        Expression = sstExpr
                    });
            }
            else
            {
                if (!IsSelfAssign(sstRef, sstExpr))
                {
                    body.Add(
                        new Assignment
                        {
                            Reference = sstRef,
                            Expression = IsFancyAssign(expr) ? new ComposedExpression() : sstExpr
                        });
                }
            }

            if (IsTargetMatch(expr, CompletionCase.EmptyCompletionAfter))
            {
                body.Add(EmptyCompletionExpression);
            }
        }

        private static bool IsSelfAssign(string id, IAssignableExpression sstExpr)
        {
            return IsSelfAssign(new VariableReference {Identifier = id}, sstExpr);
        }

        private static bool IsSelfAssign(IAssignableReference sstRef, IAssignableExpression sstExpr)
        {
            // TODO add test!
            var refExpr = sstExpr as KaVE.Commons.Model.SSTs.Expressions.Simple.IReferenceExpression;
            return refExpr != null && sstRef.Equals(refExpr.Reference);
        }

        private static bool IsFancyAssign(IAssignmentExpression expr)
        {
            return expr.AssignmentType != AssignmentType.EQ;
        }

        private EventSubscriptionOperation? TryGetEventSubscriptionOperation(IAssignmentExpression expr)
        {
            var isRegularEq = expr.AssignmentType == AssignmentType.EQ;
            if (isRegularEq)
            {
                return null;
            }

            var refExpr = expr.Dest as IReferenceExpression;
            if (refExpr == null)
            {
                return null;
            }

            var elem = refExpr.Reference.Resolve().DeclaredElement;
            if (elem == null)
            {
                return null;
            }

            var loc = elem as ITypeOwner;
            if (loc != null)
            {
                var type = loc.Type.GetName();
                if (!type.IsDelegateType)
                {
                    return null;
                }
            }

            var isAdd = expr.AssignmentType == AssignmentType.PLUSEQ;
            if (isAdd)
            {
                return EventSubscriptionOperation.Add;
            }

            var isRemove = expr.AssignmentType == AssignmentType.MINUSEQ;
            if (isRemove)
            {
                return EventSubscriptionOperation.Remove;
            }

            return null;
        }

        /*private static AssignmentOperation ToOperation(AssignmentType assignmentType)
        {
            switch (assignmentType)
            {
                case AssignmentType.EQ:
                    return AssignmentOperation.Equals;
                case AssignmentType.PLUSEQ:
                    return AssignmentOperation.Add;
                case AssignmentType.MINUSEQ:
                    return AssignmentOperation.Remove;
                default:
                    return AssignmentOperation.Unknown;
            }
        }*/

        public override void VisitExpressionStatement(IExpressionStatement stmt, IList<IStatement> body)
        {
            if (stmt.Expression != null)
            {
                var assignment = stmt.Expression as IAssignmentExpression;
                var prefix = stmt.Expression as IPrefixOperatorExpression;
                var postfix = stmt.Expression as IPostfixOperatorExpression;
                if (assignment != null)
                {
                    assignment.Accept(this, body);
                }
                else if (prefix != null)
                {
                    prefix.Accept(this, body);
                }
                else if (postfix != null)
                {
                    postfix.Accept(this, body);
                }
                else
                {
                    body.Add(
                        new ExpressionStatement
                        {
                            Expression = stmt.Expression.Accept(_exprVisitor, body) ?? new UnknownExpression()
                        });

                    if (IsTargetMatch(stmt.Expression, CompletionCase.EmptyCompletionAfter))
                    {
                        body.Add(
                            new ExpressionStatement
                            {
                                Expression = new CompletionExpression()
                            });
                    }
                }
            }
        }

        public override void VisitPrefixOperatorExpression(IPrefixOperatorExpression expr, IList<IStatement> body)
        {
            if (IsTargetMatch(expr, CompletionCase.EmptyCompletionBefore))
            {
                body.Add(EmptyCompletionExpression);
            }
            var varRef = _exprVisitor.ToVariableRef(expr.Operand, body);
            if (varRef.IsMissing)
            {
                body.Add(new UnknownStatement());
            }
            else
            {
                body.Add(
                    new Assignment
                    {
                        Reference = varRef,
                        Expression = new ComposedExpression
                        {
                            References = {varRef}
                        }
                    });
            }
            if (IsTargetMatch(expr, CompletionCase.EmptyCompletionAfter))
            {
                body.Add(EmptyCompletionExpression);
            }
        }

        public override void VisitPostfixOperatorExpression(IPostfixOperatorExpression expr, IList<IStatement> body)
        {
            if (IsTargetMatch(expr, CompletionCase.EmptyCompletionBefore))
            {
                body.Add(EmptyCompletionExpression);
            }
            var varRef = _exprVisitor.ToVariableRef(expr.Operand, body);
            if (varRef.IsMissing)
            {
                body.Add(new UnknownStatement());
            }
            else
            {
                body.Add(
                    new Assignment
                    {
                        Reference = varRef,
                        Expression = new ComposedExpression
                        {
                            References = {varRef}
                        }
                    });
            }
            if (IsTargetMatch(expr, CompletionCase.EmptyCompletionAfter))
            {
                body.Add(EmptyCompletionExpression);
            }
        }

        public override void VisitReturnStatement(IReturnStatement stmt, IList<IStatement> body)
        {
            if (IsTargetMatch(stmt, CompletionCase.EmptyCompletionBefore))
            {
                body.Add(EmptyCompletionExpression);
            }

            if (stmt.Value == null)
            {
                body.Add(
                    new ReturnStatement
                    {
                        IsVoid = true
                    });
            }
            else
            {
                body.Add(
                    new ReturnStatement
                    {
                        Expression = _exprVisitor.ToSimpleExpression(stmt.Value, body) ?? new UnknownExpression()
                    });
            }

            if (IsTargetMatch(stmt, CompletionCase.EmptyCompletionAfter))
            {
                body.Add(EmptyCompletionExpression);
            }
        }

        public override void VisitThrowStatement(IThrowStatement stmt, IList<IStatement> body)
        {
            if (IsTargetMatch(stmt, CompletionCase.EmptyCompletionBefore))
            {
                body.Add(EmptyCompletionExpression);
            }

            IVariableReference varRef = new VariableReference();

            if (stmt.Semicolon == null && IsTargetMatch(stmt, CompletionCase.EmptyCompletionAfter))
            {
                varRef = new VariableReference {Identifier = _nameGen.GetNextVariableName()};
                body.Add(
                    new VariableDeclaration
                    {
                        Type = Names.Type("System.Exception, mscorlib, 4.0.0.0"),
                        Reference = varRef
                    });
                body.Add(new Assignment {Reference = varRef, Expression = new CompletionExpression()});
            }
            else if (stmt.Exception != null)
            {
                varRef = _exprVisitor.ToVariableRef(stmt.Exception, body);
            }

            body.Add(new ThrowStatement {Reference = varRef});

            if (IsTargetMatch(stmt, CompletionCase.EmptyCompletionAfter))
            {
                body.Add(EmptyCompletionExpression);
            }
        }

        public override void VisitEmptyStatement(IEmptyStatement stmt, IList<IStatement> body)
        {
            if (stmt == _marker.HandlingNode)
            {
                body.Add(EmptyCompletionExpression);
            }
        }

        public override void VisitContinueStatement(IContinueStatement stmt, IList<IStatement> body)
        {
            if (IsTargetMatch(stmt, CompletionCase.EmptyCompletionBefore))
            {
                body.Add(EmptyCompletionExpression);
            }
            body.Add(new ContinueStatement());
            if (IsTargetMatch(stmt, CompletionCase.EmptyCompletionAfter))
            {
                body.Add(EmptyCompletionExpression);
            }
        }

        #endregion

        #region blocks

        #endregion
    }
}
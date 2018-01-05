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
using System.Linq;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi.CSharp.Parsing;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
using KaVE.Commons.Utils.Assertion;

namespace KaVE.RS.Commons.Analysis.CompletionTarget
{
    public class CompletionTargetAnalysis
    {
        public CompletionTargetMarker Analyze(ITreeNode targetNode)
        {
            var finder = new TargetFinder();
            ((ICSharpTreeNode) targetNode).Accept(finder);
            return finder.Result;
        }

        private class TargetFinder : TreeNodeVisitor
        {
            private readonly Type[] _interestingNodeTypes =
            {
                typeof(ICSharpFile),
                typeof(ICSharpNamespaceDeclaration),
                typeof(ICSharpTypeDeclaration),
                typeof(ICSharpTypeMemberDeclaration),
                typeof(IMultipleEventDeclaration),
                typeof(IMultipleFieldDeclaration),
                typeof(ICSharpStatement),
                typeof(ICSharpExpression),
                typeof(IExpressionStatement),
                typeof(IAttribute),
                typeof(IGeneralCatchClause),
                typeof(ISpecificCatchClause),
                typeof(ISwitchSection), // group of switch labels that are not separated by a break
                typeof(ISwitchCaseLabel),
                typeof(ILocalVariableDeclaration),
                typeof(IAccessorDeclaration) // property get/set
            };

            private readonly Type[] _excludedNodeTypes =
            {
                typeof(IErrorElement),
                typeof(IChameleonNode)
                //typeof(IParenthesizedExpression)
            };

            private readonly Type[] _haveDeadAreaAfterParams =
            {
                typeof(IForStatement),
                typeof(IForeachStatement),
                typeof(IIfStatement),
                typeof(ILockStatement),
                typeof(ISpecificCatchClause),
                typeof(IUsingStatement),
                typeof(IWhileStatement)
            };

            public CompletionTargetMarker Result { get; private set; }

            public override void VisitNode(ITreeNode tNode)
            {
                var comment = FindInParent<IComment>(tNode);
                if (comment != null)
                {
                    Result = new CompletionTargetMarker {HandlingNode = comment, Case = CompletionCase.Undefined};
                    return;
                }

                // TODO think about this general handling
                //tNode = ChangeTargetFromWhitespaceAfterStmtToPrevLastChild(tNode);

                Result = FindHandlingNode(tNode);
                if (Result.Case == CompletionCase.Invalid)
                {
                    return;
                }

                var isSemicolon = CSharpTokenType.SEMICOLON == tNode.GetTokenType();
                if (isSemicolon)
                {
                    Result.Case = CompletionCase.EmptyCompletionAfter;
                }

                HandleTriggerInEmptyBody(tNode);
                HandleMissingCodeAtEndOfStmtLeadsToNewEmptyStmt(tNode);
                HandleOpeningBraceForYetUndefinedCases(tNode);
                HandleClosingBraceForExpressionsAndStatements(tNode);
                HandleTriggerInDeadAreaAfterClosingParenthesis(tNode);

                Result.HandlingNode = RENAME___SelectBetterNodesThanErrors(Result.HandlingNode);
                Result.HandlingNode = StepDownIntoMultiDeclarations(Result.HandlingNode);
                StepDownIntoExpressionStatements(tNode);
                HandleSwitchCases(tNode);

                HandleTriggerOnReferences(tNode);
                SetCaseForTriggerInTypeSignature(tNode);
                SetCaseForTriggerInMethodSignature(tNode);
                SetCaseForTriggerInLambdaSignature(tNode);
                SetCaseForTriggerInFieldAndEventSignatures(tNode);
                SetCaseForTriggerInNamespaceBody(tNode);
                SetCaseForTriggerInFileBody(tNode);
                SetCaseForTriggerInProperties(tNode);
                HandleTriggerInEmptyIfElseBlock(tNode);
                // try
                HandleTriggerInTryFinallyBlock(tNode);
                HandleTriggerAfterTryBlock(tNode);
                DetectTriggerInParametersOfCatchClause(tNode);
                HandleTriggerBetweenCatchBlocks(tNode);
                HandleEmptyCompletionAfterTryBlock(tNode);
                // assignments
                HandleTriggerInAssignments(tNode);
            }

            private void HandleTriggerInAssignments(ITreeNode tNode)
            {
                var isWhitespaceToken = tNode.IsWhitespaceToken();
                var prev = isWhitespaceToken
                    ? FindPrevNonWhitespaceNode(tNode)
                    : PrevSyntaxSibling(tNode);
                var next = isWhitespaceToken
                    ? FindNextNonWhitespaceNode(tNode)
                    : NextSyntaxSibling(tNode);

                var prevRef = prev as IReferenceExpression;
                var isPrevVarKeyword = prevRef != null && "var".Equals(prevRef.NameIdentifier.Name);
                var thisRef = tNode as IReferenceExpression;
                var isVarKeyword = thisRef != null && "var".Equals(thisRef.NameIdentifier.Name);
                var isVarIsh = isVarKeyword || (isWhitespaceToken && isPrevVarKeyword);

                if (isWhitespaceToken && tNode.PrevSibling is IDeclarationStatement)
                {
                    var lastChild = ((IDeclarationStatement) tNode.PrevSibling).Children().LastOrDefault();
                    Result.Case = lastChild is IErrorElement
                        ? CompletionCase.InBody // e.g., "var o = $"
                        : CompletionCase.EmptyCompletionAfter; // e.g., "int i; i = 0;"
                    return;
                }

                if (isWhitespaceToken && (prev != null || next != null))
                {
                    tNode = prev ?? next;
                }

                if (Result.HandlingNode is IReferenceExpression && Result.HandlingNode.Parent is IAssignmentExpression)
                {
                    var n = FindNextNonWhitespaceNode(Result.HandlingNode.NextSibling);
                    if (n.GetTokenType() == CSharpTokenType.EQ)
                    {
                        Result.HandlingNode = Result.HandlingNode.Parent;
                        Result.Case = CompletionCase.InSignature;
                    }

                    if (tNode.GetTokenType() == CSharpTokenType.EQ && tNode.NextSibling is IErrorElement)
                    {
                        Result.Case = CompletionCase.InBody;
                    }
                }

                var asgn = Result.HandlingNode as IAssignmentExpression;
                if (asgn != null)
                {
                    if (isVarIsh && next != null && next.GetTokenType() == CSharpTokenType.EQ)
                    {
                        // e.g., "var $ = 1;"
                        Result.Case = CompletionCase.InSignature;
                    }
                }

                var lvd = Result.HandlingNode as ILocalVariableDeclaration;
                if (lvd != null)
                {
                    if (tNode.GetTokenType() == CSharpTokenType.VAR_KEYWORD)
                    {
                        // e.g., "var$ i = 1;"
                        Result.Case = CompletionCase.InSignature;
                    }

                    if (isPrevVarKeyword && next is ILocalVariableDeclaration)
                    {
                        // e.g., "var $ = 1;"
                        Result.Case = CompletionCase.InSignature;
                    }
                    if (next != null && next.GetTokenType() == CSharpTokenType.EQ)
                    {
                        // e.g., "var i$ = 1;"
                        Result.Case = CompletionCase.InSignature;
                    }
                }

                if (asgn != null || lvd != null)
                {
                    var ne = AssertSyntaxTokenOrUseNext(tNode.NextSibling);
                    if (ne is IErrorElement)
                    {
                        var firstChild = ne.Children().FirstOrDefault();
                        if (firstChild != null && firstChild.GetTokenType() == CSharpTokenType.EQ)
                        {
                            // e.g., "var i = $ = 1;" or  "int i,j; i = $ = 1;"
                            Result.Case = CompletionCase.InSignature;
                        }
                    }
                }

                if (Result.HandlingNode is ILocalVariableDeclaration && Result.Case == CompletionCase.Undefined)
                {
                    // TODO: get rid of this "catch all" handling
                    Result.Case = CompletionCase.InBody;
                }
            }

            private void HandleEmptyCompletionAfterTryBlock(ITreeNode tNode)
            {
                if (Result.Case == CompletionCase.EmptyCompletionAfter && Result.HandlingNode is ICatchClause)
                {
                    Result.HandlingNode = Result.HandlingNode.Parent;
                }
            }

            private void DetectTriggerInParametersOfCatchClause(ITreeNode tNode)
            {
                if (Result.HandlingNode is ICatchClause)
                {
                    // detect node on first sub level under catch
                    var subNode = tNode;
                    while (subNode.Parent != null && subNode.Parent != Result.HandlingNode)
                    {
                        subNode = subNode.Parent;
                    }

                    // try to find "(" on siblings
                    var left = subNode;
                    while (left != null && CSharpTokenType.LPARENTH != left.GetTokenType())
                    {
                        left = left.PrevSibling;
                    }

                    // try to find ")" on *next* siblings
                    // e.g., "...ption e)$ {..." is not a positive case
                    var right = subNode.NextSibling;
                    while (right != null && CSharpTokenType.RPARENTH != right.GetTokenType())
                    {
                        right = right.NextSibling;
                    }

                    // if both are found, trigger was in sig
                    if (left != null && right != null)
                    {
                        Result.Case = CompletionCase.InSignature;
                    }
                }
            }

            [NotNull]
            private static ITreeNode ChangeTargetFromWhitespaceAfterStmtToPrevLastChild([NotNull] ITreeNode tNode)
            {
                // TODO not working, e.g., "if(true) {} else {}$" prevLastChild is an IBlock
                if (tNode.IsWhitespaceToken() && tNode.PrevSibling is IStatement)
                {
                    var prevLastChild = tNode.PrevSibling.Children().LastOrDefault();
                    return prevLastChild ?? tNode;
                }
                return tNode;
            }

            private void HandleClosingBraceForExpressionsAndStatements([NotNull] ITreeNode tNode)
            {
                if (tNode.IsWhitespaceToken() && tNode.PrevSibling is IBlock)
                {
                    var prevLastSibling = tNode.PrevSibling.Children().LastOrDefault();
                    if (prevLastSibling != null && CSharpTokenType.RBRACE == prevLastSibling.GetTokenType())
                    {
                        tNode = prevLastSibling;
                    }
                }
                // skip whitespace and comments
                tNode = AssertSyntaxTokenOrUsePrev(tNode);

                if (tNode != null && CSharpTokenType.RBRACE == tNode.GetTokenType())
                {
                    Result.Case = CompletionCase.EmptyCompletionAfter;

                    var h = Result.HandlingNode;

                    // try/catch
                    var tb = (h is ICatchClause ? h.Parent : h) as ITryStatement;
                    if (tb != null)
                    {
                        Result.Case = CompletionCase.Undefined;
                        var isClosingFinally = tb.FinallyBlock != null && tb.FinallyBlock == tNode.Parent;
                        if (isClosingFinally)
                        {
                            Result.Case = CompletionCase.EmptyCompletionAfter;
                        }
                        if (tb.FinallyBlock == null)
                        {
                            var lastCatch = tb.Catches.LastOrDefault();
                            var thisCatch = FindInParent<ICatchClause>(tNode, typeof(ITryStatement));
                            var isClosingLastCatch = lastCatch != null && lastCatch == thisCatch;
                            if (isClosingLastCatch)
                            {
                                Result.Case = CompletionCase.EmptyCompletionAfter;
                            }
                        }
                        return;
                    }

                    var ib = h as IIfStatement;
                    if (ib != null)
                    {
                        var isClosingThenAndNoElse = ib.Else == null && ib.Then == tNode.Parent;
                        var isClosingElse = ib.Else != null && ib.Else == tNode.Parent;
                        Result.Case = isClosingThenAndNoElse || isClosingElse
                            ? CompletionCase.EmptyCompletionAfter
                            : Result.Case = CompletionCase.Undefined;
                        return;
                    }

                    var oce = h as IObjectCreationExpression;
                    if (oce != null)
                    {
                        var p = FindInParent<IObjectCreationExpression>(
                            tNode,
                            typeof(ICollectionElementInitializer));

                        if (p == null)
                        {
                            // traversal aborts if "}" belongs to nested dict init
                            Result.Case = CompletionCase.Undefined;
                        }
                        else
                        {
                            // try to find a second enclosing init
                            var pp = FindInParent<IObjectCreationExpression>(
                                p.Parent,
                                typeof(IStatement));

                            if (pp != null)
                            {
                                // if found, the case is undefine (it is nested)
                                Result.Case = CompletionCase.Undefined;
                            }
                        }
                    }
                }
            }

            private void HandleTriggerInEmptyBody(ITreeNode tNode)
            {
                var left = AssertSyntaxTokenOrUsePrev(tNode);
                var right = AssertSyntaxTokenOrUseNext(tNode);
                var isLeftBrace = left != null && CSharpTokenType.LBRACE == left.GetTokenType();
                var isRightBrace = right != null && CSharpTokenType.RBRACE == right.GetTokenType();
                if (isLeftBrace && isRightBrace)
                {
                    Result.Case = CompletionCase.InBody;
                }
            }

            private void HandleMissingCodeAtEndOfStmtLeadsToNewEmptyStmt(ITreeNode tNode)
            {
                // TODO: get rid of special handling for throw
                if (Result.Case == CompletionCase.EmptyCompletionAfter && !(Result.HandlingNode is IThrowStatement))
                {
                    var lastChild = Result.HandlingNode.Children().LastOrDefault();
                    if (lastChild is IErrorElement)
                    {
                        Result.Case = CompletionCase.InBody;
                    }
                }
            }

            private void HandleOpeningBraceForYetUndefinedCases(ITreeNode tNode)
            {
                var left = AssertSyntaxTokenOrUsePrev(tNode);
                var isOpeningBrace = left != null && CSharpTokenType.LBRACE == left.GetTokenType();
                // make sure that no valid entries are overriden (e.g.,"{ $ continue;}" EmptyCompletionBefore)
                if (isOpeningBrace && Result.Case == CompletionCase.Undefined)
                {
                    Result.Case = CompletionCase.InBody;
                }
            }

            private void HandleTriggerInDeadAreaAfterClosingParenthesis(ITreeNode tNode)
            {
                if (IsTriggerInDeadAreaAfterClosingParanthesis(tNode))
                {
                    // find next non-whitespace "to the right"
                    var n = AssertSyntaxTokenOrUseNext(tNode, CSharpTokenType.RPARENTH);

                    // not sure how to get to this case
                    if (n == null)
                    {
                        return;
                    }

                    // before "{"
                    if (n is IBlock)
                    {
                        Result.Case = CompletionCase.Undefined;
                        return;
                    }

                    var isBeforeSingleStatement = n is IStatement;
                    var isMissingCodeAtTheEnd = n is IErrorElement;
                    if (isBeforeSingleStatement || isMissingCodeAtTheEnd)
                    {
                        Result.Case = CompletionCase.InBody;
                    }
                }
            }

            private bool IsTriggerInDeadAreaAfterClosingParanthesis(ITreeNode tNode)
            {
                // check whether parent is potentially affected
                var isPotentialHit =
                    _haveDeadAreaAfterParams.Any(p => p.IsInstanceOfType(tNode.Parent));
                if (isPotentialHit)
                {
                    var n = AssertSyntaxTokenOrUsePrev(tNode, CSharpTokenType.LBRACE);
                    var leftIsClosingParanthesis = n != null && CSharpTokenType.RPARENTH == n.GetTokenType();
                    return leftIsClosingParanthesis;
                }
                return false;
            }

            private void HandleTriggerAfterTryBlock(ITreeNode tNode)
            {
                if (Result.HandlingNode is ITryStatement && Result.Case == CompletionCase.EmptyCompletionAfter)
                {
                    var n = FindInParent<ITryStatement>(tNode, typeof(IStatement));
                    if (n != null && n == Result.HandlingNode)
                    {
                        Result.Case = CompletionCase.Undefined;
                    }
                }
            }

            private void HandleTriggerBetweenCatchBlocks(ITreeNode tn)
            {
                var hn = Result.HandlingNode;
                var isCatchClause = hn is ISpecificCatchClause || hn is IGeneralCatchClause;

                if (isCatchClause && Result.Case == CompletionCase.EmptyCompletionBefore)
                {
                    Result.Case = CompletionCase.Undefined;
                    return;
                }

                if (isCatchClause && Result.Case == CompletionCase.EmptyCompletionAfter)
                {
                    while (hn.NextSibling != null)
                    {
                        hn = hn.NextSibling;

                        isCatchClause = hn is ISpecificCatchClause || hn is IGeneralCatchClause;
                        var isFinallyToken = CSharpTokenType.FINALLY_KEYWORD == hn.GetTokenType();

                        if (isCatchClause || isFinallyToken)
                        {
                            Result.Case = CompletionCase.Undefined;
                            return;
                        }
                    }
                }
            }

            private void HandleSwitchCases(ITreeNode tNode)
            {
                if (Result.HandlingNode is ISwitchCaseLabel)
                {
                    var label = FindInParent<ISwitchCaseLabel>(tNode, typeof(IStatement), typeof(IExpression));
                    Result.Case = label != null
                        ? CompletionCase.InSignature
                        : CompletionCase.InBody;

                    if (CSharpTokenType.COLON == tNode.GetTokenType())
                    {
                        Result.Case = CompletionCase.InBody;
                    }
                }

                if (Result.HandlingNode is ISwitchStatement)
                {
                    var stmt = FindInParent<ISwitchStatement>(tNode, typeof(IChameleonNode));
                    if (stmt == null || stmt != Result.HandlingNode)
                    {
                        return;
                    }

                    var block = FindInParent<ISwitchBlock>(tNode, typeof(ISwitchStatement));
                    Result.Case = block == null
                        ? CompletionCase.InSignature
                        : CompletionCase.InBody;
                }

                var ss = Result.HandlingNode as ISwitchSection;
                if (ss != null)
                {
                    // trigger in dead space before first label?
                    if (tNode.IsWhitespaceToken() && tNode.Parent is ISwitchBlock)
                    {
                        var prev = AssertSyntaxTokenOrUsePrev(tNode);
                        if (prev != null && CSharpTokenType.LBRACE == prev.GetTokenType())
                        {
                            Result.HandlingNode = tNode.Parent.Parent;
                            Result.Case = CompletionCase.Undefined;
                            return;
                        }
                    }

                    var lastCase = ss.CaseLabels.LastOrDefault();
                    if (lastCase != null)
                    {
                        Result.HandlingNode = lastCase;
                        if (lastCase.LastChild is IErrorElement)
                        {
                            Result.Case = CompletionCase.InSignature;
                            return;
                        }

                        ITreeNode lastSib = lastCase;
                        ITreeNode lastStmt = null;
                        while (lastSib.NextSibling != null)
                        {
                            lastSib = lastSib.NextSibling;
                            if (lastSib is IStatement)
                            {
                                lastStmt = lastSib;
                            }
                        }
                        if (lastStmt == null)
                        {
                            Result.Case = CompletionCase.InBody;
                        }
                        else
                        {
                            Result.HandlingNode = lastStmt;
                            Result.Case = CompletionCase.EmptyCompletionAfter;
                        }
                    }
                }
            }

            private void StepDownIntoExpressionStatements(ITreeNode tNode)
            {
                var es = Result.HandlingNode as IExpressionStatement;
                if (es != null && es.Expression != null)
                {
                    Result.HandlingNode = es.Expression;
                }
            }

            private void HandleTriggerOnReferences(ITreeNode tNode)
            {
                var isCompletionOnDot = CSharpTokenType.DOT == tNode.GetTokenType();
                var prev = tNode.PrevSibling;
                while (prev.IsWhitespaceToken())
                {
                    prev = prev.PrevSibling;
                }
                var isCompletionOnIdentifierAfterDot = prev != null &&
                                                       CSharpTokenType.DOT == prev.GetTokenType();

                if (isCompletionOnDot || isCompletionOnIdentifierAfterDot)
                {
                    Result.HandlingNode = Result.HandlingNode.Parent;
                    Result.Case = CompletionCase.Undefined;
                }

                // distinguish incomplete triggers from "EmptyAfter" cases
                if (Result.HandlingNode is IReferenceExpression)
                {
                    var next = Result.HandlingNode.NextSibling;
                    if (next is IErrorElement)
                    {
                        Result.Case = CompletionCase.Undefined;
                    }
                }
            }

            private void SetCaseForTriggerInFieldAndEventSignatures(ITreeNode tNode)
            {
                var h = Result.HandlingNode;
                if (h is IFieldDeclaration || h is IEventDeclaration)
                {
                    var pf = FindInParent<IMultipleFieldDeclaration>(tNode);
                    var pe = FindInParent<IMultipleEventDeclaration>(tNode);
                    if (pf != null || pe != null)
                    {
                        Result.Case = CompletionCase.InSignature;
                    }
                }
            }

            private void SetCaseForTriggerInProperties(ITreeNode tNode)
            {
                var t = Result.HandlingNode as IAccessorDeclaration;
                if (t != null)
                {
                    Result.HandlingNode = t.Parent;

                    var p = FindInParent<IPropertyDeclaration>(
                        tNode,
                        typeof(IAccessorDeclaration),
                        typeof(IChameleonNode));
                    if (p != null)
                    {
                        Result.Case = CompletionCase.InSignature;
                        return;
                    }

                    // try to find containing body node
                    var p2 = FindInParent<IChameleonNode>(
                        tNode,
                        typeof(IAccessorDeclaration));

                    // fall back to signature if not found...
                    if (t.Body == null || p2 == null)
                    {
                        Result.Case = CompletionCase.InSignature;
                        return;
                    }
                    // ... or if closing brace
                    if (CSharpTokenType.RBRACE == tNode.GetTokenType() && tNode.Parent == t.Body)
                    {
                        Result.Case = CompletionCase.InSignature;
                        return;
                    }

                    Result.Case = "get".Equals(t.NameIdentifier.Name)
                        ? CompletionCase.InGetBody
                        : CompletionCase.InSetBody;
                }

                if (Result.HandlingNode is IPropertyDeclaration && Result.Case == CompletionCase.InBody)
                {
                    Result.Case = CompletionCase.InSignature;
                }
            }

            private void SetCaseForTriggerInFileBody(ITreeNode tNode)
            {
                if (Result.HandlingNode is ICSharpFile)
                {
                    Result.Case = CompletionCase.InBody;
                }
            }

            private void SetCaseForTriggerInNamespaceBody(ITreeNode tNode)
            {
                if (Result.HandlingNode is INamespaceDeclaration && Result.Case == CompletionCase.Undefined)
                {
                    var ns = FindInParent<INamespaceDeclaration>(tNode, typeof(INamespaceBody));
                    if (ns != null)
                    {
                        Result.Case = CompletionCase.InSignature;
                        return;
                    }
                    ns = FindInParent<INamespaceDeclaration>(tNode);
                    if (ns != null)
                    {
                        Result.Case = CompletionCase.InBody;
                    }
                }
            }

            private void SetCaseForTriggerInLambdaSignature(ITreeNode tNode)
            {
                if (Result.HandlingNode is ILambdaExpression)
                {
                    var sig = FindInParent<ILambdaSignature>(tNode);
                    if (sig != null && sig.Parent == Result.HandlingNode)
                    {
                        Result.Case = CompletionCase.InSignature;
                    }

                    if (CSharpTokenType.LAMBDA_ARROW == tNode.GetTokenType())
                    {
                        Result.Case = CompletionCase.InSignature;
                    }
                }
            }

            private void HandleTriggerInTryFinallyBlock(ITreeNode tNode)
            {
                if (Result.HandlingNode is ITryStatement && Result.Case == CompletionCase.InBody)
                {
                    var p = FindInParent<IBlock>(tNode);
                    if (p != null)
                    {
                        ITreeNode prev = p;
                        while ((prev = prev.PrevSibling) != null)
                        {
                            if (CSharpTokenType.FINALLY_KEYWORD == prev.GetTokenType())
                            {
                                Result.Case = CompletionCase.InFinally;
                                return;
                            }
                        }
                    }
                }
            }

            private void HandleTriggerInEmptyIfElseBlock(ITreeNode n)
            {
                if (Result.HandlingNode is IIfStatement && Result.Case == CompletionCase.InBody)
                {
                    var enclosingBlock = FindInParent<IBlock>(n);
                    if (enclosingBlock == null)
                    {
                        return;
                    }

                    ITreeNode prev = enclosingBlock;
                    while ((prev = prev.PrevSibling) != null)
                    {
                        if (prev is IBlock)
                        {
                            Result.Case = CompletionCase.InElse;
                            return;
                        }
                    }
                }
            }

            private static bool IsOpeningBraceOrAnyParenthesis(ITreeNode tNode)
            {
                var isLBrace = CSharpTokenType.LBRACE == tNode.GetTokenType();
                var isLPara = CSharpTokenType.LPARENTH == tNode.GetTokenType();
                var isRPara = CSharpTokenType.RPARENTH == tNode.GetTokenType();
                var b = isLBrace || isLPara || isRPara;
                return b;
            }

            private ITreeNode RENAME___SelectBetterNodesThanErrors(ITreeNode n)
            {
                if (n.Parent is IMethodDeclaration)
                {
                    return n.Parent;
                }
                return n;
            }

            private void SetCaseForTriggerInMethodSignature(ITreeNode tNode)
            {
                if (Result.HandlingNode is IMethodDeclaration)
                {
                    var tmp = FindInParent<IMethodDeclaration>(tNode, typeof(IChameleonNode), typeof(IStatement));
                    if (tmp != null)
                    {
                        Result.Case = CompletionCase.InSignature;
                    }
                }
            }

            private void SetCaseForTriggerInTypeSignature(ITreeNode tNode)
            {
                if (Result.HandlingNode is ICSharpTypeDeclaration)
                {
                    var tmp = FindInParent<ICSharpTypeDeclaration>(
                        tNode,
                        typeof(IChameleonNode),
                        typeof(IStatement),
                        typeof(IMemberOwnerBody));
                    if (tmp != null)
                    {
                        Result.Case = CompletionCase.InSignature;
                    }
                }
            }

            private static ITreeNode StepDownIntoMultiDeclarations(ITreeNode n)
            {
                var x = n as IDeclarationStatement;
                if (x != null)
                {
                    var lastConst = x.ConstantDeclarations.LastOrDefault();
                    if (lastConst != null)
                    {
                        return lastConst;
                    }
                    var lastVar = x.VariableDeclarations.LastOrDefault();
                    if (lastVar != null)
                    {
                        return lastVar;
                    }
                    return x.LocalFunctionDeclaration;
                }

                var fs = n as IMultipleFieldDeclaration;
                if (fs != null)
                {
                    var lastDecl = fs.Declarators.LastOrDefault();
                    if (lastDecl != null)
                    {
                        return lastDecl;
                    }
                }

                var es = n as IMultipleEventDeclaration;
                if (es != null)
                {
                    var lastDecl = es.Declarators.LastOrDefault();
                    if (lastDecl != null)
                    {
                        return lastDecl;
                    }
                }

                return n;
            }

            private CompletionTargetMarker FindHandlingNode(ITreeNode n)
            {
                ITreeNode handler = null;
                var c = CompletionCase.Undefined;

                // error case
                if (n == null)
                {
                    return new CompletionTargetMarker {Case = CompletionCase.Invalid};
                }

                var isNonHandlingBlock = n is IBlock && !IsHandlerBlock(n);
                var isNonHandlingPartOfSwitchBloc = n.Parent is ISwitchStatement && !IsHandling(n);
                if (isNonHandlingBlock || isNonHandlingPartOfSwitchBloc ||
                    IsTriggerInDeadAreaAfterClosingParanthesis(n))
                {
                    return FindHandlingNode(n.Parent);
                }

                if (!IsSyntaxToken(n) && n.Parent is IIfStatement)
                {
                    return FindHandlingNode(n.Parent);
                }

                // base cases
                if (IsHandling(n))
                {
                    handler = n as ICSharpTreeNode;
                }

                if (IsOpeningBraceOrAnyParenthesis(n))
                {
                    return FindHandlingNode(n.Parent);
                }

                if (handler == null && FindHandlingSibling(n, true, out handler))
                {
                    c = CompletionCase.EmptyCompletionAfter;
                }
                if (handler == null && FindHandlingSibling(n, false, out handler))
                {
                    c = CompletionCase.EmptyCompletionBefore;
                }

                // recursion
                return handler != null
                    ? new CompletionTargetMarker {HandlingNode = handler, Case = c}
                    : FindHandlingNode(n.Parent);
            }

            private bool FindHandlingSibling(ITreeNode n, bool goBack, out ITreeNode target)
            {
                while ((n = goBack ? n.PrevSibling : n.NextSibling) != null)
                {
                    if (IsHandling(n))
                    {
                        target = n;
                        return true;
                    }
                }
                target = null;
                return false;
            }

            private bool IsHandling(ITreeNode n)
            {
                if (!_interestingNodeTypes.Any(t => t.IsInstanceOfType(n)))
                {
                    return false;
                }
                if (_excludedNodeTypes.Any(t => t.IsInstanceOfType(n)))
                {
                    return false;
                }
                if (n is IBlock)
                {
                    return IsHandlerBlock(n);
                }
                return true;
            }

            private static bool IsHandlerBlock(ITreeNode n)
            {
                var p = n.Parent;
                var isStandaloneBlock = p is IBlock || p is IChameleonNode || p is ISwitchSection;
                return isStandaloneBlock;
            }

            private void FindAvailableTarget(ITreeNode target)
            {
                var isOutsideMethodBody = target.Parent is IClassBody;
                if (isOutsideMethodBody)
                {
                    // TODO think about this simplification...
                    Result.HandlingNode = null;
                    Result.Case = CompletionCase.Undefined;
                    return;
                }

                var stmt = target as ICSharpStatement;
                if (stmt != null)
                {
                    Result.HandlingNode = stmt;
                    Result.Case = CompletionCase.EmptyCompletionAfter;
                    return;
                }

                var csExpr = target as ICSharpExpression;
                if (csExpr != null)
                {
                    Result.HandlingNode = csExpr;
                    Result.Case = CompletionCase.Undefined;
                    return;
                }

                var prev = FindPrevNonWhitespaceNode(target.PrevSibling);
                var next = FindNextNonWhitespaceNode(target.NextSibling);

                if (prev != null)
                {
                    var expr = prev as IExpressionStatement;
                    var decl = prev as IDeclarationStatement;
                    var isAssign = CSharpTokenType.EQ == target.GetTokenType();
                    var tpdecl = target.Parent as ILocalVariableDeclaration;

                    var scl = prev as ISwitchCaseLabel;
                    var ss = prev as ISwitchSection; // can be both

                    if (ss != null)
                    {
                        // strange bug in ReSharper AST
                        if (next == null && ss.Statements.Count > 0)
                        {
                            Result.HandlingNode = ss.Statements.LastOrDefault();
                            Result.Case = CompletionCase.EmptyCompletionAfter;
                            return;
                        }
                        scl = ss.CaseLabels.FirstOrDefault();
                    }

                    if (scl != null)
                    {
                        var isInBetweenLabels = next is ISwitchCaseLabel;

                        ss = scl.Parent as ISwitchSection;
                        Asserts.NotNull(ss);

                        if (ss.Statements.Count == 0 || isInBetweenLabels)
                        {
                            Result.HandlingNode = scl;
                            Result.Case = CompletionCase.InBody;
                        }
                        else
                        {
                            Result.HandlingNode = ss.Statements.First();
                            Result.Case = CompletionCase.EmptyCompletionBefore;
                        }
                    }
                    else if (expr != null)
                    {
                        Result.HandlingNode = prev.FirstChild as ICSharpTreeNode;
                        Result.Case = CompletionCase.EmptyCompletionAfter;
                    }
                    else if (decl != null && HasError(prev))
                    {
                        Result.Case = CompletionCase.Undefined;
                        var multi = decl.Declaration as IMultipleLocalVariableDeclaration;
                        Result.HandlingNode = multi != null ? multi.DeclaratorsEnumerable.Last() : prev;
                    }

                    else if (decl != null)
                    {
                        Result.Case = CompletionCase.EmptyCompletionAfter;
                        var multi = decl.Declaration as IMultipleLocalVariableDeclaration;
                        Result.HandlingNode = multi != null ? multi.DeclaratorsEnumerable.Last() : prev;
                    }
                    else if (isAssign && tpdecl != null)
                    {
                        Result.Case = CompletionCase.Undefined;
                        Result.HandlingNode = tpdecl;
                    }
                    else
                    {
                        Result.HandlingNode = prev;
                        Result.Case = CompletionCase.EmptyCompletionAfter;
                    }
                }
                else if (next != null)
                {
                    var decl = next as IDeclarationStatement;

                    if (decl != null)
                    {
                        Result.Case = CompletionCase.EmptyCompletionBefore;
                        var multi = decl.Declaration as IMultipleLocalVariableDeclaration;
                        Result.HandlingNode = multi != null ? multi.DeclaratorsEnumerable.Last() : next;
                    }
                    else
                    {
                        Result.HandlingNode = next;
                        Result.Case = CompletionCase.EmptyCompletionBefore;
                    }
                }
                else
                {
                    Result.Case = CompletionCase.InBody;
                    Result.HandlingNode = FindNonBlockParent(target);
                }
            }

            private static bool HasError(ITreeNode prev)
            {
                return prev.LastChild is IErrorElement;
            }

            private ITreeNode FindNonBlockParent(ITreeNode target)
            {
                if (target is IEmptyStatement)
                {
                    return FindNonBlockParent(target.Parent);
                }

                var parent = target.Parent as ICSharpTreeNode;

                if (parent is IChameleonNode)
                {
                    var methDecl = parent.Parent as IMethodDeclaration;
                    if (methDecl != null)
                    {
                        return methDecl;
                    }
                }

                var block = parent as IBlock;
                if (block != null)
                {
                    var parentStatement = block.Parent as ICSharpStatement;
                    if (parentStatement != null)
                    {
                        if (IsElseBlock(block, parentStatement))
                        {
                            Result.Case = CompletionCase.InElse;
                        }
                        if (IsFinallyBlock(block, parentStatement))
                        {
                            Result.Case = CompletionCase.InFinally;
                        }

                        return parentStatement;
                    }

                    var catchClause = block.Parent as IGeneralCatchClause;
                    if (catchClause != null)
                    {
                        return catchClause;
                    }

                    // TODO: why is the following needed?
                    FindAvailableTarget(block);
                    return Result.HandlingNode;
                }
                return parent;
            }

            private bool IsElseBlock(IBlock block, ICSharpStatement parentStatement)
            {
                var ifBlock = parentStatement as IIfStatement;
                if (ifBlock != null)
                {
                    if (ifBlock.Else == block)
                    {
                        return true;
                    }
                }
                return false;
            }

            private bool IsFinallyBlock(IBlock block, ICSharpStatement parentStatement)
            {
                var tryBlock = parentStatement as ITryStatement;
                if (tryBlock != null)
                {
                    if (tryBlock.FinallyBlock == block)
                    {
                        return true;
                    }
                }
                return false;
            }

            private static ICSharpTreeNode FindNextNonWhitespaceNode(ITreeNode node)
            {
                if (node == null)
                {
                    return null;
                }
                if (IsWhitespaceOrBraceToken(node))
                {
                    node = FindNextNonWhitespaceNode(node.NextSibling);
                }
                if (node.IsCommentToken())
                {
                    node = FindNextNonWhitespaceNode(node.NextSibling);
                }
                return node as ICSharpTreeNode;
            }

            private static bool IsWhitespaceOrBraceToken(ITreeNode node)
            {
                var isLBrace = CSharpTokenType.LBRACE == node.GetTokenType();
                var isRBrace = CSharpTokenType.RBRACE == node.GetTokenType();
                return node.IsWhitespaceToken() || isLBrace || isRBrace;
            }

            private ICSharpTreeNode FindPrevNonWhitespaceNode(ITreeNode node)
            {
                if (node == null)
                {
                    return null;
                }
                if (IsWhitespaceOrBraceToken(node))
                {
                    node = FindPrevNonWhitespaceNode(node.PrevSibling);
                }
                if (node.IsCommentToken())
                {
                    node = FindPrevNonWhitespaceNode(node.PrevSibling);
                }
                return node as ICSharpTreeNode;
            }

            #region nav utils

            [CanBeNull]
            private static ITreeNode PrevSyntaxSibling([NotNull] ITreeNode n)
            {
                var prev = n;
                while ((prev = prev.PrevSibling) != null)
                {
                    if (!prev.IsWhitespaceToken() && !prev.IsCommentToken())
                    {
                        return prev;
                    }
                }
                return null;
            }

            [CanBeNull]
            private static ITreeNode NextSyntaxSibling([NotNull] ITreeNode n)
            {
                var next = n;
                while ((next = next.NextSibling) != null)
                {
                    if (!next.IsWhitespaceToken() && !next.IsCommentToken())
                    {
                        return next;
                    }
                }
                return null;
            }

            [CanBeNull]
            private static ITreeNode AssertSyntaxTokenOrUsePrev(ITreeNode n,
                params TokenNodeType[] ignoreTokens)
            {
                if (n == null)
                {
                    return null;
                }
                return IsSyntaxToken(n, ignoreTokens) ? n : PrevSyntaxSibling(n);
            }

            [CanBeNull]
            private static ITreeNode AssertSyntaxTokenOrUseNext(ITreeNode n,
                params TokenNodeType[] ignoreTokens)
            {
                if (n == null)
                {
                    return null;
                }
                return IsSyntaxToken(n, ignoreTokens) ? n : NextSyntaxSibling(n);
            }

            private static bool IsSyntaxToken([NotNull] ITreeNode n, params TokenNodeType[] ignoreTokens)
            {
                var isIgnoredToken = ignoreTokens.Contains(n.GetTokenType());
                return!isIgnoredToken && !n.IsWhitespaceToken() && !n.IsCommentToken();
            }

            private static T FindInParent<T>(ITreeNode node, params Type[] abortTypes) where T : ITreeNode
            {
                if (node == null)
                {
                    return default(T);
                }
                var nodeType = node.GetType();
                if (abortTypes.Any(abortType => abortType.IsAssignableFrom(nodeType)))
                {
                    return default(T);
                }
                if (node is T)
                {
                    return (T) node;
                }
                return FindInParent<T>(node.Parent, abortTypes);
            }

            #endregion
        }
    }
}
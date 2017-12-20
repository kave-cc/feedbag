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
using JetBrains.ReSharper.Psi.CSharp.Parsing;
using JetBrains.ReSharper.Psi.CSharp.Tree;
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

        //private static IName GetName(IReference reference)
        //{
        //    var resolvedReference = reference.Resolve();
        //    var result = resolvedReference.Result;
        //    var declaredElement = result.DeclaredElement;
        //    return declaredElement != null ? declaredElement.GetName(result.Substitution) : null;
        //}

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
                typeof(ISwitchCaseLabel),
                typeof(ILocalVariableDeclaration),
                typeof(IErrorElement),
                typeof(IAccessorDeclaration) // property get/set
            };

            private readonly Type[] _excludedNodeTypes =
            {
                // typeof(IBlock),
                typeof(IChameleonNode)
                // typeof(IParenthesizedExpression)
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

                if (IsOpeningBraceOrAnyParenthesis(tNode))
                {
                    Result.Case = CompletionCase.InBody;
                }

                if (IsClosingBraceOfExpressionOrStatement(tNode))
                {
                    Result.Case = CompletionCase.EmptyCompletionAfter;
                }


                if (IsNonWhitespaceSiblingABrace(tNode, true) && IsNonWhitespaceSiblingABrace(tNode, false))
                {
                    Result.Case = CompletionCase.InBody;
                }

                Result.HandlingNode = RENAME___SelectBetterNodesThanErrors(Result.HandlingNode);
                Result.HandlingNode = StepDownIntoMultiDeclarations(Result.HandlingNode);

                SetCaseForTriggerInTypeSignature(tNode);
                SetCaseForTriggerInMethodSignature(tNode);
                SetCaseForTriggerInLambdaSignature(tNode);
                SetCaseForTriggerInFieldAndEventSignatures(tNode);
                SetCaseForTriggerInNamespaceBody(tNode);
                SetCaseForTriggerInFileBody(tNode);
                SetCaseForTriggerInProperties(tNode);
                HandleTriggerInEmptyIfElseBlock(tNode);
                HandleTriggerInTryFinallyBlock(tNode);

                return;

                var handler = Result.HandlingNode;
                var parent = tNode.Parent as ICSharpTreeNode;

                var isDot = CSharpTokenType.DOT == tNode.GetTokenType();
                if (isDot && tNode.Parent != null && parent is IReferenceExpression)
                {
                    Result.HandlingNode = parent;
                    Result.Case = CompletionCase.Undefined;
                    return;
                }

                var attr = FindInParent<IAttribute>(tNode, typeof(IChameleonNode));
                if (attr != null)
                {
                    Result.HandlingNode = attr;
                    Result.Case = CompletionCase.Undefined;
                    return;
                }

                var md = FindInParent<IMethodDeclaration>(tNode, typeof(IChameleonNode), typeof(IStatement));
                if (md != null)
                {
                    Result.HandlingNode = md;
                    Result.Case = CompletionCase.InSignature;
                    return;
                }

                var td = FindInParent<ICSharpTypeDeclaration>(
                    tNode,
                    typeof(IChameleonNode),
                    typeof(IStatement),
                    typeof(IMemberOwnerBody));
                if (td != null)
                {
                    Result.HandlingNode = td;
                    Result.Case = CompletionCase.InSignature;
                    return;
                }

                var isAssign = CSharpTokenType.EQ == tNode.GetTokenType();
                if (IsWhitespaceOrBraceToken(handler) || isAssign)
                {
                    FindAvailableTarget(handler);
                }

                if (Result.HandlingNode is IAssignmentExpression && HasError(Result.HandlingNode))
                {
                    Result.Case = CompletionCase.Undefined;
                    return;
                }

                if (isAssign)
                {
                    Result.Case = CompletionCase.Undefined;
                }

                if (handler is IIdentifier)
                {
                    Result.HandlingNode = handler.Parent as ICSharpTreeNode;
                    Result.Case = CompletionCase.Undefined;
                }

                var exprStatement = Result.HandlingNode as IExpressionStatement;
                if (exprStatement != null && exprStatement.Expression != null)
                {
                    Result.HandlingNode = exprStatement.Expression;
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

            private bool IsClosingBraceOfExpressionOrStatement(ITreeNode tNode)
            {
                var h = Result.HandlingNode;
                var isRBrace = CSharpTokenType.RBRACE == tNode.GetTokenType();
                var isExprOrStmt = h is IExpression || h is IStatement;
                return isRBrace && isExprOrStmt;
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

                if (n is IBlock && !IsHandlerBlock(n))
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

            private static bool IsNonWhitespaceSiblingABrace(ITreeNode n, bool goBack)
            {
                if (!n.IsWhitespaceToken() && !n.IsCommentToken())
                {
                    return false;
                }

                while ((n = goBack ? n.PrevSibling : n.NextSibling) != null)
                {
                    if (!n.IsWhitespaceToken() && !n.IsCommentToken())
                    {
                        return goBack
                            ? CSharpTokenType.LBRACE == n.GetTokenType()
                            : CSharpTokenType.RBRACE == n.GetTokenType();
                    }
                }
                return false;
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

            public override void VisitClassDeclaration(IClassDeclaration classDecl)
            {
                //Result.Parent = classDecl;
                // TODO add type for type completion
            }

            public override void VisitMethodDeclaration(IMethodDeclaration methodDeclarationParam)
            {
                //Result.Parent = methodDeclarationParam;
            }

            public override void VisitReferenceExpression(IReferenceExpression refExpr)
            {
                var parent = refExpr.Parent as ICSharpTreeNode;
                if (parent != null)
                {
                    parent.Accept(this);

                    // in case of member access, refExpr.QualifierExpression and refExpr.Delimiter are set
                    var qRrefExpr = refExpr.QualifierExpression as IReferenceExpression;
                    if (qRrefExpr != null && refExpr.Delimiter != null)
                    {
                        var refName = qRrefExpr.Reference.GetName();
                        var token = refExpr.Reference.GetName();
                        //Result.Completion = new CompletionExpression
                        //{
                        //    VariableReference = SSTUtil.VariableReference(refName),
                        //    Token = token
                        //};
                    }
                    else
                    {
                        var token = refExpr.Reference.GetName();
                        //Result.Completion = new CompletionExpression {Token = token};
                    }
                }
            }
        }
    }
}
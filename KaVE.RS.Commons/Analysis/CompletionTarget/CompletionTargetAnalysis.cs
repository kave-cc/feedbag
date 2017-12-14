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
                typeof(ICSharpNamespaceDeclaration),
                typeof(ICSharpTypeDeclaration),
                typeof(ICSharpTypeMemberDeclaration),
                typeof(ICSharpStatement),
                typeof(ICSharpExpression),
                typeof(IExpressionStatement),
                typeof(IAttribute),
                typeof(ISwitchSection),
                typeof(ILocalVariableDeclaration),
                typeof(IErrorElement)
            };

            private readonly Type[] _excludedNodeTypes =
            {
                typeof(IChameleonNode)
                // typeof(IParenthesizedExpression)
            };

            public CompletionTargetMarker Result { get; private set; }

            public override void VisitNode(ITreeNode tNode)
            {
                Result = FindInterestingNode(tNode);
                if (Result.Case == CompletionCase.Invalid)
                {
                    return;
                }

                Result.AffectedNode = SelectBetterNodesThanErrors(Result.AffectedNode);
                Result.AffectedNode = PossiblyStepDown(Result.AffectedNode);

                SetCaseForTriggerInTypeSignature(tNode);
                SetCaseForTriggerInMethodSignature(tNode);

                var isSemicolon = CSharpTokenType.SEMICOLON == tNode.GetTokenType();
                if (isSemicolon)
                {
                    Result.Case = CompletionCase.EmptyCompletionAfter;
                }

                if (IsBraceOrParenthesis(tNode))
                {
                    Result.Case = CompletionCase.InBody;
                }


                if (IsNonWhitespaceSiblingABrace(tNode, true) && IsNonWhitespaceSiblingABrace(tNode, false))
                {
                    Result.Case = CompletionCase.InBody;
                }

                return;

                var handler = Result.AffectedNode;
                var parent = tNode.Parent as ICSharpTreeNode;

                var isDot = CSharpTokenType.DOT == tNode.GetTokenType();
                if (isDot && tNode.Parent != null && parent is IReferenceExpression)
                {
                    Result.AffectedNode = parent;
                    Result.Case = CompletionCase.Undefined;
                    return;
                }

                var attr = FindInParent<IAttribute>(tNode, typeof(IChameleonNode));
                if (attr != null)
                {
                    Result.AffectedNode = attr;
                    Result.Case = CompletionCase.Undefined;
                    return;
                }

                var md = FindInParent<IMethodDeclaration>(tNode, typeof(IChameleonNode), typeof(IStatement));
                if (md != null)
                {
                    Result.AffectedNode = md;
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
                    Result.AffectedNode = td;
                    Result.Case = CompletionCase.InSignature;
                    return;
                }

                var isAssign = CSharpTokenType.EQ == tNode.GetTokenType();
                if (IsWhitespaceOrBraceToken(handler) || isAssign)
                {
                    FindAvailableTarget(handler);
                }

                if (Result.AffectedNode is IAssignmentExpression && HasError(Result.AffectedNode))
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
                    Result.AffectedNode = handler.Parent as ICSharpTreeNode;
                    Result.Case = CompletionCase.Undefined;
                }

                var exprStatement = Result.AffectedNode as IExpressionStatement;
                if (exprStatement != null && exprStatement.Expression != null)
                {
                    Result.AffectedNode = exprStatement.Expression;
                }
            }

            private static bool IsBraceOrParenthesis(ITreeNode tNode)
            {
                var isLBrace = CSharpTokenType.LBRACE == tNode.GetTokenType();
                var isRBrace = CSharpTokenType.RBRACE == tNode.GetTokenType();
                var isLPara = CSharpTokenType.LPARENTH == tNode.GetTokenType();
                var isRPara = CSharpTokenType.RPARENTH == tNode.GetTokenType();
                var b = isLBrace || isRBrace || isLPara || isRPara;
                return b;
            }

            private ITreeNode SelectBetterNodesThanErrors(ITreeNode n)
            {
                if (n.Parent is IMethodDeclaration)
                {
                    return n.Parent;
                }
                return n;
            }

            private void SetCaseForTriggerInMethodSignature(ITreeNode tNode)
            {
                if (Result.AffectedNode is IMethodDeclaration)
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
                if (Result.AffectedNode is ICSharpTypeDeclaration)
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

            private static ITreeNode PossiblyStepDown(ITreeNode n)
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

                return n;
            }

            private CompletionTargetMarker FindInterestingNode(ITreeNode n)
            {
                ITreeNode handler = null;
                var c = CompletionCase.Undefined;

                // error case
                if (n == null)
                {
                    return new CompletionTargetMarker {Case = CompletionCase.Invalid};
                }

                // base cases
                if (IsInteresting(n))
                {
                    handler = n as ICSharpTreeNode;
                }

                if (IsBraceOrParenthesis(n))
                {
                    return FindInterestingNode(n.Parent);
                }

                if (handler == null && FindInterestingSibling(n, true, out handler))
                {
                    c = CompletionCase.EmptyCompletionAfter;
                }
                if (handler == null && FindInterestingSibling(n, false, out handler))
                {
                    c = CompletionCase.EmptyCompletionBefore;
                }

                // recursion
                return handler != null
                    ? new CompletionTargetMarker {AffectedNode = handler, Case = c}
                    : FindInterestingNode(n.Parent);
            }

            private static bool IsNonWhitespaceSiblingABrace(ITreeNode n, bool goBack)
            {
                if (!n.IsWhitespaceToken())
                {
                    return false;
                }

                while ((n = goBack ? n.PrevSibling : n.NextSibling) != null)
                {
                    if (!n.IsWhitespaceToken())
                    {
                        return goBack
                            ? CSharpTokenType.LBRACE == n.GetTokenType()
                            : CSharpTokenType.RBRACE == n.GetTokenType();
                    }
                }
                return false;
            }

            private bool FindInterestingSibling(ITreeNode n, bool goBack, out ITreeNode target)
            {
                while ((n = goBack ? n.PrevSibling : n.NextSibling) != null)
                {
                    if (IsInteresting(n))
                    {
                        target = n;
                        return true;
                    }
                }
                target = null;
                return false;
            }

            private bool IsInteresting(ITreeNode n)
            {
                var isInteresting = _interestingNodeTypes.Any(t => t.IsInstanceOfType(n));
                var shouldBeExcluded = _excludedNodeTypes.Any(t => t.IsInstanceOfType(n));
                return isInteresting && !shouldBeExcluded;
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
                    Result.AffectedNode = null;
                    Result.Case = CompletionCase.Undefined;
                    return;
                }

                var stmt = target as ICSharpStatement;
                if (stmt != null)
                {
                    Result.AffectedNode = stmt;
                    Result.Case = CompletionCase.EmptyCompletionAfter;
                    return;
                }

                var csExpr = target as ICSharpExpression;
                if (csExpr != null)
                {
                    Result.AffectedNode = csExpr;
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
                            Result.AffectedNode = ss.Statements.LastOrDefault();
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
                            Result.AffectedNode = scl;
                            Result.Case = CompletionCase.InBody;
                        }
                        else
                        {
                            Result.AffectedNode = ss.Statements.First();
                            Result.Case = CompletionCase.EmptyCompletionBefore;
                        }
                    }
                    else if (expr != null)
                    {
                        Result.AffectedNode = prev.FirstChild as ICSharpTreeNode;
                        Result.Case = CompletionCase.EmptyCompletionAfter;
                    }
                    else if (decl != null && HasError(prev))
                    {
                        Result.Case = CompletionCase.Undefined;
                        var multi = decl.Declaration as IMultipleLocalVariableDeclaration;
                        Result.AffectedNode = multi != null ? multi.DeclaratorsEnumerable.Last() : prev;
                    }

                    else if (decl != null)
                    {
                        Result.Case = CompletionCase.EmptyCompletionAfter;
                        var multi = decl.Declaration as IMultipleLocalVariableDeclaration;
                        Result.AffectedNode = multi != null ? multi.DeclaratorsEnumerable.Last() : prev;
                    }
                    else if (isAssign && tpdecl != null)
                    {
                        Result.Case = CompletionCase.Undefined;
                        Result.AffectedNode = tpdecl;
                    }
                    else
                    {
                        Result.AffectedNode = prev;
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
                        Result.AffectedNode = multi != null ? multi.DeclaratorsEnumerable.Last() : next;
                    }
                    else
                    {
                        Result.AffectedNode = next;
                        Result.Case = CompletionCase.EmptyCompletionBefore;
                    }
                }
                else
                {
                    Result.Case = CompletionCase.InBody;
                    Result.AffectedNode = FindNonBlockParent(target);
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
                    return Result.AffectedNode;
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
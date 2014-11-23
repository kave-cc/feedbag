﻿/*
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
 * 
 * Contributors:
 *    - Dennis Albrecht
 *    - Uli Fahrer
 */

using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.Util;
using KaVE.Model.SSTs.Blocks;
using KaVE.Model.SSTs.Declarations;
using KaVE.Model.SSTs.Statements;
using KaVE.VsFeedbackGenerator.Analysis.Transformer.Context;
using KaVE.VsFeedbackGenerator.Utils.Names;

namespace KaVE.VsFeedbackGenerator.Analysis.Transformer
{
    public class SSTScopeTransformer : BaseSSTTransformer<ScopeTransformerContext>
    {
        public override void VisitBlock(IBlock blockParam, ScopeTransformerContext context)
        {
            blockParam.Statements.ForEach(s => s.Accept(this, context));
        }

        public override void VisitCSharpExpression(ICSharpExpression cSharpExpressionParam,
            ScopeTransformerContext context)
        {
            cSharpExpressionParam.CollectSideEffects(context);
        }

        public override void VisitDeclarationStatement(IDeclarationStatement declarationStatementParam,
            ScopeTransformerContext context)
        {
            declarationStatementParam.Declaration.Accept(this, context);
        }

        public override void VisitDoStatement(IDoStatement doStatementParam, ScopeTransformerContext context)
        {
            var doLoop = new DoLoop {Condition = doStatementParam.Condition.GetScopedReferences(context)};
            doLoop.Body.AddRange(doStatementParam.Body.GetScope(context).Body);
            context.Scope.Body.Add(doLoop);
        }

        public override void VisitExpressionStatement(IExpressionStatement expressionStatementParam,
            ScopeTransformerContext context)
        {
            expressionStatementParam.Expression.Accept(this, context);
        }

        public override void VisitForeachStatement(IForeachStatement foreachStatementParam,
            ScopeTransformerContext context)
        {
            var foreachLoop = new ForEachLoop
            {
                Decl =
                    new VariableDeclaration(
                        foreachStatementParam.IteratorName,
                        foreachStatementParam.IteratorDeclaration.DeclaredElement.Type.GetName()),
                LoopedIdentifier = foreachStatementParam.Collection.GetArgument(context)
            };
            foreachLoop.Body.AddRange(foreachStatementParam.Body.GetScope(context).Body);
            context.Scope.Body.Add(foreachLoop);
        }

        public override void VisitForInitializer(IForInitializer forInitializerParam, ScopeTransformerContext context)
        {
            if (forInitializerParam.Declaration != null)
            {
                forInitializerParam.Declaration.Accept(this, context);
            }
            forInitializerParam.Expressions.ForEach(e => e.Accept(this, context));
        }

        public override void VisitForIterator(IForIterator forIteratorParam, ScopeTransformerContext context)
        {
            forIteratorParam.Expressions.ForEach(e => e.Accept(this, context));
        }

        public override void VisitForStatement(IForStatement forStatementParam, ScopeTransformerContext context)
        {
            var forLoop = new ForLoop {Condition = forStatementParam.Condition.GetScopedReferences(context)};
            forLoop.Init.AddRange(forStatementParam.Initializer.GetScope(context).Body);
            forLoop.Step.AddRange(forStatementParam.Iterators.GetScope(context).Body);
            forLoop.Body.AddRange(forStatementParam.Body.GetScope(context).Body);
            context.Scope.Body.Add(forLoop);
        }

        public override void VisitIfStatement(IIfStatement ifStatementParam, ScopeTransformerContext context)
        {
            var ifBlock = new IfElseBlock {Condition = ifStatementParam.Condition.GetReferences(context)};
            ifBlock.Then.AddRange(ifStatementParam.Then.GetScope(context).Body);
            if (ifStatementParam.Else != null)
            {
                ifBlock.Else.AddRange(ifStatementParam.Else.GetScope(context).Body);
            }
            context.Scope.Body.Add(ifBlock);
        }

        public override void VisitInvocationExpression(IInvocationExpression invocationExpressionParam,
            ScopeTransformerContext context)
        {
            HandleInvocationExpression(
                invocationExpressionParam,
                context,
                (callee, method, args, retType) =>
                    context.Scope.Body.Add(new InvocationStatement(callee.CreateInvocation(method, args))));
        }

        public override void VisitLocalVariableDeclaration(ILocalVariableDeclaration localVariableDeclarationParam,
            ScopeTransformerContext context)
        {
            base.VisitLocalVariableDeclaration(localVariableDeclarationParam, context);

            if (localVariableDeclarationParam.Initializer is IExpressionInitializer)
            {
                var expression = (localVariableDeclarationParam.Initializer as IExpressionInitializer).Value;
                var dest = localVariableDeclarationParam.NameIdentifier.Name;
                expression.ProcessAssignment(context, dest);
            }
        }

        public override void VisitMethodDeclaration(IMethodDeclaration methodDeclarationParam,
            ScopeTransformerContext context)
        {
            methodDeclarationParam.Body.Accept(this, context);
        }

        public override void VisitMultipleDeclarationMember(IMultipleDeclarationMember multipleDeclarationMemberParam,
            ScopeTransformerContext context)
        {
            var name = multipleDeclarationMemberParam.NameIdentifier.Name;
            var type = multipleDeclarationMemberParam.Type.GetName();
            context.Scope.Body.Add(new VariableDeclaration(name, type));
        }

        public override void VisitMultipleLocalVariableDeclaration(
            IMultipleLocalVariableDeclaration multipleLocalVariableDeclarationParam,
            ScopeTransformerContext context)
        {
            multipleLocalVariableDeclarationParam.Declarators.ForEach(d => d.Accept(this, context));
        }

        public override void VisitReturnStatement(IReturnStatement returnStatementParam, ScopeTransformerContext context)
        {
            context.Scope.Body.Add(new ReturnStatement {Expression = returnStatementParam.Value.GetReferences(context)});
        }

        public override void VisitThrowStatement(IThrowStatement throwStatementParam, ScopeTransformerContext context)
        {
            context.Scope.Body.Add(new ThrowStatement {Exception = throwStatementParam.Exception.Type().GetName()});
        }

        public override void VisitUsingStatement(IUsingStatement usingStatementParam, ScopeTransformerContext context)
        {
            var usingBlock = new UsingBlock();
            usingBlock.Identifier = usingStatementParam.Expressions.GetReference(context);
            usingBlock.Body.AddRange(usingStatementParam.Body.GetScope(context).Body);
            context.Scope.Body.Add(usingBlock);
        }

        public override void VisitWhileStatement(IWhileStatement whileStatementParam, ScopeTransformerContext context)
        {
            var whileLoop = new WhileLoop {Condition = whileStatementParam.Condition.GetScopedReferences(context)};
            whileLoop.Body.AddRange(whileStatementParam.Body.GetScope(context).Body);
            context.Scope.Body.Add(whileLoop);
        }

        #region Redirections

        public override void VisitOperatorExpression(IOperatorExpression operatorExpressionParam,
            ScopeTransformerContext context)
        {
            VisitCSharpExpression(operatorExpressionParam, context);
        }

        public override void VisitAssignmentExpression(IAssignmentExpression assignmentExpressionParam,
            ScopeTransformerContext context)
        {
            VisitOperatorExpression(assignmentExpressionParam, context);
        }

        public override void VisitPostfixOperatorExpression(IPostfixOperatorExpression postfixOperatorExpressionParam,
            ScopeTransformerContext context)
        {
            VisitOperatorExpression(postfixOperatorExpressionParam, context);
        }

        public override void VisitPrefixOperatorExpression(IPrefixOperatorExpression prefixOperatorExpressionParam,
            ScopeTransformerContext context)
        {
            VisitOperatorExpression(prefixOperatorExpressionParam, context);
        }

        #endregion
    }
}
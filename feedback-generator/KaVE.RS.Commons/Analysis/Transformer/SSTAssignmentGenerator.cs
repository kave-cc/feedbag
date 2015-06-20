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
 */

using JetBrains.ReSharper.Psi.CSharp.Tree;
using KaVE.Commons.Model.SSTs.Impl;
using KaVE.Commons.Model.SSTs.Impl.Expressions.Simple;
using KaVE.RS.Commons.Analysis.Transformer.Context;

namespace KaVE.RS.Commons.Analysis.Transformer
{
    public class SSTAssignmentGenerator : BaseSSTTransformer<AssignmentGeneratorContext>
    {
        public override void VisitArrayCreationExpression(IArrayCreationExpression arrayCreationExpressionParam,
            AssignmentGeneratorContext context)
        {
            context.Scope.Body.Add(
                SSTUtil.AssignmentToLocal(
                    context.Dest,
                    arrayCreationExpressionParam.ArrayInitializer.GetReferences(context)));
        }

        public override void VisitAsExpression(IAsExpression asExpressionParam, AssignmentGeneratorContext context)
        {
            context.Scope.Body.Add(
                SSTUtil.AssignmentToLocal(context.Dest, asExpressionParam.Operand.GetReferences(context)));
        }

        public override void VisitAssignmentExpression(IAssignmentExpression assignmentExpressionParam,
            AssignmentGeneratorContext context)
        {
            var dest = assignmentExpressionParam.Dest.GetReference(context);
            assignmentExpressionParam.Source.ProcessAssignment(context, dest);
            context.Scope.Body.Add(SSTUtil.AssignmentToLocal(context.Dest, SSTUtil.ComposedExpression(dest)));
        }

        public override void VisitBinaryExpression(IBinaryExpression binaryExpressionParam,
            AssignmentGeneratorContext context)
        {
            context.Scope.Body.Add(
                SSTUtil.AssignmentToLocal(context.Dest, binaryExpressionParam.GetReferences(context)));
        }

        public override void VisitCastExpression(ICastExpression castExpressionParam, AssignmentGeneratorContext context)
        {
            if (castExpressionParam.Op != null)
            {
                context.Scope.Body.Add(
                    SSTUtil.AssignmentToLocal(context.Dest, castExpressionParam.Op.GetReferences(context)));
            }
        }

        public override void VisitConditionalTernaryExpression(
            IConditionalTernaryExpression conditionalTernaryExpressionParam,
            AssignmentGeneratorContext context)
        {
            context.Scope.Body.Add(
                SSTUtil.AssignmentToLocal(
                    context.Dest,
                    null));
            //new IfElseExpression
            //      {
            //        Condition = conditionalTernaryExpressionParam.ConditionOperand.GetScopedReferences(context),
            //      ThenExpression = conditionalTernaryExpressionParam.ThenResult.GetScopedReferences(context),
            //    ElseExpression = conditionalTernaryExpressionParam.ElseResult.GetScopedReferences(context)
            //         }));
        }

        public override void VisitCSharpLiteralExpression(ICSharpLiteralExpression cSharpLiteralExpressionParam,
            AssignmentGeneratorContext context)
        {
            context.Scope.Body.Add(SSTUtil.AssignmentToLocal(context.Dest, new ConstantValueExpression()));
        }

        public override void VisitDefaultExpression(IDefaultExpression defaultExpressionParam,
            AssignmentGeneratorContext context)
        {
            context.Scope.Body.Add(SSTUtil.AssignmentToLocal(context.Dest, new ConstantValueExpression()));
        }

        public override void VisitInvocationExpression(IInvocationExpression invocationExpressionParam,
            AssignmentGeneratorContext context)
        {
            HandleInvocationExpression(
                invocationExpressionParam,
                context,
                (callee, method, args, retType) =>
                    context.Scope.Body.Add(
                        SSTUtil.AssignmentToLocal(context.Dest, callee.CreateInvocation(method, args))));
        }

        public override void VisitIsExpression(IIsExpression isExpressionParam, AssignmentGeneratorContext context)
        {
            context.Scope.Body.Add(
                SSTUtil.AssignmentToLocal(context.Dest, isExpressionParam.Operand.GetReferences(context)));
        }

        /*public override void VisitObjectCreationExpression(IObjectCreationExpression objectCreationExpressionParam,
            AssignmentGeneratorContext context)
        {
            if (objectCreationExpressionParam.Reference != null)
            {
                var element = objectCreationExpressionParam.Reference.Resolve().DeclaredElement as ConstructorElement;
                if (element != null)
                {
                    var typeName = element.GetContainingType().GetName<ITypeName>(element.IdSubstitution);
                    var typeNames = element.Parameters.Select(p => p.GetName<ITypeName>(element.IdSubstitution));
                    var methodName =
                        MethodName.Get("[System.Void, mscorlib, 4.0.0.0] [System.Object, mscorlib, 4.0.0.0]..ctor()");
                    var arguments = objectCreationExpressionParam.ArgumentList.GetArguments(context);
                    context.Scope.Body.Add(
                        new Assignment(context.Dest, new InvocationExpression(methodName, arguments)));
                }
            }
        }*/

        public override void VisitParenthesizedExpression(IParenthesizedExpression parenthesizedExpressionParam,
            AssignmentGeneratorContext context)
        {
            parenthesizedExpressionParam.Expression.Accept(this, context);
        }

        public override void VisitPostfixOperatorExpression(IPostfixOperatorExpression postfixOperatorExpressionParam,
            AssignmentGeneratorContext context)
        {
            var reference = postfixOperatorExpressionParam.Operand.GetReference(context);
            context.Scope.Body.Add(SSTUtil.AssignmentToLocal(context.Dest, SSTUtil.ComposedExpression(reference)));
        }

        public override void VisitPrefixOperatorExpression(IPrefixOperatorExpression prefixOperatorExpressionParam,
            AssignmentGeneratorContext context)
        {
            var reference = prefixOperatorExpressionParam.Operand.GetReference(context);
            context.Scope.Body.Add(SSTUtil.AssignmentToLocal(context.Dest, SSTUtil.ComposedExpression(reference)));
        }

        public override void VisitReferenceExpression(IReferenceExpression referenceExpressionParam,
            AssignmentGeneratorContext context)
        {
            var name = referenceExpressionParam.NameIdentifier.Name;
            context.Scope.Body.Add(SSTUtil.AssignmentToLocal(context.Dest, SSTUtil.ComposedExpression(name)));
        }

        public override void VisitThisExpression(IThisExpression thisExpressionParam, AssignmentGeneratorContext context)
        {
            context.Scope.Body.Add(SSTUtil.AssignmentToLocal(context.Dest, SSTUtil.ComposedExpression("this")));
        }

        public override void VisitTypeofExpression(ITypeofExpression typeofExpressionParam,
            AssignmentGeneratorContext context)
        {
            context.Scope.Body.Add(SSTUtil.AssignmentToLocal(context.Dest, new ConstantValueExpression()));
        }

        public override void VisitUnaryOperatorExpression(IUnaryOperatorExpression unaryOperatorExpressionParam,
            AssignmentGeneratorContext context)
        {
            context.Scope.Body.Add(
                SSTUtil.AssignmentToLocal(context.Dest, unaryOperatorExpressionParam.Operand.GetReferences(context)));
        }
    }
}
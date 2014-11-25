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
 */

using System.Collections.Generic;
using System.Linq;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using KaVE.Model.Names;
using KaVE.Model.SSTs;
using KaVE.Model.SSTs.Blocks;
using KaVE.Model.SSTs.Declarations;
using KaVE.Model.SSTs.Expressions;
using KaVE.Utils.Assertion;
using KaVE.VsFeedbackGenerator.Analysis.Transformer.Context;
using KaVE.VsFeedbackGenerator.Analysis.Util;
using KaVE.VsFeedbackGenerator.Utils.Names;
using NuGet;

namespace KaVE.VsFeedbackGenerator.Analysis.Transformer
{
    public static class SSTTransformerUtil
    {
        /// <summary>
        /// Constructs an Expression from a list of references. Empty list causes ConstantExpression, ComposedExpression otherwise.
        /// </summary>
        public static Expression AsExpression(this IList<string> references)
        {
            if (references.Any())
            {
                return ComposedExpression.Create(references.Distinct().ToArray());
            }
            return new ConstantExpression();
        }

        /// <summary>
        /// Traverses the given (expression-)node and collects references. If the transformation causes declarations, invocations, assignments, etc. the result is a BlockExpression.
        /// </summary>
        public static Expression GetScopedReferences(this ICSharpTreeNode node, ITransformerContext context)
        {
            var scope = context.Factory.Scope();
            var refCollectorContext = new ReferenceCollectorContext(context, scope);
            node.Accept(context.Factory.ReferenceCollector(), refCollectorContext);
            if (scope.Body.Any())
            {
                var block = new BlockExpression {Value = refCollectorContext.References.ToArray()};
                block.Body.AddRange(refCollectorContext.Scope.Body);
                return block;
            }
            return refCollectorContext.References.AsExpression();
        }

        /// <summary>
        /// Traverses the given (expression-)node and collects references. If the transformation causes declarations, invocations, assignments, etc., such statements are added to the given scope.
        /// </summary>
        public static Expression GetReferences(this ICSharpTreeNode node, ITransformerContext context)
        {
            var refCollectorContext = new ReferenceCollectorContext(context);
            node.Accept(context.Factory.ReferenceCollector(), refCollectorContext);
            return refCollectorContext.References.AsExpression();
        }

        /// <summary>
        /// Traverses the given node and returns a reference to the result value.
        /// </summary>
        public static string GetReference(this ICSharpTreeNode node, ITransformerContext context)
        {
            var refCollectorContext = new ReferenceCollectorContext(context);
            node.Accept(context.Factory.ReferenceCollector(), refCollectorContext);
            Asserts.That(refCollectorContext.References.Count == 1);
            return refCollectorContext.References[0];
        }

        /// <summary>
        /// Traverses the given list of (expression-)nodes and collects the side-effects for all but the last one. The last one in analysed towards it's side-effects and result value.
        /// </summary>
        public static string GetReference(this IEnumerable<ICSharpTreeNode> nodes, ITransformerContext context)
        {
            var enumerator = nodes.GetEnumerator();
            var node = default(ICSharpTreeNode);
            while (enumerator.MoveNext())
            {
                if (node != null)
                {
                    node.CollectSideEffects(context);
                }
                node = enumerator.Current;
            }
            return node.GetReference(context);
        }

        /// <summary>
        /// Traverses the given (argumentlist-)node and returns a list of argument references. Every found expression that isn't a reference itself is transformed into a reference.
        /// </summary>
        public static string[] GetArguments(this ICSharpTreeNode node, ITransformerContext context)
        {
            var argCollectorContext = new ArgumentCollectorContext(context);
            node.Accept(context.Factory.ArgumentCollector(), argCollectorContext);
            return argCollectorContext.Arguments.ToArray();
        }

        /// <summary>
        /// Traverses the given (block-)node and returns the collected statements.
        /// </summary>
        public static IScope GetScope(this ICSharpTreeNode node, ITransformerContext context)
        {
            var scopeContext = new ScopeTransformerContext(context, context.Factory.Scope());
            node.Accept(context.Factory.ScopeTransformer(), scopeContext);
            return scopeContext.Scope;
        }

        /// <summary>
        /// Traverses the given right-hand-side of some assignment to the given variable and adds an assignment-node to the given context.
        /// </summary>
        public static void ProcessAssignment(this ICSharpTreeNode node, ITransformerContext context, string dest)
        {
            node.Accept(context.Factory.AssignmentGenerator(), new AssignmentGeneratorContext(context, dest));
        }

        /// <summary>
        /// Traverses the given (expression-)node and adds all side-effects to the given scope. The result value is ignored.
        /// </summary>
        public static void CollectSideEffects(this ICSharpTreeNode node, ITransformerContext context)
        {
            node.Accept(context.Factory.ReferenceCollector(), new ReferenceCollectorContext(context));
        }

        /// <summary>
        /// Constructs InvocationExpressions either as static call if callee isn't given or as non-static call otherwise.
        /// </summary>
        public static InvocationExpression CreateInvocation(this string callee, IMethodName method, string[] args)
        {
            if (callee == null)
            {
                return new InvocationExpression(method, args);
            }
            return new InvocationExpression(callee, method, args);
        }

        public static CatchBlock GetCatchBlock(this ICatchClause catchClauseParam, ITransformerContext context)
        {
            var catchBlock = new CatchBlock();
            if (catchClauseParam is ISpecificCatchClause)
            {
                var specificCatchClause = catchClauseParam as ISpecificCatchClause;
                string identifier = null;
                if (specificCatchClause.ExceptionDeclaration != null)
                {
                    identifier = specificCatchClause.ExceptionDeclaration.DeclaredName;
                }
                catchBlock.Exception = new VariableDeclaration(identifier, catchClauseParam.ExceptionType.GetName());
            }
            catchBlock.Body.AddRange(catchClauseParam.Body.GetScope(context).Body);
            return catchBlock;
        }
    }
}
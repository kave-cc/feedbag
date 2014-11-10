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
using JetBrains.Util;
using KaVE.Model.Names;
using KaVE.Model.SSTs.Declarations;
using KaVE.Model.SSTs.Expressions;
using KaVE.Model.SSTs.Statements;
using KaVE.VsFeedbackGenerator.Utils.Names;

namespace KaVE.VsFeedbackGenerator.Analysis
{
    public class SSTReferenceCollector : BaseSSTTransformer
    {
        private readonly IList<string> _references = new List<string>();

        public SSTReferenceCollector(MethodDeclaration declaration) : base(declaration) {}

        public string[] References
        {
            get { return Enumerable.ToArray(_references); }
        }

        public override void VisitBinaryExpression(IBinaryExpression binaryExpressionParam)
        {
            binaryExpressionParam.LeftOperand.Accept(this);
            binaryExpressionParam.RightOperand.Accept(this);
        }

        public override void VisitReferenceExpression(IReferenceExpression referenceExpressionParam)
        {
            _references.Add(referenceExpressionParam.NameIdentifier.Name);
        }

        public override void VisitArrayInitializer(IArrayInitializer arrayInitializerParam)
        {
            arrayInitializerParam.ElementInitializers.ForEach(i => i.Accept(this));
        }

        public override void VisitExpressionInitializer(IExpressionInitializer expressionInitializerParam)
        {
            expressionInitializerParam.Value.Accept(this);
        }

        public override void VisitInvocationExpression(IInvocationExpression invocationExpressionParam)
        {
            var invokedExpression = invocationExpressionParam.InvokedExpression as IReferenceExpression;
            if (invocationExpressionParam.Reference != null &&
                invokedExpression != null &&
                invokedExpression.QualifierExpression is IReferenceExpression)
            {
                var collector = new SSTArgumentCollector(Declaration);

                var methodName = invocationExpressionParam.Reference.ResolveMethod().GetName<IMethodName>();
                invocationExpressionParam.ArgumentList.Accept(collector);
                var name = (invokedExpression.QualifierExpression as IReferenceExpression).NameIdentifier.Name;

                var tmp = Declaration.GetNewTempVariable();
                Declaration.Body.Add(new VariableDeclaration(tmp, invocationExpressionParam.Type().GetName()));
                Declaration.Body.Add(new Assignment(tmp, new InvocationExpression(name, methodName, collector.Arguments)));
                _references.Add(tmp);
            }
        }

        public override void VisitThisExpression(IThisExpression thisExpressionParam)
        {
            _references.Add("this");
        }
    }
}
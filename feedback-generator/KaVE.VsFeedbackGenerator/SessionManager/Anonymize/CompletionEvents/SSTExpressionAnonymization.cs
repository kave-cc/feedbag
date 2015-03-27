/*
 * Copyright 2014 Technische Universit�t Darmstadt
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
 *    - Sebastian Proksch
 */

using System.Collections.Generic;
using System.Linq;
using KaVE.JetBrains.Annotations;
using KaVE.Model.Collections;
using KaVE.Model.SSTs;
using KaVE.Model.SSTs.Expressions;
using KaVE.Model.SSTs.Expressions.Assignable;
using KaVE.Model.SSTs.Expressions.LoopHeader;
using KaVE.Model.SSTs.Expressions.Simple;
using KaVE.Model.SSTs.Impl.Expressions.Assignable;
using KaVE.Model.SSTs.Impl.Expressions.Simple;
using KaVE.Model.SSTs.Impl.Visitor;
using KaVE.Model.SSTs.References;
using KaVE.Utils.Assertion;

namespace KaVE.VsFeedbackGenerator.SessionManager.Anonymize.CompletionEvents
{
    public class SSTExpressionAnonymization : AbstractNodeVisitor<int, IExpression>
    {
        private readonly SSTReferenceAnonymization _refAnon;

        public SSTExpressionAnonymization(SSTReferenceAnonymization refAnon)
        {
            _refAnon = refAnon;
        }

        public override IExpression Visit(ICompletionExpression entity, int context)
        {
            return new CompletionExpression
            {
                ObjectReference = _refAnon.Anonymize(entity.ObjectReference),
                TypeReference = entity.TypeReference.ToAnonymousName(),
                Token = entity.Token
            };
        }

        public override IExpression Visit(IComposedExpression expr, int context)
        {
            return new ComposedExpression
            {
                References = Anonymize(expr.References)
            };
        }

        private IKaVEList<IVariableReference> Anonymize(IEnumerable<IVariableReference> references)
        {
            return Lists.NewListFrom(references.Select(r => (IVariableReference) _refAnon.Visit(r, 0)));
        }

        public override IExpression Visit(IConstantValueExpression expr, int context)
        {
            return new ConstantValueExpression
            {
                Value = expr.Value.ToHash()
            };
        }

        public override IExpression Visit(IIfElseExpression expr, int context)
        {
            return new IfElseExpression
            {
                Condition = Anonymize(expr.Condition),
                ThenExpression = Anonymize(expr.ThenExpression),
                ElseExpression = Anonymize(expr.ElseExpression)
            };
        }

        public override IExpression Visit(IInvocationExpression entity, int context)
        {
            return new InvocationExpression
            {
                Reference = _refAnon.Anonymize(entity.Reference),
                MethodName = entity.MethodName.ToAnonymousName(),
                Parameters = Anonymize(entity.Parameters)
            };
        }

        public override IExpression Visit(ILambdaExpression expr, int context)
        {
            throw new AssertException("not available");
        }

        public override IExpression Visit(ILoopHeaderBlockExpression expr, int context)
        {
            throw new AssertException("not available");
        }

        public override IExpression Visit(INullExpression expr, int context)
        {
            return new NullExpression();
        }

        public override IExpression Visit(IReferenceExpression expr, int context)
        {
            return new ReferenceExpression
            {
                Reference = expr.Reference.Accept(_refAnon, 0)
            };
        }

        public override IExpression Visit(IUnknownExpression expr, int context)
        {
            return new UnknownExpression();
        }

        private IKaVEList<ISimpleExpression> Anonymize(IEnumerable<ISimpleExpression> exprs)
        {
            return Lists.NewListFrom(exprs.Select(Anonymize));
        }

        private ISimpleExpression Anonymize([NotNull] ISimpleExpression expr)
        {
            return (ISimpleExpression) expr.Accept(this, 0);
        }
    }
}
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

using KaVE.Commons.Model.SSTs;
using KaVE.Commons.Model.SSTs.Expressions.Assignable;
using KaVE.Commons.Model.SSTs.Impl.References;
using KaVE.Commons.Model.SSTs.Impl.Statements;
using KaVE.Commons.Model.SSTs.Impl.Visitor;
using KaVE.Commons.Model.SSTs.References;
using KaVE.Commons.Model.SSTs.Statements;
using KaVE.Commons.Utils.Naming;
using KaVE.JetBrains.Annotations;

namespace KaVE.VS.FeedbackGenerator.SessionManager.Anonymize.CompletionEvents
{
    public class SSTReferenceAnonymization : AbstractNodeVisitor<int, IReference>
    {
        public SSTExpressionAnonymization ExprAnon { get; set; }

        public override IReference Visit(IEventReference eventRef, int context)
        {
            return new EventReference
            {
                Reference = Anonymize(eventRef.Reference),
                EventName = eventRef.EventName.ToAnonymousName()
            };
        }

        public override IReference Visit(IFieldReference eventRef, int context)
        {
            return new FieldReference
            {
                Reference = Anonymize(eventRef.Reference),
                FieldName = eventRef.FieldName.ToAnonymousName()
            };
        }

        public override IReference Visit(IIndexAccessReference idxRef, int context)
        {
            return new IndexAccessReference
            {
                Expression = (IIndexAccessExpression) ExprAnon.Visit(idxRef.Expression, context)
            };
        }

        public override IReference Visit(IMethodReference eventRef, int context)
        {
            return new MethodReference
            {
                Reference = Anonymize(eventRef.Reference),
                MethodName = eventRef.MethodName.ToAnonymousName()
            };
        }

        public override IReference Visit(IPropertyReference eventRef, int context)
        {
            return new PropertyReference
            {
                Reference = Anonymize(eventRef.Reference),
                PropertyName = eventRef.PropertyName.ToAnonymousName()
            };
        }

        public override IReference Visit(IUnknownReference eventRef, int context)
        {
            return new UnknownReference();
        }

        public override IReference Visit(IVariableReference aref, int context)
        {
            return new VariableReference
            {
                Identifier = aref.Identifier.ToHash()
            };
        }

        public virtual IVariableDeclaration Anonymize([NotNull] IVariableDeclaration d)
        {
            return new VariableDeclaration
            {
                Reference = Anonymize(d.Reference),
                Type = d.Type.ToAnonymousName()
            };
        }

        public virtual IVariableReference Anonymize(IVariableReference vref)
        {
            if (vref == null)
            {
                return null;
            }
            return (IVariableReference) Visit(vref, 0);
        }


        public virtual IAssignableReference Anonymize([NotNull] IAssignableReference aref)
        {
            return (IAssignableReference) aref.Accept(this, 0);
        }
    }
}
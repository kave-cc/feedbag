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
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Util;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.Threading;
using KaVE.Commons.Model.Naming;
using KaVE.JetBrains.Annotations;
using KaVE.RS.Commons.Utils.Naming;

namespace KaVE.VS.FeedbackGenerator.Generators.Navigation
{
    public interface INavigationUtils
    {
        // [NotNull]
        // IName GetLocation([NotNull] ITextControl textControl);

        // [NotNull]
        // IName GetLocation([NotNull] ITreeNode psiNode);

        void GetLocation(ITextControl textControl, Action<IName> cb);
        void GetTargetAndLocation(ITextControl textControl, Action<IName, IName> cb);
    }

    [SolutionComponent]
    internal class NavigationUtils : INavigationUtils
    {
        [NotNull]
        private readonly ISolution _solution;

        public NavigationUtils([NotNull] ISolution solution)
        {
            _solution = solution;
        }

        public void GetLocation(ITextControl textControl, Action<IName> cb)
        {
            ReentrancyGuard.Current.ExecuteOrQueue(
                "NavigationUtils.GetLocation",
                () =>
                {
                    var treeNode = GetTreeNode(textControl);
                    if (treeNode != null)
                    {
                        var loc = GetLocation(treeNode);
                        cb(loc);
                    }
                });
        }

        public void GetTargetAndLocation(ITextControl textControl, Action<IName, IName> cb)
        {
            ReentrancyGuard.Current.ExecuteOrQueue(
                "NavigationUtils.GetTargetAndLocation",
                () =>
                {
                    var treeNode = GetTreeNode(textControl);
                    if (treeNode != null)
                    {
                        var target = GetTarget(treeNode);
                        var loc = GetLocation(treeNode);
                        cb(target, loc);
                    }
                });
        }

        [CanBeNull]
        private ITreeNode GetTreeNode([NotNull] ITextControl textControl)
        {
            /* ITreeNode treeNode = null;
             if (ReentrancyGuard.Current.CanExecuteNow)
             {
                 ReadLockCookie.GuardedExecute(
                     () => { treeNode = TextControlToPsi.GetElement<ITreeNode>(_solution, textControl); });
             }*/
            var treeNode = TextControlToPsi.GetElement<ITreeNode>(_solution, textControl);

            return treeNode;
        }

        public IName GetTarget(ITreeNode psiNode)
        {
            var declaredElement = TryGetDeclaredElement(psiNode);

            if (declaredElement == null && psiNode.Parent != null)
            {
                declaredElement = TryGetDeclaredElement(psiNode.Parent);
            }

            if (declaredElement == null && psiNode.PrevSibling != null)
            {
                declaredElement = TryGetDeclaredElement(psiNode.PrevSibling);
            }

            if (declaredElement == null && psiNode.Parent != null &&
                psiNode.Parent.PrevSibling != null)
            {
                declaredElement = TryGetDeclaredElement(psiNode.Parent.PrevSibling);
            }

            return declaredElement == null
                ? Names.UnknownGeneral
                : declaredElement.GetName<IName>(declaredElement.GetIdSubstitutionSafe());
        }

        public IName GetLocation(ITreeNode psiNode)
        {
            var locationName = Names.UnknownGeneral;

            var surroundingMethodDeclaration = psiNode.GetContainingNode<IMethodDeclaration>();
            if (surroundingMethodDeclaration != null)
            {
                locationName = surroundingMethodDeclaration.GetName();
            }
            else
            {
                var surroundingTypeDeclaration = psiNode.GetContainingNode<ITypeDeclaration>();
                if (surroundingTypeDeclaration != null && surroundingTypeDeclaration.DeclaredElement != null)
                {
                    locationName = surroundingTypeDeclaration.DeclaredElement.GetName();
                }
            }

            return locationName;
        }

        [Pure]
        private static IDeclaredElement TryGetDeclaredElement([NotNull] ITreeNode psiNode)
        {
            var declaration = psiNode as IDeclaration;
            if (declaration != null)
            {
                return declaration.DeclaredElement;
            }

            var referenceName = psiNode as IReferenceName;
            if (referenceName != null)
            {
                var resolvedReference = referenceName.Reference.Resolve();
                return resolvedReference.DeclaredElement;
            }

            var referenceExpression = psiNode as IReferenceExpression;
            if (referenceExpression != null)
            {
                var resolvedReference = referenceExpression.Reference.Resolve();
                return resolvedReference.DeclaredElement;
            }

            var invocationInfo = psiNode as IInvocationInfo;
            if (invocationInfo != null && invocationInfo.Reference != null)
            {
                var resolvedReference = invocationInfo.Reference.Resolve();
                return resolvedReference.DeclaredElement;
            }

            return null;
        }
    }
}
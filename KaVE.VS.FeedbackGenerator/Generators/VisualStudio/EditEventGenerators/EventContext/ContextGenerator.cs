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

using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CodeCompletion;
using JetBrains.ReSharper.Feature.Services.Util;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.TextControl;
using KaVE.Commons.Model.Events.CompletionEvents;
using KaVE.Commons.Utils.Exceptions;
using KaVE.RS.Commons.Analysis;

namespace KaVE.VS.FeedbackGenerator.Generators.VisualStudio.EditEventGenerators.EventContext
{
    [SolutionComponent]
    internal class ContextGenerator
    {
        private readonly TextControlManager _textControlManager;
        private readonly ISolution _solution;
        private readonly ILogger _logger;

        public ContextGenerator(TextControlManager textControlManager,
            IntellisenseManager intellisenseManager,
            ILogger logger)
        {
            _textControlManager = textControlManager;
            _logger = logger;
            _solution = intellisenseManager.Solution;
        }

        public Context RunContextAnalysis()
        {
            return ReadLockCookie.Execute(
                () =>
                {
                    var node = FindCurrentTreeNode();

                    if (node == null)
                    {
                        return null;
                    }

                    if (!HasSourroundingMethod(node))
                    {
                        node = FindSourroundingTypeDeclaration(node);
                    }

                    if (node == null)
                    {
                        return null;
                    }

                    return RunAnalysis(node);
                });
        }

        private ITreeNode FindCurrentTreeNode()
        {
            var textControl = _textControlManager.FocusedTextControl.Value;
            return textControl == null ? null : TextControlToPsi.GetElement<ITreeNode>(_solution, textControl);
        }

        private static bool HasSourroundingMethod(ITreeNode node)
        {
            var method = node.GetContainingNode<IMethodDeclaration>(true);
            return method != null;
        }

        private static ICSharpTypeDeclaration FindSourroundingTypeDeclaration(ITreeNode psiFile)
        {
            return psiFile.GetContainingNode<ICSharpTypeDeclaration>(true);
        }

        private Context RunAnalysis(ITreeNode node)
        {
            Context ctx = null;
            ContextAnalysis.Analyse(
                node,
                null,
                _logger,
                context => { ctx = context; },
                delegate { },
                delegate { });
            return ctx;
        }
    }
}
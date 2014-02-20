﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.CodeCompletion;
using JetBrains.ReSharper.Feature.Services.Lookup;
using JetBrains.ReSharper.Feature.Services.Tests.CSharp.FeatureServices.CodeCompletion;
using JetBrains.ReSharper.Psi;
using JetBrains.TextControl;
using KaVE.Model.Events.CompletionEvent;
using KaVE.Utils.Assertion;
using KaVE.VsFeedbackGenerator.Utils;
using NUnit.Framework;

namespace KaVE.VsFeedbackGenerator.RS8Tests
{
    internal abstract class KaVEBaseTest : CodeCompletionTestBase
    {
        private readonly List<CodeCompletionType> _myCodeCompletionTypes;

        protected KaVEBaseTest()
        {
            _myCodeCompletionTypes = new List<CodeCompletionType> {CodeCompletionType.BasicCompletion};
        }

        protected override string RelativeTestDataPath
        {
            get
            {
                var defaultNamespace = typeof (KaVEBaseTest).Namespace;
                var concreteNamespace = GetType().FullName;

                Asserts.NotNull(defaultNamespace, "KaVEBaseTest somehow moved to global namespace?!");

                string basePath;
                if (concreteNamespace.StartsWith(defaultNamespace))
                {
                    basePath = concreteNamespace.Substring(defaultNamespace.Length + 1);
                }
                else
                {
                    basePath = concreteNamespace;
                }

                return Path.Combine(basePath.Split('.'));
            }
        }

        protected override bool ExecuteAction
        {
            get { return false; }
        }

        protected void WhenCodeCompletionIsInvokedInFile(string fileName)
        {
            DoOneTest(fileName);
        }

        protected override void ExecuteCodeCompletion(Suffix suffix,
            ITextControl textControl,
            IntellisenseManager intellisenseManager,
            bool automatic,
            string documentText)
        {
            var psiSourceFile = textControl.Document.GetPsiSourceFile(Solution);
            Assert.IsNotNull(psiSourceFile, "psiSourceFile == null");
            var projectFile = psiSourceFile.ToProjectFile();
            Assert.IsNotNull(projectFile, "projectFile == null");
            var service =
                Solution.GetComponent<ILanguageManager>()
                    .TryGetService<CodeCompletionModifierProvider>(psiSourceFile.PrimaryPsiLanguage);
            var parameters = service != null
                ? service.GetModifier(_myCodeCompletionTypes)
                : CodeCompletionModifierProvider.GetModifierBasic(
                    _myCodeCompletionTypes.Take(_myCodeCompletionTypes.Count - 1).ToList(),
                    _myCodeCompletionTypes[_myCodeCompletionTypes.Count - 1]);

            var codeCompletionResult = intellisenseManager.GetCompletionResult(parameters, textControl);
            var best =
                ((SelectionStrategyWithPreferences) codeCompletionResult.SelectionStrategy).GetAllPreferredItems(
                    codeCompletionResult.GetFilteredLookupItems()).ToHashSet();
            ResultProposalCollection = GetItemsFromResult(codeCompletionResult, best).ToProposalCollection();
        }

        protected ProposalCollection ResultProposalCollection { get; private set; }
    }
}
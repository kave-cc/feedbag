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

using JetBrains.ActionManagement;
using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.UI.ActionsRevised;
using JetBrains.UI.Options;
using KaVE.VS.FeedbackGenerator.Generators;

namespace KaVE.VS.FeedbackGenerator.Menu
{
    [Action(Id, "Options...", Id = 123451)]
    public class OptionPageAction : MenuActionBase
    {
        public const string Id = "KaVE.Options";
    }

    [ShellComponent]
    public class OptionPageActionHandler : MenuActionHandlerBase<OptionPageAction>
    {
        private readonly IKaVECommandGenerator _gen;
        private readonly IActionManager _actionManager;

        public OptionPageActionHandler(IKaVECommandGenerator gen, IActionManager am) : base(am)
        {
            _gen = gen;
            _actionManager = am;
        }

        public override void Execute(IDataContext context, DelegateExecute nextExecute)
        {
            _gen.FireOpenOptions();
            _actionManager.ExecuteAction<ShowOptionsAction>();
        }
    }
}
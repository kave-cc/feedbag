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

using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.Application.UI.Actions;
using JetBrains.Application.UI.Actions.ActionManager;
using JetBrains.Application.UI.ActionsRevised.Menu;
using KaVE.VS.FeedbackGenerator.Generators;
using KaVE.VS.FeedbackGenerator.UserControls.AboutWindow;

namespace KaVE.VS.FeedbackGenerator.Menu
{
    [Action(Id, "About KaVE", Id = 21394587)]
    public class AboutAction : MenuActionBase
    {
        public const string Id = "KaVE.VsFeedbackGenerator.About";
    }

    [ShellComponent]
    public class AboutActionHandler : MenuActionHandlerBase<AboutAction>
    {
        private readonly IKaVECommandGenerator _cmdGen;

        public AboutActionHandler(IActionManager am, IKaVECommandGenerator cmdGen) : base(am)
        {
            _cmdGen = cmdGen;
        }

        public override void Execute(IDataContext context, DelegateExecute nextExecute)
        {
            _cmdGen.FireOpenAboutDialog();
            new AboutWindowControl().Show();
        }
    }
}
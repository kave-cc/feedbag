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
using JetBrains.Application.UI.ToolWindowManagement;
using JetBrains.Application.UI.UIAutomation;
using JetBrains.DataFlow;
using KaVE.JetBrains.Annotations;
using KaVE.RS.Commons;
using KaVE.RS.Commons.Settings;
using KaVE.VS.FeedbackGenerator.Generators;
using KaVE.VS.FeedbackGenerator.Utils.Export;
using KaVE.VS.FeedbackGenerator.Utils.Logging;

namespace KaVE.VS.FeedbackGenerator.SessionManager.Presentation
{
    [ShellComponent]
    public class SessionManagerWindowRegistrar
    {
        [UsedImplicitly]
        private readonly ToolWindowClass _toolWindowClass;

        public SessionManagerWindowRegistrar(Lifetime lifetime,
            ToolWindowManager toolWindowManager,
            SessionManagerWindowDescriptor descriptor,
            ActionExecutor actionExecutor,
            ILogManager logManager,
            IExporter exporter,
            ISettingsStore settingsStore,
            IKaVECommandGenerator cmdGen)
        {
            var viewModel = new FeedbackViewModel(logManager, exporter, cmdGen);
            var control = new SessionManagerControl(viewModel, actionExecutor, settingsStore, cmdGen);

            // objects are kept in fields to prevent garbage collection
            _toolWindowClass = toolWindowManager.Classes[descriptor];
            _toolWindowClass.RegisterEmptyContent(
                lifetime,
                lt =>
                {
                    var visibilitySignal = _toolWindowClass.Visible.Change;
                    visibilitySignal.Advise(lt, control.OnVisibilityChanged);
                    var wrapper = new EitherControl(control);
                    return wrapper.BindToLifetime(lt);
                });
        }
    }
}
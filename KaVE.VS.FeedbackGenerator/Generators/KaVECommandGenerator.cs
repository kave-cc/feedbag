﻿/*
 * Copyright 2017 University of Zurich
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
using JetBrains.Threading;
using KaVE.Commons.Utils;
using KaVE.JetBrains.Annotations;
using KaVE.VS.FeedbackGenerator.MessageBus;

namespace KaVE.VS.FeedbackGenerator.Generators
{
    [ShellComponent]
    public class KaVECommandGenerator : GenericCommandGenerator
    {
        public KaVECommandGenerator([NotNull] IRSEnv env,
            [NotNull] IMessageBus messageBus,
            [NotNull] IDateUtils dateUtils,
            IThreading threading) : base(env, messageBus, dateUtils, threading) { }

        public void FireDeleteDays()
        {
            Fire("KaVE.FeedBaG.DeleteDays");
        }

        public void FireDeleteEvents()
        {
            Fire("KaVE.FeedBaG.DeleteEvents");
        }

        public void FireGotoHomepage()
        {
            Fire("KaVE.FeedBaG.GotoHomepage");
        }

        public void FireGotoUploadPage()
        {
            Fire("KaVE.FeedBaG.GotoUploadPage");
        }

        public void FireOpenAboutDialog()
        {
            Fire("KaVE.FeedBaG.OpenAboutDialog");
        }

        public void FireOpenExportDialog()
        {
            Fire("KaVE.FeedBaG.OpenExportDialog");
        }

        public void FireOpenOptions()
        {
            Fire("KaVE.FeedBaG.OpenOptions");
        }

        public void FireExportIntoZip()
        {
            Fire("KaVE.FeedBaG.ExportIntoZip");
        }

        public void FireExportToServer()
        {
            Fire("KaVE.FeedBaG.ExportToServer");
        }

        public void FireReloadEvents()
        {
            Fire("KaVE.FeedBaG.ReloadEvents");
        }
    }
}
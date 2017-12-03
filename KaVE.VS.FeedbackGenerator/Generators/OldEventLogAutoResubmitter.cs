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

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Application;
using KaVE.Commons.Model;
using KaVE.Commons.Utils.Exceptions;
using KaVE.JetBrains.Annotations;
using KaVE.RS.Commons;
using KaVE.VS.Commons;
using KaVE.VS.FeedbackGenerator.Utils.Logging;

namespace KaVE.VS.FeedbackGenerator.Generators
{
    [ShellComponent]
    public class OldEventLogAutoResubmitter
    {
        public OldEventLogAutoResubmitter([NotNull] IMessageBus messageBus,
            // Dependency on EventLogger is necessary to make sure it has already been
            // created and is subscribed to the message bus.
            // ReSharper disable once UnusedParameter.Local
            [NotNull] EventLogger eventLogger,
            [NotNull] ILogger logger,
            [NotNull] FeedBaGVersionUtil versionUtil)
        {
            var pathInRoaming = Path.Combine(
                IDEEventLogFileManager.AppDataPath,
                IDEEventLogFileManager.ProjectName,
                "KaVE.VS.FeedbackGenerator",
                versionUtil.GetVariant().ToString());


            var pathWithDefaultVariant = Path.Combine(
                IDEEventLogFileManager.LocalAppDataPath,
                IDEEventLogFileManager.ProjectName,
                "KaVE.VS.FeedbackGenerator",
                "Default");

            var paths = new List<string> {pathInRoaming};

            if (versionUtil.GetVariant() == Variant.Release)
            {
                paths.Add(pathWithDefaultVariant);
            }

            Task.Factory.StartNew(() => Execute(paths, messageBus, logger));
        }

        private static void Execute(IList<string> paths, IMessageBus messageBus, ILogger logger)
        {
            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    logger.Info("Starting to migrate events from an old to a new event log path.");
                    var count = LogFileUtils.ResubmitLogs(new LogFileManager(path), messageBus);
                    logger.Info("Migrated {0} events from an old to a new event log path.", count);
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Directory.Delete(path, true);
                }
            }
        }
    }
}
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

using System.Linq;
using JetBrains.Application;
using KaVE.Commons.Model.Events;
using KaVE.Commons.Model.Events.VisualStudio;
using KaVE.Commons.Utils.Exceptions;
using KaVE.VS.FeedbackGenerator.MessageBus;
using KaVE.VS.Statistics.Calculators.BaseClasses;
using KaVE.VS.Statistics.Statistics;

namespace KaVE.VS.Statistics.Calculators
{
    [ShellComponent]
    public class BuildCalculator : StatisticCalculator<BuildStatistic>
    {
        public BuildCalculator(IStatisticListing statisticListing, IMessageBus messageBus, ILogger errorHandler)
            : base(statisticListing, messageBus, errorHandler) {}

        protected override void Calculate(BuildStatistic statistic, IDEEvent @event)
        {
            var buildEvent = @event as BuildEvent;
            if (buildEvent == null)
            {
                return;
            }

            if (buildEvent.Targets.All(target => target.Successful))
            {
                statistic.SuccessfulBuilds++;
            }
            else
            {
                statistic.FailedBuilds++;
            }
        }
    }
}
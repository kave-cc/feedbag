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

using JetBrains.Application;
using JetBrains.ReSharper.Features.Navigation.Resources;
using JetBrains.UI.ToolWindowManagement;

namespace KaVE.VS.Achievements.UI.StatisticUI
{
    [ToolWindowDescriptor(
        ProductNeutralId = "StatisticWindow",
        Text = "Statistic",
        VisibilityPersistenceScope = ToolWindowVisibilityPersistenceScope.Global,
        Type = ToolWindowType.SingleInstance,
        Icon = typeof (FeaturesFindingThemedIcons.SearchOptionsPage),
        InitialDocking = ToolWindowInitialDocking.Right
        )]
    public class StatisticWindowDescriptor : ToolWindowDescriptor
    {
        public StatisticWindowDescriptor(IApplicationHost applicationHost)
            : base(applicationHost) {}
    }
}
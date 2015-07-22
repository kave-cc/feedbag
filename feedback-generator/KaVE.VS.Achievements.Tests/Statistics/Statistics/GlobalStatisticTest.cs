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

using System;
using System.Collections.Generic;
using System.Numerics;
using KaVE.VS.Achievements.Properties;
using KaVE.VS.Achievements.Statistics.Statistics;
using KaVE.VS.Achievements.UI.StatisticUI;
using NUnit.Framework;

namespace KaVE.VS.Achievements.Tests.Statistics.Statistics
{
    [TestFixture]
    public class GlobalStatisticTest
    {
        private readonly GlobalStatistic _uut = new GlobalStatistic
        {
            CurrentNumberOfEditsBetweenCommits = 1,
            MaxNumberOfEditsBetweenCommits = 1000,
            TimeInDebugSession = new TimeSpan(0, 0, 0, 10),
            TotalEvents = BigInteger.Parse("10000"),
            TotalNumberOfEdits = 100,
            TotalWorkTime = new TimeSpan(0, 0, 0, 0, 10),
            EarliestEventTime = new DateTime(1984, 1, 1, 12, 0, 0),
            LatestEventTime = new DateTime(1984, 1, 1, 12, 0, 0)
        };

        [Test]
        public void GetCollectionTest()
        {
            var actualCollection = _uut.GetCollection();

            var expectedCollection = new List<StatisticElement>
            {
                new StatisticElement
                {
                    Name = StatisticPropertyNames.TotalEvents,
                    Value = "10.000"
                },
                new StatisticElement
                {
                    Name = StatisticPropertyNames.TotalNumberOfEdits,
                    Value = "100"
                },
                new StatisticElement
                {
                    Name = StatisticPropertyNames.CurrentNumberOfEditsBetweenCommits,
                    Value = "1"
                },
                new StatisticElement
                {
                    Name = StatisticPropertyNames.MaxNumberOfEditsBetweenCommits,
                    Value = "1.000"
                },
                new StatisticElement
                {
                    Name = StatisticPropertyNames.TimeInDebugSession,
                    Value = "10 s"
                },
                new StatisticElement
                {
                    Name = StatisticPropertyNames.TotalWorkTime,
                    Value = "10 ms"
                },
                new StatisticElement
                {
                    Name = StatisticPropertyNames.EarliestEventTime,
                    Value = "12:00"
                },
                new StatisticElement
                {
                    Name = StatisticPropertyNames.LatestEventTime,
                    Value = "12:00"
                }
            };

            CollectionAssert.AreEqual(expectedCollection, actualCollection);
        }
    }
}
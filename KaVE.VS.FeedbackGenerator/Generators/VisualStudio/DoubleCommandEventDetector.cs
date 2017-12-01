/*
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

using System;
using JetBrains.Annotations;
using KaVE.Commons.Utils;

namespace KaVE.VS.FeedbackGenerator.Generators.VisualStudio
{
    public interface IDoubleCommandEventDetector
    {
        bool ShouldProcess([NotNull] string cmdId);
    }

    public class DoubleCommandEventDetector : IDoubleCommandEventDetector
    {
        public const int TimeForDoubledEventInMs = 100;

        private readonly IDateUtils _dateUtils;

        private string _lastId;
        private DateTimeOffset _lastTime = DateTimeOffset.MinValue;


        public DoubleCommandEventDetector(IDateUtils dateUtils)
        {
            _dateUtils = dateUtils;
        }

        public bool ShouldProcess(string id)
        {
            var last = _lastTime;
            var now = _dateUtils.Now;
            _lastTime = now;

            if (id.Equals(_lastId))
            {
                var isTimeout = (now - last).TotalMilliseconds > TimeForDoubledEventInMs;
                return isTimeout;
            }
            _lastId = id;
            return true;
        }
    }
}
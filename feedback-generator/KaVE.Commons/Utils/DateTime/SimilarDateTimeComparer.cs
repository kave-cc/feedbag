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

namespace KaVE.Commons.Utils.DateTime
{
    public class SimilarDateTimeComparer : IComparer<System.DateTime>
    {
        private readonly uint _diffMillis;

        public SimilarDateTimeComparer(uint diffMillis)
        {
            _diffMillis = diffMillis;
        }

        public int Compare(System.DateTime x, System.DateTime y)
        {
            var diff = Math.Abs((x - y).TotalMilliseconds);
            return (diff <= _diffMillis) ? 0 : x.CompareTo(y);
        }

        public bool Equal(System.DateTime x, System.DateTime y)
        {
            return Compare(x, y) == 0;
        }
    }
}

/*
 * Copyright 2014 Technische Universitšt Darmstadt
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
using KaVE.Commons.Utils;

namespace KaVE.FeedbackProcessor.Activities.Intervals
{
    internal class Interval<T>
    {
        public T Id { get; internal set; }
        public DateTime Start { get; internal set; }
        public DateTime End { get; internal set; }

        protected bool Equals(Interval<T> other)
        {
            return Id.Equals(other.Id) && Start.Equals(other.Start) && End.Equals(other.End);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj, Equals);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id.GetHashCode();
                hashCode = (hashCode*397) ^ Start.GetHashCode();
                hashCode = (hashCode*397) ^ End.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return String.Format("Start: {1}, Id: {0}, End: {2}", Id, Start, End);
        }
    }
}
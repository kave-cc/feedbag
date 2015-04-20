/*
 * Copyright 2014 Technische Universit�t Darmstadt
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
 * 
 * Contributors:
 *    - Sven Amann
 */
using KaVE.FeedbackProcessor.Database;

namespace KaVE.FeedbackProcessor.Tests.Database
{
    internal class TestFeedbackDatabase : IFeedbackDatabase
    {
        private readonly IDeveloperCollection _developerCollection = new TestDeveloperCollection();
        private readonly IIDEEventCollection _originalIDEEventCollection = new TestIDEEventCollection();
        private readonly IIDEEventCollection _cleanIDEEventCollection = new TestIDEEventCollection();

        public IDeveloperCollection GetDeveloperCollection()
        {
            return _developerCollection;
        }

        public IIDEEventCollection GetOriginalEventsCollection()
        {
            return _originalIDEEventCollection;
        }

        public IIDEEventCollection GetCleanEventsCollection()
        {
            return _cleanIDEEventCollection;
        }
    }
}
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
using KaVE.Model.Events.CompletionEvent;
using KaVE.TestUtils.Model.Names;
using NUnit.Framework;

namespace KaVE.VsFeedbackGenerator.Tests.Utils.Json.Model
{
    [TestFixture]
    class MethodHierarchySerializationTest
    {
        [Test]
        public void ShouldSerializeMethodHierarchy()
        {
            var uut = new MethodHierarchy(TestNameFactory.GetAnonymousMethodName())
            {
                First = TestNameFactory.GetAnonymousMethodName(),
                Super = TestNameFactory.GetAnonymousMethodName()
            };
            JsonAssert.SerializationPreservesData(uut);
        }
    }
}

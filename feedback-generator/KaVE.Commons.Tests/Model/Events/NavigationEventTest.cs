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

using KaVE.Commons.Model.Events;
using KaVE.Commons.Model.Naming.Impl.v0;
using KaVE.Commons.TestUtils;
using NUnit.Framework;

namespace KaVE.Commons.Tests.Model.Events
{
    internal class NavigationEventTest
    {
        [Test]
        public void DefaultValues()
        {
            var sut = new NavigationEvent();
            Assert.AreEqual(Name.UnknownName, sut.Target);
            Assert.AreEqual(Name.UnknownName, sut.Location);
            Assert.AreEqual(NavigationType.Unknown, sut.TypeOfNavigation);
        }

        [Test]
        public void SettingValues()
        {
            var sut = new NavigationEvent
            {
                Target = Name.Get("A"),
                Location = Name.Get("B"),
                TypeOfNavigation = NavigationType.Click
            };
            Assert.AreEqual(Name.Get("A"), sut.Target);
            Assert.AreEqual(Name.Get("B"), sut.Location);
            Assert.AreEqual(NavigationType.Click, sut.TypeOfNavigation);
        }

        [Test]
        public void Equality_Default()
        {
            var a = new NavigationEvent();
            var b = new NavigationEvent();
            Assert.AreEqual(a, b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Equality_BaseClassIsCalled()
        {
            var a = new NavigationEvent {IDESessionUUID = "a"};
            var b = new NavigationEvent();
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Equality_ReallyTheSame()
        {
            var a = new NavigationEvent
            {
                Target = Name.Get("A"),
                Location = Name.Get("B"),
                TypeOfNavigation = NavigationType.Click
            };
            var b = new NavigationEvent
            {
                Target = Name.Get("A"),
                Location = Name.Get("B"),
                TypeOfNavigation = NavigationType.Click
            };
            Assert.AreEqual(a, b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Equality_DifferentTarget()
        {
            var a = new NavigationEvent {Target = Name.Get("A")};
            var b = new NavigationEvent();
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Equality_DifferentLocation()
        {
            var a = new NavigationEvent {Location = Name.Get("A")};
            var b = new NavigationEvent();
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void Equality_DifferentNavigationType()
        {
            var a = new NavigationEvent {TypeOfNavigation = NavigationType.Click};
            var b = new NavigationEvent();
            Assert.AreNotEqual(a, b);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void ToStringReflection()
        {
            ToStringAssert.Reflection(new NavigationEvent());
        }

        [Test]
        public void NumberingOfEnumIsStable()
        {
            Assert.AreEqual(0, (int) NavigationType.Unknown);
            Assert.AreEqual(1, (int) NavigationType.CtrlClick);
            Assert.AreEqual(2, (int) NavigationType.Click);
            Assert.AreEqual(3, (int) NavigationType.Keyboard);
        }
    }
}
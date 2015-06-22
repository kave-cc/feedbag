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
using System.Reflection;
using System.Windows.Forms;
using JetBrains.Application;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.Match;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Lookup;
using JetBrains.ReSharper.Feature.Services.Lookup;
using JetBrains.Util;
using KaVE.JetBrains.Annotations;

namespace KaVE.VS.FeedbackGenerator.CodeCompletion
{
    public interface IExtendedLookupWindowManager
    {
        IExtendedLookup CurrentLookup { get; }
        event EventHandler BeforeLookupWindowShown;
    }

    [ShellComponent]
    public class ExtendedLookupWindowManager : IExtendedLookupWindowManager
    {
        private static readonly FieldInfo LookupWindowField =
            typeof (LookupWindowManager).GetField(
                "myCachedLookupWindow",
                BindingFlags.NonPublic | BindingFlags.Instance);

        // TODO RS9: was private
        public readonly ILookupWindowManager _manager;

        public ExtendedLookupWindowManager(ILookupWindowManager manager)
        {
            _manager = manager;
            _manager.BeforeLookupWindowShown += (sender, args) => BeforeLookupWindowShown(sender, args);
        }

        public IExtendedLookup CurrentLookup
        {
            get
            {
                var currentLookup = (Lookup) _manager.CurrentLookup;
                return new ExtendedLookup(currentLookup, this);
            }
        }

        internal ILookupWindow CurrentLookupWindow
        {
            get { return (ILookupWindow) LookupWindowField.GetValue(_manager); }
        }

        public event EventHandler BeforeLookupWindowShown = delegate { };
    }

    public interface IExtendedLookup
    {
        //Lifetime Lifetime { get; }
        string Prefix { get; }
        IEnumerable<ILookupItem> DisplayedItems { get; }
        ILookupItem SelectedItem { get; }
        event ItemsHandler BeforeShownItemsUpdated;
        event EventHandler CurrentItemChanged;
        event CompletionHandler ItemCompleted;
        event EventHandler Closed;
        event MouseEventHandler MouseDown;
    }

    public class ExtendedLookup : IExtendedLookup
    {
        /*private static readonly FieldInfo LookupLifetime =
            typeof (Lookup).GetField("myLifetime", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo LookupListBox =
            typeof (ILookupWindow).GetField("myListBox", BindingFlags.NonPublic | BindingFlags.Instance);
        */

        private readonly Lookup _baseLookup;
        private readonly ExtendedLookupWindowManager _manager;

        public ExtendedLookup([NotNull] Lookup baseLookup, [NotNull] ExtendedLookupWindowManager manager)
        {
            _baseLookup = baseLookup;
            _manager = manager;
            _baseLookup.BeforeShownItemsUpdated += BaseLookupOnBeforeShownItemsUpdated;
            _baseLookup.CurrentItemChanged += (sender, args) => CurrentItemChanged(sender, args);
            _baseLookup.ItemCompleted += (sender, item, suffix, type) => ItemCompleted(sender, item, suffix, type);
            _baseLookup.Closed += (sender, args) => Closed(sender, args);
            // TODO RS9
            //ListBoxControl.MouseDown += (sender, args) => MouseDown(sender, args);
        }

        // TODO RS9: ugly hack to prevent stack overflow, access to DisplayItems seems to trigger event
        private object _currentSender;
        private IEnumerable<Pair<ILookupItem, MatchingResult>> _currentItems;

        private void BaseLookupOnBeforeShownItemsUpdated(object sender,
            IEnumerable<Pair<ILookupItem, MatchingResult>> items)
        {
            // ReSharper disable once PossibleUnintendedReferenceComparison
            if (_currentItems == items && _currentSender == sender)
            {
                return;
            }
            _currentSender = sender;
            _currentItems = items;
            BeforeShownItemsUpdated(_currentSender, _currentItems);
        }

        /*
         public Lifetime Lifetime
        {
            get { return (Lifetime) LookupLifetime.GetValue(_baseLookup); }
        }

       private CustomListBoxControl<LookupListItem> ListBoxControl
        {
            get
            {
                var lookupWindow = _manager.CurrentLookupWindow;
                _manager.CurrentLookup.
                var o = LookupListBox.GetValue(lookupWindow);
                return (CustomListBoxControl<LookupListItem>) o;
            }
        }*/

        public string Prefix
        {
            get { return _baseLookup.Prefix; }
        }

        public IEnumerable<ILookupItem> DisplayedItems
        {
            get
            {
                Debug_CheckStack();
                return _manager._manager.CurrentLookup.Items;
            }
        }

        private static void Debug_CheckStack()
        {
            System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();

            if (t.FrameCount > 300)
            {
                // nothing
            }
        }

        public ILookupItem SelectedItem
        {
            get { return _baseLookup.GetSelection().Item; }
        }

        public event ItemsHandler BeforeShownItemsUpdated = delegate { };
        public event EventHandler CurrentItemChanged = delegate { };
        public event CompletionHandler ItemCompleted = delegate { };
        public event EventHandler Closed = delegate { };
        public event MouseEventHandler MouseDown = delegate { };
    }
}
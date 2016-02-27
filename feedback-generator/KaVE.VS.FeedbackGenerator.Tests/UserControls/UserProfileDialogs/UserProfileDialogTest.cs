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

using KaVE.RS.Commons;
using KaVE.RS.Commons.Settings;
using KaVE.VS.FeedbackGenerator.Menu;
using KaVE.VS.FeedbackGenerator.Settings;
using KaVE.VS.FeedbackGenerator.Settings.ExportSettingsSuite;
using KaVE.VS.FeedbackGenerator.UserControls.UserProfile;
using KaVE.VS.FeedbackGenerator.UserControls.UserProfileDialogs;
using Moq;
using NUnit.Framework;

namespace KaVE.VS.FeedbackGenerator.Tests.UserControls.UserProfileDialogs
{
    [RequiresSTA]
    internal class UserProfileDialogTest : BaseUserControlTest
    {
        private Mock<ISettingsStore> _mockSettingsStore;
        private UserProfileDialog _sut;
        private UserProfileSettings _userProfileSettings;
        private Mock<IActionExecutor> _mockActionExecutor;

        [SetUp]
        public void SetUp()
        {
            _userProfileSettings = new UserProfileSettings();

            _mockSettingsStore = new Mock<ISettingsStore>();
            _mockSettingsStore.Setup(settingStore => settingStore.GetSettings<ExportSettings>())
                              .Returns(new ExportSettings());
            _mockSettingsStore.Setup(settingStore => settingStore.GetSettings<UserProfileSettings>())
                              .Returns(_userProfileSettings);

            _sut = Open();
        }

        private UserProfileDialog Open()
        {
            _mockActionExecutor = new Mock<IActionExecutor>();
            var userProfileReminderWindow = new UserProfileDialog(
                _mockActionExecutor.Object,
                _mockSettingsStore.Object);
            userProfileReminderWindow.Show();
            return userProfileReminderWindow;
        }

        [Test]
        public void DataContextIsSetCorrectly()
        {
            Assert.IsInstanceOf<UserProfileContext>(_sut.DataContext);
        }

        [Test]
        public void ShouldSaveSettingsOnClose()
        {
            _sut.Close();

            _mockSettingsStore.Verify(settingsStore => settingsStore.SetSettings(_userProfileSettings));
        }

        [Test]
        public void ShouldSetHasBeenAskedtoFillProfileOnClose()
        {
            _sut.Close();

            Assert.True(_userProfileSettings.HasBeenAskedToFillProfile);
        }

        [Test]
        public void ShouldOpenUploadWizardOnClose()
        {
            _sut.Close();

            _mockActionExecutor.Verify(actionExecutor => actionExecutor.ExecuteActionGuarded<UploadWizardAction>());
        }
    }
}
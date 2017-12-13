/*
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
using JetBrains.Application.Settings;

namespace KaVE.VS.FeedbackGenerator.Settings
{
    [SettingsKey(typeof(FeedbackSettings), "KaVE Feedback-Upload Settings")]
    // WARNING: Do not change classname, as it is used to identify settings
    public class UploadSettings
    {
        private const string DateTimeMinValue = "0001-01-01T00:00:00Z";

        [SettingsEntry(DateTimeMinValue, "Timestamp of the last time the upload-reminder popup was shown to the user.")]
        public DateTimeOffset LastNotificationDate;

        [SettingsEntry(DateTimeMinValue, "Timestamp of the last time an export was done.")]
        public DateTimeOffset LastUploadDate;

        public bool IsInitialized()
        {
            var hasUninitializedField = LastNotificationDate == DateTimeOffset.MinValue ||
                                        LastUploadDate == DateTimeOffset.MinValue;
            return !hasUninitializedField;
        }

        public void Initialize()
        {
            var now = DateTimeOffset.Now;
            LastUploadDate = now;
            LastNotificationDate = now;
        }
    }
}
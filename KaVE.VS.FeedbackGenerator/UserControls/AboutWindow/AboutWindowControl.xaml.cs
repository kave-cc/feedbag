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

using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Navigation;
using KaVE.RS.Commons;
using KaVE.RS.Commons.Settings;
using KaVE.VS.Commons;
using KaVE.VS.FeedbackGenerator.Settings.ExportSettingsSuite;

namespace KaVE.VS.FeedbackGenerator.UserControls.AboutWindow
{
    public partial class AboutWindowControl
    {
        private readonly string _linkPrefix;

        public AboutWindowControl()
        {
            InitializeComponent();
            DataContext = this;

            var versionUtil = Registry.GetComponent<FeedBaGVersionUtil>();
            _linkPrefix = Registry.GetComponent<SettingsStore>().GetSettings<ExportSettings>().WebAccessPrefix;

            VersionString = string.Format(
                "{0} (installed at {1:s})",
                versionUtil.GetInformalVersion(),
                File.GetLastWriteTime(typeof(AboutWindowControl).Assembly.Location));

            DebugInfoString = string.Format(
                "Current Culture: {0}, Current UI Culture: {1}",
                Thread.CurrentThread.CurrentCulture,
                Thread.CurrentThread.CurrentUICulture);

            LogoLink = string.Concat(
                _linkPrefix,
                "http://www.kave.cc/_/rsrc/1398356839176/config/kave-logoe-with-title.png.1398356838759.png");
        }

        public string VersionString { get; private set; }
        public string DebugInfoString { get; private set; }
        public string LogoLink { get; private set; }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(string.Concat(_linkPrefix, e.Uri.ToString()));
        }
    }
}
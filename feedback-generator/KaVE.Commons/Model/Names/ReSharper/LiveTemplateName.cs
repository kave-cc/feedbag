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

using KaVE.Commons.Model.Names.CSharp;
using KaVE.Commons.Utils.Collections;

namespace KaVE.Commons.Model.Names.ReSharper
{
    public class LiveTemplateName : Name
    {
        private static readonly WeakNameCache<LiveTemplateName> Registry =
            WeakNameCache<LiveTemplateName>.Get(id => new LiveTemplateName(id));

        /// <summary>
        ///     Template names follow the scheme "&lt;template name&gt;:&lt;template description&gt;".
        /// </summary>
        public new static LiveTemplateName Get(string identifier)
        {
            return Registry.GetOrCreate(identifier);
        }

        private LiveTemplateName(string identifier) : base(identifier) {}

        public string Name
        {
            get
            {
                var endOfName = Identifier.IndexOf(':');
                return Identifier.Substring(0, endOfName);
            }
        }

        public string Description
        {
            get
            {
                var startOfDescription = Identifier.IndexOf(':') + 1;
                return Identifier.Substring(startOfDescription);
            }
        }
    }
}
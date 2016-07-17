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

using KaVE.Commons.Model.Naming.IDEComponents;

namespace KaVE.Commons.Model.Naming.Impl.v0.IDEComponents
{
    public class DocumentName : BaseName, IDocumentName
    {
        public DocumentName() : base(UnknownNameIdentifier) {}
        public DocumentName(string identifier) : base(identifier) {}

        private string[] Parts()
        {
            if (Identifier.StartsWith("Plain Text "))
            {
                return new[] {"Plain Text", Identifier.Substring(11)};
            }
            return Identifier.Split(new[] {' '}, 2);
        }

        public string Language
        {
            get { return Parts()[0]; }
        }

        public string FileName
        {
            get { return Parts()[1]; }
        }

        public override bool IsUnknown
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}
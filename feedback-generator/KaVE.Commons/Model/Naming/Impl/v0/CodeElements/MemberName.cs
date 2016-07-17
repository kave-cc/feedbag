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

using KaVE.Commons.Model.Naming.CodeElements;
using KaVE.Commons.Model.Naming.Types;

namespace KaVE.Commons.Model.Naming.Impl.v0.CodeElements
{
    public abstract class MemberName : BaseName, IMemberName
    {
        protected const string UnknownMemberIdentifier = "[?] [?].???";
        public const string StaticModifier = "static";

        protected MemberName(string identifier) : base(identifier) {}

        public string Modifiers
        {
            get { return Identifier.Substring(0, Identifier.IndexOf('[')); }
        }

        public ITypeName DeclaringType
        {
            get
            {
                var endOfValueType = Identifier.EndOfNextTypeIdentifier(0);
                var startOfDecarlingType = Identifier.StartOfNextTypeIdentifier(endOfValueType) + 1;
                var endOfDeclaringType = Identifier.EndOfNextTypeIdentifier(endOfValueType) - 1;
                var lengthOfDeclaringType = endOfDeclaringType - startOfDecarlingType;
                return Names.Type(Identifier.Substring(startOfDecarlingType, lengthOfDeclaringType));
            }
        }

        public bool IsStatic
        {
            get { return Modifiers.Contains(StaticModifier); }
        }

        public virtual string Name
        {
            get
            {
                var nameWithBraces = Identifier.Substring(Identifier.LastIndexOf('.') + 1);
                var endIdx = nameWithBraces.IndexOf('(');
                return endIdx != -1
                    ? nameWithBraces.Substring(0, endIdx)
                    : nameWithBraces;
            }
        }

        public ITypeName ValueType
        {
            get
            {
                var startOfValueType = Identifier.StartOfNextTypeIdentifier(0) + 1;
                var endOfValueType = Identifier.EndOfNextTypeIdentifier(0) - 1;
                var lengthOfValueTypeIdentifier = endOfValueType - startOfValueType;
                return Names.Type(Identifier.Substring(startOfValueType, lengthOfValueTypeIdentifier));
            }
        }
    }
}
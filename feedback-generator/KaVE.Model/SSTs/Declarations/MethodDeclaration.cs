/*
 * Copyright 2014 Technische Universitšt Darmstadt
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
 *    - Sebastian Proksch
 */

using System;
using System.Collections.Generic;
using System.Linq;
using KaVE.Model.Collections;
using KaVE.Model.Names;
using KaVE.Utils;

namespace KaVE.Model.SSTs.Declarations
{
    public class MethodDeclaration
    {
        public IMethodName Name { get; set; }
        public readonly IList<Statement> Body = Lists.NewList<Statement>();
        public Boolean IsEntryPoint { get; set; }

        public override bool Equals(object obj)
        {
            return this.Equals(obj, Equals);
        }

        protected bool Equals(MethodDeclaration other)
        {
            return Equals(Name, other.Name) && Equals(Body, other.Body);
        }

        public override int GetHashCode()
        {
            var nameHash = Name != null ? Name.GetHashCode() : 0;
            return nameHash*20349 + HashCodeUtils.For(3456, Body);
        }

        public override string ToString()
        {
            var bodyRepresentation = string.Join(" ", Body.Select(s => s.ToString()));
            return string.Format("Method:{0} Body:[{1}]", Name, bodyRepresentation);
        }
    }
}
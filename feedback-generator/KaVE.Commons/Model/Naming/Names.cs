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
using System.Text;
using KaVE.Commons.Model.Naming.CodeElements;
using KaVE.Commons.Model.Naming.Impl;
using KaVE.Commons.Model.Naming.Impl.v0;
using KaVE.Commons.Model.Naming.Impl.v0.CodeElements;
using KaVE.Commons.Model.Naming.Impl.v0.IDEComponents;
using KaVE.Commons.Model.Naming.Impl.v0.Types;
using KaVE.Commons.Model.Naming.Impl.v1;
using KaVE.Commons.Model.Naming.Impl.v1.Parser;
using KaVE.Commons.Model.Naming.Types;
using KaVE.Commons.Utils.Assertion;

namespace KaVE.Commons.Model.Naming
{
    public class Names
    {
        // TODO get rid of the try-catch-all constructs

        private enum Types
        {
            TypeName,
            MethodName,
            AliasName,
            AssemblyName,
            EventName,
            FieldName,
            LambdaName,
            LocalVariableName,
            Name,
            NamespaceName,
            ParameterName,
            PropertyName
        }

        private static readonly Dictionary<string, Types> IdToType = new Dictionary<string, Types>
        {
            {"CSharp.AliasName", Types.AliasName},
            {"1a", Types.AliasName},
            {"CSharp.AssemblyName", Types.AssemblyName},
            {"1b", Types.AssemblyName},
            {"CSharp.EventName", Types.EventName},
            {"1c", Types.EventName},
            {"CSharp.FieldName", Types.FieldName},
            {"1d", Types.FieldName},
            {"CSharp.LambdaName", Types.LambdaName},
            {"1e", Types.LambdaName},
            {"CSharp.LocalVariableName", Types.LocalVariableName},
            {"1f", Types.LocalVariableName},
            {"CSharp.MethodName", Types.MethodName},
            {"1g", Types.MethodName},
            {"CSharp.Name", Types.Name},
            {"1h", Types.Name},
            {"CSharp.NamespaceName", Types.NamespaceName},
            {"1i", Types.NamespaceName},
            {"CSharp.ParameterName", Types.ParameterName},
            {"1j", Types.ParameterName},
            {"CSharp.PropertyName", Types.PropertyName},
            {"1k", Types.PropertyName},
            {"CSharp.TypeName", Types.TypeName},
            {"1l", Types.TypeName}
        };

        private static readonly Dictionary<Type, string> TypeToJson = new Dictionary<Type, string>
        {
            {typeof(AliasName), "1a"},
            {typeof(IAssemblyName), "1b"},
            {typeof(IEventName), "1c"},
            {typeof(IFieldName), "1d"},
            {typeof(ILambdaName), "1e"},
            {typeof(LocalVariableName), "1f"},
            {typeof(IMethodName), "1g"},
            {typeof(CsMethodName), "1g"},
            {typeof(Name), "1h"},
            {typeof(INamespaceName), "1i"},
            {typeof(IParameterName), "1j"},
            {typeof(IPropertyName), "1k"},
            {typeof(TypeName), "1l"},
            {typeof(CsTypeName), "1l"},
            {typeof(ITypeName), "1l"},
            {typeof(CsAssemblyName), "1b"}
        };

        public static IName ParseJson(string input)
        {
            var splitPos = input.IndexOf(":", StringComparison.Ordinal);
            var key = input.Substring(0, splitPos);
            var identifier = input.Substring(splitPos + 1, (input.Length - splitPos) - 1);
            if (!IdToType.ContainsKey(key))
                return null;
            var type = IdToType[key];
            switch (type)
            {
                case Types.AliasName:
                    return AliasName.Get(identifier);
                case Types.AssemblyName:
                    return GetAssemblyName(identifier);
                case Types.EventName:
                    return Event(identifier);
                case Types.FieldName:
                    return Field(identifier);
                case Types.LambdaName:
                    return GetLambdaName(identifier);
                case Types.LocalVariableName:
                    return LocalVariableName.Get(identifier);
                case Types.MethodName:
                    return Method(identifier);
                case Types.Name:
                    return Name.Get(identifier);
                case Types.NamespaceName:
                    return Namespace(identifier);
                case Types.ParameterName:
                    return Parameter(identifier);
                case Types.PropertyName:
                    return Property(identifier);
                case Types.TypeName:
                    return Type(identifier);
            }
            return null;
        }

        public static string ToJson(IName type)
        {
            var s = new StringBuilder();
            var name = type as UnknownName;
            if (name != null)
            {
                s.Append(TypeToJson[name.GetUnknownType()]);
            }
            else
            {
                s.Append(TypeToJson[type.GetType()]);
            }
            s.Append(":");
            s.Append(type.Identifier);
            return s.ToString();
        }

        public static ITypeName Type(string input)
        {
            try
            {
                var ctx = TypeNameParseUtil.ValidateTypeName(input);
                if (ctx.UNKNOWN() != null)
                {
                    return UnknownName.Get(typeof(ITypeName));
                }
                return new CsTypeName(ctx);
            }
            catch (Exception)
            {
                try
                {
                    var ctx = TypeNameParseUtil.ValidateTypeName(CsNameFixer.HandleOldTypeNames(input));
                    if (ctx.UNKNOWN() != null)
                    {
                        return UnknownName.Get(typeof(ITypeName));
                    }
                    return new CsTypeName(ctx);
                }
                catch (Exception)
                {
                    return UnknownName.Get(typeof(ITypeName));
                }
            }
        }

        public static IMethodName Method(string input)
        {
            try
            {
                var ctx = TypeNameParseUtil.ValidateMethodName(input);
                if (ctx.UNKNOWN() != null)
                {
                    return UnknownName.Get(typeof(IMethodName));
                }
                return new CsMethodName(ctx);
            }
            catch (Exception)
            {
                try
                {
                    var ctx = TypeNameParseUtil.ValidateMethodName(CsNameFixer.HandleOldMethodNames(input));
                    if (ctx.UNKNOWN() != null)
                    {
                        return UnknownName.Get(typeof(IMethodName));
                    }
                    return new CsMethodName(ctx);
                }
                catch (Exception)
                {
                    return UnknownName.Get(typeof(IMethodName));
                }
            }
        }

        public static INamespaceName Namespace(string input)
        {
            try
            {
                var ctx = TypeNameParseUtil.ValidateNamespaceName(input);
                return new CsNamespaceName(ctx);
            }
            catch (Exception)
            {
                return UnknownName.Get(typeof(INamespaceName));
            }
        }

        public static IAssemblyName GetAssemblyName(string input)
        {
            try
            {
                var ctx = TypeNameParseUtil.ValidateAssemblyName(input);
                return new CsAssemblyName(ctx);
            }
            catch (Exception)
            {
                return UnknownName.Get(typeof(IAssemblyName));
            }
        }

        public static IParameterName Parameter(string input)
        {
            try
            {
                var ctx = TypeNameParseUtil.ValidateParameterName(input);
                return new CsParameterName(ctx);
            }
            catch (AssertException)
            {
                return UnknownName.Get(typeof(IParameterName));
            }
        }

        private static CsMemberName GetMemberName(string input)
        {
            try
            {
                var ctx = TypeNameParseUtil.ValidateMemberName(input);
                if (ctx.UNKNOWN() == null)
                {
                    return new CsMemberName(ctx);
                }
                return null;
            }
            catch (AssertException)
            {
                return null;
            }
        }

        public static IFieldName Field(string input)
        {
            var name = GetMemberName(input);
            if (name != null)
            {
                return name;
            }
            return UnknownName.Get(typeof(IFieldName));
        }

        public static IEventName Event(string input)
        {
            var name = GetMemberName(input);
            if (name != null)
            {
                return name;
            }
            return UnknownName.Get(typeof(IEventName));
        }

        public static IPropertyName Property(string input)
        {
            var name = GetMemberName(input);
            if (name != null)
            {
                return name;
            }
            return UnknownName.Get(typeof(IPropertyName));
        }

        public static ILambdaName GetLambdaName(string input)
        {
            try
            {
                var ctx = TypeNameParseUtil.ValidateLambdaName(input);
                if (ctx.UNKNOWN() != null)
                {
                    return UnknownName.Get(typeof(ILambdaName));
                }
                return new CsLambdaName(ctx);
            }
            catch (AssertException)
            {
                return UnknownName.Get(typeof(ILambdaName));
            }
        }

        public static IName General(string s)
        {
            return Name.Get(s);
        }

        public static ILambdaName Lambda(string n)
        {
            return LambdaName.Get(n);
        }

        #region ide components

        public static WindowName Window(string n)
        {
            return WindowName.Get(n);
        }

        public static DocumentName Document(string n)
        {
            return DocumentName.Get(n);
        }

        #endregion

        #region unknowns

        public static ITypeName UnknownType
        {
            get { return UnknownName.Type(); }
        }

        public static IMethodName UnknownMethod
        {
            get { return UnknownName.Method(); }
        }

        public static IName UnknownGeneral
        {
            get { return Name.UnknownName; }
        }

        #endregion

        #region string.Format helpers

        public static ITypeName Type(string typeStr, params object[] args)
        {
            return Type(string.Format(typeStr, args));
        }

        public static INamespaceName Namespace(string name, params object[] args)
        {
            return Namespace(string.Format(name, args));
        }

        public static IMethodName Method(string input, params object[] args)
        {
            return Method(string.Format(input, args));
        }

        public static IParameterName Parameter(string input, params object[] args)
        {
            return Parameter(string.Format(input, args));
        }

        public static IFieldName Field(string input, params object[] args)
        {
            return Field(string.Format(input, args));
        }

        public static IPropertyName Property(string input, params object[] args)
        {
            return Property(string.Format(input, args));
        }

        public static ILambdaName Lambda(string input, params object[] args)
        {
            return Lambda(string.Format(input, args));
        }

        #endregion
    }
}
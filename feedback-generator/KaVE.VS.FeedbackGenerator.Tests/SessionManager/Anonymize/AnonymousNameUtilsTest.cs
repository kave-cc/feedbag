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

using KaVE.Commons.Model.Naming;
using KaVE.Commons.Model.Naming.Impl.v0;
using KaVE.Commons.Model.Naming.Impl.v0.CodeElements;
using KaVE.Commons.Model.Naming.Impl.v0.IDEComponents;
using KaVE.Commons.Model.Naming.Impl.v0.Types;
using KaVE.JetBrains.Annotations;
using KaVE.VS.FeedbackGenerator.SessionManager.Anonymize;
using NUnit.Framework;

namespace KaVE.VS.FeedbackGenerator.Tests.SessionManager.Anonymize
{
    internal class AnonymousNameUtilsTest
    {
        [Test]
        public void ShouldAnonymizeStrings()
        {
            Assert.AreEqual("QUThlfRt54o2I9pzZNBPEQ==", "a".ToHash());
        }

        [Test]
        public void ShouldAnonymizeNull()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Null(((string) null).ToHash());
        }

        [Test]
        public void ShouldNotAnonymizeEmptyString()
        {
            Assert.AreEqual("", "".ToHash());
        }

        [Test]
        public void ShouldNotAnonymizeUnknownNames()
        {
            var actual = Name.UnknownName.ToAnonymousName();
            var expected = Name.UnknownName;
            Assert.AreEqual(expected, actual);
            // equivalent...
            var actual2 = TypeName.UnknownName.ToAnonymousName();
            var expected2 = TypeName.UnknownName;
            Assert.AreEqual(expected2, actual2);
            // equivalent...
            var actual3 = MethodName.UnknownName.ToAnonymousName();
            var expected3 = MethodName.UnknownName;
            Assert.AreEqual(expected3, actual3);
        }

        [Test]
        public void ShouldAnonymizeFileNameFromDocumentName()
        {
            var original = DocumentName.Get("CSharp C:\\File.cs");
            var expected = DocumentName.Get("CSharp ixlmuLAuUg0yq59EtLWB7w==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeWindowCaptionIfItContainsAFileName()
        {
            var original = WindowName.Get("vsSomeWindowType C:\\Contains\\File.Name");
            var expected = WindowName.Get("vsSomeWindowType aVxPI-qHR-QO3bMv-Ker6w==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldKeepWindowCaptionIfItDoesNotContainAFileName()
        {
            var original = WindowName.Get("vsToolWindow Unit Test Sessions");

            AssertAnonymizedEquals(original, original);
        }

        [Test]
        public void ShouldAnonymizeSolutionPath()
        {
            var original = SolutionName.Get("C:\\Solution.sln");
            var expected = SolutionName.Get("H_MB2iBprhCn9SyXdxnVNQ==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeProjectPath()
        {
            var original = ProjectName.Get("Folder C:\\A\\B\\C");
            var expected = ProjectName.Get("Folder IklTG_YtPBAhWOIrB65I1Q==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeProjectItemPath()
        {
            var original = ProjectItemName.Get("CSharp C:\\A\\B\\Class.cs");
            var expected = ProjectItemName.Get("CSharp nmTd_-pgymTyNZrw5bGrpg==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeAlias()
        {
            var original = AliasName.Get("global");
            var expected = AliasName.Get("rW1oPYChRX9JiYuIQBWjBQ==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeGenericName()
        {
            var original = Name.Get("some name that might or might not contain private information");
            var expected = Name.Get("C5my9gXfmcktCtPzYR9MEQ==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeAssemblyIfEnclosingProject()
        {
            var original = AssemblyName.Get("MyProject");
            var expected = AssemblyName.Get("zRLpydQJBMrk8DCiP3BwEQ==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldKeepAssemblyIfFullQualified()
        {
            var original = AssemblyName.Get("SomeAssembly, 1.5.6.3");

            AssertAnonymizedEquals(original, original);
        }

        [Test]
        public void ShouldKeepTypeFromOtherAssembly()
        {
            var original = TypeName.Get("SomeType, MyProject, 1.2.3.4");

            AssertAnonymizedEquals(original, original);
        }

        [Test]
        public void ShouldKeepUnknownTypeName()
        {
            var original = UnknownTypeName.Instance;

            AssertAnonymizedEquals(original, original);
        }

        [Test]
        public void ShouldAnonymizeFieldNameIfDeclaringTypeIsUnknown()
        {
            var original = FieldName.Get("[?] [?].field");
            var expected = FieldName.Get("[?] [?].uH-HUtyKzOVVTdxGpUvTRg==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeMethodMembersIfDeclaringTypeIsUnknown()
        {
            var original = MethodName.Get("[?] [?].method([?] arg)");
            var expected = MethodName.Get("[?] [?].S2MqM0cJGKIdPyRb46oevg==([?] cjjZM6DVkmp283JnWfyH_A==)");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeTypeNameIfFromEnclosingProject()
        {
            var original = TypeName.Get("SomeType, MyProject");
            var expected = TypeName.Get("5TEfRdZBhGQY3JybERVp-w==, zRLpydQJBMrk8DCiP3BwEQ==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldKeepNestedTypeMarkersWhenAnonymizingTypeName()
        {
            var original = TypeName.Get("Outer+Intermediate+Inner, MyProject");
            var expected =
                TypeName.Get(
                    "vWJW7HmayjJvbX16XC9VnQ==+471REvNW-WCCyW7mDRT4EA==+YDcvejSpfAK3U9T4L-U5Ng==, zRLpydQJBMrk8DCiP3BwEQ==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldKeepNamespaceToTypeSeparatorWhenAnonymizingTypeName()
        {
            var original = TypeName.Get("My.Namespace.MyType, MyProject");
            var expected = TypeName.Get("L5-7Qmufwl5lDD-ks5-QzQ==.T3GwyBT-NeFSuHH-NHnMzQ==, zRLpydQJBMrk8DCiP3BwEQ==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldKeepTypeParameterShortName()
        {
            var original = TypeName.Get("TT -> AType, MyProject");
            var expected = TypeName.Get("TT -> S8jqvjvDTBSSXY7BIBFNOQ==, zRLpydQJBMrk8DCiP3BwEQ==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldKeepNestedTypeParameterShortNames()
        {
            var original = TypeName.Get("TT -> TU -> TV -> AType, MyProject");
            var expected = TypeName.Get("TT -> TU -> TV -> S8jqvjvDTBSSXY7BIBFNOQ==, zRLpydQJBMrk8DCiP3BwEQ==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeTypeParametersIfDefinedInEnclosingProject()
        {
            var original = TypeName.Get("Some.Type`1[[T -> OtherType, A]], B, 1.2.3.4");
            var expected =
                TypeName.Get("Some.Type`1[[T -> xJGI74kh-RBFid7-a1wFlg==, ghTRAD9op9mwNWwMvX7uGg==]], B, 1.2.3.4");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldKeepTypeParametersFromOtherAssembly()
        {
            var original =
                TypeName.Get(
                    "Some.Type`3[[T -> MyType, A],[U -> System.Double, mscorlib, 4.0.0.0],[V -> MyOtherType, A]], B, 1.2.3.4");
            var expected =
                TypeName.Get(
                    "Some.Type`3[[T -> Q-vTVCo_g8yayGGoDdH7BA==, ghTRAD9op9mwNWwMvX7uGg==],[U -> System.Double, mscorlib, 4.0.0.0],[V -> w20iwoM8jFvdUxBRQsKvhg==, ghTRAD9op9mwNWwMvX7uGg==]], B, 1.2.3.4");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeParameterizedTypeIfDefinedInEnclosingProject()
        {
            var original =
                TypeName.Get("MyTypeFromEnclosingProject`1[[T -> System.Int32, mscorlib, 4.0.0.0]], EnclosingProject");
            var expected =
                TypeName.Get(
                    "yqUUbRFTqfCBIMxMRH-qDA==`1[[TM6pgLI0nE5n0EEgAKIIFw== -> System.Int32, mscorlib, 4.0.0.0]], qfFVtSOtve-XEFJXWTbfXw==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeInterfaceTypeNameFromEnclosingProject()
        {
            var original = TypeName.Get("i:My.Interface, EnclosingProject");
            var expected = TypeName.Get("i:S7JFQ1Qpzr6dQZksNAcR7A==.6e_eXMoTYXtpcGd2wrWE-A==, qfFVtSOtve-XEFJXWTbfXw==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeEnumTypeNameFromEnclosingProject()
        {
            var original = TypeName.Get("e:My.Enum, EnclosingProject");
            var expected = TypeName.Get("e:S7JFQ1Qpzr6dQZksNAcR7A==.klRY89gvVPCkpyaQ3MurVQ==, qfFVtSOtve-XEFJXWTbfXw==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeDelegateTypeNameFromEnclosingProject()
        {
            var original = TypeName.Get("d:[Void, CL, 4.0.0.0] [My.Delegate, EnclosingProject].()");
            var expected =
                TypeName.Get(
                    "d:[Void, CL, 4.0.0.0] [S7JFQ1Qpzr6dQZksNAcR7A==.ssIB3MfpFeOROPNn2-P9xg==, qfFVtSOtve-XEFJXWTbfXw==].()");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeDelegateParameterNameFromEnclosingProject()
        {
            var original = TypeName.Get("d:[Void, CL, 4.0.0.0] [D, E, 1.2.3.4].([P, A] p)");
            var expected =
                TypeName.Get(
                    "d:[Void, CL, 4.0.0.0] [D, E, 1.2.3.4].([aUaDMpYpDqsiSh5nQjiWFw==, ghTRAD9op9mwNWwMvX7uGg==] xBzbwjgZ_3fD0cNcmbedKA==)");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeDelegateReturnTypeFromEnclosingProject()
        {
            var original = TypeName.Get("d:[T, EP] [D, E, 1.2.3.4].()");
            var expected = TypeName.Get("d:[TM6pgLI0nE5n0EEgAKIIFw==, vW8RYxLbF7t21szDOJMe_w==] [D, E, 1.2.3.4].()");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeCustomStructTypeNameFromEnclosingProject()
        {
            var original = TypeName.Get("s:My.Struct, EnclosingProject");
            var expected = TypeName.Get("s:S7JFQ1Qpzr6dQZksNAcR7A==.Csl4y2WI7aP5CjXqBQ8QRQ==, qfFVtSOtve-XEFJXWTbfXw==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeValueTypeOfArrayTypeIfDeclaredInEnclosingProject()
        {
            var original = TypeName.Get("SomeType[], EnclosingProject");
            var expected = TypeName.Get("5TEfRdZBhGQY3JybERVp-w==[], qfFVtSOtve-XEFJXWTbfXw==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeValueTypeOfDelegateArrayTypeIfDeckaredInEnclosingProject()
        {
            var original = TypeName.Get(
                "d:[VT, A] [ConsoleApplication1.Program+TestDelegate, A].()[]");
            var expected =
                TypeName.Get(
                    "d:[3sO2hZAKj3g4-Zk_E0A-_w==, ghTRAD9op9mwNWwMvX7uGg==] [IJXZZxKP9BYUXNF-zFV4Mg==.o4DFluOfxaQkPRb-pAXvpg==+qXAvOqA3UCqSEhbDeHkp3A==, ghTRAD9op9mwNWwMvX7uGg==].()[]");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeLocalVariableName()
        {
            var original = LocalVariableName.Get("[System.Int32, mscorlib, 4.0.0.0] variable");
            var expected = LocalVariableName.Get("[System.Int32, mscorlib, 4.0.0.0] ex1ycJF4ixZdevwEdEfKcQ==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeLocalVariableValueTypeIfDeclaredInEnclosingProject()
        {
            var original = LocalVariableName.Get("[ValueType, EnclosingProject] variable");
            var expected =
                LocalVariableName.Get("[K6-3xDZUlJ-Wew_p0xcfQg==, qfFVtSOtve-XEFJXWTbfXw==] ex1ycJF4ixZdevwEdEfKcQ==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldKeepFieldNameIfDeclaredInOtherAssembly()
        {
            var original = FieldName.Get(
                "static [System.Int32, mscorlib, 4.0.0.0] [AClass, AnAssembly, 1.2.3.4]._field");
            var expected = FieldName.Get(
                "static [System.Int32, mscorlib, 4.0.0.0] [AClass, AnAssembly, 1.2.3.4]._field");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeFieldNameIfDeclaredInEnclosingProject()
        {
            var original = FieldName.Get("[System.Int32, mscorlib, 4.0.0.0] [Class, Project]._field");
            var expected =
                FieldName.Get(
                    "[System.Int32, mscorlib, 4.0.0.0] [C30g7wWDiaLWDoT99aNK_Q==, Mxp53D4r1Kx8kPEM01ySAA==].gcnTNGyqNJv6QToYz_Vmbg==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeFieldValueTypeIfDeclaredInEnclosingProject()
        {
            var original = FieldName.Get("[ValueType, EnclosingProject] [SomeType, SomeAssembly, 1.2.3.4]._field");
            var expected =
                FieldName.Get(
                    "[K6-3xDZUlJ-Wew_p0xcfQg==, qfFVtSOtve-XEFJXWTbfXw==] [SomeType, SomeAssembly, 1.2.3.4]._field");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldKeepPropertyNameIfDeclaredInOtherAssembly()
        {
            var original =
                PropertyName.Get(
                    "set get static [System.Int32, mscorlib, 4.0.0.0] [AClass, AnAssembly, 1.2.3.4].Property");
            var expected =
                PropertyName.Get(
                    "set get static [System.Int32, mscorlib, 4.0.0.0] [AClass, AnAssembly, 1.2.3.4].Property");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizePropertyNameIfDeclaredInEnclosingProject()
        {
            var original = PropertyName.Get("get [System.Int32, mscorlib, 4.0.0.0] [Declarator, MyProject].Property");
            var expected =
                PropertyName.Get(
                    "get [System.Int32, mscorlib, 4.0.0.0] [UFthX8igK4OWY-bjuPcWaA==, zRLpydQJBMrk8DCiP3BwEQ==].3_9-BEZu3bkEMnTfk5eHKw==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizePropertyValueTypeIfDeclaredInEnclosingProject()
        {
            var original = PropertyName.Get("set [PropType, AProject] [AType, AnAssembly, 6.5.4.3].Property");
            var expected =
                PropertyName.Get(
                    "set [Mh2DRn_FRby9df2VWWFg4Q==, CD0OwIZmS7FL5zL5GiXZbg==] [AType, AnAssembly, 6.5.4.3].Property");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldKeepEventNameIfDeclaredInOtherAssembly()
        {
            var original = EventName.Get("static [ChangeEventHandler, Assembly, 6.3.5.2] [C, Foo, 9.1.2.3].Event");
            var expected = EventName.Get("static [ChangeEventHandler, Assembly, 6.3.5.2] [C, Foo, 9.1.2.3].Event");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeEventNameIfDeclaredInEnclosingProject()
        {
            var original = EventName.Get("[ChangeEventHandler, Assembly, 6.3.5.2] [A, Foo].Event");
            var expected =
                EventName.Get(
                    "[ChangeEventHandler, Assembly, 6.3.5.2] [ghTRAD9op9mwNWwMvX7uGg==, sl_wrZDQnTlQkOiin_TGPA==].Ryz5fpCQs0Nwm_x0Vy4cQg==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeEventHandlerTypeIfDeclaredInEnclosingProject()
        {
            var original = EventName.Get("[Handler, Project] [AType, AnAssembly, 6.5.4.3].Event");
            var expected =
                EventName.Get("[ooP_qY1chg4oTJoBIeq1_A==, Mxp53D4r1Kx8kPEM01ySAA==] [AType, AnAssembly, 6.5.4.3].Event");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeNamespace()
        {
            var original = NamespaceName.Get("Some.Arbitrary.Namespace");
            var expected = NamespaceName.Get("PU4V3sU7dhVQzcD16BGtuw==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeOutParameterName()
        {
            var original = ParameterName.Get("out [ParamType, A, 1.2.3.4] parameter");
            var expected = ParameterName.Get("out [ParamType, A, 1.2.3.4] jaWpYMumKzk5dZafVWTD1A==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeRefParameterName()
        {
            var original = ParameterName.Get("ref [System.Int32, mscorlib, 4.0.0.0] name");
            var expected = ParameterName.Get("ref [System.Int32, mscorlib, 4.0.0.0] mT62IUL9_OAA7vtSkeTMzg==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeParamsParameterName()
        {
            var original = ParameterName.Get("params [System.Int32, mscorlib, 4.0.0.0] name");
            var expected = ParameterName.Get("params [System.Int32, mscorlib, 4.0.0.0] mT62IUL9_OAA7vtSkeTMzg==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeOptParameterName()
        {
            var original = ParameterName.Get("opt [System.Int32, mscorlib, 4.0.0.0] name");
            var expected = ParameterName.Get("opt [System.Int32, mscorlib, 4.0.0.0] mT62IUL9_OAA7vtSkeTMzg==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeParameterValueTypeIfDeclaredInEnclosingProject()
        {
            var original = ParameterName.Get("[Type, Project] name");
            var expected =
                ParameterName.Get("[aSO4V69Y4hQtcEQCnqsGww==, Mxp53D4r1Kx8kPEM01ySAA==] mT62IUL9_OAA7vtSkeTMzg==");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeMethodNameIfDeclaringTypeIsFromEnclosingProject()
        {
            var original = MethodName.Get("[ReturnType, A, 1.2.3.4] [DeclaringType, EnclosingProject].M()");
            var expected =
                MethodName.Get(
                    "[ReturnType, A, 1.2.3.4] [HTr1vZnVhe-8SY78vI2ffQ==, qfFVtSOtve-XEFJXWTbfXw==].lNSAgClcjc9lDeUkXybdNQ==()");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeMethodReturnTypeIfDeclaringTypeIsFromEnclosingProject()
        {
            var original = MethodName.Get("[ReturnType, EP] [DT, A, 1.2.3.4].M()");
            var expected = MethodName.Get("[a6Ix9ar6tahkEo1TOfBLwg==, vW8RYxLbF7t21szDOJMe_w==] [DT, A, 1.2.3.4].M()");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeMethodTypeParametersIfDeclaredInEnclosingProject()
        {
            var original = MethodName.Get(
                "[RT, A, 1.2.3.4] [DT, A, 1.2.3.4].M`2[[T -> Foo, EP],[E -> Bar, A, 1.2.3.4]]()");
            var expected =
                MethodName.Get(
                    "[RT, A, 1.2.3.4] [DT, A, 1.2.3.4].M`2[[T -> sl_wrZDQnTlQkOiin_TGPA==, vW8RYxLbF7t21szDOJMe_w==],[E -> Bar, A, 1.2.3.4]]()");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeLambdaParametersIfDeclaredInEclosingProject()
        {
            var original = LambdaName.Get("[A, B, 1.2.3.4] ([T, EP] p)");
            var expected =
                LambdaName.Get(
                    "[A, B, 1.2.3.4] ([TM6pgLI0nE5n0EEgAKIIFw==, vW8RYxLbF7t21szDOJMe_w==] xBzbwjgZ_3fD0cNcmbedKA==)");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeLambdaReturnTypeIfDeclaredInEclosingProject()
        {
            var original = LambdaName.Get("[T, EP] ()");
            var expected = LambdaName.Get("[TM6pgLI0nE5n0EEgAKIIFw==, vW8RYxLbF7t21szDOJMe_w==] ()");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldNotAnonymizeMethodsOrTheirParametersFromOtherAssemblies()
        {
            var original =
                MethodName.Get("[RT, A, 1.2.3.4] [DT, A, 1.2.3.4].M([System.String, mscorlib, 4.0.0.0] p)");

            AssertAnonymizedEquals(original, original);
        }

        [Test]
        public void AnonymizedMethodsHaveParametersWithHashedNamesButUnhashedTypes()
        {
            var original = MethodName.Get("[T, A, 1.2.3.4] [T,P].M([T, A, 1.2.3.4] p)");
            var expected =
                MethodName.Get(
                    "[T, A, 1.2.3.4] [TM6pgLI0nE5n0EEgAKIIFw==, aUaDMpYpDqsiSh5nQjiWFw==].lNSAgClcjc9lDeUkXybdNQ==([T, A, 1.2.3.4] xBzbwjgZ_3fD0cNcmbedKA==)");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldOnlyAnonymizeParameterTypeInMethodFromOtherAssembly()
        {
            var original = MethodName.Get("[T, A, 1.2.3.4] [T, A, 1.2.3.4].M([T,P] p)");
            var expected =
                MethodName.Get(
                    "[T, A, 1.2.3.4] [T, A, 1.2.3.4].M([TM6pgLI0nE5n0EEgAKIIFw==, aUaDMpYpDqsiSh5nQjiWFw==] p)");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldNotAnonymizeConstructorName()
        {
            var original = MethodName.Get("[T, A, 1.2.3.4] [DT, P]..ctor([T,P] p, [T,A,4.0.0.0] p2)");
            var expected =
                MethodName.Get(
                    "[T, A, 1.2.3.4] [UP5Ipka5g2hTcMU6LNvz2A==, aUaDMpYpDqsiSh5nQjiWFw==]..ctor([TM6pgLI0nE5n0EEgAKIIFw==, aUaDMpYpDqsiSh5nQjiWFw==] xBzbwjgZ_3fD0cNcmbedKA==, [T, A,4.0.0.0] UIuXc44R1FaeNKJ8ldQB7A==)");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldNotAnonymizeStaticConstructorName()
        {
            var original = MethodName.Get("[T, A, 1.2.3.4] [DT, P]..cctor([T,P] p, [T,A,4.0.0.0] p2)");
            var expected =
                MethodName.Get(
                    "[T, A, 1.2.3.4] [UP5Ipka5g2hTcMU6LNvz2A==, aUaDMpYpDqsiSh5nQjiWFw==]..cctor([TM6pgLI0nE5n0EEgAKIIFw==, aUaDMpYpDqsiSh5nQjiWFw==] xBzbwjgZ_3fD0cNcmbedKA==, [T, A,4.0.0.0] UIuXc44R1FaeNKJ8ldQB7A==)");

            AssertAnonymizedEquals(original, expected);
        }

        #region anonymization of generic type parameters

        [Test]
        public void ShouldAnonymizeGenericTypeParameter_UnboundGenericInEnclosingProject()
        {
            var original = MethodName.Get("[T, A, 1.2.3.4] [C`1[[G2]], P].M([G2] p)");
            var expected =
                MethodName.Get(
                    "[T, A, 1.2.3.4] [3Rx860ySZTppa3kHpN1N8Q==`1[[HAqGEOJc_-qPti2JYHwR3Q==]], aUaDMpYpDqsiSh5nQjiWFw==].lNSAgClcjc9lDeUkXybdNQ==([HAqGEOJc_-qPti2JYHwR3Q==] xBzbwjgZ_3fD0cNcmbedKA==)");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeGenericTypeParameter_UnboundGenericAssignedToPlaceholder()
        {
            var original = MethodName.Get("[T, A, 1.2.3.4] [A`1[[G1 -> G2]], A, 0.0.0.0].M([G1] p)");
            var expected =
                MethodName.Get("[T, A, 1.2.3.4] [A`1[[G1 -> HAqGEOJc_-qPti2JYHwR3Q==]], A, 0.0.0.0].M([G1] p)");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeGenericTypeParameter_BoundGenericInEnclosingProject()
        {
            var original = MethodName.Get("[T, A, 1.2.3.4] [C`1[[G2 -> T, A, 0.0.0.0]], P].M([G2] p)");
            var expected =
                MethodName.Get(
                    "[T, A, 1.2.3.4] [3Rx860ySZTppa3kHpN1N8Q==`1[[HAqGEOJc_-qPti2JYHwR3Q== -> T, A, 0.0.0.0]], aUaDMpYpDqsiSh5nQjiWFw==].lNSAgClcjc9lDeUkXybdNQ==([HAqGEOJc_-qPti2JYHwR3Q==] xBzbwjgZ_3fD0cNcmbedKA==)");

            AssertAnonymizedEquals(original, expected);
        }

        [Test]
        public void ShouldAnonymizeGenericTypeParameter_BoundGenericInAssembly()
        {
            var original = MethodName.Get("[T, A, 1.2.3.4] [A`1[[G1 -> T, A, 0.0.0.0]], A, 0.0.0.0].M([G1] p)");
            var expected = MethodName.Get("[T, A, 1.2.3.4] [A`1[[G1 -> T, A, 0.0.0.0]], A, 0.0.0.0].M([G1] p)");

            AssertAnonymizedEquals(original, expected);
        }

        #endregion

        private static void AssertAnonymizedEquals([NotNull] IName original, [NotNull] IName expected)
        {
            var actual = original.ToAnonymousName();
            Assert.AreEqual(expected, actual);
        }
    }
}
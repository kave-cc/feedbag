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

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Application.BuildScript.Application.Zones;

[assembly: AssemblyTitle("KaVE.RS.Commons")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("KaVE.RS.Commons")]
[assembly: AssemblyCopyright("Copyright © KaVE Project 2011-2017")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]
[assembly: InternalsVisibleTo("KaVE.RS.Commons.Tests_Integration")]
[assembly: InternalsVisibleTo("KaVE.RS.Commons.Tests_Unit")]

[assembly: Guid("76ae1118-f80a-4fb9-b8f3-c1e88beefdd4")]

// our syntax: 0.<build-num>[-<variant>], see KaVE.RS.Commons.KaVEVersionUtil
[assembly: AssemblyVersion("0.0")]
[assembly: AssemblyInformationalVersion("0.0-Development")]

// ReSharper disable once CheckNamespace
namespace KaVE.RS.Commons
{
    [ZoneMarker]
    public class ZoneMarker { }
}
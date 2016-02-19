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
using System.IO;
using System.Linq;
using KaVE.Commons.Model.Events.CompletionEvents;
using KaVE.Commons.Model.Names;
using KaVE.Commons.Model.SSTs;
using KaVE.Commons.Model.SSTs.Expressions.Assignable;
using KaVE.Commons.Model.SSTs.Impl.Visitor;
using KaVE.Commons.Model.TypeShapes;
using KaVE.Commons.Utils.IO.Archives;

namespace KaVE.RS.SolutionAnalysis
{
    public class ApiStatisticsRunner
    {
        public void Run(string rootDir)
        {
            var repoCounts = new Dictionary<IAssemblyName, int>();
            var slnCounts = new Dictionary<IAssemblyName, int>();
            var sstCounts = new Dictionary<IAssemblyName, int>();

            int i = 0;
            foreach (var user in GetSubdirs(rootDir))
            {
                foreach (var repo in GetSubdirs(Path.Combine(rootDir, user)))
                {
                    Console.Write("##### {0}/{1} ##############################", user, repo);

                    var repoApis = new HashSet<IAssemblyName>();
                    var repoPath = Path.Combine(rootDir, user, repo);

                    foreach (var zip in GetArchives(repoPath))
                    {
                        if (i++ >= 20)
                        {
                            Console.WriteLine("fancy abort via goto :D");
                            goto ENDE;
                        }

                        Console.WriteLine();
                        Console.WriteLine("@@ {0} @@", zip);
                        var slnApis = new HashSet<IAssemblyName>();
                        var zipPath = Path.Combine(repoPath, zip);
                        var ra = new ReadingArchive(zipPath);

                        while (ra.HasNext())
                        {
                            Console.Write('.');
                            var ctx = ra.GetNext<Context>();

                            var apis = FindAPIs(ctx.SST);

                            foreach (var api in apis)
                            {
                                repoApis.Add(api);
                                slnApis.Add(api);
                            }

                            CountApis(apis, sstCounts);
                        }
                        CountApis(slnApis, slnCounts);
                    }
                    Console.WriteLine();
                    CountApis(repoApis, repoCounts);
                }
            }

            ENDE:
            Log(repoCounts, slnCounts, sstCounts);
        }

        private void Log(Dictionary<IAssemblyName, int> repoCounts,
            Dictionary<IAssemblyName, int> slnCounts,
            Dictionary<IAssemblyName, int> sstCounts)
        {
            Console.WriteLine();
            Console.WriteLine("Name\tVersion\t#repo\t#sln\t#sst");

            foreach (var api in repoCounts.Keys)
            {
                Console.WriteLine(
                    "\"{0}\"\t{1}\t{2}\t{3}\t{4}",
                    api.Name,
                    api.AssemblyVersion,
                    repoCounts[api],
                    slnCounts[api],
                    sstCounts[api]);
            }
        }

        private static void CountApis(ISet<IAssemblyName> apis, Dictionary<IAssemblyName, int> counts)
        {
            foreach (var api in apis)
            {
                if (counts.ContainsKey(api))
                {
                    counts[api]++;
                }
                else
                {
                    counts[api] = 1;
                }
            }
        }

        private readonly ApiIdentificationVisitor _apiVisitor = new ApiIdentificationVisitor();

        private ISet<IAssemblyName> FindAPIs(ISST sst)
        {
            var apis = new HashSet<IAssemblyName>();
            sst.Accept(_apiVisitor, apis);
            return apis;
        }

        private static IEnumerable<string> GetSubdirs(string dir)
        {
            return
                Directory.EnumerateDirectories(dir)
                         .Select(d => d.Replace(dir + @"\", ""))
                         .Select(d => d.Replace(dir, ""));
        }

        private static IEnumerable<string> GetArchives(string dir)
        {
            return
                Directory.EnumerateFiles(dir, "*.zip", SearchOption.AllDirectories)
                         .Select(f => f.Replace(dir + @"\", ""))
                         .Select(f => f.Replace(dir, ""));
        }
    }

    internal class ApiIdentificationVisitor : AbstractNodeVisitor<ISet<IAssemblyName>>
    {
        public void Visit(ITypeShape ts, ISet<IAssemblyName> apis)
        {
            Visit(ts.TypeHierarchy, apis);
            foreach (var mh in ts.MethodHierarchies)
            {
                Visit(mh, apis);
            }
        }

        public void Visit(ITypeHierarchy th, ISet<IAssemblyName> apis)
        {
            AddIf(th.Element, apis);
            if (th.Extends != null)
            {
                Visit(th.Extends, apis);
            }
            foreach (var i in th.Implements)
            {
                Visit(i, apis);
            }
        }

        public void Visit(IMethodHierarchy mh, ISet<IAssemblyName> apis)
        {
            Visit(mh.Element, apis);
            if (mh.Super != null)
            {
                Visit(mh.Super, apis);
            }
            if (mh.First != null)
            {
                Visit(mh.First, apis);
            }
        }

        public override void Visit(IInvocationExpression expr, ISet<IAssemblyName> apis)
        {
            var m = expr.MethodName;
            Visit(m, apis);
        }

        private static void Visit(IMethodName m, ISet<IAssemblyName> apis)
        {
            AddIf(m.ReturnType, apis);
            AddIf(m.DeclaringType, apis);
            foreach (var p in m.Parameters)
            {
                AddIf(p.ValueType, apis);
            }
        }

        private static void AddIf(ITypeName t, ISet<IAssemblyName> apis)
        {
            if (t.IsUnknownType)
            {
                return;
            }
            if (t.Assembly.AssemblyVersion.IsUnknown)
            {
                return;
            }
            apis.Add(t.Assembly);
        }
    }
}
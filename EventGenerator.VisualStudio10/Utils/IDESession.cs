﻿using System;
using System.Globalization;
using EnvDTE;

namespace KaVE.EventGenerator.VisualStudio10.Utils
{
    public static class IDESession
    {
        private const string UUIDGlobal = "KAVE_EventGenerator_SessionUUID";
        private const string UUIDCreatedAtGlobal = "KAVE_EventGenerator_SessionUUID_CreatedAt";
        private const string PastDate = "1987-06-20";

        // TODO test whether this is working properly in a non-experimental instance
        // in the experimental instance updates of the globals are not persisted reliably...
        public static string GetUUID(DTE dte)
        {
            var globals = dte.Globals;
            var createdAt = DateTime.Parse(globals.GetValueOrDefault(UUIDCreatedAtGlobal, PastDate));
            if (createdAt < DateTime.Today)
            {
                globals.SetValue(UUIDCreatedAtGlobal, DateTime.Today.ToString(CultureInfo.InvariantCulture));
                globals.SetValue(UUIDGlobal, Guid.NewGuid().ToString());
            }
            return globals.Get(UUIDGlobal);
        }

        private static string GetValueOrDefault(this Globals globals, string globalName, string defaultValue)
        {
            return globals.VariableExists[globalName] ? globals.Get(globalName) : defaultValue;
        }

        private static string Get(this Globals globals, string globalName)
        {
            return globals[globalName] as string;
        }

        private static void SetValue(this Globals globals, string globalName, string value)
        {
            globals[globalName] = value;
            globals.VariablePersists[globalName] = true;
        }
    }
}
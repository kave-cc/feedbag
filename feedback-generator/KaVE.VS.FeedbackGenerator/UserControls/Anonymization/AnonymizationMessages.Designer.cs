﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34209
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace KaVE.VS.FeedbackGenerator.UserControls.Anonymization {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class AnonymizationMessages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal AnonymizationMessages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("KaVE.VS.FeedbackGenerator.UserControls.Anonymization.AnonymizationMessages", typeof(AnonymizationMessages).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The information will be removed in the export process..
        /// </summary>
        public static string Desc_After {
            get {
                return ResourceManager.GetString("Desc_After", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to If you don&apos;t want to provide all generated information, you can remove specific parts of it..
        /// </summary>
        public static string Desc_Before {
            get {
                return ResourceManager.GetString("Desc_Before", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Remove identifier that originate in the project.
        /// </summary>
        public static string Label_RemoveCodeNames {
            get {
                return ResourceManager.GetString("Label_RemoveCodeNames", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Remove duration of events.
        /// </summary>
        public static string Label_RemoveDurations {
            get {
                return ResourceManager.GetString("Label_RemoveDurations", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Remove session ids.
        /// </summary>
        public static string Label_RemoveSessionIDs {
            get {
                return ResourceManager.GetString("Label_RemoveSessionIDs", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Remove start time of events.
        /// </summary>
        public static string Label_RemoveStartTimes {
            get {
                return ResourceManager.GetString("Label_RemoveStartTimes", resourceCulture);
            }
        }
    }
}
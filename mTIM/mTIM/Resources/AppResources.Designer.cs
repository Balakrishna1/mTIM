﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace mTIM.Resources {
    using System;
    using System.Reflection;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class AppResources {
        
        private static System.Resources.ResourceManager resourceMan;
        
        private static System.Globalization.CultureInfo resourceCulture;
        
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal AppResources() {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static System.Resources.ResourceManager ResourceManager {
            get {
                if (object.Equals(null, resourceMan)) {
                    System.Resources.ResourceManager temp = new System.Resources.ResourceManager("mTIM.Resources.AppResources", typeof(AppResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        internal static string AppUpdates {
            get {
                return ResourceManager.GetString("AppUpdates", resourceCulture);
            }
        }
        
        internal static string HintURL {
            get {
                return ResourceManager.GetString("HintURL", resourceCulture);
            }
        }
        
        internal static string Info {
            get {
                return ResourceManager.GetString("Info", resourceCulture);
            }
        }
        
        internal static string Refresh {
            get {
                return ResourceManager.GetString("Refresh", resourceCulture);
            }
        }
        
        internal static string Settings {
            get {
                return ResourceManager.GetString("Settings", resourceCulture);
            }
        }
        
        internal static string SyncTime {
            get {
                return ResourceManager.GetString("SyncTime", resourceCulture);
            }
        }
        
        internal static string Url {
            get {
                return ResourceManager.GetString("Url", resourceCulture);
            }
        }
    }
}

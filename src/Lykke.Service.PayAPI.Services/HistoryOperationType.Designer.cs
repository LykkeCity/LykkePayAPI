﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Lykke.Service.PayAPI.Services {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class HistoryOperationType {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal HistoryOperationType() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Lykke.Service.PayAPI.Services.HistoryOperationType", typeof(HistoryOperationType).Assembly);
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
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Incoming exchange.
        /// </summary>
        internal static string IncomingExchange {
            get {
                return ResourceManager.GetString("IncomingExchange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Incoming invoice payment.
        /// </summary>
        internal static string IncomingInvoicePayment {
            get {
                return ResourceManager.GetString("IncomingInvoicePayment", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to .
        /// </summary>
        internal static string None {
            get {
                return ResourceManager.GetString("None", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Outgoing exchange.
        /// </summary>
        internal static string OutgoingExchange {
            get {
                return ResourceManager.GetString("OutgoingExchange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Outgoing invoice payment.
        /// </summary>
        internal static string OutgoingInvoicePayment {
            get {
                return ResourceManager.GetString("OutgoingInvoicePayment", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Recharge.
        /// </summary>
        internal static string Recharge {
            get {
                return ResourceManager.GetString("Recharge", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Withdrawal.
        /// </summary>
        internal static string Withdrawal {
            get {
                return ResourceManager.GetString("Withdrawal", resourceCulture);
            }
        }
    }
}
﻿#pragma checksum "..\..\VerIPs.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "E47839D8507737EC83950235F4049ECA83B6E61B4D6F74E1E4A90DC8EF4059B9"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using PumpController;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace PumpController {
    
    
    /// <summary>
    /// VerIPs
    /// </summary>
    public partial class VerIPs : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 43 "..\..\VerIPs.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox ipVox_TextBox;
        
        #line default
        #line hidden
        
        
        #line 49 "..\..\VerIPs.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox ipBridge_TextBox;
        
        #line default
        #line hidden
        
        
        #line 55 "..\..\VerIPs.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox ipServer_TextBox;
        
        #line default
        #line hidden
        
        
        #line 61 "..\..\VerIPs.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox ipLibre_TextBox;
        
        #line default
        #line hidden
        
        
        #line 67 "..\..\VerIPs.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox ruteoEstatico_TextBox;
        
        #line default
        #line hidden
        
        
        #line 78 "..\..\VerIPs.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnGUARDAR;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/PumpController;component/verips.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\VerIPs.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.ipVox_TextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 2:
            this.ipBridge_TextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 3:
            this.ipServer_TextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 4:
            this.ipLibre_TextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 5:
            this.ruteoEstatico_TextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 6:
            this.BtnGUARDAR = ((System.Windows.Controls.Button)(target));
            
            #line 79 "..\..\VerIPs.xaml"
            this.BtnGUARDAR.Click += new System.Windows.RoutedEventHandler(this.BtnGUARDAR_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}


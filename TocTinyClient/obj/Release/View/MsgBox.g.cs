﻿#pragma checksum "..\..\..\View\MsgBox.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "DBC582795B3849889742C4BBD80CF39D8A39C606611651C318F2DD23067615DB"
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

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
using TocTiny.Client.View;


namespace TocTiny.Client.View {
    
    
    /// <summary>
    /// MsgBox
    /// </summary>
    public partial class MsgBox : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 7 "..\..\..\View\MsgBox.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal TocTiny.Client.View.MsgBox WS;
        
        #line default
        #line hidden
        
        
        #line 19 "..\..\..\View\MsgBox.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private System.Windows.Controls.Label BoxTitle;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\..\View\MsgBox.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private System.Windows.Controls.Border BoxBorder;
        
        #line default
        #line hidden
        
        
        #line 33 "..\..\..\View\MsgBox.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private System.Windows.Controls.Label BoxText;
        
        #line default
        #line hidden
        
        
        #line 34 "..\..\..\View\MsgBox.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private System.Windows.Controls.Image BoxImage;
        
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
            System.Uri resourceLocater = new System.Uri("/TocTinyClient;component/view/msgbox.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\View\MsgBox.xaml"
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
            this.WS = ((TocTiny.Client.View.MsgBox)(target));
            return;
            case 2:
            this.BoxTitle = ((System.Windows.Controls.Label)(target));
            return;
            case 3:
            this.BoxBorder = ((System.Windows.Controls.Border)(target));
            return;
            case 4:
            this.BoxText = ((System.Windows.Controls.Label)(target));
            return;
            case 5:
            this.BoxImage = ((System.Windows.Controls.Image)(target));
            
            #line 36 "..\..\..\View\MsgBox.xaml"
            this.BoxImage.MouseUp += new System.Windows.Input.MouseButtonEventHandler(this.BoxImage_MouseUp);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}


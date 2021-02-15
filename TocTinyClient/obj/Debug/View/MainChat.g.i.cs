﻿#pragma checksum "..\..\..\View\MainChat.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "021E1225D138F0F09D2F22CFCB4976E01A099667C8DB5193D0242522E5F29F75"
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
using TocTiny;


namespace TocTiny.View {
    
    
    /// <summary>
    /// MainChat
    /// </summary>
    public partial class MainChat : System.Windows.Window, System.Windows.Markup.IComponentConnector, System.Windows.Markup.IStyleConnector {
        
        
        #line 17 "..\..\..\View\MainChat.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label ChannelName;
        
        #line default
        #line hidden
        
        
        #line 19 "..\..\..\View\MainChat.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ScrollViewer ChatScroller;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\..\View\MainChat.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel ChatMsgContainer;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\..\View\MainChat.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid Stickers;
        
        #line default
        #line hidden
        
        
        #line 114 "..\..\..\View\MainChat.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox InputBox;
        
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
            System.Uri resourceLocater = new System.Uri("/TocTinyClient;component/view/mainchat.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\View\MainChat.xaml"
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
            
            #line 6 "..\..\..\View\MainChat.xaml"
            ((TocTiny.View.MainChat)(target)).Closing += new System.ComponentModel.CancelEventHandler(this.Window_Closing);
            
            #line default
            #line hidden
            
            #line 7 "..\..\..\View\MainChat.xaml"
            ((TocTiny.View.MainChat)(target)).Loaded += new System.Windows.RoutedEventHandler(this.Window_Loaded);
            
            #line default
            #line hidden
            return;
            case 2:
            
            #line 9 "..\..\..\View\MainChat.xaml"
            ((System.Windows.Controls.Grid)(target)).PreviewMouseDown += new System.Windows.Input.MouseButtonEventHandler(this.Grid_PreviewMouseDown);
            
            #line default
            #line hidden
            return;
            case 3:
            this.ChannelName = ((System.Windows.Controls.Label)(target));
            return;
            case 4:
            this.ChatScroller = ((System.Windows.Controls.ScrollViewer)(target));
            return;
            case 5:
            this.ChatMsgContainer = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 6:
            this.Stickers = ((System.Windows.Controls.Grid)(target));
            return;
            case 8:
            
            #line 108 "..\..\..\View\MainChat.xaml"
            ((System.Windows.Controls.Label)(target)).MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.OpenSticker_MouseDown);
            
            #line default
            #line hidden
            return;
            case 9:
            
            #line 109 "..\..\..\View\MainChat.xaml"
            ((System.Windows.Controls.Label)(target)).MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.SendPicture_MouseDown);
            
            #line default
            #line hidden
            return;
            case 10:
            
            #line 110 "..\..\..\View\MainChat.xaml"
            ((System.Windows.Controls.Label)(target)).MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.DrawAttention_MouseDown);
            
            #line default
            #line hidden
            return;
            case 11:
            
            #line 111 "..\..\..\View\MainChat.xaml"
            ((System.Windows.Controls.Label)(target)).MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.OnlineInfo_MouseDown);
            
            #line default
            #line hidden
            return;
            case 12:
            this.InputBox = ((System.Windows.Controls.TextBox)(target));
            
            #line 115 "..\..\..\View\MainChat.xaml"
            this.InputBox.DragEnter += new System.Windows.DragEventHandler(this.InputBox_DragEnter);
            
            #line default
            #line hidden
            
            #line 115 "..\..\..\View\MainChat.xaml"
            this.InputBox.Drop += new System.Windows.DragEventHandler(this.InputBox_Drop);
            
            #line default
            #line hidden
            
            #line 116 "..\..\..\View\MainChat.xaml"
            this.InputBox.KeyDown += new System.Windows.Input.KeyEventHandler(this.InputBox_KeyDown);
            
            #line default
            #line hidden
            return;
            case 13:
            
            #line 118 "..\..\..\View\MainChat.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.Send_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        void System.Windows.Markup.IStyleConnector.Connect(int connectionId, object target) {
            System.Windows.EventSetter eventSetter;
            switch (connectionId)
            {
            case 7:
            eventSetter = new System.Windows.EventSetter();
            eventSetter.Event = System.Windows.Controls.Primitives.Selector.SelectionChangedEvent;
            
            #line 32 "..\..\..\View\MainChat.xaml"
            eventSetter.Handler = new System.Windows.Controls.SelectionChangedEventHandler(this.Sticker_Selected);
            
            #line default
            #line hidden
            ((System.Windows.Style)(target)).Setters.Add(eventSetter);
            break;
            }
        }
    }
}


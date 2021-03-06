﻿#pragma checksum "..\..\..\..\ContentDisplays\DataPlaybackPresentation.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "B57F3F80DA9D477468B991E54D6EEA0CABDE6E263C2A64804AD7E35C4DA31E01"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
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
using SystemView.ContentDisplays;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Animation;
using Telerik.Windows.Controls.Behaviors;
using Telerik.Windows.Controls.Carousel;
using Telerik.Windows.Controls.Data.PropertyGrid;
using Telerik.Windows.Controls.Docking;
using Telerik.Windows.Controls.DragDrop;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Controls.LayoutControl;
using Telerik.Windows.Controls.Legend;
using Telerik.Windows.Controls.MultiColumnComboBox;
using Telerik.Windows.Controls.Primitives;
using Telerik.Windows.Controls.RadialMenu;
using Telerik.Windows.Controls.RibbonView;
using Telerik.Windows.Controls.TransitionEffects;
using Telerik.Windows.Controls.TreeListView;
using Telerik.Windows.Controls.TreeView;
using Telerik.Windows.Controls.Wizard;
using Telerik.Windows.Data;
using Telerik.Windows.DragDrop;
using Telerik.Windows.DragDrop.Behaviors;
using Telerik.Windows.Input.Touch;
using Telerik.Windows.Shapes;


namespace SystemView.ContentDisplays {
    
    
    /// <summary>
    /// DataPlaybackPresentation
    /// </summary>
    public partial class DataPlaybackPresentation : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 14 "..\..\..\..\ContentDisplays\DataPlaybackPresentation.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button PlayData;
        
        #line default
        #line hidden
        
        
        #line 20 "..\..\..\..\ContentDisplays\DataPlaybackPresentation.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button PauseData;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\..\..\ContentDisplays\DataPlaybackPresentation.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button PlaybackCleared;
        
        #line default
        #line hidden
        
        
        #line 85 "..\..\..\..\ContentDisplays\DataPlaybackPresentation.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox SearchBox;
        
        #line default
        #line hidden
        
        
        #line 88 "..\..\..\..\ContentDisplays\DataPlaybackPresentation.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Telerik.Windows.Controls.RadGridView PresentationGrid;
        
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
            System.Uri resourceLocater = new System.Uri("/SystemView;component/contentdisplays/dataplaybackpresentation.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\ContentDisplays\DataPlaybackPresentation.xaml"
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
            this.PlayData = ((System.Windows.Controls.Button)(target));
            
            #line 14 "..\..\..\..\ContentDisplays\DataPlaybackPresentation.xaml"
            this.PlayData.Click += new System.Windows.RoutedEventHandler(this.Resume);
            
            #line default
            #line hidden
            return;
            case 2:
            this.PauseData = ((System.Windows.Controls.Button)(target));
            
            #line 20 "..\..\..\..\ContentDisplays\DataPlaybackPresentation.xaml"
            this.PauseData.Click += new System.Windows.RoutedEventHandler(this.Pause);
            
            #line default
            #line hidden
            return;
            case 3:
            this.PlaybackCleared = ((System.Windows.Controls.Button)(target));
            
            #line 26 "..\..\..\..\ContentDisplays\DataPlaybackPresentation.xaml"
            this.PlaybackCleared.Click += new System.Windows.RoutedEventHandler(this.ClearPlayback);
            
            #line default
            #line hidden
            return;
            case 4:
            
            #line 32 "..\..\..\..\ContentDisplays\DataPlaybackPresentation.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.modifyTriggers);
            
            #line default
            #line hidden
            return;
            case 5:
            
            #line 41 "..\..\..\..\ContentDisplays\DataPlaybackPresentation.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.modifyAdanvancedTriggers);
            
            #line default
            #line hidden
            return;
            case 6:
            
            #line 50 "..\..\..\..\ContentDisplays\DataPlaybackPresentation.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.modifyDisplay);
            
            #line default
            #line hidden
            return;
            case 7:
            
            #line 59 "..\..\..\..\ContentDisplays\DataPlaybackPresentation.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.displayTP_Window);
            
            #line default
            #line hidden
            return;
            case 8:
            
            #line 68 "..\..\..\..\ContentDisplays\DataPlaybackPresentation.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.displayRadio_Window);
            
            #line default
            #line hidden
            return;
            case 9:
            this.SearchBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 10:
            this.PresentationGrid = ((Telerik.Windows.Controls.RadGridView)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}


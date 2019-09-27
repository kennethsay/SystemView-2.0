using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Docking;

namespace SystemView
{
    class CustomDockingPanesFactory : DockingPanesFactory
    {
        protected override void AddPane(RadDocking radDocking, RadPane Pane)
        {
            var _display = Pane.DataContext as DisplayView;
            if (_display != null)
            {
                RadPaneGroup _group = null;
                switch (_display.InitialPosition)
                {
                    case DockState.DockedRight:
                        _group = radDocking.SplitItems.ToList().FirstOrDefault(i => i.Control.Name == "rightGroup") as RadPaneGroup;
                        if (_group != null)
                        {
                            _group.Items.Add(Pane);

                        }
                        return;
                    case DockState.DockedBottom:
                        _group = radDocking.SplitItems.ToList().FirstOrDefault(i => i.Control.Name == "bottomGroup") as RadPaneGroup;
                        if (_group != null)
                        {
                            _group.Items.Add(Pane);
                        }
                        return;
                    case DockState.DockedTop:
                        _group = radDocking.SplitItems.ToList().FirstOrDefault(i => i.Control.Name == "topGroup") as RadPaneGroup;
                        if (_group != null)
                        {
                            _group.Items.Add(Pane);
                        }
                        return;
                    case DockState.DockedLeft:
                        _group = radDocking.SplitItems.ToList().FirstOrDefault(i => i.Control.Name == "leftGroup") as RadPaneGroup;
                        if (_group != null)
                        {
                            _group.Items.Add(Pane);
                        }
                        return;
                    case DockState.FloatingDockable:
                        var fdSplitContainer = radDocking.GeneratedItemsFactory.CreateSplitContainer();
                        _group = radDocking.GeneratedItemsFactory.CreatePaneGroup();
                        fdSplitContainer.Items.Add(_group);
                        _group.Items.Add(Pane);
                        radDocking.Items.Add(fdSplitContainer);
                        Pane.MakeFloatingDockable();
                        return;
                    case DockState.FloatingOnly:
                        var foSplitContainer = radDocking.GeneratedItemsFactory.CreateSplitContainer();
                        _group = radDocking.GeneratedItemsFactory.CreatePaneGroup();
                        foSplitContainer.Items.Add(_group);
                        _group.Items.Add(Pane);
                        radDocking.Items.Add(foSplitContainer);
                        Pane.MakeFloatingOnly();
                        return;
                    default:
                        return;
                }

                base.AddPane(radDocking, Pane);
            }
        }

        protected override RadPane CreatePaneForItem(object item)
        {
            var _display = item as DisplayView;

            if (_display != null)
            {
                var _pane = new RadPane();
                _pane.DataContext = item;
                _pane.Header = _display.Header;
                RadDocking.SetSerializationTag(_pane, _display.Header);

                if(_display.ContentType != null)
                {
                    _pane.Content = Activator.CreateInstance(_display.ContentType);
                }

                return _pane;
            }

            return base.CreatePaneForItem(item);
        }

        protected override void RemovePane(RadPane Pane)
        {
            SystemView.MainWindow._mySessionMgr.RemoveSession(Pane);

            Pane.DataContext = null;
            Pane.Content = null;
            Pane.ClearValue(RadDocking.SerializationTagProperty);
            Pane.RemoveFromParent();
            
        }
    }
}

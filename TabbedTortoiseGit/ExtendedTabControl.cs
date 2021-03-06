﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TabbedTortoiseGit
{
    class ExtendedTabControl : TabControl
    {
        private const int TCM_SETMINTABWIDTH = 0x1300 + 49;

        public event EventHandler NewTabClicked;
        public event EventHandler<TabClosedEventArgs> TabClosed;

        private readonly TabPage _newTab;

        private bool _inhibitControlActions = false;

        public ExtendedTabControl()
        {
            _newTab = new TabPage( "+" );

            this.TabPages.Add( _newTab );
        }

        private TabPage GetTabFromPoint( Point p )
        {
            for( int i = 0; i < this.TabCount; i++ )
            {
                if( this.GetTabRect( i ).Contains( p ) )
                {
                    return this.TabPages[ i ];
                }
            }
            return null;
        }

        protected void OnNewTabClicked( EventArgs e )
        {
            NewTabClicked( this, e );
        }

        protected void OnTabClosed( TabClosedEventArgs e )
        {
            TabClosed( this, e );
        }

        protected override void OnControlAdded( ControlEventArgs e )
        {
            if( _inhibitControlActions )
            {
                return;
            }

            if( e.Control == _newTab )
            {
                return;
            }

            TabPage t = e.Control as TabPage;

            if( t != null )
            {
                if( this.TabPages.IndexOf( t ) == this.TabCount - 1 )
                {
                    _inhibitControlActions = true;
                    this.TabPages.Remove( t );
                    this.TabPages.Insert( this.TabCount - 1, t );
                    _inhibitControlActions = false;
                }
            }

            base.OnControlAdded( e );
        }

        protected override void OnControlRemoved( ControlEventArgs e )
        {
            if( _inhibitControlActions )
            {
                return;
            }

            base.OnControlRemoved( e );
        }

        [DllImport( "user32.dll" )]
        private static extern IntPtr SendMessage( IntPtr hWnd, int msg, IntPtr wp, IntPtr lp );

        protected override void OnHandleCreated( EventArgs e )
        {
            SendMessage( this.Handle, TCM_SETMINTABWIDTH, IntPtr.Zero, (IntPtr)20 );

            base.OnHandleCreated( e );
        }

        protected override void OnSelecting( TabControlCancelEventArgs e )
        {
            if( e.TabPage == _newTab )
            {
                e.Cancel = true;
            }
            else
            {
                base.OnSelecting( e );
            }
        }

        protected override void OnMouseClick( MouseEventArgs e )
        {
            if( !this.DesignMode )
            {
                TabPage tab = this.GetTabFromPoint( e.Location );

                if( tab != null )
                {
                    if( e.Button == MouseButtons.Middle )
                    {
                        if( tab != _newTab )
                        {
                            this.SelectedIndex = Math.Max( 0, this.TabPages.IndexOf( tab ) - 1 );
                            this.TabPages.Remove( tab );
                            OnTabClosed( new TabClosedEventArgs( tab ) );
                            return;
                        }
                    }
                    else if( e.Button == MouseButtons.Left )
                    {
                        if( tab == _newTab )
                        {
                            OnNewTabClicked( EventArgs.Empty );
                            return;
                        }
                    }
                }
            }

            base.OnMouseClick( e );
        }
    }

    public class TabClosedEventArgs : EventArgs
    {
        public TabPage Tab { get; private set; }

        public TabClosedEventArgs( TabPage t )
        {
            Tab = t;
        }
    }
}

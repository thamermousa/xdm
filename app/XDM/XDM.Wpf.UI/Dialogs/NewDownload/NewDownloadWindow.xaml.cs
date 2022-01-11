﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using XDM.Core.Lib.Common;
using XDM.Core.Lib.UI;
using XDM.Wpf.UI.Dialogs.AdvancedDownloadOption;
using XDMApp;

namespace XDM.Wpf.UI.Dialogs.NewDownload
{
    /// <summary>
    /// Interaction logic for NewDownloadWindow.xaml
    /// </summary>
    public partial class NewDownloadWindow : Window, INewDownloadDialogSkeleton
    {
        private AuthenticationInfo? authentication;
        private ProxyInfo? proxy = Config.Instance.Proxy;
        private int speedLimit = Config.Instance.DefaltDownloadSpeed;
        private bool enableSpeedLimit = Config.Instance.EnableSpeedLimit;
        private int previousIndex = 0;

        public NewDownloadWindow()
        {
            InitializeComponent();
        }

        public bool IsEmpty { get => !TxtUrl.IsReadOnly; set => TxtUrl.IsReadOnly = !value; }
        public string Url { get => TxtUrl.Text; set => TxtUrl.Text = value; }
        public AuthenticationInfo? Authentication { get => authentication; set => authentication = value; }
        public ProxyInfo? Proxy { get => proxy; set => proxy = value; }
        public int SpeedLimit { get => speedLimit; set => speedLimit = value; }
        public bool EnableSpeedLimit { get => enableSpeedLimit; set => enableSpeedLimit = value; }
        public string SelectedFileName { get => TxtFile.Text; set => TxtFile.Text = value; }
        public int SeletedFolderIndex
        {
            get => CmbLocation.SelectedIndex;
            set
            {
                CmbLocation.SelectedIndex = value;
                previousIndex = value;
            }
        }

        public event EventHandler? DownloadClicked;
        public event EventHandler? CancelClicked;
        public event EventHandler? DestroyEvent;
        public event EventHandler? BlockHostEvent;
        public event EventHandler? UrlChangedEvent;
        public event EventHandler? UrlBlockedEvent;
        public event EventHandler? QueueSchedulerClicked;
        public event EventHandler<DownloadLaterEventArgs>? DownloadLaterClicked;
        public event EventHandler<FileBrowsedEventArgs>? FileBrowsedEvent;
        public event EventHandler<FileBrowsedEventArgs>? DropdownSelectionChangedEvent;

        public void DisposeWindow()
        {
            this.Close();
        }

        public void Invoke(Action callback)
        {
            Dispatcher.Invoke(callback);
        }

        public void SetFileSizeText(string text)
        {
            this.TxtFileSize.Text = text;
        }

        public void SetFolderValues(string[] values)
        {
            previousIndex = 0;
            CmbLocation.Items.Clear();
            foreach (var item in values)
            {
                CmbLocation.Items.Add(item);
            }
        }

        public void ShowMessageBox(string message)
        {
            MessageBox.Show(this, message);
        }

        public void ShowWindow()
        {
            this.Show();
        }

#if NET45_OR_GREATER
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            if (App.Skin == Skin.Dark)
            {
                var helper = new WindowInteropHelper(this);
                helper.EnsureHandle();
                DarkModeHelper.UseImmersiveDarkMode(helper.Handle, true);
            }
        }
#endif

        private void CmbLocation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbLocation.SelectedIndex == 1)
            {
                CmbLocation.SelectedIndex = previousIndex;
                var fc = new SaveFileDialog();
                fc.Filter = "All files (*.*)|*.*";
                fc.FileName = TxtFile.Text;
                var ret = fc.ShowDialog(this);
                if (ret.HasValue && ret.Value)
                {
                    this.FileBrowsedEvent?.Invoke(this, new FileBrowsedEventArgs(fc.FileName));
                }
            }
            else
            {
                previousIndex = CmbLocation.SelectedIndex;
                this.DropdownSelectionChangedEvent?.Invoke(this, new FileBrowsedEventArgs(CmbLocation.Text));
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.DestroyEvent?.Invoke(this, EventArgs.Empty);
        }

        private void TxtUrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            UrlChangedEvent?.Invoke(sender, e);
        }

        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            DownloadClicked?.Invoke(sender, e);
        }

        private void btnDownloadLater_Click(object sender, RoutedEventArgs e)
        {
            ShowQueuesContextMenu();
        }

        private void btnAdvanced_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new AdvancedDownloadOptionDialog();
            dlg.Owner = this;
            //dlg.Authentication = Authentication;
            //dlg.Proxy = Proxy;
            //dlg.Isspe = EnableSpeedLimit;
            //dlg.SpeedLimit = SpeedLimit;
            dlg.Show();
            this.IsEnabled = false;
            this.IsHitTestVisible = false;
            //if (ret.HasValue && ret.Value)
            //{
            //    //Authentication = dlg.Authentication;
            //    //Proxy = dlg.Proxy;
            //    //EnableSpeedLimit = dlg.EnableSpeedLimit;
            //    //SpeedLimit = dlg.SpeedLimit;
            //}
            //AdvancedDialogHelper.Show(ref authentication, ref proxy, ref enableSpeedLimit, ref speedLimit, this);
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            UrlBlockedEvent?.Invoke(sender, EventArgs.Empty);
        }

        private void ShowQueuesContextMenu()
        {
            var nctx = (ContextMenu)FindResource("DownloadLaterContextMenu");
            nctx.Items.Clear();
            foreach (var queue in QueueManager.Queues)
            {
                var menuItem = new MenuItem
                {
                    Tag = queue.ID,
                    Header = queue.Name
                };
                menuItem.Click += (s, e) =>
                {
                    MenuItem m = (MenuItem)e.OriginalSource;
                    var args = new DownloadLaterEventArgs((string)m.Tag);
                    DownloadLaterClicked?.Invoke(this, args);
                };
                nctx.Items.Add(menuItem);
            }
            nctx.Items.Add(FindResource("DontAddToQueueMenuItem"));
            nctx.Items.Add(FindResource("QueueAndSchedulerMenuItem"));


            nctx.PlacementTarget = btnDownloadLater;
            nctx.Placement = PlacementMode.Bottom;
            nctx.IsOpen = true;
        }
    }
}

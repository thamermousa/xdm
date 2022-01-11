﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace XDM.Wpf.UI.Dialogs.SpeedLimiter
{
    /// <summary>
    /// Interaction logic for SpeedLimiter.xaml
    /// </summary>
    public partial class SpeedLimiter : UserControl
    {
        public SpeedLimiter()
        {
            InitializeComponent();
        }

        public bool IsSpeedLimitEnabled
        {
            get => ChkEnabled.IsChecked.HasValue ? ChkEnabled.IsChecked.Value : false;
            set => ChkEnabled.IsChecked = value;
        }

        public int SpeedLimit
        {
            get
            {
                if (Int32.TryParse(TxtSpeedLimit.Text, out int n))
                {
                    return n;
                }
                return 0;
            }
            set => TxtSpeedLimit.Text = value.ToString();
        }

        private void TxtSpeedLimit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Int32.TryParse(e.Text, out _);
        }

        private void TxtSpeedLimit_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!Int32.TryParse(text, out _))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}

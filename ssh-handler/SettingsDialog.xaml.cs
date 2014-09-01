// Settings Dialog
//
// Douglas Thrift
//
// SettingsDialog.xaml.cs

/*  Copyright 2014 Douglas Thrift
 *
 *  Licensed under the Apache License, Version 2.0 (the "License");
 *  you may not use this file except in compliance with the License.
 *  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

public partial class SettingsDialog : Window
{
    public SettingsDialog(IList<Handler> handlers)
    {
        InitializeComponent();

        IEnumerable<string> options = new string[0];

        using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Default), key = baseKey.OpenSubKey(@"ssh\shell\open\command"))
            if (key != null)
            {
                string command = (string)key.GetValue(null);
                if (!string.IsNullOrWhiteSpace(command))
                {
                    var args = Shell32.CommandLineToArgv(command);

                    options = args.Skip(1).TakeWhile(arg => arg != "%1");
                }
            }

        foreach (Handler handler in handlers)
            SettingsPanel.Children.Add(new HandlerSettingsBox(handler, options));
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (Apply())
            DialogResult = true;
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        Apply();
    }

    private bool Apply()
    {
        var args = new List<string>();

        args.Add(Assembly.GetEntryAssembly().Location);

        foreach (GroupBox box in SettingsPanel.Children)
            if (((RadioButton)box.Header).IsChecked.Value)
            {
                if (box is HandlerSettingsBox)
                    try
                    {
                        args.AddRange(((HandlerSettingsBox)box).Options);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(this, exception.Message, "SSH Handler Settings Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                break;
            }

        args.Add("\"%1\"");

        Debug.WriteLine("{0}", string.Join(" ", args), null);

        return true;
    }
}

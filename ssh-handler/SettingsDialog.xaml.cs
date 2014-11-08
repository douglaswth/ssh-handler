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
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

public partial class SettingsDialog : Window
{
    private bool global = false;

    public SettingsDialog(IList<Handler> handlers, string type)
    {
        switch (type.ToLowerInvariant())
        {
        case "global":
            global = true;

            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
                Elevate();
            break;
        }

        InitializeComponent();

        Title = "SSH Handler " + (global ? "Global" : "User") + " Settings";

        IEnumerable<string> options;

        if (global)
            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default), key = baseKey.OpenSubKey(@"Software\Classes\ssh\shell\open\command"))
                options = Options(key);
        else
            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Default), key = baseKey.OpenSubKey(@"ssh\shell\open\command"))
                options = Options(key);

        foreach (Handler handler in handlers)
            SettingsPanel.Children.Add(new HandlerSettingsBox(handler, options, ApplyButton));

        ApplyButton.IsEnabled = false;
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (Apply())
            DialogResult = true;
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        if (Apply())
            ApplyButton.IsEnabled = false;
    }

    private void RadioButton_Unchecked(object sender, RoutedEventArgs e)
    {
        ApplyButton.IsEnabled = true;
    }

    private void Elevate()
    {
        ProcessStartInfo info = new ProcessStartInfo(Assembly.GetEntryAssembly().Location, "/settings:" + (global ? "global" : "user"));

        info.Verb = "runas";

        Process.Start(info);
        Environment.Exit(0);
    }

    private IEnumerable<string> Options(RegistryKey key)
    {
        if (key != null)
        {
            string command = (string)key.GetValue(null);
            if (!string.IsNullOrWhiteSpace(command))
                return Shell32.CommandLineToArgv(command).Skip(1).TakeWhile(arg => arg != "%1");
        }

        return new string[0];
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

        using (RegistryKey baseKey = RegistryKey.OpenBaseKey(global ? RegistryHive.LocalMachine : RegistryHive.CurrentUser, RegistryView.Default), key = baseKey.CreateSubKey(@"Software\Classes\ssh"), commandKey = key.CreateSubKey(@"shell\open\command"))
        {
            key.SetValue(null, "URL:SSH Protocol");
            key.SetValue("URL Protocol", "");
            commandKey.SetValue(null, string.Join(" ", args));
        }

        return true;
    }
}

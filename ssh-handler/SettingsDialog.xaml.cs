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

using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

public partial class SettingsDialog : Window
{
    public SettingsDialog(IList<Handler> handlers)
    {
        InitializeComponent();

        foreach (Handler handler in handlers)
            SettingsPanel.Children.Add(new HandlerSettingsBox(handler));
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        Apply();
        DialogResult = true;
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        Apply();
    }

    private void Apply()
    {
        string program = Assembly.GetEntryAssembly().Location;
        string[] arguments = { "/openssh", "/bash" };

        Debug.WriteLine("\"{0}\" {1} \"%1\"", program, string.Join(" ", arguments));
    }
}

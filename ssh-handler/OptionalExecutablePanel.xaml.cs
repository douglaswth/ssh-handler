// Optional Executable Panel
//
// Douglas Thrift
//
// OptionalExecutablePanel.xaml.cs

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
using System.Windows.Controls;

public partial class OptionalExecutablePanel : StackPanel
{
    private Setting setting;

    public OptionalExecutablePanel(Setting setting, IEnumerable<string> options)
    {
        InitializeComponent();

        this.setting = setting;

        if (setting.handler)
            SettingUsage.Text = "Use a specific executable for " + setting.name + ":";
        else
        {
            SettingCheckBox.Content = setting.name + " Executable:";
            SettingUsage.Text = setting.usage + ":";
        }
    }

    private void SettingCheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
    {
        SettingExecutablePanel.IsEnabled = true;
    }

    private void SettingCheckBox_Unchecked(object sender, System.Windows.RoutedEventArgs e)
    {
        SettingExecutablePanel.IsEnabled = false;
    }
}

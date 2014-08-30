// Optional Yes/No/Executable Panel
//
// Douglas Thrift
//
// OptionalYesNoExecutablePanel.xaml.cs

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

public partial class OptionalYesNoExecutablePanel : StackPanel
{
    private Setting setting;

    public OptionalYesNoExecutablePanel(Setting setting, IEnumerable<string> options)
    {
        InitializeComponent();

        this.setting = setting;
        SettingCheckBox.Content = setting.name + ":";
        SettingUsage.Text = setting.usage + ":";
    }

    private void SettingCheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
    {
        SettingRadioPanel.IsEnabled = true;
    }

    private void SettingCheckBox_Unchecked(object sender, System.Windows.RoutedEventArgs e)
    {
        SettingRadioPanel.IsEnabled = false;
    }

    private void SettingExecutable_Checked(object sender, System.Windows.RoutedEventArgs e)
    {
        SettingExecutablePanel.IsEnabled = true;
    }

    private void SettingExecutable_Unchecked(object sender, System.Windows.RoutedEventArgs e)
    {
        SettingExecutablePanel.IsEnabled = false;
    }
}

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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls;

public partial class OptionalExecutablePanel : StackPanel, SettingPanel
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

        Regex regex = new Regex(@"^(?:/|--?)" + setting.option.Substring(1) + @"(?:[:=](?<executable>.*))?$");

        foreach (string option in options)
        {
            Match match = regex.Match(option);
            if (match.Success)
            {
                Group group = match.Groups["executable"];
                if (group.Success)
                {
                    SettingCheckBox.IsChecked = true;
                    SettingExecutableBox.Text = group.Value;
                }
            }
        }
    }

    public bool IsSelected
    {
        get
        {
            return setting.handler || SettingCheckBox.IsChecked.Value;
        }
    }

    public string Option
    {
        get
        {
            if (SettingCheckBox.IsChecked.Value)
            {
                string executable = SettingExecutableBox.Text;

                if (File.GetAttributes(executable).HasFlag(FileAttributes.Directory))
                    throw new Exception("'" + executable + "' is not a file.");

                return setting.option + ":" + executable;
            }
            else
                return setting.option;
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

    private void SettingExecutableBrowse_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        SettingPanelHelper.DoExecutableBrowseClick(SettingExecutableBox);
    }

    private void SettingExecutableBox_Populating(object sender, PopulatingEventArgs e)
    {
        SettingPanelHelper.DoExecutableBoxPopulating(SettingExecutableBox);
    }
}

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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Controls;

public partial class OptionalYesNoExecutablePanel : StackPanel, SettingPanel
{
    private Setting setting;

    public OptionalYesNoExecutablePanel(Setting setting, IEnumerable<string> options)
    {
        InitializeComponent();

        this.setting = setting;
        SettingCheckBox.Content = setting.name + ":";
        SettingUsage.Text = setting.usage + ":";

        Regex regex = new Regex(@"^(?:/|--?)" + setting.option.Substring(1) + @"(?:[:=](?<executable>.*))?$");

        foreach (string option in options)
        {
            Match match = regex.Match(option);
            if (match.Success)
            {
                Group group = match.Groups["executable"];
                if (group.Success)
                    switch (group.Value)
                    {
                    case "yes":
                        SettingYes.IsChecked = true;
                        break;
                    case "no":
                        SettingNo.IsChecked = true;
                        break;
                    default:
                        SettingExecutable.IsChecked = true;
                        SettingExecutableBox.Text = group.Value;
                        break;
                    }

                SettingCheckBox.IsChecked = true;
            }
        }
    }

    public bool IsSelected
    {
        get
        {
            return SettingCheckBox.IsChecked.Value;
        }
    }

    public string Option
    {
        get
        {
            if (SettingYes.IsChecked.Value)
                return setting.option + ":yes";
            else if (SettingNo.IsChecked.Value)
                return setting.option + ":no";
            else
            {
                string executable = SettingExecutableBox.Text;

                if (File.GetAttributes(executable).HasFlag(FileAttributes.Directory))
                    throw new Exception("'" + executable + "' is not a file.");

                return setting.option + ":" + executable;
            }
        }
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

    private void SettingExecutableBrowse_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        SettingPanelHelper.DoExecutableBrowseClick(SettingExecutableBox);
    }

    private void SettingExecutableBox_Populating(object sender, PopulatingEventArgs e)
    {
        SettingPanelHelper.DoExecutableBoxPopulating(SettingExecutableBox);
    }
}

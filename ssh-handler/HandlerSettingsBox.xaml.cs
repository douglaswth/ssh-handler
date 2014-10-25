// Handler Settings Box
//
// Douglas Thrift
//
// HandlerSettingsBox.xaml.cs

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
using System.Text.RegularExpressions;
using System.Windows.Controls;

public partial class HandlerSettingsBox : GroupBox
{
    private Handler handler;
    private Button applyButton;

    public HandlerSettingsBox(Handler handler, IEnumerable<string> options, Button applyButton)
    {
        InitializeComponent();

        this.applyButton = applyButton;
        this.handler = handler;
        HandlerRadioButton.Content = handler.Setting.usage;

        foreach (Setting setting in handler.Settings)
            switch (setting.type)
            {
            case SettingType.OptionalExecutable:
                SettingsPanel.Children.Add(new OptionalExecutablePanel(setting, options, applyButton));
                break;
            case SettingType.OptionalYesNoExecutable:
                SettingsPanel.Children.Add(new OptionalYesNoExecutablePanel(setting, options, applyButton));
                break;
            case SettingType.OptionalYesNoDirectory:
                SettingsPanel.Children.Add(new OptionalYesNoDirectoryPanel(setting, options, applyButton));
                break;
            }

        Regex regex = new Regex(@"^(?:/|--?)" + handler.Setting.option.Substring(1) + @"(?:[:=].*)?$");

        foreach (string option in options)
            if (regex.IsMatch(option))
                HandlerRadioButton.IsChecked = true;
    }

    public IEnumerable<string> Options
    {
        get
        {
            var options = new List<string>();

            foreach (SettingPanel panel in SettingsPanel.Children)
                if (panel.IsSelected)
                    options.Add(panel.Option);

            return options;
        }
    }

    private void HandlerRadioButton_Checked(object sender, System.Windows.RoutedEventArgs e)
    {
        SettingsPanel.IsEnabled = true;
    }

    private void HandlerRadioButton_Unchecked(object sender, System.Windows.RoutedEventArgs e)
    {
        SettingsPanel.IsEnabled = false;
        applyButton.IsEnabled = true;
    }
}

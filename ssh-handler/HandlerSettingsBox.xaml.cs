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
using System.Windows.Controls;

public partial class HandlerSettingsBox : GroupBox
{
    private Handler handler;

    public HandlerSettingsBox(Handler handler, IEnumerable<string> options)
    {
        InitializeComponent();

        this.handler = handler;
        HandlerRadioButton.Content = handler.Setting.usage;

        foreach (Setting setting in handler.Settings)
            switch (setting.type)
            {
            case SettingType.OptionalExecutable:
                SettingsPanel.Children.Add(new OptionalExecutablePanel(setting, options));
                break;
            case SettingType.OptionalYesNoExecutable:
                SettingsPanel.Children.Add(new OptionalYesNoExecutablePanel(setting, options));
                break;
            case SettingType.OptionalYesNoDirectory:
                SettingsPanel.Children.Add(new OptionalYesNoDirectoryPanel(setting, options));
                break;
            }
    }

    private void HandlerRadioButton_Checked(object sender, System.Windows.RoutedEventArgs e)
    {
        SettingsPanel.IsEnabled = true;
    }

    private void HandlerRadioButton_Unchecked(object sender, System.Windows.RoutedEventArgs e)
    {
        SettingsPanel.IsEnabled = false;
    }
}

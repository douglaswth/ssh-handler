// Setting Panel Helper
//
// Douglas Thrift
//
// SettingPanelHelper.cs

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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Windows.Controls;

public static class SettingPanelHelper
{
    public static void DoExecutableBrowseClick(AutoCompleteBox executableBox)
    {
        var dialog = new System.Windows.Forms.OpenFileDialog();

        dialog.AutoUpgradeEnabled = true;

        string pathext = Environment.GetEnvironmentVariable("PathExt");

        if (pathext == null)
            pathext = ".com;.exe;.bat;.cmd";

        pathext = string.Join(";", pathext.Split(';').Select(extension => "*" + extension.ToLowerInvariant()));

        dialog.Filter = string.Format("Executable files ({0})|{0}|All files (*.*)|*.*", pathext);
        dialog.FilterIndex = 1;
        dialog.RestoreDirectory = true;

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            executableBox.Text = dialog.FileName;
    }

    public static void DoExecutableBoxPopulating(AutoCompleteBox executableBox)
    {
        try
        {
            string directoryName = Path.GetDirectoryName(executableBox.Text);
            if (directoryName == null)
                directoryName = Path.GetPathRoot(executableBox.Text);

            if (Directory.Exists(directoryName))
            {
                var directoryInfo = new DirectoryInfo(directoryName.EndsWith(":") ? directoryName + @"\" : directoryName);
                executableBox.ItemsSource = directoryInfo.EnumerateFileSystemInfos().Where(info => !info.Attributes.HasFlag(FileAttributes.Hidden)).Select(info => info.FullName);
                executableBox.PopulateComplete();
            }
        }
        catch (ArgumentException exception)
        {
            Debug.WriteLine("{0}: {1}", exception, exception.Message);
        }
        catch (PathTooLongException exception)
        {
            Debug.WriteLine("{0}: {1}", exception, exception.Message);
        }
        catch (SecurityException exception)
        {
            Debug.WriteLine("{0}: {1}", exception, exception.Message);
        }
    }

    public static void DoDirectoryBrowseClick(AutoCompleteBox directoryBox)
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog();

        dialog.ShowNewFolderButton = false;

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            directoryBox.Text = dialog.SelectedPath;
    }

    public static void DoDirectoryBoxPopulating(AutoCompleteBox directoryBox)
    {
        try
        {
            string directoryName = Path.GetDirectoryName(directoryBox.Text);
            if (directoryName == null)
                directoryName = Path.GetPathRoot(directoryBox.Text);

            if (Directory.Exists(directoryName))
            {
                var directoryInfo = new DirectoryInfo(directoryName.EndsWith(":") ? directoryName + @"\" : directoryName);
                directoryBox.ItemsSource = directoryInfo.EnumerateDirectories().Where(info => !info.Attributes.HasFlag(FileAttributes.Hidden)).Select(info => info.FullName);
                directoryBox.PopulateComplete();
            }
        }
        catch (ArgumentException exception)
        {
            Debug.WriteLine("{0}: {1}", exception, exception.Message);
        }
        catch (PathTooLongException exception)
        {
            Debug.WriteLine("{0}: {1}", exception, exception.Message);
        }
        catch (SecurityException exception)
        {
            Debug.WriteLine("{0}: {1}", exception, exception.Message);
        }
    }
}

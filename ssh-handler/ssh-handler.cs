// SSH Handler
//
// Douglas Thrift
//
// ssh-handler.cs

/*  Copyright 2013 Douglas Thrift
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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;

public class SshHandler
{
    private enum Handler { Unspecified, Putty };
    private static Handler handler = Handler.Unspecified;
    private static string puttyPath = null;

    public static void Main(string[] args)
    {
        Application.EnableVisualStyles();

        try
        {
            Regex usage = new Regex(@"^(?:/|--?)(?:h|help|usage|\?)$", RegexOptions.IgnoreCase);
            Regex putty = new Regex(@"^(?:/|--?)putty(?:[:=](?<putty_path>.*))?$", RegexOptions.IgnoreCase);
            Uri uri = null;

            foreach (string arg in args)
            {
                if (usage.IsMatch(arg))
                {
                    Usage();
                    return;
                }

                Match match;

                if ((match = putty.Match(arg)).Success)
                {
                    handler = Handler.Putty;
                    Group group = match.Groups["putty_path"];
                    if (group.Success)
                        puttyPath = group.Value;
                }
                else
                    uri = new Uri(arg, UriKind.Absolute);
            }

            if (uri != null)
                switch (handler)
                {
                    case Handler.Unspecified:
                        if (FindPutty())
                            Putty(uri);
                        else
                            throw new Exception("Could not find a suitable SSH application.");
                        break;
                    case Handler.Putty:
                        Putty(uri);
                        break;
                }
            else
                Usage();
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message, "SSH Handler Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static void Usage()
    {
        MessageBox.Show("ssh-handler [/putty[:<putty-path>]] <ssh-url>\n\n" +
            "/putty[:<putty-path>] -- Use PuTTY to connect", "SSH Handler Usage", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static void UserPassword(Uri uri, out string user, out string password)
    {
        if (uri.UserInfo.Length != 0)
        {
            string[] userInfo = uri.UserInfo.Split(new char[] { ':' }, 2);
            user = userInfo[0];
            password = userInfo.Length == 2 ? userInfo[1] : null;
        }
        else
        {
            user = null;
            password = null;
        }
    }

    private static bool FindPutty()
    {
        if (puttyPath != null)
            return true;

        foreach (RegistryHive hive in new RegistryHive[] { RegistryHive.CurrentUser, RegistryHive.LocalMachine })
        {
            IList<RegistryView> views = new List<RegistryView>(new RegistryView[] { RegistryView.Registry32 });
            if (Environment.Is64BitOperatingSystem)
                views.Insert(0, RegistryView.Registry64);

            foreach (RegistryView view in views)
                using (RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, view), key = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\PuTTY_is1"))
                    if (key != null)
                    {
                        string path = (string)key.GetValue("InstallLocation");
                        if (path == null)
                            continue;
                        puttyPath = Path.Combine(path, "putty.exe");
                        if (File.Exists(puttyPath))
                        {
                            Debug.WriteLine("Found PuTTY in registry: {0}", puttyPath, null);
                            return true;
                        }
                        else
                            puttyPath = null;
                    }
        }

        foreach (string path in Environment.GetEnvironmentVariable("PATH").Split(Path.PathSeparator))
        {
            puttyPath = Path.Combine(path, "putty.exe");
            if (File.Exists(puttyPath))
            {
                Debug.WriteLine("Found PuTTY in path: {0}", puttyPath, null);
                return true;
            }
            else
                puttyPath = null;
        }

        return false;
    }

    private static void Putty(Uri uri)
    {
        if (!FindPutty())
            throw new Exception("Could not find PuTTY executable.");

        string user, password;
        UserPassword(uri, out user, out password);

        StringBuilder args = new StringBuilder();
        if (password != null)
            args.AppendFormat("-pw {0} ", password);
        if (uri.Port != -1)
            args.AppendFormat("-P {0} ", uri.Port);
        if (user != null)
            args.AppendFormat("{0}@", user);
        args.Append(uri.Host);

        Debug.WriteLine("Running PuTTY command: {0} {1}", puttyPath, args);
        Process.Start(puttyPath, args.ToString());
    }
}

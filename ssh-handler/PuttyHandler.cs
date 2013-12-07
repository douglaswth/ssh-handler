// PuTTY Handler
//
// Douglas Thrift
//
// PuttyHandler.cs

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
using Microsoft.Win32;

public class PuttyHandler : AbstractHandler, Handler
{
    private Regex regex = new Regex(@"^(?:/|--?)putty(?:[:=](?<putty_path>.*))?$", RegexOptions.IgnoreCase);
    private string path = null;

    public IList<string> Usages
    {
        get
        {
            return new string[] { "/putty[:<putty-path>] -- Use PuTTY to connect" };
        }
    }

    public MatchOption DoMatch(string arg)
    {
        Match match;

        if ((match = regex.Match(arg)).Success)
            return SetValue(match, "putty_path", ref path);
        else
            return MatchOption.None;
    }

    public bool Find()
    {
        if (path != null)
            goto Found;

        foreach (var hive in new RegistryHive[] { RegistryHive.CurrentUser, RegistryHive.LocalMachine })
        {
            var views = new List<RegistryView>(new RegistryView[] { RegistryView.Registry32 });
            if (Environment.Is64BitOperatingSystem)
                views.Insert(0, RegistryView.Registry64);

            foreach (RegistryView view in views)
                using (RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, view), key = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\PuTTY_is1"))
                    if (key != null)
                    {
                        string location = (string)key.GetValue("InstallLocation");
                        if (location == null)
                            continue;
                        path = Path.Combine(location, "putty.exe");
                        if (File.Exists(path))
                        {
                            Debug.WriteLine("Found PuTTY in registry: {0}", path, null);
                            goto Found;
                        }
                        else
                            path = null;
                    }
        }

        if ((path = FindInPath("putty.exe")) != null)
        {
            Debug.WriteLine("Found PuTTY in path: {0}", path, null);
            goto Found;
        }

        return false;

    Found:
        path = path.Trim();
        return true;
    }

    public void Execute(Uri uri, string user, string password)
    {
        if (!Find())
            throw new Exception("Could not find PuTTY executable.");

        StringBuilder args = new StringBuilder();
        if (password != null)
            args.AppendFormat("-pw \"{0}\" ", password);
        if (uri.Port != -1)
            args.AppendFormat("-P {0} ", uri.Port);
        if (user != null)
            args.AppendFormat("{0}@", user);
        args.Append(uri.Host);

        Debug.WriteLine("Running PuTTY command: {0} {1}", path, args);
        Process.Start(path, args.ToString());
    }
}

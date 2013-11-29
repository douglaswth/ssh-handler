// OpenSSH Handler
//
// Douglas Thrift
//
// OpensshHandler.cs

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

public class OpensshHandler : AbstractHandler, Handler
{
    private Regex regex = new Regex(@"^(?:/|--?)openssh(?:[:=](?<openssh_path>.*))?$", RegexOptions.IgnoreCase);
    private Regex cygwinRegex = new Regex(@"^(?:/|--?)cygwin(?:[:=](?<cygwin_path>.*))?$", RegexOptions.IgnoreCase);
    private Regex minttyRegex = new Regex(@"^(?:/|--?)mintty(?:[:=](?<mintty_path>.*))?$", RegexOptions.IgnoreCase);
    private AutoYesNoOption cygwin = AutoYesNoOption.Auto;
    private AutoYesNoOption mintty = AutoYesNoOption.Auto;
    private string path = null;
    string cygwinPath = null;
    string minttyPath = null;

    public IList<string> Usages
    {
        get
        {
            return new string[]
            {
                "/openssh[:<openssh-path>] -- Use OpenSSH to connect",
                "/cygwin[:(yes|no|<cygwin-path>)] -- Use Cygwin for OpenSSH (by default, Cygwin will be used for OpenSSH if detected)",
                "/mintty[:(yes|no|<mintty-path>)] -- Use MinTTY for OpenSSH (by default, MinTTY will be used for OpenSSH if detected)",
            };
        }
    }

    public MatchOption DoMatch(string arg)
    {
        Match match;

        if ((match = regex.Match(arg)).Success)
        {
            Group group = match.Groups["openssh_path"];
            if (group.Success)
                path = group.Value;
            return MatchOption.Set;
        }
        else if ((match = cygwinRegex.Match(arg)).Success)
        {
            Group group = match.Groups["cygwin_path"];
            if (group.Success)
                SetYesNoValue(group.Value, out cygwin, out cygwinPath);
            return MatchOption.Option;
        }
        else if ((match = minttyRegex.Match(arg)).Success)
        {
            Group group = match.Groups["mintty_path"];
            if (group.Success)
                SetYesNoValue(group.Value, out mintty, out minttyPath);
            return MatchOption.Option;
        }
        else
            return MatchOption.None;
    }

    public bool Find()
    {
        if (path != null)
            goto Found;

        switch (cygwin)
        {
        case AutoYesNoOption.Auto:
        case AutoYesNoOption.Yes:
            if (FindCygwin())
                goto Found;
            break;
        }

        if ((path = FindInPath("ssh.exe")) != null)
        {
            Debug.WriteLine("Found OpenSSH in path: {0}", path, null);
            goto Found;
        }

        return false;

    Found:
        path = path.Trim();
        switch (mintty)
        {
        case AutoYesNoOption.Auto:
        case AutoYesNoOption.Yes:
            FindMintty();
            break;
        }
        return true;
    }

    public void Execute(Uri uri, string user, string password)
    {
        if (!Find())
            throw new Exception("Could not find OpenSSH executable.");

        string command = path;
        StringBuilder args = new StringBuilder();

        if (minttyPath != null)
        {
            command = minttyPath;
            if (cygwinPath != null)
            {
                string icon = Path.Combine(cygwinPath, "Cygwin-Terminal.ico");
                if (File.Exists(icon))
                    args.AppendFormat("-i {0} ", icon);
            }
            args.AppendFormat("-e {0} ", path);
        }

        if (password != null)
            Debug.WriteLine("Warning: OpenSSH does not support passing a password.");
        if (uri.Port != -1)
            args.AppendFormat("-p {0} ", uri.Port);
        if (user != null)
            args.AppendFormat("{0}@", user);
        args.Append(uri.Host);

        Debug.WriteLine("Running OpenSSH command: {0} {1}", command, args);
        Process.Start(command, args.ToString());
    }

    private bool FindCygwin()
    {
        if (cygwinPath != null)
            goto Found;

        foreach (var hive in new RegistryHive[] { RegistryHive.CurrentUser, RegistryHive.LocalMachine })
        {
            var views = new List<RegistryView>(new RegistryView[] { RegistryView.Registry32 });
            if (Environment.Is64BitOperatingSystem)
                views.Insert(0, RegistryView.Registry64);

            foreach (RegistryView view in views)
                using (RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, view), key = baseKey.OpenSubKey(@"SOFTWARE\Cygwin\setup"))
                    if (key != null)
                    {
                        cygwinPath = (string)key.GetValue("rootdir");
                        if (cygwinPath == null)
                            continue;
                        if (Directory.Exists(cygwinPath))
                        {
                            Debug.WriteLine("Found Cygwin in registry: {0}", cygwinPath, null);
                            goto Found;
                        }
                        else
                            cygwinPath = null;
                    }
        }

        if (cygwin == AutoYesNoOption.Yes)
            throw new Exception("Could not find Cygwin in registry.");
        return false;

    Found:
        cygwinPath = cygwinPath.Trim();
        path = Path.Combine(cygwinPath, "bin", "ssh.exe");
        if (File.Exists(path))
        {
            Debug.WriteLine("Found OpenSSH in Cygwin directory: {0}", path, null);
            return true;
        }
        else if (cygwin == AutoYesNoOption.Yes)
            throw new Exception("Could not find OpenSSH in Cygwin directory.");
        else
        {
            path = null;
            return false;
        }
    }

    private void FindMintty()
    {
        if (minttyPath != null)
            goto Found;

        if (cygwinPath != null)
        {
            minttyPath = Path.Combine(cygwinPath, "bin", "mintty.exe");
            if (File.Exists(minttyPath))
            {
                Debug.WriteLine("Found MinTTY in Cygwin directory: {0}", minttyPath, null);
                goto Found;
            }
            else
                minttyPath = null;
        }

        if ((minttyPath = FindInPath("mintty.exe")) != null)
        {
            Debug.WriteLine("Found MinTTY in path: {0}", minttyPath, null);
            goto Found;
        }

        if (mintty == AutoYesNoOption.Yes)
            throw new Exception("Could no find MinTTY executable.");
        return;

    Found:
        minttyPath = minttyPath.Trim();
    }
}

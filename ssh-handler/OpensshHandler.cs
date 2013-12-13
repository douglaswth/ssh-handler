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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

public class OpensshHandler : AbstractHandler, Handler
{
    private Regex regex = new Regex(@"^(?:/|--?)openssh(?:[:=](?<openssh_path>.*))?$", RegexOptions.IgnoreCase);
    private Regex cygwinRegex = new Regex(@"^(?:/|--?)cygwin(?:[:=](?<cygwin_path>.*))?$", RegexOptions.IgnoreCase);
    private Regex minttyRegex = new Regex(@"^(?:/|--?)mintty(?:[:=](?<mintty_path>.*))?$", RegexOptions.IgnoreCase);
    private Regex bashRegex = new Regex(@"^(?:/|--?)bash(?:[:=](?<bash_path>.*))?$", RegexOptions.IgnoreCase);
    private AutoYesNoOption cygwin = AutoYesNoOption.Auto;
    private AutoYesNoOption mintty = AutoYesNoOption.Auto;
    private bool bash = false;
    private string path = null;
    string cygwinPath = null;
    string minttyPath = null;
    string bashPath = null;

    public IList<string> Options
    {
        get
        {
            return new string[]
            {
                "/openssh[:<openssh-path>]",
                "/cygwin[:(yes|no|<cygwin-path>)]",
                "/mintty[:(yes|no|<mintty-path>)]",
                "/bash[:(yes|no|<bash-path>)]",
            };
        }
    }

    public IList<string> Usages
    {
        get
        {
            return new string[]
            {
                "Use OpenSSH to connect",
                "Use Cygwin for OpenSSH (by default, Cygwin will be used for OpenSSH if detected)",
                "Use MinTTY for OpenSSH (by default, MinTTY will be used for OpenSSH if detected)",
                "Use Bash login shell for use with ssh-agent",
            };
        }
    }

    public MatchOption DoMatch(string arg)
    {
        Match match;

        if ((match = regex.Match(arg)).Success)
            return SetValue(match, "openssh_path", ref path);
        else if ((match = cygwinRegex.Match(arg)).Success)
            return SetYesNoValue(match, "cygwin_path", out cygwin, ref cygwinPath);
        else if ((match = minttyRegex.Match(arg)).Success)
            return SetYesNoValue(match, "mintty_path", out mintty, ref minttyPath);
        else if ((match = bashRegex.Match(arg)).Success)
            return SetBooleanValue(match, "bash_path", out bash, ref bashPath);
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
        if (bash)
            FindBash();
        return true;
    }

    public void Execute(Uri uri, string user, string password)
    {
        if (!Find())
            throw new Exception("Could not find OpenSSH executable.");

        if (cygwinPath != null && bash)
        {
            ProcessStartInfo info = new ProcessStartInfo(Path.Combine(cygwinPath, "bin", "cygpath.exe"), CygwinQuote(path));

            info.CreateNoWindow = true;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            info.UseShellExecute = false;

            Process process = Process.Start(info);
            string error = process.StandardError.ReadToEnd().Trim();

            path = process.StandardOutput.ReadToEnd().Trim();

            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new Exception(error);
        }

        var command = new List<string>(new string[] { path });

        if (password != null)
            Debug.WriteLine("Warning: OpenSSH does not support passing a password.");
        if (uri.Port != -1)
            AddArguments(command, "-p", uri.Port);
        AddArguments(command, user != null ? string.Format("{0}@{1}", user, uri.Host) : uri.Host);

        if (bash)
        {
            command = new List<string>(new string[] { bashPath, "-lc", CygwinCommand(command) });
        }

        if (minttyPath != null)
        {
            var minttyCommand = new List<string>(new string[] { minttyPath });
            
            if (cygwinPath != null)
            {
                string icon = Path.Combine(cygwinPath, "Cygwin-Terminal.ico");
                if (File.Exists(icon))
                    AddArguments(minttyCommand, "-i", icon);
            }

            AddArguments(minttyCommand, "-e");
            command.InsertRange(0, minttyCommand);
        }

        string arguments = CygwinCommand(command.Skip(1));
        Debug.WriteLine("Running OpenSSH command: {0} {1}", command.First(), arguments);
        Process.Start(command.First(), arguments);
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
            throw new Exception("Could not find MinTTY executable.");
        return;

    Found:
        minttyPath = minttyPath.Trim();
    }

    private void FindBash()
    {
        if (bashPath != null)
            goto Found;

        if (cygwinPath != null)
        {
            bashPath = Path.Combine(cygwinPath, "bin", "bash.exe");
            if (File.Exists(bashPath))
            {
                Debug.WriteLine("Found Bash in Cygwin directory: {0}", bashPath, null);
                goto Found;
            }
            else
                bashPath = null;
        }

        if ((bashPath = FindInPath("bash.exe")) != null)
        {
            Debug.WriteLine("Found Bash in path: {0}", bashPath, null);
            goto Found;
        }

        throw new Exception("Could not find Bash executable.");

    Found:
        bashPath = bashPath.Trim();
    }

    private string CygwinQuote(string value)
    {
        return string.Format("'{0}'", value.Replace("\"", "\\\"").Replace('\'', '"'));
    }

    private string CygwinAutoQuote(string value)
    {
        if (Regex.IsMatch(value, "[ '\"]"))
            return CygwinQuote(value);
        else
            return value;
    }

    private string CygwinCommand(IEnumerable<string> command)
    {
        return string.Join(" ", command.Select(item => CygwinAutoQuote(item)));
    }
}

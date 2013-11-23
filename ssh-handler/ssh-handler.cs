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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;

public enum MatchOption
{
    None,
    Set,
    Option,
}

public enum AutoYesNoOption
{
    Auto,
    Yes,
    No,
}

public interface Handler
{
    IList<string> Usages
    {
        get;
    }

    MatchOption DoMatch(string arg);
    bool Find();
    void Execute(Uri uri, string user, string password);
}

public abstract class AbstractHandler
{
    protected string FindInPath(string program)
    {
        foreach (string location in Environment.GetEnvironmentVariable("PATH").Split(Path.PathSeparator))
        {
            string path = Path.Combine(location, program);
            if (File.Exists(path))
                return path;
        }

        return null;
    }

    protected void SetYesNoValue(string input, out AutoYesNoOption option, out string value)
    {
        value = null;

        switch (input.ToLowerInvariant())
        {
        case "yes":
            option = AutoYesNoOption.Yes;
            break;
        case "no":
            option = AutoYesNoOption.No;
            break;
        default:
            value = input;
            goto case "yes";
        }
    }
}

public class Putty : AbstractHandler, Handler
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
        {
            Group group = match.Groups["putty_path"];
            if (group.Success)
                path = group.Value;
            return MatchOption.Set;
        }
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
            args.AppendFormat("-pw {0} ", password);
        if (uri.Port != -1)
            args.AppendFormat("-P {0} ", uri.Port);
        if (user != null)
            args.AppendFormat("{0}@", user);
        args.Append(uri.Host);

        Debug.WriteLine("Running PuTTY command: {0} {1}", path, args);
        Process.Start(path, args.ToString());
    }
}

public class Openssh : AbstractHandler, Handler
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

public class SshHandler
{
    private static IList<Handler> handlers = new Handler[]
    {
        new Putty(),
        new Openssh(),
    };
    private static Handler handler = null;

    public static int Main(string[] args)
    {
        Application.EnableVisualStyles();

        try
        {
            Regex usage = new Regex(@"^(?:/|--?)(?:h|help|usage|\?)$", RegexOptions.IgnoreCase);
            IList<string> uriParts = null;

            foreach (string arg in args)
                if (uriParts == null)
                {
                    if (usage.IsMatch(arg))
                        return Usage(0);

                    if (!MatchHandler(arg))
                        uriParts = new List<string>(new string[] { arg });
                }
                else
                    uriParts.Add(arg);

            if (uriParts != null)
            {
                Uri uri = new Uri(string.Join(" ", uriParts), UriKind.Absolute);
                string user, password;

                SetUserPassword(uri, out user, out password);

                if (handler == null)
                    handler = FindHandler();

                handler.Execute(uri, user, password);
            }
            else
                return Usage(1);
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message, "SSH Handler Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 2;
        }

        return 0;
    }

    private static int Usage(int code)
    {
        MessageBox.Show("ssh-handler [/putty[:<putty-path>]] <ssh-url>\n\n" +
            string.Join("\n\n", handlers.SelectMany(handler => handler.Usages)), "SSH Handler Usage",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        return code;
    }

    private static bool MatchHandler(string arg)
    {
        foreach (Handler handler in handlers)
            switch (handler.DoMatch(arg))
            {
            case MatchOption.Set:
                Debug.WriteLine("Setting handler: {0}", handler, null);
                SshHandler.handler = handler;
                goto case MatchOption.Option;
            case MatchOption.Option:
                return true;
            }

        return false;
    }

    private static void SetUserPassword(Uri uri, out string user, out string password)
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

    private static Handler FindHandler()
    {
        foreach (Handler handler in handlers)
            if (handler.Find())
                return handler;

        throw new Exception("Could not find a suitable SSH application.");
    }
}

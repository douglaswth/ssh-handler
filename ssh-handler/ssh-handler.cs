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

public enum Option
{
    None,
    Set,
    Optional,
}

public interface Handler
{
    IList<string> Usages
    {
        get;
    }

    Option DoMatch(string arg);
    bool Find();
    void Execute(Uri uri, string user, string password);
}

public abstract class FindInPathMixin
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
}

public class Putty : FindInPathMixin, Handler
{
    private Regex option = new Regex(@"^(?:/|--?)putty(?:[:=](?<putty_path>.*))?$", RegexOptions.IgnoreCase);
    private string path = null;

    public IList<string> Usages
    {
        get
        {
            return new string[] { "/putty[:<putty-path>] -- Use PuTTY to connect" };
        }
    }

    public Option DoMatch(string arg)
    {
        Match match;

        if ((match = option.Match(arg)).Success)
        {
            Group group = match.Groups["putty_path"];
            if (group.Success)
                path = group.Value;
            return Option.Set;
        }
        else
            return Option.None;
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

public class Openssh : FindInPathMixin, Handler
{
    private Regex option = new Regex(@"^(?:/|--?)openssh(?:[:=](?<openssh_path>.*))?$", RegexOptions.IgnoreCase);
    private string path = null;

    public IList<string> Usages
    {
        get
        {
            return new string[]
            {
                "/openssh[:<openssh-path>] -- Use OpenSSH to connect",
            };
        }
    }

    public Option DoMatch(string arg)
    {
        Match match;

        if ((match = option.Match(arg)).Success)
        {
            Group group = match.Groups["openssh_path"];
            if (group.Success)
                path = group.Value;
            return Option.Set;
        }
        else
            return Option.None;
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
                using (RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, view), key = baseKey.OpenSubKey(@"SOFTWARE\Cygwin\setup"))
                    if (key != null)
                    {
                        string location = (string)key.GetValue("rootdir");
                        if (location == null)
                            continue;
                        path = Path.Combine(location, "bin", "ssh.exe");
                        if (File.Exists(path))
                        {
                            Debug.WriteLine("Found OpenSSH in registry: {0}", path, null);
                            goto Found;
                        }
                        else
                            path = null;
                    }
        }

        if ((path = FindInPath("ssh.exe")) != null)
        {
            Debug.WriteLine("Found OpenSSH in path: {0}", path, null);
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
            throw new Exception("Could not find OpenSSH executable.");

        StringBuilder args = new StringBuilder();
        if (password != null)
            Debug.WriteLine("Warning: OpenSSH does not support passing a password!");
        if (uri.Port != -1)
            args.AppendFormat("-p {0} ", uri.Port);
        if (user != null)
            args.AppendFormat("{0}@", user);
        args.Append(uri.Host);

        Debug.WriteLine("Running OpenSSH command: {0} {1}", path, args);
        Process.Start(path, args.ToString());
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
            string.Join("\n", handlers.SelectMany(handler => handler.Usages)), "SSH Handler Usage", MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        return code;
    }

    private static bool MatchHandler(string arg)
    {
        foreach (Handler handler in handlers)
            switch (handler.DoMatch(arg))
            {
            case Option.Set:
                Debug.WriteLine("Setting handler: {0}", handler, null);
                SshHandler.handler = handler;
                goto case Option.Optional;
            case Option.Optional:
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

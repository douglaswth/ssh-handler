// SSH Handler
//
// Douglas Thrift
//
// SshHandler.cs

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
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

public class SshHandler
{
    private static IList<Handler> handlers = new Handler[]
    {
        new PuttyHandler(),
        new OpensshHandler(),
    };
    private static Handler handler = null;

    [STAThread]
    public static int Main(string[] args)
    {
        Application.EnableVisualStyles();

        try
        {
            Regex usage = new Regex(@"^(?:/|--?)(?:h|help|usage|\?)$", RegexOptions.IgnoreCase);
            Regex settings = new Regex(@"^(?:/|--?)settings$", RegexOptions.IgnoreCase);
            IList<string> uriParts = null;

            foreach (string arg in args)
                if (uriParts == null)
                {
                    if (usage.IsMatch(arg))
                        return Usage(0);

                    if (settings.IsMatch(arg))
                        return Settings();

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
        MessageBox.Show("ssh-handler [/settings] " +
            string.Join(" ", handlers.SelectMany(handler => handler.Options.Select(option => "[" + option + "]"))) +
            " <ssh-url>\n\n" +
            "/settings -- Show settings dialog\n\n" +
            string.Join("\n\n", handlers.SelectMany(handler => handler.Options.Zip(handler.Usages, (option, usage) => option + " -- " + usage))),
            "SSH Handler Usage",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
        return code;
    }

    private static int Settings()
    {
        SettingsDialog settings = new SettingsDialog(handlers);
        Nullable<bool> result = settings.ShowDialog();
        Debug.WriteLine("Settings result: {0}", result, null);

        return 0;
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

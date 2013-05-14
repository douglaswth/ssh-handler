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

using System.Diagnostics;
using System.Text;

public class SshHandler
{
    public static void Main(string[] args)
    {
        foreach (string arg in args)
            Ssh(arg);
    }

    private static void Ssh(string arg)
    {
        System.Uri uri = new System.Uri(arg);
        string user = null, password = null;

        if (uri.UserInfo.Length != 0)
        {
            string[] userInfo = uri.UserInfo.Split(new char[] { ':' }, 2);

            user = userInfo[0];

            if (userInfo.Length == 2)
                password = userInfo[1];
        }

        StringBuilder args = new StringBuilder();

        if (password != null)
            args.Append("-pw ").Append(password).Append(' ');

        if (user != null)
            args.Append(user).Append('@');

        args.Append(uri.Host);

        if (uri.Port != -1)
            args.Append(':').Append(uri.Port);

        Process.Start(@"C:\Program Files (x86)\PuTTY\putty.exe", args.ToString());
    }
}

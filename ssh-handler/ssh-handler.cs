// SSH Handler
//
// Douglas Thrift
//
// ssh-handler

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

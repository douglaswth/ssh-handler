// Shell32
//
// Douglas Thrift
//
// Shell32.cs

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
using System.ComponentModel;
using System.Runtime.InteropServices;

public static class Shell32
{
    public static IList<string> CommandLineToArgv(string cmdLine)
    {
        int numArgs;
        var argv = CommandLineToArgvW(cmdLine, out numArgs);

        if (argv == IntPtr.Zero)
            throw new Win32Exception();

        try
        {
            string[] args = new string[numArgs];

            for (int index = 0; index != numArgs; ++index)
                args[index] = Marshal.PtrToStringUni(Marshal.ReadIntPtr(argv, index * IntPtr.Size));

            return args;
        }
        finally
        {
            Marshal.FreeHGlobal(argv);
        }
    }

    [DllImport("shell32.dll", SetLastError = true)]
    private static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);
}
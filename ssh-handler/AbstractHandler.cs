// Abstract Handler
//
// Douglas Thrift
//
// AbstractHandler.cs

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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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

    protected MatchOption SetValue(Match match, string groupName, ref string value)
    {
        Group group = match.Groups[groupName];
        if (group.Success)
            value = group.Value;

        return MatchOption.Set;
    }

    protected MatchOption SetBooleanValue(Match match, string groupName, out bool option, ref string value)
    {
        Group group = match.Groups[groupName];
        if (group.Success)
            switch (group.Value.ToLowerInvariant())
            {
            case "yes":
                option = true;
                break;
            case "no":
                option = false;
                break;
            default:
                value = group.Value;
                goto case "yes";
            }
        else
            option = true;

        return MatchOption.Option;
    }

    protected MatchOption SetYesNoValue(Match match, string groupName, out AutoYesNoOption option, ref string value)
    {
        bool optionValue;
        MatchOption matchOption = SetBooleanValue(match, groupName, out optionValue, ref value);
        option = optionValue ? AutoYesNoOption.Yes : AutoYesNoOption.No;

        return matchOption;
    }

    protected void AddArguments(List<string> command, params object[] arguments)
    {
        command.AddRange(arguments.Select(argument => argument.ToString()));
    }
}

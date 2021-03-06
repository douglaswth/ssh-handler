﻿// Handler
//
// Douglas Thrift
//
// Handler.cs

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

public interface Handler
{
    IEnumerable<string> Options
    {
        get;
    }
    IEnumerable<string> Usages
    {
        get;
    }
    IEnumerable<Setting> Settings
    {
        get;
    }
    Setting Setting
    {
        get;
    }

    MatchOption DoMatch(string arg);
    bool Find();
    void Execute(Uri uri, string user, string password);
}

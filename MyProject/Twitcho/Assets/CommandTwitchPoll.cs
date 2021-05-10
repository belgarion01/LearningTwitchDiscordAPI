using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandTwitchPoll : TwitchPoll
{
    public CommandTwitchPoll(List<string> commands)
    {
        FilterFunc = MakeFilterFromCommands();
    }

    Func<string, bool> MakeFilterFromCommands()
    {
        return s => s[0] == '!';
    }
}

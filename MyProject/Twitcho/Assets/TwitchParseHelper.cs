using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TwitchParseHelper
{
    public static IRCChatMessage ParseChatMessage(string ircString)
    {
        return new IRCChatMessage(ParseUserName(ircString), ParseChannel(ircString), ParseMessage(ircString));
    }

    public static string ParseUserName(string ircString)
    {
        return ircString.Substring(1, ircString.IndexOf('!') - 1);
    }

    public static string ParseChannel(string ircString)
    {
        string channel = ircString.Substring(ircString.IndexOf("#")+1);
        int endIndex = channel.IndexOf(' ');
        if (endIndex == -1)
        {
            return channel;
        }
        else
        {
            return channel.Substring(0, channel.IndexOf(' '));
        }
    }

    public static string ParseMessage(this string ircString)
    {
        return ircString.Substring(ircString.IndexOfWithSkip(' ', 2) + 2);
    }
}

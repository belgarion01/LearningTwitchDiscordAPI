using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IRCChatMessage
{
    public string User { get; private set; }
    public string Channel { get; private set; }
    public string Message { get; private set; }

    public IRCChatMessage(string user, string channel, string message)
    {
        User = user;
        Channel = channel;
        Message = message;
    }
}

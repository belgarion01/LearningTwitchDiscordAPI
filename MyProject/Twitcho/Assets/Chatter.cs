using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chatter
{
    private IRCTags ircTags;

    public string User { get; private set; }
    public string Channel { get; private set; }
    public string Message { get; private set; }

    public Chatter(IRCChatMessage ircChatMessage, IRCTags ircTags)
    {
        User = ircChatMessage.User;
        Channel = ircChatMessage.Channel;
        Message = ircChatMessage.Message;

        this.ircTags = ircTags;
    }
}

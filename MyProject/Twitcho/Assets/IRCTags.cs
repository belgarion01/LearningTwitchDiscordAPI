using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IRCTags
{
    private List<ChatterEmote> emotes = new List<ChatterEmote>();
    private ChatterBadge[] badges = new ChatterBadge[0];

    public string ColorHex { get; private set; } = "#FFFFFF";
    public string DisplayName { get; private set; } = "DefaultDisplayName";
    
    public List<ChatterEmote> Emotes => emotes.ToList();
    public ChatterBadge[] Badges => badges;

    public IRCTags()
    {
        
    }

    public IRCTags(string colorHex, string displayName, ChatterBadge[] badges, List<ChatterEmote> emotes)
    {
        ColorHex = colorHex;
        DisplayName = displayName;
        this.badges = badges;
        this.emotes = emotes;
    }
}

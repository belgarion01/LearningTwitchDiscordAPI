using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

public class TwitchIRC : MonoBehaviour
{
    public static TwitchIRC Instance
    {
        get => instance;
        private set => instance = value;
    }

    private static TwitchIRC instance;

    private TcpClient socketConnection;
    private NetworkStream stream;

    private Thread clientInputThread;
    private Thread clientOutputThread;

    [Header("TCP Client")]
    [SerializeField] private string ircAdress = "irc.chat.twitch.tv";
    [SerializeField] private int port = 6667;

    [Header("Bot Connection")] 
    [SerializeField] private string oauth = "oauth:z8d6zsq64ix9bjf7su5wqxo1qlsjyv";
    [SerializeField] private string user = "belgarion001";
    [SerializeField] private string channel = "belgarion001";
    
    private bool connected = false;

    public UnityEvent<Chatter> OnNewMessage;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }

        instance = this;
    }

    void Start()
    {
        StartCoroutine(PrepareConnection());
    }

    private IEnumerator PrepareConnection()
    {
        if (clientInputThread != null && clientOutputThread != null)
            while (clientInputThread.IsAlive || clientOutputThread.IsAlive) // Wait for previous threads to close (if there are any)
                yield return null;

        if (oauth.Length <= 0 || user.Length <= 0 || channel.Length <= 0)
        {
            ConnectionStateAlert(StatusType.Error, "Missing required details! Check your Twitch details.");
            yield break;
        }

        ConnectToIRC();
    }

    void ConnectToIRC()
    {
        socketConnection = new TcpClient(ircAdress, port);
            
        if(!socketConnection.Connected) ConnectionStateAlert(StatusType.Error, "Failed to connect to server");

        stream = socketConnection.GetStream();

        // Fix oauth
        string trimmedOauth = oauth.StartsWith("oauth") ? oauth.Substring(6) : oauth;
        
        WriteLine(stream, "PASS oauth:" + trimmedOauth.ToLower());
        WriteLine(stream, "NICK " + user.ToLower());
        WriteLine(stream, "CAP REQ :twitch.tv/tags twitch.tv/commands");
        
        connected = true;
        
        //Clear Waiting Command Output
        outputQueue.Clear();
        
        clientInputThread = new Thread(new ThreadStart(ListenForData));
        clientOutputThread = new Thread(new ThreadStart(SendingData));
        
        clientInputThread.Start();
        clientOutputThread.Start();
    }

    void ListenForData()
    {
        using (StreamReader reader = new StreamReader(stream))
        {
            string rawData = string.Empty;
            
            while (connected)
            {
                // Try/catch to catch error when disconnecting while reading
                try
                {
                    rawData = reader.ReadLine();
                }
                catch
                {
                    if (connected)
                    {
                        Debug.LogError("Error while reading IRC input. Reconnecting...");
                    }
                }
                
                // null check
                if(rawData == null) continue;
                
                Debug.Log("<color=#005ae0><b>[IRC INPUT]</b></color> " + rawData);

                string ircString = rawData;
                string tagString = string.Empty;

                // Check for tags
                if (rawData[0] == '@')
                {
                    int endOfTagsIndex = ircString.IndexOf(" ");

                    // Extract tags
                    tagString = ircString.Substring(0, endOfTagsIndex);
                    
                    // Remove tags from IRC
                    ircString = ircString.Substring(endOfTagsIndex).TrimStart();
                }
                
                // Handle IRC messages
                if (ircString[0] == ':')
                {
                    // Get Message Type
                    string messageType = ircString.Substring(ircString.IndexOf(" ")).TrimStart();
                    messageType = messageType.Substring(0, messageType.IndexOf(" "));
                    
                    switch (messageType)
                    {
                        // Chat Message
                        case "PRIVMSG": HandleChatMessages(ircString, tagString);
                            break;
                        
                        // User Infos
                        case "USERSTATE": HandleUserState(ircString, tagString);
                            break;
                        
                        // Connected to IRC
                        case "001": HandleConnection(messageType);
                            break;
                        
                        // Connected to channel
                        case "353": HandleConnection(messageType);
                            break;
                    }
                }
                
                // Ping/Pong Twitch response
                if (rawData == "PING :tmi.twitch.tv")
                {
                    SendCommand("PONG :tmi.twitch.tv", true);
                }
            }
        }
    }

    private Queue<string> outputQueue = new Queue<string>();
    
    void SendingData()
    {
        // Here we use a cooldown to avoid the Twitch chat rate limit (avoiding a 30mn ban for spam)
        
        System.Diagnostics.Stopwatch cooldown = new System.Diagnostics.Stopwatch();

        while (connected)
        {
            if(outputQueue.Count <= 0) continue;
            
            WriteLine(stream, outputQueue.Dequeue(), true);
            
            cooldown.Restart();

            while (cooldown.ElapsedMilliseconds < 1750) continue;
        }
    }

    void HandleConnection(string type)
    {
        switch (type)
        {
            case "001":
                SendCommand("JOIN #" + channel.ToLower());
                ConnectionStateAlert(StatusType.Success, "Connected to server. Now trying to join the channel...");
                break;
            case "353":
                Debug.Log("<color=#bd2881><b>[JOIN]</b></color> Joined channel: " + channel + " successfully");
                break;
        }
    }

    void HandleChatMessages(string ircString, string tagString)
    {
        IRCChatMessage IRCChatMessage = TwitchParseHelper.ParseChatMessage(ircString);
        OnNewMessage?.Invoke(new Chatter(IRCChatMessage, new IRCTags()));
        //Debug.Log($"{IRCChatMessage.User} said {IRCChatMessage.Message} in channel [{IRCChatMessage.Message}]");
    }

    void SendMessage(string message)
    {
        if (message.Length == 0) return;
        outputQueue.Enqueue("PRIVMSG #" + channel + " :" + message); // Place message in queue
    }

    void HandleUserState(string ircString, string tagString)
    {
        
    }

    void Disconnect()
    {
        if (!connected) return;

        connected = false;
        
        socketConnection.Close();
        stream.Close();

        Debug.LogWarning("Disconnected from Twitch IRC");
    }

    public void SendCommand(string command, bool instant = false)
    {
        if (instant)
        {
            WriteLine(stream, command);
        }
        else
        {
            outputQueue.Enqueue(command);
        }
    }

    public void WriteLine(NetworkStream stream, string output, bool debug = true)
    {
        if(debug) Debug.Log("<color=#ff9966><b>[IRC OUTPUT]</b></color> Sending command: " + output);
        
        // Convert string to bytes
        byte[] bytes = Encoding.UTF8.GetBytes(output);
        
        // ???
        stream.Write(bytes, 0, bytes.Length);
        stream.WriteByte((byte)'\r');
        stream.WriteByte((byte)'\n');
        stream.Flush();
    }

    void OnDisable()
    {
        Disconnect();
    }

    void OnDestroy()
    {
        Disconnect();
    }

    public enum StatusType { Normal, Success, Error };
    public void ConnectionStateAlert(StatusType state, string message)
    {
        switch (state)
        {
            case StatusType.Error:
                Debug.LogError("<color=red><b>[ERROR]</b></color>: " + message);
                break;

            case StatusType.Normal:
                Debug.Log("<color=#0018a1><b>[STATUS]</b></color>: " + message);
                break;

            case StatusType.Success:
                Debug.Log("<color=#0ea300><b>[SUCCESS]</b></color>: " + message);
                break;
        }
    }
}

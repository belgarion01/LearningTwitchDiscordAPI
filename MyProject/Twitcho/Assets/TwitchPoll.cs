using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwitchPoll
{
    private List<string> messages = new List<string>();

    private bool isListeningForMessage = false;
    private float listenTime = 10f;

    public Action<List<string>> OnPollEnded;
    protected Func<string, bool> FilterFunc;

    private float time = 0f;

    public Action<float> OnTick;

    public TwitchPoll()
    {
        if (TwitchIRC.Instance == null)
        {
            Debug.LogError("Tried to make a TwitchPoll without a TwitchIRC");
        }
        else
        {
            Init();
            FilterFunc = s => true;
        }
    }

    public TwitchPoll(Func<string, bool> filter)
    {
        if (TwitchIRC.Instance == null)
        {
            Debug.LogError("Tried to make a TwitchPoll without a TwitchIRC");
        }
        else
        {
            Init();
            FilterFunc = filter;
        }
    }

    public IEnumerator StartAutomatic()
    {
        if(TwitchIRC.Instance == null || isListeningForMessage) yield break;
        messages.Clear();

        time = 0f;

        while (time < listenTime)
        {
            Tick();
            yield return new WaitForEndOfFrame();
        }
        
        StopListening();
    }

    public void Init()
    {
        TwitchIRC.Instance.OnNewMessage.AddListener(OnNewMessageHandle);
    }

    public void Reset()
    {
        isListeningForMessage = false;
        messages.Clear();
        time = 0f;
    }
    
    public void Tick()
    {
        if (isListeningForMessage || TwitchIRC.Instance == null) return;
        time += Time.deltaTime;
        OnTick?.Invoke(time);
    }

    void OnNewMessageHandle(Chatter chatter)
    {
        TryAddMessage(chatter, FilterFunc);
    }

    void TryAddMessage(Chatter chatter, Func<string, bool> filter)
    {
        if (filter(chatter.Message))
        {
            messages.Add(chatter.Message);
        }

        Debug.Log("Received Message !");
    }

    void StopListening()
    {
        Debug.Log(messages.Count);
        OnPollEnded?.Invoke(messages);
        
        isListeningForMessage = false;
    }
}

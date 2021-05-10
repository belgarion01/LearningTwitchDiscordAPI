using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Hura : MonoBehaviour
{
    private TwitchPoll poll;

    [SerializeField] private TextMeshProUGUI textField;
    
    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            poll = new CommandTwitchPoll(new List<string>());
            poll.OnPollEnded += PrintMessages;
            poll.OnTick += UpdateUI;
            StartCoroutine(poll.StartAutomatic());
        }
    }

    public bool IsFirstCharUpper(string val)
    {
        return Char.IsUpper(val[0]);
    }
    
    void PrintMessages(List<string> messages)
    {
        foreach (var message in messages)
        {
            Debug.Log(message.ToUpper());
        }
    }

    void UpdateUI(float timer)
    {
        textField.text = Mathf.Floor(timer).ToString();
    }
}

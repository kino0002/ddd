using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MessageDisplayManager : MonoBehaviour
{
    public static MessageDisplayManager Instance;

    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float messageDisplayDuration = 2f;
    private Queue<string> messageQueue = new Queue<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void DisplayMessage(string message)
    {
        messageQueue.Enqueue(message);
        if (!messageText.gameObject.activeInHierarchy)
        {
            ShowNextMessage();
        }
    }

    private void ShowNextMessage()
    {
        if (messageQueue.Count > 0)
        {
            string message = messageQueue.Dequeue();
            messageText.text = message;
            messageText.gameObject.SetActive(true);
            CancelInvoke("HideMessage");
            Invoke("HideMessage", messageDisplayDuration);
        }
    }

    private void HideMessage()
    {
        messageText.gameObject.SetActive(false);
        ShowNextMessage();
    }
}

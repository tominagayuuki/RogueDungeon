using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageWindow : MonoBehaviour
{
    public Text MessagePrefab;
    [Range(1, 15)]
    public int MessageLimit = 5;

    Transform Root;

    public static MessageWindow Find()
    {
        return Object.FindObjectOfType<MessageWindow>();
    }

    public void AppendMessage(string message)
    {
        var obj = Object.Instantiate(MessagePrefab, Root);
        obj.text = message;

        if (Root.childCount > MessageLimit)
        {
            var removeCount = Root.childCount - MessageLimit;
            for (var i = removeCount - 1; 0 <= i; i--)
            {//�폜����ƎqGameObject�̕��я����ς��̂ŁA�폜������̂̌�납�珇�ɂ݂Ă���B
                Object.Destroy(Root.GetChild(i).gameObject);
            }
        }
    }

    private void Awake()
    {
        Root = transform.Find("Root");
        foreach (Transform child in Root)
        {
            Object.Destroy(child.gameObject);
        }
    }
}

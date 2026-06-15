using UnityEngine;
using TMPro;

public class CreateTodoScript : MonoBehaviour
{

    [SerializeField] private TMP_InputField titleInputField;

    public void HandleAdd()
    {
        Debug.Log("CreateTodoScript: Adding todo '" + titleInputField.text +"'");
        TodoManagerScript.Instance.AddTodo(titleInputField.text);
        titleInputField.text = "";
        MenuScript.Instance.ShowTodoMenu();
    }
}

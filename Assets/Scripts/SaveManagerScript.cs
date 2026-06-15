using UnityEngine;

public class SaveManagerScript : MonoBehaviour
{
    [System.Serializable]
    public class SaveWrapper
    {
        public List<TodoManagerScript.Todo> todoList;
    }
}

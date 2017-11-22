using UnityEngine;

public class ReorderableListAttribute : PropertyAttribute
{
    public string listName;

    public ReorderableListAttribute() { }

    public ReorderableListAttribute(string name)
    {
        listName = name;
    }
}
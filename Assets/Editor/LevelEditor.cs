using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Level))]
public class LevelEditor : Editor
{
    Level level;

    bool shouldResize;
    Vector2Int initialConditionGridSize;

    const float SIZE_SPACE = 16f;
    const float SIZE_PREVIEW = 64f;

    public void OnEnable()
    {
        level = (Level)target;
        shouldResize = false;
        initialConditionGridSize = level.gridSize;
    }

    public override void OnInspectorGUI()
    {
        // update grid size
        Vector2Int newGridSize = EditorGUILayout.Vector2IntField(
            "Grid Size", level.gridSize
        );

        bool hasSizeChanged = level.gridSize != newGridSize;
        if (hasSizeChanged)
        {
            Vector2Int oldGridSize = level.gridSize;
            level.gridSize = newGridSize;
            if (!shouldResize)
            {
                shouldResize = true;
                initialConditionGridSize = oldGridSize;
            }
            else if (newGridSize == initialConditionGridSize)
            {
                shouldResize = false;
            }
        }

        // activate button for resizing
        if (shouldResize)
        {
            if (GUILayout.Button("Resize initial condition"))
            {
                level.initialCondition = ResizeInitialCondition(level.initialCondition, initialConditionGridSize, newGridSize);
                initialConditionGridSize = newGridSize;
            }
        }

        GUILayout.Space(SIZE_SPACE);
        // update initial condition
        GUILayout.Label("Initial Condition");
        bool hasInitialConditionChanged = false;
        for (int y = 0; y < initialConditionGridSize.y; y++)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < initialConditionGridSize.x; x++)
            {
                Item.Types type = level.GetTile(x, y, initialConditionGridSize);
                Item.Types selectedType = (Item.Types)EditorGUILayout.Popup((int)type, Enum.GetNames(typeof(Item.Types)));
                if (type != selectedType)
                {
                    int index = level.GetTileIndex(x, y, initialConditionGridSize);
                    level.initialCondition[index] = selectedType;
                    hasInitialConditionChanged = true;
                    ItemDatabase.GetItemTexture(selectedType);
                }
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(SIZE_SPACE);
        // update initial condition
        GUILayout.Label("Preview");
        for (int y = 0; y < initialConditionGridSize.y; y++)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < initialConditionGridSize.x; x++)
            {
                Item.Types type = level.GetTile(x, y, initialConditionGridSize);
                Texture texture = ItemDatabase.GetItemTexture(type);
                GUILayout.Button(texture, GUILayout.Width(SIZE_PREVIEW), GUILayout.Height(SIZE_PREVIEW));
            }
            GUILayout.EndHorizontal();
        }

        if (hasSizeChanged || hasInitialConditionChanged) EditorUtility.SetDirty(level);
    }

    private Item.Types[] ResizeInitialCondition(Item.Types[] condition, Vector2Int oldSize, Vector2Int newSize)
    {
        Item.Types[] resized = new Item.Types[newSize.x * newSize.y];
        for (int y = 0; y < newSize.y; y++)
        {
            for (int x = 0; x < newSize.x; x++)
            {
                int oldIndex = x + y * oldSize.x;
                int newIndex = x + y * newSize.x;

                bool hasOldValue = oldIndex < condition.Length && x < oldSize.x && y < oldSize.y;
                resized[newIndex] = hasOldValue ? condition[oldIndex] : Item.Types.RANDOM;
            }
        }
        return resized;
    }

}

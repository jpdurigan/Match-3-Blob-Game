using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Level))]
public class LevelEditor : Editor
{
    Level level;

    const float SIZE_SPACE = 16f;
    const float SIZE_PREVIEW = 32f;

    public void OnEnable()
    {
        level = (Level)target;
    }

    public override void OnInspectorGUI()
    {
        // base.OnInspectorGUI();

        // update grid size
        Vector2Int newGridSize = EditorGUILayout.Vector2IntField(
            "Grid Size", level.gridSize
        );

        bool hasSizeChanged = level.gridSize != newGridSize;
        if (hasSizeChanged)
        {
            Vector2Int oldGridSize = level.gridSize;
            level.gridSize = newGridSize;
            level.initialCondition = ResizeInitialCondition(level.initialCondition, oldGridSize, newGridSize);
        }

        GUILayout.Space(SIZE_SPACE);
        // update initial condition
        GUILayout.Label("Initial Condition");
        bool hasInitialConditionChanged = false;
        for (int y = 0; y < level.gridSize.y; y++)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < level.gridSize.x; x++)
            {
                Item.Types type = level.GetTile(x, y);
                Item.Types selectedType = (Item.Types)EditorGUILayout.Popup((int)type, Enum.GetNames(typeof(Item.Types)));
                if (type != selectedType)
                {
                    int index = level.GetTileIndex(x, y);
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
        // float buttonWidth = EditorGUIUtility.currentViewWidth / (float)level.gridSize.x;
        for (int y = 0; y < level.gridSize.y; y++)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < level.gridSize.x; x++)
            {
                Item.Types type = level.GetTile(x, y);
                Texture texture = ItemDatabase.GetItemTexture(type);
                // EditorGUILayout
                GUILayout.Button(
                    texture,
                    GUILayout.MinWidth(SIZE_PREVIEW), GUILayout.MaxWidth(SIZE_PREVIEW),
                    GUILayout.MinHeight(SIZE_PREVIEW), GUILayout.MaxHeight(SIZE_PREVIEW)
                );
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
                // Debug.Log(resized[newIndex]);
            }
        }
        return resized;
    }

}

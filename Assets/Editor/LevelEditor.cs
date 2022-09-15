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
        bool hasChangedLevelData = false;

        GUILayout.Label("Parameters", EditorStyles.boldLabel);

        int newTurnsAmount = EditorGUILayout.IntField("Number of Turns", level.turnsAmount);
        if (newTurnsAmount != level.turnsAmount)
        {
            level.turnsAmount = newTurnsAmount;
            hasChangedLevelData = true;
        }

        EditorGUILayout.Separator();

        GUILayout.Label("Goal");
        Item.Types newGoalType = (Item.Types)EditorGUILayout.EnumPopup("Item", level.goalType);
        if (newGoalType != level.goalType)
        {
            level.goalType = newGoalType;
            hasChangedLevelData = true;
        }

        int newGoalAmount = EditorGUILayout.IntField("Amount", level.goalAmount);
        if (newGoalAmount != level.goalAmount)
        {
            level.goalAmount = newGoalAmount;
            hasChangedLevelData = true;
        }

        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        GUILayout.Label("Grid", EditorStyles.boldLabel);

        // update grid size
        Vector2Int newGridSize = EditorGUILayout.Vector2IntField("Size", level.gridSize);
        if (newGridSize != level.gridSize)
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

        EditorGUILayout.Separator();

        // update initial condition
        GUILayout.Label("Initial Condition");
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
                    hasChangedLevelData = true;
                    ItemDatabase.GetItemTexture(selectedType);
                }
            }
            GUILayout.EndHorizontal();
        }

        EditorGUILayout.Separator();

        // update initial condition
        GUILayout.Label("Preview");
        for (int y = 0; y < initialConditionGridSize.y; y++)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < initialConditionGridSize.x; x++)
            {
                Item.Types type = level.GetTile(x, y, initialConditionGridSize);
                Texture texture = ItemDatabase.GetItemTexture(type);
                bool pressed = GUILayout.Button(texture, GUILayout.Width(SIZE_PREVIEW), GUILayout.Height(SIZE_PREVIEW));
                if (pressed)
                {
                    int newType = ((int)type + 1) % (int)Item.Types.INVALID;
                    int index = level.GetTileIndex(x, y, initialConditionGridSize);
                    level.initialCondition[index] = (Item.Types)newType;
                }
            }
            GUILayout.EndHorizontal();
        }

        if (hasChangedLevelData) EditorUtility.SetDirty(level);
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

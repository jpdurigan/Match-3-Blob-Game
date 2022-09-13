using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Level))]
public class LevelEditor : Editor
{
    Level level;

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

        if (hasSizeChanged) EditorUtility.SetDirty(level);
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

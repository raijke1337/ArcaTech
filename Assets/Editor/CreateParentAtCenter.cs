#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CreateParentAtCenter
{
    // Горячая клавиша: %#g означает Ctrl+Shift+G (или Cmd+Shift+G на macOS)
    [MenuItem("GameObject/Create Empty Parent at Center %#g", false, 0)]
    private static void CreateParent()
    {
        Transform[] transforms = Selection.transforms;
        if (transforms.Length == 0) return;

        // 1. Вычисляем геометрический центр по Renderer.bounds
        Bounds bounds = new Bounds();
        bool hasBounds = false;

        foreach (var t in transforms)
        {
            Renderer[] renderers = t.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                if (!hasBounds)
                {
                    bounds = r.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(r.bounds);
                }
            }
        }

        // Если у объектов нет Renderer (например, пустые объекты), берем среднее по позициям
        Vector3 centerPosition;
        if (hasBounds)
        {
            centerPosition = bounds.center;
        }
        else
        {
            Vector3 sum = Vector3.zero;
            foreach (var t in transforms)
            {
                sum += t.position;
            }
            centerPosition = sum / transforms.Length;
        }

        // 2. Создаем родительский объект
        GameObject parent = new GameObject("GameObject");
        parent.transform.position = centerPosition;

        // Если у всех выбранных объектов уже был общий родитель, сохраняем вложенность
        Transform commonParent = transforms[0].parent;
        bool allShareParent = true;
        for (int i = 1; i < transforms.Length; i++)
        {
            if (transforms[i].parent != commonParent)
            {
                allShareParent = false;
                break;
            }
        }
        if (allShareParent && commonParent != null)
        {
            parent.transform.SetParent(commonParent, true);
        }

        // Регистрируем создание в системе Undo (чтобы работал Ctrl+Z)
        Undo.RegisterCreatedObjectUndo(parent, "Create Parent at Center");

        // 3. Перемещаем объекты внутрь нового родителя
        foreach (var t in transforms)
        {
            Undo.SetTransformParent(t, parent.transform, "Create Parent at Center");
        }

        // Выделяем созданный объект
        Selection.activeGameObject = parent;
    }

    // Делаем пункт активным только тогда, когда выделен хотя бы один объект
    [MenuItem("GameObject/Create Empty Parent at Center %#g", true)]
    private static bool ValidateCreateParent()
    {
        return Selection.transforms.Length > 0;
    }
}
#endif
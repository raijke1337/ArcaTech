using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using UnityEngine;

namespace Arcatech.Interactions
{
    using UnityEditor;
    using System.Reflection;
    public partial class ItemMovesInteraction
    {
        // ===================== Editor Preview =====================
        [Header("Editor Preview")] [SerializeField]
        bool previewPath = true;

        [SerializeField] bool drawOnlyWhenSelected = true;
        [SerializeField] Color pathColor = new Color(0f, 1f, 1f, 0.9f);
        [SerializeField] Color startColor = new Color(0.1f, 1f, 0.1f, 0.9f);
        [SerializeField] Color endColor = new Color(1f, 0.6f, 0.1f, 0.9f);
        [SerializeField] float nodeRadius = 0.06f;
        [SerializeField] float arrowSize = 0.25f;
        [SerializeField] bool useCustomStart = false;
        [SerializeField] Vector3 customStartPosition;

        // If true, process sequence steps strictly in list order (simple, stable).
        // If false, you can replace logic to sort by insertTime, simulate joins, etc.
        [SerializeField] bool sequencePreviewInListOrder = true;

        void OnDrawGizmos()
        {
            if (!previewPath || drawOnlyWhenSelected) return;
            DrawPreviewInternal();
        }

        void OnDrawGizmosSelected()
        {
            if (!previewPath) return;
            DrawPreviewInternal();
        }

        void DrawPreviewInternal()
        {
            if (tween == null) return;

            Transform t = transform;
            Vector3 start = useCustomStart ? customStartPosition : t.position;

            // Build points to draw
            List<Vector3> points = BuildPreviewPoints(t, start, tween);
            if (points == null || points.Count < 2) return;

            // Draw nodes and path
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

            // Start node
            Handles.color = startColor;
            Handles.SphereHandleCap(0, points[0], Quaternion.identity, nodeRadius, EventType.Repaint);

            // Segments
            Handles.color = pathColor;
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 a = points[i];
                Vector3 b = points[i + 1];
                Handles.DrawAAPolyLine(3f, new Vector3[] { a, b });

                // Arrow near the end of each segment
                Vector3 dir = (b - a);
                float mag = dir.magnitude;
                if (mag > 0.001f)
                {
                    dir /= mag;
                    Vector3 arrowPos = Vector3.Lerp(a, b, 0.85f);
                    Handles.ArrowHandleCap(0, arrowPos, Quaternion.LookRotation(dir), arrowSize, EventType.Repaint);
                }

                // Intermediate nodes (except start)
                Handles.SphereHandleCap(0, b, Quaternion.identity, nodeRadius, EventType.Repaint);
            }

            // End node in special color
            Handles.color = endColor;
            Handles.SphereHandleCap(0, points[points.Count - 1], Quaternion.identity, nodeRadius * 1.2f,
                EventType.Repaint);
        }

        List<Vector3> BuildPreviewPoints(Transform target, Vector3 start, SerializedDOTweener source)
        {
            var pts = new List<Vector3> { start };

            if (source is MovementTweenPreset movePreset)
            {
                var end = ResolveMovementEnd(movePreset, start);
                pts.Add(end);
                return pts;
            }

            if (source is SerializedDOTweenSequence seq)
            {
                Vector3 curr = start;

                if (sequencePreviewInListOrder)
                {
                    // Simple, robust pass: process in step order and only draw movement steps.
                    foreach (var step in seq.steps)
                    {
                        // Skip non-tween steps
                        if (step.stepType == SequenceStep.StepType.AppendInterval) continue;
                        if (step.stepType == SequenceStep.StepType.AppendCallback) continue;

                        var movement = TryExtractMovementPreset(step.Action);
                        if (movement == null) continue;

                        var next = ResolveMovementEnd(movement, curr);
                        if ((next - curr).sqrMagnitude > 0.000001f)
                        {
                            pts.Add(next);
                            curr = next;
                        }
                    }

                    return pts;
                }
                else
                {
                    // Advanced handling could go here:
                    // - Accumulate a timeline time cursor
                    // - Respect Insert/Join by start times
                    // - Resolve conflicts when multiple moves overlap
                    // For now, default to the simple behavior above.
                    foreach (var step in seq.steps)
                    {
                        if (step.stepType == SequenceStep.StepType.AppendInterval) continue;
                        if (step.stepType == SequenceStep.StepType.AppendCallback) continue;

                        var movement = TryExtractMovementPreset(step.Action);
                        if (movement == null) continue;

                        var next = ResolveMovementEnd(movement, curr);
                        if ((next - curr).sqrMagnitude > 0.000001f)
                        {
                            pts.Add(next);
                            curr = next;
                        }
                    }

                    return pts;
                }
            }

            // Unknown preset type: nothing to draw
            return pts;
        }

        static Vector3 ResolveMovementEnd(MovementTweenPreset preset, Vector3 startWorld)
        {
            Vector3 end = preset.isRelative ? startWorld + preset.targetPosition : preset.targetPosition;

            if (preset.snapping)
            {
                end = new Vector3(
                    Mathf.Round(end.x),
                    Mathf.Round(end.y),
                    Mathf.Round(end.z)
                );
            }

            return end;
        }

        // Attempts to find a MovementTweenPreset from a step's Action without invoking DOTween.
        static MovementTweenPreset TryExtractMovementPreset(object action)
        {
            if (action == null) return null;

            // Direct cast
            if (action is MovementTweenPreset direct) return direct;

            // Reflectively search for a field or property that holds a SerializedDOTweener (or derived)
            var t = action.GetType();

            // Fields
            var fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var f in fields)
            {
                if (typeof(SerializedDOTweener).IsAssignableFrom(f.FieldType))
                {
                    var val = f.GetValue(action) as SerializedDOTweener;
                    if (val is MovementTweenPreset mp) return mp;
                }
            }

            // Properties
            var props = t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var p in props)
            {
                if (!p.CanRead) continue;
                if (typeof(SerializedDOTweener).IsAssignableFrom(p.PropertyType))
                {
                    var val = p.GetValue(action, null) as SerializedDOTweener;
                    if (val is MovementTweenPreset mp) return mp;
                }
            }

            return null;
        }
    }
}

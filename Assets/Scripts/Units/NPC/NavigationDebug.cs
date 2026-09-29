using System.Text;
using UnityEngine;
using UnityEngine.AI;

namespace Arcatech.Units
{


    /// <summary>
    /// Временный диагностический компонент: повесьте на «проблемного» врага (или добавьте в префаб),
    /// запустите сцену, и в консоли будет отчёт о том, почему агент не двигается / проваливается.
    /// Можно вызвать повторно из контекстного меню компонента (Run Diagnostics).
    /// </summary>
    [DisallowMultipleComponent]
    public class NavigationDebug : MonoBehaviour
    {
        [SerializeField] private float _delay = 0.5f;
        [SerializeField] private float _repeatEvery = 0f; // 0 = один раз

        private void Start()
        {
            if (_repeatEvery > 0f) InvokeRepeating(nameof(RunDiagnostics), _delay, _repeatEvery);
            else Invoke(nameof(RunDiagnostics), _delay);
        }

        [ContextMenu("Run Diagnostics")]
        public void RunDiagnostics()
        {
            var sb = new StringBuilder($"[NavDiag] {name} @ {transform.position}\n");

            // --- агент ---
            if (!TryGetComponent(out NavMeshAgent agent))
            {
                sb.AppendLine("  НЕТ NavMeshAgent");
                Debug.Log(sb.ToString(), this);
                return;
            }

            string typeName = NavMesh.GetSettingsNameFromID(agent.agentTypeID);
            sb.AppendLine(
                $"  agent: enabled={agent.enabled}, isOnNavMesh={agent.isOnNavMesh}, type='{typeName}' (id {agent.agentTypeID}), " +
                $"areaMask={agent.areaMask}, isStopped={(agent.isOnNavMesh ? agent.isStopped.ToString() : "n/a")}, " +
                $"updatePosition={agent.updatePosition}, radius={agent.radius}, height={agent.height}");

            if (!agent.isOnNavMesh)
            {
                var own = agent.QueryFilter();
                foreach (float d in new[] { 0.5f, 2f, 10f, 50f })
                {
                    if (NavMesh.SamplePosition(transform.position, out var hit, d, own))
                    {
                        sb.AppendLine(
                            $"  ближайший навмеш ТИПА '{typeName}': {Vector3.Distance(transform.position, hit.position):F2} м (искали в радиусе {d})");
                        break;
                    }

                    if (d >= 50f)
                        sb.AppendLine(
                            $"  !!! навмеша типа '{typeName}' в радиусе 50 м нет вообще. Он не запечён для этого типа агента " +
                            "(Window > AI > Navigation > Agents: проверьте Bot, Bake для этого типа).");
                }

                if (NavMesh.SamplePosition(transform.position, out var any, 2f, NavMesh.AllAreas))
                    sb.AppendLine(
                        $"  (навмеш ТИПА ПО УМОЛЧАНИЮ рядом есть, {Vector3.Distance(transform.position, any.position):F2} м. " +
                        "Если агент не 'Humanoid' - вы видите чужой навмеш на превью.)");
            }

            // --- rigidbody ---
            if (TryGetComponent(out Rigidbody rb))
            {
                sb.AppendLine($"  rigidbody: isKinematic={rb.isKinematic}, useGravity={rb.useGravity}, mass={rb.mass}");
                if (!rb.isKinematic)
                    sb.AppendLine(
                        "  !!! Rigidbody НЕ kinematic: агент и физика дерутся за transform, ECM2 может толкать юнита, гравитация тянет вниз.");
            }

            // --- пол под ногами ---
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out var floor, 5f, ~0,
                    QueryTriggerInteraction.Ignore))
                sb.AppendLine(
                    $"  пол под ногами: '{floor.collider.name}' (слой {LayerMask.LayerToName(floor.collider.gameObject.layer)}), " +
                    $"{floor.distance - 0.5f:F2} м ниже ног. Слои сталкиваются: " +
                    $"{!Physics.GetIgnoreLayerCollision(gameObject.layer, floor.collider.gameObject.layer)}");
            else
                sb.AppendLine(
                    "  !!! ПОД НОГАМИ НЕТ КОЛЛАЙДЕРА. Навмеш - не пол: без коллайдера динамическое тело провалится.");

            // --- коллайдеры юнита ---
            foreach (var c in GetComponentsInChildren<Collider>())
                sb.AppendLine(
                    $"  collider: {c.name} [{c.GetType().Name}] trigger={c.isTrigger} layer={LayerMask.LayerToName(c.gameObject.layer)}" +
                    (!c.isTrigger && c.gameObject != gameObject
                        ? "   <- не trigger на дочернем объекте: хитбокс оружия? Он будет блокировать/толкать игрока"
                        : ""));

            Debug.Log(sb.ToString(), this);
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying || !TryGetComponent(out NavMeshAgent agent) || agent.isOnNavMesh) return;
            if (agent.SampleOwnMesh(transform.position, 20f, out var hit))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, hit.position);
                Gizmos.DrawWireSphere(hit.position, 0.2f);
            }
        }
    }
}
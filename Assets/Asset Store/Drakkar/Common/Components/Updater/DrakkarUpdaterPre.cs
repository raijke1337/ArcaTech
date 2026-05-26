using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
#if UNITY_EDITOR
#endif

namespace Drakkar
{
#if ODIN_INSPECTOR
	[HideMonoScript]
#endif
	public class DrakkarUpdaterPre : MonoBehaviour
	{
		#region UNITY STUFF
		private void Update()
		{
		#if DRAKKAR
			if (DrakkarUpdater.instance.WaitForColdstart && !DrakkarUpdater.instance.started && !ColdStart.Ready)
				return;
		#endif
			DrakkarUpdater.instance.processUpdatePre();
		}

		private void LateUpdate()
		{
		#if DRAKKAR
			if (DrakkarUpdater.instance.WaitForColdstart && !DrakkarUpdater.instance.started && !ColdStart.Ready)
				return;
		#endif
			DrakkarUpdater.instance.processLateUpdatePre();
		}
		#endregion
	}
}

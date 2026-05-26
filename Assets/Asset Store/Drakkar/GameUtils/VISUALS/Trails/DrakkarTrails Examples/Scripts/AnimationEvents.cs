using Drakkar.GameUtils;
using UnityEngine;

namespace Drakkar.Examples
{
	public class AnimationEvents : MonoBehaviour
	{
		public DrakkarTrail Trail;

		public void StartTrail()
		{
			Trail.Begin();
		}

		public void StopTrail()
		{
			Trail.End();
		}
	}
}
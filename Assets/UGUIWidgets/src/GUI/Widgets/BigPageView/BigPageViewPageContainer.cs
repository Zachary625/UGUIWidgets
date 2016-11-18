using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Assets.src.GUI.BigPageView
{
	public class BigPageViewPageContainer : MonoBehaviour {
		public int pageIndex;

		public int LayoutPriority;

		public int layoutPriority {
			get {
				return this.LayoutPriority;
			}
		}
	}
}


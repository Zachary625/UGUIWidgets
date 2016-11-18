using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Assets.src.GUI.BigPageView {
	public class BigPageViewContentLayoutGroup : MonoBehaviour, ILayoutGroup, ILayoutSelfController {

		public GameObject bigPageViewGameObject;
		
		public void SetLayoutHorizontal() {
			BigPageView bigPageView = bigPageViewGameObject.GetComponent<BigPageView> ();
			RectTransform bigPageViewRectTransform = (bigPageViewGameObject.transform as RectTransform);
			float contentWidth = bigPageViewRectTransform.rect.width;
			float contentHeight = bigPageViewRectTransform.rect.height;
			RectTransform contentRectTransform = this.GetComponent<RectTransform> ();

			switch (bigPageView.direction) {
			case BigPageView.Direction.Horizontal: {
					contentWidth *= bigPageView.bigPageViewDelegate != null? bigPageView.pages: 0;

					if (contentWidth != contentRectTransform.rect.width || contentHeight != contentRectTransform.rect.height) {
						contentRectTransform.offsetMin = new Vector2(0, -contentHeight);
						contentRectTransform.offsetMax = new Vector2 (contentWidth, 0);
					}

					for (int childIndex = 0; childIndex < this.transform.childCount; childIndex++) {
						RectTransform pageContainerTransform = this.transform.GetChild (childIndex) as RectTransform;
						BigPageViewPageContainer pageContainer = pageContainerTransform.GetComponent<BigPageViewPageContainer> ();

						pageContainerTransform.anchorMin = new Vector2(0, 1);
						pageContainerTransform.anchorMax = new Vector2(0, 1);

						pageContainerTransform.offsetMin = new Vector2(bigPageViewRectTransform.rect.width * pageContainer.pageIndex, -bigPageViewRectTransform.rect.height);
						pageContainerTransform.offsetMax = new Vector2(bigPageViewRectTransform.rect.width * (pageContainer.pageIndex + 1), 0);

					}

					break;
				}					
			}

		}

		public void SetLayoutVertical() {
			BigPageView bigPageView = bigPageViewGameObject.GetComponent<BigPageView> ();
			RectTransform bigPageViewRectTransform = (bigPageViewGameObject.transform as RectTransform);
			float contentWidth = bigPageViewRectTransform.rect.width;
			float contentHeight = bigPageViewRectTransform.rect.height;
			RectTransform contentRectTransform = this.GetComponent<RectTransform> ();

			switch (bigPageView.direction) {
			case BigPageView.Direction.Vertical: {
					contentWidth *= bigPageView.bigPageViewDelegate != null?bigPageView.pages:0;

					if (contentWidth != contentRectTransform.rect.width || contentHeight != contentRectTransform.rect.height) {
						contentRectTransform.offsetMin = new Vector2(0, -contentHeight);
						contentRectTransform.offsetMax = new Vector2 (contentWidth, 0);
					}

					for (int childIndex = 0; childIndex < this.transform.childCount; childIndex++) {
						RectTransform pageContainerTransform = this.transform.GetChild (childIndex) as RectTransform;
						BigPageViewPageContainer pageContainer = pageContainerTransform.GetComponent<BigPageViewPageContainer> ();

						pageContainerTransform.anchorMin = new Vector2(0, 1);
						pageContainerTransform.anchorMax = new Vector2(0, 1);

						pageContainerTransform.offsetMin = new Vector2(0, -bigPageViewRectTransform.rect.height * (pageContainer.pageIndex + 1));
						pageContainerTransform.offsetMax = new Vector2(bigPageViewRectTransform.rect.width, -bigPageViewRectTransform.rect.height * pageContainer.pageIndex);
					}

					break;
				}					
			}

		}
	}
}


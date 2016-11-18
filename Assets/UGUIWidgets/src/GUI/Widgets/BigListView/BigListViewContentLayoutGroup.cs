using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Assets.src.GUI.BigListView
{
	public class BigListViewContentLayoutGroup : MonoBehaviour, ILayoutSelfController, ILayoutGroup {
		public GameObject bigListViewGameObject;

		public void SetLayoutHorizontal() {
			BigListView bigListView = bigListViewGameObject.GetComponent<BigListView> ();
			RectTransform bigListViewRectTransform = (bigListViewGameObject.transform as RectTransform);
			RectTransform contentRectTransform = this.GetComponent<RectTransform> ();

			switch (bigListView.direction) {
			case BigListView.Direction.Horizontal: {

					for (int childIndex = 0; childIndex < this.transform.childCount; childIndex++) {
						RectTransform itemContainerTransform = this.transform.GetChild (childIndex) as RectTransform;
						BigListViewItemContainer itemContainer = itemContainerTransform.GetComponent<BigListViewItemContainer> ();

						itemContainerTransform.anchorMin = new Vector2(0, 1);
						itemContainerTransform.anchorMax = new Vector2(0, 1);

						itemContainerTransform.offsetMin = new Vector2(bigListView.GetItemPosition(itemContainer.itemIndex), -bigListViewRectTransform.rect.height);
						itemContainerTransform.offsetMax = new Vector2(bigListView.GetItemPosition(itemContainer.itemIndex) + bigListView.GetItemSize(itemContainer.itemIndex), 0);
					}

					break;
				}					
			}

		}

		public void SetLayoutVertical() {
			BigListView bigListView = bigListViewGameObject.GetComponent<BigListView> ();
			RectTransform bigListViewRectTransform = (bigListViewGameObject.transform as RectTransform);
			RectTransform contentRectTransform = this.GetComponent<RectTransform> ();

			switch (bigListView.direction) {
			case BigListView.Direction.Vertical: {
					for (int childIndex = 0; childIndex < this.transform.childCount; childIndex++) {
						RectTransform itemContainerTransform = this.transform.GetChild (childIndex) as RectTransform;
						BigListViewItemContainer itemContainer = itemContainerTransform.GetComponent<BigListViewItemContainer> ();

						itemContainerTransform.anchorMin = new Vector2(0, 1);
						itemContainerTransform.anchorMax = new Vector2(0, 1);

						itemContainerTransform.offsetMin = new Vector2(0, bigListView.GetItemPosition(itemContainer.itemIndex) - bigListView.GetItemSize(itemContainer.itemIndex));
						itemContainerTransform.offsetMax = new Vector2(bigListViewRectTransform.rect.width, bigListView.GetItemPosition(itemContainer.itemIndex));
					}

					break;
				}					
			}

		}
	}
}


using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Assets.src.GUI.BigListView
{

	public class BigListViewTest : MonoBehaviour, IBigListViewDelegate {
		public Font font;

		private int _itemNum = 100;
		private Vector2 _itemSize;

		public InputField UpdateItemNumInputField;

		// Use this for initialization
		void Start () {
			BigListView bigListView = this.GetComponent<BigListView> ();
			switch (bigListView.direction) {
			case BigListView.Direction.Horizontal:
				{
					this._itemSize = new Vector2 (300, 50);
					break;
				}
			case BigListView.Direction.Vertical:
				{
					this._itemSize = new Vector2 (300, 50);
					break;
				}
			}
			bigListView.bigListViewDelegate = this;
		}

		// Update is called once per frame
		void Update () {
			
		}

		public int GetItems() {
			Debug.Log (" @ BigListViewTest.GetItems(): " + this._itemNum);
			return this._itemNum;			
		}

		public void GetItem(GameObject itemContainer, int itemIndex) {
//			Debug.Log (" @ BigListViewTest.GetItem(" + itemIndex + ")");
			Transform itemContentTransform = itemContainer.transform.Find ("ItemContent");
			if (!itemContentTransform) {
				GameObject itemContent = new GameObject ();
				itemContent.name = "ItemContent";
				RectTransform contentRT = itemContent.AddComponent<RectTransform> ();
				Text contentText = itemContent.AddComponent<Text> ();

				contentRT.SetParent (itemContainer.transform);

				contentRT.anchorMin = Vector2.zero;
				contentRT.anchorMax = Vector2.one;

				contentRT.offsetMin = Vector2.zero;
				contentRT.offsetMax = Vector2.zero;

				contentText.alignment = TextAnchor.MiddleCenter;
				contentText.fontSize = 16;
				contentText.color = Color.red;
				contentText.text = "ItemIndex: " + itemIndex;
				contentText.font = this.font;

			} else {
				itemContentTransform.GetComponent<Text> ().text = "ItemIndex: " + itemIndex;
			}
		}

		public Vector2 GetItemSize(int itemIndex) {
			return this._itemSize;
		}

		public void UpdateItemNum() {
			this._itemNum = System.Int32.Parse (this.UpdateItemNumInputField.text);

			this.GetComponent<BigListView> ().UpdateItems();

		}
	}
}


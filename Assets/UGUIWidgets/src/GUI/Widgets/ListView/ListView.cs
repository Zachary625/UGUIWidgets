using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

namespace Assets.src.GUI.ListView
{
	public class ListView : MonoBehaviour {
		public enum Direction
		{
			None,
			Vertical,
			Horizontal,
		}

		private Direction _direction = Direction.None;
		private GameObject _contentPanel;
		private List<GameObject> _itemContents = new List<GameObject>();

		public Direction direction {
			get {
				return this._direction;
			}
		}

		public GameObject itemContainerPrefab;

		private float _normalizedPosition {
			get { 
				float result = 0;
				ScrollRect scrollRect = this.GetComponent<ScrollRect> ();

				switch (this._direction) {
				case Direction.Horizontal:
					{
						result = scrollRect.horizontalNormalizedPosition;
						break;
					}
				case Direction.Vertical:
					{
						result = scrollRect.verticalNormalizedPosition;
						break;
					}
				}
				return result;
			}
			set { 
				ScrollRect scrollRect = this.GetComponent<ScrollRect> ();

				switch (this._direction) {
				case Direction.Horizontal:
					{
						scrollRect.horizontalNormalizedPosition = value;
						break;
					}
				case Direction.Vertical:
					{
						scrollRect.verticalNormalizedPosition = value;
						break;
					}
				}
			}
		}


		// Use this for initialization
		void Start () {
			this._contentPanel = this.transform.Find("Viewport").Find("Content").gameObject;

			bool hasVertical = this._contentPanel.GetComponent<VerticalLayoutGroup> () != null;
			bool hasHorizontal = this._contentPanel.GetComponent<HorizontalLayoutGroup> () != null;

			if (hasVertical && !hasHorizontal) {
				this._direction = Direction.Vertical;
			} else if(!hasVertical && hasHorizontal) {
				this._direction = Direction.Horizontal;
			}
		}

		// Update is called once per frame
		void Update () {

		}

		private bool _isValidItemContent(GameObject content) {
			if (content == null) {
				return false;
			}
			if (content.GetComponent<RectTransform> () == null) {
				return false;
			}
			if (this._itemContents.IndexOf (content) >= 0) {
				return false;
			}
			return true;
		}


		public void addItem(GameObject content) {
			if (!this._isValidItemContent (content)) {
				return;
			}
			this._itemContents.Add (content);
			this._updateItems ();
		}

		public void addItem(GameObject content, int index) {
			if (!this._isValidItemContent (content)) {
				return;
			}
			this._itemContents.Insert (index, content);
			this._updateItems ();
		}

		public GameObject getItem(int itemIndex) {
			try {
				return this._itemContents[itemIndex];				
			}
			catch {
				return null;
			}
		}

		public void removeItem(GameObject content) {
			this._itemContents.Remove (content);
			this._updateItems ();
		}

		public void removeItem(int index) {
			this._itemContents.RemoveAt (index);
			this._updateItems ();
		}

		public void removeAllItems() {
			this._itemContents.Clear ();
			this._updateItems ();
		}

		private void _updateItems() {
			if (this._itemContents.Count < this._contentPanel.transform.childCount) {
				while (true) {
					GameObject item = this._contentPanel.transform.GetChild (this._itemContents.Count).gameObject;
					if (item) {
						this._removeItemContainer (item);
					} else {
						break;
					}
				}
			}
			else {
				while (this._itemContents.Count > this._contentPanel.transform.childCount) {
					this._createItemContainer ();
				}
			}

			for (int itemIndex = 0; itemIndex < this._itemContents.Count; itemIndex++) {
				GameObject itemContainer = this._contentPanel.transform.GetChild (itemIndex).gameObject;
				ListViewItemContainer itemContainerComponent = itemContainer.GetComponent<ListViewItemContainer> ();

				itemContainer.transform.DetachChildren ();
				this._itemContents [itemIndex].transform.SetParent (itemContainer.transform);

				itemContainerComponent.itemIndex = itemIndex;
				itemContainerComponent.content = this._itemContents[itemIndex];
			}

		}

		private GameObject _createItemContainer() {
			GameObject itemContainer = Instantiate (this.itemContainerPrefab);
			ListViewItemContainer itemContainerComponent = itemContainer.GetComponent<ListViewItemContainer> ();
//			ContentSizeFitter contentSizeFitter = itemContainer.AddComponent<ContentSizeFitter> ();

			itemContainer.transform.SetParent (this._contentPanel.transform);

			itemContainerComponent.listView = this.gameObject;

			switch (this.direction) {
			case Direction.Vertical:
				{
					VerticalLayoutGroup verticalLayoutGroup = itemContainer.AddComponent<VerticalLayoutGroup> ();
					verticalLayoutGroup.childAlignment = TextAnchor.UpperCenter;
					verticalLayoutGroup.childForceExpandWidth = true;
					verticalLayoutGroup.childForceExpandHeight = false;

//					contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
//					contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
					break;
				}
			case Direction.Horizontal:
				{
					HorizontalLayoutGroup horizontalLayoutGroup = itemContainer.AddComponent<HorizontalLayoutGroup> ();
					horizontalLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
					horizontalLayoutGroup.childForceExpandWidth = false;
					horizontalLayoutGroup.childForceExpandHeight = true;

//					contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
//					contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
					break;
				}
			}
				
			return itemContainer;
		}

		private void _removeItemContainer(GameObject itemContainer) {
			ListViewItemContainer itemContainerComponent = itemContainer.GetComponent<ListViewItemContainer> ();

			itemContainerComponent.listView = null;
			itemContainerComponent.content = null;

			itemContainer.transform.SetParent (null);
			itemContainer.transform.DetachChildren ();

		}






	}
}

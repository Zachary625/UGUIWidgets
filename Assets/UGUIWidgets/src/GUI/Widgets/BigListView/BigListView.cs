using UnityEngine;
using System.Collections;

using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;


namespace Assets.src.GUI.BigListView
{
	public class BigListView : MonoBehaviour, IDragHandler {
		public enum Direction
		{
			Vertical,
			Horizontal,
		}

		public Direction direction;

		public GameObject itemContainerPrefab;

		public int items {
			get { 
				return this._items;
			}
		}

		public IBigListViewDelegate bigListViewDelegate {
			get {
				return this._bigListViewDelegate;
			}
			set { 
				this._bigListViewDelegate = value;
				this.UpdateItems ();
			}
		}

		private GameObject _contentPanel;
		private List<GameObject> _itemContainerCache = new List<GameObject>();

		private int _items = 0;

		private int _itemCacheSize = 1;

		private IBigListViewDelegate _bigListViewDelegate = null;

		private float _displaySegmentHead {
			get { 
				float result = 0;
				if (this._contentPanel != null) {
					switch (this.direction) {
					case Direction.Horizontal:
						{
							result = - this._contentPanel.GetComponent<RectTransform> ().localPosition.x;
							break;
						}
					case Direction.Vertical:
						{
							result = - this._contentPanel.GetComponent<RectTransform> ().localPosition.y;						
							break;
						}
					}
				}
				return result;
			}
		}

		private float _displaySegmentTail {
			get {
				float result = 0;
				if (this._contentPanel != null) {
					switch (this.direction) {
					case Direction.Horizontal:
						{
							result = - this._contentPanel.GetComponent<RectTransform> ().localPosition.x + this.GetComponent<RectTransform>().rect.width;
							break;
						}
					case Direction.Vertical:
						{
							result = - this._contentPanel.GetComponent<RectTransform> ().localPosition.y - this.GetComponent<RectTransform>().rect.height;						
							break;
						}
					}
				}
				return result;
			}
		}

/*
		private float _normalizedPosition {
			get { 
				float result = 0;
				ScrollRect scrollRect = this.GetComponent<ScrollRect> ();

				switch (this.direction) {
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

				switch (this.direction) {
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
*/
		private Vector2 _contentSize = Vector2.zero;
		private List<float> _itemSizeList = new List<float> ();
		private List<float> _itemPositionList = new List<float> ();

		public Vector2 contentSize {
			get {
				return this._contentSize;
			}
		}

		public float GetItemSize(int itemIndex) {
			return this._itemSizeList [itemIndex];
		}

		public float GetItemPosition(int itemIndex) {
			return this._itemPositionList [itemIndex];
		}

		void Awake() {
			this._contentPanel = this.transform.Find("Viewport/Content").gameObject;
		}


		// Use this for initialization
		void Start () {
			ScrollRect scrollRect = this.GetComponent<ScrollRect> ();
			scrollRect.horizontal = (this.direction == Direction.Horizontal);
			scrollRect.vertical = (this.direction == Direction.Vertical);
		}

		// Update is called once per frame
		void Update () {
		}

		private GameObject _allocItemContainer() {
			GameObject itemContainerGameObject;

			if(this._itemContainerCache.Count > 0) {
				itemContainerGameObject = this._itemContainerCache[0];				
				this._itemContainerCache.RemoveAt (0);
				itemContainerGameObject.SetActive (true);
			} else {
				itemContainerGameObject = Instantiate (this.itemContainerPrefab);
			}
			itemContainerGameObject.transform.SetParent (this._contentPanel.transform);
			return itemContainerGameObject;
		}

		private void _freeItemContainer(GameObject itemContainerGameObject) {
			if (!this._itemContainerCache.Contains (itemContainerGameObject)) {
				this._itemContainerCache.Add(itemContainerGameObject);
			}
			if (itemContainerGameObject.activeSelf) {
				itemContainerGameObject.SetActive (false);
			}
		}

		private void _clearItemContainers () {
			this._itemPositionList.Clear ();
			this._itemSizeList.Clear ();

			for (int childIndex = 0; childIndex < this._contentPanel.transform.childCount; childIndex++) {
				GameObject itemContainerGameObject = this._contentPanel.transform.GetChild (childIndex).gameObject;
				this._freeItemContainer (itemContainerGameObject);
			}
			this._contentPanel.transform.DetachChildren ();
			while (this._itemContainerCache.Count != 0) {
				GameObject itemContainer = this._itemContainerCache [0];
				this._itemContainerCache.RemoveAt (0);
				Destroy (itemContainer);
			}

		}

		private void _updateItems() {
			// TODO
			int minDisplayItemIndex = -1;
			int maxDisplayItemIndex = -1;

			for (int itemIndex = 0; itemIndex < this.items; itemIndex++) {
				bool displayItem = this._isItemInDisplaySegment (itemIndex);
				if (displayItem) {
					if (minDisplayItemIndex < 0) {
						minDisplayItemIndex = itemIndex;
					} else {
						minDisplayItemIndex = Mathf.Min (minDisplayItemIndex, itemIndex);
					}

					if (maxDisplayItemIndex < 0) {
						maxDisplayItemIndex = itemIndex;
					} else {
						maxDisplayItemIndex = Mathf.Max (maxDisplayItemIndex, itemIndex);
					}
				}
			}

//			Debug.Log (" @ BigListView._updateItems(): " + this._displaySegmentHead + " -> " + this._displaySegmentTail);
//			Debug.Log (" @ BigListView._updateItems(): [" + minDisplayItemIndex + ", " + maxDisplayItemIndex + "]");

			bool[] cacheStatus = new bool[2 * this._itemCacheSize + (maxDisplayItemIndex - minDisplayItemIndex) + 1];

			for (int childIndex = 0; childIndex < this._contentPanel.transform.childCount; childIndex++) {
				GameObject itemContainerGameObject = this._contentPanel.transform.GetChild (childIndex).gameObject;
				if (itemContainerGameObject.activeSelf) {
					BigListViewItemContainer itemContainer = itemContainerGameObject.GetComponent<BigListViewItemContainer> ();
					if (itemContainer.itemIndex < minDisplayItemIndex - this._itemCacheSize || itemContainer.itemIndex > maxDisplayItemIndex + this._itemCacheSize) {
						this._freeItemContainer (itemContainerGameObject);
//						Debug.Log (" @ BigListView._free(" + itemContainer.itemIndex + ")");
					} else {
						cacheStatus [itemContainer.itemIndex - (minDisplayItemIndex - this._itemCacheSize)] = true;
					}
				}
			}

			for(int itemIndex = minDisplayItemIndex - this._itemCacheSize; itemIndex <= maxDisplayItemIndex + this._itemCacheSize; itemIndex++) {
				if (itemIndex >= 0 && itemIndex <= this.items - 1 && !cacheStatus [itemIndex - (minDisplayItemIndex - this._itemCacheSize)]) {
					GameObject itemContainerGameObject = this._allocItemContainer ();
					BigListViewItemContainer itemContainer = itemContainerGameObject.GetComponent<BigListViewItemContainer> ();
//					Debug.Log (" @ BigListView._alloc(" + itemIndex + "): prev: " + itemContainer.itemIndex);
					itemContainer.itemIndex = itemIndex;
					this.bigListViewDelegate.GetItem (itemContainerGameObject, itemIndex);
				}
			}


		}

		public bool _isItemInDisplaySegment(int index) {
			float itemHead = this._itemPositionList [index];
			float itemTail = this._itemPositionList [index];

			bool result = true;
			switch (this.direction) {
			case Direction.Horizontal:
				{
					itemTail += this._itemSizeList [index];					
					if (itemTail < this._displaySegmentHead) {
						result = false;
					}
					if (itemHead > this._displaySegmentTail) {
						result = false;						
					}

					break;
				}
			case Direction.Vertical:
				{
					itemTail -= this._itemSizeList [index];
					if (itemTail > this._displaySegmentHead) {
						result = false;
					}
					if (itemHead < this._displaySegmentTail) {
						result = false;						
					}
					break;
				}
			}

//			Debug.Log (" @ BigListView._isItemInDisplaySegment(" + index + "): " + result + "\nitem: " + itemHead + " -> " + itemTail + "\nsegment: " + this._displaySegmentHead + " -> " + this._displaySegmentTail);

			return result;
		}

		public void OnDrag(PointerEventData data) {
			
		}


		public void UpdateItems() {
			this._clearItemContainers ();

			if (this.bigListViewDelegate == null) {
				this._items = 0;
				return;
			}

			this._items = this.bigListViewDelegate.GetItems ();

			this._updateContentLayoutData ();
			this._updateItems();
		}

		private void _updateContentLayoutData() {
			RectTransform bigPageViewRectTransform = this.GetComponent<RectTransform>();
			float listViewWidth = bigPageViewRectTransform.rect.width;
			float listViewHeight = bigPageViewRectTransform.rect.height;
			RectTransform contentRectTransform = this._contentPanel.GetComponent<RectTransform> ();

			float contentWidth = listViewWidth;
			float contentHeight = listViewHeight;

			switch (this.direction) {
			case Direction.Horizontal: {
					contentWidth = 0;
					if (this.bigListViewDelegate != null) {
						for (int itemIndex = 0; itemIndex < this.items; itemIndex++) {
							this._itemPositionList.Add (contentWidth);
							this._itemSizeList.Add (this.bigListViewDelegate.GetItemSize(itemIndex).x);
							contentWidth += this._itemSizeList[itemIndex];
						}
					}
					break;
				}					
			case Direction.Vertical: {
					contentHeight = 0;
					if (this.bigListViewDelegate != null) {
						for (int itemIndex = 0; itemIndex < this.items; itemIndex++) {
							this._itemPositionList.Add (-contentHeight);
							this._itemSizeList.Add (this.bigListViewDelegate.GetItemSize(itemIndex).y);
							contentHeight += this._itemSizeList[itemIndex];
						}
					}
					break;
				}					
			}

			Debug.Log(" @ BigListView._updateContentLayoutData(): contentSize: " + contentWidth + " * " + contentHeight);

			Vector3 contentLocalPosition = contentRectTransform.localPosition;

			contentRectTransform.anchorMin = new Vector2(0, 1);
			contentRectTransform.anchorMax = new Vector2(0, 1);

			contentRectTransform.offsetMin = new Vector2(0, -contentHeight);
			contentRectTransform.offsetMax = new Vector2 (contentWidth, 0);

			contentLocalPosition.x = Mathf.Max (contentLocalPosition.x, -contentWidth + listViewWidth);
			contentLocalPosition.y = Mathf.Min (contentLocalPosition.y, contentHeight - listViewHeight);

			contentRectTransform.localPosition = contentLocalPosition;
		}

		public void OnScroll() {
//			Debug.Log (" @ BigListView.OnScroll(): " + this._displaySegmentHead + " -> " + this._displaySegmentTail);
			this._updateItems();
		}
	}

	public interface IBigListViewDelegate {
		int GetItems();

		void GetItem(GameObject itemContainer, int itemIndex);

		Vector2 GetItemSize(int itemIndex);
	}
}


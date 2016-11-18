using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.src.GUI.BigPageView
{
	public class BigPageView : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler {
		public enum Direction
		{
			None,
			Vertical,
			Horizontal,
		}

		public struct BigPageViewEventArgs {
			public int PrevPageIndex;
			public int NextPageIndex;

			public BigPageViewEventArgs(int prevPageIndex, int nextPageIndex) {
				this.PrevPageIndex = prevPageIndex;
				this.NextPageIndex = nextPageIndex;
			}
		}

		public delegate void PageIndexChangeHandler(GameObject bigPageView, BigPageViewEventArgs args);
		public delegate void PageScrollStartHandler(GameObject bigPageView, BigPageViewEventArgs args);
		public delegate void PageScrollStopHandler(GameObject bigPageView, BigPageViewEventArgs args);
		public delegate void PageDragStartHandler (GameObject bigPageView);
		public delegate void PageDragStopHandler (GameObject bigPageView);

		public PageIndexChangeHandler pageIndexChangeHandler;
		public PageScrollStartHandler pageScrollStartHandler;
		public PageScrollStopHandler pageScrollStopHandler;
		public PageDragStartHandler pageDragStartHandler;
		public PageDragStopHandler pageDragStopHandler;

		public Direction direction;

		public GameObject pageContainerPrefab;

		public float scrollDuration = 0.2f;

		public int pageIndex {
			get { 
				return this._pageIndex;	
			}
		}

		public int pages {
			get { 
				return this._pages;
			}
		}

		public bool dragging {
			get {
				return this._dragging;
			}
		}

		public bool scrolling {
			get { 
				return this._scrolling;
			}
		}

		public IBigPageViewDelegate bigPageViewDelegate {
			get {
				return this._bigPageViewDelegate;
			}
			set { 
				this._bigPageViewDelegate = value;
				this.UpdatePages ();
			}
		}

		private GameObject _contentPanel;
//		private GameObject _pageContainerCache;
		private List<GameObject> _pageContainerCache = new List<GameObject>();

		private int _pages = 0;
		private int _pageIndex = 0;

		private int _scrollFromPageIndex = -1;
		private int _scrollToPageIndex = -1;

		private int _pageCacheSize = 2;

		private bool _dragging = false;
		private bool _scrolling = false;
		private float _beginPosition = 0;
		private float _endPosition = 0;
		private float _scrollTime = 0;
		private float _scrollAcceleration = 0;
		private Coroutine _scrollCoroutine = null;
		private IBigPageViewDelegate _bigPageViewDelegate = null;



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

		void Awake() {
			this._contentPanel = this.transform.Find("Viewport/Content").gameObject;
//			this._pageContainerCache = this.transform.Find ("PageContainerCache").gameObject;
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

		private GameObject _allocPageContainer() {
			GameObject pageContainerGameObject;

//			if (this._pageContainerCache.transform.childCount > 0) {
//				pageContainerGameObject = this._pageContainerCache.transform.GetChild (0).gameObject;
//				pageContainerGameObject.SetActive (true);
			if(this._pageContainerCache.Count > 0) {
				pageContainerGameObject = this._pageContainerCache[0];				
				this._pageContainerCache.RemoveAt (0);
				pageContainerGameObject.SetActive (true);
			} else {
				pageContainerGameObject = Instantiate (this.pageContainerPrefab);
			}
			pageContainerGameObject.transform.SetParent (this._contentPanel.transform);
			return pageContainerGameObject;
		}

		private void _freePageContainer(GameObject pageContainerGameObject) {
//			pageContainerGameObject.transform.SetParent (this._pageContainerCache.transform);
			if(!this._pageContainerCache.Contains(pageContainerGameObject)) {
				this._pageContainerCache.Add(pageContainerGameObject);
			}
			if (pageContainerGameObject.activeSelf) {
				pageContainerGameObject.SetActive (false);
			}
		}

		private void _clearPageContainers () {
			for (int childIndex = 0; childIndex < this._contentPanel.transform.childCount; childIndex++) {
				GameObject itemContainerGameObject = this._contentPanel.transform.GetChild (childIndex).gameObject;
				this._freePageContainer (itemContainerGameObject);
			}
			this._contentPanel.transform.DetachChildren ();
			while (this._pageContainerCache.Count != 0) {
				GameObject pageContainer = this._pageContainerCache [0];
				this._pageContainerCache.RemoveAt (0);
				Destroy (pageContainer);
			}

		}

		private void _updatePages() {
			// TODO
//			Debug.Log(" @ BigPageView._updatePages(): " + this._pageIndex);
			bool[] cacheStatus = new bool[2 * this._pageCacheSize + 1];
			for (int childIndex = 0; childIndex < this._contentPanel.transform.childCount; childIndex++) {
				GameObject pageContainerGameObject = this._contentPanel.transform.GetChild (childIndex).gameObject;
				if (pageContainerGameObject.activeSelf) {
					BigPageViewPageContainer pageContainer = pageContainerGameObject.GetComponent<BigPageViewPageContainer> ();
					if (pageContainer.pageIndex < this._pageIndex - this._pageCacheSize || pageContainer.pageIndex > this._pageIndex + this._pageCacheSize) {
						this._freePageContainer (pageContainerGameObject);
						//					Debug.Log (" @ BigPageview._free(" + pageContainer.pageIndex + ")");
					} else {
						cacheStatus [pageContainer.pageIndex - (this._pageIndex - this._pageCacheSize)] = true;
					}
				}
			}

			for(int pageIndex = this._pageIndex - this._pageCacheSize; pageIndex <= this._pageIndex + this._pageCacheSize; pageIndex++) {
				if (pageIndex >= 0 && pageIndex <= this.pages - 1 && !cacheStatus [pageIndex - (this._pageIndex - this._pageCacheSize)]) {
					GameObject pageContainerGameObject = this._allocPageContainer ();
//					Debug.Log (" @ BigPageview._alloc(" + pageIndex + ")");
					BigPageViewPageContainer pageContainer = pageContainerGameObject.GetComponent<BigPageViewPageContainer> ();
					pageContainer.pageIndex = pageIndex;
					this.bigPageViewDelegate.GetPage (pageContainerGameObject, pageIndex);
				}
			}
		
		
		}

		private float _pageIndexToNormalizedPosition(int pageIndex) {
			if (this.pages <= 1) {
				return 0;
			}
			return (float)(pageIndex * 1.0 / (this.pages - 1));
		}

		private int _normalizedPositionToPageIndex(float position) {
			if (this.pages <= 1) {
				return 0;
			}
			return Mathf.Clamp( Mathf.RoundToInt(this._normalizedPosition * (this.pages - 1)), 0, this.pages - 1);
		}

		public void JumpToPage(int pageIndex) {
			if (this.pages < 1) {
				return;
			}

			if (this._dragging) {
				return;
			}

			this._endScroll ();

			pageIndex = Mathf.Clamp (pageIndex, 0, this.pages - 1);

			int prevPageIndex = this._pageIndex;
			this._pageIndex = pageIndex;
			this._updatePages ();
			this._normalizedPosition = this._pageIndexToNormalizedPosition (pageIndex);


			if (prevPageIndex != this._pageIndex) {
				if (this.pageIndexChangeHandler != null) {
					this.pageIndexChangeHandler (this.gameObject, new BigPageViewEventArgs (prevPageIndex, this._pageIndex));
				}
			}
		}

		public void OnBeginDrag(PointerEventData data) {
			//			base.OnBeginDrag (data);

			this._dragging = true;
			this._endScroll ();

			if (this.pageDragStartHandler != null) {
				this.pageDragStartHandler (this.gameObject);
			}
		}

		public void OnEndDrag(PointerEventData data) {
			//			base.OnEndDrag (data);

			this._dragging = false;
			if (this.pageDragStopHandler != null) {
				this.pageDragStopHandler (this.gameObject);
			}
			this._beginScroll (this._normalizedPositionToPageIndex(this._normalizedPosition));
		}

		public void OnDrag(PointerEventData data) {
			this._updatePageIndex ();
		}

		private void _beginScroll(int pageIndex) {
			if (this.pages < 1) {
				return;
			}

			this._endScroll ();

			pageIndex = Mathf.Clamp (pageIndex, 0, this.pages - 1);
			this._scrollFromPageIndex = pageIndex;
			this._scrollToPageIndex = pageIndex;

			if (this._scrollFromPageIndex != this._scrollToPageIndex) {
				if (this.pageScrollStartHandler != null) {
					this.pageScrollStartHandler (this.gameObject, new BigPageViewEventArgs (this._scrollFromPageIndex, this._scrollToPageIndex));
				}
			}

			this._beginPosition = this._normalizedPosition;
			this._endPosition = this._pageIndexToNormalizedPosition (pageIndex);

			this._scrollAcceleration = 2 * (this._endPosition - this._beginPosition) / this.scrollDuration / this.scrollDuration;

			this._scrollTime = 0;
			this._scrolling = true;


			this._scrollCoroutine = StartCoroutine (this._scroll());
		}

		private void _endScroll() {
			if (this._scrollCoroutine != null) {
				StopCoroutine (this._scrollCoroutine);
				this._scrollCoroutine = null;
			}
			if (this._scrolling) {
				this._scrolling = false;
				this._scrollTime = 0;

				this._beginPosition = 0;
				this._endPosition = 0;

				this._scrollAcceleration = 0;

				if (this._scrollFromPageIndex != this._scrollToPageIndex) {
					if (this.pageScrollStopHandler != null) {
						this.pageScrollStopHandler (this.gameObject, new BigPageViewEventArgs (this._scrollFromPageIndex, this._scrollToPageIndex));
					}
				}

				this._scrollFromPageIndex = -1;
				this._scrollToPageIndex = -1;
			}
		}

		private IEnumerator _scroll() {
			//			Debug.Log (" @ PageView._scroll(): " + this._dragging + ", " + this._scrolling);
			while (!this._dragging && this._scrolling) {
				this._scrollTime += Time.deltaTime;
				if (this._scrollTime >= this.scrollDuration) {
					this._normalizedPosition = this._endPosition;
					this._endScroll ();
				} else {
					this._normalizedPosition = (float)(this._beginPosition + 0.5 * this._scrollAcceleration * this._scrollTime * this._scrollTime);
					this._updatePageIndex ();
					yield return null;
				}
			}
		}

		public void UpdatePages() {
			this._clearPageContainers ();
			if (this._bigPageViewDelegate == null) {
				return;
			}

			this._clearPageContainers ();
			this._pages = this._bigPageViewDelegate.GetPages ();
			if (this._pageIndex >= this._pages && this._pages > 0) {
				int prevPageIndex = this._pageIndex;
				this._pageIndex = Mathf.Max (0, this._pages - 1);
				if (this.pageIndexChangeHandler != null) {
					this.pageIndexChangeHandler(this.gameObject, new BigPageViewEventArgs (prevPageIndex, this._pageIndex));
				}
			}

 
			this._updateContentPanelRect ();
			this._normalizedPosition = this._pageIndexToNormalizedPosition (this._pageIndex);
			this._updatePages();
		}

		private void _updateContentPanelRect() {
			RectTransform bigPageViewRectTransform = this.GetComponent<RectTransform>();
			float contentWidth = bigPageViewRectTransform.rect.width;
			float contentHeight = bigPageViewRectTransform.rect.height;
			RectTransform contentRectTransform = this._contentPanel.GetComponent<RectTransform> ();

			switch (this.direction) {
			case Direction.Horizontal: {
					contentWidth *= this.bigPageViewDelegate != null? this.pages: 0;
					break;
				}					
			case Direction.Vertical: {
					contentWidth *= this.bigPageViewDelegate != null?this.pages:0;
					break;
				}					
			}
			contentRectTransform.anchorMin = new Vector2(0, 1);
			contentRectTransform.anchorMax = new Vector2(0, 1);

			contentRectTransform.offsetMin = new Vector2(0, -contentHeight);
			contentRectTransform.offsetMax = new Vector2 (contentWidth, 0);

		}

		private void _updatePageIndex() {
			int currentPageIndex = this._normalizedPositionToPageIndex (this._normalizedPosition);
			if (currentPageIndex != this._pageIndex) {
				int prevPageIndex = this._pageIndex;
				this._pageIndex = currentPageIndex;
				this._updatePages ();
				if (this.pageIndexChangeHandler != null) {
					this.pageIndexChangeHandler(this.gameObject, new BigPageViewEventArgs (prevPageIndex, currentPageIndex));
				}
			}
		}



	}

	public interface IBigPageViewDelegate {
		int GetPages();

		void GetPage(GameObject pageContainer, int pageIndex);
	}

}

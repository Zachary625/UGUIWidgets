using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.src.GUI.PageView
{
	public class PageView : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler {
		private enum _Direction
		{
			None,
			Vertical,
			Horizontal,
		}

		public struct PageViewEventArgs {
			public int PrevPageIndex;
			public int NextPageIndex;

			public PageViewEventArgs(int prevPageIndex, int nextPageIndex) {
				this.PrevPageIndex = prevPageIndex;
				this.NextPageIndex = nextPageIndex;
			}
		}

		public delegate void PageIndexChangeHandler(GameObject pageView, PageViewEventArgs args);
		public delegate void PageScrollStartHandler(GameObject pageView, PageViewEventArgs args);
		public delegate void PageScrollStopHandler(GameObject pageView, PageViewEventArgs args);
		public delegate void PageDragStartHandler (GameObject pageView);
		public delegate void PageDragStopHandler (GameObject pageView);

		public PageIndexChangeHandler pageIndexChangeHandler;
		public PageScrollStartHandler pageScrollStartHandler;
		public PageScrollStopHandler pageScrollStopHandler;
		public PageDragStartHandler pageDragStartHandler;
		public PageDragStopHandler pageDragStopHandler;

		private _Direction _direction = _Direction.None;

		public GameObject pageContainerPrefab;

		public float scrollDuration = 0.2f;

		public int pageIndex {
			get { 
				return this._pageIndex;	
			}
		}

		public int pages {
			get { 
				return this._pageContents.Count;
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

		private GameObject _contentPanel;
		private List<GameObject> _pageContents = new List<GameObject>();

		private int _pageIndex = 0;

		private int _scrollFromPageIndex = -1;
		private int _scrollToPageIndex = -1;

		private bool _dragging = false;
		private bool _scrolling = false;
		private float _beginPosition = 0;
		private float _endPosition = 0;
		private float _scrollTime = 0;
		private float _scrollAcceleration = 0;
		private Coroutine _scrollCoroutine = null;

		private float _normalizedPosition {
			get { 
				float result = 0;
				ScrollRect scrollRect = this.GetComponent<ScrollRect> ();

				switch (this._direction) {
				case _Direction.Horizontal:
					{
						result = scrollRect.horizontalNormalizedPosition;
						break;
					}
				case _Direction.Vertical:
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
				case _Direction.Horizontal:
					{
						scrollRect.horizontalNormalizedPosition = value;
						break;
					}
				case _Direction.Vertical:
					{
						scrollRect.verticalNormalizedPosition = value;
						break;
					}
				}
			}
		}

		void Awake() {
			this._contentPanel = this.transform.Find("Viewport/Content").gameObject;
		}

		// Use this for initialization
		void Start () {
			bool hasVertical = this._contentPanel.GetComponent<VerticalLayoutGroup> () != null;
			bool hasHorizontal = this._contentPanel.GetComponent<HorizontalLayoutGroup> () != null;

			if (hasVertical && !hasHorizontal) {
				this._direction = _Direction.Vertical;
			} else if(!hasVertical && hasHorizontal) {
				this._direction = _Direction.Horizontal;
			}
		}

		// Update is called once per frame
		void Update () {
		}

		private bool _isValidPageContent(GameObject content) {
			if (content == null) {
				return false;
			}
			if (content.GetComponent<RectTransform> () == null) {
				return false;
			}
			if (this._pageContents.IndexOf (content) >= 0) {
				return false;
			}
			return true;
		}

		public void addPage(GameObject content) {
			if (!this._isValidPageContent (content)) {
				return;
			}
			this._pageContents.Add (content);
			this._updatePages ();
 		}

		public void addPage(GameObject content, int index) {
			if (!this._isValidPageContent (content)) {
				return;
			}
			this._pageContents.Insert (index, content);
			this._updatePages ();
		}

		public GameObject getPage(int pageIndex) {
			try {
				return this._pageContents[pageIndex];				
			}
			catch {
				return null;
			}
		}

		public void removePage(GameObject content) {
			this._pageContents.Remove (content);
			this._updatePages ();
		}

		public void removePage(int index) {
			this._pageContents.RemoveAt (index);
			this._updatePages ();
		}

		public void removeAllPages() {
			this._pageContents.Clear ();
			this._updatePages ();
		}

		private void _updatePages() {
			if (this._pageContents.Count < this._contentPanel.transform.childCount) {
				while (true) {
					GameObject pageContainer = this._contentPanel.transform.GetChild (this._pageContents.Count).gameObject;
					if (pageContainer) {
						this._removePageContainer (pageContainer);
					} else {
						break;
					}
				}
			}
			else {
				while (this._pageContents.Count > this._contentPanel.transform.childCount) {
					this._createPageContainer ();
				}
			}

			for (int pageIndex = 0; pageIndex < this._pageContents.Count; pageIndex++) {
				GameObject pageContainer = this._contentPanel.transform.GetChild (pageIndex).gameObject;
				PageViewPageContainer pageComponent = pageContainer.GetComponent<PageViewPageContainer> ();

				pageContainer.transform.DetachChildren ();
				this._pageContents [pageIndex].transform.SetParent (pageContainer.transform);

				pageComponent.pageIndex = pageIndex;
				pageComponent.content = this._pageContents[pageIndex];
			}
			if (this._pageContents.Count > 0 && this._pageIndex >= this._pageContents.Count) {
				this.JumpToPage (this._pageContents.Count - 1);
			}
		}

		private GameObject _createPageContainer() {
			GameObject pageContainer = Instantiate (this.pageContainerPrefab);
			PageViewPageContainer pageComponent = pageContainer.GetComponent<PageViewPageContainer> ();

			pageContainer.transform.SetParent (this._contentPanel.transform);

			pageComponent.pageView = this.gameObject;

			return pageContainer;
		}

		private void _removePageContainer(GameObject pageContainer) {
			PageViewPageContainer pageComponent = pageContainer.GetComponent<PageViewPageContainer> ();

			pageComponent.pageView = null;
			pageComponent.content = null;

			pageContainer.transform.SetParent (null);
			pageContainer.transform.DetachChildren ();

		}

		private float _pageIndexToNormalizedPosition(int pageIndex) {
			return (float)(pageIndex * 1.0 / (this._pageContents.Count - 1));
		}

		private int _normalizedPositionToPageIndex(float position) {
			return Mathf.Clamp( Mathf.RoundToInt(this._normalizedPosition * (this._pageContents.Count - 1)), 0, this._pageContents.Count - 1);
		}

		public void JumpToPage(int pageIndex) {
			if (this._pageContents.Count < 1) {
				return;
			}

			if (this._dragging) {
				return;
			}

			this._endScroll ();

			pageIndex = Mathf.Clamp (pageIndex, 0, this._pageContents.Count - 1);

			this._normalizedPosition = this._pageIndexToNormalizedPosition (pageIndex);
			this._updatePageIndex ();

		}

		public void ScrollToPage(int pageIndex) {
			if (this._pageContents.Count <= 1) {
				return;
			}

			if (this._dragging) {
				return;
			}

			this._beginScroll (pageIndex);
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
//			Debug.Log (" @ PageView.OnDrag(): " + this._dragging + ", " + this._scrolling);
			this._updatePageIndex ();
		}
			
		private void _beginScroll(int pageIndex) {
			if (this._pageContents.Count <= 1) {
				return;
			}

			this._endScroll ();

			pageIndex = Mathf.Clamp (pageIndex, 0, this._pageContents.Count - 1);
			this._scrollFromPageIndex = this._pageIndex;
			this._scrollToPageIndex = pageIndex;

			if (this._scrollFromPageIndex != _scrollToPageIndex) {
				if (this.pageScrollStartHandler != null) {
					this.pageScrollStartHandler (this.gameObject, new PageViewEventArgs (this._scrollFromPageIndex, this._scrollToPageIndex));
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
						this.pageScrollStopHandler (this.gameObject, new PageViewEventArgs (_scrollFromPageIndex, this._scrollToPageIndex));
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

		private void _updatePageIndex() {
			int currentPageIndex = this._normalizedPositionToPageIndex (this._normalizedPosition);
			if (currentPageIndex != this._pageIndex) {
				int prevPageIndex = this._pageIndex;
				this._pageIndex = currentPageIndex;
				if (this.pageIndexChangeHandler != null) {
					this.pageIndexChangeHandler (this.gameObject, new PageViewEventArgs (prevPageIndex, currentPageIndex));
				}
			}
		}

	}
}

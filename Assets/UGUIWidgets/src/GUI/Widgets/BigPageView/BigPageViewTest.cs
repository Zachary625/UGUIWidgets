using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Assets.src.GUI.BigPageView {
	public class BigPageViewTest : MonoBehaviour, IBigPageViewDelegate {

		public Font font;

		private int _pageNum = 100;

		public InputField gotoPageIndexInputField;

		public InputField updatePageNumInputField;

		// Use this for initialization
		void Start () {
			BigPageView bigPageView = this.GetComponent<BigPageView> ();

			bigPageView.pageIndexChangeHandler += new BigPageView.PageIndexChangeHandler (delegate(GameObject sender, BigPageView.BigPageViewEventArgs args) {
				Debug.Log(" @ BigPageViewTest.PageIndexChangeHandler(" + args.PrevPageIndex + " -> " + args.NextPageIndex + ")");
			});
			bigPageView.pageScrollStartHandler += new BigPageView.PageScrollStartHandler (delegate(GameObject sender, BigPageView.BigPageViewEventArgs args) {
				Debug.Log(" @ BigPageViewTest.PageScrollStartHandler(" + args.PrevPageIndex + " -> " + args.NextPageIndex + ")");
			});
			bigPageView.pageScrollStopHandler += new BigPageView.PageScrollStopHandler (delegate(GameObject sender, BigPageView.BigPageViewEventArgs args) {
				Debug.Log(" @ BigPageViewTest.PageScrollStopHandler(" + args.PrevPageIndex + " -> " + args.NextPageIndex + ")");
			});

			bigPageView.bigPageViewDelegate = this;

		}

		private GameObject _createPageContent() {
			GameObject content = new GameObject ();
			RectTransform contentRT = content.AddComponent<RectTransform> ();
			return content;
		}

		// Update is called once per frame
		void Update () {

		}

		public int GetPages() {
			return this._pageNum;
		}

		public void GetPage(GameObject pageContainer, int pageIndex) {
//			Debug.Log (" @ BigPageViewTest.getPage(" + pageIndex + ")");
			Transform pageContentTransform = pageContainer.transform.Find ("PageContent");
			if (!pageContentTransform) {
				GameObject pageContent = new GameObject ();
				pageContent.name = "PageContent";
				RectTransform contentRT = pageContent.AddComponent<RectTransform> ();
				Text contentText = pageContent.AddComponent<Text> ();

				contentRT.SetParent (pageContainer.transform);

				contentRT.anchorMin = Vector2.zero;
				contentRT.anchorMax = Vector2.one;

				contentRT.offsetMin = Vector2.zero;
				contentRT.offsetMax = Vector2.zero;

				contentText.alignment = TextAnchor.MiddleCenter;
				contentText.fontSize = 30;
				contentText.color = Color.red;
				contentText.text = "PageIndex: " + pageIndex;
				contentText.font = this.font;

			} else {
				pageContentTransform.GetComponent<Text> ().text = "PageIndex: " + pageIndex;
			}
		}

		public void GotoPageIndex() {
			this.GetComponent<BigPageView> ().JumpToPage (System.Int32.Parse(this.gotoPageIndexInputField.text));
		}

		public void UpdatePageNum() {
			this._pageNum = System.Int32.Parse (this.updatePageNumInputField.text);
			this.GetComponent<BigPageView> ().UpdatePages ();
		}
	}
}


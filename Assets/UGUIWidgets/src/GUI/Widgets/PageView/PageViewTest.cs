using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Assets.src.GUI.PageView {
	public class PageViewTest : MonoBehaviour {
		public InputField pageIndexInputField;

		public Font font;

		// Use this for initialization
		void Start () {
			PageView pageView = this.GetComponent<PageView> ();
			if (pageView) {
				int pages = 10;

				for(int pageIndex = 0; pageIndex < pages; pageIndex++) {
					GameObject pageContent = new GameObject ();
					pageContent.name = "PageContent";
					RectTransform contentRT = pageContent.AddComponent<RectTransform> ();
					Text contentText = pageContent.AddComponent<Text> ();

					pageView.addPage (pageContent);

					contentRT.anchorMin = Vector2.zero;
					contentRT.anchorMax = Vector2.one;

					contentRT.offsetMin = Vector2.zero;
					contentRT.offsetMax = Vector2.zero;

					contentText.alignment = TextAnchor.MiddleCenter;
					contentText.fontSize = 30;
					contentText.color = Color.red;
					contentText.text = "PageIndex: " + pageIndex;
					contentText.font = this.font;

				}
			}

			pageView.pageIndexChangeHandler += new PageView.PageIndexChangeHandler (delegate(GameObject sender, PageView.PageViewEventArgs args) {
				Debug.Log(" @ PageViewText.PageIndexChangeHandler(" + args.PrevPageIndex + " -> " + args.NextPageIndex + ")");
			});
			pageView.pageScrollStartHandler += new PageView.PageScrollStartHandler (delegate(GameObject sender, PageView.PageViewEventArgs args) {
				Debug.Log(" @ PageViewText.PageScrollStartHandler(" + args.PrevPageIndex + " -> " + args.NextPageIndex + ")");
			});
			pageView.pageScrollStopHandler += new PageView.PageScrollStopHandler (delegate(GameObject sender, PageView.PageViewEventArgs args) {
				Debug.Log(" @ PageViewText.PageScrollStopHandler(" + args.PrevPageIndex + " -> " + args.NextPageIndex + ")");
			});

		}

		private GameObject _createPageContent() {
			GameObject content = new GameObject ();
			RectTransform contentRT = content.AddComponent<RectTransform> ();
			return content;
		}

		// Update is called once per frame
		void Update () {

		}

		public void ScrollToPage() {
			this.GetComponent<PageView> () .ScrollToPage(System.Int32.Parse(this.pageIndexInputField.text));
		}

		public void JumpToPage() {
			this.GetComponent<PageView> () .JumpToPage(System.Int32.Parse(this.pageIndexInputField.text));
		}

	}
}


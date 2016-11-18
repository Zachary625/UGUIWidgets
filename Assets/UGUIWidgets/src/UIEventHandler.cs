using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class UIEventHandler : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	void Update () {
		
	}

	public void onCreateButtonClicked(GameObject srcObj) {
		GameObject scrollViewObj = GameObject.Find ("Scroll View");
		GameObject contentObj = scrollViewObj.transform.Find("Viewport").transform.Find("Content").gameObject;

		GameObject panelObj = new GameObject ();

		GameObject inputFieldObj = GameObject.Find ("InputField");
		InputField inputField = inputFieldObj.GetComponent<InputField> ();
		Text inputText = inputFieldObj.transform.Find ("Text").gameObject.GetComponent<Text>();

		Debug.Log (" @ UIEventHandler.onCreateButtonClicked(): inputField.text.Length: " + inputField.text.Length);

		if (inputField.text == null || inputField.text.Length == 0) {
			return;
		}
		Text text = panelObj.AddComponent<Text> ();
		text.horizontalOverflow = HorizontalWrapMode.Wrap;
		text.verticalOverflow = VerticalWrapMode.Truncate;
		text.color = Color.black;
		text.text = inputField.text;
		text.font = inputText.font;
		text.fontSize = inputText.fontSize;

		panelObj.transform.SetParent (contentObj.transform);

		inputField.text = "";
	}

	public void onClearButtonClicked(GameObject srcObj) {
		Debug.Log (" @ UIEventHandler.onCreateButtonClicked()");
		GameObject scrollViewObj = GameObject.Find ("Scroll View");
		GameObject contentObj = scrollViewObj.transform.Find("Viewport").transform.Find("Content").gameObject;

		contentObj.transform.DetachChildren ();
	}
}

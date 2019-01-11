using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TranslateWholeScene : MonoBehaviour
{
	private void Start () 
	{
		var allText = FindObjectsOfType<Text> ();

		foreach (Text t in allText)
		{
			if (t.text [0] != '%')
				continue;

			t.text = DataHelper.instance.GetStringText (t.text);
		}
	}
}

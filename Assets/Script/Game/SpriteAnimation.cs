using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SpriteAnimation : MonoBehaviour 
{
	public Sprite[] sprites;
	public int spriteFrameDelta = 5;

	private int frameCount = 0;
	private int amnSprites = 0;
	private int currentSprite = 0;
	private Image img;

	private void Start()
	{
		img = GetComponent<Image> ();
		amnSprites = sprites.Length;
	}

	private void Update()
	{
		frameCount++;
		if (frameCount < spriteFrameDelta)
			return;

		frameCount = 0;
		currentSprite++;
		if (currentSprite >= amnSprites)
			currentSprite = 0;
		
		img.sprite = sprites [currentSprite];
	}
}

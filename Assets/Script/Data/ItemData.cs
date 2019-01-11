using UnityEngine;
using System.Collections;

public class ItemData
{
	public string ItemName{set;get;}
	public string Description{set;get;}
	public int GoldCost{set;get;}

	public ItemData(string itemString)
	{
		var data = itemString.Split (',');

		ItemName = data [0];
		Description = data [1];
		GoldCost = int.Parse (data [2]);
	}

	public override string ToString ()
	{
		return string.Format ("[ItemData: ItemName={0}, Description={1}, GoldCost={2}]", ItemName, Description, GoldCost);
	}
}
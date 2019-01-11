using UnityEngine;
using System.Collections;

public class LevelData
{
	// SaveState
	public bool Unlocked{set;get;}
	//

	public string LevelName{set;get;}
	public int Id{ set; get;}
	public int SizeX{ set; get;}
	public int SizeY{ set; get;}
	public bool[,] Data{ set; get;}

    public LevelData(){}
	public LevelData(string levelString)
	{
		var data = levelString.Split (',');

		LevelName = data [0];
		Id = int.Parse (data [1]);
		SizeX = int.Parse (data [2]);
		SizeY = int.Parse (data [3]);

		Data = new bool[SizeX, SizeY];
		int x, y;
		x = y = 0;
		foreach(char c in data[4])
		{
			if (c == ' ' || (int)c == 13)
				continue;

			if (x >= SizeX) 
			{
				x = 0;
				y++;
			}
				
			Data[x,y] = (c == '1') ? true : false;
			x++;
		}
	}

    public LevelData Clone()
    {
		LevelData ld = new LevelData(){LevelName = this.LevelName,Id = this.Id,SizeX = this.SizeX,SizeY = this.SizeY};
		ld.Data = new bool[SizeX,SizeY];
		for(int i = 0; i < SizeX; i++)
			for(int j = 0; j < SizeY; j++)
				ld.Data[i,j] = this.Data[i,j];
        return ld; 
    }

	public override string ToString ()
	{
		return string.Format ("[LevelData: LevelName={0}, Id={1}, SizeX={2}, SizeY={3}, Data={4}]", LevelName, Id, SizeX, SizeY, Data);
	}
}
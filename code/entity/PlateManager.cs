using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Pl8Mayhem.entity;

public class PlateManager : Entity
{
	public IEnumerable<PlateEntity> Plates()
	{
		return Entity.All.OfType<PlateEntity>();
	}

	public void CreateBoard()
	{
		for(var i=-4; i<4; i++){
			for(var j=-4; j<4; j++)
			{
				new PlateEntity(new Vector3((i+0.5f)*92*4,(j+0.5f)*92*4,0), 1, "Nobody");
			}
		}
	}

	public void ClearBoard()
	{
		foreach (var plateEntity in Plates())
		{
			plateEntity.Delete();
		}
	}
}

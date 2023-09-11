using System;
using Sandbox;

namespace PlatesGame;

public partial class MeshEntity : ModelEntity
{
	[Net] public string ModelString { get; set; }
	[Net] public string Material { get; set; } = "materials/plate.vmat";

	private Model VertexModel
	{
		get
		{
			return string.IsNullOrEmpty(ModelString) ? null : VertexMeshBuilder.Models[ModelString];
		}
	}

	[Net, Predicted] public Vector3 scale {get;set;} = new Vector3(1f, 1f, 0.01f);
	[Net, Predicted] public Vector3 ToScale {get;set;} = new Vector3(1f, 1f, 0.01f);
	public PhysicsMotionType motionType = PhysicsMotionType.Static;
	private string surface = "normal";
	private string _lastModel;
	private string _lastMaterial;

	public override void Spawn()
	{
		ConstructModel();
	}

	[GameEvent.Tick]
	public virtual void Tick()
	{
		if ( ModelString == null )
		{
			return; // happens before the plate is initialized fully
		}
		if (!VertexMeshBuilder.Models.ContainsKey(ModelString))
		{
			return; // happens after a hot reload :()
		}
		if (ModelString != "" && ModelString != _lastModel)
		{
			Model = VertexModel;
			SetupPhysicsFromModel(motionType);

			_lastModel = ModelString;
			_lastMaterial = "";
		}

		if ( !Game.IsClient || string.IsNullOrEmpty( Material ) || _lastMaterial == Material )
		{
			return;
		}

		SceneObject.SetMaterialOverride(Sandbox.Material.Load(Material));
		_lastMaterial = Material;
	}

	public void SetSurface(string newSurface)
	{
		surface = newSurface;
		ConstructModel();
	}

	public void ConstructModel()
	{
		ModelString = VertexMeshBuilder.GenerateRectangleServer((int)MathX.Floor(scale.x*200), (int)MathX.Floor(scale.y*200), (int)MathX.Floor(scale.z*200), Material, surface);
		Model = VertexModel;
		SetupPhysicsFromModel(motionType);
	}
}

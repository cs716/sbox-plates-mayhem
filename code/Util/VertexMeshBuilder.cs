﻿using System.Collections.Generic;
using System.Runtime.InteropServices;
using Sandbox;

namespace PlatesGame;

public partial class VertexMeshBuilder
{
	private readonly List<MeshVertex> _vertices = new();
	public static readonly Dictionary<string, Model> Models = new();

	private static string CreateRectangleModel(Vector3 size, string material, string surface)
	{
		var key = $"rect_{size.x}_{size.y}_{size.z}_{material}_{surface}";
		if (Models.ContainsKey(key))
		{
			return key;
		}

		var minSize = size * -0.5f;
		var maxSize = size * 0.5f;
		var vertexBuilder = new VertexMeshBuilder();
		vertexBuilder.AddRectangle(Vector3.Zero, size, 64, Color.White);

		var mesh = new Mesh { Material = Material.Load( material ) };

		mesh.CreateVertexBuffer<MeshVertex>(vertexBuilder._vertices.Count, MeshVertex.Layout, vertexBuilder._vertices.ToArray());
		mesh.SetBounds(minSize, maxSize);
		
		var modelBuilder = new ModelBuilder();
		modelBuilder.AddMesh(mesh);
		modelBuilder.WithSurface(surface);
		var box = new BBox(minSize, maxSize);
		modelBuilder.AddCollisionBox(box.Size * 0.5f, box.Center);
		modelBuilder.WithMass(655);

		Models[key] = modelBuilder.Create();
		return key;
	}

	public static MeshEntity SpawnEntity(int length, int width, int height, string material, string surface)
	{
		var vertexModel = GenerateRectangleServer(length, width, height, material, surface);
		MeshEntity entity = new() { ModelString = vertexModel };
		entity.Tick();
		return entity;
	}

	[ClientRpc]
	public static void GenerateRectangleClient(int length, int width, int height, string material, string surface)
	{
		GenerateRectangle(length, width, height, material, surface);
	}
	public static string GenerateRectangleServer(int length, int width, int height, string material, string surface)
	{
		GenerateRectangleClient(length, width, height, material, surface);
		return GenerateRectangle(length, width, height, material, surface);
	}

	private static string GenerateRectangle(int length, int width, int height, string material, string surface)
	{
		return CreateRectangleModel(new Vector3(length, width, height), material, surface);
	}

	private void AddRectangle(Vector3 position, Vector3 size, int texSize, Color color = new Color())
	{
		var rot = Rotation.Identity;

		var f = size.x * rot.Forward * 0.5f;
		var l = size.y * rot.Left * 0.5f;
		var u = size.z * rot.Up * 0.5f;

		CreateQuad(_vertices, new Ray(position + f, f.Normal), l, u, texSize, color);
		CreateQuad(_vertices, new Ray(position - f, -f.Normal), l, -u, texSize, color);

		CreateQuad(_vertices, new Ray(position + l, l.Normal), -f, u, texSize, color);
		CreateQuad(_vertices, new Ray(position - l, -l.Normal), f, u, texSize, color);

		CreateQuad(_vertices, new Ray(position + u, u.Normal), f, l, texSize, color);
		CreateQuad(_vertices, new Ray(position - u, -u.Normal), f, -l, texSize, color);
	}

	private static void CreateQuad(ICollection<MeshVertex> vertices, Ray origin, Vector3 width, Vector3 height, int texSize = 64, Color color = new Color())
	{
		var normal = origin.Forward;
		var tangent = width.Normal;

		MeshVertex a = new(origin.Forward - width - height, normal, tangent, new Vector2(0, 0), color);
		MeshVertex b = new(origin.Forward + width - height, normal, tangent, new Vector2(width.Length / texSize, 0), color);
		MeshVertex c = new(origin.Forward + width + height, normal, tangent, new Vector2(width.Length / texSize, height.Length / texSize), color);
		MeshVertex d = new(origin.Forward - width + height, normal, tangent, new Vector2(0, height.Length / texSize), color);

		vertices.Add(a);
		vertices.Add(b);
		vertices.Add(c);

		vertices.Add(c);
		vertices.Add(d);
		vertices.Add(a);
	}


	[StructLayout(LayoutKind.Sequential)]
	public struct MeshVertex
	{
		public Vector3 Position;
		public Vector3 Normal;
		public Vector3 Tangent;
		public Vector2 TexCoord;
		public Color Color;

		public MeshVertex(Vector3 position, Vector3 normal, Vector3 tangent, Vector2 texCoord, Color color)
		{
			Position = position;
			Normal = normal;
			Tangent = tangent;
			TexCoord = texCoord;
			Color = color;
		}

		public static readonly VertexAttribute[] Layout = {
				new(VertexAttributeType.Position, VertexAttributeFormat.Float32),
				new(VertexAttributeType.Normal, VertexAttributeFormat.Float32),
				new(VertexAttributeType.Tangent, VertexAttributeFormat.Float32),
				new(VertexAttributeType.TexCoord, VertexAttributeFormat.Float32, components: 2),
				new(VertexAttributeType.Color, VertexAttributeFormat.Float32, components: 4)
			};
	}
}

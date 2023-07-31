using System;
using System.Collections.Generic;
using System.Threading;
using Pl8Mayhem.util;
using Sandbox;
using Sandbox.Component;

namespace Pl8Mayhem.entity;

public partial class PlateEntity : MeshEntity
{
	[Net] public IClient PlateOwner {get;set;} = null;
    [Net] public string OwnerName {get;set;}
    [Net] public bool IsDead {get;set;} = false;

    [Net] public List<Entity> PlateEnts {get;set;} = new();
    [Net] private Vector3 TargetPosition {get;set;}
    [Net] private Rotation TargetRotation {get;set;}
    [Net] private RealTimeSince MovementTime {get;set;}
    [Net] private Vector3 MovementSpeed {get;set;}
    [Net] public bool IsFragile {get;set;} = false;
    
    private Glow glow;
    //private PlateNameTag plateTag = null;

    int fadeOutIncrement = 0;

    public PlateEntity(){}

    public PlateEntity(Vector3 pos, float size, string own)
    {
        Tags.Add("plate");
        Position = pos;
        OwnerName = own;
        TargetPosition = Position;
        TargetRotation = Rotation;
        
        var newScale = new Vector3(size, size, 0.01f);
        scale = newScale;
        ToScale = newScale;

        glow = Components.GetOrCreate<Glow>();
        glow.Enabled = false;
        //glow.RangeMin = 0;
        //glow.RangeMax = 2000;
        glow.Color = Color.Blue;

        motionType = PhysicsMotionType.Keyframed;
    }

    public sealed override Vector3 Position
    {
	    get { return base.Position; }
	    set { base.Position = value; }
    }

    public PlateEntity(Vector3 pos, float size, IClient own) : this(pos, size, own.Name)
    {
        PlateOwner = own;
    }

    public override void Spawn(){
        base.Spawn();

        SetupPhysicsFromModel(motionType);
        EnableAllCollisions = true;
        Tags.Add("solid");
        RenderColor = Color.White;
    }

    public override void Tick(){
        base.Tick();

        //if(Game.IsClient && plateTag == null)
        //{
        //    plateTag = new PlateNameTag(this);
		//}

        var lastScale = scale;
        scale = MathC.Lerp(scale,ToScale,0.125f);
        if(scale != lastScale)
        {
            ConstructModel();
        }
    }

    [GameEvent.Tick.Server]
    public void ServerTick()
    {
        if(scale.x <= 0 || scale.y <= 0 || scale.z <= 0)
        {
            Delete();
        }

        if(MovementTime < 0f)
        {
            //if(motionType == PhysicsMotionType.Static) SetMotionType(PhysicsMotionType.Dynamic);
            Position += MovementSpeed;
            Velocity = MovementSpeed;
        }

        if(IsDead)
        {
            if(fadeOutIncrement % 2 == 0 && RenderColor.a > 0f)
            {
                SetAlpha(RenderColor.a - 0.004f);
            }
            fadeOutIncrement++;
        }
    }

    // public override void Simulate(Client cl)
    // {
    //     base.Simulate(cl);

    //     var lastScale = scale;
    //     scale = MathC.Lerp(scale,toScale,0.125f);
    //     if(scale != lastScale)
    //     {
    //         ConstructModel();
    //     }

    //     if(scale.x <= 0 || scale.y <= 0 || scale.z <= 0)
    //     {
    //         Delete();
    //     }
    // }

    public void Kill()
    {
	    if ( IsDead )
		    return;

	    Sound.FromEntity("plates_death", this);
	    SetColor(Color.Red);
	    DeleteAsync(7);
	    IsDead = true;
    }

    protected override void OnDestroy()
    {
        //if(plateTag != null) plateTag.Delete();
        if(Game.IsServer)
        {
            foreach(var ent in PlateEnts)
            {
                ent.Delete();
            }
        }
        base.OnDestroy();
    }

    public void SetMotionType(PhysicsMotionType type)
    {
        motionType = type;
        SetupPhysicsFromModel(motionType);
    }

    public void AddEntity(Entity ent, bool setTransform = false)
    {
        if(setTransform) ent.Parent = this;
        PlateEnts.Add(ent);
    }

    public void SetColor(Color color)
    {
        RenderColor = color;//.WithAlpha(RenderColor.a);
    }

    public void SetAlpha(float alpha)
    {
        RenderColor = RenderColor.WithAlpha(alpha);
    }

    public void SetGlow(bool visible, Color color = default)
    {
        if ( color == default )
            color = glow.Color;

        glow.Enabled = visible;
    }

    public void SetPosition(Vector3 target)
    {
        Position = target;
        TargetPosition = target;
        MovementTime = 0f;
        MovementSpeed = Vector3.Zero;
    }

    new public void MoveTo(Vector3 target, float time = 1f)
    {
        TargetPosition = target;
        if(MovementTime > 0) MovementTime = 0f;
        MovementTime -= time;
        MovementSpeed = (TargetPosition - Position) / (Math.Abs(MovementTime) * 60f);
    }

    public void MoveToLocal(Vector3 localTarget, float time = 1f)
    {
        MoveTo(TargetPosition + localTarget, time);
    }

    public void Rise(float amount, float time = 1f)
    {
        MoveTo(TargetPosition.WithZ(TargetPosition.z + amount), time);
    }

    public void SetSize(float _size)
    {
        ToScale = new Vector3(_size).WithZ(ToScale.z);
    }

    public float GetSize()
    {
        return ToScale.x;
    }

    public void Grow(float _amount)
    {
        ToScale = (ToScale + _amount).WithZ(ToScale.z);
    }

    public void Shrink(float _amount)
    {
        ToScale = (ToScale - _amount).WithZ(ToScale.z);
    }

    public void SetHeight(float _height)
    {
        ToScale = ToScale.WithZ(_height);
    }

    public void AddHeight(float _amount)
    {
        ToScale = ToScale.WithZ(ToScale.z + _amount);
    }

    public float GetHeight()
    {
        return ToScale.z;
    }

    public override void StartTouch(Entity other)
    {
        base.StartTouch(other);

        if ( !IsFragile || (!(other.Velocity.Length > 80) && Random.Shared.Int( 99999 ) != 1) )
	        return;

        Sound.FromWorld("plates_glass_break", Position);
        Delete();
    }
}

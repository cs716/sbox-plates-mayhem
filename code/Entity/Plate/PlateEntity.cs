using System;
using System.Collections.Generic;
using Sandbox;
using Sandbox.Component;

namespace PlatesGame;

public sealed partial class PlateEntity : MeshEntity
{
	[Net] public IClient PlateOwner {get;set;} = null;
    [Net] public string OwnerName {get;set;}
    [Net] public bool IsDead {get;set;}
    
    [Net] public bool WasImpacted { get; set; }

    [Net] public IList<Entity> PlateEnts {get; set;}
    [Net] private Vector3 TargetPosition {get;set;}
    [Net] private Rotation TargetRotation {get;set;}
    [Net] private RealTimeSince MovementTime {get;set;}
    [Net] private Vector3 MovementSpeed {get;set;}
    [Net] public bool IsFragile {get;set;} = false;

    private Glow Glow { get; set; }

    //private PlateNameTag plateTag = null;
    
    public PlateEntity() { }
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
        
        motionType = PhysicsMotionType.Keyframed;

        Glow = Components.GetOrCreate<Glow>();
        Glow.Enabled = false;
        Glow.ObscuredColor = Color.Cyan;
        Glow.InsideColor = Color.Cyan;
        Glow.Color = Color.Blue;
        
    }

    public override void ClientSpawn()
    {
	    base.ClientSpawn();
	    _ = new UI.PlateTag( this );
    }

    public override void Spawn(){
        base.Spawn();

        SetupPhysicsFromModel(motionType);
        EnableAllCollisions = true;
        Tags.Add("solid");
        RenderColor = Color.White;
        LifeState = LifeState.Alive;
    }

    private int FadeOutIncrement; 
    public override void Tick(){
        base.Tick();

        var lastScale = scale;
        scale = MathC.Lerp(scale,ToScale,0.125f);
        if(scale != lastScale)
        {
            ConstructModel();
        }

        if ( IsDead && RenderColor != Color.Red )
	        SetColor( Color.Red );
        else if ( RenderColor == Color.White && WasImpacted )
	        SetColor( Color.Yellow );
        else if ( RenderColor == Color.Yellow && !WasImpacted )
	        SetColor( Color.White );

        if ( !Game.IsServer )
        {
	        return;
        }

        if(scale.x <= 0 || scale.y <= 0 || scale.z <= 0)
        {
	        Delete();
        }

        if(MovementTime < 0f)
        {
	        Position += MovementSpeed;
	        Velocity = MovementSpeed;
        }

        if(IsDead)
        {
	        if(FadeOutIncrement % 2 == 0 && RenderColor.a > 0f)
	        {
		        SetAlpha(RenderColor.a - 0.004f);
	        }
	        FadeOutIncrement++;
        }
    }

    public void Kill()
    {
	    if ( IsDead )
		    return;

	    //Sound.FromEntity("plates_death", this);
	    SetColor(Color.Red);
	    DeleteAsync(7);
	    WasImpacted = false;
	    IsDead = true;
    }

    protected override void OnDestroy()
    {
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
        RenderColor = color;
    }

    public void SetAlpha(float alpha)
    {
        RenderColor = RenderColor.WithAlpha(alpha);
    }

    public void SetPosition(Vector3 target)
    {
        Position = target;
        TargetPosition = target;
        MovementTime = 0f;
        MovementSpeed = Vector3.Zero;
    }

    public void MoveTo(Vector3 target, float time = 1f)
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

    public void SetSize(float size)
    {
        ToScale = new Vector3(size).WithZ(ToScale.z);
    }

    public float GetSize()
    {
        return ToScale.x;
    }

    public void Grow(float amount)
    {
        ToScale = (ToScale + amount).WithZ(ToScale.z);
    }

    public void Shrink(float amount)
    {
        ToScale = (ToScale - amount).WithZ(ToScale.z);
    }

    public void SetHeight(float height)
    {
        ToScale = ToScale.WithZ(height);
    }

    public void AddHeight(float amount)
    {
        ToScale = ToScale.WithZ(ToScale.z + amount);
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

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
    [Net] public IList<Entity> PlateEnts {get; set;}
    [Net] private Vector3 TargetPosition {get;set;}
    [Net] private Rotation TargetRotation {get;set;}
    [Net] private RealTimeSince MovementTime {get;set;}
    [Net] private Vector3 MovementSpeed {get;set;}

    public enum PlateModifier
    {
	    Fragile,
	    Poison
    }

    [Net] public IList<PlateModifier> PlateModifiers { get; set; }

    public PlateEntity()
    {
	    motionType = PhysicsMotionType.Keyframed;
    }
    public PlateEntity(Vector3 pos, float size, string own) : this()
    {
        Position = pos;
        OwnerName = own;
        TargetPosition = Position;
        TargetRotation = Rotation;
        
        var newScale = new Vector3(size, size, 0.01f);
        scale = newScale;
        ToScale = newScale;
    }

    public void AddModifier( PlateModifier modifier )
    {
	    if ( !PlateModifiers.Contains( modifier ) )
		    PlateModifiers.Add( modifier );

	    PlateModifiersChanged();
    }

    public void RemoveModifier( PlateModifier modifier )
    {
	    if ( PlateModifiers.Contains( modifier ) )
		    PlateModifiers.Remove( modifier );

	    PlateModifiersChanged();
    }

    private void PlateModifiersChanged( )
    {
	    Log.Info($"{(Game.IsClient ? "CLIENT" : "SERVER")} PlateModifiersChanged - {OwnerName}");
	    if ( PlateModifiers.Contains( PlateModifier.Poison ) )
	    {
		    SetColor( Color.Green );
	    }

	    if ( PlateModifiers.Contains( PlateModifier.Fragile ) )
	    {
		    SetAlpha( 0.5f );
	    }
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
        RenderColor = Color.White;
        EnableTouch = true;
        EnableTouchPersists = true;
		
        Tags.Add("plate", "solid", "trigger");

        if ( Game.IsServer )
	        PlateModifiers = new List<PlateModifier>();
    }

    private int _fadeOutIncrement; 
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
	        if(_fadeOutIncrement % 2 == 0 && RenderColor.a > 0f)
	        {
		        SetAlpha(RenderColor.a - 0.004f);
	        }
	        _fadeOutIncrement++;
        }
    }

    public void Kill()
    {
	    if ( IsDead )
		    return;

	    //Sound.FromEntity("plates_death", this);
	    SetColor(Color.Red);
	    DeleteAsync(7);
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

    private void SetColor(Color color)
    {
        RenderColor = color;
    }

    private void SetAlpha(float alpha)
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

    public override void StartTouch( Entity other )
    {
	    base.StartTouch( other );
	    Log.Info( "StartTouch" );
    }

    public override void EndTouch( Entity other )
    {
	    base.EndTouch( other );
	    Log.Info( "EndTouch" );
    }

    public void PlayerLanded( PlatesPlayer player, Vector3 velocity )
    {
	    Log.Info("Touch Plate with landing velocity " + velocity.z  );
	    
	    if ( Game.IsClient )
		    return; 
	    
	    if ( PlateModifiers.Contains( PlateModifier.Fragile ) )
	    {
		    if ( velocity.z < -390 ) // Default gravity landing speed
		    {
			    Sound.FromWorld( "plate_glass_break", Position );
			    Delete();
			    return;
		    }
	    }

	    if ( PlateModifiers.Contains( PlateModifier.Poison ) && !player.PlayerModifiers.Contains( PlatesPlayer.PlayerModifier.Poisoned  ))
	    {
		    player.PlayerModifiers.Add( PlatesPlayer.PlayerModifier.Poisoned  );
	    }
    }
}

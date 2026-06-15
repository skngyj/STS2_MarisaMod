using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace marisamod.Scenes.Vfx.StarLit;

public partial class StarlitVfx : Node2D
{
    public const string ScenePath = "res://Scenes/Vfx/StarLit/StarlitVfx.tscn";
    #region Child Node and Material
    private Sprite2D? _star;
    public Sprite2D Star => _star ??= GetNode<Sprite2D>("star");
    private ShaderMaterial? _starShader;
    private ShaderMaterial StarShader => _starShader ??= (ShaderMaterial)Star.Material;
    
    private Sprite2D? _ring;
    public Sprite2D Ring => _ring ??= GetNode<Sprite2D>("ring");
    private ShaderMaterial? _ringShader;
    private ShaderMaterial RingShader => _ringShader ??= (ShaderMaterial)Ring.Material;
    
    private GpuParticles2D? _particle;
    public GpuParticles2D Particle => _particle ??= GetNode<GpuParticles2D>("particle");
    #endregion
    
    #region 公开属性
    public Vector2 Velocity = Vector2.Zero;
    public NCreature? PlayerOwner; 
    public Vector2? Target; 
    #endregion
    #region 
    private float _startSpeed;
    private float _orbitPhase = Mathf.Tau;
    private float _orbitSpeed = 1f;
    private float _ellipseRotation = Mathf.Tau;
    private float _ellipseRotationSpeed = 0.2f;
    private float _majorRadius;
    private float _minorRadius;
    private bool _exploding = false;
    private float _explodTime = 0f;
    #endregion
    private void UpdateWandering(float delta)
    {
        if (PlayerOwner == null)
            return;

        Vector2 center = PlayerOwner.VfxSpawnPosition;

        _orbitPhase += _orbitSpeed * delta;
        _ellipseRotation += _ellipseRotationSpeed * delta;

        // 椭圆上的局部点
        Vector2 localTarget = new Vector2(
            Mathf.Cos(_orbitPhase) * _majorRadius,
            Mathf.Sin(_orbitPhase) * _minorRadius
        );

        // 椭圆整体旋转
        Vector2 targetPos = center + localTarget.Rotated(_ellipseRotation);

        Vector2 toTarget = targetPos - Position;
        float dist = toTarget.Length();
        
        float desiredSpeed = Mathf.Lerp(
            0.35f * _startSpeed,
            0.85f * _startSpeed,
            Mathf.Clamp(dist / 80f, 0f, 1f)
        );
        if (toTarget.Length() <= desiredSpeed)
        {
            Velocity = toTarget;
        }
        else
        {
            Vector2 desiredVelocity = toTarget.Normalized() * desiredSpeed;
            float steering = 4.0f;
            Velocity = Velocity.Lerp(desiredVelocity, steering * delta);
        }
        Position += Velocity * delta;
    }
    public void StartExplosion()
    {
        _exploding = true;
    }
    public override void _Process(double delta)
    {
        if (_exploding)
        {
            _explodTime += (float)delta;
            if (_explodTime > 1f) Kill();
        }
        else
        {
            UpdateWandering((float)delta);
        }
    }
    public void Kill()
    {
        if (PlayerOwner != null)
            Instances.Remove(PlayerOwner);
        this.QueueFreeSafely();
    }
    public void ApplySize(int size)
    {
        size = Mathf.Clamp(size, 1, 256);
        var logRatio = Mathf.Log(size) / Mathf.Log(2) / 8.0f;
        var scale = 3f + 3f * logRatio;
        Scale = new Vector2(scale, scale);
        float armFactor = 1f + 0.5f* logRatio;
        StarShader.SetShaderParameter("arm_factor", armFactor);
        RingShader.SetShaderParameter("power", logRatio);
        Particle.AmountRatio = 0.2f + 0.8f* logRatio;
    }

    public void VelocityInit()
    {
        Vector2 containerSize = NCombatRoom.Instance?.CombatVfxContainer.Size ?? new Vector2(1,640);
        float baseScale = containerSize.Y;
        float dampingDistance = 0.15f * baseScale;//减速距离为0.15个屏幕宽度
        
        float speed = 2.0f* dampingDistance / 0.5f;
        _startSpeed = speed;
        //随机化方向
        float dir = -0.4f * Mathf.Pi + 0.8f * Mathf.Pi * GD.Randf();
        Velocity = speed * Vector2.FromAngle(dir);
        
        _majorRadius = baseScale * 0.2f *(float)GD.RandRange(1f, 1.25f);
        _minorRadius = baseScale * 0.1f*(float)GD.RandRange(0.8f, 1f);
        _orbitPhase *= GD.Randf();
        _ellipseRotation *= GD.Randf();
        _orbitSpeed = (float)GD.RandRange(1.4f, 1.8f);
        _ellipseRotationSpeed = (float)GD.RandRange(0.1f, 0.3f);
        if (GD.Randf() < 0.5f)
            _ellipseRotationSpeed *= -1;
    }

    private static readonly Dictionary<NCreature, StarlitVfx> Instances = new ();

    public static StarlitVfx? GetInstance(NCreature? player)
    {
        if (player == null) return null;
        if (Instances.TryGetValue(player, out var instance))
        {
            if (IsInstanceValid(instance))
                return instance;
            else
                Instances.Remove(player);
        }
        
        var newVfx = Create(player);
        
        Instances.Add(player,newVfx);
        return newVfx;
    }
    public static StarlitVfx Create()
    {
        var instance = GD.Load<PackedScene>(ScenePath)
            .Instantiate<StarlitVfx>();
        return instance;
    }
    public static StarlitVfx Create(NCreature player)
    {
        StarlitVfx vfx = Create();
        vfx.Position = player.VfxSpawnPosition;
        vfx.PlayerOwner= player;
        NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(vfx);
        vfx.VelocityInit();
        return vfx;
    }
}
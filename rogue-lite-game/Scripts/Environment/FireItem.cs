using Godot;

public partial class FireItem : StaticBody2D, IExtinguishable, IIgnitable
{
    [Export] public string BurningAnim = "Burning";
    [Export] public string ExtinguishedAnim = "Off";
    [Export] public string IgnitingAnim = "Igniting";

    private AnimatedSprite2D _sprite;
    private bool _isLit = true;
    private bool _hasIgnitingAnim;
    private float _burningFrameHeight;

    public override void _Ready()
    {
        _sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        if (_sprite == null) return;

        _sprite.AnimationFinished += OnAnimationFinished;
        _hasIgnitingAnim = _sprite.SpriteFrames != null && _sprite.SpriteFrames.HasAnimation(IgnitingAnim);

        if (_sprite.SpriteFrames != null && _sprite.SpriteFrames.HasAnimation(BurningAnim))
        {
            var tex = _sprite.SpriteFrames.GetFrameTexture(BurningAnim, 0);
            _burningFrameHeight = tex != null ? tex.GetHeight() : 0f;
            _sprite.Play(BurningAnim);
        }
    }

    public void Extinguish()
    {
        if (!_isLit || _sprite == null) return;
        _isLit = false;
        if (_sprite.SpriteFrames != null && _sprite.SpriteFrames.HasAnimation(ExtinguishedAnim))
        {
            _sprite.Play(ExtinguishedAnim);
            AlignBottomEdge(ExtinguishedAnim);
        }
        else
            _sprite.Stop();
    }

    public void Ignite()
    {
        if (_isLit || _sprite == null) return;
        _isLit = true;
        if (_hasIgnitingAnim)
        {
            _sprite.Play(IgnitingAnim);
            AlignBottomEdge(IgnitingAnim);
        }
        else if (_sprite.SpriteFrames != null && _sprite.SpriteFrames.HasAnimation(BurningAnim))
        {
            _sprite.Play(BurningAnim);
            _sprite.Offset = Vector2.Zero;
        }
    }

    private void OnAnimationFinished()
    {
        if (_sprite.Animation == IgnitingAnim)
        {
            _sprite.Play(BurningAnim);
            _sprite.Offset = Vector2.Zero;
        }
    }

    private void AlignBottomEdge(string animName)
    {
        if (_burningFrameHeight == 0f || _sprite.SpriteFrames == null) return;
        var tex = _sprite.SpriteFrames.GetFrameTexture(animName, 0);
        if (tex == null) return;
        float heightDiff = _burningFrameHeight - tex.GetHeight();
        _sprite.Offset = new Vector2(0, heightDiff / 2f);
    }
}

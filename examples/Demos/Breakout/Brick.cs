using System.Drawing;
using System.Numerics;
using Common;
using Pie;
using Pie.Audio;

namespace Breakout;

public class Brick : Entity
{
    private Texture _texture;
    private Ball _ball;

    private bool _hasHit;

    public int NumHits;

    public Brick(Texture texture, Ball ball)
    {
        _texture = texture;
        _ball = ball;
    }

    public override void Update(double dt, Main main)
    {
        base.Update(dt, main);

        if (CollisionRect.IntersectsWith(_ball.CollisionRect))
        {
            if (_ball.HasHit) return;
            
            _ball.Velocity.Y *= -1;
            _ball.HasHit = true;
            _ball.Score += 10;
            _hasHit = true;
            main.AudioDevice.PlayBuffer(main.Hit, Utils.GetFreeChannel(main.AudioDevice), new PlayProperties(speed: 0.4));
            NumHits--;
            if (NumHits <= 0)
                ShouldDestroy = true;
        }
        else
        {
            _hasHit = false;
        }
    }

    public override void Draw(double dt, SpriteRenderer renderer)
    {
        base.Draw(dt, renderer);

        Color color = NumHits switch
        {
            >= 6 => Color.DarkRed,
            5 => Color.Red,
            4 => Color.Orange,
            3 => Color.Yellow,
            2 => Color.LawnGreen,
            1 => Color.Green
        };
        
        renderer.Draw(_texture, Position, color, 0, new Vector2(Size.Width, Size.Height), Vector2.Zero);
    }
}
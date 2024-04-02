public interface IEasable
{
    public float Ease(float time);
}

public struct Linear : IEasable
{
    readonly float start;
    readonly float delta;
    readonly float easeTime;

    /// <summary>
    /// start����from�܂ł�easeTime�b�ŃC�[�W���O
    /// </summary>
    public Linear(float from, float easeTime, float start = 0)
    {
        this.start = start;
        this.easeTime = easeTime;
        delta = from - start;
    }
    public float Ease(float time) => start + delta * time / easeTime;
}

public struct InQuad : IEasable
{
    readonly float start;
    readonly float a;

    /// <summary>
    /// start����from�܂ł�easeTime�b�ŃC�[�W���O
    /// </summary>
    public InQuad(float from, float easeTime, float start = 0)
    {
        this.start = start;
        a = (from - start) / (easeTime * easeTime);
    }

    public float Ease(float time)
    {
        return a * time * time + start;
    }
}

public struct OutQuad : IEasable
{
    readonly float start;
    readonly float from;
    readonly float easeTime;
    readonly float b;

    /// <summary>
    /// start����from�܂ł�easeTime�b�ŃC�[�W���O
    /// </summary>
    public OutQuad(float from, float easeTime, float start = 0)
    {
        this.start = start;
        this.from = from;
        this.easeTime = easeTime;
        b = easeTime * easeTime;
    }

    public float Ease(float time)
    {
        var a = easeTime - time;
        return (from * (b - a * a) + start * easeTime * a) / b;
    }
}

public struct InOutQuad : IEasable
{
    readonly float start;
    readonly float easeTime;
    readonly float delta;

    /// <summary>
    /// start����from�܂ł�easeTime�b�ŃC�[�W���O
    /// </summary>
    public InOutQuad(float from, float easeTime, float start = 0)
    {
        this.start = start;
        this.easeTime = easeTime;
        delta = from - start;
    }

    public float Ease(float time)
    {
        time /= easeTime;
        if (time / 2f < 1f)
            return delta * time * time + start;

        time--;
        return -delta * (time * (time - 2f) - 1f) + start;
    }
}

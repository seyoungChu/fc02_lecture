
using System.Collections;
using UnityEngine;

/// <summary>
/// 보간 관련 헬퍼 클래스.
/// </summary>
public class Interpolate
{
    public delegate float Function(float start, float distance, float elapsedTime, float duration);

    public enum EaseType
    {
        Linear,
        EaseInQuad,
        EaseOutQuad,
        EaseInOutQuad,
        EaseInCubic,
        EaseOutCubic,
        EaseInOutCubic,
        EaseInQuart,
        EaseOutQuart,
        EaseInOutQuart,
        EaseInQuint,
        EaseOutQuint,
        EaseInOutQuint,
        EaseInSine,
        EaseOutSine,
        EaseInOutSine,
        EaseInExpo,
        EaseOutExpo,
        EaseInOutExpo,
        EaseInCirc,
        EaseOutCirc,
        EaseInOutCirc
    }



    /*
     * float interpolation using given easing method.
     */
    public static float Ease(Function ease,
                         float start, float distance,
                         float elapsedTime, float duration)
    {
        return ease(start, distance, elapsedTime, duration);
    }

    /*
     * Vector2 interpolation using given easing method. Easing is done independently
     * on all three vector axis.
     */
    public static Vector2 Ease(Function ease,
                         Vector2 start, Vector2 distance,
                         float elapsedTime, float duration)
    {
        start.x = ease(start.x, distance.x, elapsedTime, duration);
        start.y = ease(start.y, distance.y, elapsedTime, duration);
        return start;
    }

    /*
     * Vector3 interpolation using given easing method. Easing is done independently
     * on all three vector axis.
     */
    public static Vector3 Ease(Function ease,
                         Vector3 start, Vector3 distance,
                         float elapsedTime, float duration)
    {
        start.x = ease(start.x, distance.x, elapsedTime, duration);
        start.y = ease(start.y, distance.y, elapsedTime, duration);
        start.z = ease(start.z, distance.z, elapsedTime, duration);
        return start;
    }

    /*
     * Quaternion interpolation using given easing method. Easing is done independently
     * on all axis.
     */
    public static Quaternion Ease(Function ease,
                         Quaternion begin, Quaternion end,
                         float elapsedTime, float duration)
    {
        Vector3 start = begin.eulerAngles;
        Vector3 distance = end.eulerAngles - start;
        if (distance.x < -190) distance.x += 360; else if (distance.x >= 190) distance.x -= 360;
        if (distance.y < -190) distance.y += 360; else if (distance.y >= 190) distance.y -= 360;
        if (distance.z < -190) distance.z += 360; else if (distance.z >= 190) distance.z -= 360;
        start.x = ease(start.x, distance.x, elapsedTime, duration);
        start.y = ease(start.y, distance.y, elapsedTime, duration);
        start.z = ease(start.z, distance.z, elapsedTime, duration);
        return Quaternion.Euler(start.x, start.y, start.z);
    }

    /*
     * Returns the static method that implements the given easing type for scalars.
     * Use this method to easily switch between easing interpolation types.
     *
     * All easing methods clamp elapsedTime so that it is always <= duration.
     *
     * var ease = Interpolate.Ease(EaseType.EaseInQuad);
     * i = ease(start, distance, elapsedTime, duration);
     */
    public static Function Ease(EaseType type)
    {
        // Source Flash easing functions:
        // http://gizma.com/easing/
        // http://www.robertpenner.com/easing/easing_demo.html
        //
        // Changed to use more friendly variable names, that follow my Lerp
        // conventions:
        // start = b (start value)
        // distance = c (change in value)
        // elapsedTime = t (current time)
        // duration = d (time duration)

        Function f = null;
        switch (type)
        {
            case EaseType.Linear: f = new Function(Interpolate.Linear); break;
            case EaseType.EaseInQuad: f = new Function(Interpolate.EaseInQuad); break;
            case EaseType.EaseOutQuad: f = new Function(Interpolate.EaseOutQuad); break;
            case EaseType.EaseInOutQuad: f = new Function(Interpolate.EaseInOutQuad); break;
            case EaseType.EaseInCubic: f = new Function(Interpolate.EaseInCubic); break;
            case EaseType.EaseOutCubic: f = new Function(Interpolate.EaseOutCubic); break;
            case EaseType.EaseInOutCubic: f = new Function(Interpolate.EaseInOutCubic); break;
            case EaseType.EaseInQuart: f = new Function(Interpolate.EaseInQuart); break;
            case EaseType.EaseOutQuart: f = new Function(Interpolate.EaseOutQuart); break;
            case EaseType.EaseInOutQuart: f = new Function(Interpolate.EaseInOutQuart); break;
            case EaseType.EaseInQuint: f = new Function(Interpolate.EaseInQuint); break;
            case EaseType.EaseOutQuint: f = new Function(Interpolate.EaseOutQuint); break;
            case EaseType.EaseInOutQuint: f = new Function(Interpolate.EaseInOutQuint); break;
            case EaseType.EaseInSine: f = new Function(Interpolate.EaseInSine); break;
            case EaseType.EaseOutSine: f = new Function(Interpolate.EaseOutSine); break;
            case EaseType.EaseInOutSine: f = new Function(Interpolate.EaseInOutSine); break;
            case EaseType.EaseInExpo: f = new Function(Interpolate.EaseInExpo); break;
            case EaseType.EaseOutExpo: f = new Function(Interpolate.EaseOutExpo); break;
            case EaseType.EaseInOutExpo: f = new Function(Interpolate.EaseInOutExpo); break;
            case EaseType.EaseInCirc: f = new Function(Interpolate.EaseInCirc); break;
            case EaseType.EaseOutCirc: f = new Function(Interpolate.EaseOutCirc); break;
            case EaseType.EaseInOutCirc: f = new Function(Interpolate.EaseInOutCirc); break;
        }
        return f;
    }


    public static Vector3 Identity(Vector3 v)
    {
        return v;
    }

    public static Vector3 TransformDotPosition(Transform t)
    {
        return t.position;
    }

    /*
     * Linear interpolation (same as Mathf.Lerp)
     */
    public static float Linear(float start, float distance,
                           float elapsedTime, float duration)
    {
        // clamp elapsedTime to be <= duration
        if (elapsedTime > duration) { elapsedTime = duration; }
        return distance * (elapsedTime / duration) + start;
    }

    /*
     * quadratic easing in - accelerating from zero velocity
     */
    public static float EaseInQuad(float start, float distance,
                         float elapsedTime, float duration)
    {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
        return distance * elapsedTime * elapsedTime + start;
    }

    /*
     * quadratic easing out - decelerating to zero velocity
     */
    public static float EaseOutQuad(float start, float distance,
                         float elapsedTime, float duration)
    {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
        return -distance * elapsedTime * (elapsedTime - 2) + start;
    }

    /*
     * quadratic easing in/out - acceleration until halfway, then deceleration
     */
    public static float EaseInOutQuad(float start, float distance,
                         float elapsedTime, float duration)
    {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 2.0f : elapsedTime / (duration / 2);
        if (elapsedTime < 1) return distance / 2 * elapsedTime * elapsedTime + start;
        elapsedTime--;
        return -distance / 2 * (elapsedTime * (elapsedTime - 2) - 1) + start;
    }

    /*
     * cubic easing in - accelerating from zero velocity
     */
    public static float EaseInCubic(float start, float distance,
                         float elapsedTime, float duration)
    {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
        return distance * elapsedTime * elapsedTime * elapsedTime + start;
    }

    /*
     * cubic easing out - decelerating to zero velocity
     */
    public static float EaseOutCubic(float start, float distance,
                         float elapsedTime, float duration)
    {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
        elapsedTime--;
        return distance * (elapsedTime * elapsedTime * elapsedTime + 1) + start;
    }

    /*
     * cubic easing in/out - acceleration until halfway, then deceleration
     */
    public static float EaseInOutCubic(float start, float distance,
                         float elapsedTime, float duration)
    {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 2.0f : elapsedTime / (duration / 2);
        if (elapsedTime < 1) return distance / 2 * elapsedTime * elapsedTime * elapsedTime +
                               start;
        elapsedTime -= 2;
        return distance / 2 * (elapsedTime * elapsedTime * elapsedTime + 2) + start;
    }

    /*
     * quartic easing in - accelerating from zero velocity
     */
    public static float EaseInQuart(float start, float distance,
                         float elapsedTime, float duration)
    {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
        return distance * elapsedTime * elapsedTime * elapsedTime * elapsedTime + start;
    }

    /*
     * quartic easing out - decelerating to zero velocity
     */
    public static float EaseOutQuart(float start, float distance,
                         float elapsedTime, float duration)
    {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
        elapsedTime--;
        return -distance * (elapsedTime * elapsedTime * elapsedTime * elapsedTime - 1) +
                               start;
    }

    /*
     * quartic easing in/out - acceleration until halfway, then deceleration
     */
    public static float EaseInOutQuart(float start, float distance,
                         float elapsedTime, float duration)
    {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 2.0f : elapsedTime / (duration / 2);
        if (elapsedTime < 1) return distance / 2 *
                               elapsedTime * elapsedTime * elapsedTime * elapsedTime +
                               start;
        elapsedTime -= 2;
        return -distance / 2 * (elapsedTime * elapsedTime * elapsedTime * elapsedTime - 2) +
                               start;
    }


    /*
     * quintic easing in - accelerating from zero velocity
     */
    public static float EaseInQuint(float start, float distance,
                         float elapsedTime, float duration)
    {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
        return distance * elapsedTime * elapsedTime * elapsedTime * elapsedTime * elapsedTime +
                               start;
    }

    /*
     * quintic easing out - decelerating to zero velocity
     */
    public static float EaseOutQuint(float start, float distance,
                         float elapsedTime, float duration)
    {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
        elapsedTime--;
        return distance * (elapsedTime * elapsedTime * elapsedTime * elapsedTime *
                           elapsedTime + 1) + start;
    }

    /*
     * quintic easing in/out - acceleration until halfway, then deceleration
     */
    public static float EaseInOutQuint(float start, float distance,
                         float elapsedTime, float duration)
    {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 2.0f : elapsedTime / (duration / 2);
        if (elapsedTime < 1) return distance / 2 * elapsedTime * elapsedTime *
                               elapsedTime * elapsedTime * elapsedTime + start;
        elapsedTime -= 2;
        return distance / 2 * (elapsedTime * elapsedTime * elapsedTime * elapsedTime *
                             elapsedTime + 2) + start;
    }

    /*
     * sinusoidal easing in - accelerating from zero velocity
     */
    public static float EaseInSine(float start, float distance,
                         float elapsedTime, float duration)
    {
        // clamp elapsedTime to be <= duration
        if (elapsedTime > duration) { elapsedTime = duration; }
        return -distance * Mathf.Cos(elapsedTime / duration * (Mathf.PI / 2)) +
                               distance + start;
    }

    /*
     * sinusoidal easing out - decelerating to zero velocity
     */
    public static float EaseOutSine(float start, float distance,
                         float elapsedTime, float duration)
    {
        // clamp elapsedTime to be <= duration
        if (elapsedTime > duration) { elapsedTime = duration; }
        return distance * Mathf.Sin(elapsedTime / duration * (Mathf.PI / 2)) + start;
    }

    /*
     * sinusoidal easing in/out - accelerating until halfway, then decelerating
     */
    public static float EaseInOutSine(float start, float distance,
                         float elapsedTime, float duration)
    {
        // clamp elapsedTime to be <= duration
        if (elapsedTime > duration) { elapsedTime = duration; }
        return -distance / 2 * (Mathf.Cos(Mathf.PI * elapsedTime / duration) - 1) + start;
    }

    /*
     * exponential easing in - accelerating from zero velocity
     */
    public static float EaseInExpo(float start, float distance,
                         float elapsedTime, float duration)
    {
        // clamp elapsedTime to be <= duration
        if (elapsedTime > duration) { elapsedTime = duration; }
        return distance * Mathf.Pow(2, 10 * (elapsedTime / duration - 1)) + start;
    }

    /*
     * exponential easing out - decelerating to zero velocity
     */
    public static float EaseOutExpo(float start, float distance,
                         float elapsedTime, float duration)
    {
        // clamp elapsedTime to be <= duration
        if (elapsedTime > duration) { elapsedTime = duration; }
        return distance * (-Mathf.Pow(2, -10 * elapsedTime / duration) + 1) + start;
    }

    /*
     * exponential easing in/out - accelerating until halfway, then decelerating
     */
    public static float EaseInOutExpo(float start, float distance,
                         float elapsedTime, float duration)
    {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 2.0f : elapsedTime / (duration / 2);
        if (elapsedTime < 1) return distance / 2 *
                               Mathf.Pow(2, 10 * (elapsedTime - 1)) + start;
        elapsedTime--;
        return distance / 2 * (-Mathf.Pow(2, -10 * elapsedTime) + 2) + start;
    }

    /*
     * circular easing in - accelerating from zero velocity
     */
    public static float EaseInCirc(float start, float distance,
                         float elapsedTime, float duration)
    {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
        return -distance * (Mathf.Sqrt(1 - elapsedTime * elapsedTime) - 1) + start;
    }

    /*
     * circular easing out - decelerating to zero velocity
     */
    public static float EaseOutCirc(float start, float distance,
                         float elapsedTime, float duration)
    {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 1.0f : elapsedTime / duration;
        elapsedTime--;
        return distance * Mathf.Sqrt(1 - elapsedTime * elapsedTime) + start;
    }

    /*
     * circular easing in/out - acceleration until halfway, then deceleration
     */
    public static float EaseInOutCirc(float start, float distance,
                         float elapsedTime, float duration)
    {
        // clamp elapsedTime so that it cannot be greater than duration
        elapsedTime = (elapsedTime > duration) ? 2.0f : elapsedTime / (duration / 2);
        if (elapsedTime < 1) return -distance / 2 *
                               (Mathf.Sqrt(1 - elapsedTime * elapsedTime) - 1) + start;
        elapsedTime -= 2;
        return distance / 2 * (Mathf.Sqrt(1 - elapsedTime * elapsedTime) + 1) + start;
    }
}
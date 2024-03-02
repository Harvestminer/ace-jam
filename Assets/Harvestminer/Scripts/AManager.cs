using UnityEngine;

public abstract class AManager<T> : MonoBehaviour where T : AManager<T>
{
    public static T instance;

    public void Awake()
    {
        if (instance == null)
            instance = (T)this;
        else
            Destroy(this);
    }
}

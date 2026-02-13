using System;
using UnityEngine;

public abstract class BaseBehavior : MonoBehaviour, IComparable<BaseBehavior>
{

    //Entre 0f y 5f
    public float weightFactor = 0f;
    [SerializeField] float priority = 0f;


    public abstract Vector3 DesiredDirectionNormalized();

    public int CompareTo(BaseBehavior other)
    {
        return other.priority == priority ? 0 : (int)Math.Sign(other.priority - priority);
    }

    public void setWeightFactor(float weightFactor)
    {
        this.weightFactor = weightFactor;
    }   
    public float GetWeightFactor()
    {
        return weightFactor;
    }
}

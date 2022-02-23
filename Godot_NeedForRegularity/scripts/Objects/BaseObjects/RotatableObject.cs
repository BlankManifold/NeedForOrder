using Godot;
using System;

namespace GameObjects{
public class RotatableObject : BaseObject, IRotatable
{
    
    public override void _Ready()
    {
        base._Ready();
    }

    public virtual void RotateObject(float _){}


}
}

using System;

public class BooleanType : MessagePackObject
{
    private readonly bool value;

    public BooleanType(bool value)
    {
        this.value = value;
    }

    public bool IsBooleanType()
    {
        return true;
    }

    public bool AsBoolean()
    {
        return value;
    }

    public override void MessagePack(Packer packer)
    {
        packer.PackBool(value);
    }

    public bool equals(Object obj)
    {
        return obj.GetType() == GetType() && ((BooleanType) obj).value == value;
    }

    public override int GetHashCode()
    {
        return value ? 1231 : 1237;
    }

    public override Object Clone()
    {
        return new BooleanType(value);
    }
}
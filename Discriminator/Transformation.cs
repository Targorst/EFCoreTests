using System.Text.Json.Serialization;

namespace Discriminator;


public abstract class TransformationBase
{
    private ITransformationProperties Properties { get; }
    
    public TransformationType TransformationType { get; init; }

    public string Name { get; init; }
}

public class Transformation<T> : TransformationBase where T : class, ITransformationProperties 
{
    public Transformation(T properties, string name)
    {
        Properties = properties;
        Name = name;
    }
    
    public T Properties { get; init; }
}

public enum TransformationType
{
    Divide,
    Multiply
}


[JsonPolymorphic(TypeDiscriminatorPropertyName = "TransformationType")]
[JsonDerivedType(typeof(DivisionTransformationProperties), typeDiscriminator: "Divide")]
[JsonDerivedType(typeof(MultiplyTransformationProperties), typeDiscriminator: "Multiply")]
public interface ITransformationProperties
{
    public TransformationType TransformationType { get; }
}

public class DivisionTransformationProperties : ITransformationProperties
{
    public TransformationType TransformationType => TransformationType.Divide;

    public int DivideBy { get; init; }

    public int NumberOne { get; init; }
    
    public int NumberTwo { get; init; }
}

public class MultiplyTransformationProperties : ITransformationProperties
{
    public TransformationType TransformationType => TransformationType.Multiply;
    
    public int MultiplyOn { get; init; }

    public int FirstNumber { get; init; }
    
    public int SecondNumber { get; init; }
}
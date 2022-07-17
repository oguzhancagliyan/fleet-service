using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;
using Moq;
namespace Fleet.UnitTest.AutoFixture;

public class StrictAutoMoqCustomization : ICustomization
{
    public StrictAutoMoqCustomization() : this(new MockRelay())
    {
    }

    public StrictAutoMoqCustomization(ISpecimenBuilder relay)
    {
        Relay = relay;
    }

    public ISpecimenBuilder Relay { get; }

    public void Customize(IFixture fixture)
    {
        fixture.Customizations.Add(new MockPostprocessor(new MethodInvoker(new StrictMockConstructorQuery())));
        fixture.ResidueCollectors.Add(Relay);
    }
}

public class StrictMockConstructorMethod : IMethod
{
    private readonly ConstructorInfo _ctor;
    private readonly ParameterInfo[] _paramInfos;

    public StrictMockConstructorMethod(ConstructorInfo ctor, ParameterInfo[] paramInfos)
    {
        // TODO Null check params
        this._ctor = ctor;
        this._paramInfos = paramInfos;
    }

    public IEnumerable<ParameterInfo> Parameters => _paramInfos;

    public object Invoke(IEnumerable<object> parameters) => _ctor.Invoke(parameters?.ToArray() ?? new object[] { });
}

public class StrictMockConstructorQuery : IMethodQuery
{
    public IEnumerable<IMethod> SelectMethods(Type type)
    {
        if (!IsMock(type))
        {
            return Enumerable.Empty<IMethod>();
        }

        if (!GetMockedType(type).IsInterface && !IsDelegate(type))
        {
            return Enumerable.Empty<IMethod>();
        }

        ConstructorInfo ctor = type.GetConstructor(new[] { typeof(MockBehavior) });

        return new IMethod[] { new StrictMockConstructorMethod(ctor, ctor.GetParameters()) };
    }

    private static bool IsMock(Type type)
    {
        return type is { IsGenericType: true } && typeof(Mock<>).IsAssignableFrom(type.GetGenericTypeDefinition()) && !GetMockedType(type).IsGenericParameter;
    }

    private static Type GetMockedType(Type type)
    {
        return type.GetGenericArguments().Single();
    }

    internal static bool IsDelegate(Type type)
    {
        return typeof(MulticastDelegate).IsAssignableFrom(type.BaseType);
    }
}
using Xunit;

namespace equality_demo.Tests;

// 与 demo 的 8 个小节一一对应
public class EqualityTests
{
    // ============ 1. 值类型 ============
    [Fact]
    public void ValueType_值相等_装箱后ReferenceEquals为False()
    {
        int a = 5, b = 5;
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        // 装箱到 object 局部变量，运行时行为与 demo 中的 ReferenceEquals(a, b) 一致
        object boxedA = a, boxedB = b;
        Assert.False(ReferenceEquals(boxedA, boxedB));
    }

    // ============ 2. 普通类 ============
    [Fact]
    public void PlainClass_三者皆为False_仅引用相等才为True()
    {
        var p1 = new PersonModel { FirstName = "Tim" };
        var p2 = new PersonModel { FirstName = "Tim" };
        Assert.False(p1 == p2);
        Assert.False(p1.Equals(p2));
        Assert.False(ReferenceEquals(p1, p2));
    }

    // ============ 3. 同一引用 ============
    [Fact]
    public void SameReference_三者皆为True()
    {
        var p1 = new PersonModel { FirstName = "Tim" };
        var p3 = p1;
        Assert.True(p1 == p3);
        Assert.True(p1.Equals(p3));
        Assert.True(ReferenceEquals(p1, p3));
    }

    // ============ 4. 字符串字面量（驻留）============
    [Fact]
    public void StringLiteral_被驻留_三者皆为True()
    {
        string a = "Tim", b = "Tim";
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.True(ReferenceEquals(a, b));
    }

    // ============ 5. new string（不驻留）============
    [Fact]
    public void NewString_值相等_但不是同一实例()
    {
        string a = "Tim";
        string b = new string("Tim".ToCharArray());
        Assert.True(a == b);
        Assert.True(a.Equals(b));
        Assert.False(ReferenceEquals(a, b));
    }

    // ============ 6. record ============
    [Fact]
    public void Record_值相等_ReferenceEquals为False()
    {
        var r1 = new PersonRecord("Tim");
        var r2 = new PersonRecord("Tim");
        Assert.True(r1 == r2);
        Assert.True(r1.Equals(r2));
        Assert.False(ReferenceEquals(r1, r2));
    }

    // ============ 7. object 陷阱 ============
    [Fact]
    public void ObjectTypeTrap_的编译期绑定引用比较_Equals仍为值比较()
    {
        string s1 = "Tim";
        string s2 = new string("Tim".ToCharArray());
        object o1 = s1, o2 = s2;
        Assert.False(o1 == o2);     // == 编译期绑定 object 的引用比较
        Assert.True(o1.Equals(o2)); // 虚方法分派到 string.Equals
        Assert.False(ReferenceEquals(o1, o2));
    }

    // ============ 8. Null 处理 ============
    [Fact]
    public void Null_与null比较为True_实例Equals抛NRE()
    {
        PersonModel? pNull = null;
        Assert.True(pNull == null);
        Assert.True(ReferenceEquals(pNull, null));
        Assert.Throws<NullReferenceException>(() => pNull!.Equals(null));
    }
}

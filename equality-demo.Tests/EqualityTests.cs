using EqualityOverride;
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

    // ============ 9. 同一条声明：驻留 vs 不驻留 ============
    [Fact]
    public void SingleDeclaration_字面量驻留_newString不驻留()
    {
        string n1 = "Tim", n2 = new string("Tim".ToCharArray());
        Assert.True(n1 == n2);
        Assert.True(n1.Equals(n2));
        Assert.False(ReferenceEquals(n1, n2));
    }

    // ============ 10. record class vs record struct ============
    [Fact]
    public void RecordClassVsStruct_值语义相同_struct是值类型且装箱引用必False()
    {
        var rc1 = new PersonRecord("Tim");
        var rc2 = new PersonRecord("Tim");
        Assert.True(rc1 == rc2);
        Assert.True(rc1.Equals(rc2));
        Assert.False(ReferenceEquals(rc1, rc2));
        Assert.False(rc1.GetType().IsValueType);

        var rs1 = new PersonRecordStruct("Tim");
        var rs2 = new PersonRecordStruct("Tim");
        Assert.True(rs1 == rs2);
        Assert.True(rs1.Equals(rs2));
        Assert.True(rs1.GetType().IsValueType);

        // struct 装箱后引用必不相等——每次装箱都是新对象
        object b1 = rs1, b2 = rs1;
        Assert.False(ReferenceEquals(b1, b2));

        var rsCopy = rs1 with { };
        Assert.True(rsCopy == rs1);
    }

    // ============ 11. double.NaN ============
    [Fact]
    public void DoubleNaN_的为False_Equals为True()
    {
        double d1 = double.NaN, d2 = double.NaN;
        Assert.False(d1 == d2);     // IEEE 754：NaN 不等于任何值
        Assert.True(d1.Equals(d2)); // IEquatable 破例，保证与 GetHashCode 一致
        Assert.False(ReferenceEquals(d1, d2));
    }

    // ============ 12. 普通 struct ============
    [Fact]
    public void PlainStruct_无运算符_Equals按字段比较()
    {
        var p1 = new PointStruct { X = 1, Y = 2 };
        var p2 = new PointStruct { X = 1, Y = 2 };
        Assert.True(p1.Equals(p2)); // ValueType.Equals 逐字段
        Assert.False(ReferenceEquals(p1, p2)); // 各自装箱
    }

    // ============ 13. dynamic ============
    [Fact]
    public void Dynamic_运算期绑定到string的值比较()
    {
        string s1 = "Tim";
        dynamic d1 = s1, d2 = new string("Tim".ToCharArray());
        Assert.True(d1 == d2);  // 运行期绑定 string ==（object 下是 False）
        Assert.True(d1.Equals(d2));
        Assert.False(ReferenceEquals(d1, d2));
    }

    // ============ 14. int? ============
    [Fact]
    public void NullableInt_提升比较不抛NRE_空值装箱即null引用()
    {
        int? x = null;
        Assert.True(x == null);
        Assert.True(x.Equals(null)); // struct 实例方法，不抛
        Assert.True(ReferenceEquals(x, null)); // 空 Nullable 装箱 → null 引用
    }

    // ============ 15. enum ============
    [Fact]
    public void Enum_值比较_装箱后引用不同()
    {
        ConsoleColor c1 = ConsoleColor.Red, c2 = ConsoleColor.Red;
        Assert.True(c1 == c2);
        Assert.True(c1.Equals(c2));
        Assert.False(ReferenceEquals(c1, c2));
    }

    // ============ 16. 匿名类型 ============
    [Fact]
    public void AnonymousType_没有相等运算符_Equals为值语义()
    {
        var a1 = new { Id = 1, Name = "Tim" };
        var a2 = new { Id = 1, Name = "Tim" };
        Assert.True(a1.Equals(a2));
        Assert.False(ReferenceEquals(a1, a2));
    }

    // ============ 17. ValueTuple vs Tuple ============
    [Fact]
    public void ValueTuple有EqEq_Tuple没有但Equals值比较()
    {
        (int Id, string Name) vt1 = (1, "Tim"), vt2 = (1, "Tim");
        Assert.True(vt1 == vt2); // struct，编译器生成 ==
        Assert.True(vt1.Equals(vt2));

        Tuple<int, string> t1 = Tuple.Create(1, "Tim"), t2 = Tuple.Create(1, "Tim");
        Assert.True(t1.Equals(t2)); // class，但重写 Equals 为值比较
        Assert.False(ReferenceEquals(t1, t2));
    }

    // ============ 18. 委托 ============
    [Fact]
    public void Delegate_引用比较_相同lambda不等_赋值后相等()
    {
        Action a1 = () => { }, a2 = () => { };
        Assert.False(a1 == a2); // 两个 lambda → 不同方法 → 不同委托实例
        Assert.False(a1.Equals(a2));
        Action a3 = a1;
        Assert.True(a1 == a3);  // 同一实例
        Assert.True(ReferenceEquals(a1, a3));
    }

    // ============ 19. 手动重写相等性（EqualityOverride 项目）============
    [Fact]
    public void ManualOverride_值相等_新实例可命中字典()
    {
        var p3 = new Person { FirstName = "Tim", LastName = "Smith" };
        var lookup = new Person { FirstName = "Tim", LastName = "Smith" };
        Assert.True(p3 == lookup);
        Assert.True(p3.Equals(lookup));
        Assert.False(ReferenceEquals(p3, lookup));

        var dict = new Dictionary<Person, int> { [p3] = 3 };
        Assert.Equal(3, dict[lookup]); // GetHashCode + Equals 使新实例命中
    }
}

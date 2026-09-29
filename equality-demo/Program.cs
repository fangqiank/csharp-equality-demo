
// ============ 1. 值类型 ============
Console.WriteLine("=== 1. 值类型 (int) ===");
int count1 = 5, count2 = 5;
Console.WriteLine($"== : {count1 == count2}");
Console.WriteLine($"Equals : {count1.Equals(count2)}");
// 注：下一行会触发 CA2013 警告（值类型装箱传入 ReferenceEquals）——这正是本节要演示的坑
Console.WriteLine($"ReferenceEquals (装箱) : {ReferenceEquals(count1, count2)}");
Console.WriteLine();

// ============ 2. 普通类 ============
Console.WriteLine("=== 2. 普通类 (class) ===");
var p1 = new PersonModel { FirstName = "Tim" };
var p2 = new PersonModel { FirstName = "Tim" };
Console.WriteLine($"== : {p1 == p2}");
Console.WriteLine($"Equals : {p1.Equals(p2)}");
Console.WriteLine($"ReferenceEquals : {ReferenceEquals(p1, p2)}");
Console.WriteLine();

// ============ 3. 同一引用 ============
Console.WriteLine("=== 3. 指向同一引用 ===");
var p3 = p1;
Console.WriteLine($"== : {p1 == p3}");
Console.WriteLine($"Equals : {p1.Equals(p3)}");
Console.WriteLine($"ReferenceEquals : {ReferenceEquals(p1, p3)}");
Console.WriteLine();

// ============ 4. 字符串（驻留）============
Console.WriteLine("=== 4. 字符串字面量（驻留）===");
string name1 = "Tim", name2 = "Tim";
Console.WriteLine($"== : {name1 == name2}");
Console.WriteLine($"Equals : {name1.Equals(name2)}");
Console.WriteLine($"ReferenceEquals : {ReferenceEquals(name1, name2)}");
Console.WriteLine();

// ============ 5. 字符串（不驻留）============
Console.WriteLine("=== 5. 字符串（new string）===");
string name3 = new string("Tim".ToCharArray());
Console.WriteLine($"== : {name1 == name3}");
Console.WriteLine($"Equals : {name1.Equals(name3)}");
Console.WriteLine($"ReferenceEquals : {ReferenceEquals(name1, name3)}");
Console.WriteLine();

// ============ 6. record ============
Console.WriteLine("=== 6. record ===");
var pr1 = new PersonRecord("Tim");
var pr2 = new PersonRecord("Tim");
Console.WriteLine($"== : {pr1 == pr2}");
Console.WriteLine($"Equals : {pr1.Equals(pr2)}");
Console.WriteLine($"ReferenceEquals : {ReferenceEquals(pr1, pr2)}");
Console.WriteLine();

// ============ 7. object 陷阱 ============
Console.WriteLine("=== 7. object 类型的陷阱 ===");
// object 变量上的 == 在编译期绑定到 object 的 ==（引用比较），而不是运行时 string 的值比较
object obj1 = name1, obj2 = name3; // 与第 4/5 节相同的两个字符串，只是声明类型变成了 object
Console.WriteLine($"== (陷阱!) : {obj1 == obj2}");
Console.WriteLine($"Equals : {obj1.Equals(obj2)}");
Console.WriteLine($"ReferenceEquals : {ReferenceEquals(obj1, obj2)}");
Console.WriteLine();

// ============ 8. Null 处理 ============
Console.WriteLine("=== 8. Null 处理 ===");
PersonModel? pNull = null;
Console.WriteLine($"pNull == null : {pNull == null}");
Console.WriteLine($"ReferenceEquals(pNull, null) : {ReferenceEquals(pNull, null)}");
#pragma warning disable CS8602 // 有意解引用 null，演示 NullReferenceException
try
{
    Console.WriteLine($"pNull.Equals(null) : {pNull.Equals(null)}");
}
catch (NullReferenceException)
{
    Console.WriteLine("pNull.Equals(null) : ❌ NullReferenceException!");
}
#pragma warning restore CS8602

// ============ 类型定义（放在顶级语句之后）============
// 类型默认 internal，测试项目通过 InternalsVisibleTo 访问
class PersonModel
{
    public string FirstName { get; set; } = "";
}

record PersonRecord(string FirstName);

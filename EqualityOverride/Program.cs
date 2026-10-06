using EqualityOverride;

// ============ 手动重写 Person 的相等性：== / Equals / ReferenceEquals ============
// IEquatable<T> + Equals + GetHashCode + == 四件套；等价实现一行 record 即可自动生成
// （见 equality-demo 第 5 节 PersonRecord）
var dict = new Dictionary<Person, int>();

var p1 = new Person { FirstName = "Tim", LastName = "Corey" };
var p2 = new Person { FirstName = "Bob", LastName = "Smith" };
var p3 = new Person { FirstName = "Tim", LastName = "Smith" };

dict[p1] = 1;
dict[p2] = 2;
dict[p3] = 3;

var lookup = new Person { FirstName = "Tim", LastName = "Smith" };
Console.WriteLine($"Equals: {p3.Equals(lookup)}");                    // True
Console.WriteLine($"==: {p3 == lookup}");                             // True
Console.WriteLine($"ReferenceEquals: {ReferenceEquals(p3, lookup)}"); // False（不同实例，但值相等）
Console.WriteLine($"字典查找 p1: {dict[p1]}");                         // 1
Console.WriteLine($"字典查找 p2: {dict[p2]}");                         // 2
Console.WriteLine($"字典查找 p3（新实例，值相等命中）: {dict[lookup]}");  // 3

# csharp-equality-demo

C# 相等语义对照演示：`==`、`Equals()`、`ReferenceEquals()` 在不同类型下的行为差异。

## 演示内容

| # | 场景 | `==` | `Equals` | `ReferenceEquals` |
|---|------|------|----------|-------------------|
| 1 | 值类型 `int`（装箱） | True | True | False |
| 2 | 普通类（不同实例） | False | False | False |
| 3 | 同一引用 | True | True | True |
| 4 | 字符串字面量（驻留） | True | True | True |
| 5 | `new string`（不驻留） | True | True | False |
| 6 | `record`（编译器生成值语义） | True | True | False |
| 7 | **object 陷阱**：`==` 编译期绑定到 `object` 的引用比较 | **False** | True | False |
| 8 | null 处理：`pNull.Equals(null)` 抛 `NullReferenceException` | True | 💥 | True |
| 9 | 同一条声明：字面量驻留 vs `new string` 不驻留 | True | True | False |
| 10 | record class vs record struct：值语义相同；struct 是值类型，赋值即复制、装箱后引用必 False | True | True | False（装箱） |
| 11 | `double.NaN`：IEEE 754（`==`）与 IEquatable（`Equals`）打架 | **False** | True | False（装箱） |
| 12 | 普通 struct：默认没有 `==`（编译错误），`Equals` 走 `ValueType` 逐字段比较 | ❌ 编译错误 | True | False（装箱） |
| 13 | `dynamic`：`==` 运行期绑定到 `string`——第 7 节 object 陷阱的对照面 | True | True | False |
| 14 | `int?`：提升运算符永不抛 NRE；空 Nullable 装箱即 null 引用 | True | True（不抛 💥） | True（装箱即 null） |
| 15 | `enum`：按底层值比较 | True | True | False（装箱） |
| 16 | 匿名类型：没有 `==`，`Equals` 是编译器生成的值语义 | ❌ 编译错误 | True | False |
| 17 | `ValueTuple`（struct，有 `==`）vs `Tuple`（class，无 `==` 但 `Equals` 值比较） | True / ❌ | True | False |
| 18 | 委托 `Action`：`==` 比较调用列表，同一实例才 True | False / True | False | True |
| 19 | **手动重写**（`EqualityOverride` 项目）：`IEquatable<T>` + `Equals` + `GetHashCode` + `==` 四件套，不同实例按值命中字典 | True | True | False |

第 7 节是核心陷阱：同样的两个字符串（一个字面量、一个 `new string`），声明为 `string` 时
`==` 是值比较（第 4/5 节），声明为 `object` 后 `==` 在**编译期**绑定到 `object` 的引用比较，
结果变为 False——而 `Equals()` 是虚方法，运行时分派到 `string.Equals`，仍是值比较。

## EqualityOverride：手动重写相等性

`EqualityOverride/` 项目用一个 `Person` 类手写完整的相等性实现，并演示 `Dictionary` 按**值**（而非引用）命中。

### 1. 为什么实现 `IEquatable<Person>`（Person.cs:3）

只重写 `Equals(object)` 的问题：

- 参数是 `object`，每次比较都要类型检查 + 强转（`(Person)obj`），还可能装箱；
- 值类型装箱、引用类型多一次虚调用。

`IEquatable<Person>` 提供强类型 `Equals(Person?)`，`Dictionary<TKey, TValue>`、`List<T>.IndexOf` 等泛型容器通过 `EqualityComparer<T>.Default` 优先走这个接口——所以实现了它，字典查找才能高效工作。

### 2. `Equals(Person?)`（Person.cs:8-11）

```csharp
public bool Equals(Person? other)
    => other is not null
       && FirstName == other.FirstName
       && LastName == other.LastName;
```

- `other is not null`：任何实例和 null 都不相等；短路，null 时直接返回 false，不会碰字段。
- 字段逐一用 `==`：这里字段是 `string`，`string` 的 `==` 本身就是值比较，直接复用即可。

### 3. `Equals(object?)` 为什么必须写（Person.cs:13-14）

```csharp
public override bool Equals(object? obj)
    => obj is Person other && Equals(other);
```

- `obj is Person other` 一行做了三件事：null 检查（null 对任何模式都返回 false）、类型检查、模式匹配强转。
- 必须转发到第 2 点的强类型版本，保证两条路径结果永远一致。
- 不重写它的话，基类 `object.Equals` 是引用比较——`((object)p3).Equals(lookup)` 会返回 False，而 `p3.Equals(lookup)` 返回 True，同一个对象两种调法结果相反，是非预期行为。

### 4. `GetHashCode` 为什么必须重写（Person.cs:16-17）

约定：**`a.Equals(b)` 为 true ⇒ 两者哈希码必须相等**。

`Dictionary` 查找分两步：先 `GetHashCode()` 定位桶，再在桶内用 `Equals` 逐个比较。不重写时用的是 `object.GetHashCode()`（基于引用身份），`p3` 和 `lookup` 是不同实例 → 哈希不同 → 定位到不同桶 → 根本走不到 `Equals` 那一步 → `dict[lookup]` 抛 `KeyNotFoundException`，尽管 `p3.Equals(lookup)` 明明是 True。

`HashCode.Combine(FirstName, LastName)` 就是把两个参与相等比较的字段都喂进去，一行完成。反向（哈希相同但对象不等，即碰撞）是允许的，桶内 `Equals` 会兜底。

### 5. `operator ==` 的顺序（Person.cs:19-24）

```csharp
if (ReferenceEquals(left, right)) return true;
if (left is null || right is null) return false;
return left.Equals(right);
```

- 第 1 行快路径有两个作用：同引用直接 true（省一次字段比较）；更关键的是覆盖 `null == null` —— 没有它，两个 null 会掉进第 2 行返回 false，违反「null == null 应为 true」的直觉约定。
- 第 2 行处理单边 null：必须在调用 `left.Equals` 之前，否则 `null == p` 会在 `left.Equals` 处直接 NRE。
- 此时 `left.Equals(right)` 绑定的是强类型 `Equals(Person?)`，不经过装箱。
- `!=`（Person.cs:26-27）直接 `!(left == right)`：C# 要求 `==` 和 `!=` 必须成对重写，且两者结果必须互斥，取反是最不会写错的实现。

### 6. 删掉某个重写会怎样（结合 Program.cs）

当前输出：`Equals: True`、`==: True`、`ReferenceEquals: False`、`字典查找 p1/p2/p3: 1/2/3`。

| 删掉 | 结果 |
|---|---|
| `GetHashCode` | `dict[lookup]` 抛 `KeyNotFoundException`（见第 4 点）；三个比较输出不变 |
| `operator ==`（和 `!=`） | `==` 变 False（退回引用比较，`p3` 和 `lookup` 是不同实例）；字典不受影响——它不用 `==` |
| 只删 `Equals(object?)` | 程序输出不变（字典走 `IEquatable<T>`，`p3.Equals(lookup)` 绑定强类型版本），但 `object` 类型变量上的 `Equals` 退回引用比较——就是第 3 点说的不一致地雷 |
| 只删 `Equals(Person?)` 并去掉接口 | 字典仍正确（回退到重写过的 `object.Equals`），只是每次比较多一次转型+虚调用，变慢 |

一句话总结：**字典只依赖 `GetHashCode` + `Equals`，`==` 是给使用者的语法糖；四个成员必须互相一致，缺哪个哪个路径就崩。**

顺带：`record Person(string FirstName, string LastName)` 一行就等价于上面全部——这正是 record 存在的意义（见主 demo 第 5/6 节），也是这个对比 demo 想说明的事。

## 运行

```bash
dotnet run --project equality-demo      # 主 demo：18 节
dotnet run --project EqualityOverride   # 第 19 节：手动重写相等性
```

## 测试

`equality-demo.Tests/` 中 19 个单元测试与 demo 各小节一一对应：

```bash
dotnet test
```

环境：.NET 10（`net10.0`）

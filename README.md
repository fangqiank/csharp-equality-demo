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

第 7 节是核心陷阱：同样的两个字符串（一个字面量、一个 `new string`），声明为 `string` 时
`==` 是值比较（第 4/5 节），声明为 `object` 后 `==` 在**编译期**绑定到 `object` 的引用比较，
结果变为 False——而 `Equals()` 是虚方法，运行时分派到 `string.Equals`，仍是值比较。

## 运行

```bash
dotnet run --project equality-demo
```

## 测试

`equality-demo.Tests/` 中 18 个单元测试与 demo 的 18 个小节一一对应：

```bash
dotnet test
```

环境：.NET 10（`net10.0`）

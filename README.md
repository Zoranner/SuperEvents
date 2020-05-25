# SuperEvents

```csharp
// 声明事件
public SuperEvent Value1ChangedEvent { get; } = new SuperEvent();
public SuperEvent<int> Value2ChangedEvent { get; } = new SuperEvent<int>();

// 触发事件
Value1ChangedEvent?.Dispatch();
Value2ChangedEvent?.Dispatch(10);

// 引用事件
Value1ChangedEvent?.AddListenner(Value1Changed);
Value2ChangedEvent?.AddListenner(Value2Changed);

// 接收方法1
private void Value1Changed()
{
    Console.WriteLine("Value1Changed");
}

// 接收方法2
private void Value2Changed(int value2)
{
    Console.WriteLine($"Value2Changed: {value2}");
}
```

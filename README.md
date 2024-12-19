Blazor 计时器组件
============
[![构建状态](https://dev.azure.com/major-soft/GitHub/_apis/build/status/blazor-components/blazor-components-build-check)](https://dev.azure.com/major-soft/GitHub/_build/latest?definitionId=6)
[![包版本](https://img.shields.io/nuget/v/Majorsoft.Blazor.Components.Timer?label=Latest%20Version)](https://www.nuget.org/packages/Majorsoft.Blazor.Components.Timer/)
[![NuGet下载量](https://img.shields.io/nuget/dt/Majorsoft.Blazor.Components.Timer?label=Downloads)](https://www.nuget.org/packages/Majorsoft.Blazor.Components.Timer/)
[![许可证](https://img.shields.io/badge/License-MIT-green.svg)](https://github.com/majorimi/blazor-components/blob/master/LICENSE)

# 关于
基于 Majorsoft.Blazor.Components.Timer，做的升级！

# 介绍
这是一个 Blazor 组件，可用作简单的调度程序或通过调用自定义异步代码来执行周期性重复任务。**所有组件都支持 WebAssembly 和 Server 托管模式**。
代码示例请[参见用法](https://github.com/majorimi/blazor-components/blob/master/src/Majorsoft.Blazor.Components.TestApps.Common/Components/TimerComponent.razor)。

您可以通过[演示应用](https://blazorextensions.z6.web.core.windows.net/timer)来试用它。

![计时器演示](https://github.com/majorimi/blazor-components-docs/raw/main/github/docs/gifs/timer.gif)

# 组件

- **`AdvancedTimer`**: 包装成 Blazor 组件的计时器对象，用于在计时事件触发时执行异步操作。

## `AdvancedTimer` 组件

此组件不会渲染任何 HTML 元素。它被包装成一个组件以便更简单使用。
组件允许您调用 `async` 操作，资源由框架自动释放等。
当您需要定期更新 UI 时很有用，例如每 30 秒通过调用 API 端点刷新仪表板。

**注意：这种技术称为"轮询"。这不是通知客户端最有效的方式。现在您可以使用更现代的技术，如基于"推送"的通信：SignalR 或 WebSocket 等。请确保在没有其他选择的情况下才使用"轮询"。**

### 属性
- **`IntervalInMilisec`: `double { get; set; }` (默认值: 200)** <br />
  通知超时时间(毫秒)。如果设置为 `0 或更小`，将设为 1 毫秒。
- **`DelayInMilisec`: `double { get; set; }` (默认值: 0)** <br />
  计时器启动前的延迟时间(毫秒)。如果设置为 `0`，计时器将立即启动。
- **`AutoStart`: `bool { get; set; }` (默认值: true)** <br />
 如果为 `true`，计时器将在组件 `OnInitialized` 事件运行时启动，否则必须通过将 `IsEnabled` 属性设置为 `true` 来启动计时器。
- **`Occurring`: `Times { get; set; }` (默认值: Times.Once())** <br />
  计时事件将触发的次数。请参见 `Times` 记录说明。
- **`IsEnabled`: `bool { get; }`** <br />
 可以设置为启动 `true` 或停止 `false` 计时器。返回计时器的内部状态。如果计时器正在运行则为 `True`，否则为 `false`。

**可以应用任意 HTML 属性，例如：`id="load1"`，但不会在 HTML DOM 中产生结果**。

### 事件
- **`OnIntervalElapsed`: `EventCallback<ulong>` 委托 - 必需** <br />
  计时器事件，当指定的超时时间到达时调用此函数，参数是迭代计数。

### 函数
- **已过时 (设置 IsEnabled 为 true): `Start()`: `void Start()`** <br />
启动内部计时器，计时器将在设定的延迟后启动，并在给定的发生次数内触发事件。
- **已过时 (设置 IsEnabled 为 false): `Stop()`: `void Stop()`** <br />
停止内部计时器，不再触发事件。
- **`Reset()`: `void Reset()`** <br />
重启内部计时器并将发生计数器重置为 0。事件将在给定的发生次数内触发。
- **`Dispose()`: `实现 IDisposable 接口`** <br />
组件实现 `IDisposable` 接口，当父组件从渲染树中移除时，Blazor 框架将调用它。

### Times 记录
这是一个包装 `ulong` 值的记录对象，用于设置 `AdvancedTimer` 的 `Occurring` 属性。
#### 属性
- **`IntervalInMilisec`: `ulong { get; }` - 必需**
返回设置的值。计时器将使用它来计数已触发的事件。

#### 函数
- **`Once()`: `Times Once()`** <br />
创建值为 `1` 的新 `Times` 实例的工厂方法。
- **`Infinite()`: `Times Infinite()`** <br />
创建值为 `ulong.MaxValue` 的新 `Times` 实例的工厂方法。
- **`Exactly()`: `Times Exactly(ulong count)`** <br />
使用给定参数值创建新 `Times` 实例的工厂方法。

# 配置

## 安装

**Tc.Blazor.Components.Timer** 可在 [NuGet] 上获取。

```sh
dotnet add package Tc.Blazor.Components.Timer
```
使用 `--version` 选项来指定要安装的[预览版本]
## 使用方法

在您的 Blazor `<component/page>.razor` 文件中添加 using 语句。或者在 `_Imports.razor` 文件中全局引用它。

```
@using Tc.Blazor.Components.Timer
```

以下代码示例展示了如何在 Blazor 应用中使用 **`AdvancedTimer`** 组件。具有 2 秒延迟、1 秒间隔、仅发生 10 次并带有重置功能。

```
<span>延迟计数器 (2秒后开始): <strong>@_count</strong></span>
<AdvancedTimer @ref="_counter" IntervalInMilisec="1000" DelayInMilisec="2000" Occurring="Times.Exactly(10)" OnIntervalElapsed="@(c => Counter(c))" />

<br />
<button class="btn btn-sm btn-primary" @onclick="CounterReset">重置</button>

@code {
    //计数器
    private AdvancedTimer _counter;
    private ulong _count = 0;
    private void Counter(ulong count)
    {
        _count = count;
    }
    private void CounterReset() => _counter.Reset();
}
```

以下代码示例展示了如何在 Blazor 应用中使用 **`AdvancedTimer`** 组件。具有无限循环和可在 UI 上设置的间隔，并使用启动/停止功能。

```
<div>
    <input type="range" min="100" max="2000" @bind="clockInterval" /> 时钟间隔: @clockInterval 毫秒
</div>
<span>无限时钟 (手动启动): <strong>@_time</strong></span>
<AdvancedTimer IsEnabled="@_clockEnabled" IntervalInMilisec="@clockInterval" Occurring="Times.Infinite()" AutoStart="false" OnIntervalElapsed="@Clock" />

<br />
<button class="btn btn-sm btn-primary" @onclick="StartStopClock">@_buttonText</button>

@code {
	//Clock
	private double clockInterval = 300;
	private string _time = DateTime.Now.ToString("hh:mm:ss.f");
	private bool _clockEnabled = false;
	private string _buttonText = "Start";
	private void Clock()
	{
		_time = DateTime.Now.ToString("hh:mm:ss.f");
	}
	private void StartStopClock()
	{
		if (_clockEnabled)
		{
			_clockEnabled = false;
			_buttonText = "Start";
		}
		else
		{
			_clockEnabled = true;
			_buttonText = "Stop";
		}
	}
}
```# Tc.Blazor.Components

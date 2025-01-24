using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.Timers;

namespace Tc.Blazor.Components.Timer;

public partial class AdvancedTimer
{
    /// <summary>timer是否已经释放</summary>
    private bool _disposedValue;

    private System.Timers.Timer? _timer;

    /// <summary>timer触发的事件计数</summary>
    private ulong _eventCount = 0;

    /// <summary>Called when the component is initialized. 当组件初始化时调用。</summary>
    protected override void OnInitialized()
    {
        ValidateAndAdjustParameters();

        if (DelayInMilisec > 0)
        {
            _timer = new System.Timers.Timer(DelayInMilisec);
            _timer.Elapsed += OnDelay;
            _timer.AutoReset = false;
        }
        else
        {
            _timer = new System.Timers.Timer(IntervalInMilisec);
            _timer.Elapsed += OnElapsed;
            _timer.AutoReset = Occurring.Count == 1 ? false : true;
        }

        if (AutoStart)
        {
            _timer.Start();
        }

        WriteDiag($"Initialized Timer with Interval: '{IntervalInMilisec}' ms. Delay before start: {DelayInMilisec} ms. Max occurring: {Occurring.Count}, AutoStart: {AutoStart}");
    }

    /// <summary>Notification timeout in ms. If set to 0 or less it set to 1 ms. 通知超时时间（毫秒）。如果设置为0或更小，则设置为1毫秒。</summary>
    [Parameter]
    public double IntervalInMilisec { get; set; } = 200;

    /// <summary>
    /// Delay in ms. before the timer will start. If set to 0 timer will start immediately. 定时器启动前的延迟时间（毫秒）。如果设置为0，定时器将立即启动。
    /// </summary>
    [Parameter]
    public double DelayInMilisec { get; set; } = 0;

    /// <summary>
    /// Number of times elapsed event will be fired. See <see cref="Times"/> record description.
    /// 定时器触发的次数。参见 <see cref="Times"/> 记录描述。
    /// </summary>
    [Parameter]
    public Times Occurring { get; set; } = Times.Once();

    /// <summary>
    /// If true timer will start when component OnInitialized event run, otherwise timer must be
    /// started by IsEnabled property set to true. 如果为true，则在组件OnInitialized事件运行时定时器将启动，否则必须通过将IsEnabled属性设置为true来启动定时器。
    /// </summary>
    [Parameter]
    public bool AutoStart { get; set; } = true;

    /// <summary>
    /// Timer event this Function called when specified timeout elapsed, parameter is the iteration
    /// count. 当指定的超时时间过去时调用此函数的定时器事件，参数是迭代计数。
    /// </summary>
    [Parameter]
    public EventCallback<ulong> OnIntervalElapsed { get; set; }

    /// <summary>
    /// Can be set to start `true` or stop `false` timer. Returns the inner state of the timer.
    /// `True` if timer is running otherwise `false`. 可以设置为启动（true）或停止（false）定时器。返回定时器的内部状态。如果定时器正在运行，则为true，否则为false。
    /// </summary>
    [Parameter]
    public bool IsEnabled { get; set; }

    /// <summary>当前定时器是否正在运行</summary>
    public bool IsRunning => _timer?.Enabled ?? false;

    /// <summary>获取当前的事件计数</summary>
    public ulong CurrentCount => _eventCount;

    /// <summary>
    /// Reset internal timer and reset occurrence counter to 0. Events will be fired for the given
    /// occurrence times. 重置内部计时器并将发生次数计数器重置为0。事件将在给定的发生次数触发。
    /// </summary>
    public void Reset()
    {
        WriteDiag($"Resetting Timer with Interval: '{IntervalInMilisec}' ms. Max occurring: {Occurring.Count}");
        _timer?.Stop();
        _eventCount = 0;
        _timer?.Start();
    }

    /// <summary>Called when the timer delay has elapsed. 当定时器延迟时间到达时调用。</summary>
    /// <param name="source">The event source</param>
    /// <param name="e">The elapsed event arguments</param>
    private void OnDelay(object? source, ElapsedEventArgs e)
    {
        WriteDiag($"Timer required Delay: {DelayInMilisec} ms elapsed. Stopping timer to switch event handler.");

        if (_timer is null)
            return;

        _timer.Stop();
        _timer.Elapsed -= OnDelay;
        _timer.Elapsed += OnElapsed;
        _timer.Interval = IntervalInMilisec;
        _timer.AutoReset = Occurring.Count == 1 ? false : true;
        _timer.Start();

        WriteDiag($"Timer started with Interval: '{IntervalInMilisec}' ms. Max occurring: {Occurring.Count}, AutoReset: {_timer.AutoReset}");
    }

    /// <summary>Called when the timer interval has elapsed. 当定时器间隔时间到达时调用</summary>
    /// <param name="source">The event source</param>
    /// <param name="e">The elapsed event arguments</param>
    private void OnElapsed(object? source, ElapsedEventArgs e)
    {
        if (_disposedValue)
        {
            return;
        }

        if (_eventCount >= Occurring.Count)
        {
            _timer?.Stop();
            WriteDiag($"Timer triggered after: '{IntervalInMilisec}' ms. current occurrence: {_eventCount} reached the required occurrence: '{Occurring.Count}' Timer stopped.");
            return;
        }

        _eventCount++;
        WriteDiag($"Timer triggered after: '{IntervalInMilisec}' ms. current occurrence: '{_eventCount}', required occurrence: '{Occurring.Count}' AutoReset: '{_timer?.AutoReset}'. Invoke '{nameof(OnIntervalElapsed)}' event.");

        InvokeAsync(async () =>
        {
            await OnIntervalElapsed.InvokeAsync(_eventCount);
        });
    }

    /// <summary>Writes diagnostic information to the logger. 将诊断信息写入日志。</summary>
    /// <param name="message">The message to log</param>
    private void WriteDiag(string message)
    {
        _logger.LogDebug($"{DateTime.Now.TimeOfDay} - Component {this.GetType()}: {message}");
    }

    /// <summary>Disposes the component's resources. 释放组件的资源。</summary>
    /// <param name="disposing">True if disposing managed resources</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing && _timer is not null)
            {
                _timer.Stop();
                _timer.Elapsed -= OnElapsed;
                _timer.Elapsed -= OnDelay;
                _timer.Dispose();
                _timer = null;
            }
            _disposedValue = true;
        }
    }

    /// <summary>Dispose component 释放组件资源</summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Called when the component parameters are set. 当组件参数设置时调用。</summary>
    protected override void OnParametersSet()
    {
        ValidateAndAdjustParameters();

        if (_timer is not null)
        {
            if (_timer.Interval != IntervalInMilisec)
            {
                _timer.AutoReset = Occurring.Count == 1 ? false : true;
                _timer.Interval = IntervalInMilisec;
            }

            if (IsEnabled)
            {
                _timer?.Start();
            }
            else
            {
                _timer?.Stop();
            }
        }

        base.OnParametersSet();
    }

    /// <summary>Validates and adjusts the parameters. 验证并调整参数。</summary>
    private void ValidateAndAdjustParameters()
    {
        if (IntervalInMilisec <= 0)
        {
            IntervalInMilisec = 1;
            WriteDiag($"Invalid IntervalInMilisec value, reset to 1ms");
        }

        if (DelayInMilisec < 0)
        {
            DelayInMilisec = 0;
            WriteDiag($"Invalid DelayInMilisec value, reset to 0ms");
        }
    }
}
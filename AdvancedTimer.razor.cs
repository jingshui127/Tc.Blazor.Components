using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.Timers;

namespace Tc.Blazor.Components.Timer;

public partial class AdvancedTimer
{
    /// <summary>timer�Ƿ��Ѿ��ͷ�</summary>
    private bool _disposedValue;

    private System.Timers.Timer? _timer;

    /// <summary>timer�������¼�����</summary>
    private ulong _eventCount = 0;

    /// <summary>Called when the component is initialized. �������ʼ��ʱ���á�</summary>
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

    /// <summary>Notification timeout in ms. If set to 0 or less it set to 1 ms. ֪ͨ��ʱʱ�䣨���룩���������Ϊ0���С��������Ϊ1���롣</summary>
    [Parameter]
    public double IntervalInMilisec { get; set; } = 200;

    /// <summary>
    /// Delay in ms. before the timer will start. If set to 0 timer will start immediately. ��ʱ������ǰ���ӳ�ʱ�䣨���룩���������Ϊ0����ʱ��������������
    /// </summary>
    [Parameter]
    public double DelayInMilisec { get; set; } = 0;

    /// <summary>
    /// Number of times elapsed event will be fired. See <see cref="Times"/> record description.
    /// ��ʱ�������Ĵ������μ� <see cref="Times"/> ��¼������
    /// </summary>
    [Parameter]
    public Times Occurring { get; set; } = Times.Once();

    /// <summary>
    /// If true timer will start when component OnInitialized event run, otherwise timer must be
    /// started by IsEnabled property set to true. ���Ϊtrue���������OnInitialized�¼�����ʱ��ʱ�����������������ͨ����IsEnabled��������Ϊtrue��������ʱ����
    /// </summary>
    [Parameter]
    public bool AutoStart { get; set; } = true;

    /// <summary>
    /// Timer event this Function called when specified timeout elapsed, parameter is the iteration
    /// count. ��ָ���ĳ�ʱʱ���ȥʱ���ô˺����Ķ�ʱ���¼��������ǵ���������
    /// </summary>
    [Parameter]
    public EventCallback<ulong> OnIntervalElapsed { get; set; }

    /// <summary>
    /// Can be set to start `true` or stop `false` timer. Returns the inner state of the timer.
    /// `True` if timer is running otherwise `false`. ��������Ϊ������true����ֹͣ��false����ʱ�������ض�ʱ�����ڲ�״̬�������ʱ���������У���Ϊtrue������Ϊfalse��
    /// </summary>
    [Parameter]
    public bool IsEnabled { get; set; }

    /// <summary>��ǰ��ʱ���Ƿ���������</summary>
    public bool IsRunning => _timer?.Enabled ?? false;

    /// <summary>��ȡ��ǰ���¼�����</summary>
    public ulong CurrentCount => _eventCount;

    /// <summary>
    /// Reset internal timer and reset occurrence counter to 0. Events will be fired for the given
    /// occurrence times. �����ڲ���ʱ������������������������Ϊ0���¼����ڸ����ķ�������������
    /// </summary>
    public void Reset()
    {
        WriteDiag($"Resetting Timer with Interval: '{IntervalInMilisec}' ms. Max occurring: {Occurring.Count}");
        _timer?.Stop();
        _eventCount = 0;
        _timer?.Start();
    }

    /// <summary>Called when the timer delay has elapsed. ����ʱ���ӳ�ʱ�䵽��ʱ���á�</summary>
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

    /// <summary>Called when the timer interval has elapsed. ����ʱ�����ʱ�䵽��ʱ����</summary>
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

    /// <summary>Writes diagnostic information to the logger. �������Ϣд����־��</summary>
    /// <param name="message">The message to log</param>
    private void WriteDiag(string message)
    {
        _logger.LogDebug($"{DateTime.Now.TimeOfDay} - Component {this.GetType()}: {message}");
    }

    /// <summary>Disposes the component's resources. �ͷ��������Դ��</summary>
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

    /// <summary>Dispose component �ͷ������Դ</summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Called when the component parameters are set. �������������ʱ���á�</summary>
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

    /// <summary>Validates and adjusts the parameters. ��֤������������</summary>
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
using System.Windows.Input;

namespace NexsusVKVideo.App.ViewModels;

public sealed class AsyncRelayCommand : ICommand, IDisposable
{
    private readonly Func<CancellationToken, Task> _execute;
    private readonly Action<Exception> _onError;
    private CancellationTokenSource? _cancellationSource;
    private bool _isExecuting;

    public AsyncRelayCommand(Func<CancellationToken, Task> execute, Action<Exception> onError)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _onError = onError ?? throw new ArgumentNullException(nameof(onError));
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => !_isExecuting;

    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter))
        {
            return;
        }

        _isExecuting = true;
        _cancellationSource = new CancellationTokenSource();
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        try
        {
            await _execute(_cancellationSource.Token);
        }
        catch (OperationCanceledException) when (_cancellationSource.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _onError(exception);
        }
        finally
        {
            _cancellationSource.Dispose();
            _cancellationSource = null;
            _isExecuting = false;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Cancel() => _cancellationSource?.Cancel();

    public void Dispose()
    {
        _cancellationSource?.Cancel();
        _cancellationSource?.Dispose();
    }
}

public sealed class AsyncRelayCommand<T> : ICommand, IDisposable
    where T : class
{
    private readonly Func<T, CancellationToken, Task> _execute;
    private readonly Action<Exception> _onError;
    private CancellationTokenSource? _cancellationSource;
    private bool _isExecuting;

    public AsyncRelayCommand(Func<T, CancellationToken, Task> execute, Action<Exception> onError)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _onError = onError ?? throw new ArgumentNullException(nameof(onError));
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => !_isExecuting && parameter is T;

    public async void Execute(object? parameter)
    {
        if (parameter is not T value || !CanExecute(value))
        {
            return;
        }

        _isExecuting = true;
        _cancellationSource = new CancellationTokenSource();
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        try
        {
            await _execute(value, _cancellationSource.Token);
        }
        catch (OperationCanceledException) when (_cancellationSource.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            _onError(exception);
        }
        finally
        {
            _cancellationSource.Dispose();
            _cancellationSource = null;
            _isExecuting = false;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Cancel() => _cancellationSource?.Cancel();

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    public void Dispose()
    {
        _cancellationSource?.Cancel();
        _cancellationSource?.Dispose();
    }
}

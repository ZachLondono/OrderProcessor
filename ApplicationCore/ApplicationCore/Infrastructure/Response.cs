namespace ApplicationCore.Infrastructure;

public class Response<TSuccess> {

    private readonly TSuccess? _value;
    private readonly Error? _error;
    private readonly bool _isSuccess;

    public bool IsError {
        get => !_isSuccess;
    }

    public Response(TSuccess value) {
        _value = value;
        _isSuccess = true;
    }

    public Response(Error error) {
        _error = error;
        _isSuccess = false;
    }

    public void Match(Action<TSuccess> onSuccess, Action<Error> onError) {
        switch (_isSuccess) {
            case true:
                onSuccess(_value!);
                break;
            case false:
                onError(_error!);
                break;
        }
    }

    public async Task MatchAsync(Func<TSuccess, Task> onSuccessAsync, Func<Error, Task> onErrorAsync) {
        switch (_isSuccess) {
            case true:
                await onSuccessAsync(_value!);
                break;
            case false:
                await onErrorAsync(_error!);
                break;
        }
    }

    public void OnSuccess(Action<TSuccess> onSuccess) {
        if (!_isSuccess || _value is null) return;
        onSuccess(_value!);
    }

    public async Task OnSuccessAsync(Func<TSuccess, Task> onSuccessAsync) {
        if (!_isSuccess || _value is null) return;
        await onSuccessAsync(_value!);
    }

    public void OnError(Action<Error> onError) {
        if (_isSuccess || _error is null) return;
        onError(_error!);
    }

    public async Task OnErrorAsync(Func<Error, Task> onErrorAsync) {
        if (_isSuccess || _error is null) return;
        await onErrorAsync(_error!);
    }

}

public class Response : Response<Unit> {
    public Response() : base(new Unit()) { }
    public Response(Error error) : base(error) { }
}
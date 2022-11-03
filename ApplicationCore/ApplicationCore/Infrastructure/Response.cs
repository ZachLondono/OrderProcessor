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

    public void Match(Action<TSuccess> onSucces, Action<Error> onError) {
        switch (_isSuccess) {
            case true:
                onSucces(_value!);
                break;
            case false:
                onError(_error!);
                break;
        }
    }

    public void OnError(Action<Error> onError) {
        if (_isSuccess || _error is null) return;
        onError(_error!);
    }

}

public class Response : Response<Unit> {
    public Response() : base(new Unit()) { }
    public Response(Error error) : base(error) { }
}
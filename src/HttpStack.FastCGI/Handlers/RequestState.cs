namespace HttpStack.FastCGI.Handlers;

public enum RequestState
{
    None,
    Headers,
    RequestBody,
    ResponseBody,
    End
}

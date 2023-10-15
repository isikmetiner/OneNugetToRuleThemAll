using DefaultNuget.Dto;

namespace DefaultNuget.Invoker
{
    public interface IRestInvoker
    {
        Task<ApiResponse> SendAsync<T>(ApiRequest apiRequest) where T : class, new();
    }
}
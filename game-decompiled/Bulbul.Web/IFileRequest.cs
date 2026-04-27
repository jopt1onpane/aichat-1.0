namespace Bulbul.Web;

public interface IFileRequest<TResponse> : IRequest<TResponse>
{
	string FileName { get; }
}

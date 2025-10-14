using NextPapyros.Domain.Entities;

namespace NextPapyros.Infrastructure.Auth;

public interface ITokenService
{
    string CreateToken(Usuario user, IEnumerable<string> roles);
}
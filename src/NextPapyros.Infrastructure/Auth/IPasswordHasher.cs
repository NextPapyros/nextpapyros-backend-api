namespace NextPapyros.Infrastructure.Auth;

public interface IPasswordHasher
{
    string Hash(string plain);
    bool Verify(string plain, string hash);
}
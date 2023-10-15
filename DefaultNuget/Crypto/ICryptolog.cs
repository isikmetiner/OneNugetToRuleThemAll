namespace OneNugetToRuleThemAll.Crypto
{
    public interface ICryptolog
    {
        string Encrypt(string data);
        string Decrypt(string token);
    }
}
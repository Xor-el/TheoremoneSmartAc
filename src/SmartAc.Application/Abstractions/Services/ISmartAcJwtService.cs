namespace SmartAc.Application.Abstractions.Services;

public interface ISmartAcJwtService
{
    (string tokenId, string token) GenerateJwtFor(string targetId, string role);

    string JwtScopeApiKey { get; }

    string JwtScopeDeviceIngestionService { get; }

    string JwtScopeDeviceAdminService { get; }
}
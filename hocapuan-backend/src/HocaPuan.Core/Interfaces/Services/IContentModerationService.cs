using HocaPuan.Core.Moderation;

namespace HocaPuan.Core.Interfaces.Services;

public interface IContentModerationService
{
    ModerationResult Moderate(string text);
}

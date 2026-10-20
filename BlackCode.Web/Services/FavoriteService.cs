using BlackCode.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace BlackCode.Web.Services;

public class FavoriteService(BlackCodeContext context)
{
    private string? cachedUserId;
    private HashSet<int>? cachedIds;

    public async Task<HashSet<int>> GetProductIdsAsync(string userId)
    {
        if (cachedIds is null || cachedUserId != userId)
        {
            cachedUserId = userId;
            cachedIds = (await context.Favorites.AsNoTracking()
                .Where(f => f.UserId == userId)
                .Select(f => f.ProductId).ToListAsync()).ToHashSet();
        }
        return cachedIds;
    }

    public void Forget() => cachedIds = null;
}

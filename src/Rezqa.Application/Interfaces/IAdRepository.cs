using Rezqa.Application.Common.Models;
using Rezqa.Application.Features.Ad.Request.Query;
using Rezqa.Domain.Entities;

namespace Rezqa.Application.Interfaces;

public interface IAdRepository : IRepository<Ad>
{
    Task<PaginatedResult<Ad>> SearchAdsAsync(SearchAdsQuery request, CancellationToken cancellationToken);
    Task<IEnumerable<Ad>> GetAdsByUserIdAsync(Guid userId);
    Task<IEnumerable<Ad>> GetAdsByLocationAsync(string location);
    Task<IEnumerable<Ad>> GetAdsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
    Task<IEnumerable<Ad>> GetAdsWithFieldValuesAsync();
    Task<Ad?> GetAdWithFieldValuesByIdAsync(int id);
}
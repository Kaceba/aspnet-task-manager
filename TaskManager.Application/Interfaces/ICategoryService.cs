using TaskManager.Core.DTOs.Categories;

namespace TaskManager.Application.Interfaces;

public interface ICategoryService
{
    Task<CategoryResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CategoryResponse> CreateAsync(CategoryRequest request, CancellationToken cancellationToken = default);
    Task<CategoryResponse> UpdateAsync(int id, CategoryRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}

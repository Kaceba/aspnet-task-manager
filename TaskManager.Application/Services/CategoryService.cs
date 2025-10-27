using AutoMapper;
using TaskManager.Application.Interfaces;
using TaskManager.Core.DTOs.Categories;
using TaskManager.Core.Entities;
using TaskManager.Core.Interfaces;

namespace TaskManager.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CategoryResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);

        if (category == null || category.IsDeleted)
            throw new KeyNotFoundException($"Category with ID {id} not found");

        return _mapper.Map<CategoryResponse>(category);
    }

    public async Task<IEnumerable<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Categories.GetAllAsync(cancellationToken);
        var activeCategories = categories.Where(c => !c.IsDeleted);
        return _mapper.Map<IEnumerable<CategoryResponse>>(activeCategories);
    }

    public async Task<CategoryResponse> CreateAsync(CategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = _mapper.Map<Category>(request);
        category.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Categories.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CategoryResponse>(category);
    }

    public async Task<CategoryResponse> UpdateAsync(int id, CategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);

        if (category == null || category.IsDeleted)
            throw new KeyNotFoundException($"Category with ID {id} not found");

        category.Name = request.Name;
        category.Description = request.Description;
        category.Color = request.Color;
        category.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CategoryResponse>(category);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id, cancellationToken);

        if (category == null || category.IsDeleted)
            throw new KeyNotFoundException($"Category with ID {id} not found");

        // Soft delete
        category.IsDeleted = true;
        category.DeletedAt = DateTime.UtcNow;

        await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

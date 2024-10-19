using AuthServer.Core.Repositories;
using AuthServer.Core.Services;
using AuthServer.Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Dtos;
using System.Linq.Expressions;

namespace AuthServer.Service.Services;

public class GenericService<TEntity, TDto> : IGenericService<TEntity, TDto> where TEntity : class where TDto : class
{
	private readonly IUnitOfWork unitOfWork;
	private readonly IGenericRepository<TEntity> repository;

    public GenericService(IUnitOfWork unitOfWork, IGenericRepository<TEntity> repository)
    {
        this.unitOfWork = unitOfWork;
		this.repository = repository;
    }

    public async Task<Response<TDto>> AddAsync(TDto dto)
	{
		var newEntity = ObjectMapper.Mapper.Map<TEntity>(dto);
		await repository.AddAsync(newEntity);
		await unitOfWork.CommitAsync();
		var newDto = ObjectMapper.Mapper.Map<TDto>(newEntity);
		return Response<TDto>.Success(newDto, 200);
	}

	public async Task<Response<IEnumerable<TDto>>> GetAllAsync()
	{
		var products = ObjectMapper.Mapper.Map<List<TDto>>(await repository.GetAllAsync());
		return Response<IEnumerable<TDto>>.Success(products, 200);
	}

	public async Task<Response<TDto>> GetByIdAsync(string id)
	{
		var product = await repository.GetByIdAsync(id);
		if(product == null)
		{
			return Response<TDto>.Fail("Id Not Found", 404, true);
		}
		return Response<TDto>.Success(ObjectMapper.Mapper.Map<TDto>(product), 200);
	}

	public async Task<Response<NoDataDto>> RemoveAsync(string id)
	{
		var entity = await repository.GetByIdAsync(id);
		if (entity == null)
		{
			return Response<NoDataDto>.Fail("Id Not Found", 404, true);
		}
		repository.Remove(entity);
		await unitOfWork.CommitAsync();
		return Response<NoDataDto>.Success(204);
	}

	public async Task<Response<NoDataDto>> UpdateAsync(TDto dto, string id)
	{
		var entity = await repository.GetByIdAsync(id);
		if(entity == null)
		{
			return Response<NoDataDto>.Fail("Id Not Found", 404, true);
		}
		var updatedEntity = ObjectMapper.Mapper.Map<TEntity>(dto);
		repository.Update(updatedEntity);
		await unitOfWork.CommitAsync();
		return Response<NoDataDto>.Success(204);
	}

	public async Task<Response<IEnumerable<TDto>>> Where(Expression<Func<TEntity, bool>> predicate)
	{
		var list = repository.Where(predicate);
		return Response<IEnumerable<TDto>>.Success(ObjectMapper.Mapper.Map<IEnumerable<TDto>>(await list.ToListAsync()), 200);
	}
}

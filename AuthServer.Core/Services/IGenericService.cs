using SharedLibrary.Dtos;
using System.Linq.Expressions;

namespace AuthServer.Core.Services;

public interface IGenericService<TEntity, TDto> where TEntity : class where TDto : class
{
	Task<Response<TDto>> GetByIdAsync(string id);
	Task<Response<IEnumerable<TDto>>> GetAllAsync();
	Task<Response<IEnumerable<TDto>>> Where(Expression<Func<TEntity, bool>> predicate);
	Task<Response<TDto>> AddAsync(TDto dto);
	Task<Response<NoDataDto>> RemoveAsync(string id);
	Task<Response<NoDataDto>> UpdateAsync(TDto dto, string id);

}

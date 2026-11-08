using eShop.Shared.Models;
using System.Data;

namespace eShop.OrderService.BusinessLogic.Infrastructure
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        IDbConnection Connection { get; }
        IDbTransaction? Transaction { get; }
        InvokeDataModel Data { get; }
        Task BeginAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}

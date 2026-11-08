using eShop.Shared.Models;
using Microsoft.Data.SqlClient;
using System.Data;
namespace eShop.InventoryService.BusinessLogic.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SqlConnection _connection;
        private SqlTransaction? _transaction;
        private bool _disposed;

        public IDbConnection Connection => _connection;
        public IDbTransaction? Transaction => _transaction;
        public InvokeDataModel Data { get; }

        public UnitOfWork(SqlConnection connection, InvokeDataModel data)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public async Task BeginAsync()
        {
            EnsureNotDisposed();

            if (_connection.State != ConnectionState.Open)
            {
                await _connection.OpenAsync();
            }

            _transaction ??= (SqlTransaction)await _connection.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            EnsureNotDisposed();

            if (_transaction is null)
            {
                throw new InvalidOperationException("No active transaction context found to commit.");
            }

            try
            {
                await _transaction.CommitAsync();
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        public async Task RollbackAsync()
        {
            EnsureNotDisposed();

            if (_transaction is null)
            {
                return;
            }

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        private async Task DisposeTransactionAsync()
        {
            if (_transaction is not null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(UnitOfWork));
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _transaction?.Dispose();
            _transaction = null;

            _connection.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            await DisposeTransactionAsync();

            if (_connection is not null)
            {
                await _connection.DisposeAsync();
            }
        }
    }
}
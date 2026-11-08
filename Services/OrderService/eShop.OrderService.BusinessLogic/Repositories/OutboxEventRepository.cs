using Dapper;
using eShop.OrderService.BusinessLogic.Infrastructure;
using eShop.OrderService.BusinessLogic.Interfaces;
using eShop.OrderService.Models;

namespace eShop.OrderService.BusinessLogic.Repositories
{
    public class OutboxEventRepository : BaseRepository, IOutboxEventRepository
    {
        public OutboxEventRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<OutboxEventModel> Add(OutboxEventModel model)
        {
            model.f_uid = Guid.NewGuid();
            model.f_create_date = DateTime.UtcNow;
            model.f_create_by = _userUid;

            var sql = $@"
                INSERT INTO [{_schema}].[t_outbox_event] 
                (
                    [f_uid], 
                    [f_event_type], 
                    [f_payload], 
                    [f_status], 
                    [f_create_date], 
                    [f_create_by]
                )
                VALUES 
                (
                    @f_uid, 
                    @f_event_type, 
                    @f_payload, 
                    @f_status, 
                    @f_create_date, 
                    @f_create_by
                )";

            await _connection.ExecuteAsync(sql, model, transaction: _transaction, commandTimeout: _commandTimeout);
            return model;
        }

        public async Task<IEnumerable<OutboxEventModel>> GetPending(int batchSize)
        {
            var sql = $@"
                SELECT TOP (@BatchSize)
                    [f_uid],
                    [f_iid],
                    [f_event_type],
                    [f_payload],
                    [f_status],
                    [f_create_date],
                    [f_processed_date],
                    [f_error_log],
                    [f_create_by],
                    [f_update_date],
                    [f_update_by],
                    [f_delete_date],
                    [f_delete_by]
                FROM 
                    [{_schema}].[t_outbox_event] WITH (ROWLOCK, UPDLOCK, READPAST) 
                WHERE 
                    [f_status] = 'Pending' AND 
                    [f_delete_date] IS NULL 
                ORDER BY 
                    [f_create_date] ASC";

            return await _connection.QueryAsync<OutboxEventModel>(sql, new { BatchSize = batchSize }, transaction: _transaction, commandTimeout: _commandTimeout);
        }

        public async Task<bool> UpdateStatus(Guid uid, string status, DateTime? processedDate, string? errorLog)
        {
            var sql = $@"
                UPDATE 
                    [{_schema}].[t_outbox_event]
                SET 
                    [f_status] = @Status,
                    [f_processed_date] = @ProcessedDate,
                    [f_error_log] = @ErrorLog,
                    [f_update_date] = @UpdateDate,
                    [f_update_by] = @UpdateBy
                WHERE 
                    [f_uid] = @Uid AND 
                    [f_delete_date] IS NULL";

            var rows = await _connection.ExecuteAsync(sql, new
            {
                Uid = uid,
                Status = status,
                ProcessedDate = processedDate,
                ErrorLog = errorLog,
                UpdateDate = DateTime.UtcNow,
                UpdateBy = _userUid
            }, transaction: _transaction, commandTimeout: _commandTimeout);

            return rows > 0;
        }
    }
}
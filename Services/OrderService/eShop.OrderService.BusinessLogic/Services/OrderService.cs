using eShop.Contracts.Enums;
using eShop.Contracts.Events;
using eShop.OrderService.BusinessLogic.Infrastructure;
using eShop.OrderService.BusinessLogic.Interfaces;
using eShop.OrderService.Models;
using System.Text.Json;

namespace eShop.OrderService.BusinessLogic.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IOrderHistoryRepository _orderHistoryRepository;
        private readonly IOutboxEventRepository _outboxEventRepository;

        public OrderService(
            IUnitOfWork unitOfWork,
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            IOrderHistoryRepository orderHistoryRepository,
            IOutboxEventRepository outboxEventRepository)
        {
            _unitOfWork = unitOfWork;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _orderHistoryRepository = orderHistoryRepository;
            _outboxEventRepository = outboxEventRepository;
        }

        public Task<IEnumerable<OrderModel>> GetAllAsync()
        {
            return _orderRepository.Get();
        }

        public Task<OrderModel?> GetByUidAsync(Guid uid)
        {
            return _orderRepository.Find(uid);
        }

        public Task<IEnumerable<OrderModel>> GetByCustomerAsync(Guid customerUid)
        {
            return _orderRepository.GetByCustomer(customerUid);
        }

        public Task<IEnumerable<OrderItemModel>> GetItemsByOrderAsync(Guid orderUid)
        {
            return _orderItemRepository.GetByOrder(orderUid);
        }

        public Task<IEnumerable<OrderHistoryModel>> GetHistoryByOrderAsync(Guid orderUid)
        {
            return _orderHistoryRepository.GetByOrder(orderUid);
        }

        public async Task<OrderModel> PlaceOrderAsync(OrderModel order, IEnumerable<OrderItemModel> items)
        {
            var itemList = items.ToList();
            if (itemList.Count == 0)
            {
                throw new InvalidOperationException("An order must contain at least one item.");
            }

            order.f_total_price = itemList.Sum(i => i.f_quantity * i.f_unit_price);
            order.f_status = OrderStatus.Pending;

            await _unitOfWork.BeginAsync();

            try
            {
                var createdOrder = await _orderRepository.Add(order);

                foreach (var item in itemList)
                {
                    item.f_order_uid = createdOrder.f_uid;
                    await _orderItemRepository.Add(item);
                }

                await _orderHistoryRepository.Add(new OrderHistoryModel
                {
                    f_order_uid = createdOrder.f_uid,
                    f_old_status = string.Empty,
                    f_new_status = createdOrder.f_status.ToString(),
                    f_comment = "Order placed."
                });

                var orderSubmittedEvent = new OrderSubmittedEvent
                {
                    OrderUid = createdOrder.f_uid,
                    CustomerUid = createdOrder.f_customer_uid
                };

                var outboxEvent = new OutboxEventModel
                {
                    f_event_type = nameof(OrderSubmittedEvent),
                    f_payload = JsonSerializer.Serialize(orderSubmittedEvent),
                    f_status = "Pending"
                };

                await _outboxEventRepository.Add(outboxEvent);

                await _unitOfWork.CommitAsync();
                return createdOrder;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<OrderModel?> UpdateStatusAsync(Guid uid, OrderStatus newStatus, string? comment = null)
        {
            var existing = await _orderRepository.Find(uid);
            if (existing is null)
            {
                return null;
            }

            var oldStatus = existing.f_status;
            existing.f_status = newStatus;

            await _unitOfWork.BeginAsync();

            try
            {
                var updatedOrder = await _orderRepository.Update(existing);

                await _orderHistoryRepository.Add(new OrderHistoryModel
                {
                    f_order_uid = updatedOrder.f_uid,
                    f_old_status = oldStatus.ToString(),
                    f_new_status = newStatus.ToString(),
                    f_comment = comment
                });

                if (newStatus == OrderStatus.Paid)
                {
                    var orderPaidEvent = new OrderPaidEvent
                    {
                        OrderUid = updatedOrder.f_uid
                    };

                    var outboxEvent = new OutboxEventModel
                    {
                        f_event_type = nameof(OrderPaidEvent),
                        f_payload = JsonSerializer.Serialize(orderPaidEvent),
                        f_status = "Pending"
                    };

                    await _outboxEventRepository.Add(outboxEvent);
                }

                await _unitOfWork.CommitAsync();
                return updatedOrder;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<int> CancelOrderAsync(Guid uid, string? comment = null)
        {
            var existing = await _orderRepository.Find(uid);
            if (existing is null)
            {
                return 0;
            }

            var oldStatus = existing.f_status;
            existing.f_status = OrderStatus.Cancelled;

            await _unitOfWork.BeginAsync();

            try
            {
                await _orderRepository.Update(existing);

                await _orderHistoryRepository.Add(new OrderHistoryModel
                {
                    f_order_uid = existing.f_uid,
                    f_old_status = oldStatus.ToString(),
                    f_new_status = OrderStatus.Cancelled.ToString(),
                    f_comment = comment ?? "Order cancelled."
                });

                var affectedRows = await _orderRepository.Remove(existing);

                await _unitOfWork.CommitAsync();
                return affectedRows;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}
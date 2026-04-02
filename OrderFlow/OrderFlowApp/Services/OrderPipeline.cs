using OrderFlow.OrderFlowApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFlow.OrderFlowApp.Services
{
    public class OrderPipeline
    {
        private readonly OrderValidator _validator;

        public OrderPipeline()
        {
            _validator = new OrderValidator();
        }

        //  ZDARZENIA
        public event EventHandler<OrderStatusChangedEventArgs> StatusChanged;
        public event EventHandler<OrderValidationEventArgs> ValidationCompleted;

        //  METODA GŁÓWNA
        public void ProcessOrder(Order order)
        {
            // 1. Walidacja
            var errors = _validator.ValidateAll(order);
            bool isValid = errors.Count == 0;

            OnValidationCompleted(order, isValid, errors);

            if (!isValid)
                return;

            // 2. Zmiany statusów
            ChangeStatus(order, OrderStatus.Validated);
            SimulateWork();

            ChangeStatus(order, OrderStatus.Processing);
            SimulateWork();

            ChangeStatus(order, OrderStatus.Completed);
        }

        //  Zmiana statusu + event
        private void ChangeStatus(Order order, OrderStatus newStatus)
        {
            var oldStatus = order.Status;
            order.Status = newStatus;

            OnStatusChanged(order, oldStatus, newStatus);
        }

        //  Wywołania zdarzeń
        protected virtual void OnStatusChanged(Order order, OrderStatus oldStatus, OrderStatus newStatus)
        {
            StatusChanged?.Invoke(this, new OrderStatusChangedEventArgs(order, oldStatus, newStatus));
        }

        protected virtual void OnValidationCompleted(Order order, bool isValid, List<string> errors)
        {
            ValidationCompleted?.Invoke(this, new OrderValidationEventArgs(order, isValid, errors));
        }

        private void SimulateWork()
        {
            Thread.Sleep(500); // symulacja pracy
        }
    }
}

using SepWebshop.Application.Abstractions.Messaging;
using SepWebshop.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SepWebshop.Application.Orders.UpdatePsp;
public sealed record UpdatePspOrderCommand(
    Guid OrderId,
    OrderStatus OrderStatus,
    PaymentMethodType PaymentMethod,
    string PspId,
    string PspPassword
) : ICommand<Guid>;


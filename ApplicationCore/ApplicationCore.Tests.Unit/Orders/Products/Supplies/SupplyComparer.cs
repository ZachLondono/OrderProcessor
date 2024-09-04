using Domain.Orders.Entities.Hardware;
using FluentAssertions.Equivalency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Tests.Unit.Orders.Products.Supplies;

internal static class SupplyComparer {

    public static bool Compare(Supply a, Supply b) {

        return a.Qty == b.Qty && a.Description == b.Description;

    }

}

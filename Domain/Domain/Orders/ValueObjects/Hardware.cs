using Domain.Orders.Entities.Hardware;

namespace Domain.Orders.ValueObjects;

public record Hardware(Supply[] Supplies, DrawerSlide[] DrawerSlides, HangingRail[] HangingRails) {

    public static Hardware None() => new([], [], []);

};
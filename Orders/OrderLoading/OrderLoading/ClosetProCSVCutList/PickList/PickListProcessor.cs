using Domain.Orders.Entities.Hardware;
using Domain.ValueObjects;
using OrderLoading.ClosetProCSVCutList.CSVModels;

namespace OrderLoading.ClosetProCSVCutList.PickList;

public class PickListProcessor {

    public static PickListComponents ParsePickList(IEnumerable<PickPart> pickList) {

        Dimension hardwareSpread = GetHardwareSpread(pickList, []);

        var hangRailBrackets = GetHangingRailBrackets(pickList);

        return new(hardwareSpread, hangRailBrackets);

    }

	public static Dimension GetHardwareSpread(IEnumerable<PickPart> parts, Dictionary<string, Dimension> frontHardwareSpreads) {

		var pulls = parts.Where(p => p.Type == "Pull/Knob").ToArray();

        // TODO: check pick list for "Pull/Knob" part types, if there is only one then the drilling spacing for drawer fronts can be inferred from that, if there are multiple then spacing cannot be inferred 
		if (pulls.Length != 1) return Dimension.Zero;

		var pull = pulls[0];

        if (frontHardwareSpreads.TryGetValue(pull.PartName, out Dimension spread)) {
			return spread;
		}

		return Dimension.Zero;

	}

	public static Supply[] GetHangingRailBrackets(IEnumerable<PickPart> parts) {

		List<Supply> supplies = [];

		foreach (var part in parts) {

			if (part.PartName == "Left Rod End") {
				supplies.Add(Supply.RodMountingBracketOpen(part.Quantity));
			} else if (part.PartName == "Right Rod End") {
				supplies.Add(Supply.RodMountingBracketOpen(part.Quantity));
			}

        }

        return supplies.ToArray();

	}

}
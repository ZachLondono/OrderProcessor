namespace OrderExporting.HardwareList;

public class HardwareList {

	public int RafixCamCount { get; set; }
	public int CamBoltCount { get; set; }
	public int DoubleSidedCamBoltCount {get; set; }
	public int StraightAdjustableShelfPinsCount {get; set; }
	public int LockingAdjustableShelfPinsCount {get; set; }
	public int HangingBracketLHCount {get; set; }
	public int HangingBracketRHCount {get; set; }
	public int HingePlateCount {get; set; }
	public int HingeCount {get; set; }
	public int OpenHangingRodBracketCount {get; set; }
	public int ClosedHangingRodBracketCount {get; set; }

	public HangingRod[] HangingRods { get; set; } = [];

}

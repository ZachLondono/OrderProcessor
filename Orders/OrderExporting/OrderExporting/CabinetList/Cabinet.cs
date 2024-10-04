namespace OrderExporting.CabinetList;

public record Cabinet(int CabNum,
                      int Qty,
                      string Description,
                      double Width,
                      double Height,
                      double Depth,
                      HingedSide HingedSide,
                      bool FinishedLeft,
                      bool FinishedRight,
                      string[] Notes);

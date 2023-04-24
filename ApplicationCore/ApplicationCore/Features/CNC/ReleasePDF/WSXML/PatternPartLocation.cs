using ApplicationCore.Features.CNC.Domain;

namespace ApplicationCore.Features.CNC.ReleasePDF.WSXML;

internal record PatternPartLocation(Point Insert, bool IsRotated);

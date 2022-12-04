using ApplicationCore.Features.CNC.Contracts.Machining;
using System.Diagnostics;

namespace ApplicationCore.Features.CNC.Domain;

public interface ShapeSegment { };

public record ArcSegment(Point Start, Point End, double Radius, ArcDirection Direction) : ShapeSegment;

public record LineSegment(Point Start, Point End) : ShapeSegment;

public class Shape {

	public bool IsClosed { get; private set; }

	private readonly LinkedList<IShapeComponent> _list;

	public Shape() {
		_list = new();
		IsClosed = false;
	}

	public void AddLine(Point start, Point end) {

		if (start == end) throw new InvalidOperationException("Zero length route operation");

		if (_list.Last is not null) {

			var last = _list.Last;

			if (last.Value is Fillet fillet) {

				var vector = new Vector(end.X - start.X, end.Y - start.Y);
				var normal = vector.GetNormal();
				start = new(
					start.X + normal.X * fillet.Radius,
					start.Y + normal.Y * fillet.Radius
				);

				// Adjust start position of this new line, and end position of previous fillet
				fillet.End = start;

				if (fillet.End != start) throw new InvalidOperationException("Non-continuous route");

				if (last.Previous is not null && last.Previous.Value is Line prevline) {

					fillet.Direction = IsLeftOfLine(prevline, end) switch {
						true => ArcDirection.CounterClockwise,
						false => ArcDirection.Clockwise,
					};


				}

			} else if (last.ValueRef is Line line && line.End != start) {
				throw new InvalidOperationException("Non-continuous route");
			}

		}

		if (_list.First is not null) {
			if (_list.First.Value is Line firstLine) {
				if (firstLine.Start == end) IsClosed = true;
			} else throw new UnreachableException("Cannot start a route sequence with a fillet"); ;
		}

		AddComponent(new Line() {
			Start = start,
			End = end
		});

	}

	public void AddFillet(double radius) {
		if (radius == 0) return;
		if (_list.Last is null) throw new InvalidOperationException("Cannot start a route sequence with a fillet");
		if (_list.Last() is Fillet) throw new InvalidOperationException("Cannot add two successive fillets to the same route sequence");
		IsClosed = false; // Cannot end on a fillet

		if (_list.Last() is not Line line) throw new UnreachableException();

		var vector = new Vector(line.Start.X - line.End.X, line.Start.Y - line.End.Y);
		var normal = vector.GetNormal();
		line.End = new(
			line.End.X + normal.X * radius,
			line.End.Y + normal.Y * radius
		);

		AddComponent(new Fillet() {
			Start = line.End,
			Radius = radius,
			End = new(0, 0),                 // To be set when next line is given
			Direction = ArcDirection.Unknown // To be set when next line is given
		});

	}

	public IEnumerable<ShapeSegment> GetSegments() {

		if (!IsClosed) throw new InvalidOperationException("Shape is not closed");

		return _list.Select<IShapeComponent, ShapeSegment>(c => {
			if (c is Line line) return new LineSegment(line.Start, line.End);
			else if (c is Fillet fillet) return new ArcSegment(fillet.Start, fillet.End, fillet.Radius, fillet.Direction);
			throw new UnreachableException("Unknown segment type");
		});

	}

	private void AddComponent(IShapeComponent component) {
		var node = new LinkedListNode<IShapeComponent>(component);
		_list.AddLast(node);
	}

	/// <summary>
	/// Returns true if the point is to the left of the line
	/// </summary>
	private static bool IsLeftOfLine(Line line, Point point) {
		return ((line.End.X - line.Start.X) * (point.Y - line.Start.Y) - (line.End.Y - line.Start.Y) * (point.X - line.Start.X)) > 0;
	}

	private interface IShapeComponent { }

	private class Line : IShapeComponent {
		public required Point Start { get; set; }
		public required Point End { get; set; }
	}

	private class Fillet : IShapeComponent {
		public required Point Start { get; set; }
		public required Point End { get; set; }
		public ArcDirection Direction { get; set; }
		public double Radius { get; set; }
	}

	struct Vector {

		public double X { get; set; }
		public double Y { get; set; }

		public double Magnitude => Math.Sqrt((X * X) + (Y * Y));

		public Vector GetNormal() {
			var magnitude = Magnitude;
			return new(X / magnitude, Y / magnitude);
		}

		public Vector(double x, double y) {
			X = x;
			Y = y;
		}

	}

}
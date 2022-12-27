using ApplicationCore.Features.CNC.GCode.Configuration;

namespace ApplicationCore.Features.CNC.GCode.Services;

public class CADCodeFactory : ICADCodeFactory {

	private readonly IToolFileReader _toolFileReader;

	public CADCodeFactory(IToolFileReader toolFileReader) {
		_toolFileReader = toolFileReader;
	}

	public ICADCodeProxy CreateCADCode() {

		var configuration = new CADCodeConfiguration();

		return new CADCodeProxy(_toolFileReader, configuration);

	}

}
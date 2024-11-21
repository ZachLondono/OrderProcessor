using OrderLoading.ClosetProCSVCutList;
using Domain.Services;
using OrderLoading.ClosetProCSVCutList.PartList;
using OrderLoading.ClosetProCSVCutList.Header;

namespace OrderLoading.LoadClosetProOrderData.LoadClosetProFileOrderData;

public class ClosetProFileCSVOrderProvider : ClosetProCSVOrderProvider {

	private readonly IFileReader _fileReader;

	public ClosetProFileCSVOrderProvider(ClosetProCSVReader reader, PartListProcessor partListProcessor, OrderHeaderProcessor orderHeaderProcessor,
										IFileReader fileReader)
									 : base(reader, partListProcessor, orderHeaderProcessor) {
		_fileReader = fileReader;
	}


	protected override async Task<string?> GetCSVDataFromSourceAsync(string source) {
		try {
			using var stream = _fileReader.OpenReadFileStream(source);
			using var reader = new StreamReader(stream);
			return await reader.ReadToEndAsync();
		} catch (Exception ex) {
			OrderLoadingViewModel?.AddLoadingMessage(MessageSeverity.Error, $"Could not load order data from Closet Pro: {ex.Message}");
			return null;
		}
	}

}

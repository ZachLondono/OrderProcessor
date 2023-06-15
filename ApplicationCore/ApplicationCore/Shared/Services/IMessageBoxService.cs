namespace ApplicationCore.Shared.Services;

public interface IMessageBoxService {

    // TODO: create separate methods for different message boxes (ie. error, information, Yes/No)

    public void OpenDialog(string text, string caption);

    public OKCancelResult OpenDialogOKCancel(string text, string caption);

    public YesNoResult OpenDialogYesNo(string text, string caption);

}

public enum OKCancelResult {
    Unknown,
    OK,
    Cancel
}


public enum YesNoResult {
    Unknown,
    Yes,
    No
}

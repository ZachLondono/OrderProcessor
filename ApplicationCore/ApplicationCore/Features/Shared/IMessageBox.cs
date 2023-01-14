namespace ApplicationCore.Features.Shared;

public interface IMessageBoxService {

    // TODO: create seperate methods for different message boxes (ie. error, informaiton, Yes/No)

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

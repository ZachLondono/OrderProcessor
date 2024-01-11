namespace ApplicationCore.Shared.CNC.WorkOrderReleaseEmail;

public class ReleaseEmailBodyGenerator {

    public static string GenerateTextReleaseEmailBody(ReleasedWorkOrderSummary model, bool includeSummary) {

        string body = "Please see attached release";

        if (!string.IsNullOrWhiteSpace(model.Note)) {
            body += $"\nNote: {model.Note}";
        }

        if (!includeSummary) {
            return body;
        }

        foreach (var job in model.ReleasedJobs) {

            body += $"\n{job.JobName} Materials:\n";
            foreach (var material in job.UsedMaterials) {
                body += $"({material.Qty}) {material.Name} - {material.Width}x{material.Length}x{material.Thickness}\n";
            }

        }

        return body;

    }

    public static string GenerateHTMLReleaseEmailBody(ReleasedWorkOrderSummary model, bool includeSummary) {

        string body = "Please see attached release";

        if (!string.IsNullOrWhiteSpace(model.Note) || true) {

            body +=
                $"""

                <br />
                <br />

                <div>
                    <table>
                        <tr>
                            <td style="border: 1px solid black; padding: 5px;">
                                <div style="font-weight: bold;">Note:</div>
                                <div style="white-space: pre-wrap;">{model.Note}</div>
                            </td>
                        </tr>
                    </table>
                </div>

                """;

        }

        if (model.ContainsDrawerBoxes || model.ContainsMDFDoors || model.ContainsFivePieceDoors) {
            body +=
                """

                <br />
                <br />

                """;
        }

        if (model.ContainsDrawerBoxes) {
            body +=
                """
                <div>
                    <b>Order Contains Drawer Boxes</b>
                </div>
                """;
        }

        if (model.ContainsMDFDoors) {
            body +=
                """
                <div>
                    <b>Order Contains MDF Doors</b>
                </div>
                """;
        }

        if (model.ContainsFivePieceDoors) {
            body +=
                """
                <div>
                    <b>Order Contains Five-Piece Doors</b>
                </div>
                """;
        }

        if (!includeSummary) {
            return body;
        }

        // TODO: generate the data to create a table of required materials in a similar way to the QuestPDFWriter
        foreach (var job in model.ReleasedJobs) {

            string tableHeader =
                $"""

                    <br />
                    <br />

                    <div>
                        <table style="border: 1px solid black;">

                            <caption><b>{job.JobName} Materials</b></caption>

                            <tr style="border: 1px solid black; border-collapse: collapse;">
                                <th style="border: 1px solid black; border-collapse: collapse; padding-left: 5px; padding-right:5px;">Qty</th>
                                <th style="border: 1px solid black; border-collapse: collapse; padding-left: 5px; padding-right:5px;">Name</th>
                                <th style="border: 1px solid black; border-collapse: collapse; padding-left: 5px; padding-right:5px;">Width</th>
                                <th style="border: 1px solid black; border-collapse: collapse; padding-left: 5px; padding-right:5px;">Length</th>
                                <th style="border: 1px solid black; border-collapse: collapse; padding-left: 5px; padding-right:5px;">Thickness</th>
                            </tr>

                """;

            string tableBody = "";
            foreach (var material in job.UsedMaterials) {
                tableBody +=
                    $"""

                                <tr style="border: 1px solid black; border-collapse: collapse;">
                                    <td style="border: 1px solid black; border-collapse: collapse; padding-left: 5px; padding-right:5px;">{material.Qty}</td>
                                    <td style="border: 1px solid black; border-collapse: collapse; padding-left: 5px; padding-right:5px;">{material.Name}</td>
                                    <td style="border: 1px solid black; border-collapse: collapse; padding-left: 5px; padding-right:5px;">{material.Width}</td>
                                    <td style="border: 1px solid black; border-collapse: collapse; padding-left: 5px; padding-right:5px;">{material.Length}</td>
                                    <td style="border: 1px solid black; border-collapse: collapse; padding-left: 5px; padding-right:5px;">{material.Thickness}</td>
                                </tr>

                    """;

            }

            string tableFooter =
                """

                        </table>
                    </div>
                """;

            body += tableHeader + tableBody + tableFooter;

        }

        return body;

    }

}

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CGTCalculator;

internal static class ReportPdfExporter
{
    private enum CellAlignment
    {
        Left,
        Center,
        Right
    }

    internal static IReadOnlyList<string> Export(string folderPath, CgtReport report, decimal sellPrice, string currency)
    {
        Directory.CreateDirectory(folderPath);

        var filePaths = new List<string>();

        if (report.Open.LineItems.Count > 0)
        {
            filePaths.Add(ExportSingleReport(folderPath, report.Open, "Open positions", sellPrice, currency));
        }

        foreach (var singleYearReport in report.Reports)
        {
            filePaths.Add(ExportSingleReport(folderPath, singleYearReport, singleYearReport.TaxYear, sellPrice, currency));
        }

        return filePaths;
    }

    private static string ExportSingleReport(string folderPath, CgtSingleYearReport report, string title, decimal sellPrice, string currency)
    {
        var filePath = Path.Combine(folderPath, $"{GetSafeFileName(title)}.pdf");

        Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(text => text.FontSize(10).FontColor("#172033"));

                page.Header().Element(container => ComposeHeader(container, title, report));

                page.Content().Column(column =>
                {
                    column.Spacing(18);
                    column.Item().Element(container => ComposeSummaryPage(container, report, sellPrice, currency));

                    if (report.ContributingTransactions.Count > 0)
                    {
                        column.Item().PageBreak();
                        column.Item().Element(container => ComposeContributingTransactionsPage(container, report));
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf(filePath);

        return filePath;
    }

    private static void ComposeHeader(IContainer container, string title, CgtSingleYearReport report)
    {
        container.Column(column =>
        {
            column.Spacing(8);

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(details =>
                {
                    details.Item().Text("CGT Calculator").FontSize(22).SemiBold().FontColor("#2563EB");
                    details.Item().Text($"Report: {title}").FontSize(16).SemiBold();
                });

                row.ConstantItem(185).AlignRight().Column(meta =>
                {
                    meta.Item().Text($"Generated {DateTime.Now:dd MMM yyyy HH:mm}");
                    meta.Item().Text($"{report.LineItems.Count} report line item(s)");
                    meta.Item().Text($"{report.ContributingTransactions.Count} contributing transaction(s)");
                });
            });

            column.Item()
                .Height(1)
                .Background("#D9E3F0");
        });
    }

    private static void ComposeSummaryPage(IContainer container, CgtSingleYearReport report, decimal sellPrice, string currency)
    {
        container.Column(column =>
        {
            column.Spacing(16);

            if (report.TaxYearSort < 0)
            {
                column.Item().Element(ContextCardStyle).Text($"Open positions use an assumed sell price of {sellPrice:0.####} {currency}.");
            }

            column.Item().Element(SectionCardStyle).Column(section =>
            {
                section.Spacing(12);
                section.Item().Text("Report line items").FontSize(16).SemiBold().FontColor("#0F172A");
                section.Item().Element(tableContainer => ComposeLineItemsTable(tableContainer, report.LineItems));
            });

            column.Item().Element(SectionCardStyle).Element(summaryContainer => ComposeSummaryTable(summaryContainer, report));
        });
    }

    private static void ComposeContributingTransactionsPage(IContainer container, CgtSingleYearReport report)
    {
        container.Column(column =>
        {
            column.Spacing(12);

            column.Item().Element(SectionCardStyle).Column(section =>
            {
                section.Spacing(10);
                section.Item().Text("Contributing transactions").FontSize(16).SemiBold().FontColor("#0F172A");
                section.Item().Text("These are the underlying transactions from the database that contribute to this report.")
                    .FontColor("#5B6B83");
                section.Item().Element(tableContainer =>
                    ComposeTransactionsTable(
                        tableContainer,
                        report.ContributingTransactions
                            .OrderBy(transaction => transaction.Date)
                            .ThenBy(transaction => transaction.Symbol)
                            .ThenBy(transaction => transaction.Type)
                            .ToList()));
            });
        });
    }

    private static void ComposeLineItemsTable(IContainer container, IReadOnlyList<CgtEvent> lineItems)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1.1f);
                columns.RelativeColumn(1f);
                columns.RelativeColumn(1f);
                columns.RelativeColumn(0.8f);
                columns.RelativeColumn(1f);
                columns.RelativeColumn(1f);
                columns.RelativeColumn(1f);
                columns.RelativeColumn(0.75f);
            });

            table.Header(header =>
            {
                HeaderCell(header.Cell(), "Symbol");
                HeaderCell(header.Cell(), "Purchase");
                HeaderCell(header.Cell(), "Sale");
                HeaderCell(header.Cell(), "Quantity");
                HeaderCell(header.Cell(), "Cost Base");
                HeaderCell(header.Cell(), "Sales Value");
                HeaderCell(header.Cell(), "Profit");
                HeaderCell(header.Cell(), "Gain");
            });

            foreach (var item in lineItems)
            {
                BodyCell(table.Cell(), item.Symbol);
                BodyCell(table.Cell(), item.PurchaseDate.ToString("dd MMM yyyy"));
                BodyCell(table.Cell(), item.SellDate.ToString("dd MMM yyyy"));
                BodyCell(table.Cell(), item.Quantity.ToString("0.####"), CellAlignment.Right);
                BodyCell(table.Cell(), item.CostBase.ToString("$#,##0.00"), CellAlignment.Right);
                BodyCell(table.Cell(), item.SalesValue.ToString("$#,##0.00"), CellAlignment.Right);
                BodyCell(table.Cell(), item.Profit.ToString("$#,##0.00"), CellAlignment.Right, item.Profit < 0 ? "#C2410C" : "#15803D");
                BodyCell(table.Cell(), item.Long ? "Long" : "Short", CellAlignment.Center);
            }
        });
    }

    private static void ComposeTransactionsTable(IContainer container, IReadOnlyList<Transaction> transactions)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1f);
                columns.RelativeColumn(0.9f);
                columns.RelativeColumn(1.1f);
                columns.RelativeColumn(0.9f);
                columns.RelativeColumn(1f);
                columns.RelativeColumn(0.8f);
            });

            table.Header(header =>
            {
                HeaderCell(header.Cell(), "Date");
                HeaderCell(header.Cell(), "Type");
                HeaderCell(header.Cell(), "Symbol");
                HeaderCell(header.Cell(), "Quantity");
                HeaderCell(header.Cell(), "Value");
                HeaderCell(header.Cell(), "Tax Year");
            });

            foreach (var transaction in transactions)
            {
                BodyCell(table.Cell(), transaction.Date.ToString("dd MMM yyyy"));
                BodyCell(table.Cell(), transaction.Type.ToString());
                BodyCell(table.Cell(), transaction.Symbol);
                BodyCell(table.Cell(), transaction.Quantity.ToString("0.####"), CellAlignment.Right);
                BodyCell(table.Cell(), transaction.Value.ToString("$#,##0.00"), CellAlignment.Right);
                BodyCell(table.Cell(), transaction.TaxYear.ToString(), CellAlignment.Center);
            }
        });
    }

    private static void ComposeSummaryTable(IContainer container, CgtSingleYearReport report)
    {
        container.Column(column =>
        {
            column.Spacing(12);
            column.Item().Text("Summary").FontSize(16).SemiBold().FontColor("#0F172A");

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.ConstantColumn(24);
                    columns.ConstantColumn(120);
                });

                void AddSection(string title)
                {
                    table.Cell().ColumnSpan(3).PaddingTop(8).PaddingBottom(4).Text(title).SemiBold().FontColor("#1E3A8A");
                }

                void AddRow(string label, string op, decimal value, bool bold = false)
                {
                    var text = value.ToString("$#,##0.00");

                    var labelCell = table.Cell().Element(SummaryCellStyle);
                    var opCell = table.Cell().Element(SummaryCellStyle).AlignCenter();
                    var valueCell = table.Cell().Element(SummaryCellStyle).AlignRight();

                    if (bold)
                    {
                        labelCell.Text(label).SemiBold();
                        opCell.Text(op).SemiBold();
                        valueCell.Text(text).SemiBold();
                        return;
                    }

                    labelCell.Text(label);
                    opCell.Text(op);
                    valueCell.Text(text);
                }

                if (report.TaxYearSort < 0)
                {
                    AddSection("Value");
                    AddRow("Short Term Total", string.Empty, report.ShortTermTotal);
                    AddRow("Long Term Total", "+", report.LongTermTotal);
                }

                AddRow("Total Value", "=", report.TotalValue, true);

                if (report.TaxYearSort < 0)
                {
                    AddSection("If All Sold");
                }

                AddRow("Short Term Gains", string.Empty, report.ShortTermGains);
                AddRow("Long Term Gains", "+", report.LongTermGains);
                AddRow("CGT Concession", "-", report.CGTConcession);
                AddRow("Capital Gain", "=", report.TotalCapitalGain, true);

                if (report.TaxYearSort < 0)
                {
                    AddSection("If Only Long Sold");
                    AddRow("Long Term Gains", string.Empty, report.LongTermGains);
                    AddRow("CGT Concession", "-", report.CGTConcession);
                    AddRow("Capital Gain", "=", report.LongTermGains - report.CGTConcession, true);
                }

                AddRow("Total Buys and Washes", "=", report.TotalBuysAndWashes, true);
            });
        });
    }

    private static void HeaderCell(IContainer container, string text)
    {
        container
            .Element(HeaderCellStyle)
            .Text(text)
            .SemiBold()
            .FontColor("#1E293B");
    }

    private static void BodyCell(IContainer container, string text, CellAlignment alignment = CellAlignment.Left, string? color = null)
    {
        var cell = container.Element(BodyCellStyle);

        cell = alignment switch
        {
            CellAlignment.Center => cell.AlignCenter(),
            CellAlignment.Right => cell.AlignRight(),
            _ => cell
        };

        var textDescriptor = cell.Text(text);
        if (!string.IsNullOrWhiteSpace(color))
        {
            textDescriptor.FontColor(color);
        }
    }

    private static IContainer HeaderCellStyle(IContainer container)
    {
        return container
            .Background("#EEF4FF")
            .BorderBottom(1)
            .BorderColor("#D9E3F0")
            .PaddingVertical(6)
            .PaddingHorizontal(6);
    }

    private static IContainer BodyCellStyle(IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor("#E5EAF2")
            .PaddingVertical(6)
            .PaddingHorizontal(6);
    }

    private static IContainer SectionCardStyle(IContainer container)
    {
        return container
            .Background(Colors.White)
            .Border(1)
            .BorderColor("#D9E3F0")
            .Padding(14);
    }

    private static IContainer ContextCardStyle(IContainer container)
    {
        return container
            .Background("#EEF4FF")
            .Border(1)
            .BorderColor("#D9E3F0")
            .Padding(12);
    }

    private static IContainer SummaryCellStyle(IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor("#E5EAF2")
            .PaddingVertical(5)
            .PaddingHorizontal(4);
    }

    private static string GetSafeFileName(string title)
    {
        var fileName = $"CGT Report - {title}";
        foreach (var invalidCharacter in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(invalidCharacter, '-');
        }

        return fileName;
    }
}

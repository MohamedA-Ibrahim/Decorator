using Decorator.DataAccess;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace Decorator.App.Reporting
{
    public class ProductSalesDocument : IDocument
    {
        public List<ProductOrdersDTO> Model { get;}

        public ProductSalesDocument(List<ProductOrdersDTO> model)
        {
            Model = model;
        }

        public void Compose(IDocumentContainer container)
        {
            container
                
               .Page(page =>
               {
                   page.DefaultTextStyle(x => x.FontFamily("Calibri"));
                   page.ContentFromRightToLeft();
                   page.Margin(20);
                   page.Header().Element(ComposeHeader);
                   page.Content().Element(ComposeContent);

                   page.Footer().AlignCenter().Text(text =>
                   {
                       text.CurrentPageNumber();
                       text.Span(" / ");
                       text.TotalPages();
                   });
               });

        }

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.ConstantItem(100).Image("Assets/Decorator.jpg");

                row.RelativeItem().Column(Column =>
                {
                    Column
                        .Item().AlignCenter().PaddingTop(40)
                        .Text($"تقرير جرد مشتريات")
                        .FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                });

            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(40).Column(column =>
            {
                column.Spacing(5);

                column.Item().Element(ComposeTable);

                var totalPrice = Model.Sum(x => x.Price * x.Quantity);
                column.Item().PaddingLeft(5).AlignLeft().Text("الإجمالي: " + totalPrice.ToString()).SemiBold();

            });
        }

        private void ComposeTable(IContainer container)
        {
            var headerStyle = TextStyle.Default.SemiBold();

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(20);
                    columns.RelativeColumn();
                    columns.RelativeColumn(2);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Text("#");
                    header.Cell().Text("تاريخ الشراء").Style(headerStyle);
                    header.Cell().Text("الصنف").Style(headerStyle);
                    header.Cell().Text("العميل").Style(headerStyle);
                    header.Cell().AlignRight().Text("الكمية").Style(headerStyle);
                    header.Cell().AlignRight().Text("السعر").Style(headerStyle);
                    header.Cell().AlignRight().Text("اجمالي السعر").Style(headerStyle);

                    header.Cell().ColumnSpan(7).PaddingTop(7).BorderBottom(1).BorderColor(Colors.Black);
                });



                foreach (var p in Model)
                {
                    var index = Model.IndexOf(p) + 1;

                    table.Cell().Element(CellStyle).Text($"{index}");
                    table.Cell().Element(CellStyle).Text($"{p.PurchaseDate:yyyy/MM/dd}");
                    table.Cell().Element(CellStyle).Text(p.ProductName);
                    table.Cell().Element(CellStyle).Text(p.CustomerName);
                    table.Cell().Element(CellStyle).AlignCenter().Text($"{p.Quantity}");
                    table.Cell().Element(CellStyle).AlignRight().Text(p.Price.ToString("0.00"));
                    table.Cell().Element(CellStyle).AlignCenter().Text($"{p.Price * p.Quantity :0.00}");

                    static IContainer CellStyle(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                }
            });
        }


        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    }
}

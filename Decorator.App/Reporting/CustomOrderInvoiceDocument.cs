using Decorator.DataAccess;
using Decorator.DataAccess.Models.DatabaseModels;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Linq;

namespace Decorator.App.Reporting
{
    public class CustomOrderInvoiceDocument : IDocument
    {
        public CustomOrder Model { get;}

        public CustomOrderInvoiceDocument(CustomOrder model)
        {
            Model = model;
        }

        public void Compose(IDocumentContainer container)
        {
            container           
               .Page(page =>
               {
                   page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(14));
                   page.ContentFromRightToLeft();
                   page.Margin(50);
                   page.Header().Element(ComposeHeader);
                   page.Content().Element(ComposeContent);
                   page.Footer().AlignCenter().Element(ComposeComments);
               });

        }

        private void ComposeHeader(IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(Column =>
                {
                    Column.Item().Text("Decorator").FontFamily("SimSun").FontSize(32).SemiBold();
                    Column.Item().Text("لجميع مستلزمات الديكور").FontSize(18).SemiBold();

                });

                row.ConstantItem(130).Image("Assets/Decorator.jpg");
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(40).Column(column =>
            {
                column.Spacing(5);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Component(new CustomerInfoComponent(Model));
                });

                column.Item().Element(ComposeTable);

                var grandTotal = Model.CustomOrderItems.Sum(x => x.Price * x.Quantity);
                var discount = Model.Discount;

                if (discount != 0)
                {
                    column.Item().PaddingLeft(5).AlignLeft().Text(text =>
                    {
                        text.Span("الخصم: ").SemiBold();
                        text.Span(discount.ToString("0")).DirectionFromLeftToRight();
                    });             
                }

                column.Item().PaddingTop(3).PaddingLeft(5).AlignLeft().Text(text =>
                {
                    text.Span("الإجمالي: ").SemiBold();
                    text.Span((grandTotal - discount).ToString("0")).DirectionFromLeftToRight();
                });
            });
        }

        private void ComposeTable(IContainer container)
        {
            var headerStyle = TextStyle.Default.SemiBold();

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);
                    columns.RelativeColumn(3);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Text("#");
                    header.Cell().Text("الصنف").Style(headerStyle);
                    header.Cell().AlignRight().Text("إجمالي الكمية").Style(headerStyle);
                    header.Cell().AlignRight().Text("السعر").Style(headerStyle);
                    header.Cell().AlignRight().Text("اجمالي السعر").Style(headerStyle);

                    header.Cell().ColumnSpan(5).PaddingTop(5).BorderBottom(1).BorderColor(Colors.Black);
                });

                foreach (var od in Model.CustomOrderItems)
                {
                    var index = Model.CustomOrderItems.IndexOf(od) + 1;

                    table.Cell().Element(CellStyle).Text($"{index}");

                    table.Cell().Element(CellStyle).Text($"{od.Name}");

                    table.Cell().Element(CellStyle).AlignCenter().Text($"{od.Quantity}");
                    table.Cell().Element(CellStyle).AlignRight().Text(od.Price.ToString("0.00"));
                    table.Cell().Element(CellStyle).AlignCenter().Text($"{od.Price * od.Quantity:0.00}");

                    static IContainer CellStyle(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                }
            });
        }

        private void ComposeComments(IContainer container)
        {
            container.ShowEntire().Background(Colors.LightBlue.Lighten3).Padding(10).Column(column =>
            {
                column.Spacing(5);
                column.Item().Text("استلمت انا البضاعة عالية الجودة وسليمة وبحالة ممتازة وفقا للطلبية المتفق عليها ").FontSize(16).SemiBold();
                column.Item().Text("التاريخ: ....................................................................................").FontSize(16);
                column.Item().Text("التوقيع: ....................................................................................").FontSize(16);
                column.Item().Text("العنوان: الشارع الجديد خلف فيلا المحافظ بجوار مطعم مفرح").FontSize(16);

                column.Item().Text(text =>
                {
                    text.Span("ت:     ").FontSize(16);
                    text.Span("01009094462").FontSize(16).DirectionFromLeftToRight();
                });

                column.Item().Text(text =>
                {
                    text.Span("واتس: ").FontSize(16);
                    text.Span("01099797984").FontSize(16).DirectionFromLeftToRight();
                });
            });
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    }

    public class CustomerInfoComponent : IComponent
    {
        private CustomOrder Model;

        public CustomerInfoComponent(CustomOrder model)
        {
            Model = model;
        }

        public void Compose(IContainer container)
        {
            container.ShowEntire().Column(column =>
            {
                column.Spacing(2);

                column.Item().Text("بيانات العميل").SemiBold();
                column.Item().PaddingBottom(5).LineHorizontal(1);

                column.Item().Text(text =>
                {
                    text.Span("اسم العميل: ").SemiBold();
                    text.Span(Model.CustomerName);
                });

                if (!string.IsNullOrWhiteSpace(Model.CustomerName))
                {
                    column.Item().Text(text =>
                    {
                        text.Span("عنوان العميل: ").SemiBold();
                        text.Span(Model.CustomerAddress);
                    });
                }
                if (!string.IsNullOrWhiteSpace(Model.CustomerPhone))
                {
                    column.Item().Text(text =>
                    {
                        text.Span("رقم الهاتف: ").SemiBold();
                        text.Span(Model.CustomerPhone);
                    });
                }

                column.Item().Text(text =>
                {
                    text.Span("تاريخ الفاتورة: ").SemiBold();
                    text.Span($"{Model.PurchaseDate:d}");
                });
            });
        }
    }

}

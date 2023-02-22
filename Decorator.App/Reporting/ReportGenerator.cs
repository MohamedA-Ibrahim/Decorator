using Decorator.DataAccess;
using Decorator.DataAccess.Models.DatabaseModels;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Decorator.App.Reporting
{
    public static class ReportGenerator
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void GenerateOrderReport(Order order)
        {
            var document = new OrderInvoiceDocument(order);

            GenerateDocumentAndShow(document);
        }

        public static void GenerateCustomOrderReport(CustomOrder order)
        {
            var document = new CustomOrderInvoiceDocument(order);

            GenerateDocumentAndShow(document);
        }

        public static void GenerateProductReport(List<ProductOrdersDTO> productOrders)
        {
            var document = new ProductSalesDocument(productOrders);
            GenerateDocumentAndShow(document, "ProductOrdersReport");

        }

        private static void GenerateDocumentAndShow(IDocument document, string fileName = "invoice")
        {
            try
            {
                fileName += ".pdf";

                logger.Info("Started generating report");

                document.GeneratePdf(fileName);

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo(fileName)
                    {
                        UseShellExecute = true
                    }
                };

                process.Start();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

        }
    }
}

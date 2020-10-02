using FiverrNotifications.Client.Models;
using FiverrNotifications.Logic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FiverrNotifications.Client
{
    public static class RequestMapper
    {
        class Headers
        {
            public int? Date { get; set; }
            public int? Request { get; set; }
            public int? Duration { get; set; }
            public int? Budget { get; set; }
        }
        public static List<FiverrRequest> Map(this FiverrResponse response)
        {
            var columnIndexes = response.Results.Head
                .Select((h, idx) => new KeyValuePair<string, int>(h.Text, idx))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase);

            var headers = new Headers
            {
                Date = columnIndexes.GetValueOrDefault("Date"),
                Request = columnIndexes.GetValueOrDefault("request"),
                Duration = columnIndexes.GetValueOrDefault("Duration"),
                Budget = columnIndexes.GetValueOrDefault("Budget")
            };

            var requests = response.Results.Rows
                .Where(row => row.Identifier != null)
                .Select(row =>
                    new FiverrRequest
                    {
                        RequestId = row.Identifier,
                        Date = headers.Date.HasValue ? DateTime.Parse(row.Cells[headers.Date.Value].Text) : (DateTime?)null,
                        Request = headers.Request.HasValue ? row.Cells[headers.Request.Value].Text : null,
                        Duration = headers.Duration.HasValue ? row.Cells[headers.Duration.Value].Text : null,
                        Budget = headers.Budget.HasValue ? row.Cells[headers.Budget.Value].Text : null
                    })
                .ToList();

            foreach (var request in requests)
            {
                request.Request = HttpUtility.HtmlDecode(request.Request);
            }

            return requests;
        }
    }
}

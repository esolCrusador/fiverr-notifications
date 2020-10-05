using Dapper;
using FiverrNotifications.Logic.Models;
using FiverrNotifications.Logic.Repositories;
using FiverrTelegramNotifications.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FiverrTelegramNotifications.Data
{
    public class MessagesRepository : IMessagesRepository
    {
        private const string InsertMessagesQuery = @"
						BEGIN TRANSACTION

						CREATE TABLE #Requests
						(
							RequestId VARCHAR(64),
						)

						BEGIN TRY
							INSERT INTO #Requests
								(RequestId)
							VALUES
								{0}

							DELETE FROM er
								FROM fiverr.Request AS er
								WHERE NOT EXISTS 
								(
									SELECT TOP 1 1 FROM #Requests AS nr
										WHERE nr.RequestId = er.RequestId AND er.SessionId = @sessionId
								)

							DELETE FROM nr
								FROM #Requests AS nr
								WHERE EXISTS 
								(
									SELECT TOP 1 1 FROM fiverr.Request AS er
										WHERE nr.RequestId = er.RequestId AND er.SessionId = @sessionId
								)
	
							INSERT INTO fiverr.Request 
								(RequestId, SessionId)
							SELECT RequestId, @sessionId
								FROM #Requests

							COMMIT TRANSACTION

							SELECT RequestId FROM #Requests
						END TRY
						BEGIN CATCH 
							ROLLBACK TRANSACTION;
						END CATCH

						DROP TABLE #Requests
						";

		private readonly SqlConnectionFactory _sqlConnectionFactory;

        public MessagesRepository(SqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }
        public async Task<IReadOnlyCollection<FiverrRequest>> FindNewRequests(int sessionId, IReadOnlyCollection<FiverrRequest> requests)
        {
			if (requests.Count == 0)
				return requests;

            await using var sqlConnection = await _sqlConnectionFactory.Create();
			var newRequestsEnumerable = await sqlConnection.QueryAsync<RequestEntity>(
				GetMessagesQuery(requests.Select(r => r.RequestId)),
				new { sessionId }
			);
			var newRequests = newRequestsEnumerable.Select(r => r.RequestId).ToHashSet();

			return requests.Where(r => newRequests.Contains(r.RequestId)).ToList();
        }

		private static string GetMessagesQuery(IEnumerable<string> requestIds) =>
			string.Format(InsertMessagesQuery, string.Join(", ", requestIds.Select(id => $"('{id}')")));
	}
}

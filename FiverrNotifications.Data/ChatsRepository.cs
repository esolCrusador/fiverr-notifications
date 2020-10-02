﻿using Dapper;
using FiverrNotifications.Logic.Models;
using FiverrNotifications.Logic.Repositories;
using FiverrTelegramNotifications.Data.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace FiverrTelegramNotifications.Data
{
    public class ChatsRepository : IChatsRepository
    {
        private const string SessionSelect = "SELECT s.SessionId, s.ChatId, s.BotId, s.ChatName, s.FiverrUsername, s.FiverrSession, s.FiverrToken, s.IsAuthRequested FROM fiverr.TelegramBotChat AS s";

        private readonly SqlConnectionFactory _sqlConnectionFactory;

        public ChatsRepository(SqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }
        public async Task<StoredSession> AddChat(int botId, long chatId)
        {
            await using var sqlConnection = await _sqlConnectionFactory.Create();
            var sessionId = await sqlConnection.QueryFirstOrDefaultAsync<int>(
                "IF (EXISTS (SELECT TOP 1 1 FROM fiverr.TelegramBotChat AS tbc WHERE tbc.ChatId = @chatId AND tbc.BotId = @botId))" +
                "\r\n SELECT -1" +
                "\r\n ELSE" +
                "\r\n BEGIN" +
                "\r\nINSERT INTO fiverr.TelegramBotChat (ChatId, BotId, IsAuthRequested) VALUES (@chatId, @botId, 1)" +
                "\r\n SELECT SCOPE_IDENTITY()" +
                "\r\n END",
                new { botId, chatId }
            );
            if (sessionId == -1)
                return null;

            return new StoredSession
            {
                SessionId = sessionId,
                BotId = botId,
                Username = null,
                Session = null,
                Token = null
            };
        }

        public async Task<IReadOnlyCollection<BotData>> GetBots()
        {
            await using var sqlConnection = await _sqlConnectionFactory.Create();

            var bots = await sqlConnection.QueryAsync<BotFiverrEntity>("SELECT b.BotId, b.Name, b.Secret FROM fiverr.TelegramBot AS b");

            return bots.Select(b => new BotData
            {
                BotId = b.BotId,
                BotSecret = b.Secret
            }).ToList();
        }

        public async Task<IReadOnlyCollection<StoredSession>> GetSessions(IReadOnlyCollection<int> botIds)
        {
            await using var sqlConnection = await _sqlConnectionFactory.Create();

            if (botIds.Count == 0)
                return new List<StoredSession>();

            var sessions = await sqlConnection.QueryAsync<StoredSessionEntity>(
                SessionSelect +
                $"\r\nWHERE s.BotId IN ({string.Join(", ", botIds)})"
            );

            return sessions.Select(Map).ToList();
        }

        public async Task<StoredSession> RemoveChat(int botId, long chatId)
        {
            await using var sqlConnection = await _sqlConnectionFactory.Create();

            var storedSession = await sqlConnection.QueryFirstOrDefaultAsync<StoredSessionEntity>(
                SessionSelect +
                $"\r\n WHERE s.BotId = @botId AND s.ChatId = @chatId"
            );
            if (storedSession == null)
                return null;

            await sqlConnection.QueryAsync("DELETE FROM fiverr.Request WHERE SessionId = @sessionId" +
                "\r\nDELETE FROM fiverr.TelegramBotChat WHERE SessionId = @sessionId", new { sessionId = storedSession.SessionId });

            return Map(storedSession);
        }

        public async Task UpdateSession(StoredSession storedSession)
        {
            await using var sqlConnection = await _sqlConnectionFactory.Create();

            var session = Map(storedSession);

            await sqlConnection.QueryAsync(
                "UPDATE s " +
                "SET FiverrUsername = @fiverrUsername, FiverrSession = @fiverrSession, FiverrToken = @fiverrToken" +
                "\r\nFROM fiverr.TelegramBotChat AS s" +
                "\r\n WHERE s.SessionId = @sessionId",
                new
                {
                    sessionId = storedSession.SessionId,
                    fiverrUsername = storedSession.Username,
                    fiverrSession = storedSession.Session,
                    fiverrToken = storedSession.Token
                }
            );
        }

        private static StoredSession Map(StoredSessionEntity session) => new StoredSession
        {
            SessionId = session.SessionId,
            BotId = session.BotId,
            ChatId = session.ChatId,
            Username = session.FiverrUsername,
            Session = session.FiverrSession,
            Token = session.FiverrToken
        };

        private static StoredSessionEntity Map(StoredSession session) => new StoredSessionEntity
        {
            SessionId = session.SessionId,
            BotId = session.BotId,
            ChatId = session.ChatId,
            FiverrUsername = session.Username,
            FiverrSession = session.Session,
            FiverrToken = session.Token
        };
    }
}
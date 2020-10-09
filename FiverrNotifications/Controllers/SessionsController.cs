using FiverrNotifications.Logic.Models;
using FiverrNotifications.Logic.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FiverrNotifications.Controllers
{
    public class SessionsController: Controller
    {
        private readonly IChatsRepository _chatsRepository;

        public SessionsController(IChatsRepository chatsRepository)
        {
            _chatsRepository = chatsRepository;
        }

        [HttpGet("sessions-statistics")]
        public async Task<ActionResult<SessionStatistics>> GetSessionsStatistics()
        {
            var statistics = await _chatsRepository.GetSessionsStatistics();

            return statistics;
        }
    }
}

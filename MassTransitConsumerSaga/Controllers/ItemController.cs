﻿using System.Threading.Tasks;
using MassTransit;
using MassTransitConsumerSaga.MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MassTransitConsumerSaga.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly ILogger<ItemController> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public ItemController(ILogger<ItemController> logger, IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        [HttpPut]
        [Route("{itemId}/approve")]
        public async Task ApproveItem(int itemId)
        {
            await _publishEndpoint.Publish<ItemApproved>(new { ItemId = itemId });
        }
    }
}

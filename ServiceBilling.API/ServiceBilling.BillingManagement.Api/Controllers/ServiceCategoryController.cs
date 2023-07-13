﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using ServiceBilling.API.Application.Features.ServiceCategories.Queries.GetServiceCategoryList;

namespace ServiceBilling.BillingManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceCategoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ServiceCategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("all", Name = "GetAllServiceCategories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ServiceCategoryListVm>>> GetAllServiceCategories()
        {
            var dtos = await _mediator.Send(new GetServiceCategoryListQuery());
            return Ok(dtos);
        }
    }
}

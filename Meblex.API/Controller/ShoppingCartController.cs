﻿using System.Collections.Generic;
using Dawn;
using Meblex.API.FormsDto.Request;
using Meblex.API.FormsDto.Response;
using Meblex.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Meblex.API.Controller
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class ShoppingCartController:ControllerBase
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IJWTService _jwtService;
        public ShoppingCartController(IShoppingCartService shoppingCartService, IJWTService jwtService)
        {
            _shoppingCartService = shoppingCartService;
            _jwtService = jwtService;
        }

        [Authorize(Roles = "Client")]
        [HttpPost("make")]
        [SwaggerResponse(500)]
        [SwaggerResponse(201, "" , typeof(OrderResponse))]
        public IActionResult AddOrder([FromBody] OrderAddForm order)
        {
            var userId = _jwtService.GetAccessTokenUserId(User);
            var id = _shoppingCartService.AddOrder(userId, order);
            var response = _shoppingCartService.GetClientById(id, userId);

            return StatusCode(201, response);
        }
        [Authorize(Roles = "Client")]
        [HttpGet("client/list")]
        [SwaggerResponse(500)]
        [SwaggerResponse(200, "", typeof(List<OrderResponse>))]
        public IActionResult GetAllClientOrders()
        {
            var userId = _jwtService.GetAccessTokenUserId(User);
            var response = _shoppingCartService.GetAllClientOrders(userId);
            return   StatusCode(200, response);
        }

        [Authorize(Roles = "Client")]
        [HttpGet("client/list/{id}")]
        [SwaggerResponse(500)]
        [SwaggerResponse(404)]
        [SwaggerResponse(200, "", typeof(OrderResponse))]
        public IActionResult GetOneClientOrder(int id)
        {
            var userId = _jwtService.GetAccessTokenUserId(User);
            var response = _shoppingCartService.GetClientById(id, userId);
            return StatusCode(200, response);
        }

        [Authorize(Roles = "Client")]
        [HttpPut("client/realize-reservation/{id}")]
        [SwaggerResponse(200)]
        [SwaggerResponse(500)]
        [SwaggerResponse(404)]
        public IActionResult RealizeReservation(int id)
        {
            var Id = Guard.Argument(id, nameof(id)).NotNegative().NotZero().Value;

            _shoppingCartService.RealizeReservation(Id);

            return Ok();
        }

    }
}

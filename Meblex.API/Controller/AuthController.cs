﻿using System;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Meblex.API.DTO;
using Meblex.API.FormsDto.Response;
using Meblex.API.Helper;
using Meblex.API.Interfaces;
using Meblex.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Swagger;
using Mapper = AgileObjects.AgileMapper.Mapper;

namespace Meblex.API.Controller
{

    [Authorize]
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController: ControllerBase
    {
        private readonly IStringLocalizer<AuthController> _localizer;
        private readonly IAuthService _authService;
        private readonly IJWTService _jwtService;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        public AuthController(IAuthService authService, IJWTService jwtService, IMapper mapper, IUserService userService, IStringLocalizer<AuthController> localizer)
        {
            _localizer = localizer;
            _authService = authService;
            _jwtService = jwtService;
            _mapper = mapper;
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [SwaggerOperation(
            Summary = "Login endpoint",
            Description = "User can get tokens and user data",
            OperationId = "AuthLogin")]
        [SwaggerResponse(200,"",typeof(AuthLoginResponse))]
        [SwaggerResponse(500)]
        [SwaggerResponse(404)]
        [SwaggerResponse(401)]
        public async Task<IActionResult> Login([FromBody] UserLoginForm user)
        {
            var loginCheck = await _authService.CheckUser(user.Email, user.Password);
            if (!loginCheck) throw new HttpStatusCodeException(HttpStatusCode.Unauthorized, _localizer["Wrong password or email"]);

            var accessToken = await _authService.GetAccessToken(user.Email, user.Password);
            var refreshToken = await _authService.GetRefreshToken(user.Email, user.Password);
            var userData = await _userService.GetUserData(user.Email);

            
            var response = new AuthLoginResponse();
            response.Address = userData.Address;
            response.City = userData.City;
            response.Name = userData.Name;
            response.NIP = userData.NIP == null ? (int?) null : int.Parse(userData.NIP);
            response.PostCode = userData.PostCode;
            response.State = userData.State;
            response.AccessToken = accessToken;
            response.RefreshToken = refreshToken;
            response.Email = user.Email;
            
            

            return  accessToken != null && refreshToken != null && userData != null ? 
                (IActionResult) StatusCode(200, response) : StatusCode(500);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        [SwaggerOperation(
            Summary = "Register endpoint",
            Description = "Can register a new user",
            OperationId = "AuthRegister")]
        [SwaggerResponse(201, "", typeof(TokenResponse))]
        [SwaggerResponse(500)]
        [SwaggerResponse(409)]
        public async Task<IActionResult> Register([FromBody] AuthRegisterForm registerForm)
        {
            var checkEmail = await _userService.CheckIfUserWithEmailExist(registerForm.Email);
            if (checkEmail) throw new HttpStatusCodeException(HttpStatusCode.Conflict, "User with that email exist");
            var registedUserInfo = await _authService.RegisterNewUser(registerForm);
            if (registedUserInfo == null) return StatusCode(500);
            var accessToken = await  _authService.GetAccessToken(registerForm.Email, registerForm.Password);
            var refreshToken = await _authService.GetRefreshToken(registerForm.Email, registerForm.Password);

            return refreshToken != null && accessToken != null ? (IActionResult)StatusCode(201, new TokenResponse() { AccessToken = accessToken, RefreshToken = refreshToken }) : StatusCode(500);
        }

        [AllowAnonymous]
        [HttpPut("refresh")]
        [SwaggerOperation(
            Summary = "Token refresh",
            Description = "Can refresh user tokens with refresh token",
            OperationId = "AuthRefresh")]
        [SwaggerResponse(201, "", typeof(TokenResponse))]
        [SwaggerResponse(500)]
        [SwaggerResponse(404)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenForm token)
        {
            try
            {
                var userId = _jwtService.GetRefreshTokenUserId(token.Token);
                var accessToken = await _authService.GetAccessToken(userId);
                var refreshToken = await _authService.GetRefreshToken(userId);
                return refreshToken != null && accessToken != null ? (IActionResult)StatusCode(201, new TokenResponse(){AccessToken = accessToken, RefreshToken = refreshToken}) : StatusCode(500);
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpGet("check")]
        [SwaggerOperation(
            Summary = "Token access check",
            Description = "Endpoint response 200 if access token is valid",
            OperationId = "AuthCheck")]
        [SwaggerResponse(500)]
        [SwaggerResponse(401)]
        [SwaggerResponse(200)]
        [SwaggerResponse(404)]
        public IActionResult Check()
        {
            return Ok();
        }

    }
}
﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using snapnow.DTOS;
using snapnow.Models;
using snapnow.Services;

namespace snapnow.Controllers;

[ApiController]
[Route("api/authentication")]
public class AuthenticationController : ControllerBase
{
    private readonly IUserService _userService;
    public AuthenticationController(IUserService userService)
    {
        _userService = userService;
       
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUserRoute([FromForm]RegisterUserModel userModel)
    {
        var result = await _userService.RegisterUser(userModel, Url);
        if (result.IsSuccess)
        {
            return Ok(result);
        }

        return Problem(
            statusCode: result.StatusCode,
            title: result.Message,
            detail: result.Errors != null ? String.Join("/n", result.Errors) : ""
        ); 
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginUserRoute([FromForm] LoginUserModel userModel)
    {
        var response = await _userService.LoginUser(userModel);
        if (response.IsSuccess)
        {
            return Ok(response);
        }
        
        return Problem(
            statusCode: response.StatusCode,
            title: response.Message,
            detail: response.Errors != null ? String.Join("/n", response.Errors) : ""
        );
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult LogoutUserRoute()
    {
        var response = _userService.Logout();
        if (response.IsSuccess)
        {
            return Ok(response);
        }

        return Problem(
            statusCode: response.StatusCode,
            title: response.Message,
            detail: response.Errors != null ? String.Join("/n", response.Errors) : ""
        );
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(string Id, string token)
    {
        if (string.IsNullOrEmpty(Id) || string.IsNullOrEmpty(token))
        {
            return Problem(
                title: "Email NOT confirmed.",
                detail: "Some required email confirmation information was not specified.",
                statusCode: 400
            );
        }

        var confirmEmailResponse = await _userService.ConfirmUserEmail(Id, token);
        
        if (!confirmEmailResponse.IsSuccess)
        {
            return Problem(
                title: confirmEmailResponse.Message,
                detail: confirmEmailResponse.Errors != null ? String.Join("/n", confirmEmailResponse.Errors) : "",
                statusCode: confirmEmailResponse.StatusCode
            );
        }
        return Ok(confirmEmailResponse);
    }
    
    [HttpPost("google")]
    public async Task<IActionResult> AuthenticateWithGoogle([FromBody] AccessTokenModel userAuthorizationResponse)
    {
        var accessToken = userAuthorizationResponse.AccessToken;

        if (accessToken == null)
        {
            return BadRequest("Access token is missing.");
        }
        
        var googleAuthenticationResponse = await _userService.GoogleAuthentication(accessToken);

        if (googleAuthenticationResponse.IsSuccess)
        {
            return Ok(googleAuthenticationResponse);
        }

        return Problem(
            title: googleAuthenticationResponse.Message,
            detail: googleAuthenticationResponse.Errors != null
                ? String.Join("/n", googleAuthenticationResponse.Errors)
                : "",
            statusCode: googleAuthenticationResponse.StatusCode
        );
    }
}
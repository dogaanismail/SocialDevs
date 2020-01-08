﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialDevs.Business.Interfaces;
using SocialDevs.Core.Security;
using SocialDevs.Domain.Api;
using SocialDevs.Domain.Common;
using SocialDevs.Domain.Dto;
using SocialDevs.Entities.Entities;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SocialDevs.Api.Controllers
{
    public class AccountController : BaseController
    {
        #region Ctor
        private readonly ITokenService _tokenService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserDetailService _userDetailService;

        public AccountController(ITokenService tokenService,
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager,
            IUserDetailService userDetailService)
        {
            _tokenService = tokenService;
            _signInManager = signInManager;
            _userManager = userManager;
            _userDetailService = userDetailService;
        }
        #endregion

        /// <summary>
        /// Member Login
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<JsonResult> Login([FromBody] LoginApiRequest model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, false);
            var user = _userDetailService.GetUserDetailByUsername(model.Username);
            if (result.Succeeded && user != null)
            {
                var token = _tokenService.GenerateToken(new AppUserDto
                {
                    AppUserId = user.Id,
                    UserName = user.UserName,
                    CoverPhotoUrl = user.CoverPhotoUrl,
                    ProfilePhotoUrl = user.ProfilePhotoUrl,
                    UserPosts = user.UserPosts,
                    RegisteredDate = user.RegisteredDate
                });
                return OkResponse(token);
            }
            else
            {
                Result.Status = false;
                Result.Message = "Username or password are wrong !";
                return BadResponse(Result);
            }
        }


        /// <summary>
        /// Member Register
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("register")]
        public async Task<JsonResult> Register([FromBody] RegisterApiRequest model)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    if (model.RePassword != model.Password)
                    {
                        return BadResponse(ResultModel.Error("Repassword must match password"));
                    }

                    AppUserDetail appUserDetail = new AppUserDetail
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        ProfilePhotoPath = "http://placehold.it/300x300",
                        CoverPhotoPath = "http://placehold.it/1030x360"
                    };
                    ResultModel resultModel = _userDetailService.Create(appUserDetail);
                    if (!resultModel.Status)
                    {
                        return BadResponse(resultModel);
                    }

                    AppUser userEntity = new AppUser
                    {
                        UserName = model.UserName,
                        Email = model.Email,
                        CreatedDate = DateTime.Now,
                        DetailId = appUserDetail.Id
                    };

                    IdentityResult result = await _userManager.CreateAsync(userEntity, model.Password);
                    if (!result.Succeeded)
                    {
                        Result.Status = false;
                        Result.Message = string.Join(",", result.Errors.Select(x => x.Description));
                        scope.Dispose();
                        return BadResponse(Result);
                    }
                    scope.Complete();
                    return OkResponse(Result);
                }
                catch (Exception ex)
                {
                    scope.Dispose();
                    Result.Status = false;
                    Result.Message = ex.ToString();
                    return BadResponse(Result);
                }
            }
        }

    }
}

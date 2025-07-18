﻿using PersonalFinance.Services.UserManagement.Application.DTOs;

namespace PersonalFinance.Services.UserManagement.Application.DataTransferObjects.Response
{
    public class LoginResponse
    {
        public UserTransferObject User { get; set; } = null!;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public List<string> Permissions { get; set; } = new();
    }
}

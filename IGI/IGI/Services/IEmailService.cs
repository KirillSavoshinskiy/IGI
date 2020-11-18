﻿using System.Threading.Tasks;

namespace IGI.Services
{
    public interface IEmailService
    {
        public Task SendEmailAsync(string email, string subject, string mess);
    }
}
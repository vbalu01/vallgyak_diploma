using AutoPortal.Models.AppModels;
using AutoPortal.Models.DbModels;
using Microsoft.EntityFrameworkCore;

namespace AutoPortal.Libs
{
    public static class TokenHandler
    {
        public static Token GenerateMailConfirmToken(int id, eVehicleTargetTypes type)
        {
            Token t = new Token()
            {
                token = Functions.ReplaceSpecials(Convert.ToBase64String(Guid.NewGuid().ToByteArray())) + id,
                expire = DateTime.Now.AddHours(24),
                token_type = eTokenType.MAIL_CONFIRM,
                target_type = type,
                target_id = id,
            };

            return t;
        }
    }
}
